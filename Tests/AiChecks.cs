// Purpose: Checks the Ollama connection using simulated HTTP responses, without a model.
// Data: Uses the existing sample risk and short JSON responses.
// Methods: Verifies local URLs, request fields, successful replies, and errors.

using System.Net;
using System.Text;
using System.Text.Json;
using ProjectHealthTracker.Data;
using ProjectHealthTracker.Models;
using ProjectHealthTracker.Services;

namespace TrackerChecks;

public static class AiChecks
{
    public static void Run(Action<bool, string> check)
    {
        Project project = MockProjectData.GetProjects()[1];
        Risk risk = (Risk)project.Items[2];
        ProjectService service = new ProjectService(new List<Project> { project });
        check(service.CalculateRiskScore(risk) == 20, "Sample risk priority score is 20, separate from health");

        using HttpClient client = new HttpClient(new FakeOllamaHandler(check));
        AiRiskService ai = new AiRiskService(client);
        List<string> models = ai.GetModels();
        check(models.Count == 1 && models[0] == "existing-local:latest", "Lists installed local models and excludes cloud models");
        string answer = ai.ExplainRisk(risk, models[0], 20);
        check(answer == "Confirm delivery and prepare manual entry.", "Reads generated text from Ollama response");
        check(risk.Status == ItemStatus.Open && risk.Impact == 5 && service.CalculateHealth(project) == HealthStatus.OffTrack,
            "AI explanation does not change risk data or project health");

        using HttpClient emptyClient = new HttpClient(new FixedResponseHandler("{\"models\":[]}"));
        check(new AiRiskService(emptyClient).GetModels().Count == 0, "Empty installed-model list is supported");
        using HttpClient blankClient = new HttpClient(new FixedResponseHandler("{\"response\":\" \"}"));
        check(new AiRiskService(blankClient).ExplainRisk(risk, "local", 20).Contains("no explanation"), "Blank model reply gives a useful message");
        using HttpClient badClient = new HttpClient(new FixedResponseHandler("not json"));
        try
        {
            new AiRiskService(badClient).ExplainRisk(risk, "local", 20);
            check(false, "Malformed JSON reaches the menu's error handler");
        }
        catch (JsonException)
        {
            check(true, "Malformed JSON reaches the menu's error handler");
        }
        using HttpClient failedClient = new HttpClient(new FixedResponseHandler("{\"error\":\"model not found\"}", HttpStatusCode.NotFound));
        try
        {
            new AiRiskService(failedClient).ExplainRisk(risk, "missing", 20);
            check(false, "Unsuccessful HTTP response is rejected");
        }
        catch (HttpRequestException)
        {
            check(true, "Unsuccessful HTTP response is rejected");
        }
    }

    private class FakeOllamaHandler : HttpMessageHandler
    {
        private readonly Action<bool, string> check;
        public FakeOllamaHandler(Action<bool, string> check) { this.check = check; }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            check(request.RequestUri!.Host == "localhost" && request.RequestUri.Port == 11434, "Request stays on local Ollama address");
            if (request.Method == HttpMethod.Get)
            {
                check(request.RequestUri.AbsolutePath == "/api/tags", "Model discovery uses Ollama tags endpoint");
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(
                    "{\"models\":[{\"name\":\"existing-local:latest\"},{\"name\":\"example:cloud\"},{\"name\":\"remote-alias\",\"remote_model\":\"remote\"}]}" ) };
            }

            check(request.Method == HttpMethod.Post && request.RequestUri.AbsolutePath == "/api/generate", "Explanation uses Ollama generation endpoint");
            using JsonDocument body = JsonDocument.Parse(request.Content!.ReadAsStream());
            JsonElement json = body.RootElement;
            check(json.GetProperty("model").GetString() == "existing-local:latest" && !json.GetProperty("stream").GetBoolean(),
                "Request uses the selected model and disables streaming");
            string prompt = json.GetProperty("prompt").GetString()!;
            check(prompt.Contains("Scanner hardware may arrive late") && prompt.Contains("20/25") && prompt.Contains("manual item numbers")
                && prompt.Contains("not a statistical probability"), "Prompt supplies real risk data and explains score limitations");
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"response\":\"Confirm delivery and prepare manual entry.\",\"done\":true}") };
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Send(request, cancellationToken));
        }
    }

    private class FixedResponseHandler : HttpMessageHandler
    {
        private readonly string body;
        private readonly HttpStatusCode status;
        public FixedResponseHandler(string body, HttpStatusCode status = HttpStatusCode.OK)
        {
            this.body = body;
            this.status = status;
        }
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Send(request, cancellationToken));
        }
    }
}

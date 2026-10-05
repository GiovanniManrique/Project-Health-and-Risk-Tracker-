// Purpose: Checks project metrics and the OpenAI request without spending API credits.
// Data: Uses fresh sample objects and simulated HTTP replies; no real key is needed.
// Methods: Checks prompt facts, response handling, and error cases.

using System.Net;
using System.Text;
using System.Text.Json;
using ProjectHealthTracker.Data;
using ProjectHealthTracker.Models;
using ProjectHealthTracker.Services;

namespace TrackerChecks;

public static class CloudAiChecks
{
    public static void Run(Action<bool, string> check)
    {
        List<Project> projects = MockProjectData.GetProjects();
        ProjectService service = new ProjectService(projects);
        Project inventory = projects[1];
        Risk risk = (Risk)inventory.Items[2];
        string metrics = service.GetProjectMetrics(inventory);
        check(metrics.Contains("Open risks: 1") && metrics.Contains("High-impact open risks (impact 4 or 5): 1"),
            "Project review counts open and high-impact risks");
        check(metrics.Contains("Late unfinished milestones: 0") && metrics.Contains("Tasks completed: 0 of 1"),
            "Project review reports late milestones and task completion");
        check(metrics.Contains("Days until planned end date: 75"), "Days remaining are measured from today");
        check(service.GetProjectMetrics(projects[0]).Contains("Tasks completed: 1 of 1"), "Completed tasks are counted");
        check(service.GetProjectMetrics(projects[2]).Contains("Late unfinished milestones: 1"), "Late unfinished milestone is included");
        Project empty = new Project(20, "Empty", "Tester", DateTime.Today, DateTime.Today.AddDays(-1));
        check(service.GetProjectMetrics(empty).Contains("Tasks completed: 0 of 0") &&
            service.GetProjectMetrics(empty).Contains("Days until planned end date: -1"), "Empty tasks and passed deadlines are handled without division");
        risk.Status = ItemStatus.Closed;
        check(service.GetProjectMetrics(inventory).Contains("High-impact open risks (impact 4 or 5): 0"), "Closed high-impact risk is excluded from metrics");
        risk.Status = ItemStatus.Open;
        risk.Impact = 3;
        check(service.GetProjectMetrics(inventory).Contains("High-impact open risks (impact 4 or 5): 0"), "Impact 3 is not counted as high impact");
        risk.Impact = 4;
        check(service.GetProjectMetrics(inventory).Contains("High-impact open risks (impact 4 or 5): 1"), "Impact 4 is counted as high impact");
        risk.Impact = 5;

        string review = service.BuildRiskReview(inventory, risk, "", "  ");
        check(review.Contains("Unknown; no evidence supplied.") && review.Contains("Unknown; no affected work described."),
            "Blank answers remain unknown instead of invented evidence");
        check(review.Contains("Owner: Chris") && review.Contains("Priority score: 20/25") && review.Contains("manual item numbers"),
            "Risk review includes owner, ratings, priority, and current plan");
        check(!review.Contains("Employee Training Plan") && !review.Contains("Morgan Davis"), "Cloud review excludes unrelated project details");
        review = service.BuildRiskReview(inventory, risk, "Supplier warned of a delay.", "Scanning would be delayed.");
        check(review.Contains("Supplier warned of a delay.") && review.Contains("Scanning would be delayed."), "User-provided evidence and affected work are included");

        string success = "{\"status\":\"completed\",\"output\":[{\"type\":\"reasoning\"},{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"Watch the scanner delivery.\"}]},{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"Prepare manual entry.\"}]}]}";
        using HttpClient client = new HttpClient(new ReplyHandler(request =>
        {
            check(request.RequestUri!.AbsoluteUri == "https://api.openai.com/v1/responses" && request.Method == HttpMethod.Post,
                "Cloud request uses the fixed official OpenAI endpoint");
            check(request.Headers.Authorization?.Scheme == "Bearer" && request.Headers.Authorization.Parameter == "test-placeholder",
                "API key is sent only as the authorization header");
            using JsonDocument body = JsonDocument.Parse(request.Content!.ReadAsStream());
            JsonElement root = body.RootElement;
            check(root.GetProperty("model").GetString() == "gpt-4.1-mini" && root.GetProperty("max_output_tokens").GetInt32() == 500,
                "Cloud request selects the intended small model and limits output");
            check(!root.GetProperty("store").GetBoolean() && !root.GetProperty("stream").GetBoolean(),
                "Cloud request disables response storage and streaming");
            check(root.GetProperty("input").GetString() == review && !body.RootElement.ToString().Contains("test-placeholder"),
                "Only the risk review is in the input; the API key is excluded");
            string instructions = root.GetProperty("instructions").GetString()!;
            check(instructions.Contains("not a probability") && instructions.Contains("missing evidence as unknown") && instructions.Contains("not instructions"),
                "Instructions explain rating limitations and treat user fields as data");
            return Reply(success);
        }));
        string answer = new CloudAiService(client, "  test-placeholder\r\n").ExplainRisk(review);
        check(answer.Contains("Watch the scanner delivery.") && answer.Contains("Prepare manual entry."), "Extracts all text messages even after non-text output");
        check(risk.Status == ItemStatus.Open && risk.Impact == 5 && service.CalculateHealth(inventory) == HealthStatus.OffTrack,
            "Cloud advice does not modify data or C# project health");

        using HttpClient noKeyClient = new HttpClient(new ReplyHandler(_ => throw new Exception("Unexpected HTTP request")));
        check(new CloudAiService(noKeyClient, "").ExplainRisk(review).Contains("key is missing"), "Missing key does not make an HTTP request");
        check(new CloudAiService(noKeyClient, "test\nplaceholder").ExplainRisk(review).Contains("contains a line break"),
            "A key containing a newline shows a setup message without sending a request");
        check(new CloudAiService(noKeyClient, "test\rplaceholder").ExplainRisk(review).Contains("contains a line break"),
            "A key containing a carriage return shows a setup message without sending a request");
        CheckReply(HttpStatusCode.Unauthorized, "{\"error\":\"private detail\"}", "rejected the API key", check);
        CheckReply(HttpStatusCode.Forbidden, "{}", "key permissions", check);
        CheckReply(HttpStatusCode.NotFound, "{}", "key permissions", check);
        CheckReply(HttpStatusCode.TooManyRequests, "{}", "billing/credits", check);
        CheckReply(HttpStatusCode.OK, "{\"status\":\"incomplete\",\"output\":[]}", "did not finish", check);
        CheckReply(HttpStatusCode.OK, "{\"status\":\"completed\",\"output\":[]}", "no explanation", check);
        CheckReply(HttpStatusCode.OK, "{\"status\":\"completed\",\"output\":[{\"type\":\"message\",\"content\":[{\"type\":\"refusal\"}]}]}", "declined", check);

        ExpectError(new ReplyHandler(_ => Reply("not JSON")), typeof(JsonException), "Invalid JSON is reported to the menu", check);
        ExpectError(new ReplyHandler(_ => Reply("{}")), typeof(KeyNotFoundException), "Missing response fields are reported to the menu", check);
        ExpectError(new ReplyHandler(_ => Reply("{\"status\":\"completed\",\"output\":null}")), typeof(InvalidOperationException), "Unexpected JSON types are reported to the menu", check);
        ExpectError(new ReplyHandler(_ => Reply("provider error", HttpStatusCode.InternalServerError)), typeof(HttpRequestException), "Server errors are reported to the menu", check);
        ExpectError(new ReplyHandler(_ => throw new HttpRequestException("Offline")), typeof(HttpRequestException), "Connection errors are reported to the menu", check);
        ExpectError(new ReplyHandler(_ => throw new TaskCanceledException()), typeof(TaskCanceledException), "Timeouts are reported to the menu", check);
    }

    private static HttpResponseMessage Reply(string body, HttpStatusCode status = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
    }

    private static void CheckReply(HttpStatusCode status, string body, string expected, Action<bool, string> check)
    {
        using HttpClient client = new HttpClient(new ReplyHandler(_ => Reply(body, status)));
        string result = new CloudAiService(client, "test-placeholder").ExplainRisk("sample review");
        check(result.Contains(expected) && !result.Contains("private detail"), $"Cloud response handles {expected}");
    }

    private static void ExpectError(HttpMessageHandler handler, Type expected, string description, Action<bool, string> check)
    {
        using HttpClient client = new HttpClient(handler);
        try
        {
            new CloudAiService(client, "test-placeholder").ExplainRisk("sample review");
            check(false, description);
        }
        catch (Exception error)
        {
            check(expected.IsInstanceOfType(error), description);
        }
    }

    private class ReplyHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> reply;
        public ReplyHandler(Func<HttpRequestMessage, HttpResponseMessage> reply) { this.reply = reply; }
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) { return reply(request); }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) { return Task.FromResult(reply(request)); }
    }
}

// Run with: dotnet run --project Tests/TrackerChecks.csproj
// Tests use fake HTTP responses. They never load or run an AI model.
using System.Net;
using System.Text;
using System.Text.Json;
using ProjectHealthTracker.Models;
using Tracker = ProjectHealthTracker.Program;

int passed = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new Exception("FAILED: " + message);
    passed++;
}

List<Project> projects = Tracker.CreateSampleProjects();
Check(projects.Count == 3, "Three sample projects");
Check(projects.All(p => p.Items.Count == 3), "Three items per project");
Check(projects[0].CalculateHealth() == HealthStatus.OnTrack, "Website is OnTrack");
Check(projects[1].CalculateHealth() == HealthStatus.OffTrack, "Inventory is OffTrack");
Check(projects[2].CalculateHealth() == HealthStatus.AtRisk, "Training is AtRisk");
Project inventory = projects[1];
ProjectItem risk = inventory.Items[2];
Check(risk.CalculateScore() == 20 && risk.GetDetails().Contains("not a percentage"), "20 is a priority score");
Check(inventory.CountOpenRisks() == 1, "Count open risks");
Check(!risk.ChangeStatus(ItemStatus.Completed), "Reject work status for risk");
Check(!risk.ChangeStatus((ItemStatus)999), "Reject undefined status");
Check(risk.Status == ItemStatus.Open, "Rejected change preserves status");
Check(risk.ChangeStatus(ItemStatus.Closed), "Close risk");
Check(inventory.CalculateHealth() == HealthStatus.OnTrack && inventory.CountOpenRisks() == 0, "Closing risk updates health and count");
Check(risk.ChangeStatus(ItemStatus.Open), "Reopen risk");
ProjectItem task = inventory.Items[0];
Check(!task.ChangeStatus(ItemStatus.Closed), "Task cannot use risk status");
Check(task.ChangeStatus(ItemStatus.Completed), "Complete task without duplicate flag");
Check(task.ChangeStatus(ItemStatus.Blocked), "Reopen task");
ProjectItem milestone = projects[2].Items[1];
Check(milestone.ChangeStatus(ItemStatus.Completed), "Complete milestone");
Check(projects[2].CalculateHealth() == HealthStatus.OnTrack, "Completion removes overdue problem");
milestone.ChangeStatus(ItemStatus.InProgress);
milestone.DueDate = DateTime.Today;
Check(projects[2].CalculateHealth() == HealthStatus.OnTrack, "Due today is not overdue");
milestone.DueDate = DateTime.Today.AddDays(-1);
Check(projects[2].CalculateHealth() == HealthStatus.AtRisk, "Due yesterday is overdue");
milestone.DueDate = null;
Check(projects[2].CalculateHealth() == HealthStatus.OnTrack, "Missing due date does not invent overdue state");
Check(!milestone.GetDetails().Contains("Due date:"), "Missing date displays safely");
Check(new Project(4, "Empty", "Nobody").CalculateHealth() == HealthStatus.OnTrack, "Empty project");
risk.Impact = 3;
Check(inventory.CalculateHealth() == HealthStatus.AtRisk, "Open impact 3 is AtRisk");
risk.Impact = 4;
Check(inventory.CalculateHealth() == HealthStatus.OffTrack, "Impact 4 is OffTrack boundary");
risk.Probability = 1;
Check(inventory.CalculateHealth() == HealthStatus.OffTrack, "Health rule uses impact, not score");
inventory.Items.Insert(0, new ProjectItem(299, "Earlier late milestone", "Pat", ItemType.Milestone, ItemStatus.InProgress)
    { DueDate = DateTime.Today.AddDays(-2) });
Check(inventory.CalculateHealth() == HealthStatus.OffTrack, "High-impact risk wins regardless of item order");
risk.ChangeStatus(ItemStatus.Closed);
Check(inventory.CalculateHealth() == HealthStatus.AtRisk, "Late milestone remains after closing risk");
foreach (int rating in new[] { 1, 5 })
{
    risk.Probability = rating;
    risk.Impact = rating;
    Check(risk.CalculateScore() == rating * rating, "Score boundary " + rating);
}
foreach (ItemType type in new[] { ItemType.Task, ItemType.Milestone, ItemType.Risk })
{
    ItemStatus invalid = type == ItemType.Risk ? ItemStatus.Completed : ItemStatus.Open;
    bool rejected = false;
    try { _ = new ProjectItem(1, "Bad status", "Owner", type, invalid); }
    catch (ArgumentException) { rejected = true; }
    Check(rejected, "Constructor rejects incompatible status: " + type);
}
Check(Tracker.CreateSampleProjects()[1].Items[2].Status == ItemStatus.Open, "New session creates fresh objects");

projects = Tracker.CreateSampleProjects();
inventory = projects[1];
risk = inventory.Items[2];
using (FakeBionic handler = new FakeBionic())
using (HttpClient client = new HttpClient(handler))
{
    string answer = await Tracker.ExplainRiskAsync(client, inventory, risk);
    Check(answer.Contains("LOCAL AI ANSWER") && answer.Contains("Contact the supplier"), "Final answer is returned");
    Check(!answer.Contains("PRIVATE REASONING"), "Reasoning is not displayed");
    Check(handler.Requests == 2 && handler.Posts == 1, "One preflight and one generation request");
    Check(handler.AllLocal, "Requests stay on configured loopback endpoint");
    using JsonDocument body = JsonDocument.Parse(handler.LastBody);
    JsonElement root = body.RootElement;
    Check(root.GetProperty("model").GetString() == "qwen-demo-instance", "Uses loaded Qwen instance ID");
    Check(root.GetProperty("reasoning").GetString() == "off", "Qwen reasoning disabled");
    Check(!root.GetProperty("stream").GetBoolean() && !root.GetProperty("store").GetBoolean(), "No streaming or saved conversation");
    Check(root.GetProperty("integrations").GetArrayLength() == 0, "No tools or integrations");
    string prompt = root.GetProperty("input").GetString()!;
    Check(prompt.Contains("Inventory System") && prompt.Contains("20/25") && prompt.Contains("OffTrack"), "Request includes C# project results");
    Check(prompt.Contains(risk.Scenario) && prompt.Contains(risk.MitigationPlan), "Request includes fictional facts and mitigation");
    Check(risk.Status == ItemStatus.Open && inventory.CalculateHealth() == HealthStatus.OffTrack, "AI does not change data");
}
foreach (string models in new[]
{
    "{\"models\":[]}",
    "{\"models\":[{\"key\":\"qwen/qwen3.5-9b\",\"loaded_instances\":[]}]}",
    "{\"models\":[{\"key\":\"other-27b\",\"loaded_instances\":[{\"id\":\"wrong\"}]}]}"
})
{
    using FakeBionic handler = new FakeBionic { Models = models };
    using HttpClient client = new HttpClient(handler);
    string answer = await Tracker.ExplainRiskAsync(client, inventory, risk);
    Check(answer.Contains("not loaded") && handler.Posts == 0, "Missing exact loaded model never triggers generation");
}
foreach (string malformed in new[] { "not json", "{}", "{\"models\":null}", "{\"models\":{}}" })
{
    using FakeBionic handler = new FakeBionic { Models = malformed };
    using HttpClient client = new HttpClient(handler);
    string answer = await Tracker.ExplainRiskAsync(client, inventory, risk);
    Check(answer.Contains("response") && handler.Posts == 0, "Malformed model list returns useful error");
}
foreach (string reply in new[] { "{\"output\":[]}", "{\"output\":[{\"type\":\"message\",\"content\":\" \"}]}" })
{
    using FakeBionic handler = new FakeBionic { Reply = reply };
    using HttpClient client = new HttpClient(handler);
    Check((await Tracker.ExplainRiskAsync(client, inventory, risk)).Contains("no explanation"), "Empty answer is reported");
}
foreach (string reply in new[] { "bad json", "{}", "{\"output\":null}" })
{
    using FakeBionic handler = new FakeBionic { Reply = reply };
    using HttpClient client = new HttpClient(handler);
    Check((await Tracker.ExplainRiskAsync(client, inventory, risk)).Contains("response"), "Malformed answer is reported");
}
foreach (string failure in new[] { "http", "network", "timeout" })
{
    using FakeBionic handler = new FakeBionic { Failure = failure };
    using HttpClient client = new HttpClient(handler);
    string answer = await Tracker.ExplainRiskAsync(client, inventory, risk);
    Check(answer.Contains(failure == "timeout" ? "too long" : "could not complete"), "Recover from " + failure);
}
using (FakeBionic handler = new FakeBionic())
using (HttpClient client = new HttpClient(handler))
{
    risk.ChangeStatus(ItemStatus.Closed);
    Check((await Tracker.ExplainRiskAsync(client, inventory, risk)).Contains("open risk"), "Closed risk rejected");
    risk.ChangeStatus(ItemStatus.Open);
    Check((await Tracker.ExplainRiskAsync(client, projects[0], risk)).Contains("belongs"), "Foreign risk rejected");
    Check((await Tracker.ExplainRiskAsync(client, inventory, inventory.Items[0])).Contains("open risk"), "Task rejected");
    foreach (int rating in new[] { 0, 6 })
    {
        risk.Probability = rating;
        Check((await Tracker.ExplainRiskAsync(client, inventory, risk)).Contains("between 1 and 5"), "Bad rating rejected");
    }
    Check(handler.Requests == 0, "Invalid data never contacts Bionic");
}

async Task<string> RunConsole(string input)
{
    TextReader originalInput = Console.In;
    TextWriter originalOutput = Console.Out;
    using StringWriter output = new StringWriter();
    try
    {
        Console.SetIn(new StringReader(input));
        Console.SetOut(output);
        await Tracker.Main();
        return output.ToString();
    }
    finally { Console.SetIn(originalInput); Console.SetOut(originalOutput); }
}
string transcript = await RunConsole("1\n2\n2\n3\n2\n203\n2\n1\n3\n3\n302\n3\n1\n5\n");
Check(transcript.Contains("1 OnTrack, 1 AtRisk, 1 OffTrack"), "Console initial summary");
Check(transcript.Contains("Scanner hardware") && transcript.Contains("Priority score: 20/25"), "Console details");
Check(transcript.Contains("2 OnTrack, 1 AtRisk, 0 OffTrack"), "Console close-risk flow");
Check(transcript.Contains("3 OnTrack, 0 AtRisk, 0 OffTrack"), "Console complete-milestone flow");
Check(transcript.EndsWith("Goodbye." + Environment.NewLine), "Console exits normally");
transcript = await RunConsole("bad\n2\n999\n3\n2\n999\n3\n2\n203\n99\n4\n1\n4\n2\n201\n4\n2\n203\nn\n5\n");
foreach (string expected in new[] { "number from 1 to 5", "project was not found", "not an available choice", "not a valid status", "no open risks", "No AI request was sent" })
    Check(transcript.Contains(expected), "Console handles " + expected);
foreach (string input in new[] { "", "2\n", "3\n2\n", "3\n2\n203\n", "4\n2\n203\n", "2\n0\n5\n" })
    Check((await RunConsole(input)).Contains("Goodbye."), "EOF or cancellation returns safely");
Console.WriteLine($"PASS: {passed} checks. No model inference was used.");

class FakeBionic : HttpMessageHandler
{
    public string Models = "{\"models\":[{\"key\":\"other\",\"loaded_instances\":[{\"id\":\"other\"}]},{\"key\":\"qwen/qwen3.5-9b\",\"loaded_instances\":[{\"id\":\"qwen-demo-instance\"}]}]}";
    public string Reply = "{\"output\":[{\"type\":\"reasoning\",\"content\":\"PRIVATE REASONING\"},{\"type\":\"message\",\"content\":\"Risk: Late scanners. Why: Delivery missed. Next action: Contact the supplier.\"}]}";
    public string Failure = "", LastBody = "";
    public int Requests, Posts;
    public bool AllLocal = true;
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests++;
        AllLocal &= request.RequestUri!.Host == "127.0.0.1" && request.RequestUri.Port == 51500;
        if (Failure == "network") throw new HttpRequestException("simulated offline");
        if (Failure == "timeout") throw new TaskCanceledException("simulated timeout");
        if (request.Method == HttpMethod.Get)
        {
            if (request.RequestUri.AbsolutePath != "/api/v1/models") throw new Exception("Unexpected GET");
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Models) };
        }
        if (request.Method != HttpMethod.Post || request.RequestUri.AbsolutePath != "/api/v1/chat") throw new Exception("Unexpected request");
        Posts++;
        LastBody = await request.Content!.ReadAsStringAsync(cancellationToken);
        return new HttpResponseMessage(Failure == "http" ? HttpStatusCode.InternalServerError : HttpStatusCode.OK)
            { Content = new StringContent(Reply, Encoding.UTF8, "application/json") };
    }
}

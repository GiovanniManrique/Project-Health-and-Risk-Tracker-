// These checks use simulated HTTP replies, never a model.
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
Check(projects.Count == 3, "Three projects");
Check(projects[0].CalculateHealth() == HealthStatus.OnTrack, "Website starts OnTrack");
Check(projects[1].CalculateHealth() == HealthStatus.OffTrack, "Inventory starts OffTrack");
Check(projects[2].CalculateHealth() == HealthStatus.AtRisk, "Training starts AtRisk");
Project inventory = projects[1];
Check(inventory.CalculateRiskScore() == 20, "Risk score 4 times 5");
Check(inventory.GetDetails().Contains("not a percentage"), "Score label is honest");
foreach (Project project in projects)
    Check(project.TaskOwner.Length > 0 && project.MilestoneOwner.Length > 0 && project.RiskOwner.Length > 0, "Owners retained");
Check(inventory.GetAllowedStatuses(3).SequenceEqual(new[] { ItemStatus.Open, ItemStatus.Closed }), "Risk choices and their order");
Check(inventory.GetAllowedStatuses(1).SequenceEqual(inventory.GetAllowedStatuses(2)), "Task and milestone share work choices");
foreach (int section in new[] { -1, 0, 4 })
{
    Check(inventory.GetAllowedStatuses(section).Count == 0, "Invalid section has no choices");
    Check(!inventory.ChangeStatus(section, ItemStatus.Open), "Invalid section cannot change data");
}
foreach (ItemStatus status in new[] { ItemStatus.NotStarted, ItemStatus.InProgress, ItemStatus.Completed, ItemStatus.Blocked })
{
    Check(inventory.ChangeStatus(1, status) && inventory.TaskStatus == status, "Valid task status");
    Check(inventory.ChangeStatus(2, status) && inventory.MilestoneStatus == status, "Valid milestone status");
    Check(!inventory.ChangeStatus(3, status), "Risk rejects work status");
}
foreach (ItemStatus status in new[] { ItemStatus.Open, ItemStatus.Closed })
{
    Check(inventory.ChangeStatus(3, status) && inventory.RiskStatus == status, "Valid risk status");
    Check(!inventory.ChangeStatus(1, status) && !inventory.ChangeStatus(2, status), "Work rejects risk status");
}
Check(!inventory.ChangeStatus(3, (ItemStatus)999) && inventory.RiskStatus == ItemStatus.Closed, "Undefined status preserves old value");
Check(inventory.CalculateHealth() == HealthStatus.OnTrack, "Closing risk updates health");
inventory.ChangeStatus(3, ItemStatus.Open);
inventory.RiskImpact = 3;
Check(inventory.CalculateHealth() == HealthStatus.AtRisk, "Open impact 3");
inventory.RiskImpact = 4;
inventory.RiskLikelihood = 1;
Check(inventory.CalculateHealth() == HealthStatus.OffTrack && inventory.CalculateRiskScore() == 4, "Health independent of score");
inventory.MilestoneDueDate = DateTime.Today.AddDays(-1);
Check(inventory.CalculateHealth() == HealthStatus.OffTrack, "High-impact risk wins over overdue milestone");
inventory.ChangeStatus(3, ItemStatus.Closed);
Check(inventory.CalculateHealth() == HealthStatus.AtRisk, "Overdue milestone remains after risk closes");
inventory.ChangeStatus(2, ItemStatus.Completed);
Check(inventory.CalculateHealth() == HealthStatus.OnTrack, "Completed late milestone ignored");
inventory.ChangeStatus(2, ItemStatus.InProgress);
inventory.MilestoneDueDate = DateTime.Today;
Check(inventory.CalculateHealth() == HealthStatus.OnTrack, "Due today is not overdue");
inventory.MilestoneDueDate = null;
inventory.TaskDueDate = DateTime.Today.AddDays(-10);
Check(inventory.CalculateHealth() == HealthStatus.OnTrack, "Missing milestone date and overdue task do not invent a warning");
Check(inventory.GetDetails().Contains("Due:"), "Missing date formats without crashing");
Check(Tracker.CreateSampleProjects()[1].RiskStatus == ItemStatus.Open, "Restart creates fresh objects");
foreach (int rating in new[] { 1, 5 })
{
    inventory.RiskLikelihood = rating;
    inventory.RiskImpact = rating;
    Check(inventory.CalculateRiskScore() == rating * rating, "Score boundary");
}

inventory = Tracker.CreateSampleProjects()[1];
using (FakeBionic handler = new FakeBionic())
using (HttpClient client = new HttpClient(handler))
{
    string answer = await Tracker.ExplainRiskAsync(client, inventory);
    Check(answer.Contains("LOCAL AI ANSWER") && answer.Contains("Contact the supplier"), "Final answer returned");
    Check(!answer.Contains("PRIVATE REASONING"), "Only message text displayed");
    Check(handler.Requests == 2 && handler.Posts == 1 && handler.AllLocal, "One local preflight then one POST");
    using JsonDocument body = JsonDocument.Parse(handler.LastBody);
    JsonElement request = body.RootElement;
    Check(request.GetProperty("model").GetString() == "qwen-demo-instance", "Uses matching loaded instance ID");
    Check(request.GetProperty("reasoning").GetString() == "off", "Reasoning disabled");
    Check(!request.GetProperty("stream").GetBoolean() && !request.GetProperty("store").GetBoolean(), "No streaming or stored chat");
    Check(request.GetProperty("integrations").GetArrayLength() == 0, "No enabled tools");
    string prompt = request.GetProperty("input").GetString()!;
    Check(prompt.Contains(inventory.Name) && prompt.Contains("OffTrack") && prompt.Contains("20/25"), "C# results included");
    Check(prompt.Contains(inventory.RiskScenario) && prompt.Contains(inventory.RiskPlan) && prompt.Contains(inventory.RiskOwner), "Risk facts included");
    Check(!prompt.Contains(inventory.TaskTitle), "Prompt contains selected risk, not unrelated work details");
    Check(inventory.RiskStatus == ItemStatus.Open && inventory.RiskLikelihood == 4 && inventory.RiskImpact == 5, "AI changes no ratings or status");
}
foreach (string models in new[] { "{\"models\":[]}", "{\"models\":[{\"key\":\"qwen/qwen3.5-9b\",\"loaded_instances\":[]}]}", "{\"models\":[{\"key\":\"other\",\"loaded_instances\":[{\"id\":\"wrong\"}]}]}" })
{
    using FakeBionic handler = new FakeBionic { Models = models };
    using HttpClient client = new HttpClient(handler);
    Check((await Tracker.ExplainRiskAsync(client, inventory)).Contains("not loaded") && handler.Posts == 0, "Missing loaded Qwen stops generation");
}
foreach (string models in new[] { "bad json", "{}", "{\"models\":null}", "{\"models\":{}}" })
{
    using FakeBionic handler = new FakeBionic { Models = models };
    using HttpClient client = new HttpClient(handler);
    Check((await Tracker.ExplainRiskAsync(client, inventory)).Contains("response") && handler.Posts == 0, "Bad preflight recovered");
}
foreach (string reply in new[] { "{\"output\":[]}", "{\"output\":[{\"type\":\"message\",\"content\":\" \"}]}" })
{
    using FakeBionic handler = new FakeBionic { Reply = reply };
    using HttpClient client = new HttpClient(handler);
    Check((await Tracker.ExplainRiskAsync(client, inventory)).Contains("no explanation"), "Empty answer reported");
}
foreach (string reply in new[] { "not json", "{}", "{\"output\":null}", "{\"output\":[{\"type\":\"message\",\"content\":42}]}" })
{
    using FakeBionic handler = new FakeBionic { Reply = reply };
    using HttpClient client = new HttpClient(handler);
    Check((await Tracker.ExplainRiskAsync(client, inventory)).Contains("response"), "Bad response recovered");
}
foreach (string failure in new[] { "http", "network", "timeout" })
{
    using FakeBionic handler = new FakeBionic { Failure = failure };
    using HttpClient client = new HttpClient(handler);
    Check((await Tracker.ExplainRiskAsync(client, inventory)).Contains(failure == "timeout" ? "too long" : "could not complete"), "Expected failure recovered");
}
using (FakeBionic handler = new FakeBionic())
using (HttpClient client = new HttpClient(handler))
{
    inventory.ChangeStatus(3, ItemStatus.Closed);
    Check((await Tracker.ExplainRiskAsync(client, inventory)).Contains("open risk"), "Closed risk rejected");
    inventory.ChangeStatus(3, ItemStatus.Open);
    foreach (int rating in new[] { 0, 6 })
    {
        inventory.RiskLikelihood = rating;
        Check((await Tracker.ExplainRiskAsync(client, inventory)).Contains("between 1 and 5"), "Invalid likelihood rejected");
        inventory.RiskLikelihood = 4;
        inventory.RiskImpact = rating;
        Check((await Tracker.ExplainRiskAsync(client, inventory)).Contains("between 1 and 5"), "Invalid impact rejected");
        inventory.RiskImpact = 5;
    }
    Check(handler.Requests == 0, "Invalid inputs never contact model");
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
string transcript = await RunConsole("1\n2\n2\n3\n2\n3\n2\n1\n3\n3\n2\n3\n1\n3\n2\n3\n1\n4\n2\nn\n5\n");
foreach (string expected in new[] { "1 OnTrack, 1 AtRisk, 1 OffTrack", "Task: Create item classes", "Risk: Scanner", "Priority score: 20/25", "2 OnTrack, 1 AtRisk, 0 OffTrack", "3 OnTrack, 0 AtRisk, 0 OffTrack", "Updated to Open. Project health: OffTrack", "No AI request was sent" })
    Check(transcript.Contains(expected), "Console demo: " + expected);
Check(transcript.EndsWith("Goodbye." + Environment.NewLine), "Exit ends Main");
transcript = await RunConsole("wrong\n2\n999\n3\n2\n203\n3\n2\n3\n99\n4\n1\n5\n");
foreach (string expected in new[] { "number from 1 to 5", "project was not found", "Choose section", "not a valid status", "risk is closed" })
    Check(transcript.Contains(expected), "Invalid input handled: " + expected);
foreach (string input in new[] { "", "2\n", "3\n2\n", "3\n2\n3\n", "4\n2\n", "2\n0\n5\n", "3\n2\n0\n5\n", "3\n2\n3\n0\n5\n" })
    Check((await RunConsole(input)).Contains("Goodbye."), "EOF/cancel returns safely");
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

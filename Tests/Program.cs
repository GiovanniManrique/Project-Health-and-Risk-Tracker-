using System.Net;
using System.Text;
using System.Text.Json;
using ProjectHealthTracker;
using ProjectHealthTracker.Models;
using Tracker = ProjectHealthTracker.Program;

if (args.Contains("--live"))
{
    int index = Array.IndexOf(args, "--live");
    await LiveChecks.RunAsync(index + 1 < args.Length ? args[index + 1] : null);
    return;
}

int passed = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new Exception("FAILED: " + message);
    passed++;
}

// Preserve the original tracker rules while supporting several risks.
List<Project> samples = Tracker.CreateSampleProjects();
Check(samples.Count == 3, "Three fictional sample projects");
Check(samples[0].CalculateHealth() == HealthStatus.OnTrack, "Website starts OnTrack");
Check(samples[1].CalculateHealth() == HealthStatus.OffTrack, "Inventory starts OffTrack");
Check(samples[2].CalculateHealth() == HealthStatus.AtRisk, "Training starts AtRisk");
Check(samples[1].Risks.Count == 3, "Inventory provides multiple risks to rank");
Check(samples[1].GetDetails().Contains("not a percentage"), "Base score is not called a probability");

Project project = new Project(99, "Offline checks", "Test manager");
Risk low = new Risk("LOW", "Low impact", "Alex") { Likelihood = 5, Impact = 3 };
Risk high = new Risk("HIGH", "High impact", "Sam") { Likelihood = 1, Impact = 4 };
project.Risks.Add(low);
project.Risks.Add(high);
Check(project.TaskStatus == ItemStatus.NotStarted && low.Status == ItemStatus.Closed, "Safe defaults");
foreach (int section in new[] { -1, 0, 3, 4 })
    Check(!project.ChangeStatus(section, ItemStatus.Open), "Invalid work section preserves state");
foreach (ItemStatus status in new[] { ItemStatus.NotStarted, ItemStatus.InProgress, ItemStatus.Completed, ItemStatus.Blocked })
{
    Check(project.ChangeStatus(1, status) && project.TaskStatus == status, "Allowed task status");
    Check(project.ChangeStatus(2, status) && project.MilestoneStatus == status, "Allowed milestone status");
    Check(!low.ChangeStatus(status), "Risk rejects work status");
}
Check(!project.ChangeStatus(1, ItemStatus.Open) && !project.ChangeStatus(2, ItemStatus.Closed), "Work rejects risk status");
Check(!low.ChangeStatus((ItemStatus)999) && low.Status == ItemStatus.Closed, "Undefined risk status preserves state");
low.ChangeStatus(ItemStatus.Open);
Check(project.CalculateHealth() == HealthStatus.AtRisk, "Any open low-impact risk gives AtRisk");
high.ChangeStatus(ItemStatus.Open);
Check(project.CalculateHealth() == HealthStatus.OffTrack && high.CalculateScore() == 4, "Later high-impact risk wins independently of score");
high.ChangeStatus(ItemStatus.Closed);
Check(project.CalculateHealth() == HealthStatus.AtRisk, "Closing high risk leaves another open risk");
low.ChangeStatus(ItemStatus.Closed);
project.ChangeStatus(2, ItemStatus.InProgress);
project.MilestoneDueDate = DateTime.Today.AddDays(-1);
Check(project.CalculateHealth() == HealthStatus.AtRisk, "Late unfinished milestone still warns with no open risks");
project.ChangeStatus(2, ItemStatus.Completed);
Check(project.CalculateHealth() == HealthStatus.OnTrack, "Completed late milestone ignored");
project.ChangeStatus(2, ItemStatus.InProgress);
project.MilestoneDueDate = DateTime.Today;
Check(project.CalculateHealth() == HealthStatus.OnTrack, "Due today is not overdue");
project.MilestoneDueDate = null;
project.TaskDueDate = DateTime.Today.AddDays(-10);
Check(project.CalculateHealth() == HealthStatus.OnTrack, "Missing milestone date and late task do not invent a warning");
foreach (int rating in new[] { 1, 5 })
{
    low.Likelihood = rating;
    low.Impact = rating;
    Check(low.CalculateScore() == rating * rating, "Risk score boundaries");
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
    finally
    {
        Console.SetIn(originalInput);
        Console.SetOut(originalOutput);
    }
}

string transcript = await RunConsole("1\n2\n2\n3\n2\n3\nINV-1\n2\n1\n5\n");
Check(transcript.Contains("1 OnTrack, 1 AtRisk, 1 OffTrack"), "Console lists original sample health states");
Check(transcript.Contains("INV-1") && transcript.Contains("INV-2") && transcript.Contains("INV-3"), "Console details show all inventory risks");
Check(transcript.Contains("1 OnTrack, 2 AtRisk, 0 OffTrack"), "Closing one inventory risk leaves other risks open");
Check(transcript.Contains("4.") && transcript.Contains("AI"), "Console exposes the AI feature");
Check(transcript.EndsWith("Goodbye." + Environment.NewLine), "Option 5 exits");
foreach (string input in new[] { "", "2\n", "3\n2\n", "3\n2\n3\n", "4\n", "2\n0\n5\n", "3\n2\n0\n5\n", "4\n0\n5\n" })
    Check((await RunConsole(input)).Contains("Goodbye."), "EOF/cancel returns safely without contacting a model");
transcript = await RunConsole("wrong\n2\n999\n3\n2\n203\n2\nabc\n3\n2\nabc\n1\n5\n");
Check(transcript.Contains("number from 1 to 5") && transcript.Contains("project was not found"), "Invalid menu and project input handled");
Check(transcript.Contains("Choose section") && transcript.Contains("Please enter a section number"), "Invalid section input handled");
Check(transcript.Contains("1 OnTrack, 1 AtRisk, 1 OffTrack"), "Rejected selections leave sample states unchanged");

// This HTTP substitute exercises model replies without contacting any server.
string evidenceDirectory = Path.Combine(Path.GetTempPath(), "tracker-checks-" + Guid.NewGuid());
Directory.CreateDirectory(evidenceDirectory);
try
{
    File.WriteAllText(Path.Combine(evidenceDirectory, "DEMO-EMAIL.eml"), "From: supplier@example.invalid\nSubject: Fictional delay\n\nFAKE-EMAIL-CONTENT: delivery delayed by 7 days.");
    File.WriteAllText(Path.Combine(evidenceDirectory, "DEMO-NOTES.txt"), "FAKE-DOCUMENT-CONTENT: the backup plan has not been approved.");
    File.WriteAllText(Path.Combine(evidenceDirectory, "DEMO-METRICS.json"), "{\"plannedTasks\":10,\"completedTasks\":4,\"budgetUsed\":0.82}");

    Project MakeProject()
    {
        Project result = new Project(8, "Fictional model test", "Test manager")
        {
            MilestoneTitle = "Demo checkpoint",
            MilestoneOwner = "Jordan",
            MilestoneDueDate = DateTime.Today.AddDays(2),
            EvidenceFiles = new List<string> { "DEMO-EMAIL.eml", "DEMO-NOTES.txt", "DEMO-METRICS.json" }
        };
        Risk first = new Risk("HIGH-1", "Supplier delay", "Chris")
        {
            Likelihood = 2, Impact = 4, Scenario = "Supplier date unknown", Plan = "",
            SourceIds = new List<string> { "DEMO-EMAIL", "DEMO-METRICS" }
        };
        Risk second = new Risk("LOW-1", "Backup approval", "Sam")
        {
            Likelihood = 1, Impact = 2, Scenario = "Approval pending", Plan = "Ask the manager",
            SourceIds = new List<string> { "DEMO-NOTES", "DEMO-METRICS" }
        };
        first.ChangeStatus(ItemStatus.Open);
        second.ChangeStatus(ItemStatus.Open);
        result.Risks.Add(first);
        result.Risks.Add(second);
        result.Risks.Add(new Risk("CLOSED-1", "Resolved issue", "Mia") { SourceIds = new List<string> { "DEMO-EMAIL" } });
        return result;
    }

    object Rating(string id, int likelihood, int impact, string source)
    {
        return new { id, likelihood, impact, reason = "The supplied fictional evidence supports this rating.", sourceIds = new[] { source }, action = "Confirm the supplied plan with the owner." };
    }
    string Reply(params object[] risks)
    {
        return JsonSerializer.Serialize(new { summary = "Review the supplied evidence with the owners.", missingInformation = new[] { "Mitigation plan details are missing." }, risks });
    }
    string validReply = Reply(Rating("LOW-1", 2, 3, "DEMO-NOTES"), Rating("HIGH-1", 4, 5, "DEMO-EMAIL"));
    Project inputProject = MakeProject();
    string before = JsonSerializer.Serialize(inputProject);
    using StubHandler success = new StubHandler(validReply);
    using HttpClient client = new HttpClient(success);
    AiRiskService service = new AiRiskService(client, "http://127.0.0.1:1234/v1", "test-gemma");
    string brief = await service.CreateBriefAsync(inputProject, evidenceDirectory);
    Check(success.Calls == 1 && success.Method == HttpMethod.Post && success.Url!.AbsolutePath == "/v1/chat/completions", "One bounded chat request");
    using (JsonDocument request = JsonDocument.Parse(success.Body))
    {
        Check(request.RootElement.GetProperty("model").GetString() == "test-gemma", "Configured local model requested");
        Check(request.RootElement.GetProperty("response_format").GetProperty("type").GetString() == "json_schema", "Structured output requested");
        JsonElement riskSchema = request.RootElement.GetProperty("response_format").GetProperty("json_schema")
            .GetProperty("schema").GetProperty("properties").GetProperty("risks");
        Check(riskSchema.GetProperty("minItems").GetInt32() == 2 && riskSchema.GetProperty("maxItems").GetInt32() == 2,
            "Generation is constrained to exactly the two open risks");
        HashSet<string?> allowedIds = riskSchema.GetProperty("items").GetProperty("properties").GetProperty("id")
            .GetProperty("enum").EnumerateArray().Select(id => id.GetString()).ToHashSet();
        Check(allowedIds.SetEquals(new[] { "HIGH-1", "LOW-1" }), "Generation permits only current open IDs, excluding closed risks");
        string sent = string.Join("\n", request.RootElement.GetProperty("messages").EnumerateArray().Select(message => message.GetProperty("content").GetString()));
        Check(sent.Contains("FAKE-EMAIL-CONTENT") && sent.Contains("FAKE-DOCUMENT-CONTENT") && sent.Contains("0.82"), "Actual email, document, and metric file contents ingested");
        Check(sent.Contains("HIGH-1") && sent.Contains("LOW-1") && !sent.Contains("CLOSED-1"), "Only open risks supplied for assessment");
    }
    Check(brief.Contains("1. HIGH-1:") && brief.Contains("2. LOW-1:"), "C# ranks scores despite the model's response order");
    Check(brief.Contains("20/25") && brief.Contains("6/25"), "C# multiplies model likelihood and impact");
    Check(brief.Contains("test-gemma") && brief.Contains("DEMO-EMAIL"), "Model and evidence citations displayed");
    Check(brief.Contains("Mitigation plan details are missing"), "Missing mitigation information displayed");
    Check(JsonSerializer.Serialize(inputProject) == before, "AI estimates do not mutate project, base ratings, or health");

    string omittedGapReply = JsonSerializer.Serialize(new
    {
        summary = "Review the supplied evidence.",
        missingInformation = Array.Empty<string>(),
        risks = new[] { Rating("HIGH-1", 4, 5, "DEMO-EMAIL"), Rating("LOW-1", 2, 3, "DEMO-NOTES") }
    });
    using StubHandler omittedGap = new StubHandler(omittedGapReply);
    using HttpClient omittedGapClient = new HttpClient(omittedGap);
    string gapBrief = await new AiRiskService(omittedGapClient, "http://127.0.0.1:1234/v1", "test-gemma")
        .CreateBriefAsync(MakeProject(), evidenceDirectory);
    Check(gapBrief.Contains("Recorded data gaps (C# checks):") && gapBrief.Contains("- [HIGH-1] No mitigation plan recorded."),
        "C# always discloses an empty open-risk plan even when the model omits it");
    Check(gapBrief.Contains("- None flagged by the model."), "Recorded C# gaps are not misrepresented as model-generated notes");

    inputProject.Risks[0].ChangeStatus(ItemStatus.Closed);
    using StubHandler afterClose = new StubHandler(Reply(Rating("LOW-1", 2, 3, "DEMO-NOTES")));
    using HttpClient afterClient = new HttpClient(afterClose);
    string afterBrief = await new AiRiskService(afterClient, "http://127.0.0.1:1234/v1", "test-gemma").CreateBriefAsync(inputProject, evidenceDirectory);
    Check(afterBrief.Contains("LOW-1") && !afterBrief.Contains("20/25"), "Closed risk excluded from the next assessment");
    Check(inputProject.CalculateHealth() == HealthStatus.AtRisk, "Closing one risk leaves the other risk's deterministic health");

    foreach (bool late in new[] { false, true })
    {
        Project empty = MakeProject();
        foreach (Risk risk in empty.Risks) risk.ChangeStatus(ItemStatus.Closed);
        empty.MilestoneDueDate = late ? DateTime.Today.AddDays(-3) : DateTime.Today.AddDays(2);
        string noRiskReply = JsonSerializer.Serialize(new
        {
            summary = "Review the supplied milestone and tracker health.",
            missingInformation = late ? new[] { "The next review date is not confirmed." } : Array.Empty<string>(),
            risks = Array.Empty<object>()
        });
        using StubHandler noRisks = new StubHandler(noRiskReply);
        using HttpClient noRiskClient = new HttpClient(noRisks);
        string emptyBrief = await new AiRiskService(noRiskClient, "http://127.0.0.1:1234/v1", "test-gemma").CreateBriefAsync(empty, evidenceDirectory);
        Check(emptyBrief.Contains("No open risks") && noRisks.Calls == 1, "No-open-risk project receives one brief without invented ratings");
        using (JsonDocument request = JsonDocument.Parse(noRisks.Body))
        {
            JsonElement riskSchema = request.RootElement.GetProperty("response_format").GetProperty("json_schema")
                .GetProperty("schema").GetProperty("properties").GetProperty("risks");
            Check(riskSchema.GetProperty("minItems").GetInt32() == 0 && riskSchema.GetProperty("maxItems").GetInt32() == 0,
                "Generation requires an empty risk list when the tracker has no open risks");
            int missingLimit = request.RootElement.GetProperty("response_format").GetProperty("json_schema")
                .GetProperty("schema").GetProperty("properties").GetProperty("missingInformation").GetProperty("maxItems").GetInt32();
            Check(late ? missingLimit > 0 && missingLimit <= 5 : missingLimit == 0,
                late ? "Overdue milestone still permits relevant information gaps" : "Healthy no-risk project cannot generate information gaps");
        }
        Check(empty.CalculateHealth() == (late ? HealthStatus.AtRisk : HealthStatus.OnTrack), "No-open-risk brief preserves milestone-based health");
        if (late)
            Check(noRisks.Body.Contains("Demo checkpoint") && noRisks.Body.Contains("AtRisk"), "Late milestone context reaches the model");
    }

    async Task Reject(string response, string label, HttpStatusCode status = HttpStatusCode.OK, Exception? failure = null)
    {
        Project rejectedProject = MakeProject();
        string unchanged = JsonSerializer.Serialize(rejectedProject);
        using StubHandler handler = new StubHandler(response, status, failure);
        using HttpClient rejectedClient = new HttpClient(handler);
        try
        {
            await new AiRiskService(rejectedClient, "http://127.0.0.1:1234/v1", "test-gemma").CreateBriefAsync(rejectedProject, evidenceDirectory);
            Check(false, "Reject " + label);
        }
        catch (InvalidOperationException error)
        {
            Check(!string.IsNullOrWhiteSpace(error.Message), "Clear error for " + label);
        }
        Check(handler.Calls == 1, "No hidden retries for " + label);
        Check(JsonSerializer.Serialize(rejectedProject) == unchanged, "Rejected " + label + " preserves data");
    }

    await Reject(Reply(Rating("HIGH-1", 4, 5, "DEMO-EMAIL")), "missing risk");
    await Reject(Reply(Rating("UNKNOWN", 4, 5, "DEMO-EMAIL"), Rating("LOW-1", 2, 3, "DEMO-NOTES")), "unknown risk");
    await Reject(Reply(Rating("HIGH-1", 4, 5, "DEMO-EMAIL"), Rating("HIGH-1", 2, 3, "DEMO-EMAIL")), "duplicate risk");
    await Reject(Reply(Rating("HIGH-1", 4, 5, "DEMO-EMAIL"), Rating("LOW-1", 2, 3, "DEMO-NOTES"), Rating("CLOSED-1", 2, 3, "DEMO-EMAIL")), "closed risk");
    foreach (int invalidRating in new[] { 0, 6 })
        await Reject(Reply(Rating("HIGH-1", invalidRating, 5, "DEMO-EMAIL"), Rating("LOW-1", 2, 3, "DEMO-NOTES")), "out-of-range rating " + invalidRating);
    await Reject(Reply(Rating("HIGH-1", 4, 5, "INVENTED-SOURCE"), Rating("LOW-1", 2, 3, "DEMO-NOTES")), "unknown citation");
    await Reject(Reply(Rating("HIGH-1", 4, 5, "DEMO-NOTES"), Rating("LOW-1", 2, 3, "DEMO-NOTES")), "citation assigned to another risk");
    await Reject("", "empty content");
    await Reject("not JSON", "malformed JSON");
    await Reject("{}", "missing required fields");
    await Reject("{\"summary\":\"Test\",\"missingInformation\":[],\"risks\":[null,null]}", "null risk entries");
    await Reject(validReply, "unavailable model", HttpStatusCode.ServiceUnavailable);
    await Reject(validReply, "connection failure", failure: new HttpRequestException("Offline test"));
    await Reject(validReply, "timeout", failure: new TaskCanceledException("Offline test timeout"));

    using StubHandler missingFile = new StubHandler(validReply);
    using HttpClient missingFileClient = new HttpClient(missingFile);
    try
    {
        Project missing = MakeProject();
        missing.EvidenceFiles.Add("missing.txt");
        await new AiRiskService(missingFileClient, "http://127.0.0.1:1234/v1", "test-gemma").CreateBriefAsync(missing, evidenceDirectory);
        Check(false, "Missing evidence is reported");
    }
    catch (IOException error)
    {
        Check(!string.IsNullOrWhiteSpace(error.Message) && missingFile.Calls == 0, "Missing evidence fails clearly before a model request");
    }
}
finally
{
    // Only this test's unique temporary fixture directory is removed.
    string temporaryRoot = Path.GetFullPath(Path.GetTempPath());
    string target = Path.GetFullPath(evidenceDirectory);
    if (target.StartsWith(temporaryRoot, StringComparison.OrdinalIgnoreCase) &&
        Path.GetFileName(target).StartsWith("tracker-checks-"))
        Directory.Delete(target, true);
}

Console.WriteLine($"PASS: {passed} offline checks. No live model requests were made.");

class StubHandler : HttpMessageHandler
{
    private readonly string content;
    private readonly HttpStatusCode status;
    private readonly Exception? failure;
    public int Calls { get; private set; }
    public string Body { get; private set; } = "";
    public HttpMethod? Method { get; private set; }
    public Uri? Url { get; private set; }

    public StubHandler(string content, HttpStatusCode status = HttpStatusCode.OK, Exception? failure = null)
    {
        this.content = content;
        this.status = status;
        this.failure = failure;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        Method = request.Method;
        Url = request.RequestUri;
        Body = request.Content == null ? "" : await request.Content.ReadAsStringAsync(cancellationToken);
        if (failure != null) throw failure;
        string envelope = JsonSerializer.Serialize(new { choices = new[] { new { message = new { content } } } });
        return new HttpResponseMessage(status) { Content = new StringContent(envelope, Encoding.UTF8, "application/json") };
    }
}

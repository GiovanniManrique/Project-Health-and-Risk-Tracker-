using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ProjectHealthTracker;
using ProjectHealthTracker.Models;
using Tracker = ProjectHealthTracker.Program;

// Optional evaluation helper. The normal test command never calls a live model.
static class LiveChecks
{
    public static async Task RunAsync(string? scenario)
    {
        if (scenario != null && !new[] { "1", "2", "3", "4", "5" }.Contains(scenario))
            throw new InvalidOperationException("Choose a live scenario from 1 to 5, or omit the number to run all five.");
        string repository = Directory.GetCurrentDirectory();
        while (!File.Exists(Path.Combine(repository, "ProjectHealthTracker.slnx")))
        {
            repository = Directory.GetParent(repository)?.FullName
                ?? throw new InvalidOperationException("Run live checks inside the repository.");
        }
        string evidenceDirectory = Path.Combine(repository, "ProjectHealthTracker", "DemoData");
        string evaluationDirectory = Path.Combine(repository, "evaluation");
        Directory.CreateDirectory(evaluationDirectory);
        string outputFile = Path.Combine(evaluationDirectory,
            scenario == null ? "live-model-checks.txt" : $"live-model-checks-{scenario}.txt");
        string temporaryEvidence = Path.Combine(Path.GetTempPath(), "tracker-live-" + Guid.NewGuid());
        Directory.CreateDirectory(temporaryEvidence);
        StringBuilder transcript = new StringBuilder();
        void Record(string text)
        {
            Console.WriteLine(text);
            transcript.AppendLine(text);
        }

        string baseUrl = Environment.GetEnvironmentVariable("BIONIC_BASE_URL") ?? "http://127.0.0.1:1235/v1";
        string model = Environment.GetEnvironmentVariable("BIONIC_MODEL") ?? "google/gemma-4-e2b";
        using CountingHandler handler = new CountingHandler();
        using HttpClient client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(120) };
        AiRiskService service = new AiRiskService(client, baseUrl, model);
        Record("LIVE LOCAL MODEL EVALUATION");
        Record("Started (computer local time): " + DateTimeOffset.Now.ToString("O"));
        Record("Model requested: " + model);
        Record("Input data: fictional local email, notes, and metrics. No real inbox used.");
        Record("Each scenario makes one request. Ratings may vary between model runs.");

        async Task RunCase(string name, Project project, string directory)
        {
            if (scenario != null && !name.StartsWith(scenario + ".", StringComparison.Ordinal)) return;
            Record("\n=== " + name + " ===");
            Record("Before: stored health " + project.CalculateHealth() + "; open risks: " +
                string.Join(", ", project.Risks.Where(risk => risk.Status == ItemStatus.Open).Select(risk => risk.Id)));
            string before = JsonSerializer.Serialize(project);
            int callsBefore = handler.Calls;
            Stopwatch elapsed = Stopwatch.StartNew();
            string brief = await service.CreateBriefAsync(project, directory);
            Record(brief);
            Require(handler.Calls == callsBefore + 1, "Exactly one model request");
            Require(before == JsonSerializer.Serialize(project), "Model leaves all stored data and health unchanged");
            List<string> expectedIds = project.Risks.Where(risk => risk.Status == ItemStatus.Open).Select(risk => risk.Id).ToList();
            MatchCollection rows = Regex.Matches(brief,
                @"^\d+\. (\S+): [^\r\n]*\r?\nLikelihood: (\d)/5 \| Impact: (\d)/5 \| Priority score: (\d+)/25", RegexOptions.Multiline);
            Require(rows.Count == expectedIds.Count, "Exactly the current open risks are ranked");
            int previousScore = 26;
            foreach (Match row in rows)
            {
                string id = row.Groups[1].Value;
                Require(expectedIds.Remove(id), "Known risk ID appears once: " + id);
                int likelihood = int.Parse(row.Groups[2].Value);
                int impact = int.Parse(row.Groups[3].Value);
                int score = int.Parse(row.Groups[4].Value);
                Require(score == likelihood * impact && score <= previousScore, "Correct arithmetic and descending order: " + id);
                previousScore = score;
            }
            string actionsSection = brief.Split("Recommended actions", StringSplitOptions.None)[1]
                .Split("Missing information:", StringSplitOptions.None)[0]
                .Split("Recorded data gaps", StringSplitOptions.None)[0];
            int actions = Regex.Matches(actionsSection, @"^- \[", RegexOptions.Multiline).Count;
            Require(actions <= 3, "At most three risk actions");
            Record($"PASS: one request; valid IDs, scores and order; at most three actions; unchanged data. Elapsed: {elapsed.Elapsed.TotalSeconds:F1}s.");
        }

        try
        {
            List<Project> samples = Tracker.CreateSampleProjects();
            Project inventory = samples[1];
            await RunCase("1. Inventory: multiple open risks", inventory, evidenceDirectory);

            inventory.Risks.First(risk => risk.Id == "INV-1").ChangeStatus(ItemStatus.Closed);
            await RunCase("2. Inventory: after closing INV-1", inventory, evidenceDirectory);

            await RunCase("3. Website: no open risks", samples[0], evidenceDirectory);
            await RunCase("4. Training: overdue milestone and no open risks", samples[2], evidenceDirectory);

            if (scenario == null || scenario == "5")
            {
                foreach (string path in Directory.GetFiles(evidenceDirectory))
                    File.Copy(path, Path.Combine(temporaryEvidence, Path.GetFileName(path)));
                Project missingPlan = Tracker.CreateSampleProjects()[1];
                missingPlan.Risks.First(risk => risk.Id == "INV-2").Plan = "";
                string notesPath = Path.Combine(temporaryEvidence, "INV-NOTES.txt");
                string notes = File.ReadAllText(notesPath);
                string oldPlan = "Current plan: review extra costs and request approval before placing new orders.";
                Require(notes.Contains(oldPlan), "Missing-mitigation fixture matches current demo notes");
                File.WriteAllText(notesPath, notes.Replace(oldPlan, "Current plan: not recorded for INV-2."));
                await RunCase("5. Inventory: INV-2 mitigation plan deliberately missing", missingPlan, temporaryEvidence);
            }

            Record((scenario == null ? "\nPASS: all five live scenarios." : $"\nPASS: selected live scenario {scenario}.") +
                " Total model requests: " + handler.Calls);
            Record("Human review: confirm factual grounding, useful actions, change after closing INV-1, explicit late-milestone mention, and missing INV-2 mitigation flag.");
            Record("Unavailable-model, timeout, malformed-response, and citation failures are covered by the offline suite; they are not simulated successful AI results here.");
        }
        catch (Exception error)
        {
            Record("\nFAIL: " + error.GetType().Name + ": " + error.Message);
            throw;
        }
        finally
        {
            File.WriteAllText(outputFile, transcript.ToString());
            Console.WriteLine("Evaluation transcript: " + outputFile);
            // Remove only this run's unique temporary copy of the fictional data.
            string temporaryRoot = Path.GetFullPath(Path.GetTempPath());
            string target = Path.GetFullPath(temporaryEvidence);
            if (target.StartsWith(temporaryRoot, StringComparison.OrdinalIgnoreCase) &&
                Path.GetFileName(target).StartsWith("tracker-live-"))
                Directory.Delete(target, true);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Live evaluation failed: " + message);
    }
}

class CountingHandler : DelegatingHandler
{
    public int Calls { get; private set; }

    public CountingHandler() : base(new HttpClientHandler()) { }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        return base.SendAsync(request, cancellationToken);
    }
}

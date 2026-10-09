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

string RunConsole(string input)
{
    TextReader originalInput = Console.In;
    TextWriter originalOutput = Console.Out;
    using StringWriter output = new StringWriter();
    try
    {
        Console.SetIn(new StringReader(input));
        Console.SetOut(output);
        Tracker.Main();
        return output.ToString();
    }
    finally { Console.SetIn(originalInput); Console.SetOut(originalOutput); }
}
string transcript = RunConsole("1\n2\n2\n3\n2\n3\n2\n1\n3\n3\n2\n3\n1\n3\n2\n3\n1\n4\n");
foreach (string expected in new[] { "1 OnTrack, 1 AtRisk, 1 OffTrack", "Task: Create item classes", "Risk: Scanner", "Priority score: 20/25", "2 OnTrack, 1 AtRisk, 0 OffTrack", "3 OnTrack, 0 AtRisk, 0 OffTrack", "Updated to Open. Project health: OffTrack" })
    Check(transcript.Contains(expected), "Console demo: " + expected);
Check(transcript.EndsWith("Goodbye." + Environment.NewLine), "Exit ends Main");
transcript = RunConsole("wrong\n2\n999\n3\n2\n203\n3\n2\n3\n99\n4\n");
foreach (string expected in new[] { "number from 1 to 4", "project was not found", "Choose section", "not a valid status" })
    Check(transcript.Contains(expected), "Invalid input handled: " + expected);
foreach (string input in new[] { "", "2\n", "3\n2\n", "3\n2\n3\n", "2\n0\n4\n", "3\n2\n0\n4\n", "3\n2\n3\n0\n4\n" })
    Check((RunConsole(input)).Contains("Goodbye."), "EOF/cancel returns safely");
transcript = RunConsole("2\nabc\n3\n2\nabc\n3\n2\n3\nabc\n1\n4\n");
Check(transcript.Contains("project was not found") && transcript.Contains("Please enter a section number") && transcript.Contains("Please enter a status number"), "Separate conversion checks reject text at every selection");
Check(transcript.Contains("1 OnTrack, 1 AtRisk, 1 OffTrack"), "Rejected selections leave original project states unchanged");
Check(!transcript.Contains("local AI") && !transcript.Contains("Qwen"), "Menu has no model option");
Check(RunConsole("4\n").EndsWith("Goodbye." + Environment.NewLine), "Option 4 exits immediately");
Check(RunConsole("5\n4\n").Contains("number from 1 to 4"), "Old exit number is rejected safely");
Console.WriteLine($"PASS: {passed} checks.");

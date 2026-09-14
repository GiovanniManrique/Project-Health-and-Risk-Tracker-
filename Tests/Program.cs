// Purpose: Checks the tracker without installing a testing library.
// Data: Counts checks that pass or fail and creates fresh sample data for each group.
// Methods: Checks business rules and runs the console menu with sample input.

using ProjectHealthTracker.Data;
using ProjectHealthTracker.Models;
using ProjectHealthTracker.Services;

namespace TrackerChecks;

public class Program
{
    private static int passed = 0;
    private static int failed = 0;

    public static int Main()
    {
        CheckHealthRules();
        CheckStatusUpdates();
        CheckMenu();
        CheckStatusMenuNumbers();
        Console.WriteLine($"Results: {passed} passed, {failed} failed.");
        return failed == 0 ? 0 : 1;
    }

    private static void Check(bool condition, string description)
    {
        if (condition)
        {
            passed++;
            Console.WriteLine("PASS: " + description);
        }
        else
        {
            failed++;
            Console.WriteLine("FAIL: " + description);
        }
    }

    private static void CheckHealthRules()
    {
        List<Project> projects = MockProjectData.GetProjects();
        ProjectService service = new ProjectService(projects);
        Check(projects.Count == 3, "Three sample projects are available");
        Check(service.CalculateHealth(projects[0]) == HealthStatus.OnTrack, "Website starts OnTrack");
        Check(service.CalculateHealth(projects[1]) == HealthStatus.OffTrack, "Inventory starts OffTrack");
        Check(service.CalculateHealth(projects[2]) == HealthStatus.AtRisk, "Training starts AtRisk");

        Project project = new Project(10, "Boundary checks", "Tester", DateTime.Today, DateTime.Today.AddDays(10));
        Check(service.CalculateHealth(project) == HealthStatus.OnTrack, "Empty project is OnTrack");
        Milestone milestone = new Milestone(1, "Review", "Tester", ItemStatus.NotStarted, DateTime.Today.AddHours(12), false);
        project.Items.Add(milestone);
        Check(service.CountLateMilestones(project) == 0, "A milestone due today is not late");
        milestone.TargetDate = DateTime.Today.AddDays(-1);
        Check(service.CalculateHealth(project) == HealthStatus.AtRisk, "Yesterday's unfinished milestone is AtRisk");
        milestone.IsAchieved = true;
        milestone.Status = ItemStatus.Completed;
        Check(service.CalculateHealth(project) == HealthStatus.OnTrack, "An achieved milestone is not late");

        Risk risk = new Risk(2, "Delay", "Tester", ItemStatus.Open, 1, 3, "Make a backup plan");
        project.Items.Add(risk);
        Check(service.CalculateHealth(project) == HealthStatus.AtRisk, "Open impact 3 risk is AtRisk");
        risk.Impact = 4;
        Check(service.CalculateHealth(project) == HealthStatus.OffTrack, "Impact 4 is the high-impact boundary");
        risk.Impact = 5;
        Check(service.CalculateHealth(project) == HealthStatus.OffTrack, "Impact 5 is OffTrack even with low probability");
        milestone.IsAchieved = false;
        milestone.Status = ItemStatus.InProgress;
        Check(service.CalculateHealth(project) == HealthStatus.OffTrack, "High-impact open risk takes priority over a late milestone");
        risk.Status = ItemStatus.Closed;
        Check(service.CalculateHealth(project) == HealthStatus.AtRisk, "Closing a risk still leaves a late milestone AtRisk");
        Check(service.CountOpenRisks(project) == 0 && service.GetOpenRisks(project).Count == 0, "Closed risks are excluded");
        risk.Status = ItemStatus.Open;
        project.Items.Add(new Risk(3, "Another risk", "Tester", ItemStatus.Open, 2, 2, "Monitor it"));
        Check(service.CountOpenRisks(project) == 2 && service.GetOpenRisks(project).Count == 2, "Multiple open risks are counted");
    }

    private static void CheckStatusUpdates()
    {
        List<Project> projects = MockProjectData.GetProjects();
        ProjectService service = new ProjectService(projects);
        ProjectTask task = (ProjectTask)projects[1].Items[0];
        Milestone milestone = (Milestone)projects[2].Items[1];
        Check(service.GetProjectById(999) == null, "Unknown project is not found");
        Check(!service.UpdateItemStatus(999, 201, ItemStatus.Completed), "Unknown project update is rejected");
        Check(!service.UpdateItemStatus(1, 201, ItemStatus.Completed), "Item from another project is rejected");
        Check(!service.UpdateItemStatus(2, 201, ItemStatus.Closed), "Task cannot use Closed");
        Check(!service.UpdateItemStatus(2, 203, ItemStatus.Completed), "Risk cannot use Completed");
        Check(!service.UpdateItemStatus(2, 201, (ItemStatus)999), "Undefined status is rejected");
        Check(task.Status == ItemStatus.InProgress && !task.IsCompleted, "Rejected changes preserve task state");
        Check(service.UpdateItemStatus(2, 201, ItemStatus.Completed) && task.IsCompleted, "Completing a task sets its completion flag");
        Check(service.UpdateItemStatus(2, 201, ItemStatus.Blocked) && !task.IsCompleted, "Reopening a task clears its completion flag");
        Check(!service.UpdateItemStatus(3, 302, ItemStatus.Open), "Milestone cannot use Open");
        Check(service.UpdateItemStatus(3, 302, ItemStatus.Completed) && milestone.IsAchieved, "Completing a milestone sets its achievement flag");
        Check(service.CalculateHealth(projects[2]) == HealthStatus.OnTrack, "Completing the late milestone restores OnTrack");
        Check(service.UpdateItemStatus(3, 302, ItemStatus.InProgress) && !milestone.IsAchieved, "Reopening a milestone clears its achievement flag");
        Check(service.CalculateHealth(projects[2]) == HealthStatus.AtRisk, "Reopening the late milestone restores AtRisk");
        Check(service.UpdateItemStatus(2, 203, ItemStatus.Closed), "Risk can be closed");
        Check(service.CalculateHealth(projects[1]) == HealthStatus.OnTrack, "Closing the only open risk restores OnTrack");
        Check(service.UpdateItemStatus(2, 203, ItemStatus.Open), "Risk can be reopened");
        Check(service.CalculateHealth(projects[1]) == HealthStatus.OffTrack, "Reopening the high-impact risk restores OffTrack");
    }

    private static string RunMenu(string input)
    {
        TextReader originalInput = Console.In;
        TextWriter originalOutput = Console.Out;
        using StringReader reader = new StringReader(input);
        using StringWriter writer = new StringWriter();
        try
        {
            Console.SetIn(reader);
            Console.SetOut(writer);
            ProjectHealthTracker.Program.Main();
            return writer.ToString();
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }
    }

    private static void CheckStatusMenuNumbers()
    {
        foreach (ProjectItem item in MockProjectData.GetProjects()[1].Items)
        {
            string[] statuses = { "NotStarted", "InProgress", "Completed", "Blocked" };
            if (item is Risk)
            {
                statuses = new string[] { "Open", "Closed" };
            }

            for (int i = 0; i < statuses.Length; i++)
            {
                string output = RunMenu($"3\n2\n{item.Id}\n{i + 1}\n2\n2\n6\n");
                string details = $"{item.Id}: {item.Title} | Owner: {item.Owner} | Status: {statuses[i]}";
                Check(output.Contains($"{i + 1}. {statuses[i]}") && output.Contains(details),
                    $"Menu choice {i + 1} sets item {item.Id} to {statuses[i]}");
            }
        }
    }

    private static void CheckMenu()
    {
        string output = RunMenu("1\n2\n2\n4\n2\n5\n6\n");
        Check(output.Contains("Company Website Update") && output.Contains("Employee Training Plan"), "Menu lists projects");
        Check(output.Contains("Probability: 4/5 | Impact: 5/5") && output.Contains("Plan: Use manual item numbers"), "Details and risks include scores and mitigation");
        Check(output.Contains("Totals - OnTrack: 1 | AtRisk: 1 | OffTrack: 1"), "Initial summary counts all three health states");
        output = RunMenu("3\n2\n203\n2\n4\n2\n5\n3\n3\n302\n3\n5\n6\n");
        Check(output.Contains("Updated Scanner hardware may arrive late to Closed."), "Menu closes a risk");
        Check(output.Contains("There are no open risks."), "Closed risk disappears from open risk view");
        Check(output.Contains("Totals - OnTrack: 3 | AtRisk: 0 | OffTrack: 0"), "Status updates are reflected in summary during the session");
        output = RunMenu("3\n2\n201\n3\n2\n2\n6\n");
        Check(output.Contains("Status: Completed") && output.Contains("Completed: True"), "Task completion appears in project details");
        output = RunMenu("oops\n2\nabc\n2\n999\n3\n2\nabc\n3\n2\n101\n3\n2\n203\n3\n3\n2\n201\n5\n3\n2\n201\nabc\n6\n");
        Check(output.Contains("not a valid menu choice"), "Invalid menu choice has a helpful message");
        Check(output.Contains("project ID must be a whole number") && output.Contains("A project with that ID was not found"), "Invalid project IDs are handled");
        Check(output.Contains("item ID must be a whole number") && output.Contains("not found in this project"), "Invalid item IDs are handled");
        Check(output.Contains("Choose a status from 1 to 2") && output.Contains("Choose a status from 1 to 4") && output.Contains("Enter one of the status numbers shown"), "Invalid status choices are handled");
        output = RunMenu("3\n2\n0\n3\n2\n203\n0\n5\n6\n");
        Check(output.Contains("Update canceled.") && output.Contains("Totals - OnTrack: 1 | AtRisk: 1 | OffTrack: 1"), "Cancel preserves data");
        Check(RunMenu(" 6 \n").Contains("Goodbye."), "Menu accepts surrounding spaces");
        Check(RunMenu("").Contains("Project Health and Risk Tracker"), "End of input exits without an infinite loop");
        Check(RunMenu("3\n2\n203\n").Contains("Update canceled."), "End of input during status choice cancels the update");
        Check(RunMenu("5\n6\n").Contains("Totals - OnTrack: 1 | AtRisk: 1 | OffTrack: 1"), "Restart restores sample data");
    }
}

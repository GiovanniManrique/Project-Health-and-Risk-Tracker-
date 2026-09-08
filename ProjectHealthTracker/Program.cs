// Purpose: Runs the console menu and handles the user's choices.
// Data: Uses mock projects stored in a List<Project>.
// Methods: Displays projects, details, risks, and health; reads and updates item status.

using ProjectHealthTracker.Data;
using ProjectHealthTracker.Models;
using ProjectHealthTracker.Services;

namespace ProjectHealthTracker;

public class Program
{
    public static void Main()
    {
        List<Project> projects = MockProjectData.GetProjects();
        ProjectService projectService = new ProjectService(projects);
        bool applicationRunning = true;

        Console.WriteLine("Project Health and Risk Tracker");
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Changes last until you exit. Sample data resets when you restart.");

        while (applicationRunning)
        {
            DisplayMenu();
            Console.Write("Choose an option: ");
            string? choice = Console.ReadLine();
            Console.WriteLine();

            // A null input means the console input has ended.
            if (choice == null)
            {
                break;
            }

            switch (choice.Trim())
            {
                case "1":
                    ListProjects(projectService);
                    break;

                case "2":
                    ViewProjectDetails(projectService);
                    break;

                case "3":
                    UpdateItemStatus(projectService);
                    break;

                case "4":
                    ShowProjectRisks(projectService);
                    break;

                case "5":
                    ShowHealthSummary(projectService);
                    break;

                case "6":
                    applicationRunning = false;
                    Console.WriteLine("Goodbye.");
                    break;

                default:
                    Console.WriteLine("That is not a valid menu choice. Please enter 1 through 6.");
                    break;
            }

            Console.WriteLine();
        }
    }

    private static void DisplayMenu()
    {
        Console.WriteLine("1. List projects");
        Console.WriteLine("2. View project details");
        Console.WriteLine("3. Update item status");
        Console.WriteLine("4. Show project risks");
        Console.WriteLine("5. Show health summary");
        Console.WriteLine("6. Exit");
    }

    private static void ListProjects(ProjectService projectService)
    {
        Console.WriteLine("Projects");

        foreach (Project project in projectService.GetAllProjects())
        {
            int openRiskCount = projectService.CountOpenRisks(project);
            Console.WriteLine($"{project.Id}. {project.Name}");
            Console.WriteLine($"   Manager: {project.Manager} | Open risks: {openRiskCount} | Health: {projectService.CalculateHealth(project)}");
        }
    }

    private static void ViewProjectDetails(ProjectService projectService)
    {
        Project? project = ReadProject(projectService);

        if (project == null)
        {
            return;
        }

        Console.WriteLine($"Project: {project.Name}");
        Console.WriteLine($"Manager: {project.Manager}");
        Console.WriteLine($"Dates: {project.StartDate:d} to {project.EndDate:d}");
        Console.WriteLine($"Health: {projectService.CalculateHealth(project)}");
        Console.WriteLine("Items:");

        if (project.Items.Count == 0)
        {
            Console.WriteLine("There are no items in this project.");
        }

        foreach (ProjectItem item in project.Items)
        {
            Console.WriteLine("- " + item.GetDetails());
        }
    }

    private static void ShowProjectRisks(ProjectService projectService)
    {
        Project? project = ReadProject(projectService);

        if (project == null)
        {
            return;
        }

        List<Risk> risks = projectService.GetOpenRisks(project);
        Console.WriteLine($"Open risks for {project.Name}:");

        if (risks.Count == 0)
        {
            Console.WriteLine("There are no open risks.");
        }
        else
        {
            foreach (Risk risk in risks)
            {
                Console.WriteLine("- " + risk.GetDetails());
            }
        }
    }

    private static void UpdateItemStatus(ProjectService projectService)
    {
        Project? project = ReadProject(projectService);

        if (project == null)
        {
            return;
        }

        Console.WriteLine($"Items for {project.Name}:");
        foreach (ProjectItem item in project.Items)
        {
            Console.WriteLine("- " + item.GetDetails());
        }

        if (project.Items.Count == 0)
        {
            Console.WriteLine("There are no items to update.");
            return;
        }

        Console.Write("Enter the item ID (0 to cancel): ");
        if (!int.TryParse(Console.ReadLine(), out int itemId))
        {
            Console.WriteLine("The item ID must be a whole number.");
            return;
        }

        if (itemId == 0)
        {
            Console.WriteLine("Update canceled.");
            return;
        }

        ProjectItem? selectedItem = projectService.GetItemById(project, itemId);
        if (selectedItem == null)
        {
            Console.WriteLine("An item with that ID was not found in this project.");
            return;
        }

        Console.WriteLine($"Current status: {selectedItem.Status}");
        Console.WriteLine("0. Cancel");
        if (selectedItem is Risk)
        {
            Console.WriteLine("1. Open");
            Console.WriteLine("2. Closed");
        }
        else
        {
            Console.WriteLine("1. Not started");
            Console.WriteLine("2. In progress");
            Console.WriteLine("3. Completed");
            Console.WriteLine("4. Blocked");
        }

        Console.Write("Choose the new status: ");
        string? input = Console.ReadLine();
        if (input == null || input.Trim() == "0")
        {
            Console.WriteLine("Update canceled.");
            return;
        }

        if (!int.TryParse(input, out int statusChoice))
        {
            Console.WriteLine("Enter one of the status numbers shown. Nothing was changed.");
            return;
        }

        ItemStatus newStatus;
        if (selectedItem is Risk)
        {
            if (statusChoice == 1)
            {
                newStatus = ItemStatus.Open;
            }
            else if (statusChoice == 2)
            {
                newStatus = ItemStatus.Closed;
            }
            else
            {
                Console.WriteLine("Choose 1 or 2 for a risk. Nothing was changed.");
                return;
            }
        }
        else
        {
            switch (statusChoice)
            {
                case 1:
                    newStatus = ItemStatus.NotStarted;
                    break;
                case 2:
                    newStatus = ItemStatus.InProgress;
                    break;
                case 3:
                    newStatus = ItemStatus.Completed;
                    break;
                case 4:
                    newStatus = ItemStatus.Blocked;
                    break;
                default:
                    Console.WriteLine("Choose 1 through 4 for a task or milestone. Nothing was changed.");
                    return;
            }
        }

        if (projectService.UpdateItemStatus(project.Id, selectedItem.Id, newStatus))
        {
            Console.WriteLine($"Updated {selectedItem.Title} to {newStatus}.");
            Console.WriteLine($"Project health is now {projectService.CalculateHealth(project)}.");
        }
        else
        {
            Console.WriteLine("The status could not be updated.");
        }
    }

    private static void ShowHealthSummary(ProjectService projectService)
    {
        int onTrackCount = 0;
        int atRiskCount = 0;
        int offTrackCount = 0;

        Console.WriteLine("Project health summary");
        Console.WriteLine("OffTrack: an open risk has impact 4 or 5.");
        Console.WriteLine("AtRisk: another risk is open, or an unfinished milestone is past its target date.");
        Console.WriteLine("OnTrack: neither condition applies.");
        Console.WriteLine();

        foreach (Project project in projectService.GetAllProjects())
        {
            HealthStatus health = projectService.CalculateHealth(project);
            Console.WriteLine($"{project.Id}. {project.Name}: {health}");
            Console.WriteLine($"   Open risks: {projectService.CountOpenRisks(project)} | Late milestones: {projectService.CountLateMilestones(project)}");

            if (health == HealthStatus.OnTrack)
            {
                onTrackCount++;
            }
            else if (health == HealthStatus.AtRisk)
            {
                atRiskCount++;
            }
            else
            {
                offTrackCount++;
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Totals - OnTrack: {onTrackCount} | AtRisk: {atRiskCount} | OffTrack: {offTrackCount}");
    }

    private static Project? ReadProject(ProjectService projectService)
    {
        Console.Write("Enter the project ID: ");
        string? input = Console.ReadLine();

        if (!int.TryParse(input, out int projectId))
        {
            Console.WriteLine("The project ID must be a whole number.");
            return null;
        }

        Project? project = projectService.GetProjectById(projectId);

        if (project == null)
        {
            Console.WriteLine("A project with that ID was not found.");
        }

        return project;
    }
}

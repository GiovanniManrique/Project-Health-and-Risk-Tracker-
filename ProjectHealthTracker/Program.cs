// Purpose: Runs the console menu and handles input.
// Data: Uses sample projects stored in a List<Project>.
// Methods: Shows projects, items, risks, and health; updates item status.

using ProjectHealthTracker.Data;
using ProjectHealthTracker.Models;
using ProjectHealthTracker.Services;

namespace ProjectHealthTracker;

public class Program
{
    public static void Main()
    {
        ProjectService service = new ProjectService(MockProjectData.GetProjects());
        bool applicationRunning = true;
        Console.WriteLine("Project Health and Risk Tracker");
        Console.WriteLine("Changes last until you exit. Sample data resets when you restart.");

        while (applicationRunning)
        {
            Console.WriteLine("\n1. List projects\n2. View project details\n3. Update item status");
            Console.WriteLine("4. Show project risks\n5. Show health summary\n6. Exit");
            Console.Write("Choose an option: ");
            string? choice = Console.ReadLine();
            if (choice == null)
            {
                break;
            }

            switch (choice.Trim())
            {
                case "1":
                    ShowProjects(service, false);
                    break;
                case "2":
                    ViewProjectDetails(service);
                    break;
                case "3":
                    UpdateItemStatus(service);
                    break;
                case "4":
                    ShowProjectRisks(service);
                    break;
                case "5":
                    ShowProjects(service, true);
                    break;
                case "6":
                    applicationRunning = false;
                    Console.WriteLine("Goodbye.");
                    break;
                default:
                    Console.WriteLine("That is not a valid menu choice. Please enter 1 through 6.");
                    break;
            }
        }
    }

    private static void ShowProjects(ProjectService service, bool showTotals)
    {
        int onTrack = 0;
        int atRisk = 0;
        int offTrack = 0;

        foreach (Project project in service.GetAllProjects())
        {
            HealthStatus health = service.CalculateHealth(project);
            Console.WriteLine($"{project.Id}. {project.Name}: {health}");
            Console.WriteLine($"   Manager: {project.Manager} | Open risks: {service.CountOpenRisks(project)} | Late milestones: {service.CountLateMilestones(project)}");
            if (health == HealthStatus.OnTrack)
            {
                onTrack++;
            }
            else if (health == HealthStatus.AtRisk)
            {
                atRisk++;
            }
            else
            {
                offTrack++;
            }
        }

        if (showTotals)
        {
            Console.WriteLine($"Totals - OnTrack: {onTrack} | AtRisk: {atRisk} | OffTrack: {offTrack}");
        }
    }

    private static void ViewProjectDetails(ProjectService service)
    {
        Project? project = ReadProject(service);
        if (project == null)
        {
            return;
        }

        Console.WriteLine($"Project: {project.Name} | Manager: {project.Manager}");
        Console.WriteLine($"Dates: {project.StartDate:d} to {project.EndDate:d}");
        Console.WriteLine($"Health: {service.CalculateHealth(project)}");
        ShowItems(project);
    }

    private static void ShowItems(Project project)
    {
        Console.WriteLine($"Items for {project.Name}:");
        if (project.Items.Count == 0)
        {
            Console.WriteLine("There are no items in this project.");
        }

        foreach (ProjectItem item in project.Items)
        {
            Console.WriteLine("- " + item.GetDetails());
        }
    }

    private static void ShowProjectRisks(ProjectService service)
    {
        Project? project = ReadProject(service);
        if (project == null)
        {
            return;
        }

        List<Risk> risks = service.GetOpenRisks(project);
        Console.WriteLine($"Open risks for {project.Name}:");
        if (risks.Count == 0)
        {
            Console.WriteLine("There are no open risks.");
        }

        foreach (Risk risk in risks)
        {
            Console.WriteLine("- " + risk.GetDetails());
        }
    }

    private static void UpdateItemStatus(ProjectService service)
    {
        Project? project = ReadProject(service);
        if (project == null)
        {
            return;
        }

        ShowItems(project);
        if (project.Items.Count == 0)
        {
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

        ProjectItem? item = service.GetItemById(project, itemId);
        if (item == null)
        {
            Console.WriteLine("An item with that ID was not found in this project.");
            return;
        }

        List<ItemStatus> statuses = service.GetAllowedStatuses(item);
        Console.WriteLine($"Current status: {item.Status}\n0. Cancel");
        for (int i = 0; i < statuses.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {statuses[i]}");
        }

        Console.Write("Choose the new status: ");
        string? input = Console.ReadLine();
        if (input == null || input.Trim() == "0")
        {
            Console.WriteLine("Update canceled.");
            return;
        }
        if (!int.TryParse(input, out int choice))
        {
            Console.WriteLine("Enter one of the status numbers shown. Nothing was changed.");
            return;
        }
        if (choice < 1 || choice > statuses.Count)
        {
            Console.WriteLine($"Choose a status from 1 to {statuses.Count}. Nothing was changed.");
            return;
        }

        // Menu numbers start at 1; list positions start at 0.
        ItemStatus newStatus = statuses[choice - 1];
        if (service.UpdateItemStatus(project.Id, item.Id, newStatus))
        {
            Console.WriteLine($"Updated {item.Title} to {newStatus}.");
            Console.WriteLine($"Project health is now {service.CalculateHealth(project)}.");
        }
        else
        {
            Console.WriteLine("The status could not be updated.");
        }
    }

    private static Project? ReadProject(ProjectService service)
    {
        Console.Write("Enter the project ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("The project ID must be a whole number.");
            return null;
        }

        Project? project = service.GetProjectById(id);
        if (project == null)
        {
            Console.WriteLine("A project with that ID was not found.");
        }
        return project;
    }
}

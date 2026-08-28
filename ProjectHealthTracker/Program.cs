// Purpose: Runs the console menu and handles the user's choices.
// Data: Uses mock projects stored in a List<Project>.
// Methods: Displays projects, project details, risks, and validates project IDs.
// Progress: Status updates and the health summary menu are left for the final part.

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

        while (applicationRunning)
        {
            DisplayMenu();
            Console.Write("Choose an option: ");
            string? choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    ListProjects(projectService);
                    break;

                case "2":
                    ViewProjectDetails(projectService);
                    break;

                case "3":
                    Console.WriteLine("Updating a status will be added in the final version.");
                    break;

                case "4":
                    ShowProjectRisks(projectService);
                    break;

                case "5":
                    Console.WriteLine("The health summary will be added in the final version.");
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
        Console.WriteLine("3. Update item status (not finished)");
        Console.WriteLine("4. Show project risks");
        Console.WriteLine("5. Show health summary (not finished)");
        Console.WriteLine("6. Exit");
    }

    private static void ListProjects(ProjectService projectService)
    {
        Console.WriteLine("Projects");

        foreach (Project project in projectService.GetAllProjects())
        {
            int openRiskCount = projectService.CountOpenRisks(project);
            Console.WriteLine($"{project.Id}. {project.Name}");
            Console.WriteLine($"   Manager: {project.Manager} | Open risks: {openRiskCount}");
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
        Console.WriteLine("Items:");

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

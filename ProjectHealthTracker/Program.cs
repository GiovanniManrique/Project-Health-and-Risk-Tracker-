// Purpose: Runs the console menu and handles input.
// Data: Uses sample projects stored in a List<Project>.
// Methods: Shows projects, items, risks, and health; updates status and asks local AI for advice.

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
            Console.WriteLine("4. Show project risks\n5. Show health summary\n6. Exit\n7. Explain a risk with local AI");
            Console.WriteLine("8. Review a risk with OpenAI");
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
                case "7":
                    ExplainRiskWithAi(service);
                    break;
                case "8":
                    ReviewRiskWithCloudAi(service);
                    break;
                default:
                    Console.WriteLine("That is not a valid menu choice. Please enter 1 through 8.");
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

    private static void ExplainRiskWithAi(ProjectService service)
    {
        Project? project = ReadProject(service);
        if (project == null)
        {
            return;
        }

        List<Risk> risks = service.GetOpenRisks(project);
        if (risks.Count == 0)
        {
            Console.WriteLine("There are no open risks to explain.");
            return;
        }
        foreach (Risk openRisk in risks)
        {
            Console.WriteLine(openRisk.GetDetails());
        }

        Console.Write("Enter an open risk ID (0 to cancel): ");
        if (!int.TryParse(Console.ReadLine(), out int riskId) || riskId == 0)
        {
            Console.WriteLine("AI explanation canceled. Enter a whole-number risk ID next time.");
            return;
        }
        ProjectItem? item = service.GetItemById(project, riskId);
        if (item is not Risk risk || risk.Status != ItemStatus.Open)
        {
            Console.WriteLine("That ID is not an open risk in this project.");
            return;
        }

        int score = service.CalculateRiskScore(risk);
        Console.WriteLine($"Priority score: {risk.Probability} x {risk.Impact} = {score}/25 (not a percentage).");
        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(2);
            AiRiskService ai = new AiRiskService(client);
            List<string> models = ai.GetModels();
            if (models.Count == 0)
            {
                Console.WriteLine("Ollama returned no installed local models. Check your existing model with ollama list.");
                return;
            }
            for (int i = 0; i < models.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {models[i]}");
            }
            Console.Write("Choose a model number (0 to cancel): ");
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > models.Count)
            {
                Console.WriteLine("AI explanation canceled. Choose one of the listed model numbers.");
                return;
            }

            Console.WriteLine("Asking your local model. This may take up to two minutes...");
            string explanation = ai.ExplainRisk(risk, models[choice - 1], score);
            Console.WriteLine("AI suggestion (review it; project data and health are unchanged):");
            Console.WriteLine(explanation);
        }
        catch (HttpRequestException)
        {
            Console.WriteLine("Ollama could not complete the request. Check that it is running at localhost:11434 and the selected text model works in Ollama.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("The local model took too long. Try again or choose a smaller installed model.");
        }
        catch (System.Text.Json.JsonException)
        {
            Console.WriteLine("Ollama returned an unreadable response. No project data was changed.");
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("Ollama's response was missing an expected field. No project data was changed.");
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("Ollama returned an unexpected response format. No project data was changed.");
        }
    }

    private static void ReviewRiskWithCloudAi(ProjectService service)
    {
        Project? project = ReadProject(service);
        if (project == null)
        {
            return;
        }
        Console.WriteLine(service.GetProjectMetrics(project));
        List<Risk> risks = service.GetOpenRisks(project);
        if (risks.Count == 0)
        {
            Console.WriteLine("There are no open risks to review.");
            return;
        }
        foreach (Risk openRisk in risks)
        {
            Console.WriteLine(openRisk.GetDetails());
        }
        Console.Write("Enter an open risk ID (0 to cancel): ");
        if (!int.TryParse(Console.ReadLine(), out int riskId) || riskId == 0)
        {
            Console.WriteLine("Risk review canceled. Enter a whole-number risk ID next time.");
            return;
        }
        ProjectItem? item = service.GetItemById(project, riskId);
        if (item is not Risk risk || risk.Status != ItemStatus.Open)
        {
            Console.WriteLine("That ID is not an open risk in this project.");
            return;
        }

        Console.WriteLine($"Priority score: {risk.Probability} x {risk.Impact} = {service.CalculateRiskScore(risk)}/25 (not a percentage).");
        Console.WriteLine("The current ratings are supplied by people/sample data, not estimated by AI.");
        Console.WriteLine("Answer two optional questions. Press Enter for unknown; use up to 500 characters each.");
        Console.Write("What evidence suggests this problem might happen? ");
        string? evidence = Console.ReadLine();
        Console.Write("What work would be affected if it happens? ");
        string? affectedWork = Console.ReadLine();
        if (evidence == null || affectedWork == null)
        {
            Console.WriteLine("Risk review canceled.");
            return;
        }
        if (evidence.Length > 500 || affectedWork.Length > 500)
        {
            Console.WriteLine("Please keep each answer to 500 characters. No request was sent.");
            return;
        }
        string review = service.BuildRiskReview(project, risk, evidence, affectedWork);
        Console.WriteLine("\nRisk review:\n" + review);

        // Check Windows user settings too, so a newly saved key works without restarting Visual Studio.
        string? apiKey = Environment.GetEnvironmentVariable("AI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable("AI_API_KEY", EnvironmentVariableTarget.User);
        }
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("OpenAI is not configured yet. Set AI_API_KEY in Windows user environment variables.");
            Console.WriteLine("The risk metrics above work without an API key. See README.md for setup.");
            return;
        }

        Console.Write("Send the review above to OpenAI for advice? API usage may cost money. Enter y to send: ");
        if (!string.Equals(Console.ReadLine()?.Trim(), "y", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("No request was sent.");
            return;
        }
        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(1);
            CloudAiService ai = new CloudAiService(client, apiKey);
            Console.WriteLine("Asking OpenAI...");
            Console.WriteLine(ai.ExplainRisk(review));
            Console.WriteLine("AI advice does not change project data or the C# health rules.");
        }
        catch (HttpRequestException)
        {
            Console.WriteLine("OpenAI could not complete the request. Check your connection and try again later.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("OpenAI took too long to respond. Try again later.");
        }
        catch (System.Text.Json.JsonException)
        {
            Console.WriteLine("OpenAI returned an unreadable response. No project data was changed.");
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("OpenAI's response was missing an expected field. No project data was changed.");
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("OpenAI returned an unexpected response format. No project data was changed.");
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

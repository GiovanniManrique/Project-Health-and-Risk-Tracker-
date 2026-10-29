using ProjectHealthTracker.Models;

namespace ProjectHealthTracker;

public class Program
{
    public static async Task Main()
    {
        // The same sample objects are reused until the program exits.
        List<Project> projects = CreateSampleProjects();
        using HttpClient client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(120);
        string baseUrl = Environment.GetEnvironmentVariable("BIONIC_BASE_URL") ?? "http://127.0.0.1:1235/v1";
        string model = Environment.GetEnvironmentVariable("BIONIC_MODEL") ?? "google/gemma-4-e2b";
        string evidenceDirectory = Path.Combine(AppContext.BaseDirectory, "DemoData");

        Console.WriteLine("PROJECT HEALTH AND RISK TRACKER");
        Console.WriteLine("Fictional demo data: one task, one milestone, and a risk list per project.");
        Console.WriteLine("Changes reset on restart. AI suggestions do not change the recorded ratings or health.\n");

        while (true)
        {
            Console.WriteLine("1. List projects and health\n2. View project details");
            Console.WriteLine("3. Update a status\n4. AI risk brief\n5. Exit");
            Console.Write("Choose an option: ");
            string? choice = Console.ReadLine();
            if (choice != null) choice = choice.Trim();
            switch (choice)
            {
                case "1":
                    ShowProjects(projects);
                    break;
                case "2":
                    ViewProject(projects);
                    break;
                case "3":
                    UpdateStatus(projects);
                    break;
                case "4":
                    await ShowAiRiskBriefAsync(projects, client, baseUrl, model, evidenceDirectory);
                    break;
                case "5":
                case null:
                    Console.WriteLine("Goodbye.");
                    return;
                default:
                    Console.WriteLine("Please choose a number from 1 to 5.");
                    break;
            }
            Console.WriteLine();
        }
    }

    private static void ShowProjects(List<Project> projects)
    {
        int onTrack = 0;
        int atRisk = 0;
        int offTrack = 0;
        foreach (Project project in projects)
        {
            HealthStatus health = project.CalculateHealth();
            int openRisks = 0;
            foreach (Risk risk in project.Risks)
            {
                if (risk.Status == ItemStatus.Open) openRisks++;
            }
            Console.WriteLine($"{project.Id}. {project.Name} | Manager: {project.Manager} | {health} | Open risks: {openRisks}");
            if (health == HealthStatus.OnTrack) onTrack++;
            else if (health == HealthStatus.AtRisk) atRisk++;
            else offTrack++;
        }
        Console.WriteLine($"Summary: {onTrack} OnTrack, {atRisk} AtRisk, {offTrack} OffTrack");
    }

    private static Project? SelectProject(List<Project> projects)
    {
        ShowProjects(projects);
        Console.Write("Project ID (0 to cancel): ");
        string? input = Console.ReadLine();
        if (input == null || input.Trim() == "0") return null;

        int id;
        if (int.TryParse(input, out id) == false)
        {
            Console.WriteLine("That project was not found. Enter a project ID number.");
            return null;
        }
        foreach (Project project in projects)
        {
            if (project.Id == id) return project;
        }
        Console.WriteLine("That project was not found.");
        return null;
    }

    private static void ViewProject(List<Project> projects)
    {
        Project? selectedProject = SelectProject(projects);
        if (selectedProject == null) return;
        Console.WriteLine(selectedProject.GetDetails());
    }

    private static void UpdateStatus(List<Project> projects)
    {
        Project? selectedProject = SelectProject(projects);
        if (selectedProject == null) return;
        Console.WriteLine("1. Task\n2. Milestone\n3. Risk");
        Console.Write("Section number (0 to cancel): ");
        string? input = Console.ReadLine();
        if (input == null || input.Trim() == "0") return;

        int section;
        if (int.TryParse(input, out section) == false)
        {
            Console.WriteLine("Please enter a section number.");
            return;
        }
        if (section < 1 || section > 3)
        {
            Console.WriteLine("Choose section 1, 2, or 3.");
            return;
        }

        Risk? selectedRisk = null;
        List<ItemStatus> allowedStatuses;
        if (section == 3)
        {
            foreach (Risk risk in selectedProject.Risks)
                Console.WriteLine($"{risk.Id}. {risk.Title} | {risk.Status}");
            Console.Write("Risk ID (0 to cancel): ");
            input = Console.ReadLine();
            if (input == null || input.Trim() == "0") return;
            foreach (Risk risk in selectedProject.Risks)
            {
                if (risk.Id.Equals(input.Trim(), StringComparison.OrdinalIgnoreCase)) selectedRisk = risk;
            }
            if (selectedRisk == null)
            {
                Console.WriteLine("That risk was not found. Enter an ID shown above.");
                return;
            }
            allowedStatuses = new List<ItemStatus> { ItemStatus.Open, ItemStatus.Closed };
        }
        else
        {
            allowedStatuses = selectedProject.GetAllowedStatuses(section);
        }

        for (int i = 0; i < allowedStatuses.Count; i++)
            Console.WriteLine($"{i + 1}. {allowedStatuses[i]}");
        Console.Write("New status number (0 to cancel): ");
        input = Console.ReadLine();
        if (input == null || input.Trim() == "0") return;

        int statusNumber;
        if (int.TryParse(input, out statusNumber) == false)
        {
            Console.WriteLine("Please enter a status number.");
            return;
        }
        if (statusNumber < 1 || statusNumber > allowedStatuses.Count)
        {
            Console.WriteLine("That is not a valid status choice.");
            return;
        }

        ItemStatus selectedStatus = allowedStatuses[statusNumber - 1];
        bool statusChanged;
        if (selectedRisk != null) statusChanged = selectedRisk.ChangeStatus(selectedStatus);
        else statusChanged = selectedProject.ChangeStatus(section, selectedStatus);
        if (statusChanged)
            Console.WriteLine($"Updated to {selectedStatus}. Project health: {selectedProject.CalculateHealth()}");
    }

    private static async Task ShowAiRiskBriefAsync(List<Project> projects, HttpClient client, string baseUrl, string model, string evidenceDirectory)
    {
        Project? selectedProject = SelectProject(projects);
        if (selectedProject == null) return;
        try
        {
            AiRiskService aiService = new AiRiskService(client, baseUrl, model);
            Console.WriteLine("Reading this project's fictional evidence and asking the local model for a risk brief...");
            string brief = await aiService.CreateBriefAsync(selectedProject, evidenceDirectory);
            Console.WriteLine(brief);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("AI brief unavailable: the local model request timed out. Check Bionic and try again.");
        }
        catch (HttpRequestException error)
        {
            Console.WriteLine($"AI brief unavailable: could not complete the Bionic request. {error.Message}");
        }
        catch (IOException error)
        {
            Console.WriteLine($"AI brief unavailable: could not read the evidence files. {error.Message}");
        }
        catch (UnauthorizedAccessException error)
        {
            Console.WriteLine($"AI brief unavailable: evidence file access was denied. {error.Message}");
        }
        catch (Exception error)
        {
            // A failed request must return to the menu rather than inventing a brief.
            Console.WriteLine($"AI brief unavailable: {error.Message}");
        }
    }

    public static List<Project> CreateSampleProjects()
    {
        Project website = new Project(1, "Company Website Update", "Jordan Lee")
        {
            TaskTitle = "Create page layout",
            TaskOwner = "Sam",
            TaskDueDate = DateTime.Today.AddDays(-5),
            MilestoneTitle = "Design approved",
            MilestoneOwner = "Jordan",
            MilestoneDueDate = DateTime.Today.AddDays(-2),
            EvidenceFiles = new List<string> { "WEB-EMAIL.eml", "WEB-NOTES.txt", "WEB-METRICS.json" }
        };
        website.ChangeStatus(1, ItemStatus.Completed);
        website.ChangeStatus(2, ItemStatus.Completed);
        website.Risks.Add(new Risk("WEB-1", "Old images may be low quality", "Mia")
        {
            Likelihood = 2,
            Impact = 2,
            Scenario = "The team replaced the blurry images and approved the new ones.",
            Plan = "Use the approved image collection.",
            SourceIds = new List<string> { "WEB-EMAIL", "WEB-NOTES", "WEB-METRICS" }
        });

        Project inventory = new Project(2, "Inventory System", "Taylor Smith")
        {
            TaskTitle = "Create item classes",
            TaskOwner = "Alex",
            TaskDueDate = DateTime.Today.AddDays(7),
            MilestoneTitle = "First working demo",
            MilestoneOwner = "Taylor",
            MilestoneDueDate = DateTime.Today.AddDays(10),
            EvidenceFiles = new List<string> { "INV-EMAIL.eml", "INV-NOTES.txt", "INV-METRICS.json" }
        };
        inventory.ChangeStatus(1, ItemStatus.InProgress);
        Risk supplier = new Risk("INV-1", "Scanner hardware may arrive late", "Chris")
        {
            Likelihood = 4,
            Impact = 5,
            Scenario = "The supplier missed the delivery date and has not confirmed a replacement date. The demo is in ten days, and scanning still needs testing.",
            Plan = "Use manual item numbers until the scanners arrive.",
            SourceIds = new List<string> { "INV-EMAIL", "INV-NOTES", "INV-METRICS" }
        };
        supplier.ChangeStatus(ItemStatus.Open);
        inventory.Risks.Add(supplier);
        Risk budget = new Risk("INV-2", "Forecast cost may exceed the budget", "Taylor")
        {
            Likelihood = 3,
            Impact = 3,
            Scenario = "The forecast is $12,500 against a $10,000 budget. Extra scanner shipping and temporary test support are not yet approved.",
            Plan = "Review the extra costs and request approval before placing new orders.",
            SourceIds = new List<string> { "INV-NOTES", "INV-METRICS" }
        };
        budget.ChangeStatus(ItemStatus.Open);
        inventory.Risks.Add(budget);
        Risk staffing = new Risk("INV-3", "Limited staff may delay scanner testing", "Alex")
        {
            Likelihood = 3,
            Impact = 3,
            Scenario = "Only one of the three planned testers is available. Scanning tests have not started, and the first demo is in ten days.",
            Plan = "Ask for temporary test support and prepare manual-entry tests now.",
            SourceIds = new List<string> { "INV-NOTES", "INV-METRICS" }
        };
        staffing.ChangeStatus(ItemStatus.Open);
        inventory.Risks.Add(staffing);

        Project training = new Project(3, "Employee Training Plan", "Morgan Davis")
        {
            TaskTitle = "Write training guide",
            TaskOwner = "Riley",
            TaskDueDate = DateTime.Today.AddDays(4),
            MilestoneTitle = "Manager review",
            MilestoneOwner = "Morgan",
            MilestoneDueDate = DateTime.Today.AddDays(-3),
            EvidenceFiles = new List<string> { "TRN-EMAIL.eml", "TRN-NOTES.txt", "TRN-METRICS.json" }
        };
        training.ChangeStatus(1, ItemStatus.InProgress);
        training.ChangeStatus(2, ItemStatus.InProgress);
        training.Risks.Add(new Risk("TRN-1", "Not enough training computers", "Riley")
        {
            Likelihood = 2,
            Impact = 3,
            Scenario = "The shared computer lab has been reserved for every training session.",
            Plan = "Use the reserved computer lab.",
            SourceIds = new List<string> { "TRN-EMAIL", "TRN-NOTES", "TRN-METRICS" }
        });
        return new List<Project> { website, inventory, training };
    }
}

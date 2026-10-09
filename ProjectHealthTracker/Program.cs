using ProjectHealthTracker.Models;

namespace ProjectHealthTracker;

public class Program
{
    public static void Main()
    {
        // Create the sample objects once. Menu actions reuse these same objects.
        List<Project> projects = CreateSampleProjects();
        Console.WriteLine("PROJECT HEALTH AND RISK TRACKER");
        Console.WriteLine("Fictional data: one task, one milestone, and one risk per project. Changes reset on restart.\n");

        while (true)
        {
            Console.WriteLine("1. List projects and health\n2. View project details");
            Console.WriteLine("3. Update a status\n4. Exit");
            Console.Write("Choose an option: ");
            string? choice = Console.ReadLine();
            if (choice != null) choice = choice.Trim();
            // Call the method for this menu choice, then return to the menu.
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
                case null:
                    Console.WriteLine("Goodbye.");
                    return;
                default:
                    Console.WriteLine("Please choose a number from 1 to 4.");
                    break;
            }
            Console.WriteLine();
        }
    }

    private static void ShowProjects(List<Project> projects)
    {
        // Count the health categories while displaying each project.
        int onTrack = 0;
        int atRisk = 0;
        int offTrack = 0;
        foreach (Project project in projects)
        {
            HealthStatus health = project.CalculateHealth();
            int openRisks = 0;
            if (project.RiskStatus == ItemStatus.Open) openRisks = 1;
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
        // null means input ended; zero means the user cancelled.
        if (input == null) return null;
        if (input.Trim() == "0") return null;

        int id;
        bool isNumber = int.TryParse(input, out id);
        if (isNumber == false)
        {
            Console.WriteLine("That project was not found. Enter a project ID number.");
            return null;
        }
        // Return the matching object from the list; do not create a copy.
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
        string details = selectedProject.GetDetails();
        Console.WriteLine(details);
    }

    private static void UpdateStatus(List<Project> projects)
    {

        Project? selectedProject = SelectProject(projects);
        if (selectedProject == null) return;
        Console.WriteLine("1. Task\n2. Milestone\n3. Risk");
        Console.Write("Section number (0 to cancel): ");
        string? input = Console.ReadLine();
        if (input == null) return;
        if (input.Trim() == "0") return;

        int section;
        bool isSectionNumber = int.TryParse(input, out section);
        if (isSectionNumber == false)
        {
            Console.WriteLine("Please enter a section number.");
            return;
        }
        if (section < 1 || section > 3)
        {
            Console.WriteLine("Choose section 1, 2, or 3.");
            return;
        }


        List<ItemStatus> allowedStatuses = selectedProject.GetAllowedStatuses(section);
        for (int i = 0; i < allowedStatuses.Count; i++)
        {
            int menuNumber = i + 1;
            ItemStatus status = allowedStatuses[i];
            Console.WriteLine($"{menuNumber}. {status}");
        }
        Console.Write("New status number (0 to cancel): ");
        input = Console.ReadLine();
        if (input == null) return;
        if (input.Trim() == "0") return;

        int statusNumber;
        bool isStatusNumber = int.TryParse(input, out statusNumber);
        if (isStatusNumber == false)
        {
            Console.WriteLine("Please enter a status number.");
            return;
        }
        if (statusNumber < 1 || statusNumber > allowedStatuses.Count)
        {
            Console.WriteLine("That is not a valid status choice.");
            return;
        }


        int statusIndex = statusNumber - 1;
        ItemStatus selectedStatus = allowedStatuses[statusIndex];
        bool statusChanged = selectedProject.ChangeStatus(section, selectedStatus);
        if (statusChanged)
        {
            HealthStatus health = selectedProject.CalculateHealth();
            Console.WriteLine($"Updated to {selectedStatus}. Project health: {health}");
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
            RiskTitle = "Old images may be low quality",
            RiskOwner = "Mia",
            RiskLikelihood = 2,
            RiskImpact = 2,
            RiskScenario = "The team replaced the blurry images and approved the new ones.",
            RiskPlan = "Use the approved image collection."
        };
        website.ChangeStatus(1, ItemStatus.Completed);
        website.ChangeStatus(2, ItemStatus.Completed);

        Project inventory = new Project(2, "Inventory System", "Taylor Smith")
        {
            TaskTitle = "Create item classes",
            TaskOwner = "Alex",
            TaskDueDate = DateTime.Today.AddDays(7),
            MilestoneTitle = "First working demo",
            MilestoneOwner = "Taylor",
            MilestoneDueDate = DateTime.Today.AddDays(10),
            RiskTitle = "Scanner hardware may arrive late",
            RiskOwner = "Chris",
            RiskLikelihood = 4,
            RiskImpact = 5,
            RiskScenario = "The supplier missed the delivery date and has not confirmed a replacement date. The demo is in ten days, and scanning still needs testing.",
            RiskPlan = "Use manual item numbers until the scanners arrive."
        };
        inventory.ChangeStatus(1, ItemStatus.InProgress);
        inventory.ChangeStatus(3, ItemStatus.Open);

        Project training = new Project(3, "Employee Training Plan", "Morgan Davis")
        {
            TaskTitle = "Write training guide",
            TaskOwner = "Riley",
            TaskDueDate = DateTime.Today.AddDays(4),
            MilestoneTitle = "Manager review",
            MilestoneOwner = "Morgan",
            MilestoneDueDate = DateTime.Today.AddDays(-3),
            RiskTitle = "Not enough training computers",
            RiskOwner = "Riley",
            RiskLikelihood = 2,
            RiskImpact = 3,
            RiskScenario = "The shared computer lab has been reserved for every training session.",
            RiskPlan = "Use the reserved computer lab."
        };
        training.ChangeStatus(1, ItemStatus.InProgress);
        training.ChangeStatus(2, ItemStatus.InProgress);
        return new List<Project> { website, inventory, training };
    }
}

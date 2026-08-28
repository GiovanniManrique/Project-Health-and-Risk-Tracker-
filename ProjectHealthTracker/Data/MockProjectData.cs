// Purpose: Creates sample projects so the program can run without a database.
// Data: Returns three projects containing tasks, milestones, and risks.
// Methods: GetProjects builds and returns a List<Project>.

using ProjectHealthTracker.Models;

namespace ProjectHealthTracker.Data;

public static class MockProjectData
{
    public static List<Project> GetProjects()
    {
        List<Project> projects = new List<Project>();

        Project websiteProject = new Project(
            1,
            "Company Website Update",
            "Jordan Lee",
            DateTime.Today.AddDays(-30),
            DateTime.Today.AddDays(45));

        websiteProject.Items.Add(new ProjectTask(
            101, "Create page layout", "Sam", ItemStatus.Completed,
            DateTime.Today.AddDays(-5), true));
        websiteProject.Items.Add(new Milestone(
            102, "Design approved", "Jordan", ItemStatus.Completed,
            DateTime.Today.AddDays(-2), true));
        websiteProject.Items.Add(new Risk(
            103, "Old images may be low quality", "Mia", ItemStatus.Closed,
            2, 2, "Replace the images that do not meet the new size."));

        Project inventoryProject = new Project(
            2,
            "Inventory System",
            "Taylor Smith",
            DateTime.Today.AddDays(-15),
            DateTime.Today.AddDays(75));

        inventoryProject.Items.Add(new ProjectTask(
            201, "Create item classes", "Alex", ItemStatus.InProgress,
            DateTime.Today.AddDays(7), false));
        inventoryProject.Items.Add(new Milestone(
            202, "First working demo", "Taylor", ItemStatus.NotStarted,
            DateTime.Today.AddDays(25), false));
        inventoryProject.Items.Add(new Risk(
            203, "Scanner hardware may arrive late", "Chris", ItemStatus.Open,
            4, 5, "Use manual item numbers until the scanners arrive."));

        Project trainingProject = new Project(
            3,
            "Employee Training Plan",
            "Morgan Davis",
            DateTime.Today.AddDays(-45),
            DateTime.Today.AddDays(20));

        trainingProject.Items.Add(new ProjectTask(
            301, "Write training guide", "Riley", ItemStatus.InProgress,
            DateTime.Today.AddDays(4), false));
        trainingProject.Items.Add(new Milestone(
            302, "Manager review", "Morgan", ItemStatus.InProgress,
            DateTime.Today.AddDays(-3), false));
        trainingProject.Items.Add(new Risk(
            303, "Not enough training computers", "Riley", ItemStatus.Closed,
            2, 3, "Reserve the shared computer lab."));

        projects.Add(websiteProject);
        projects.Add(inventoryProject);
        projects.Add(trainingProject);

        return projects;
    }
}

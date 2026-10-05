// Purpose: Contains the main rules used by the project tracker.
// Data: Stores the in-memory list of projects passed into the constructor.
// Methods: Finds projects, updates item status, counts risks, and calculates health.

using ProjectHealthTracker.Models;

namespace ProjectHealthTracker.Services;

public class ProjectService
{
    private readonly List<Project> projects;

    public ProjectService(List<Project> projects)
    {
        this.projects = projects;
    }

    public List<Project> GetAllProjects()
    {
        return projects;
    }

    public Project? GetProjectById(int projectId)
    {
        foreach (Project project in projects)
        {
            if (project.Id == projectId)
            {
                return project;
            }
        }

        return null;
    }

    public ProjectItem? GetItemById(Project project, int itemId)
    {
        foreach (ProjectItem item in project.Items)
        {
            if (item.Id == itemId)
            {
                return item;
            }
        }

        return null;
    }

    public bool UpdateItemStatus(int projectId, int itemId, ItemStatus newStatus)
    {
        Project? project = GetProjectById(projectId);

        if (project == null)
        {
            return false;
        }

        ProjectItem? item = GetItemById(project, itemId);
        if (item == null)
        {
            return false;
        }

        if (!GetAllowedStatuses(item).Contains(newStatus))
        {
            return false;
        }

        item.Status = newStatus;
        if (item is ProjectTask task)
        {
            task.IsCompleted = newStatus == ItemStatus.Completed;
        }
        else if (item is Milestone milestone)
        {
            milestone.IsAchieved = newStatus == ItemStatus.Completed;
        }

        return true;
    }

    public List<ItemStatus> GetAllowedStatuses(ProjectItem item)
    {
        if (item is Risk)
        {
            return new List<ItemStatus> { ItemStatus.Open, ItemStatus.Closed };
        }

        return new List<ItemStatus>
        {
            ItemStatus.NotStarted, ItemStatus.InProgress, ItemStatus.Completed, ItemStatus.Blocked
        };
    }

    public int CountOpenRisks(Project project)
    {
        return GetOpenRisks(project).Count;
    }

    public List<Risk> GetOpenRisks(Project project)
    {
        List<Risk> openRisks = new List<Risk>();

        foreach (ProjectItem item in project.Items)
        {
            if (item is Risk risk && risk.Status == ItemStatus.Open)
            {
                openRisks.Add(risk);
            }
        }

        return openRisks;
    }

    public int CountLateMilestones(Project project)
    {
        int count = 0;
        foreach (ProjectItem item in project.Items)
        {
            if (item is Milestone milestone && !milestone.IsAchieved &&
                milestone.TargetDate.Date < DateTime.Today)
            {
                count++;
            }
        }

        return count;
    }

    public int CalculateRiskScore(Risk risk)
    {
        return risk.Probability * risk.Impact;
    }

    public string GetProjectMetrics(Project project)
    {
        int highImpactRisks = 0;
        int totalTasks = 0;
        int completedTasks = 0;
        foreach (ProjectItem item in project.Items)
        {
            if (item is Risk risk && risk.Status == ItemStatus.Open && risk.Impact >= 4)
            {
                highImpactRisks++;
            }
            if (item is ProjectTask task)
            {
                totalTasks++;
                if (task.IsCompleted)
                {
                    completedTasks++;
                }
            }
        }

        int daysRemaining = (project.EndDate.Date - DateTime.Today).Days;
        return $"Project: {project.Name}\n" +
            $"Current health from C# rules: {CalculateHealth(project)}\n" +
            $"Open risks: {CountOpenRisks(project)}\n" +
            $"High-impact open risks (impact 4 or 5): {highImpactRisks}\n" +
            $"Late unfinished milestones: {CountLateMilestones(project)}\n" +
            $"Tasks completed: {completedTasks} of {totalTasks} (tasks may differ in size)\n" +
            $"Days until planned end date: {daysRemaining} (negative means the date has passed)";
    }

    public string BuildRiskReview(Project project, Risk risk, string evidence, string affectedWork)
    {
        // Blank answers mean unknown. The model must not make up evidence for a stored rating.
        if (string.IsNullOrWhiteSpace(evidence))
        {
            evidence = "Unknown; no evidence supplied.";
        }
        if (string.IsNullOrWhiteSpace(affectedWork))
        {
            affectedWork = "Unknown; no affected work described.";
        }

        return GetProjectMetrics(project) + "\n\nSelected risk:\n" +
            $"Title: {risk.Title}\nOwner: {risk.Owner}\nStatus: {risk.Status}\n" +
            $"Human likelihood rating: {risk.Probability}/5\nHuman impact rating: {risk.Impact}/5\n" +
            $"Priority score: {CalculateRiskScore(risk)}/25 (not a percentage)\n" +
            $"Current mitigation plan: {risk.MitigationPlan}\n" +
            $"What evidence suggests this might happen? {evidence.Trim()}\n" +
            $"What work would be affected? {affectedWork.Trim()}";
    }

    public HealthStatus CalculateHealth(Project project)
    {
        List<Risk> risks = GetOpenRisks(project);
        foreach (Risk risk in risks)
        {
            if (risk.Impact >= 4)
            {
                return HealthStatus.OffTrack;
            }
        }

        if (risks.Count > 0 || CountLateMilestones(project) > 0)
        {
            return HealthStatus.AtRisk;
        }

        return HealthStatus.OnTrack;
    }
}

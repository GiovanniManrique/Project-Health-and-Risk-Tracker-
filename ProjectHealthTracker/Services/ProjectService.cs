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

    public bool UpdateItemStatus(int projectId, int itemId, ItemStatus newStatus)
    {
        Project? project = GetProjectById(projectId);

        if (project == null)
        {
            return false;
        }

        foreach (ProjectItem item in project.Items)
        {
            if (item.Id == itemId)
            {
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
        }

        return false;
    }

    public int CountOpenRisks(Project project)
    {
        int count = 0;

        foreach (ProjectItem item in project.Items)
        {
            if (item is Risk risk && risk.Status == ItemStatus.Open)
            {
                count++;
            }
        }

        return count;
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

    public HealthStatus CalculateHealth(Project project)
    {
        bool hasOpenRisk = false;
        bool hasLateMilestone = false;

        foreach (ProjectItem item in project.Items)
        {
            if (item is Risk risk && risk.Status == ItemStatus.Open)
            {
                hasOpenRisk = true;

                if (risk.Impact >= 4)
                {
                    return HealthStatus.OffTrack;
                }
            }

            if (item is Milestone milestone)
            {
                if (!milestone.IsAchieved && milestone.TargetDate < DateTime.Today)
                {
                    hasLateMilestone = true;
                }
            }
        }

        if (hasOpenRisk || hasLateMilestone)
        {
            return HealthStatus.AtRisk;
        }

        return HealthStatus.OnTrack;
    }
}

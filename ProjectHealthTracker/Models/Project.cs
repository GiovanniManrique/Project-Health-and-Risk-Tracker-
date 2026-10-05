// One project owns its items and calculates health using simple classroom rules.
namespace ProjectHealthTracker.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Manager { get; set; }
    public List<ProjectItem> Items { get; } = new List<ProjectItem>();

    public Project(int id, string name, string manager)
    {
        Id = id;
        Name = name;
        Manager = manager;
    }

    public int CountOpenRisks()
    {
        int count = 0;
        foreach (ProjectItem item in Items)
        {
            if (item.Type == ItemType.Risk && item.Status == ItemStatus.Open) count++;
        }
        return count;
    }

    public HealthStatus CalculateHealth()
    {
        bool needsAttention = false;
        foreach (ProjectItem item in Items)
        {
            if (item.Type == ItemType.Risk && item.Status == ItemStatus.Open)
            {
                if (item.Impact >= 4) return HealthStatus.OffTrack;
                needsAttention = true;
            }
            if (item.Type == ItemType.Milestone && item.Status != ItemStatus.Completed &&
                item.DueDate.HasValue && item.DueDate.Value.Date < DateTime.Today)
            {
                needsAttention = true;
            }
        }
        if (needsAttention) return HealthStatus.AtRisk;
        return HealthStatus.OnTrack;
    }
}

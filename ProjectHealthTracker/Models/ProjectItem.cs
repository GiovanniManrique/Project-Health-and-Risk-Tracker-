// One item can be a task, milestone, or risk. Type tells the program which fields to use.
namespace ProjectHealthTracker.Models;

public class ProjectItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Owner { get; set; }
    public ItemType Type { get; }
    public ItemStatus Status { get; private set; }
    public DateTime? DueDate { get; set; }
    public int Probability { get; set; }
    public int Impact { get; set; }
    public string Scenario { get; set; } = "";
    public string MitigationPlan { get; set; } = "";

    public ProjectItem(int id, string title, string owner, ItemType type, ItemStatus status)
    {
        Id = id;
        Title = title;
        Owner = owner;
        Type = type;
        if (!ChangeStatus(status)) throw new ArgumentException("The status does not match the item type.");
    }

    public List<ItemStatus> GetAllowedStatuses()
    {
        if (Type == ItemType.Risk)
            return new List<ItemStatus> { ItemStatus.Open, ItemStatus.Closed };
        if (Type == ItemType.Task || Type == ItemType.Milestone)
            return new List<ItemStatus> { ItemStatus.NotStarted, ItemStatus.InProgress, ItemStatus.Completed, ItemStatus.Blocked };
        return new List<ItemStatus>();
    }

    public bool ChangeStatus(ItemStatus newStatus)
    {
        if (!GetAllowedStatuses().Contains(newStatus)) return false;
        Status = newStatus;
        return true;
    }

    public int CalculateScore()
    {
        return Probability * Impact;
    }

    public string GetDetails()
    {
        string details = $"{Id}. {Title} ({Type})\nOwner: {Owner} | Status: {Status}";
        if (Type == ItemType.Risk)
            return details + $"\nLikelihood: {Probability}/5 | Impact: {Impact}/5 | Priority score: {CalculateScore()}/25 (not a percentage)" +
                $"\nScenario: {Scenario}\nCurrent plan: {MitigationPlan}";
        if (DueDate.HasValue) details += $"\nDue date: {DueDate.Value:d}";
        return details;
    }
}

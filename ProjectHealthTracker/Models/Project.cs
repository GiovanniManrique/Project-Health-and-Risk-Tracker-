namespace ProjectHealthTracker.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Manager { get; set; }

    // Keep one task and one milestone so the menu stays small.
    public string TaskTitle { get; set; } = "";
    public string TaskOwner { get; set; } = "";
    public DateTime? TaskDueDate { get; set; }
    public ItemStatus TaskStatus { get; private set; } = ItemStatus.NotStarted;

    public string MilestoneTitle { get; set; } = "";
    public string MilestoneOwner { get; set; } = "";
    public DateTime? MilestoneDueDate { get; set; }
    public ItemStatus MilestoneStatus { get; private set; } = ItemStatus.NotStarted;

    public List<Risk> Risks { get; set; } = new List<Risk>();
    public List<string> EvidenceFiles { get; set; } = new List<string>();

    public Project(int id, string name, string manager)
    {
        Id = id;
        Name = name;
        Manager = manager;
    }

    // Risks have their own ChangeStatus method. These sections are task/milestone.
    public List<ItemStatus> GetAllowedStatuses(int section)
    {
        if (section == 1 || section == 2)
            return new List<ItemStatus> { ItemStatus.NotStarted, ItemStatus.InProgress, ItemStatus.Completed, ItemStatus.Blocked };
        return new List<ItemStatus>();
    }

    public bool ChangeStatus(int section, ItemStatus newStatus)
    {
        List<ItemStatus> allowedStatuses = GetAllowedStatuses(section);
        if (allowedStatuses.Contains(newStatus) == false) return false;

        if (section == 1) TaskStatus = newStatus;
        if (section == 2) MilestoneStatus = newStatus;
        return true;
    }

    public HealthStatus CalculateHealth()
    {
        bool hasOpenRisk = false;
        foreach (Risk risk in Risks)
        {
            if (risk.Status == ItemStatus.Open)
            {
                hasOpenRisk = true;
                if (risk.Impact >= 4) return HealthStatus.OffTrack;
            }
        }
        if (hasOpenRisk) return HealthStatus.AtRisk;

        bool milestoneIsLate = false;
        if (MilestoneDueDate.HasValue)
            milestoneIsLate = MilestoneDueDate.Value.Date < DateTime.Today;
        bool milestoneIsFinished = MilestoneStatus == ItemStatus.Completed;
        if (milestoneIsLate && milestoneIsFinished == false) return HealthStatus.AtRisk;
        return HealthStatus.OnTrack;
    }

    public string GetRiskDetails()
    {
        string details = "";
        foreach (Risk risk in Risks)
        {
            details += $"Risk {risk.Id}: {risk.Title} | Owner: {risk.Owner} | Status: {risk.Status}\n";
            details += $"Base likelihood: {risk.Likelihood}/5 | Base impact: {risk.Impact}/5 | Base priority score: {risk.CalculateScore()}/25 (not a percentage)\n";
            details += $"Scenario: {risk.Scenario}\nCurrent plan: {risk.Plan}\n";
            details += $"Evidence IDs: {string.Join(", ", risk.SourceIds)}\n";
        }
        if (Risks.Count == 0) return "No risks recorded.\n";
        return details;
    }

    public string GetDetails()
    {
        return $"{Name} | Manager: {Manager} | Health: {CalculateHealth()}\n" +
            $"Task: {TaskTitle} | Owner: {TaskOwner} | Status: {TaskStatus} | Due: {TaskDueDate:d}\n" +
            $"Milestone: {MilestoneTitle} | Owner: {MilestoneOwner} | Status: {MilestoneStatus} | Due: {MilestoneDueDate:d}\n" +
            GetRiskDetails();
    }
}

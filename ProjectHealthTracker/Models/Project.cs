// One project has one task, one milestone, and one risk.
namespace ProjectHealthTracker.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Manager { get; set; }

    public string TaskTitle { get; set; } = "";
    public string TaskOwner { get; set; } = "";
    public DateTime? TaskDueDate { get; set; }
    public ItemStatus TaskStatus { get; private set; } = ItemStatus.NotStarted;

    public string MilestoneTitle { get; set; } = "";
    public string MilestoneOwner { get; set; } = "";
    public DateTime? MilestoneDueDate { get; set; }
    public ItemStatus MilestoneStatus { get; private set; } = ItemStatus.NotStarted;

    public string RiskTitle { get; set; } = "";
    public string RiskOwner { get; set; } = "";
    public ItemStatus RiskStatus { get; private set; } = ItemStatus.Closed;
    public int RiskLikelihood { get; set; }
    public int RiskImpact { get; set; }
    public string RiskScenario { get; set; } = "";
    public string RiskPlan { get; set; } = "";

    public Project(int id, string name, string manager)
    {
        Id = id;
        Name = name;
        Manager = manager;
    }

    // The section numbers match the menu: 1 = task, 2 = milestone, 3 = risk.
    public List<ItemStatus> GetAllowedStatuses(int section)
    {
        if (section == 3)
            return new List<ItemStatus> { ItemStatus.Open, ItemStatus.Closed };
        if (section == 1 || section == 2)
            return new List<ItemStatus> { ItemStatus.NotStarted, ItemStatus.InProgress, ItemStatus.Completed, ItemStatus.Blocked };
        return new List<ItemStatus>();
    }

    public bool ChangeStatus(int section, ItemStatus newStatus)
    {
        if (!GetAllowedStatuses(section).Contains(newStatus)) return false;
        if (section == 1) TaskStatus = newStatus;
        if (section == 2) MilestoneStatus = newStatus;
        if (section == 3) RiskStatus = newStatus;
        return true;
    }

    public int CalculateRiskScore()
    {
        return RiskLikelihood * RiskImpact;
    }

    public HealthStatus CalculateHealth()
    {
        if (RiskStatus == ItemStatus.Open && RiskImpact >= 4)
            return HealthStatus.OffTrack;
        if (RiskStatus == ItemStatus.Open)
            return HealthStatus.AtRisk;
        if (MilestoneStatus != ItemStatus.Completed && MilestoneDueDate.HasValue &&
            MilestoneDueDate.Value.Date < DateTime.Today)
            return HealthStatus.AtRisk;
        return HealthStatus.OnTrack;
    }

    public string GetRiskDetails()
    {
        return $"Risk: {RiskTitle} | Owner: {RiskOwner} | Status: {RiskStatus}\n" +
            $"Likelihood: {RiskLikelihood}/5 | Impact: {RiskImpact}/5 | Priority score: {CalculateRiskScore()}/25 (not a percentage)\n" +
            $"Scenario: {RiskScenario}\nCurrent plan: {RiskPlan}";
    }

    public string GetDetails()
    {
        return $"{Name} | Manager: {Manager} | Health: {CalculateHealth()}\n" +
            $"Task: {TaskTitle} | Owner: {TaskOwner} | Status: {TaskStatus} | Due: {TaskDueDate:d}\n" +
            $"Milestone: {MilestoneTitle} | Owner: {MilestoneOwner} | Status: {MilestoneStatus} | Due: {MilestoneDueDate:d}\n" +
            GetRiskDetails();
    }
}

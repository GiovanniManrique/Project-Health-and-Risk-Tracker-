// One project has one task, one milestone, and one risk.
namespace ProjectHealthTracker.Models;

public class Project
{
    // Each object has its own values. The constructor fills these three identity properties.
    public int Id { get; set; }
    public string Name { get; set; }
    public string Manager { get; set; }

    // One task: what someone needs to do. A ? date can be missing (null).
    public string TaskTitle { get; set; } = "";
    public string TaskOwner { get; set; } = "";
    public DateTime? TaskDueDate { get; set; }
    public ItemStatus TaskStatus { get; private set; } = ItemStatus.NotStarted;

    // One milestone: a checkpoint with a date that can affect project health.
    public string MilestoneTitle { get; set; } = "";
    public string MilestoneOwner { get; set; } = "";
    public DateTime? MilestoneDueDate { get; set; }
    public ItemStatus MilestoneStatus { get; private set; } = ItemStatus.NotStarted;

    // One risk: a possible problem, its ratings, scenario and response plan.
    // private set keeps status assignment inside this class, through ChangeStatus.
    public string RiskTitle { get; set; } = "";
    public string RiskOwner { get; set; } = "";
    public ItemStatus RiskStatus { get; private set; } = ItemStatus.Closed;
    public int RiskLikelihood { get; set; }
    public int RiskImpact { get; set; }
    public string RiskScenario { get; set; } = "";
    public string RiskPlan { get; set; } = "";

    public Project(int id, string name, string manager)
    {
        // Copy the constructor's incoming arguments into this object's properties.
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
        // Reject a status that does not belong to the chosen section.
        List<ItemStatus> allowedStatuses = GetAllowedStatuses(section);
        bool isAllowed = allowedStatuses.Contains(newStatus);
        if (isAllowed == false) return false;

        // Change one property on this same Project object.
        if (section == 1) TaskStatus = newStatus;
        if (section == 2) MilestoneStatus = newStatus;
        if (section == 3) RiskStatus = newStatus;
        return true;
    }

    public int CalculateRiskScore()
    {
        // This priority score is a ranking, not a percentage or a failure probability.
        return RiskLikelihood * RiskImpact;
    }

    public HealthStatus CalculateHealth()
    {
        // Check the most serious rule first. return ends this method immediately.
        bool riskIsOpen = RiskStatus == ItemStatus.Open;
        if (riskIsOpen)
        {
            if (RiskImpact >= 4) return HealthStatus.OffTrack;
            return HealthStatus.AtRisk;
        }

        // Only read the date's Value after confirming that a date exists.
        bool milestoneIsLate = false;
        if (MilestoneDueDate.HasValue)
        {
            milestoneIsLate = MilestoneDueDate.Value.Date < DateTime.Today;
        }
        bool milestoneIsFinished = MilestoneStatus == ItemStatus.Completed;
        if (milestoneIsLate && milestoneIsFinished == false) return HealthStatus.AtRisk;
        return HealthStatus.OnTrack;
    }

    public string GetRiskDetails()
    {
        // Return display/prompt text; this method does not change the project.
        int score = CalculateRiskScore();
        return $"Risk: {RiskTitle} | Owner: {RiskOwner} | Status: {RiskStatus}\n" +
            $"Likelihood: {RiskLikelihood}/5 | Impact: {RiskImpact}/5 | Priority score: {score}/25 (not a percentage)\n" +
            $"Scenario: {RiskScenario}\nCurrent plan: {RiskPlan}";
    }

    public string GetDetails()
    {
        // Build the details screen using the health result and risk description.
        HealthStatus health = CalculateHealth();
        string riskDetails = GetRiskDetails();
        return $"{Name} | Manager: {Manager} | Health: {health}\n" +
            $"Task: {TaskTitle} | Owner: {TaskOwner} | Status: {TaskStatus} | Due: {TaskDueDate:d}\n" +
            $"Milestone: {MilestoneTitle} | Owner: {MilestoneOwner} | Status: {MilestoneStatus} | Due: {MilestoneDueDate:d}\n" +
            riskDetails;
    }
}

// One project has one task, one milestone, and one risk.
namespace ProjectHealthTracker.Models;

public class Project
{
    // Each object has its own values
    public int Id { get; set; }
    public string Name { get; set; }
    public string Manager { get; set; }

    // One task: what someone needs to do
    public string TaskTitle { get; set; } = "";
    public string TaskOwner { get; set; } = "";
    public DateTime? TaskDueDate { get; set; }
    public ItemStatus TaskStatus { get; private set; } = ItemStatus.NotStarted;

    // One milestone: a checkpoint with a date 
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
       
        List<ItemStatus> allowedStatuses = GetAllowedStatuses(section);
        bool isAllowed = allowedStatuses.Contains(newStatus);
        if (isAllowed == false) return false;

        
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
        
        bool riskIsOpen = RiskStatus == ItemStatus.Open;
        if (riskIsOpen)
        {
            if (RiskImpact >= 4) return HealthStatus.OffTrack;
            return HealthStatus.AtRisk;
        }

        
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

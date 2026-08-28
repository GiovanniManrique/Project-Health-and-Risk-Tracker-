// Purpose: Represents an important date or goal for a project.
// Properties: Includes the shared item properties, TargetDate, and IsAchieved.
// Methods: GetDetails returns a line of information about the milestone.

namespace ProjectHealthTracker.Models;

public class Milestone : ProjectItem
{
    public DateTime TargetDate { get; set; }
    public bool IsAchieved { get; set; }

    public Milestone(int id, string title, string owner, ItemStatus status,
        DateTime targetDate, bool isAchieved)
        : base(id, title, owner, status)
    {
        TargetDate = targetDate;
        IsAchieved = isAchieved;
    }

    public override string GetDetails()
    {
        return $"Milestone {Id}: {Title} | Owner: {Owner} | Status: {Status} | Target: {TargetDate:d}";
    }
}

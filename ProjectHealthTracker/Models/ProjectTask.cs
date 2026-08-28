// Purpose: Represents a task that belongs to a project.
// Properties: Includes the shared item properties, DueDate, and IsCompleted.
// Methods: GetDetails returns a line of information about the task.

namespace ProjectHealthTracker.Models;

public class ProjectTask : ProjectItem
{
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }

    public ProjectTask(int id, string title, string owner, ItemStatus status,
        DateTime dueDate, bool isCompleted)
        : base(id, title, owner, status)
    {
        DueDate = dueDate;
        IsCompleted = isCompleted;
    }

    public override string GetDetails()
    {
        return $"Task {Id}: {Title} | Owner: {Owner} | Status: {Status} | Due: {DueDate:d}";
    }
}

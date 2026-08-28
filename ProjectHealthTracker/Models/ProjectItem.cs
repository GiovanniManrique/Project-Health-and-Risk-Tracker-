// Purpose: Provides the properties shared by every item in a project.
// Properties: Id, Title, Owner, and Status.
// Methods: GetDetails is completed differently by each child class.

namespace ProjectHealthTracker.Models;

public abstract class ProjectItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Owner { get; set; }
    public ItemStatus Status { get; set; }

    protected ProjectItem(int id, string title, string owner, ItemStatus status)
    {
        Id = id;
        Title = title;
        Owner = owner;
        Status = status;
    }

    public abstract string GetDetails();
}

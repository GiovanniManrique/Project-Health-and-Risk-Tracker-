// Purpose: Stores basic project information and all items assigned to the project.
// Properties: Id, Name, Manager, StartDate, EndDate, and Items.
// Methods: The constructor sets the project information and creates the item list.

namespace ProjectHealthTracker.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Manager { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<ProjectItem> Items { get; set; }

    public Project(int id, string name, string manager, DateTime startDate, DateTime endDate)
    {
        Id = id;
        Name = name;
        Manager = manager;
        StartDate = startDate;
        EndDate = endDate;
        Items = new List<ProjectItem>();
    }
}

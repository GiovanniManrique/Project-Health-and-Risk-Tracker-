namespace ProjectHealthTracker.Models;

public class Risk
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Owner { get; set; }
    public string Scenario { get; set; } = "";
    public string Plan { get; set; } = "";
    public int Likelihood { get; set; }
    public int Impact { get; set; }
    public ItemStatus Status { get; private set; } = ItemStatus.Closed;
    public List<string> SourceIds { get; set; } = new List<string>();

    public Risk(string id, string title, string owner)
    {
        Id = id;
        Title = title;
        Owner = owner;
    }

    public bool ChangeStatus(ItemStatus newStatus)
    {
        if (newStatus != ItemStatus.Open && newStatus != ItemStatus.Closed) return false;
        Status = newStatus;
        return true;
    }

    public int CalculateScore()
    {
        return Likelihood * Impact;
    }
}

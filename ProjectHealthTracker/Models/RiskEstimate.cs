namespace ProjectHealthTracker.Models;

// This is advice returned by the model, not a saved Risk record.
public class RiskEstimate
{
    public string Id { get; set; } = "";
    public int Likelihood { get; set; }
    public int Impact { get; set; }
    public string Reason { get; set; } = "";
    public List<string> SourceIds { get; set; } = new List<string>();
    public string Action { get; set; } = "";

    public int CalculateScore()
    {
        return Likelihood * Impact;
    }
}

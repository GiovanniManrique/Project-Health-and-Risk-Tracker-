// Purpose: Represents a possible problem that may affect a project.
// Properties: Includes the shared item properties, Probability, Impact, and MitigationPlan.
// Methods: GetDetails returns a line of information about the risk.

namespace ProjectHealthTracker.Models;

public class Risk : ProjectItem
{
    public int Probability { get; set; }
    public int Impact { get; set; }
    public string MitigationPlan { get; set; }

    public Risk(int id, string title, string owner, ItemStatus status,
        int probability, int impact, string mitigationPlan)
        : base(id, title, owner, status)
    {
        Probability = probability;
        Impact = impact;
        MitigationPlan = mitigationPlan;
    }

    public override string GetDetails()
    {
        return $"Risk {Id}: {Title} | Owner: {Owner} | Status: {Status} | " +
               $"Probability: {Probability}/5 | Impact: {Impact}/5 | Plan: {MitigationPlan}";
    }
}

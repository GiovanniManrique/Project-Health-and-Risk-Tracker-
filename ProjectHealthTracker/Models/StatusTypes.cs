// Enums are fixed sets of named choices. They are not classes or methods.

namespace ProjectHealthTracker.Models;

public enum ItemType
{
    Task,
    Milestone,
    Risk
}

public enum ItemStatus
{
    NotStarted,
    InProgress,
    Completed,
    Blocked,
    Open,
    Closed
}

public enum HealthStatus
{
    OnTrack,
    AtRisk,
    OffTrack
}

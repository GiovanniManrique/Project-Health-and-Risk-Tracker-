// Enums are fixed sets of named choices. They are not classes or methods.

namespace ProjectHealthTracker.Models;

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



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

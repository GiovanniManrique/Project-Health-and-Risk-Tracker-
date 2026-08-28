// Purpose: Stores the status choices used by the project tracker.
// Enums: ItemStatus is for project items, and HealthStatus is for projects.

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

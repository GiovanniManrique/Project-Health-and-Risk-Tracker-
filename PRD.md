# Project Health and Risk Tracker - Product Requirements Document

## Purpose

The Project Health and Risk Tracker helps a program manager monitor tasks, milestones, risks, and overall project health.
## Technology

- **Language:** C#
- **Framework/version:** .NET 10
- **Application type:** Console application
- **Storage:** In-memory mock data using `List<T>`

## Custom Data Types

- `ProjectItem` *(abstract parent)*: `Id`, `Title`, `Owner`, `Status`, and abstract `GetDetails()`.
  - `ProjectTask : ProjectItem`: adds `DueDate` and `IsCompleted`.
  - `Milestone : ProjectItem`: adds `TargetDate` and `IsAchieved`.
  - `Risk : ProjectItem`: adds `Probability`, `Impact`, and `MitigationPlan`.
- `Project`: project details and a `List<ProjectItem>`.
- `ProjectService`: updates status, counts risks, and calculates health.
- Enums: `ItemStatus` and `HealthStatus`.

## Preliminary Solution Structure

```text
ProjectHealthTracker/
|-- Program.cs                       # Menu, input, branching, loops
|-- Models/
|   |-- Project.cs
|   |-- ProjectItem.cs              # Abstract parent
|   |-- ProjectTask.cs
|   |-- Milestone.cs
|   |-- Risk.cs
|   `-- StatusTypes.cs              # Enums
|-- Services/ProjectService.cs      # Business rules
`-- Data/MockProjectData.cs         # Sample data
```

Each code file will start with comments outlining its purpose, properties, and methods.

## External Resources

No database, cloud service, or API is required. `MockProjectData.cs` will return a small `List<Project>`. A database can replace this source after it is covered in class without changing the remaining application.

## Planned Development Time

**10 hours:** setup (1), models/inheritance (2), mock data (1), services (2), menu/validation (2), testing (1), and documentation (1).

## Pseudocode Implementation

```text
Create a .NET 10 console project named ProjectHealthTracker
Create the listed folders/files and add outline comments to each code file
Define ProjectItem; inherit ProjectTask, Milestone, and Risk from it
Create Project, status enums, and 2-3 mock projects in MockProjectData
Pass the mock projects into ProjectService

SET applicationRunning to true
WHILE applicationRunning
    DISPLAY menu: list projects, view details, update status, show risks,
                  show health summary, or exit
    READ user choice
    USE switch branching to call a method
    USE foreach loops to display projects/items
    USE if/else to validate IDs and calculate health:
        IF a high-impact risk is open, project is OffTrack
        ELSE IF any risk is open or milestone is late, project is AtRisk
        ELSE project is OnTrack
    DISPLAY a helpful message for invalid input
END WHILE
```

The app showcases branching, loops, methods, classes, collections, enums, validation, encapsulation and inheritance.

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

### Optional local AI extension

Menu option 7 connects to an existing Ollama server at `localhost:11434`. The user selects an open risk and an installed local text model. C# calculates a priority score as Probability multiplied by Impact; the model explains the supplied risk information and suggests one action. This score is not a statistical probability. AI advice does not modify data or replace the health rules below. The core tracker remains usable when Ollama or a model is unavailable. No cloud API key, model download, database, or additional NuGet package is required by this extension.

### Optional OpenAI risk review

Menu option 8 uses a key stored in the Windows user environment variable `AI_API_KEY`. The user selects an open risk and can answer two short questions: what evidence suggests the problem may happen, and what work would be affected. Empty answers remain unknown. The review displays the stored likelihood and impact ratings, their product, risk owner and mitigation, open/high-impact risk counts, late milestones, completed tasks, days to the planned end date, and the existing calculated health. It previews the information and sends it only when the user chooses to request advice.

`CloudAiService` sends one HTTPS request to OpenAI's Responses API using `gpt-4.1-mini` and returns text. The model explains what needs attention, the supporting evidence, and one action. It does not assign probabilities, invent facts, edit data, or replace the health rules. Credentials stay outside source control. API usage requires an appropriately configured account and may incur charges; error cases return to the menu. No additional packages, database, or AI framework are introduced.

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

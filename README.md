# Project Health and Risk Tracker

A C# .NET 10 console application for viewing projects, tracking tasks and milestones, updating statuses, and checking project risks and health. It follows the requirements in [PRD.md](PRD.md).

## Run the application

Open `ProjectHealthTracker.slnx` in Visual Studio with .NET 10 support. Run the `ProjectHealthTracker` project with **Ctrl+F5**.

Or, from this repository's folder, run:

```powershell
dotnet run --project ProjectHealthTracker/ProjectHealthTracker.csproj
```

The program uses three sample projects stored in `List<Project>`. Changes remain available while the program runs and reset when it restarts. There is no database, file storage, API, or extra package to install.

## Menu

1. **List projects:** View IDs, managers, open risk counts, and current health.
2. **View project details:** Enter a project ID to see its dates and all tasks, milestones, and risks.
3. **Update item status:** Enter a project ID, an item ID shown for that project, and a status number. Enter `0` at the item or status prompt to cancel.
4. **Show project risks:** View open risks with probability, impact, owner, and mitigation plan. Closed risks remain visible in project details.
5. **Show health summary:** View each project's health, open risks, late milestones, and overall totals.
6. **Exit.**

Tasks and milestones can be NotStarted, InProgress, Completed, or Blocked. Risks can be Open or Closed. Completing a task sets `IsCompleted` to true; completing a milestone sets `IsAchieved` to true. Changing either back to another progress status clears that flag. Invalid input displays a helpful message and returns to the menu without changing data.

## Health rules

Rules are checked in this order:

| Health | Condition |
| --- | --- |
| OffTrack | At least one open risk has impact 4 or 5 on the sample data's 1–5 scale. |
| AtRisk | Another risk is open, or an unachieved milestone has a target date before today. |
| OnTrack | Neither condition applies. |

The PRD does not define the high-impact threshold; this app retains the existing code's threshold of 4. A milestone due today is not late. Closed risks and achieved milestones do not lower health. Task due dates and risk probability are displayed but do not change these PRD health rules.

## Quick demonstration

1. Choose **5**. Website is OnTrack, Inventory is OffTrack, and Training is AtRisk.
2. Choose **4**, project **2**. Risk **203** is open with impact 5.
3. Choose **3**, project **2**, item **203**, status **2** (Closed). Inventory becomes OnTrack.
4. Choose **3**, project **3**, item **302**, status **3** (Completed). Training becomes OnTrack.
5. Choose **5**. All three projects are now OnTrack.
6. Reopen risk **203** with status **1**. Inventory becomes OffTrack again.

Sample dates are relative to today so this demonstration works on later days too.

## Understand the code

| File | Responsibility |
| --- | --- |
| `Program.cs` | Runs the `while` menu loop, branches with `switch` and `if/else`, reads input with `TryParse`, and displays results. |
| `Models/ProjectItem.cs` | Abstract parent containing the shared ID, title, owner, status, and `GetDetails()` method. |
| `Models/ProjectTask.cs` | Inherits shared properties and adds a due date and completion flag. |
| `Models/Milestone.cs` | Inherits shared properties and adds a target date and achievement flag. |
| `Models/Risk.cs` | Inherits shared properties and adds probability, impact, and a mitigation plan. |
| `Models/Project.cs` | Holds project details and a `List<ProjectItem>`. |
| `Models/StatusTypes.cs` | Defines named item and health statuses with enums. |
| `Services/ProjectService.cs` | Uses `foreach` loops to find items, validate status changes, count risks and late milestones, and calculate health. |
| `Data/MockProjectData.cs` | Creates three sample projects with ordinary constructors and `List.Add`. |

Application files are inside the `ProjectHealthTracker` folder. Each begins with outline comments.

Inheritance lets tasks, milestones, and risks share the parent properties. Each child overrides `GetDetails()`, so one `foreach (ProjectItem item ...)` loop can display different item types. The service keeps its project-list field private and groups the business rules into methods. The application uses ordinary classes, constructors, lists, loops, enums, and methods; it does not need LINQ, async code, or a dependency injection framework.

## Build and verify

```powershell
dotnet build ProjectHealthTracker.slnx --configuration Release
dotnet run --project Tests/TrackerChecks.csproj --configuration Release
```

The optional checks are a separate console project with no testing packages. They test the health boundaries, status changes and reversals, invalid IDs and statuses, menu flows, cancellation, and input ending. A failed check returns a nonzero exit code. The main solution includes only the application so its startup project stays straightforward.

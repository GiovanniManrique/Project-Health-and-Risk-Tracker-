# Project Health and Risk Tracker

A small C# .NET 10 console app with **two authored application classes: `Program` and `Project`**. Each fictional project has exactly **one task, one milestone, and one risk**. There is no `ProjectItem` class, nested item list, item ID, or custom domain inheritance hierarchy.

## Open and run

Open `ProjectHealthTracker.slnx` in Visual Studio 2026 with .NET 10 installed. Press **Ctrl+F5**. Type a menu number and press Enter.

```text
1. List projects and health
2. View project details
3. Update a status
4. Exit
```

Changes are kept in memory and reset on restart. At a selection/status prompt, `0` cancels. Invalid choices and end-of-input are handled without a crash or an endless loop.

## Learn it in the order it runs

From PowerShell in this repository, start the local guide:

```powershell
& .\presentation\Start-Guide.ps1
```

Open **[the learning page on localhost](http://127.0.0.1:8765/)**. The launcher uses installed Python, with Codex's bundled runtime as a fallback. It reuses an existing guide server. Run it again after restarting the computer. By default the server listens only on this computer and serves only the guide. You can also open [presentation/learn.html](presentation/learn.html) directly without a server.

To read the guide on another device on your home network, run the launcher with this computer's LAN IPv4 address (shown by `ipconfig`):

```powershell
& .\presentation\Start-Guide.ps1 -BindAddress 10.0.0.176
```

Then open `http://10.0.0.176:8765/#start` on the other device. This is the current address of the development computer; replace it if it changes. Keep the hosting computer awake. The server still serves only the teaching page and health endpoint. Windows must allow the chosen Python executable through the firewall on the private network. This serves only the teaching page, not project files.

Start with **Start to finish** for ten whole code blocks, then use the **8-minute presentation** to rehearse. The guide includes current code, before/after state, plain-English meaning, technical detail, questions, and multiple-choice/written practice. Scores below 70% activate repeat practice until at least 90%; written scoring is self-assessed.

Regenerate the guide after code edits with `python presentation/build_guide.py`. The page and application work without a model or API key. The former local-model integration and its menu option have been removed.

## Two classes, three C# files

| File | Responsibility |
|---|---|
| `Program.cs` | Main, menu, project selection, status input, and sample data. |
| `Models/Project.cs` | Project identity; direct task/milestone/risk properties; validated status changes; score, health, and details. |
| `Models/StatusTypes.cs` | Two enums, `ItemStatus` and `HealthStatus`. Enums are named choices, not authored classes. |

A new Project defaults to NotStarted task/milestone statuses and a Closed risk. The sample-data method changes selected defaults through `ChangeStatus`. Only Project can directly assign its status properties; ratings are public properties initialized to sample values from 1–5, without setter validation. A larger project with multiple tasks or risks would need a broader data design. Test helpers and .NET library classes are separate from the two authored application classes.

## Repeatable demo

Start a fresh session. Each arrow below means press Enter after typing the preceding value.

| Input | Result |
|---|---|
| `1` | Website OnTrack, Inventory OffTrack, Training AtRisk. |
| `2 → 2` | View Inventory's full details. |
| `3 → 2 → 3 → 2` | Update Inventory's risk to Closed; Inventory becomes OnTrack. |
| `3 → 3 → 2 → 3` | Update Training's milestone to Completed; Training becomes OnTrack. |
| `1` | All three are OnTrack. |
| `3 → 2 → 3 → 1` | Reopen Inventory's risk; Inventory returns to OffTrack. |
| `4` | Exit. |

The update menu uses section **1 Task, 2 Milestone, 3 Risk**. There are no item IDs such as 203.

## Rules and verification

- An Open risk with impact 4 or 5 gives OffTrack.
- Otherwise any Open risk or overdue unfinished milestone gives AtRisk.
- Otherwise health is OnTrack. Due today is not overdue; an overdue task alone does not affect health.
- Priority score is likelihood × impact. **20/25 is not an 80% chance of failure.** Health uses separate direct checks and no longer loops through items.

```powershell
dotnet build ProjectHealthTracker.slnx -c Release
dotnet run --project Tests/TrackerChecks.csproj -c Release
```

The checks cover status validation, health rules, sample data, console navigation, cancellation, and end-of-input. They run entirely offline.

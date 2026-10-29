# Project Health and Risk Tracker

A small C# .NET 10 console project for a classroom demonstration. It tracks three fictional projects, their task and milestone statuses, and multiple risks. A local **Gemma 4 E2B** model in **Bionic** reads fictional emails, project notes, and metrics to suggest likelihood and impact ratings. C# validates the reply, multiplies the ratings, and ranks the risks.

The source uses simple classes, properties, lists, loops, and named methods. It has no database, custom inheritance, extra NuGet package, cloud AI service, or real email connection.

## Open and run

Open `ProjectHealthTracker.slnx` in Visual Studio with .NET 10 installed, then press **Ctrl+F5**. You can also run:

```powershell
dotnet run --project ProjectHealthTracker/ProjectHealthTracker.csproj
```

```text
1. List projects and health
2. View project details
3. Update a status
4. AI risk brief
5. Exit
```

Type a number and press Enter. Selection and status prompts accept `0` to cancel. Project status changes stay in memory and reset on restart. The ordinary list, details, and update features work without a model.

## Run the local AI brief

1. Open Bionic and load **Gemma 4 E2B** (`google/gemma-4-e2b`). Having the model in the Library is different from having a loaded instance.
2. Start Bionic's **Local Model API** on this computer. The default application address is `http://127.0.0.1:1235/v1`.
3. Start the tracker, choose menu **4**, and select project **2**, Inventory System.

If your local API uses a different port or model ID, set these variables in PowerShell before launching the tracker:

```powershell
$env:BIONIC_BASE_URL = 'http://127.0.0.1:1235/v1'
$env:BIONIC_MODEL = 'google/gemma-4-e2b'
dotnet run --project ProjectHealthTracker/ProjectHealthTracker.csproj
```

The application reads the selected project's files from its `DemoData` folder on each request. For Inventory these are `INV-EMAIL.eml`, `INV-NOTES.txt`, and `INV-METRICS.json`. They simulate an email message, project notes, and numeric observations. No mailbox is read, and no email is sent. These files are copied with the application when it builds; rebuild after editing the source fixtures.

One brief uses one bounded local model request with a 120-second timeout. Local inference speed depends on the computer. The output identifies the requested and returned model and source IDs (filenames without extensions), shows proposed likelihood and impact ratings from 1–5, calculates a 1–25 priority score for each open risk, and lists up to three next actions. It also identifies missing evidence. The model cannot run tools, update project records, or change stored ratings and health. With no open risks, a brief can still discuss health and the milestone; its risk list must be empty.

**A score of 20/25 is a priority ranking, not an 80% probability of failure.** The AI ratings are estimates from fictional evidence, not measured or calibrated probabilities. The stored tracker health and the separate AI estimates serve different purposes.

C# separately reports a missing recorded mitigation plan under **Recorded data gaps (C# checks)**. Model-generated reasons and missing-information notes still need review: they can be incomplete or ask for details already supplied. Validating an evidence ID does not independently verify a claim.

If the model is unavailable, a request times out, evidence is missing, or a response fails validation, the app explains the problem and returns to the menu. It does not substitute a made-up successful AI response.

See [DEMO.md](DEMO.md) for the presentation sequence and scenarios to check.

## Where the work happens

| Source | Responsibility |
|---|---|
| `Program` | Sample projects, repeated menu, selections, status updates, and displaying the brief. |
| `Project` | Project identity, one task, one milestone, a list of risks, and stored health rules. |
| `Risk` | One stored risk's ID, title, owner, status, ratings, scenario, and plan. |
| `DemoData/` | Nine fictional fixture files: one email, notes file, and metrics file for each project. This is a folder, not a class. |
| `AiRiskService` | Building context, calling the local model, validating estimates/citations, and producing the brief. |
| `RiskEstimate` | Separate AI-proposed ratings, explanation, and evidence references. |
| `StatusTypes.cs` | `ItemStatus` and `HealthStatus` enums: named choices rather than classes. |

## Stored health rules

- Any Open risk with stored impact 4 or 5 makes the project OffTrack.
- Otherwise any Open risk or overdue unfinished milestone makes it AtRisk.
- Otherwise it is OnTrack. Due today is not overdue; an overdue task alone does not determine health.
- Stored risk priority is likelihood × impact. Closing a risk removes it from the AI brief; other open risks can still keep the project OffTrack or AtRisk.

Fresh samples are Website OnTrack, Inventory OffTrack with three open risks, and Training AtRisk with an overdue unfinished milestone and a closed risk.

## Learn it in execution order

Open [presentation/learn.html](presentation/learn.html) directly, or start the local guide:

```powershell
python presentation/build_guide.py
& .\presentation\Start-Guide.ps1
```

Open [the learning page](http://127.0.0.1:8765/). This is a separate teaching server; the page does not call the model. It serves only the guide and a health endpoint. The launcher uses installed Python, with Codex's bundled runtime as a fallback.

The guide contains complete code blocks, explanations, speaking notes, full current C# source, and a practice quiz. A score below 70% activates repeat practice until a later score reaches at least 90%. The written answer is openly self-assessed. Regenerate the guide after source changes and review the explanations as well as the embedded code.

## Build and checks

```powershell
dotnet build ProjectHealthTracker.slnx -c Release
dotnet run --project Tests/TrackerChecks.csproj -c Release
dotnet run --project Tests/TrackerChecks.csproj -c Release -- --live
python presentation/build_guide.py
```

The normal test command runs offline checks of C# rules and simulated model responses. The optional `--live` command calls Bionic for five scenarios and saves `evaluation/live-model-checks.txt`; it needs the local server and model running. Current verification evidence is recorded in [presentation/REVIEW.md](presentation/REVIEW.md); historical model runs do not prove that the current setup is running.

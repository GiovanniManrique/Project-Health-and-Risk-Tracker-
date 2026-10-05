# Project Health and Risk Tracker

A small C# .NET 10 console app with **two authored application classes: `Program` and `Project`**. Each fictional project has exactly **one task, one milestone, and one risk**. There is no `ProjectItem` class, nested item list, item ID, or custom domain inheritance hierarchy.

## Open and run

Open `ProjectHealthTracker.slnx` in Visual Studio 2026 with .NET 10 installed. Press **Ctrl+F5**. Type a menu number and press Enter.

```text
1. List projects and health
2. View project details
3. Update a status
4. Explain a risk with local AI
5. Exit
```

Changes are kept in memory and reset on restart. At a selection/status prompt, `0` cancels. Invalid choices and end-of-input are handled without a crash or an endless loop.

## Learn it in the order it runs

From PowerShell in this repository, start the local guide:

```powershell
& .\presentation\Start-Guide.ps1
```

Open **[the learning page on localhost](http://127.0.0.1:8765/)**. The launcher uses the bundled Python runtime on this computer, or Python on PATH elsewhere. It reuses an existing guide server. Run it again after restarting the computer. The server listens only on this computer and serves the guide, not the repository. You can also open [presentation/learn.html](presentation/learn.html) directly without a server.

Start with **14-minute study**, then use the separate **8-minute talk**. The short study path covers every menu option and all 15 methods plus the Project constructor. Its 872 words of plain/technical explanation and 77 primary source lines are a first pass; additional excerpts, design reasoning and speaking cues are optional. The seven sections are:

- **14-minute study:** twelve chronological learning steps with current commented source, a complete method map, and three final prediction questions. Next advances directly to the next study step.
- **Big picture:** what C#, .NET, Visual Studio and Bionic do; the two classes; and the exact meaning of `List<Project> projects = CreateSampleProjects();`.
- **Code detail:** a chronological run from Main through calls and returns, including the concrete change from typed text `"2"` to integer `2`, index `1`, Closed status and OnTrack health.
- **AI detail:** ten chapters covering the actual connection. Each block has plain-English and technical explanations, reasons for the choice, example input/output values, and clickable syntax. A separate seven-stage display shows the full prompt, JSON request, illustrative reply and printed result.
- **Questions & commands:** 28 audience questions with answers, technical follow-ups and links to the relevant code; nine examples distinguish Visual Studio shortcuts, console input, C# calls, HTTP operations and PowerShell commands.
- **8-minute talk:** a 556-word script covering the complete program's responsibilities, plus a repeatable demo and timer. Explaining the design and important statements fits this route; reading every source line aloud takes longer.
- **Practice quiz:** four multiple-choice questions and one written answer per round. Written scoring is an explicit self-check. Below 70% starts repeat practice until at least 90%; this does not certify understanding.

The guide server on port **8765** and Bionic's model API on port **51500** are separate. The page does not call AI or edit the app. All source shown is embedded from the three application files. Regenerate it using `python presentation/build_guide.py` after code edits and manually review explanations and line ranges for changed meaning. The complete request/reply examples are labelled teaching examples; the earlier real test reply is separately labelled as recorded.

The readability revision separates conversion from range checks, names intermediate values, expands menu actions and sample assignments onto separate lines, and adds purpose comments. It uses a regular string for the short AI answer. More statements are visible, so the source has more lines; there are still two authored classes and the same application features.

## Two classes, three C# files

| File | Responsibility |
|---|---|
| `Program.cs` | Main, menu, project selection, status input, sample data, and the Bionic request/response. |
| `Models/Project.cs` | Project identity; direct task/milestone/risk properties; validated status changes; score, health, and details. |
| `Models/StatusTypes.cs` | Two enums, `ItemStatus` and `HealthStatus`. Enums are named choices, not authored classes. |

A new Project defaults to NotStarted task/milestone statuses and a Closed risk. The sample-data method changes selected defaults through `ChangeStatus`. Only Project can directly assign its status properties; public rating properties are checked for the 1–5 range before AI submission, not on every assignment. A larger project with multiple tasks or risks would need a broader data design. Test helpers and .NET library classes are separate from the two authored application classes.

## Local AI setup and connection

Use the installed `qwen/qwen3.5-9b` model in **Bionic**, loaded into memory before the demonstration. In Bionic's **Settings > Local Model API**, enable the local server. On this computer its address is `http://127.0.0.1:51500`; the nonsecret `BionicUrl` and `ModelKey` constants in Program select the address and exact model. Change them and rebuild if your setup differs. A 4,096-token context was used in the live test.

Bionic uses the LM Studio local runtime API underneath. This application calls that local API directly, not the Bionic chat UI or an agent session. No OpenAI key, subscription, cloud fallback, or extra NuGet package is used. Keep the runtime/server running with Qwen loaded; its chat window need not be in front.

The request path is:

1. `ReviewRiskAsync` selects a Project with an Open risk, previews the supplied facts, and asks for `y`.
2. `GetLoadedModelAsync` uses GET `/api/v1/models` to find a loaded instance of the exact Qwen model. A downloaded file alone is insufficient.
3. `ExplainRiskAsync` builds the prompt from project name, C# health, and risk details. `JsonSerializer.Serialize` creates JSON; `StringContent` wraps the request body.
4. `await client.PostAsync(BionicUrl + "/api/v1/chat", body)` sends the actual generation request.
5. The code parses the JSON reply, collects only `message` content, and returns a string. `Console.WriteLine(answer)` prints it in the same console.

The app sends no model-download/load command. A state change between preflight and chat can still cause the runtime to reload a model. The request disables reasoning mode, streaming, stored chat through the API, and integrations/tools. The five-second readiness check and 120-second chat-request timeout prevent indefinite HTTP waits. Expected HTTP/network, timeout, malformed, and empty responses produce a readable message. There is no canned-answer fallback. The console waits for the AI operation before accepting another menu choice.

AI explains fictional facts; it does not calculate or change the ratings, status, score, or health. The prompt asks for a short Risk / Why / Next action response, but does not guarantee factual accuracy, length, or format.

References: [Bionic local models](https://lmstudio.ai/docs/bionic/models), [native chat API](https://lmstudio.ai/docs/developer/rest/chat).

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
| `4 → 2 → y` | Ask local Qwen to explain Inventory's risk. |
| `5`, after the request finishes | Exit. |

The update menu uses section **1 Task, 2 Milestone, 3 Risk**. There are no item IDs such as 203. Put AI last in a timed presentation. If it takes more than ten seconds, show the guide's labelled recorded reply and finish speaking while the live request remains pending; do not present that recorded answer as the current live response.

## Rules and verification

- An Open risk with impact 4 or 5 gives OffTrack.
- Otherwise any Open risk or overdue unfinished milestone gives AtRisk.
- Otherwise health is OnTrack. Due today is not overdue; an overdue task alone does not affect health.
- Priority score is likelihood × impact. **20/25 is not an 80% chance of failure.** Health uses separate direct checks and no longer loops through items.

```powershell
dotnet build ProjectHealthTracker.slnx -c Release
dotnet run --project Tests/TrackerChecks.csproj -c Release
```

The readability revision passed 106 assertions using simulated HTTP responses without model inference. A separate live console run before this refactor completed the demo status changes and received a real Qwen answer. This revision did not make another live model request. See [presentation/REVIEW.md](presentation/REVIEW.md) for the independent code/teaching review and verification record.

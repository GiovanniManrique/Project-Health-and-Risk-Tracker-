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

Open **[presentation/learn.html](presentation/learn.html)** in a browser. It is one offline visual aid with a suggested 20-minute practice route:

- Start with the two classes and six basic programming words.
- Follow a single run from Main, through calls and returns, to a status change and exit.
- Follow the local AI integration, with exact source blocks and optional detailed syntax explanations. The POST step has buttons explaining each part of the actual request call.
- Rehearse the short talk and demo sequence, then try four multiple-choice questions and one written answer. Written scoring is an explicit self-check. Below 70% starts repeat practice until at least 90%; this does not certify understanding.

The page does not call AI or edit the app. All source shown is embedded from the three application files. Regenerate it using `python presentation/build_guide.py` after code edits and manually review explanations and line ranges for changed meaning.

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

The updated runner passed 101 assertions using simulated HTTP responses without model inference. A separate live console run completed the demo status changes and received a real Qwen answer. See [presentation/REVIEW.md](presentation/REVIEW.md) for the independent code/teaching review and verification record.

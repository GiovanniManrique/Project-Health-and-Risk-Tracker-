# Project Health and Risk Tracker

A small C# .NET 10 console application with **three application classes**: `Program`, `Project`, and `ProjectItem`. It tracks fictional tasks, milestones and risks, updates their status, and prints a real local Qwen explanation through **Bionic**.

## Open and run in Visual Studio

1. Open `ProjectHealthTracker.slnx` in Visual Studio 2026 with .NET 10 installed.
2. Press **Ctrl+F5** to run without debugging (or **F5** to debug).
3. Enter a menu number and press Enter. Enter `0` at an ID/status prompt to cancel.

```text
1. List projects and health
2. View project items
3. Update an item's status
4. Explain a risk with local AI
5. Exit
```

Changes live in memory and reset when the application restarts. No database or account is needed for the tracker.

## Bionic setup

Use the **local** `qwen/qwen3.5-9b` model in Bionic. Load it before the demo; a 4,096-token context is sufficient for this short example. Under **Settings > Local Model API**, enable the local API server and check its address. The two nonsecret constants at the top of `Program.cs` select the server (`http://127.0.0.1:51500` on this computer) and exact model. Change the address there if your Bionic server uses another port. Keep the runtime/server running while using AI.

Bionic uses LM Studio's local model runtime/API. The app calls that API directly; it does not automate Bionic's chat window or start a Bionic agent session. No OpenAI key, subscription, cloud fallback, or extra NuGet package is used. Other computers need their own Bionic runtime and downloaded model.

Option 4 checks `/api/v1/models` for a loaded instance of the exact Qwen model, then sends one `/api/v1/chat` request after you answer `y`. It never issues load/download commands. A preflight check cannot prevent the runtime from reloading a model if its state changes between the check and the request. The request disables tools, stored chat, streaming, and Qwen's configurable reasoning. The answer prints in the same console. Allow up to two minutes; the model-list check has a five-second timeout. Missing models and unsuccessful/empty responses return a message and the menu. There is no canned-answer fallback.

References: [Bionic local models](https://lmstudio.ai/docs/bionic/models), [Local Model API setup](https://github.com/meta-models/meta-oss-cookbook/blob/main/inference-server/lm-studio.md#serve), [native chat API](https://lmstudio.ai/docs/developer/rest/chat).

## Repeatable demonstration

Start a fresh session for these steps:

1. Choose `1`: see one OnTrack, one AtRisk, one OffTrack project.
2. Choose `2`, then project `2`: inspect Inventory System and risk `203` (score 20/25).
3. Choose `3`, project `2`, item `203`, status choice `2` (Closed). Inventory becomes OnTrack.
4. Choose `3`, project `3`, item `302`, status choice `3` (Completed). Training becomes OnTrack.
5. Choose `1`: all three projects now show OnTrack.
6. Choose `3`, project `2`, item `203`, status choice `1` (Open). Inventory returns to OffTrack, and its risk is eligible for AI again.
7. With Qwen loaded, choose `4`, project `2`, item `203`, then `y`. Read the real local answer. Wording can vary; the model is asked to stay consistent with fictional facts but that is not guaranteed. Choose `5` to exit after the request finishes.

Only open risks are eligible. Put AI last in a timed presentation because the console waits for the request to finish before accepting more menu choices. If it takes more than about ten seconds, use the guide's labelled recorded reply and finish speaking without waiting for another console action. Say the live request is still pending. If Bionic is unavailable, the status demonstration still works; do not claim an error or saved example is live AI.

## Learn and present the code

Open **[presentation/learn.html](presentation/learn.html)** in a browser. Start with the short overview, then follow the four code files in order. It contains the exact code with line numbers, explanations beside each section, a local simulation of status changes, a nine-minute presentation script, and multiple-choice plus written practice. The simulator and quiz do not run an AI model. Written responses use disclosed self-check criteria, not an AI grader.

The application has four authored C# files:

| File | Purpose |
|---|---|
| `Models/StatusTypes.cs` | Three enums: fixed choices for item type, item status, and project health. |
| `Models/ProjectItem.cs` | One item, allowed status changes, risk score, and details text. |
| `Models/Project.cs` | One project, its item list, open-risk count, and health rules. |
| `Program.cs` | Entry point, menu/input, sample objects, and local AI request/response. |

There is **no custom domain inheritance hierarchy** in this simplified version. Task, milestone and risk are enum choices within the same item class. A project *contains* items; it is not their parent class. `Status` has a private setter, so normal status changes go through validation. Some fields are relevant only to certain item types. Program has several responsibilities to keep this small classroom app to three classes; a larger application would separate networking again. The HTTP/JSON/async portion is the most advanced part.

## Rules and limits

- Open risk with impact 4 or 5: **OffTrack**.
- Otherwise, any open risk or overdue unfinished milestone: **AtRisk**.
- Otherwise: **OnTrack**.
- Due today is not overdue. An overdue task alone does not change health under these rules.
- Score = likelihood rating x impact rating (each 1-5). **20/25 is not an 80% probability.** Health uses impact and status, so a low score can still accompany OffTrack.
- AI supplies advice only. It does not edit ratings, status or health; the sample stories are fictional.

## Build and checks

```powershell
dotnet build ProjectHealthTracker.slnx
dotnet run --project Tests/TrackerChecks.csproj
```

The small check runner verifies rules, input handling, request construction and simulated response failures without contacting a model. Its helper `FakeBionic` is test code, not a fourth application class. The assertion count is reported on each run. The teaching guide embeds a source snapshot; regenerate it with `python presentation/build_guide.py` after editing C# files, then review the explanations for semantic changes.

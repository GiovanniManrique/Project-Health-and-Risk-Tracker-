# Project Health and Risk Tracker

A C# .NET 10 console application for viewing projects, tracking tasks and milestones, updating statuses, and checking project risks and health. It follows the requirements in [PRD.md](PRD.md).

## Run the application

Open `ProjectHealthTracker.slnx` in Visual Studio with .NET 10 support. Run the `ProjectHealthTracker` project with **Ctrl+F5**.

Or, from this repository's folder, run:

```powershell
dotnet run --project ProjectHealthTracker/ProjectHealthTracker.csproj
```

The program uses three sample projects stored in `List<Project>`. Changes remain available while the program runs and reset when it restarts. The core tracker requires no database or extra packages. Optional menu option 7 connects to your existing local Ollama server; option 8 reviews a risk with OpenAI.

## Menu

1. **List projects:** View IDs, managers, open risk counts, and current health.
2. **View project details:** Enter a project ID to see its dates and all tasks, milestones, and risks.
3. **Update item status:** Enter a project ID, an item ID shown for that project, and a status number. Enter `0` at the item or status prompt to cancel.
4. **Show project risks:** View open risks with probability, impact, owner, and mitigation plan. Closed risks remain visible in project details.
5. **Show health summary:** View each project's health, open risks, late milestones, and overall totals.
6. **Exit.**
7. **Explain a risk with local AI:** Choose a project, an open risk, and an installed local model. Show the priority score and ask the model for a short explanation and a next action.
8. **Review a risk with OpenAI:** See the project's metrics, select an open risk, and optionally describe the evidence and affected work. Preview the information before sending it to OpenAI for advice.

## OpenAI setup in Visual Studio

The cloud option uses an OpenAI API key, an API account with available usage, and an internet connection. It calls the Responses API with `gpt-4.1-mini`; the model name is visible in `Services/CloudAiService.cs`. It uses C#'s built-in `HttpClient` and JSON tools, with no additional NuGet packages.

1. Create an API key in your own [OpenAI project](https://platform.openai.com/api-keys). A restricted key needs Responses API write access. API billing is separate from a ChatGPT subscription. Never put the key in a `.cs` file or commit it to GitHub.
2. Save the key as `AI_API_KEY` in the ignored `.env.local` file next to `ProjectHealthTracker.slnx`. The secure setup can create this file for you. The program finds it automatically when run from Visual Studio or `dotnet run`; do not share this file or include it in a project submission. As an alternative, use **Edit environment variables for your account** in Windows to create a user variable named **AI_API_KEY**. Environment variables take priority over the local file; restart Visual Studio if it has inherited an older key. A published copy outside the solution folder uses environment variables.
3. Open `ProjectHealthTracker.slnx` in Visual Studio and press **Ctrl+F5**.
4. Choose **8**, project **2**, then risk **203**. Give a short evidence statement and affected-work statement, or press Enter to leave either unknown.
5. Review the displayed information. Enter **y** to send it to OpenAI. The application makes one request and waits up to one minute. It limits output to 500 tokens and requests that the response not be stored through the API's `store` option. Provider data policies still apply.

Only the selected risk, the selected project's computed metrics, and your two answers are sent. A key is sent in the authorization header, never in the prompt or console output. Without a key, the metrics and review still display and the program explains how to finish setup. Invalid keys, account/usage limits, connection problems, and incomplete responses have helpful messages. There are no automatic retries or background requests. Keys with an expiration must be replaced when they expire.

Official references: [API setup](https://developers.openai.com/api/docs/quickstart), [Responses text generation](https://developers.openai.com/api/docs/guides/text), and [the selected model](https://developers.openai.com/api/docs/models/gpt-4.1-mini).

### What the review measures

| Input or metric | Meaning |
| --- | --- |
| Likelihood and impact | Existing human/sample ratings, each from 1 to 5. AI does not invent or change them. |
| Priority score | Likelihood multiplied by impact. `4 x 5 = 20/25` is a priority score, not an 80% probability. |
| Open risks | Count of risks still marked Open. |
| High-impact open risks | Count of open risks whose impact is at least 4. |
| Late milestones | Unfinished milestones with a target date before today. |
| Completed tasks | Completed task count out of all tasks; this is not a percentage of effort. |
| Days to planned end | Planned end date minus today; negative means the planned date has passed. It does not prove the work is late. |
| Evidence and affected work | Two optional answers from the user. Blank answers remain unknown. |
| Owner and mitigation | The selected risk's existing owner and response plan. |

The AI is asked what needs attention, what evidence supports that conclusion, and one practical next action. It does not update objects or replace the C# project-health rules. The program does not track costs, staffing capacity, or dependencies.

The following is a teaching guide for the human ratings, not a calibrated measurement standard:

| Rating | Likelihood | Impact |
| --- | --- | --- |
| 1 | Little reason to expect it | Minor inconvenience |
| 2 | Possible, few warning signs | Small disruption, easy workaround |
| 3 | Some credible warning signs | Meaningful delay or extra work |
| 4 | Strong warning signs | Major disruption to an important deliverable |
| 5 | Expected based on current evidence | Main objective or deadline threatened |

This first version uses the ratings already stored in `Risk`. Editing them in the console is outside this feature; the two questions add context to the current review only. Nothing is saved after exit.

## Use your existing local model

1. Make your existing text model available in Ollama and keep Ollama running. `ollama list` should show it. This application does not download or import models.
2. Run the tracker and choose **7**, project **2**, then risk **203**.
3. Choose your model's number from the list shown by the application.
4. The program displays **4 x 5 = 20/25**, then the model's explanation. Local inference can take time; the request times out after two minutes.

The app sends requests only to `http://localhost:11434`. `localhost` means this computer. It lists models using `GET /api/tags` and asks for an explanation using `POST /api/generate`. It excludes models identified as cloud models. No API key is used. No available models, connection failures, and timeouts return helpful messages instead of ending the menu.

The priority score is `Probability * Impact` using the sample ratings of 1–5. It is not a percentage or a measured likelihood. The model supplies optional advice; it does not change project data or determine the existing OnTrack/AtRisk/OffTrack health result.

The connection has three parts:

- `Program.ExplainRiskWithAi` reads the user's choices and displays the result.
- `ProjectService.CalculateRiskScore` multiplies the two existing ratings.
- `AiRiskService.GetModels` and `ExplainRisk` communicate with Ollama. `HttpClient` sends the requests; JSON is the message format. `stream = false` asks for one complete answer. This console application deliberately waits for the answer before returning to its menu.

For a presentation: "I use an existing local model through Ollama. My code calculates a priority score and sends the risk details to the model. The model returns advice, which my program displays. The original project-health rules still run in C#."

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
| `Program.cs` | Runs the `while` menu loop, reads input with `TryParse`, and shares display methods for projects and items. A numbered status list handles updates. |
| `Models/ProjectItem.cs` | Abstract parent containing the shared ID, title, owner, status, and `GetDetails()` method. |
| `Models/ProjectTask.cs` | Inherits shared properties and adds a due date and completion flag. |
| `Models/Milestone.cs` | Inherits shared properties and adds a target date and achievement flag. |
| `Models/Risk.cs` | Inherits shared properties and adds probability, impact, and a mitigation plan. |
| `Models/Project.cs` | Holds project details and a `List<ProjectItem>`. |
| `Models/StatusTypes.cs` | Defines named item and health statuses with enums. |
| `Services/ProjectService.cs` | Finds items, supplies allowed statuses for the menu and validation, and reuses the open-risk list for counts and health. |
| `Services/AiRiskService.cs` | Lists existing local Ollama models and asks one to explain an open risk. |
| `Services/CloudAiService.cs` | Sends the previewed risk review to OpenAI and reads the returned text. |
| `Data/MockProjectData.cs` | Creates three sample projects with ordinary constructors and `List.Add`. |

Application files are inside the `ProjectHealthTracker` folder. Each begins with outline comments.

Inheritance lets tasks, milestones, and risks share the parent properties. Each child overrides `GetDetails()`, so one `foreach (ProjectItem item ...)` loop can display different item types. The service keeps its project-list field private and groups the business rules into methods. The application uses ordinary classes, constructors, lists, loops, enums, and methods; it does not need LINQ, async code, or a dependency injection framework.

## Build and verify

```powershell
dotnet build ProjectHealthTracker.slnx --configuration Release
dotnet run --project Tests/TrackerChecks.csproj --configuration Release
```

The optional checks are a separate console project with no testing packages. They test the health boundaries, status changes and reversals, every status-menu number, invalid IDs and statuses, menu flows, cancellation, input ending, project metrics, and both AI connections using simulated responses. No real API key or running model is needed, and automated checks spend no API credits. A failed check returns a nonzero exit code. The main solution includes only the application so its startup project stays straightforward.

## Explain the cloud feature to the class

1. **Program.cs — `ReviewRiskWithCloudAi`:** "This method reads the project and risk IDs. It asks two optional questions and shows the information before it is sent."
2. **ProjectService.cs — `GetProjectMetrics`:** "This method uses a foreach loop to count important risks and completed tasks. It reuses my existing methods for open risks, late milestones, and health."
3. **ProjectService.cs — `BuildRiskReview`:** "This method puts the selected risk, metrics, and my answers into one string. An unanswered question is labeled unknown."
4. **CloudAiService.cs — `ExplainRisk`:** "This class receives an HTTP client and the API key in its constructor. Its method sends a JSON request to OpenAI and returns the answer as text. The API key is only used to authenticate the request."
5. **Back in Program:** "Console.WriteLine displays the returned string. The model gives advice, while my original C# rules still calculate the project's health."

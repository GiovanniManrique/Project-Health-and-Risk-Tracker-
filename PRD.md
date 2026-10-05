# Project Health and Risk Tracker — simplified console requirements

## Purpose and scope

A beginner-readable C# .NET 10 console application for a fictional classroom demonstration. It monitors tasks, milestones, risks and overall health, and can ask the user's local Qwen 3.5 9B model through Bionic to explain one risk.

## Revised design

This revision intentionally replaces the original abstract `ProjectItem` and its three subclasses with one concrete `ProjectItem` and an `ItemType` enum. It removes `ProjectService`, the separate mock-data class, duplicate completion flags, and the Ollama/OpenAI/Codex provider implementations. No WinForms interface is included.

Three application classes:

- `Program`: menu, input validation, `CreateSampleProjects`, model preflight and AI request.
- `Project`: ID, name, manager, `List<ProjectItem>`, open-risk counting and health calculation.
- `ProjectItem`: ID, title, owner, immutable item type, validated status, optional due date, likelihood/impact ratings, fictional scenario and mitigation. Methods return details, allowed statuses and score, and validate status changes.

Three enums: `ItemType` (Task, Milestone, Risk), `ItemStatus` (NotStarted, InProgress, Completed, Blocked, Open, Closed), `HealthStatus` (OnTrack, AtRisk, OffTrack). Enums are not application classes. `Status` is privately set; constructor and later changes reject incompatible statuses. Completion is represented solely by Status.

## Menu and required behavior

1. List projects with health, manager, open-risk count and overall summary.
2. View the selected project's items and details, including risks.
3. Update an item's status using choices valid for that type.
4. Explain an open risk using local Qwen after showing the supplied data and obtaining `y` confirmation.
5. Exit.

Invalid menu choices, IDs, status numbers, cancellation and end-of-input must be handled without crashing or looping forever. Data is in memory; restarting recreates the three sample projects. No persistence, database, cloud credentials, model training or additional NuGet dependency is required.

## Data and health

Three projects, each containing one task, one milestone and one risk:

- Website: OnTrack, no open risk or overdue unfinished milestone.
- Inventory: OffTrack because risk 203 is open with likelihood 4 and impact 5.
- Training: AtRisk because milestone 302 is overdue and unfinished, with no open risk.

Dates are relative to the day the data is created. An open impact >=4 risk takes precedence over all other conditions and yields OffTrack. Otherwise any open risk or overdue unfinished milestone yields AtRisk; otherwise OnTrack. Due today is not overdue. Tasks alone do not change health. Priority score is Probability x Impact, not a probability percentage.

## Bionic local AI

The configured loopback server is `http://127.0.0.1:51500`; the exact installed model key is `qwen/qwen3.5-9b`. These are nonsecret constants in Program. Bionic's Local Model API uses the LM Studio runtime API. The application checks GET `/api/v1/models` and uses that model's loaded instance ID for POST `/api/v1/chat`. It issues no load/download commands and does not silently substitute another model or a stored answer.

The request contains the selected fictional project name, calculated health and risk details, including ratings, score, owner, scenario and mitigation. It asks for Risk, Why, and Next action in fewer than 120 words. This is an instruction to the model, not a guarantee of factual accuracy or length. AI never changes stored data or replaces C# calculations. Reasoning is off, chat storage and streaming are off, and no integrations/tools are enabled. Only final message content is displayed in the console.

Preflight has a five-second timeout; the app's HTTP client has a 120-second request timeout. Missing models, network/HTTP failures, timeouts and malformed or empty replies return a helpful message and the menu. The core tracker works without Bionic. Live verification requires an actually loaded local model; automated checks use simulated HTTP responses.

## Learning deliverable and completion

Provide an offline browser guide with exact source, line references, elementary vocabulary, section-by-section explanations, a status/health simulator, fixed file-order presentation cues, and practice with disclosed grading limits. Independent review should check technical accuracy and whether unexplained vocabulary remains; it cannot certify the learner's understanding without their own practice.

Completion requires a successful build, behavior checks, a real local answer when authorized, opening/building/launching in Visual Studio, updated documentation and a reviewed teaching guide. Git publication must exclude private settings, credentials and generated build files.

# Project Health and Risk Tracker — two-class console requirements

## Approved classroom scope

Keep the command-line app and local Bionic / Qwen integration. Use two authored application classes, `Program` and `Project`. Each Project holds exactly one task, one milestone, and one risk directly in its properties. Multiple items of each kind are deliberately outside this version's scope.

Remove ProjectItem, the item list, ItemType, item IDs and item selection. Keep project IDs, names, managers, task/milestone/risk titles and owners, optional task/milestone dates, validated statuses, likelihood/impact ratings, scenario, and mitigation plan. Keep ItemStatus and HealthStatus as enums. They are not classes. No custom inheritance, database, cloud credentials, extra NuGet package, or model training is needed.

## Structure and behavior

- Program: entry point, console menu, input validation, sample data, readiness check, and local AI request/response.
- Project: identity, direct fields as properties, constructor, allowed statuses, validated changes, risk score, health and details text.
- Status setters are private. `ChangeStatus(section, newStatus)` rejects incompatible values or invalid sections before mutation. The update menu maps 1 to Task, 2 to Milestone, and 3 to Risk.
- Task and milestone statuses: NotStarted, InProgress, Completed, Blocked. Risk statuses: Open, Closed. Initial defaults are NotStarted work and a Closed risk, changed explicitly for selected samples.
- Other fields remain simple public properties. Ratings must be 1–5 before AI submission; this is not validation on every assignment.
- Lists of Project objects remain. The app uses .NET classes and test helper classes in addition to its two authored application classes.

Menu: list projects/health, view details, update a status, explain a risk with local AI, exit. Invalid selections, cancellation and end-of-input must return safely or exit as appropriate. No changes persist after restart.

## Data and health

Three fictional samples start in distinct states: Website OnTrack; Inventory OffTrack because its risk is Open with likelihood 4 and impact 5; Training AtRisk because its unfinished milestone is three days overdue while its risk is Closed. Dates are relative to today.

Health uses direct checks in priority order: Open risk with impact >=4 → OffTrack; otherwise Open risk → AtRisk; otherwise overdue unfinished milestone with a date → AtRisk; otherwise OnTrack. Due today is not overdue. A task alone cannot cause a health warning. No item loop or needsAttention flag remains. Open-risk counts are zero or one per project.

Priority score = likelihood × impact. It is a classroom ranking, not a probability or percentage. It does not directly determine health.

## Bionic / Qwen

Use `http://127.0.0.1:51500` and model key `qwen/qwen3.5-9b` for this installation. Bionic's local API uses the LM Studio runtime. Use the loaded-instance ID from GET `/api/v1/models` for POST `/api/v1/chat` after the user selects an Open risk's project and confirms `y`. No separate item selection is needed.

Send the fictional project name, calculated health, and risk title/owner/status/ratings/score/scenario/plan. JSON includes instructions for a short Risk / Why / Next action answer, with reasoning off, stream false, store false and no integrations. Format/accuracy instructions are requests to the model, not guarantees.

The app sends no model-load/download commands and does not silently choose another model. A runtime state change between preflight and chat can still cause a reload. GET timeout is five seconds; the HTTP client request timeout is 120 seconds. Expected network/HTTP/timeout/JSON-shape/empty-answer failures return friendly text and the menu. No stored fallback answer is presented as live AI. Only final message content is printed; the response never modifies project data. The console waits for the operation before accepting another menu choice.

## Teaching and completion

One locally hosted visual aid must help a complete non-coder prepare in a suggested 20-minute first-rehearsal route, with deeper reference material available afterward. Serve the standalone page on loopback port 8765; keep direct-file use available. Show one code block at a time, exact source/line numbers, concrete before/after values, visible plain-English and technical explanations, reasons for the code choices, words to say aloud, clickable syntax, and explicit calls/returns. Begin with Main and trace a status update back to the menu and exit. Separate reference declarations from execution steps.

Give extra depth to the local AI integration: settings, .NET HttpClient, readiness GET, prompt, anonymous request properties, JSON serialization, StringContent, actual PostAsync call, await, response parsing, message extraction, return, WriteLine, and errors. Show the full prompt and request plus an explicitly illustrative response and printed result. Clearly distinguish a class, object, variable, property, method, enum, and API request. Prepare audience questions with plain and technical answers and likely follow-ups. Explain where each shortcut, console input, C# call, HTTP operation and shell command belongs, what it does, and whether it generates an AI answer.

Include a short presentation script, a repeatable demo that runs AI last, and multiple-choice/written practice. Written scoring is disclosed self-assessment. Below 70% repeats until at least 90%, with no claim that a score or reviewer proves the learner's understanding. The page itself makes no model requests.

Completion requires a successful build, focused behavior checks, a live local model test when available and authorized, independent code and teaching reviews with substantive findings addressed, source-aligned documentation/guide, and Git publication without private settings or generated build artifacts.

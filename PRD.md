# Project Health and Risk Tracker — two-class console requirements

## Approved classroom scope

Keep the command-line app. Remove the local-model API integration. Use two authored application classes, `Program` and `Project`. Each Project holds exactly one task, one milestone, and one risk directly in its properties. Multiple items of each kind are deliberately outside this version's scope.

Remove ProjectItem, the item list, ItemType, item IDs and item selection. Keep project IDs, names, managers, task/milestone/risk titles and owners, optional task/milestone dates, validated statuses, likelihood/impact ratings, scenario, and mitigation plan. Keep ItemStatus and HealthStatus as enums. They are not classes. No custom inheritance, database, cloud credentials, extra NuGet package, or model training is needed.

## Structure and behavior

- Program: entry point, console menu, input validation, and sample data.
- Project: identity, direct fields as properties, constructor, allowed statuses, validated changes, risk score, health and details text.
- Status setters are private. `ChangeStatus(section, newStatus)` rejects incompatible values or invalid sections before mutation. The update menu maps 1 to Task, 2 to Milestone, and 3 to Risk.
- Task and milestone statuses: NotStarted, InProgress, Completed, Blocked. Risk statuses: Open, Closed. Initial defaults are NotStarted work and a Closed risk, changed explicitly for selected samples.
- Other fields remain simple public properties. The samples use ratings from 1–5; public setters do not validate every assignment.
- Lists of Project objects remain. The app uses .NET classes and test helper classes in addition to its two authored application classes.

Menu: list projects/health, view details, update a status, exit (menu options 1–4). Invalid selections, cancellation and end-of-input must return safely or exit as appropriate. No changes persist after restart.

## Data and health

Three fictional samples start in distinct states: Website OnTrack; Inventory OffTrack because its risk is Open with likelihood 4 and impact 5; Training AtRisk because its unfinished milestone is three days overdue while its risk is Closed. Dates are relative to today.

Health uses direct checks in priority order: Open risk with impact >=4 → OffTrack; otherwise Open risk → AtRisk; otherwise overdue unfinished milestone with a date → AtRisk; otherwise OnTrack. Due today is not overdue. A task alone cannot cause a health warning. No item loop or needsAttention flag remains. Open-risk counts are zero or one per project.

Priority score = likelihood × impact. It is a classroom ranking, not a probability or percentage. It does not directly determine health.

## Offline operation and teaching

The application has no model endpoint, API key, HTTP client, or model request. Risk scores and health remain ordinary C# calculations. Do not present these rule-based results as generated AI advice.

Keep the locally hosted guide with full code blocks in execution order, plain and technical explanations, presentation notes, and multiple-choice/written practice. A score below 70% activates repeated practice until at least 90%. Written answers use disclosed self-assessment. Keep the server's allowlist so it only serves the teaching page and health endpoint.

Build the application, run focused offline behavior checks, regenerate the guide from current code, and publish to GitHub without local credentials or build artifacts.

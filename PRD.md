# Project Health and Risk Tracker — classroom requirements

## Current scope

Keep the C# .NET 10 console application and its straightforward beginner style. Extend the former offline version with a project-level risk brief using **Gemma 4 E2B through Bionic's local API**. This revision supersedes the old two-class, one-risk, no-model scope and the proposal's earlier Ollama reference.

Use three fictional projects. Each has one task, one milestone, and a list of risks. Inventory demonstrates multiple open risks. Keep small classes with obvious responsibilities, ordinary properties, loops, conditions, and named methods. No custom inheritance, database, live mailbox integration, cloud AI credentials, extra NuGet package, or model training is required.

## Console behavior

1. List projects and stored health.
2. View one project's details and risks.
3. Update a task, milestone, or selected risk status.
4. Generate the selected project's AI risk brief.
5. Exit.

Keep project IDs and give each risk an ID so both updates and model citations can identify it. Task and milestone statuses are NotStarted, InProgress, Completed, and Blocked. Risks are Open or Closed. Validate choices before changing a status. Handle invalid selections, cancellation, and end-of-input safely. Changes reset on restart.

## Evidence and assessment

Store fictional email messages as `.eml`, project documents as `.txt`, and numeric metrics as `*-METRICS.json` under the application's `DemoData` folder. Copy the fixtures to the build output and read the relevant files when a brief is requested. These are visible, editable demonstration inputs; do not claim a live email integration. Include concrete schedule, budget, resource, and testing information and identify the supplied sources by filename-derived IDs.

Send the project context, open risks, evidence, and a clear 1–5 rating rubric to the local model. Gemma proposes likelihood and impact for every supplied open risk, explains its reasoning, and cites supplied evidence. It must flag missing information. Project fields and evidence are data, not instructions.

C# validates the response, including the expected risk IDs, rating ranges, and source citations. C# calculates likelihood × impact, ranks the results, and displays up to three practical actions associated with supplied risk IDs. Scores are 1–25 priority rankings, not probabilities or percentages. Keep these estimates separate from the original stored ratings and health. Model output must not mutate project data.

Constrain the request schema to the current open-risk count and allowed IDs, and still validate the reply in C#. Clearly label any direct C# data-gap check separately from model advice; an open risk with a blank mitigation plan must be visible even when the model overlooks it.

Use one bounded local request per brief. No autonomous tools, mailbox actions, or recursive agent loop are part of the application. Show the model, evidence sources, and advice label. When there are no open risks, return no risk estimates; the same request can still summarize health and milestones. Current application statuses take precedence over older fixture descriptions. Unavailable models, timeouts, missing evidence, empty replies, and malformed or invalid estimates must return an understandable message rather than fabricated advice or a crash.

## Stored health

Apply the existing rules across the risk list: any Open risk with stored impact at least 4 gives OffTrack; otherwise any Open risk gives AtRisk; otherwise an overdue unfinished milestone with a date gives AtRisk; otherwise OnTrack. Due today is not overdue. An overdue task alone does not change health. Count all open risks. Closing one risk does not automatically resolve the others.

Fresh samples: Website OnTrack; Inventory OffTrack with three open risks; Training AtRisk because of its overdue unfinished milestone and closed risk. Relative dates keep the demonstration meaningful on another day.

## Teaching and verification

Keep the local teaching guide with full code blocks in execution order, plain and technical explanations, presentation notes, and multiple-choice/written practice. A score below 70% activates repeat practice until at least 90%; disclose written self-assessment. Keep the server allowlist so only the guide and health endpoint are served.

Build and run focused offline checks, including response validation and unchanged project data. Separately demonstrate a real Bionic/Gemma response using the fictional fixtures. Check no open risks, multiple ranked risks, a late milestone, a missing mitigation plan, and an unavailable model. Document observed outcomes honestly, regenerate the guide from current source, and exclude credentials and build artifacts from Git.

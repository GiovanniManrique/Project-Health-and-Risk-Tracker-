# Recorded mitigation plan correction — October 29, 2026

A live presentation run incorrectly said "Missing mitigation plan for INV-1" even though the scanner risk already stored "Use manual item numbers until the scanners arrive." The previous free-form model-owned missingInformation section allowed this contradiction. This revision replaces that output responsibility with an exact C# check of the current Risk.Plan; it does not depend on wording filters or another prompt-only correction.

## Current behavior

- **Recorded mitigation plans (C# from saved data)** displays each open risk's actual plan text.
- **Recorded data gaps (C# checks)** flags only an empty or whitespace-only plan. Fresh Inventory has no such gaps. The check establishes presence, not adequacy.
- Gemma returns only a summary and risk estimates with ratings, reasons, citations, and actions. Its contract has no missing-information or plan-status field.
- C# validates the exact required keys at the top level and inside each estimate, rejecting unknown or duplicate fields even when a server ignores the requested schema. The historical false claim in a legacy missingInformation field is rejected as an invalid response, without a retry or substitute result.
- One request per brief, real fixture ingestion, rating/citation validation, C# arithmetic and ordering, and immutable stored records remain in place.

## Verification on this revision

The Release build passed with **zero warnings and zero errors**. The offline runner passed **160 checks in both Debug and Release**. Regressions cover the exact false INV-1 claim, a genuinely empty plan, a whitespace-only plan, existing and closed-risk plans, legacy fields even when empty, contradictory extra root/risk fields, duplicate JSON keys, one request, and unchanged records.

Six real Bionic calls covered all five scenarios, including a repeated fresh Inventory run, using **google/gemma-4-e2b** at **127.0.0.1:1235/v1**:

| Scenario | Transcript | Elapsed | Observed result |
|---|---|---|---|
| Fresh Inventory, first run | [live-model-checks-1-20261029-001028.txt](../evaluation/live-model-checks-1-20261029-001028.txt) | 28.7 seconds | All three recorded plans displayed; no C# missing-plan flags; no false missing-plan narrative. |
| Fresh Inventory, second run | [live-model-checks-1-20261029-001100.txt](../evaluation/live-model-checks-1-20261029-001100.txt) | 21.0 seconds | Same correct recorded-plan result; the model explicitly recognized the manual-entry mitigation. |
| Deliberately blank INV-2 plan | [live-model-checks-5-20261029-001226.txt](../evaluation/live-model-checks-5-20261029-001226.txt) | 25.5 seconds | Only INV-2 was flagged by C#; the existing INV-1 and INV-3 plans remained visible. |
| Inventory after closing INV-1 | [live-model-checks-2-20261029-001413.txt](../evaluation/live-model-checks-2-20261029-001413.txt) | 33.8 seconds | Only INV-2 and INV-3 ranked, at 16 and 12; their plans shown; stored health remained AtRisk. |
| Website with no open risks | [live-model-checks-3-20261029-001452.txt](../evaluation/live-model-checks-3-20261029-001452.txt) | 10.4 seconds | OnTrack summary, empty ranked-risk list, no risk actions or plan gaps. |
| Training with overdue milestone | [live-model-checks-4-20261029-001515.txt](../evaluation/live-model-checks-4-20261029-001515.txt) | 10.6 seconds | AtRisk summary explicitly names overdue Manager review; empty risk list. The narrative imprecision noted below remains. |

Each call passed checks for one request, correct open risk IDs, valid ratings and citations, correct multiplication and descending order, at most three actions, exact recorded-plan/gap results, and unchanged stored data. Fresh Inventory and the deliberately missing-plan scenario returned priority scores of 20, 12, and 12 in these runs; these are observations, not fixed required model ratings. Narrative review found the Inventory supplier, budget, and staffing explanations grounded in the supplied fictional facts and no false missing-plan claim in the six outputs.

All five scenarios were run individually against this revision, with fresh Inventory repeated. The older five-scenario and console transcripts below remain evidence for their earlier revisions. The live helper now writes timestamped transcripts so repeated evaluations preserve prior results.

The teaching guide was regenerated with **15 blocks and all six current application C# files**, including ValidateFields and the authoritative recorded-plan output. All embedded source text and SHA-256 hashes match; block ranges are valid and no template markers remain. JavaScript syntax and git diff whitespace checks passed. The guide continues to separate model judgment from C# validation, arithmetic, ordering, and recorded-plan checks.

## Limits

The fix makes recorded-plan presence deterministic. It does not guarantee arbitrary AI summaries, reasons, or actions are factually perfect, and it does not judge whether a nonempty plan is sufficient. A valid source ID proves that a supplied source was named, not that every sentence follows from it. The user should compare AI judgments with the displayed saved plans and evidence. A 1–25 priority score remains an ordinal ranking, not a calibrated probability.

An observed example remains in the new Training transcript: its summary assigns the 60% actual / 80% planned progress to the training-guide task, while the source metrics describe overall project progress. Its overdue-milestone statement and C# recorded-plan checks are correct. This is a model narrative imprecision, not evidence that all semantic output is verified by the structural tests.

The sections below are historical and describe their own source revisions and observed runs.

---

# Historical local Gemma review — October 28, 2026

The current implementation supersedes the offline-only scope below. The application now has five small authored classes (`Program`, `Project`, `Risk`, `AiRiskService`, and `RiskEstimate`) and two enums. A Project still has one task and one milestone; its risk list supports several risks. The console's fourth option requests an AI brief and its fifth option exits.

Nine fictional local fixtures supply an email, notes, and metrics for each of three projects. `Program` builds the sample records; `AiRiskService` reads the selected project's listed files from the shared `DemoData` folder on each request. The service sends one local request, validates risk IDs, allowed source IDs, rating ranges, and response text, then calculates and ranks the separate estimates. It leaves stored project records and health unchanged.

## Current evidence

- Final Release build passed with **zero warnings and zero errors**. The offline runner passed **121 checks in Debug and Release**, covering stored rules, response handling, dynamic schema constraints, and C# detection of a blank mitigation plan without live model inference.
- A real console run used **Bionic at `127.0.0.1:1235/v1`**, with requested/returned model **`google/gemma-4-e2b`**. Inventory received scores of 20, 12, and 12 for `INV-1`, `INV-2`, and `INV-3` in that run. The reasons cited supplied supplier-delay, budget, and staffing evidence. These are observed examples, not fixed expected model ratings.
- After closing `INV-1` through the console, stored health became AtRisk. A second live brief ranked only `INV-2` and `INV-3`, with scores of 12 each in that run and a revised summary. The transcript is [evaluation/live-console-demo.txt](../evaluation/live-console-demo.txt).
- The full [five-scenario live run](../evaluation/live-model-checks.txt) passed structural checks: one request per scenario; correct open risk IDs; validated ratings and citations; correct C# multiplication and ordering; at most three risk actions; and unchanged stored data. Human review confirmed the multiple-risk, closed-risk, no-open-risk, and late-milestone cases. That run's missing-mitigation case did not explicitly flag its blank plan, despite passing structural checks.
- After adding an explicit MISSING marker to the snapshot and a separately labeled C# recorded-data-gap check, the [targeted scenario 5 rerun](../evaluation/live-model-checks-5.txt) succeeded in **49.4 seconds with one request**. The model explicitly reported “Missing mitigation plan for INV-2”; C# separately reported the same missing recorded field. Stored data was unchanged. The five scenarios are supported by the full run plus this targeted rerun, not by a claim that all five were rerun together on the final source.
- The guide was regenerated with **15 teaching blocks and all six current application C# files**. Embedded source text and SHA-256 hashes match the current files, block ranges are valid, and no template markers remain. JavaScript syntax and `git diff --check` passed.
- Browser checks exercised code-block navigation and the quiz: a 0% score activated remedial practice, and a later 100% score cleared it. The final page reload displayed the JsonNode schema block correctly with no browser console warnings or errors. Guide routes `/` and `/health` returned 200; `/README.md` and `/../ProjectHealthTracker/Program.cs` returned 404. The page makes no model requests.

The local request uses a JSON schema, a 120-second timeout, and direct response mode (`reasoning_effort: "none"`). A JsonNode copy of the schema constrains the result to the current number of open risks and allowed IDs; healthy projects with no open risks also require an empty missing-information list. Runtime validation remains necessary. The separate **Recorded data gaps (C# checks)** section reports blank mitigation plans directly from stored data, while model-generated missing-information notes are displayed separately.

Model explanations can still be incomplete or ask for additional details that overlap with supplied facts. Neither a schema nor valid source IDs proves factual reasoning. The manager must review judgments and citations. The 1–25 score is an ordinal priority ranking rather than a calibrated probability.

## Teaching changes

The guide now follows startup, sample creation, object definitions, the menu, status updates, evidence loading, prompt construction, the model request, response validation, ranking, and exit. It separates model judgment from C# arithmetic, shows the actual fixture filenames and IDs, explains how a closed risk is excluded, and retains the below-70% / until-90% quiz rule with disclosed written self-assessment. The guide server's route allowlist is unchanged. The page itself does not call the model.

Regenerate with `python presentation/build_guide.py` after C# changes, then review both the code blocks and their explanations. Historical reports below describe their own earlier revisions and are not evidence for the current model setup.

---

# Historical review — October 5, 2026

The application still has two authored classes, Program and Project, plus two enums in a third C# file. It has 15 methods and one Project constructor. Each Project holds one task, one milestone and one risk. No features or authored application classes were added or removed by this revision.

The code now separates conversion from range checks, names intermediate values, expands menu actions and sample assignments, and adds purpose comments. The AI reply uses an ordinary string. There are 422 nonblank, non-comment application lines, compared with 320 before this readability refactor. The larger line count is deliberate: previously packed operations are written as visible steps. Tests and teaching-page code are excluded.

## Current application verification

- .NET 10 Release build succeeded with zero warnings and zero errors.
- The runner passed 106 assertions without model inference. It covers default/sample values, valid and invalid status changes, health precedence, date boundaries and missing dates, cancellation, end-of-input, and the console demonstration.
- Simulated HTTP checks cover the exact loaded-model instance, local endpoint paths, request fields and prompt, unchanged project data, missing/unloaded models, invalid ratings, closed risks, empty/malformed responses, HTTP failures and timeouts.
- Added checks cover separate nonnumeric selection errors without changing project state, normalized cancellation, multiple message concatenation, an ignored non-message entry with no content property, and completeness of the separately named instructions string.
- An independent source/diff reviewer found no correctness blocker. It confirmed guarded null/date access, validation before mutation, preserved health rules and request field names, message-only extraction, confirmation, timeouts and error paths.
- The reviewer's final readability suggestions were applied: loadedModelId clearly distinguishes the returned identifier from the API field model, and the main switch puts calls and break/return on separate lines.

The only intended behavior change is more specific error wording for nonnumeric input. The local endpoint, model, health rules, status choices, API settings and return paths are preserved.

## Earlier live and IDE evidence

Before this refactor, the two-class application at commit 2d20950 passed 101 assertions, completed a real local Qwen request through the console, returned to its menu, and showed that AI had not changed project values. Visual Studio 2026 also built Debug and launched that version using Ctrl+F5. The guide labels its saved model reply as a previous test.

This revision was built and behavior-tested through the .NET tools. It did not repeat a live model request or an interactive Visual Studio run. The current simulated HTTP results and earlier live evidence are separate checks.

## Teaching review loop

The short path is designed for a 10–15-minute first study pass, with a separate roughly eight-minute presentation. It contains twelve steps, 872 visible words of plain/technical explanation and 77 primary source lines. Its timeline totals fourteen minutes. Design reasons, speaking cues and the other related excerpts are optional; Next advances directly to the next study step.

The independent reviewer identified and helped fix the following issues:

1. Listing and viewing details were promoted into the short path and presentation. The complete map covers every method and the constructor.
2. Object creation follows call, constructor and resumed initializer. The risk-rating annotation highlights the actual rating assignments.
3. The short path explicitly reopens Inventory after closing its risk, before selecting AI. Otherwise the same example run would correctly reject a closed risk.
4. Old StringBuilder/AppendLine, model-null and request-shorthand explanations were replaced with the current string accumulation and loadedModelId code.
5. The parsing question uses valid quoted JSON. The serializer explanation distinguishes producing JSON from validating the API's schema.
6. Required reading was reduced from 1,476 words to the visible 872-word core. Reasons, speaking cues and secondary excerpts remain available without being required stops.

The reviewer confirmed these fixes against the served page and current C# files. No substantive issue remained from the targeted review. This establishes source accuracy and suitable scope for a first pass; it does not certify the learner's understanding.

The 556-word speaking script covers data and startup (one minute), listing/details (one minute), status and health (ninety seconds), local AI (two and a half minutes), and demonstration/exit (two minutes). It explains each responsibility and the central statements rather than reading every source line aloud. AI is last; a clearly labelled recorded reply is available if the live response is slow.

## Current website verification

- All three embedded source snapshots and their hashes match the current C# files. The served page matches the generated HTML.
- Browser checks exercised all twelve study steps, all 22 selectable excerpts, direct Next navigation, three prediction questions, and the full 16-member code map.
- All 33 required code blocks across the 20 detailed chapters were checked against exact source text. All 142 selectable expression explanations and their line highlights were exercised. Three optional declaration blocks also remain available.
- All 28 audience answers and code-navigation links, nine command cards and seven AI data stages were checked. The displayed full request and printed example match the embedded teaching data. The builder derives the full instructions string from the current source and checks its ending.
- All seven sections fit the browser's 304-pixel-wide viewport without page-level horizontal overflow. No browser JavaScript errors were reported.
- The existing quiz scoring algorithm is unchanged. Earlier checks exercised 0%, 80%, 90% and exact 70%, including saved state and the below-70-until-90 rule. Current question wording was updated to the new code; written answers remain explicitly self-assessed.

The local server remains bound to 127.0.0.1:8765 and serves only the guide and its health endpoint. Its existing service marker is version 3; the embedded guide content is version 4. Bionic's model API uses the separate port 51500. The guide makes no model requests, executes no copied commands and sends no quiz answers anywhere.

After future C# edits, run `python presentation/build_guide.py` and review the explanations, ranges and script again. Source embedding cannot determine whether an explanation still means the right thing.

## 2026-10-09 — remove the local-model connection

This revision supersedes the model setup and AI teaching notes in the historical reviews above. Removed the local endpoint/model constants, HttpClient, async model methods, and AI menu option. Menu option 4 now exits. Cleared the unused local API-key entry without including the environment file in Git.

Kept the two application classes, status validation, C# risk score and health rules, sample data, and the user's existing comment edits. Updated the PRD, README, and chronological guide to match the offline app; the guide has ten blocks plus presentation notes, questions, and practice.

Validation: Release build passed with zero warnings/errors; 70 offline assertions passed, covering domain rules, the complete console demo, input validation, cancellation, end-of-input, and exit numbering. Guide rebuilt from current sources; all ten blocks rendered in the browser with no console errors. JavaScript syntax and a credential-pattern scan passed. Historical reviews below/above apply to their own earlier revisions, not an independent review of this removal.

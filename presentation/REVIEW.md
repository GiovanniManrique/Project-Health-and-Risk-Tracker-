# Readability refactor and teaching review — October 5, 2026

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

# Two-class application and deeper teaching guide review — October 5, 2026

The application now has two authored classes, Program and Project, and three C# source files. StatusTypes.cs holds two enums. There are 320 nonblank, non-comment application lines, compared with 355 in the preceding three-class version (`775d8f2`). Tests and the separate learning page are excluded from these counts.

The simplification removes ProjectItem, its objects, the nested item list, item IDs, ItemType, item selection, and the health loop. Each Project instead has one task, one milestone, and one risk. Repeated properties and the section numbers 1/2/3 are an intentional tradeoff for this fixed classroom scope; the model is not designed for several risks per project.

## Application evidence

The following application checks were completed for commit `2d20950` before the deeper guide revision. This guide revision changes no C# application files and makes no fresh model request.

- .NET 10 Release build succeeded with zero warnings and zero errors.
- The updated runner passed 101 assertions without running a model. Checks cover default and sample values, all valid statuses, incompatible status/section rejection without mutation, health precedence, date boundaries and missing dates, the new demo sequence, invalid input, cancellation and end-of-input.
- Simulated HTTP checks cover the loaded instance ID, exact local endpoint paths, JSON prompt/settings, extraction of final message content, unchanged domain data, missing/wrong/unloaded models, invalid ratings, closed risks, malformed/empty/unexpected responses, HTTP failures and timeouts.
- A separate live Release console run closed Inventory's risk, completed Training's milestone, showed all projects OnTrack, reopened Inventory's risk, and submitted it to the loaded local `qwen/qwen3.5-9b` through Bionic. A real answer appeared in the console. The menu returned and subsequent output confirmed that the AI had not changed project health or statuses. Its reply is labelled as a recorded test in the learning page.
- Visual Studio 2026 opened the current solution and built Debug successfully: one succeeded, zero failed. Ctrl+F5 launched the rebuilt application from its Debug/net10.0 directory. Detailed input flows and the live AI exchange were checked through the console process separately; the IDE check covered build and launch.
- The independent code reviewer found no blocking correctness issue in the inspected behavior. The review confirmed that only statuses have private setters; public likelihood/impact properties are validated before the AI request, not on every assignment. Expected requests use no cloud key and never assign model output to project fields.

## Teaching review loop

1. After the learner reported that the earlier guide was superficial, an independent reviewer identified the missing concrete transformations, complete request payload, intermediate types, command distinctions and prediction questions. The guide was rewritten around those gaps.
2. The reviewer compared the new lessons with the current source. The constructor walkthrough was corrected to show the call at Program.cs line 235, the Project constructor, then the resumed initializer and status changes. Other fixes distinguish a variable declaration from reuse, preserve literal `\n` in syntax labels, explain that the request follows the API schema, and distinguish receiving an HTTP error response from `EnsureSuccessStatusCode` throwing afterward.
3. The reviewer inspected the assembled page, full prompt/request/reply examples and local server. It independently confirmed that all three source snapshots match the app, the displayed data agrees with the C# code, examples are labelled, and the server serves the guide while rejecting tested repository paths. No blocking issue remained. Its final suggestions added a beginner orientation for C#/.NET/Visual Studio/Bionic and updated the README and PRD to match localhost hosting and the deeper content.

Every required block now has visible plain-English and technical explanations, the reason for the choice, concrete before/after values, speaking notes and selectable expression explanations. The main route begins at Main and follows one status change through calls and returns to the menu and exit. A separate AI route covers settings, HttpClient, readiness GET, prompt text, JSON serialization, StringContent, PostAsync, await, response parsing, return, WriteLine and expected failures. Reference declarations stay separate from the execution order.

The AI data display shows seven stages, including the complete Inventory prompt, full request JSON, an illustrative loaded instance ID, an illustrative response with an ignored non-message entry, and the precise text that the extraction code would print. The earlier real reply is separately labelled as a recorded test. The guide's port 8765 is distinguished from the model API's port 51500.

The question bank contains 28 audience questions with short answers, technical explanations, likely follow-ups and code links. Nine command examples state where each belongs, its effect and whether it generates a model response. Copy buttons copy text only. No command executes in the page.

The suggested first-rehearsal route is overview 0–2 minutes, status trace 2–5, AI 5–10, rehearsal 10–18, and practice 18–20. The 598-word script has an eight-minute timer and includes demonstration time. Reading all deeper material takes longer. AI is last in the demo, with a clearly labelled recorded reply available if the live request is slow. Neither the review nor a quiz score certifies the learner's understanding.

## Website evidence

Browser checks verified all three embedded source snapshots against the C# files; all 36 exact source blocks across 20 chapters; all 125 selectable annotations and line highlights; each required block's forward and reverse navigation; constructor order; optional declarations; six additional reference sections; all three whole-file views; 28 question/answer/code links; all seven AI data stages; and the eight-minute timer.

The quiz was exercised at 0%, 80%, then 90%. A below-70% result activated repeat practice; 80% preserved it; 90% cleared it. A subsequent exact-70% result did not activate the below-70% rule. Written scenarios rotated, the saved score state survived reload, and every round included a question about the actual AI call. Written responses use two explicit self-assessment criteria. Only multiple-choice answers are automatically graded. Written answers are neither stored nor sent anywhere.

All six sections were checked at a 390-pixel viewport without page-level horizontal overflow. Desktop and mobile screenshots were inspected. There were no JavaScript errors or requests outside the guide's own origin. The health endpoint returned version 3; tested private/source repository paths returned 404. The Python server binds only to 127.0.0.1 and reads the current generated page on each request. The PowerShell launcher starts it in the background or reuses the existing guide server.

After future C# edits, run `python presentation/build_guide.py` and review the explanations, block ranges, and presentation script again. Regeneration cannot detect a change in the meaning of the code.

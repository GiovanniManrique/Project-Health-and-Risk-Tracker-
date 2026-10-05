# Two-class verification and teaching review — October 5, 2026

The application now has two authored classes, Program and Project, and three C# source files. StatusTypes.cs holds two enums. There are 320 nonblank, non-comment application lines, compared with 355 in the preceding three-class version (`775d8f2`). Tests and the separate learning page are excluded from these counts.

The simplification removes ProjectItem, its objects, the nested item list, item IDs, ItemType, item selection, and the health loop. Each Project instead has one task, one milestone, and one risk. Repeated properties and the section numbers 1/2/3 are an intentional tradeoff for this fixed classroom scope; the model is not designed for several risks per project.

## Application evidence

- .NET 10 Release build succeeded with zero warnings and zero errors.
- The updated runner passed 101 assertions without running a model. Checks cover default and sample values, all valid statuses, incompatible status/section rejection without mutation, health precedence, date boundaries and missing dates, the new demo sequence, invalid input, cancellation and end-of-input.
- Simulated HTTP checks cover the loaded instance ID, exact local endpoint paths, JSON prompt/settings, extraction of final message content, unchanged domain data, missing/wrong/unloaded models, invalid ratings, closed risks, malformed/empty/unexpected responses, HTTP failures and timeouts.
- A separate live Release console run closed Inventory's risk, completed Training's milestone, showed all projects OnTrack, reopened Inventory's risk, and submitted it to the loaded local `qwen/qwen3.5-9b` through Bionic. A real answer appeared in the console. The menu returned and subsequent output confirmed that the AI had not changed project health or statuses. Its reply is labelled as a recorded test in the learning page.
- Visual Studio 2026 opened the current solution and built Debug successfully: one succeeded, zero failed. Ctrl+F5 launched the rebuilt application from its Debug/net10.0 directory. Detailed input flows and the live AI exchange were checked through the console process separately; the IDE check covered build and launch.
- The independent code reviewer found no blocking correctness issue in the inspected behavior. The review confirmed that only statuses have private setters; public likelihood/impact properties are validated before the AI request, not on every assignment. Expected requests use no cloud key and never assign model output to project fields.

## Teaching review loop

1. The reviewer compared all 20 new lessons with the current source. Corrections made the status and health calls appear before their called methods, added an explicit private-set property reference, distinguished a returned list from its object references, clarified StringBuilder's class/object roles, and explained the Contains/negation validation path. The readiness description acknowledges that server state can change between GET and POST.
2. Review of the assembled guide found that a Next button could skip hidden called methods. Required code blocks now have their own counter and are visited one at a time before Next advances to another lesson. Previous reverses this sequence. Enum/property/import declarations remain optional references. The AI settings screen explicitly provides setup context.
3. The overview now explicitly calls Program a class. The 20-minute schedule reserves seven minutes for the 561-word core talk and demo. The exact-70% quiz message was corrected. The reviewer verified these targeted fixes and found no substantive issue remaining in this review. This does not certify the learner's understanding.

The main route starts at Main and follows one concrete status change through calls and returns to the menu and exit. A separate AI route covers local settings, HttpClient, readiness GET, prompt text, JSON serialization, StringContent, the actual PostAsync call, await, JSON parsing, returned text, WriteLine and expected failures. The POST screen has eight selectable syntax explanations. Each step supplies plain language, example values, words to say, and optional deeper reasoning.

The suggested study route is overview 0–2 minutes, execution 2–6, AI 6–11, rehearsal 11–18, and practice 18–20. It is an ambitious first pass, not a promise of mastery. The demo places AI last and offers a clearly labelled recorded reply if the live request is slow.

## Website evidence

Headless Chrome checks verified all three embedded source snapshots against the C# files; the exact text of all 35 source blocks across 20 lessons; forward navigation through each required block before the next lesson; optional reference blocks; previous navigation; whole-file views; all eight POST syntax buttons; and the rehearsal timer.

The quiz was exercised at 0%, 80%, then 90%. A below-70% result activated repeat practice; 80% preserved it; 90% cleared it. Three different written prompts appeared, the saved score state survived reload, and every round included a question about the actual AI call. Written responses use two explicit self-assessment criteria. Only multiple-choice answers are automatically graded. Written answers are neither stored nor sent anywhere.

All five sections were checked at a 390-pixel viewport without page-level horizontal overflow. Desktop and mobile screenshots were inspected. There were no JavaScript errors or HTTP requests during the checks. This standalone page never runs the model.

After future C# edits, run `python presentation/build_guide.py` and review the explanations, block ranges, and presentation script again. Regeneration cannot detect a change in the meaning of the code.

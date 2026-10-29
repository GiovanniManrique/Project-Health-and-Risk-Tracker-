# AI Enhancement Proposal: Project Risk Brief

**Author:** Giovanni Manrique

**Project:** [Project Health and Risk Tracker](https://github.com/GiovanniManrique/Project-Health-and-Risk-Tracker-)

**Proposal due:** October 21 | **Presentation:** October 29

**Current design:** October 29, 2026

## Existing project and proposed enhancement

My C# .NET 10 console application tracks projects, tasks, milestones, risks, and project health. The baseline uses ordinary C# rules with fictional data. This enhancement adds multiple risks per project and a **project-level AI risk brief** through **Bionic running Gemma 4 E2B locally**. Bionic replaces the earlier proposal's Ollama choice.

A project manager selects a project and sees a ranked assessment based on fictional emails, project notes, and metrics. The model proposes likelihood and impact ratings from the evidence; C# validates those ratings, calculates each priority score, and orders the results. The brief explains urgent concerns and gives up to three practical next actions.

## AI integration design

- **Model:** `google/gemma-4-e2b`, served by Bionic's local model API. The model must be loaded and the API running. The application uses the local endpoint and does not require a paid cloud API or model training.
- **Evidence:** Read the selected project's listed `.eml`, `.txt`, and `*-METRICS.json` files from the shared `DemoData` folder on each brief. These are fictional local fixtures, not a live mailbox connection.
- **Prompt:** Supply project identity, stored health, open risk IDs, owners, scenarios, plans, dates, metrics, and source filenames. Give a 1–5 rubric and request one likelihood/impact estimate for each open risk, a short rationale with valid citations, a summary, and at most three displayed actions. Treat evidence as data and use only supplied facts.
- **C# responsibility:** Use `HttpClient` and JSON with no extra packages. Validate exact response fields, IDs, citations, and ratings; multiply likelihood × impact; rank the estimates. Display 1–25 priority scores without presenting them as calibrated probabilities. Show the actual recorded mitigation plans and detect blank plans directly from saved data; the model does not decide whether a plan exists.
- **Model boundary:** One bounded request per brief. The model generates advice and cannot modify records or call tools. AI estimates remain separate from stored ratings and health.
- **Console:** Menu option 4 creates the brief; option 5 exits. Show the model and sources. Handle no open risks, missing evidence, unavailable models, timeouts, and empty or invalid output clearly.

## Scope and effort

Keep the implementation suitable for a first-year C# project: a few small classes, explicit steps, simple loops and conditions, and the existing menu structure. No web application, database, real email synchronization, or external cloud service is needed.

Original planning estimate: **17 hours** — sample context and ranking (3), prompting and local service (4), console flow and errors (3), evaluation and fixes (4), documentation and demonstration preparation (3). This is a planning estimate, not a record of time spent.

## Validation and demonstration

Evaluate five scenarios: no open risks, multiple ranked risks, a late milestone, missing mitigation details, and an unavailable model. Check valid risk/source references and rating ranges, correct arithmetic and ordering, explanations grounded in the supplied facts, and unchanged project data after inference.

Demonstrate an Inventory brief, close one of its risks, and create another brief. The closed risk must disappear while the remaining open risks are still assessed. Show the fictional input files and distinguish model judgment from C# arithmetic. Keep offline checks and the observed live Bionic result separate in the review notes.

# Demonstrate the local AI risk brief

## Before class

Open Bionic, load `google/gemma-4-e2b`, and start Local Model API. The tracker defaults to `http://127.0.0.1:1235/v1`. Set `BIONIC_BASE_URL` and `BIONIC_MODEL` before launching the app if your local setup differs. A model in the Library may still need to be loaded.

Build and start a fresh tracker session:

```powershell
dotnet build ProjectHealthTracker.slnx -c Release
dotnet run --project ProjectHealthTracker/ProjectHealthTracker.csproj -c Release
```

The model is real; the project evidence is fictional. Local inference time varies, and the application allows up to 120 seconds for a request. A model error returns to the menu. Do not present a failed request or a saved earlier result as a successful current run.

## Main demonstration

Each arrow means press Enter after the preceding value.

| Input | What to show |
|---|---|
| `1` | Website OnTrack, Inventory OffTrack, and Training AtRisk. Inventory has three open risks. |
| `2 → 2` | Stored Inventory risks `INV-1`, `INV-2`, and `INV-3`; their base scores are 20, 9, and 9. |
| `4 → 2` | A fresh local AI brief with ratings, source citations, C# priority scores, and up to three actions. |
| `2 → 2` | The brief has not overwritten stored ratings or health. |
| `3 → 2 → 3 → INV-1 → 2` | Close the supplier risk. Inventory becomes AtRisk because two other risks remain open. |
| `4 → 2` | A second brief ranks only `INV-2` and `INV-3`. The fixture text is unchanged, but current statuses take precedence. |
| `5` | Exit. Restarting recreates the samples. |

Show these source files in the `ProjectHealthTracker/DemoData/` folder:

- `INV-EMAIL.eml`: the scanner shipment is six days late, with no confirmed replacement date.
- `INV-NOTES.txt`: meeting notes link the supplier, cost, and staffing issues to risk IDs.
- `INV-METRICS.json`: a $12,500 forecast against a $10,000 budget; 40% actual progress against 65% planned; one available tester out of three planned.

The model judges likelihood and impact from those facts. C# checks the returned IDs, citations, and rating ranges, multiplies the ratings, and orders the risks. Exact AI ratings and wording can vary. A result of 20/25 is a priority score, not an 80% chance of failure. A valid citation ID lets the manager find a source; it does not prove that every sentence of reasoning is correct.

Point out **Recorded mitigation plans (C# from saved data)**: `INV-1` already says "Use manual item numbers until the scanners arrive." C# displays this exact saved text and reports no missing plans for the fresh Inventory sample. The model estimates risk and proposes actions; it does not decide whether a recorded plan exists. A present plan can still be inadequate, so review the AI narrative and source evidence separately.

## Additional scenarios

| Scenario | How to exercise it | Expected behavior |
|---|---|---|
| No open risks | Fresh session: `4 → 1`. | Website receives no risk estimates or risk actions. A summary can still discuss its health and milestone. |
| Late milestone | Fresh session: `4 → 3`. | Training remains AtRisk; the brief mentions its overdue unfinished milestone without reopening the closed risk. |
| Multiple ranked risks | Fresh session: `4 → 2`. | Exactly the three open Inventory IDs appear, ordered by C#-calculated score. |
| Missing recorded mitigation plan | Run the live evaluation helper below. | The helper makes a separate temporary scenario with an empty `INV-2` plan. C# shows that plan as blank and flags only `INV-2` under Recorded data gaps. Existing `INV-1` and `INV-3` plans remain visible. |
| Unavailable model/server | Run the offline checks below. | A simulated failure must yield an understandable error instead of fabricated advice or changed data. |

## Verification commands

```powershell
dotnet run --project Tests/TrackerChecks.csproj -c Release
dotnet run --project Tests/TrackerChecks.csproj -c Release -- --live
dotnet run --project Tests/TrackerChecks.csproj -c Release -- --live 5
python presentation/build_guide.py
```

The first command runs offline behavior and response-validation checks, including rejection of the historical false claim "Missing mitigation plan for INV-1" in an unexpected model field. The second deliberately calls the local model and records all five scenarios in a timestamped `evaluation/live-model-checks-*.txt` file; it needs Bionic running and can take several requests. Adding a scenario number runs just that case and writes a separate timestamped transcript, such as `evaluation/live-model-checks-5-YYYYMMDD-HHMMSS.txt`. Each application brief still makes only one model request. The source fixtures and original project samples are not replaced by the helper's temporary scenario.

The observed console demo is recorded separately in `evaluation/live-console-demo.txt` when available. Review notes identify actual checks and distinguish current results from historical runs.

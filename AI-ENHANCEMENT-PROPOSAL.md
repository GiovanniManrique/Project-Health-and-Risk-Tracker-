# AI Enhancement Proposal: Project Risk Brief

**Author:** Giovanni Manrique  
**Project:** [Project Health and Risk Tracker](https://github.com/GiovanniManrique/Project-Health-and-Risk-Tracker-)  
**Proposal due:** October 21 | **Presentation:** October 29

## Existing Project and AI Integration

My C# .NET 10 console application tracks projects, tasks, milestones, risks, and project health. The local AI version already connects to Ollama, lets users select an installed model, and generates a short explanation and next action for an individual open risk. C# calculates the risk priority score as probability rating × impact rating; separate rules determine project health.

## Proposed Enhancement

I will extend this integration with a **project-level AI risk brief**. After selecting a project, users will receive its calculated health, a ranked list of open risks, and a concise explanation of the most urgent concerns and practical next steps. This will help users interpret the tracker’s data and decide where to focus attention.

## AI Integration Design

- **Model choice:** Reuse my existing local text model through Ollama. Keep model selection available and record the exact model used for the demonstration. Local inference keeps project data on my computer and avoids a paid cloud API.
- **Prompting:** Supply a structured snapshot containing calculated health, open risks, scores, owners, mitigation plans, and late milestones. Request a brief summary and up to three recommended actions linked to supplied item IDs. Require the model to use only these facts, flag missing information, and treat project fields as data rather than instructions.
- **Tools:** Extend `AiRiskService` using C# `HttpClient`, JSON, and the existing Ollama endpoints. Rank risks in C# by score and include that order in the prompt; the model explains the results.
- **Agents:** Use one bounded model request per brief. The application gathers context and displays advice; the model cannot modify records or run tools.
- **UI:** Add a console menu option for the brief, showing the selected model, source item IDs, and advice label. Handle unavailable models, timeouts, and empty responses with clear messages.

## Scope and Effort

Target **17 hours**: project snapshot and ranking (3), prompt and service extension (4), console flow and error handling (3), evaluation and fixes (4), documentation and demonstration preparation (3).

## Validation and Demonstration

Evaluate five scenarios: no open risks, multiple ranked risks, a late milestone, missing mitigation details, and an unavailable model. Check that cited IDs exist, advice reflects supplied facts, and AI output leaves project data and calculated health unchanged. Demonstrate a brief before and after closing a risk, then explain how the generated advice changes. Deliver the enhancement, evaluation notes, and a repeatable demonstration for October 29.

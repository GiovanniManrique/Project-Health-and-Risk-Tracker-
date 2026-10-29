"""Connect beginner explanations to complete blocks in the current C# source."""
import re


def build_chronology(sources):
    lines = {name: value["text"].splitlines() for name, value in sources.items()}
    program = "Program.cs"
    project = "Models/Project.cs"
    risk = "Models/Risk.cs"
    service = "AiRiskService.cs"
    estimate = "Models/RiskEstimate.cs"

    def find(file, text, start=1):
        return next(i for i, line in enumerate(lines[file], 1) if i >= start and text in line)

    def block(file, first, last, label):
        assert 1 <= first <= last <= len(lines[file]), (file, first, last)
        return dict(file=file, start=first, end=last, label=label)

    def whole(file):
        return block(file, 1, len(lines[file]), file + " · complete file")

    def method(file, name):
        first = next(i for i, line in enumerate(lines[file], 1)
                     if re.match(r"    (public|private) ", line) and re.search(r"\b" + name + r"\(", line))
        last = next(i for i in range(first + 1, len(lines[file]) + 1) if lines[file][i - 1] == "    }")
        return block(file, first, last, name + " · complete method")

    def constant(name):
        first = find(service, "private const string " + name)
        last = find(service, '""";', first)
        return block(service, first, last, name + " · complete text")

    def lesson(key, title, phase, blocks, plain, steps, technical, why, say, before, after, terms=()):
        return dict(key=key, title=title, phase=phase, blocks=blocks, simple=plain,
                    steps=steps, technical=technical, why=why, say=say,
                    before=before, after=after, terms=list(terms))

    out = []
    out.append(lesson("start", "Main starts and calls CreateSampleProjects", "Startup",
        [block(program, 1, find(program, "List<Project> projects = CreateSampleProjects();"), "Imports, entry point, and sample-data call")],
        "Main begins the application. It first asks a named method to create the three sample projects.",
        ["using makes model names available. namespace groups related types.",
         "The right side runs first: CreateSampleProjects builds a list, then Main stores its returned reference in projects.",
         "async Task allows Main to await the model request later. Startup itself does not ask the model anything."],
        ["Program has static methods, so Main does not need a Program object.",
         "List<Project> is a type. projects is a variable. CreateSampleProjects() is a method call."],
        "A separate setup method keeps example data out of the menu loop.",
        "Execution starts in Main. It creates the samples once, then reuses those same objects for each menu action.",
        "Application starts.", "CreateSampleProjects runs.",
        [("class", "A definition that groups data and operations."), ("method", "A named group of instructions."),
         ("async / await", "Allow a method to wait for an operation such as an HTTP request without blocking that thread.")]))

    out.append(lesson("samples", "Create the sample projects and their risk lists", "Startup · fictional data",
        [method(program, "CreateSampleProjects")],
        "This method makes Website, Inventory, and Training objects, assigns their facts, and returns one list.",
        ["new Project calls a constructor. The object initializer then assigns task, milestone, and evidence-file properties.",
         "Each new Risk is added to its project's Risks list. Inventory has three open risks with IDs INV-1, INV-2, and INV-3.",
         "ChangeStatus sets valid initial states. DateTime.Today.AddDays keeps dates relative to the day the demo runs."],
        ["Lists hold references to the objects. Selecting an object later does not create a separate copy.",
         "EvidenceFiles names real local fixtures. SourceIds names the permitted evidence identifiers for each risk, without file extensions.",
         "Website and Training have closed risks. Inventory's stored base scores are 20, 9, and 9; these are not AI estimates."],
        "A small reproducible dataset makes the different health outcomes and the risk ranking easy to demonstrate.",
        "These are fictional project records. Inventory combines a late supplier, a budget overrun forecast, and limited testing staff. Its evidence lives in separate files.",
        "Main is waiting for its sample list.", "The three Project objects are returned to Main."))

    ctor = method(project, "Project")
    out.append(lesson("objects", "Reference pause: Project, Risk, and the status enums", "Definitions used by startup",
        [block(project, 1, ctor["end"], "Project · properties and constructor"), whole(risk), whole("Models/StatusTypes.cs")],
        "Project groups one project's facts. Risk describes one entry in its risk list. Enums define the named status choices.",
        ["Project has one task and one milestone, plus List<Risk> so it can contain several risks.",
         "A constructor copies incoming identity values into a new object. Default statuses are NotStarted work and Closed risks.",
         "Private status setters make callers use validation methods. Risk.ChangeStatus allows only Open or Closed."],
        ["A class is a definition; each call to new creates an object. The project uses composition: a Project contains a list of Risk references.",
         "DateTime? allows a missing date. Public rating properties are simple stored values; the AI response is validated separately.",
         "Risk.CalculateScore multiplies stored likelihood by stored impact. This value does not become a probability."],
        "A separate Risk class represents multiple risks without a complicated inheritance hierarchy.",
        "Project and Risk are simple data classes. The statuses use named enum values, and guarded status methods keep incompatible values out.",
        "Sample creation calls constructors and status methods.", "Return to the sample method, then Main.",
        [("property", "A value belonging to an object."), ("constructor", "The setup method with the same name as its class."),
         ("private set", "Other classes can read this property but cannot directly assign it."), ("enum", "A type containing named choices.")]))

    out.append(lesson("connection", "Set up one local model client", "Startup · connection settings",
        [block(program, find(program, "using HttpClient client"), find(program, "string evidenceDirectory"), "Main · local client and evidence location"),
         block(service, 1, method(service, "AiRiskService")["end"], "AiRiskService · fields and constructor")],
        "Main prepares an HTTP client and the local address and model name. The brief action creates AiRiskService when needed; setup does not send a request.",
        ["BIONIC_BASE_URL and BIONIC_MODEL can override the visible defaults. The default URL includes /v1.",
         "The client has a 120-second timeout. using disposes it when Main finishes.",
         "The service accepts only a local loopback HTTP address. Evidence is read from DemoData beside the running application."],
        ["The constructor saves dependencies in private fields. this.client means the field, while client is the incoming parameter.",
         "HttpClient and JSON types come from .NET. No extra package or paid cloud AI API is needed.",
         "Bionic must have a usable loaded model and a running Local Model API for an actual brief."],
        "One reused client and visible configuration keep setup small and easy to explain.",
        "Bionic serves the local Gemma model. The tracker sends it a request only when the user selects an AI brief.",
        "Sample objects have returned to Main.", "The menu can now pass the client and settings to the brief action."))

    out.append(lesson("menu", "Repeat the menu and run only the chosen action", "Menu",
        [block(program, find(program, 'Console.WriteLine("PROJECT'), method(program, "Main")["end"], "Main · menu loop")],
        "The menu repeats until the user chooses 5 or input ends. Option 4 awaits an AI brief.",
        ["ReadLine returns text or null. Trim removes outside spaces before the switch checks the choice.",
         "Choices 1, 2, and 3 list, view, or update records. Choice 4 calls the asynchronous brief method.",
         "break leaves a switch case; return ends Main. Invalid choices print a message and the loop continues."],
        ["Only the selected case runs. The printed menu is not a sequence of automatic actions.",
         "await pauses this method until the chosen operation completes, then execution continues after the await."],
        "A single loop makes every action return to the same menu.",
        "The original console flow remains. The new fourth option asks for a risk brief; the fifth exits.",
        "The client and settings are ready.", "The program waits for a choice."))

    out.append(lesson("health", "Option 1: list projects and calculate stored health", "Demo · choice 1",
        [method(program, "ShowProjects"), method(project, "CalculateHealth")],
        "The list asks each Project for its health and counts its open risks. These values use stored C# rules.",
        ["foreach visits each project, then each risk in that project's list.",
         "Any open risk with stored impact at least 4 gives OffTrack. Other open risks give AtRisk.",
         "Without open risks, an overdue unfinished milestone gives AtRisk. Otherwise health is OnTrack."],
        ["hasOpenRisk remembers whether a previous iteration found an open risk. A later high-impact risk still takes precedence.",
         "HasValue checks an optional date before Value is read. Due today is not late.",
         "These rules do not call AI and do not use the proposed model ratings."],
        "Stored health stays repeatable even when the model is unavailable.",
        "Website starts OnTrack, Inventory OffTrack, and Training AtRisk. The model cannot silently replace those stored health rules.",
        "Enter 1.", "Health rows and counts print; the menu returns."))

    out.append(lesson("details", "Option 2: select the existing project and show details", "Demo · choice 2",
        [method(program, "ViewProject"), method(program, "SelectProject"), method(project, "GetDetails"), method(project, "GetRiskDetails")],
        "Selection returns an existing Project object. Its methods build text containing project facts and every stored risk.",
        ["TryParse checks the project ID. Zero or ended input cancels, and an unmatched ID returns null.",
         "The caller checks for null before reading the selected project.",
         "The detail text includes stored ratings, risk IDs, plans, and evidence IDs. C# displays that string."],
        ["Project? describes a result that may be absent. It is not an additional project class.",
         "String interpolation inserts values into text. The methods return text rather than reading keyboard input."],
        "Sharing one selection method avoids repeated input logic and keeps the same object accessible.",
        "Viewing Inventory shows the three original risks and their base scores. An AI brief will create separate estimates from evidence.",
        "Enter 2, then project 2.", "Inventory's stored details print."))

    out.append(lesson("update", "Option 3: validate and change one status", "Demo · choice 3",
        [method(program, "UpdateStatus"), method(project, "GetAllowedStatuses"), method(project, "ChangeStatus"), method(risk, "ChangeStatus")],
        "The update flow selects a project and section. For a risk it also asks for a risk ID, then applies a valid status.",
        ["Task and milestone choices come from Project.GetAllowedStatuses. Risk choices are Open or Closed.",
         "The displayed status number is one-based; subtracting one turns it into the list index.",
         "ChangeStatus validates before assignment. The resulting stored health is recalculated immediately."],
        ["Case-insensitive risk ID comparison permits inv-1 or INV-1. This still selects an existing Risk.",
         "Closing INV-1 leaves INV-2 and INV-3 open, so Inventory becomes AtRisk rather than OnTrack.",
         "The AI path reads these updated statuses and excludes closed risks from its assessment list."],
        "Explicit IDs prevent a status change from affecting the wrong risk.",
        "Close INV-1 using 3, 2, 3, INV-1, 2. The other risks remain open. A later brief must rank only those remaining risks.",
        "Choose the update menu.", "One valid status changes; then the menu returns."))

    out.append(lesson("request", "Option 4: select a project and await its brief", "AI · console entry",
        [method(program, "ShowAiRiskBriefAsync")],
        "This method gets a project, asks the service for a brief, and prints either the result or an understandable failure.",
        ["The same selection helper is reused. Cancellation returns without sending a request. The service is created inside try so a bad setting does not disable ordinary menu features.",
         "await waits for CreateBriefAsync to return a string.",
         "The catch blocks handle timeouts, request failures, file problems, and invalid responses so the menu can return."],
        ["An exception interrupts the normal path and goes to a matching catch block.",
         "A failed request does not display fabricated success or modify the recorded risk values."],
        "Keeping console interaction here separates it from HTTP and response parsing.",
        "The user chooses a project. The app reads its fictional evidence and requests one local model response, with a clear error if that fails.",
        "Enter 4, then project 2.", "CreateBriefAsync runs."))

    out.append(lesson("evidence", "Read the project's actual fictional evidence files", "AI · evidence loading",
        [method(service, "LoadEvidence")],
        "LoadEvidence reads the files named by the project and builds a lookup from source ID to text.",
        ["Each entry must be a filename inside DemoData. A missing or oversized file stops the request.",
         "File.ReadAllText reads the .eml, .txt, and .json contents. The JSON metrics are supplied as evidence text.",
         "Removing the filename extension creates IDs such as INV-EMAIL and INV-METRICS. Duplicate or empty evidence is rejected."],
        ["Dictionary<string, string> maps a unique source ID to its content.",
         "The files are re-read for each request. The .csproj copies source fixtures into the build output.",
         "This simulates ingestion without connecting to Gmail, Outlook, or any real mailbox."],
        "Real local fixtures make the input visible and editable instead of pretending that a hardcoded answer came from AI.",
        "The model's inputs include an email, meeting notes, and metrics. They are all labeled fictional and can be inspected before the brief.",
        "The service receives the chosen project and evidence folder.", "The evidence dictionary is ready."))

    out.append(lesson("prompt", "Build the snapshot and give the model a rating rubric", "AI · prompt",
        [method(service, "BuildSnapshot"), constant("Instructions")],
        "The snapshot packages current project data and source text. Separate instructions tell Gemma what to estimate and how to cite it.",
        ["The service supplies current health, dates, open risk IDs, owners, scenarios, plans, and allowed source IDs.",
         "The rubric defines likelihood and impact from 1 to 5 and asks for explanations grounded in the supplied facts.",
         "The snapshot marks a blank mitigation plan as MISSING and states whether the milestone is overdue. The model must treat evidence as data and leave score calculation to C#."],
        ["StringBuilder collects lines into one prompt string. It does not execute text from an email.",
         "The model should consider schedule, budget, staffing, and mitigation together. It is not trained or calibrated by this project.",
         "Current tracker statuses, dates, and mitigation override older fixture text. Closed risks are not assessment targets even if their names remain in those files."],
        "A shared rubric gives the estimates a meaning, while citations let a person check the reasoning.",
        "Gemma judges likelihood and impact using the evidence. C# will handle multiplication, ordering, and validation.",
        "Evidence has loaded and open risks have been collected.", "A complete text snapshot is ready."))

    out.append(lesson("http", "Send one request and parse the structured reply", "AI · local HTTP",
        [method(service, "CreateBriefAsync"), constant("ResponseSchema")],
        "CreateBriefAsync coordinates evidence, prompting, the local HTTP request, parsing, validation, and formatting.",
        ["Use JsonNode to edit a copy of the JSON schema: require exactly the current number of open risks and restrict IDs to that list. Zero open risks means an empty array.",
         "Post once to the local /chat/completions endpoint with a timeout. The response envelope contains the model's message.",
         "Parse the message as JSON. Reject an empty or truncated reply, then deserialize estimates and validate them."],
        ["JsonNode is an editable JSON tree. JsonSerializer writes the request; JsonDocument reads the response. None of these proves factual accuracy.",
         "The response schema asks for summary, missingInformation, and risks. Each risk includes its ID, ratings, reason, sources, and action.",
         "A healthy project with no open risks is constrained to empty missing-information notes too. Runtime checks still reject duplicates or invalid values.",
         "No tool loop or model training occurs. With no open risks, the model returns an empty risk list and can still summarize health and the milestone."],
        "A bounded request and an explicit result shape keep the integration understandable and failures visible.",
        "The app sends one local HTTP request. It accepts a structured answer only after checking its required content.",
        "The snapshot and instructions are ready.", "Parsed estimates and notes are ready for checks."))

    out.append(lesson("validate", "Keep AI estimates separate and validate them", "AI · response checks",
        [whole(estimate), method(service, "ReadText"), method(service, "ValidateEstimates")],
        "RiskEstimate holds proposed values, while Risk keeps stored values. Validation checks IDs, rating ranges, text, and permitted citations.",
        ["Every current open risk must appear exactly once. Unknown, closed, and duplicate risk IDs are rejected.",
         "Likelihood and impact must be integers from 1 to 5. A reason, an action, and valid source references are required.",
         "CalculateScore returns likelihood times impact. It does not divide by 25 or claim a failure probability."],
        ["Deserializing into a separate class prevents the service from overwriting stored Risk values by accident.",
         "Checking a citation ID proves it names an allowed source, not that every generated sentence is true. A project manager still reviews the explanation.",
         "A score such as 20/25 is a priority ranking, not an 80% chance of failure."],
        "Mechanical checks stop malformed advice from being presented as a valid assessment.",
        "The AI estimates are reviewed suggestions. We validate their shape and sources, calculate scores in C#, and preserve the original records.",
        "The reply has been parsed.", "Valid separate estimates can be ranked."))

    out.append(lesson("rank", "Rank the estimates and format the manager's brief", "AI · C# output",
        [method(service, "FormatBrief")],
        "C# sorts the estimates by calculated score and prints their reasons, sources, and up to three recommended actions.",
        ["The nested loops find the highest remaining score. A temporary variable swaps it into position.",
         "Equal scores use the risk ID as a stable tie-breaker.",
         "The text identifies the model, stored health, evidence IDs, scores, actions, and missing information. A separate C# checks section reports an open risk with no recorded mitigation plan."],
        ["This is selection sort. It is explicit and appropriate for a tiny classroom risk list.",
         "Only the separate estimate list is reordered. Project.Risks and its stored ratings are unchanged.",
         "The top three actions are linked to the highest-ranked supplied risk IDs. An empty risk list produces a clear no-open-risk message.",
         "Recorded data gaps are checked directly by C#, so a blank plan is visible even if the model overlooks it. Model-generated missing-information notes remain separately labeled and need review."],
        "The manager can see both the numeric ordering and the evidence behind the proposed priorities.",
        "The model supplies judgment. C# supplies exact arithmetic and ordering. The final brief is advice the manager can inspect, not an automatic change to the project.",
        "Validation has passed.", "The brief string returns to the console and is printed."))

    main = method(program, "Main")
    out.append(lesson("exit", "Return to the menu, or choose 5 to exit", "Finish",
        [block(program, find(program, 'case "4":', main["start"]), find(program, 'default:', main["start"]) - 1, "Main · AI return and exit")],
        "The awaited brief finishes, the menu repeats, and option 5 ends Main.",
        ["A returned brief or handled error resumes the menu loop.",
         "Choose another action to inspect the unchanged stored records or update a status.",
         "Choosing 5 exits. Restarting recreates the sample objects; the source fixture files remain."],
        ["A helper's return resumes its caller. Main's return completes the application.",
         "The order of method calls controls execution, not the order of files on disk."],
        "The same menu supports a before-and-after demonstration without restarting between assessments.",
        "After a brief, I can close a risk and generate another brief. That closed risk is excluded, while the other open risks remain. Five exits.",
        "The AI action returns.", "The next menu choice runs, or the application exits."))
    return out

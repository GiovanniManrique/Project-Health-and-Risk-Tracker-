"""Resolve whole teaching blocks against the current C# files."""
import re



def build_chronology(sources):
    lines = {name: value["text"].splitlines() for name, value in sources.items()}
    program = "Program.cs"
    project = "Models/Project.cs"

    def find(file, text, start=1):
        return next(i for i, line in enumerate(lines[file], 1) if i >= start and text in line)

    def block(file, first, last, label):
        assert 1 <= first <= last <= len(lines[file])
        return dict(file=file, start=first, end=last, label=label)

    def method(file, name):
        first = next(i for i, line in enumerate(lines[file], 1)
                     if re.match(r"    (public|private) ", line) and re.search(r"\b" + name + r"\(", line))
        last = next(i for i in range(first + 1, len(lines[file]) + 1) if lines[file][i - 1] == "    }")
        return block(file, first, last, name + " · complete method")

    def inside(name, start, end=None, label=None, end_before=None):
        whole = method(program, name)
        first = find(program, start, whole["start"]) if start else whole["start"]
        last = find(program, end, first) if end else whole["end"]
        if end_before:
            last = find(program, end_before, first) - 1
        return block(program, first, last, label or name)

    def lesson(key, title, phase, blocks, plain, steps, technical, why, say, before, after, terms=()):
        return dict(key=key, title=title, phase=phase, blocks=blocks, simple=plain,
                    steps=steps, technical=technical, why=why, say=say,
                    before=before, after=after, terms=list(terms))

    out = []
    out.append(lesson("start", "Main starts and asks for the sample projects", "Startup",
        [block(program, 1, find(program, "List<Project> projects = CreateSampleProjects();"), "Imports, settings, and the starting call")],
        "Main is the method where execution begins. At CreateSampleProjects(), Main pauses while that method builds the data.",
        ["using makes library and model names available. namespace groups the code; class Program contains the application's methods.",
         "Main creates the sample data once. Every menu action uses those same objects.",
         "The right side of the assignment runs first. Its returned list will be stored in projects."],
        ["List<Project> is a type: a list whose elements refer to Project objects. projects is a variable; CreateSampleProjects() is a method call.",
         "static means no Program instance is needed. void means Main returns no value to its caller."],
        "A named setup method keeps the menu separate from the example data.",
        "Execution starts in Main. It calls CreateSampleProjects, which builds the example objects and returns their list. Main waits for that method to finish before continuing.",
        "Application has just started.", "Control enters CreateSampleProjects.",
        [("class", "A definition that groups information and actions."), ("method", "A named set of instructions."),
         ("void", "The method does not return a value.")]))

    samples = method(program, "CreateSampleProjects")
    inventory = find(program, "Project inventory = new Project", samples["start"])
    training = find(program, "Project training = new Project", inventory)
    out.append(lesson("website", "Create the Website object — the full block", "Startup · sample data",
        [block(program, samples["start"], inventory - 1, "CreateSampleProjects · complete Website setup")],
        "This block makes one Project object for the Company Website Update, fills its information, and marks its task and milestone complete.",
        ["new Project passes 1, the project name, and Jordan Lee to the constructor. That setup routine runs first.",
         "The following { ... } is an object initializer: it assigns the task, milestone, and risk properties on this one object.",
         "website.ChangeStatus(1, ...) completes the task; section 2 completes the milestone. Its risk starts Closed by default."],
        ["Project is the variable's type. website is the variable referring to this particular object. Creating this object does not create a new class.",
         "DateTime.Today.AddDays(-5) gives a date five days before the current date. The unfinished-state rules matter too: a completed milestone does not count as late.",
         "While this block runs, new Project invokes the constructor; each ChangeStatus call invokes methods in Project. The next two stops inspect those called definitions."],
        "Keeping all the facts for one example in one block makes the setup easy to read and demonstrate.",
        "This whole block creates the Website project. The constructor sets its identity, the initializer fills the rest, and the last two calls mark the task and milestone complete.",
        "CreateSampleProjects is running.", "The Website object exists. The next stops explain the constructor and status calls used here.",
        [("object initializer", "The property assignments inside the braces after new Project(...)."),
         ("argument", "A concrete value passed into a call, such as 1 or Jordan Lee."),
         ("website", "A variable referring to the one Website object; it is not a class or a method.")]))

    ctor = method(project, "Project")
    out.append(lesson("project", "Inside new Project: its data and constructor", "Reference pause · called by new",
        [block(project, 1, ctor["end"], "Project · properties and complete constructor")],
        "Project defines the information each project object holds. The constructor puts the incoming ID, name, and manager into the new object.",
        ["Every object gets its own identity, task, milestone, and risk properties. In this simplified design there are no separate task or risk classes.",
         "The constructor is named Project, just like the class. Id = id copies an incoming value into the object's property.",
         "DateTime? permits a missing date. private set keeps direct status assignments inside Project."],
        ["Properties are values belonging to an object; constructor parameters exist for that call. Uppercase Name and lowercase name are different identifiers.",
         "Property initializers supply defaults such as NotStarted and Closed. The constructor body runs before the object initializer shown in the Website block.",
         "This is a definition lookup while explaining new Project, not an extra file that the program executes from top to bottom. Program and Project do not inherit from each other."],
        "One object keeps one project's related facts together; private setters direct status changes through validation.",
        "Project is our definition for one project's data and behavior. Each example is a separate object. The constructor supplies its identity, and its properties hold the task, milestone, and risk facts.",
        "new Project(...) has been called.", "The constructor finishes; the caller continues with the object's remaining property assignments.",
        [("property", "Information belonging to an object, such as Name."), ("get / set", "Read a property / assign a value."),
         ("constructor", "The setup routine run by new Project(...); it has the class name and no return type.")]))

    out.append(lesson("statuses", "Inside ChangeStatus: choices, checking, and assignment", "Reference pause · called during setup and updates",
        [block("Models/StatusTypes.cs", 1, len(lines["Models/StatusTypes.cs"]), "StatusTypes.cs · both enum definitions"),
         method(project, "GetAllowedStatuses"), method(project, "ChangeStatus")],
        "The enums define the status names. Project's methods decide which names are valid for a section and apply an allowed change.",
        ["ItemStatus supplies named choices. GetAllowedStatuses returns Open/Closed for risk, or four progress choices for task and milestone.",
         "ChangeStatus calls GetAllowedStatuses and checks Contains(newStatus). It returns false if the choice is invalid.",
         "Sections 1, 2, and 3 assign TaskStatus, MilestoneStatus, or RiskStatus. return true reports that the change succeeded."],
        ["StatusTypes.cs is a file containing two enum types, not a class called StatusTypes. HealthStatus names the overall health results.",
         "The assignment changes the existing object. The bool return only reports success. GetAllowedStatuses is reused by both the menu and this validation method.",
         "The enum itself does not enforce task-versus-risk rules; GetAllowedStatuses and ChangeStatus enforce those rules. An invalid section yields an empty list."],
        "The same validation works for sample setup and later user actions, with one place to maintain the rules.",
        "StatusTypes defines the names we use. GetAllowedStatuses limits the choices for each section, and ChangeStatus checks a choice before updating the same Project object.",
        "A caller supplies a section number and status.", "A permitted property changes and true returns; otherwise false returns.",
        [("enum", "A type containing named choices."), ("List<ItemStatus>", "A list of status values, not a method."),
         ("Contains", "Checks whether a value occurs in the list."), ("bool", "A true-or-false result.")]))

    out.append(lesson("remaining-samples", "Create Inventory and Training, then return the list", "Startup · finish sample data",
        [block(program, inventory, training - 1, "CreateSampleProjects · complete Inventory setup"),
         block(program, training, samples["end"], "CreateSampleProjects · Training setup and returned list")],
        "The same class creates two more objects with different facts. The final return gives Main one list containing all three objects.",
        ["Inventory has an open risk with likelihood 4 and impact 5, plus a future milestone.",
         "Training has a closed risk but an unfinished milestone dated three days ago.",
         "return new List<Project> { website, inventory, training } creates the list and returns it to the assignment waiting in Main."],
        ["The list stores references to the objects already created; it does not recreate their data. Selecting one later gives access to that same object.",
         "The sample dates are relative to DateTime.Today. Restarting recreates the objects and resets changes; there is no database or file persistence."],
        "The three samples demonstrate different health outcomes with a small, predictable dataset.",
        "Inventory and Training use the same Project class with different values. The method returns all three objects in one list, which Main uses for the rest of the session.",
        "The Website object has been created.", "Main's projects variable now refers to the returned list." ))

    out.append(lesson("menu", "Return to Main and repeat the menu", "Menu · automatic after startup",
        [inside("Main", 'Console.WriteLine("PROJECT', label="Main · complete menu loop")],
        "Main repeatedly reads a menu choice and calls the matching method.",
        ["while (true) repeats the menu until the method returns.",
         "ReadLine reads text, Trim removes outside spaces, and switch chooses the matching branch.",
         "break leaves the switch; return leaves Main. Option 4 or ended input ends the program."],
        ["Only the chosen branch runs. The demo chooses 1, 2, 3, then 4; that sequence is controlled by the user.",
         "string? allows a string or null. Empty text is a string with no characters; null means input ended."],
        "One menu loop coordinates the features without duplicating the menu after each action.",
        "Main reads a choice and calls the corresponding method. The menu repeats afterward. Four exits the application.",
        "The sample list has returned to Main.", "The program waits for a menu choice." ))

    out.append(lesson("health", "Option 1: list projects and calculate their health", "Demo choice · menu 1",
        [method(program, "ShowProjects"), method(project, "CalculateHealth")],
        "ShowProjects visits each project, asks it to calculate its health, prints a row, and counts the health categories.",
        ["foreach visits the existing Project objects. CalculateHealth runs separately for each object's current values.",
         "An open risk with impact at least 4 returns OffTrack; another open risk returns AtRisk. Otherwise a late unfinished milestone returns AtRisk; the remaining case is OnTrack.",
         "The category counters use ++ to add one. After all projects are visited, the summary prints."],
        ["CalculateHealth returns an enum value; health is a local variable holding that result. Health is calculated when called rather than stored as a property.",
         "HasValue checks that an optional date exists before Value is read. A date before today is late; an already completed milestone does not trigger the late rule.",
         "Likelihood affects the priority score, not this health rule. Task status is displayed but does not directly determine health in this version."],
        "Deterministic C# rules make the result repeatable and testable, without any external service.",
        "Listing calls CalculateHealth for each project. That method applies the risk and milestone rules in order and returns a named health result. The list prints those results and counts them.",
        "Menu choice 1.", "Website OnTrack; Inventory OffTrack; Training AtRisk. Return to the menu." ))

    out.append(lesson("details", "Option 2: select a project and show its details", "Demo choice · menu 2",
        [method(program, "ViewProject"), method(program, "SelectProject"),
         method(project, "GetDetails"), method(project, "GetRiskDetails"), method(project, "CalculateRiskScore")],
        "ViewProject asks for one project, gets its description, and prints it. The helper methods below show each job in that call chain.",
        ["SelectProject shows the list, reads an ID, and uses TryParse to check that the typed text is a number. It returns the matching existing object or null.",
         "GetDetails combines identity, task, milestone, health, and risk text. GetRiskDetails includes the scenario and plan.",
         "CalculateRiskScore returns likelihood × impact. Inventory's 4 × 5 is 20 out of 25, a priority ranking rather than a failure probability."],
        ["Project? means the result may be absent. The caller checks null before using the selected object.",
         "GetDetails and GetRiskDetails return strings; Console.WriteLine in Program displays them. The same risk text is reused later in the AI prompt.",
         "$ inserts property values into strings. The newline escape creates line breaks, and :d formats a date as a short date."],
        "Selection is shared by view, update, and AI features; description methods prepare text without reading keyboard input.",
        "Viewing details first finds the existing project by ID. Its methods build the description and risk score, and Program prints the returned text. The score is a ranking, not a percentage.",
        "Menu choice 2, then project ID 2.", "Inventory's information is printed. Return to the menu." ))

    out.append(lesson("update", "Option 3: change a status and see the new health", "Demo choice · menu 3",
        [method(program, "UpdateStatus")],
        "This entire method handles one update: select the project, choose its section, choose a valid status, apply it, and display the new health.",
        ["Select the project and read the section: 1 task, 2 milestone, 3 risk. TryParse and range checks reject invalid input.",
         "GetAllowedStatuses supplies the menu choices. The for loop numbers them from 1; the user types a choice.",
         "Subtract 1 to obtain a zero-based list index, call ChangeStatus, then recalculate and print health when the change succeeds."],
        ["Typing 2 at the risk-status prompt gives string 2 → integer 2 → index 1 → ItemStatus.Closed. That differs from the enum's underlying numeric value.",
         "ChangeStatus and its validation were shown in the setup reference block. This time the chosen value comes from the user instead of sample code.",
         "Closing Inventory's risk makes it OnTrack because its milestone is still in the future. To demonstrate another change, reopen the risk with menu 3 → project 2 → section 3 → status 1."],
        "The UI checks the typed input, while Project checks whether the requested status is valid for the section.",
        "This block collects and validates a status change. The menu number becomes a list index, ChangeStatus updates the existing object, and CalculateHealth immediately reflects its new values.",
        "Type 3 → 2 → 3 → 2 to close Inventory's risk.", "Inventory becomes OnTrack. Reopen it with 3 → 2 → 3 → 1 to see OffTrack again." ))

    main = method(program, "Main")
    out.append(lesson("exit", "Return to the menu, then choose 4 to exit", "Finish",
        [block(program, find(program, 'case "3":', main["start"]), find(program, 'default:', main["start"]) - 1, "Main · menu return and exit")],
        "When a menu action finishes, the menu returns. Choosing 4 ends the program.",
        ["UpdateStatus returns to Main when it finishes.",
         "break exits the switch so the while loop can repeat.",
         "return exits Main. All changes were in memory, so restarting recreates the samples."],
        ["A helper's return resumes its caller. Main's return completes this console application.",
         "Physical source-file order is different from execution order: method calls decide what runs next."],
        "An explicit exit lets the user end the repeated menu cleanly.",
        "The menu repeats after each action. Four returns from Main and ends the application. Changes reset on restart.",
        "A menu action has returned.", "Application ends." ))
    assert len(out) == 10
    return out

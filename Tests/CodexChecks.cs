// Purpose: Checks the Codex connection without using a subscription or making AI requests.
// Data: Uses this test executable as a small pretend Codex process.
// Methods: Checks success, missing program, empty answers, and unsuccessful process exits.

using System.Text;
using ProjectHealthTracker.Services;

namespace TrackerChecks;

public static class CodexChecks
{
    public static void Run(Action<bool, string> check)
    {
        string[] names = { "CODEX_EXE", "TRACKER_CODEX_TEST", "AI_API_KEY", "OPENAI_API_KEY", "CODEX_API_KEY", "CODEX_ACCESS_TOKEN" };
        string?[] saved = new string?[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            saved[i] = Environment.GetEnvironmentVariable(names[i]);
        }

        try
        {
            string executable = OperatingSystem.IsWindows() ? "TrackerChecks.exe" : "TrackerChecks";
            Environment.SetEnvironmentVariable("CODEX_EXE", Path.Combine(AppContext.BaseDirectory, executable));
            for (int i = 2; i < names.Length; i++)
            {
                Environment.SetEnvironmentVariable(names[i], "test-placeholder");
            }

            CodexAiService service = new CodexAiService();
            Environment.SetEnvironmentVariable("TRACKER_CODEX_TEST", "success");
            string result = service.ExplainRisk("Test risk \"quoted\": café delivery is uncertain.");
            check(result == "Ask the supplier for a delivery update.",
                "Codex reads the full UTF-8 review from stdin, uses ChatGPT settings, removes key variables, and drains large stderr output");
            check(service.ExplainRisk(" ").Contains("no risk information"), "Empty review does not launch Codex");

            Environment.SetEnvironmentVariable("TRACKER_CODEX_TEST", "empty");
            check(service.ExplainRisk("sample").Contains("no explanation"), "Empty Codex output has a helpful message");
            Environment.SetEnvironmentVariable("TRACKER_CODEX_TEST", "error");
            result = service.ExplainRisk("sample");
            check(result.Contains("could not complete") && !result.Contains("private diagnostic") && !result.Contains("unfinished answer"),
                "Failed Codex process does not display diagnostics or a partial answer");

            Environment.SetEnvironmentVariable("CODEX_EXE", Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".exe"));
            check(service.ExplainRisk("sample").Contains("Codex could not start"), "Missing Codex returns setup instructions");
        }
        finally
        {
            for (int i = 0; i < names.Length; i++)
            {
                Environment.SetEnvironmentVariable(names[i], saved[i]);
            }
        }
    }

    public static int FakeReply(string[] args)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = new UTF8Encoding(false);
        string input = Console.In.ReadToEnd();
        string? mode = Environment.GetEnvironmentVariable("TRACKER_CODEX_TEST");
        if (mode == "empty") return 0;
        if (mode == "error")
        {
            Console.Error.Write("private diagnostic");
            Console.Write("unfinished answer");
            return 1;
        }
        if (mode != "success") return 2;

        string[] credentials = { "AI_API_KEY", "OPENAI_API_KEY", "CODEX_API_KEY", "CODEX_ACCESS_TOKEN" };
        foreach (string name in credentials)
        {
            if (Environment.GetEnvironmentVariable(name) != null) return 3;
        }
        string arguments = string.Join(" ", args);
        if (!arguments.Contains("forced_login_method=chatgpt") || !arguments.Contains("--sandbox read-only") ||
            arguments.Contains("Test risk") || !input.Contains("Test risk \"quoted\": café") ||
            !input.Contains("not a probability or percentage")) return 4;

        Console.Error.Write(new string('x', 262144));
        Console.WriteLine("Ask the supplier for a delivery update.");
        return 0;
    }
}

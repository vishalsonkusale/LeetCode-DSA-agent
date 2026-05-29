namespace LeetCodeAgent;

public static class ConsoleRenderer
{
    public static void WriteWelcome(Database database)
    {
        WriteTitle("LeetCode DSA Assistant");
        WriteMuted($"Context DB: {database.FilePath}");
        WriteMuted("Model: qwen3:8b via Ollama at http://localhost:11434");
        Console.WriteLine();
        WriteCapabilities();
        Console.WriteLine();
        WriteSection("Recent context");
        Console.WriteLine(database.BuildStartupSummary());
        Console.WriteLine();
    }

    public static void WriteCapabilities()
    {
        WriteSection("Capabilities");
        Console.WriteLine("/solve <LeetCode URL or problem text>  Fetch problem links and explain all approaches");
        Console.WriteLine("/code <problem or idea>                Generate C# LeetCode solution code");
        Console.WriteLine("/debug <code or issue>                 Review correctness, edge cases, and complexity");
        Console.WriteLine("/explain <concept>                     Explain a DSA concept or algorithm");
        Console.WriteLine("/path <topic>                          Build a study/practice roadmap");
        Console.WriteLine("/history [count]                       Show saved recent context");
        Console.WriteLine("/forget                                Clear saved context");
        Console.WriteLine("/clear                                 Clear the console");
        Console.WriteLine("/cmd or /help                          Show this command list");
        Console.WriteLine("/exit                                  Quit");
    }

    public static void WriteAssistant(string response)
    {
        WriteAssistantHeader();
        Console.WriteLine(StripAssistantPrefix(response));
    }

    public static void WriteAssistantHeader()
    {
        WriteSection("Assistant");
    }

    public static void WriteAssistantChunk(string chunk)
    {
        Console.Write(chunk);
    }

    public static void FinishAssistantStream()
    {
        Console.WriteLine();
    }

    public static void WriteInfo(string message)
    {
        WithColor(ConsoleColor.Cyan, () => Console.WriteLine(message));
    }

    public static void WriteWarning(string message)
    {
        WithColor(ConsoleColor.Yellow, () => Console.WriteLine(message));
    }

    public static void WritePrompt()
    {
        WithColor(ConsoleColor.Green, () => Console.Write("You> "));
    }

    public static void WriteSection(string title)
    {
        WithColor(ConsoleColor.DarkCyan, () => Console.WriteLine($"== {title} =="));
    }

    private static void WriteTitle(string title)
    {
        WithColor(ConsoleColor.Cyan, () =>
        {
            Console.WriteLine(title);
            Console.WriteLine(new string('=', title.Length));
        });
    }

    private static void WriteMuted(string message)
    {
        WithColor(ConsoleColor.DarkGray, () => Console.WriteLine(message));
    }

    private static string StripAssistantPrefix(string response) =>
        response.StartsWith("Assistant:", StringComparison.OrdinalIgnoreCase)
            ? response["Assistant:".Length..].Trim()
            : response.Trim();

    private static void WithColor(ConsoleColor color, Action write)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        write();
        Console.ForegroundColor = previousColor;
    }
}

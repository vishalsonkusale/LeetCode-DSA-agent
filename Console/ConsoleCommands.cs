namespace LeetCodeAgent;

public sealed record ConsoleCommandResult(
    bool Handled,
    bool ShouldExit,
    string? AgentInput);

public static class ConsoleCommands
{
    public static ConsoleCommandResult Handle(string input, Database database)
    {
        if (!input.StartsWith('/'))
        {
            return new ConsoleCommandResult(Handled: false, ShouldExit: false, AgentInput: input);
        }

        var trimmed = input.Trim();
        var commandEnd = trimmed.IndexOf(' ');
        var command = commandEnd < 0 ? trimmed[1..] : trimmed[1..commandEnd];
        var argument = commandEnd < 0 ? string.Empty : trimmed[(commandEnd + 1)..].Trim();

        return command.ToLowerInvariant() switch
        {
            "cmd" or "help" or "capabilities" => ShowCapabilities(),
            "history" => ShowHistory(database, argument),
            "forget" => Forget(database),
            "clear" => Clear(database),
            "exit" or "quit" or "q" => new ConsoleCommandResult(true, true, null),
            "solve" => BuildAgentCommand(argument, "Solve this LeetCode or DSA problem. If this is a LeetCode URL, fetch the problem statement first:"),
            "code" => BuildAgentCommand(argument, "Generate a complete C# LeetCode-style solution for:"),
            "debug" => BuildAgentCommand(argument, "Review and debug this DSA code or issue:"),
            "explain" => BuildAgentCommand(argument, "Explain this DSA concept or algorithm:"),
            "path" => BuildAgentCommand(argument, "Create a DSA learning path for:"),
            _ => Unknown(command)
        };
    }

    private static ConsoleCommandResult ShowCapabilities()
    {
        ConsoleRenderer.WriteCapabilities();
        return new ConsoleCommandResult(true, false, null);
    }

    private static ConsoleCommandResult ShowHistory(Database database, string argument)
    {
        var count = int.TryParse(argument, out var parsedCount) ? parsedCount : 5;
        var turns = database.RecentTurns(count);

        ConsoleRenderer.WriteSection($"History ({turns.Count})");
        if (turns.Count == 0)
        {
            Console.WriteLine("No saved context yet.");
        }
        else
        {
            foreach (var turn in turns)
            {
                Console.WriteLine($"{turn.Timestamp.LocalDateTime:g} [{turn.Intent}] {turn.UserInput}");
            }
        }

        return new ConsoleCommandResult(true, false, null);
    }

    private static ConsoleCommandResult Forget(Database database)
    {
        database.Clear();
        ConsoleRenderer.WriteInfo("Saved context cleared.");
        return new ConsoleCommandResult(true, false, null);
    }

    private static ConsoleCommandResult Clear(Database database)
    {
        Console.Clear();
        ConsoleRenderer.WriteWelcome(database);
        return new ConsoleCommandResult(true, false, null);
    }

    private static ConsoleCommandResult BuildAgentCommand(string argument, string instruction)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            ConsoleRenderer.WriteWarning("Please provide text after the command. Example: /solve https://leetcode.com/problems/two-sum/");
            return new ConsoleCommandResult(true, false, null);
        }

        return new ConsoleCommandResult(true, false, $"{instruction}{Environment.NewLine}{argument}");
    }

    private static ConsoleCommandResult Unknown(string command)
    {
        ConsoleRenderer.WriteWarning($"Unknown command: /{command}");
        ConsoleRenderer.WriteCapabilities();
        return new ConsoleCommandResult(true, false, null);
    }
}

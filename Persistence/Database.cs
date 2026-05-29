using System.Text.Json;

namespace LeetCodeAgent;

public sealed class Database
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    private readonly string filePath;
    private readonly List<ConversationTurn> turns;

    public Database(string? filePath = null)
    {
        this.filePath = filePath ?? Path.Combine(Environment.CurrentDirectory, "Data", "context-log.jsonl");
        turns = LoadTurns();
    }

    public string FilePath => filePath;

    public IReadOnlyList<ConversationTurn> Turns => turns;

    public IReadOnlyList<ConversationTurn> RecentTurns(int count) =>
        turns.TakeLast(Math.Max(0, count)).ToList();

    public void Record(ConversationTurn turn)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.AppendAllText(filePath, JsonSerializer.Serialize(turn, JsonOptions) + Environment.NewLine);
        turns.Add(turn);
    }

    public void Clear()
    {
        turns.Clear();

        if (File.Exists(filePath))
        {
            File.WriteAllText(filePath, string.Empty);
        }
    }

    public string BuildStartupSummary(int count = 3)
    {
        var recentTurns = RecentTurns(count);
        if (recentTurns.Count == 0)
        {
            return "No previous context found.";
        }

        return string.Join(Environment.NewLine, recentTurns.Select(turn =>
            $"{turn.Timestamp.LocalDateTime:g}: {turn.Subject} | {turn.UserInput}"));
    }

    private List<ConversationTurn> LoadTurns()
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        var loadedTurns = new List<ConversationTurn>();

        foreach (var line in File.ReadLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var turn = JsonSerializer.Deserialize<ConversationTurn>(line, JsonOptions);
                if (turn is not null)
                {
                    loadedTurns.Add(turn);
                }
            }
            catch (JsonException)
            {
                // Ignore malformed history lines so one bad edit does not break startup.
            }
        }

        return loadedTurns;
    }
}

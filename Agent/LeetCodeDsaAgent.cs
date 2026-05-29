namespace LeetCodeAgent;

public sealed class LeetCodeDsaAgent
{
    private readonly ProblemSolver solver;
    private readonly DSAAssistant assistant;
    private readonly CodeGenerator generator;
    private readonly LearningPath learningPath;
    private readonly CodeDebugger debugger;
    private readonly Database database;
    private readonly IChatClient chatClient;
    private readonly IProblemClient problemClient;

    public LeetCodeDsaAgent(
        ProblemSolver solver,
        DSAAssistant assistant,
        CodeGenerator generator,
        LearningPath learningPath,
        CodeDebugger debugger,
        Database database,
        IChatClient chatClient,
        IProblemClient problemClient)
    {
        this.solver = solver;
        this.assistant = assistant;
        this.generator = generator;
        this.learningPath = learningPath;
        this.debugger = debugger;
        this.database = database;
        this.chatClient = chatClient;
        this.problemClient = problemClient;
    }

    public AgentRequest Parse(string input)
    {
        var intent = DetectIntent(input);
        return new AgentRequest(intent, input, ExtractSubject(input));
    }

    public async Task<string> RespondAsync(string input, CancellationToken cancellationToken = default)
    {
        var request = Parse(input);
        var problem = await problemClient.TryGetProblemAsync(input, cancellationToken);
        var messages = BuildMessages(request, problem);

        string response;
        try
        {
            response = await chatClient.GetResponseAsync(messages, cancellationToken);
            if (string.IsNullOrWhiteSpace(response))
            {
                response = "I did not receive a response from the local Ollama model.";
            }
        }
        catch (HttpRequestException exception)
        {
            response = $"Could not reach Ollama. Make sure Ollama is running at http://localhost:11434 and qwen3:8b is pulled. Details: {exception.Message}";
        }
        catch (TaskCanceledException)
        {
            response = "The Ollama request timed out.";
        }

        var formattedResponse = $"Assistant:{Environment.NewLine}{response.Trim()}";

        database.Record(new ConversationTurn(
            DateTimeOffset.Now,
            input,
            formattedResponse,
            request.Intent,
            request.Subject));

        return formattedResponse;
    }

    private List<ChatMessage> BuildMessages(AgentRequest request, LeetCodeProblem? problem)
    {
        var messages = new List<ChatMessage>
        {
            new(
                "system",
                """
                You are a local LeetCode DSA assistant powered by Ollama.
                Be practical, concise, and accurate.
                Prefer C# for code unless the user requests another language.
                Use previous context when the user refers to earlier discussion.
                Do not invent execution results.
                """)
        };

        foreach (var turn in database.RecentTurns(8))
        {
            messages.Add(new ChatMessage("user", turn.UserInput));
            messages.Add(new ChatMessage("assistant", StripAssistantPrefix(turn.AssistantResponse)));
        }

        messages.Add(new ChatMessage("user", BuildPrompt(request, problem)));
        return messages;
    }

    private string BuildPrompt(AgentRequest request, LeetCodeProblem? problem) =>
        request.Intent switch
        {
            AgentIntent.SolveProblem => solver.BuildPrompt(request.OriginalInput, problem),
            AgentIntent.GenerateCode => generator.BuildPrompt(request.OriginalInput),
            AgentIntent.RecommendPath => learningPath.BuildPrompt(request.OriginalInput),
            AgentIntent.ReviewCode => debugger.BuildPrompt(request.OriginalInput),
            AgentIntent.ExplainConcept => assistant.BuildPrompt(request.OriginalInput),
            _ when problem is not null => solver.BuildPrompt(request.OriginalInput, problem),
            _ => assistant.BuildPrompt(request.OriginalInput)
        };

    private static AgentIntent DetectIntent(string input)
    {
        if (Utilities.ExtractLeetCodeSlug(input) is not null)
        {
            return AgentIntent.SolveProblem;
        }

        if (Utilities.ContainsAny(input, "debug", "review", "optimize", "error", "bug", "why fails", "fix code"))
        {
            return AgentIntent.ReviewCode;
        }

        if (Utilities.ContainsAny(input, "code", "generate", "template", "solution class", "implement"))
        {
            return AgentIntent.GenerateCode;
        }

        if (Utilities.ContainsAny(input, "learn", "path", "recommend", "related", "practice", "topics", "roadmap"))
        {
            return AgentIntent.RecommendPath;
        }

        if (Utilities.ContainsAny(input, "explain", "what is", "algorithm", "concept", "intuition"))
        {
            return AgentIntent.ExplainConcept;
        }

        if (Utilities.ContainsAny(input, "solve", "leetcode", "problem") ||
            Utilities.ExtractProblemId(input) is not null)
        {
            return AgentIntent.SolveProblem;
        }

        return AgentIntent.General;
    }

    private static string ExtractSubject(string input)
    {
        var trimmed = input.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? "General DSA question" : trimmed;
    }

    private static string StripAssistantPrefix(string response) =>
        response.StartsWith("Assistant:", StringComparison.OrdinalIgnoreCase)
            ? response["Assistant:".Length..].Trim()
            : response;
}

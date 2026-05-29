namespace LeetCodeAgent;

public enum AgentIntent
{
    SolveProblem,
    ExplainConcept,
    GenerateCode,
    RecommendPath,
    ReviewCode,
    General
}

public sealed record AgentRequest(
    AgentIntent Intent,
    string OriginalInput,
    string Subject);

public sealed record LeetCodeProblem(
    string Title,
    string Slug,
    string Difficulty,
    IReadOnlyList<string> Topics,
    string Statement,
    IReadOnlyList<string> Hints,
    string? CSharpStarterCode);

public sealed record ConversationTurn(
    DateTimeOffset Timestamp,
    string UserInput,
    string AssistantResponse,
    AgentIntent Intent,
    string Subject);

public sealed record ChatMessage(string Role, string Content);

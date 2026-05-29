namespace LeetCodeAgent;

public sealed class DSAAssistant
{
    public string BuildPrompt(string userInput) =>
        $"""
        Explain the DSA concept, algorithm, pattern, or problem mentioned below.

        User request:
        {userInput}

        Keep the explanation generic and reusable. Include intuition, when to use it,
        common pitfalls, complexity, and a small C# example only if it helps.
        """;
}

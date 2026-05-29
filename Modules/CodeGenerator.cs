namespace LeetCodeAgent;

public sealed class CodeGenerator
{
    public string BuildPrompt(string userInput) =>
        $"""
        Generate a clean C# LeetCode-style solution for this request.

        User request:
        {userInput}

        Requirements:
        - Do not rely on hard-coded templates.
        - Infer the algorithm from the prompt.
        - Return the approach, complexity, and complete C# code.
        - Mention assumptions if the problem statement is incomplete.
        """;
}

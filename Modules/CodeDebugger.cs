namespace LeetCodeAgent;

public sealed class CodeDebugger
{
    public string BuildPrompt(string userInput) =>
        $"""
        Review and debug the following DSA or LeetCode code/request.

        User request:
        {userInput}

        Respond with correctness issues, edge cases, complexity problems,
        suggested fixes, and corrected C# code if appropriate.
        """;
}

namespace LeetCodeAgent;

public sealed class ProblemSolver
{
    public string BuildPrompt(string userInput, LeetCodeProblem? problem = null) =>
        problem is null ? BuildPromptFromUserInput(userInput) : BuildPromptFromProblem(userInput, problem);

    private static string BuildPromptFromUserInput(string userInput) =>
        $"""
        Solve this LeetCode or DSA problem request.

        User request:
        {userInput}

        Respond with:
        1. Problem understanding
        2. Key observations
        3. Algorithm
        4. Correctness intuition
        5. Time and space complexity
        6. C# solution when enough information is available
        """;

    private static string BuildPromptFromProblem(string userInput, LeetCodeProblem problem) =>
        $"""
        Solve this LeetCode problem from the fetched problem statement.

        User request:
        {userInput}

        Problem:
        {problem.Title}

        Slug:
        {problem.Slug}

        Difficulty:
        {problem.Difficulty}

        Topics:
        {string.Join(", ", problem.Topics)}

        Statement:
        {problem.Statement}

        Hints:
        {FormatList(problem.Hints)}

        C# starter code:
        {problem.CSharpStarterCode ?? "No C# starter code provided by LeetCode."}

        Provide:
        1. How to think about the problem from first principles
        2. Pattern recognition cues
        3. Brute force approach
        4. Better intermediate approaches if any
        5. Optimized approach
        6. Correctness intuition
        7. Complexity for each approach
        8. Complete C# solution for the optimized approach
        9. Edge cases and dry run
        """;

    private static string FormatList(IReadOnlyList<string> items) =>
        items.Count == 0 ? "No hints provided." : string.Join(Environment.NewLine, items.Select(item => $"- {item}"));
}

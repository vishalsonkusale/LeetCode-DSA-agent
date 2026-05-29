namespace LeetCodeAgent;

public sealed class LearningPath
{
    public string BuildPrompt(string userInput) =>
        $"""
        Create a DSA learning path for this request.

        User request:
        {userInput}

        Include prerequisite concepts, a suggested study order, practice problem types,
        and how to know when the topic is mastered.
        """;
}

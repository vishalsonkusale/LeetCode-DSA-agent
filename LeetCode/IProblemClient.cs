namespace LeetCodeAgent;

public interface IProblemClient
{
    Task<LeetCodeProblem?> TryGetProblemAsync(
        string userInput,
        CancellationToken cancellationToken = default);
}

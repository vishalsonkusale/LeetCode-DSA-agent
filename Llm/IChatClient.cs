namespace LeetCodeAgent;

public interface IChatClient
{
    Task<string> GetResponseAsync(
        IReadOnlyList<ChatMessage> messages,
        CancellationToken cancellationToken = default);
}

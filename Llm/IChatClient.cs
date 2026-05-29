namespace LeetCodeAgent;

public interface IChatClient
{
    IAsyncEnumerable<string> StreamResponseAsync(
        IReadOnlyList<ChatMessage> messages,
        CancellationToken cancellationToken = default);

    async Task<string> GetResponseAsync(
        IReadOnlyList<ChatMessage> messages,
        CancellationToken cancellationToken = default)
    {
        var response = new System.Text.StringBuilder();

        await foreach (var chunk in StreamResponseAsync(messages, cancellationToken))
        {
            response.Append(chunk);
        }

        return response.ToString();
    }
}

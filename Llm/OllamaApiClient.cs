using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeetCodeAgent;

public sealed class OllamaApiClient : IChatClient
{
    private readonly HttpClient httpClient;
    private readonly string model;

    public OllamaApiClient(HttpClient httpClient, string model)
    {
        this.httpClient = httpClient;
        this.model = model;
    }

    public async IAsyncEnumerable<string> StreamResponseAsync(
        IReadOnlyList<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new OllamaChatRequest(
            model,
            messages.Select(message => new OllamaMessage(message.Role, message.Content)).ToList(),
            Stream: true);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/chat")
        {
            Content = JsonContent.Create(request)
        };

        using var response = await httpClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var result = JsonSerializer.Deserialize<OllamaChatResponse>(line);
            var content = result?.Message?.Content;
            if (!string.IsNullOrEmpty(content))
            {
                yield return content;
            }
        }
    }

    private sealed record OllamaChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyList<OllamaMessage> Messages,
        [property: JsonPropertyName("stream")] bool Stream);

    private sealed record OllamaChatResponse(
        [property: JsonPropertyName("message")] OllamaMessage? Message);

    private sealed record OllamaMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);
}

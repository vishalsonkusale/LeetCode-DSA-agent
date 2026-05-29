using System.Net.Http.Json;
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

    public async Task<string> GetResponseAsync(
        IReadOnlyList<ChatMessage> messages,
        CancellationToken cancellationToken = default)
    {
        var request = new OllamaChatRequest(
            model,
            messages.Select(message => new OllamaMessage(message.Role, message.Content)).ToList(),
            Stream: false);

        using var response = await httpClient.PostAsJsonAsync("/api/chat", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken);
        return result?.Message?.Content?.Trim() ?? string.Empty;
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

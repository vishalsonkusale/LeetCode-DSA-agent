using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeetCodeAgent;

public sealed class LeetCodeProblemClient : IProblemClient
{
    private const string QuestionQuery =
        """
        query questionData($titleSlug: String!) {
          question(titleSlug: $titleSlug) {
            title
            titleSlug
            difficulty
            content
            hints
            topicTags {
              name
            }
            codeSnippets {
              langSlug
              code
            }
          }
        }
        """;

    private readonly HttpClient httpClient;

    public LeetCodeProblemClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<LeetCodeProblem?> TryGetProblemAsync(
        string userInput,
        CancellationToken cancellationToken = default)
    {
        var slug = Utilities.ExtractLeetCodeSlug(userInput);
        if (slug is null)
        {
            return null;
        }

        try
        {
            var request = new GraphQlRequest(
                QuestionQuery,
                "questionData",
                new Dictionary<string, string> { ["titleSlug"] = slug });

            using var response = await httpClient.PostAsJsonAsync("/graphql", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<QuestionGraphQlResponse>(cancellationToken);
            var question = result?.Data?.Question;
            if (question is null)
            {
                return null;
            }

            return new LeetCodeProblem(
                question.Title,
                question.TitleSlug,
                question.Difficulty,
                question.TopicTags.Select(topic => topic.Name).ToList(),
                Utilities.StripHtml(question.Content),
                question.Hints.Select(Utilities.StripHtml).ToList(),
                question.CodeSnippets.FirstOrDefault(snippet => snippet.LangSlug == "csharp")?.Code);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }

    private sealed record GraphQlRequest(
        [property: JsonPropertyName("query")] string Query,
        [property: JsonPropertyName("operationName")] string OperationName,
        [property: JsonPropertyName("variables")] Dictionary<string, string> Variables);

    private sealed record QuestionGraphQlResponse(
        [property: JsonPropertyName("data")] QuestionData? Data);

    private sealed record QuestionData(
        [property: JsonPropertyName("question")] QuestionNode? Question);

    private sealed record QuestionNode(
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("titleSlug")] string TitleSlug,
        [property: JsonPropertyName("difficulty")] string Difficulty,
        [property: JsonPropertyName("content")] string Content,
        [property: JsonPropertyName("hints")] IReadOnlyList<string> Hints,
        [property: JsonPropertyName("topicTags")] IReadOnlyList<TopicTag> TopicTags,
        [property: JsonPropertyName("codeSnippets")] IReadOnlyList<CodeSnippet> CodeSnippets);

    private sealed record TopicTag(
        [property: JsonPropertyName("name")] string Name);

    private sealed record CodeSnippet(
        [property: JsonPropertyName("langSlug")] string LangSlug,
        [property: JsonPropertyName("code")] string Code);
}

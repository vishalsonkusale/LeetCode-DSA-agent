using LeetCodeAgent;
using System.Text;

var database = new Database();
var assistant = new DSAAssistant();
var solver = new ProblemSolver();
var generator = new CodeGenerator();
var learningPath = new LearningPath();
var debugger = new CodeDebugger();

var httpClient = new HttpClient()
{
    BaseAddress = new Uri("http://localhost:11434"),
    Timeout = TimeSpan.FromSeconds(10000)
};

IChatClient chatClient = new OllamaApiClient(httpClient, "qwen3:8b");

var leetCodeHttpClient = new HttpClient()
{
    BaseAddress = new Uri("https://leetcode.com"),
    Timeout = TimeSpan.FromSeconds(30)
};
leetCodeHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("LeetCodeAgent/1.0");

IProblemClient problemClient = new LeetCodeProblemClient(leetCodeHttpClient);
var agent = new LeetCodeDsaAgent(solver, assistant, generator, learningPath, debugger, database, chatClient, problemClient);

ConsoleRenderer.WriteWelcome(database);

while (true)
{
    ConsoleRenderer.WritePrompt();
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        ConsoleRenderer.WriteInfo("Ask a DSA question, paste a LeetCode URL, or type /cmd.");
        continue;
    }

    if (Utilities.IsExitCommand(input))
    {
        ConsoleRenderer.WriteInfo("Good luck with the next problem.");
        break;
    }

    var commandResult = ConsoleCommands.Handle(input, database);
    if (commandResult.ShouldExit)
    {
        ConsoleRenderer.WriteInfo("Good luck with the next problem.");
        break;
    }

    if (commandResult.Handled && commandResult.AgentInput is null)
    {
        Console.WriteLine();
        continue;
    }

    var agentInput = commandResult.AgentInput ?? input;

    Console.WriteLine();
    ConsoleRenderer.WriteAssistantHeader();

    var streamedResponse = new StringBuilder();
    try
    {
        await foreach (var chunk in agent.StreamResponseAsync(agentInput))
        {
            streamedResponse.Append(chunk);
            ConsoleRenderer.WriteAssistantChunk(chunk);
        }
    }
    catch (HttpRequestException exception)
    {
        streamedResponse.Append($"Could not reach Ollama. Make sure Ollama is running at http://localhost:11434 and qwen3:8b is pulled. Details: {exception.Message}");
        ConsoleRenderer.WriteAssistantChunk(streamedResponse.ToString());
    }
    catch (TaskCanceledException)
    {
        streamedResponse.Append("The Ollama request timed out.");
        ConsoleRenderer.WriteAssistantChunk(streamedResponse.ToString());
    }

    if (streamedResponse.Length == 0)
    {
        streamedResponse.Append("I did not receive a response from the local Ollama model.");
        ConsoleRenderer.WriteAssistantChunk(streamedResponse.ToString());
    }

    agent.RecordResponse(agentInput, streamedResponse.ToString());
    ConsoleRenderer.FinishAssistantStream();
    Console.WriteLine();
}

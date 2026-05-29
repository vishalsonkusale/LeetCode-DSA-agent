using LeetCodeAgent;

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
    ConsoleRenderer.WriteAssistant(await agent.RespondAsync(agentInput));
    Console.WriteLine();
}

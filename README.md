# LeetCode DSA Assistant

A .NET 10 console agent for LeetCode and DSA practice. It uses a local Ollama model for reasoning, can fetch LeetCode problem statements from problem URLs, remembers previous conversation context in a local JSONL file, and exposes slash commands for common workflows.

## Capabilities

- Solve LeetCode/DSA problems from text prompts.
- Fetch a LeetCode problem from a URL and ask the model for:
  - how to think about the problem,
  - pattern recognition cues,
  - brute force and optimized approaches,
  - complexity analysis,
  - edge cases and dry runs,
  - complete C# solution code.
- Generate C# LeetCode-style solutions.
- Debug/review DSA code.
- Explain DSA concepts and algorithms.
- Build learning paths for topics.
- Persist conversation context across app restarts.
- Stream model responses in the console as Ollama generates them.

## Prerequisites

- .NET SDK 10
- Ollama running locally
- The `qwen3:8b` model pulled in Ollama
- Internet access when fetching LeetCode problem URLs

Check .NET:

```bash
dotnet --info
```

Install/pull the Ollama model:

```bash
ollama pull qwen3:8b
```

Start Ollama if it is not already running:

```bash
ollama serve
```

The app expects Ollama at:

```text
http://localhost:11434
```

## Run The App

From the repo root:

```bash
dotnet run --project LeetCodeAgent.csproj
```

On startup, the console shows the context database path, model information, recent context, and available commands.

## Slash Commands

Use `/cmd` or `/help` inside the console to show the command list.

```text
/solve <LeetCode URL or problem text>  Fetch problem links and explain all approaches
/code <problem or idea>                Generate C# LeetCode solution code
/debug <code or issue>                 Review correctness, edge cases, and complexity
/explain <concept>                     Explain a DSA concept or algorithm
/path <topic>                          Build a study/practice roadmap
/history [count]                       Show saved recent context
/forget                                Clear saved context
/clear                                 Clear the console
/cmd or /help                          Show this command list
/exit                                  Quit
```

Examples:

```text
/solve https://leetcode.com/problems/two-sum/
/code binary search in rotated sorted array
/debug int mid = (left + right) / 2;
/explain sliding window
/path dynamic programming
/history 10
```

You can also type natural language directly:

```text
How do I solve a two pointer problem?
Explain monotonic stack with examples.
```

## Persistent Context

Conversation history is stored locally at:

```text
Data/context-log.jsonl
```

The agent reloads recent context when the app starts, so follow-up questions can refer to previous discussion.

To clear saved context from the console:

```text
/forget
```

## Project Structure

```text
LeetCodeAgent/
├── Program.cs
├── Agent/
│   └── LeetCodeDsaAgent.cs
├── Console/
│   ├── ConsoleCommands.cs
│   └── ConsoleRenderer.cs
├── LeetCode/
│   ├── IProblemClient.cs
│   └── LeetCodeProblemClient.cs
├── Llm/
│   ├── IChatClient.cs
│   └── OllamaApiClient.cs
├── Modules/
│   ├── CodeDebugger.cs
│   ├── CodeGenerator.cs
│   ├── DSAAssistant.cs
│   ├── LearningPath.cs
│   └── ProblemSolver.cs
├── Persistence/
│   └── Database.cs
├── Shared/
│   ├── Models.cs
│   └── Utilities.cs
├── Data/
│   └── context-log.jsonl
└── Tests/
    ├── LeetCodeAgent.Tests.csproj
    └── Program.cs
```

## Run Tests

```bash
dotnet run --project Tests/LeetCodeAgent.Tests.csproj --no-restore
```

The tests use fake chat/problem clients, so they do not require Ollama or LeetCode network access.

## Build

```bash
dotnet build LeetCodeAgent.csproj
```

## Notes

- The current model is hard-coded in `Program.cs` as `qwen3:8b`.
- The Ollama client uses `/api/chat`.
- Responses are streamed from Ollama, so output starts appearing before the full answer is complete.
- The LeetCode client uses LeetCode's GraphQL endpoint to fetch public problem data by URL slug.
- If Ollama is not running or the model is missing, the agent will show a connection/model error in the console.

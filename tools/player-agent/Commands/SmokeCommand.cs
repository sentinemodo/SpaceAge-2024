using System.CommandLine;
using SpaceAge.PlayerAgent.Inference;

namespace SpaceAge.PlayerAgent.Commands;

internal static class SmokeCommand
{
    public static Command Create()
    {
        var command = new Command(
            "smoke",
            "Verify Ollama chat (qwen2.5-coder:7b default) and embedding endpoints.");
        command.AddOption(CommandHelpers.AllowRunPodOption);

        command.SetHandler(async (context) =>
        {
            var settings = CommandHelpers.LoadSettings(context);
            CommandHelpers.PrintSettings(settings);

            using var client = new OllamaClient(settings);
            await client.SmokeTestAsync(context.GetCancellationToken());

            Console.WriteLine("Smoke test passed (chat + embeddings).");
        });

        return command;
    }
}

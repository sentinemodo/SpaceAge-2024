using System.CommandLine;
using SpaceAge.PlayerAgent.Configuration;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Usage;

namespace SpaceAge.PlayerAgent.Commands;

internal static class SmokeCommand
{
    public static Command Create()
    {
        var command = new Command(
            "smoke",
            "Verify Ollama chat (qwen2.5-coder:7b default) and embedding endpoints.");
        command.AddOption(CommandHelpers.AllowRunPodOption);
        command.AddOption(CommandHelpers.YesOption);

        command.SetHandler(async (context) =>
        {
            var settings = CommandHelpers.LoadSettings(context);
            var assumeYes = context.ParseResult.GetValueForOption(CommandHelpers.YesOption);
            CommandHelpers.PrintSettings(settings);

            var inference = RemoteInferenceContext.Create(
                settings,
                assumeYes,
                new ConsoleUserPrompt(),
                command: "smoke",
                runId: null,
                factionIds: null,
                dryRun: false);

            try
            {
                if (inference.Client is GuardedOllamaClient guarded)
                {
                    await guarded.SmokeTestAsync(context.GetCancellationToken());
                }
                else if (inference.Client is OllamaClient plain)
                {
                    await plain.SmokeTestAsync(context.GetCancellationToken());
                }

                Console.WriteLine("Smoke test passed (chat + embeddings).");
            }
            finally
            {
                await inference.DisposeAsync();
            }
        });

        return command;
    }
}

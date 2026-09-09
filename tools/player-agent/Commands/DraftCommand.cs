using System.CommandLine;
using SpaceAge.PlayerAgent.Paths;

namespace SpaceAge.PlayerAgent.Commands;

internal static class DraftCommand
{
    public static Command Create()
    {
        var command = new Command("draft", "Draft UTF-8 orders from report + RAG (Phase 3).");
        command.AddOption(CommandHelpers.ModeOption);
        command.AddOption(CommandHelpers.RunOption);
        command.AddOption(CommandHelpers.FactionOption);
        command.AddOption(CommandHelpers.OutputOption);
        command.AddOption(CommandHelpers.AllowRunPodOption);
        command.AddOption(CommandHelpers.DryRunOption);

        command.SetHandler((context) =>
        {
            var mode = CommandHelpers.ParseRequiredMode(context);
            var settings = CommandHelpers.LoadSettings(context);
            var dryRun = context.ParseResult.GetValueForOption(CommandHelpers.DryRunOption);
            var output = context.ParseResult.GetValueForOption(CommandHelpers.OutputOption);
            var runId = context.ParseResult.GetValueForOption(CommandHelpers.RunOption);
            var factionId = context.ParseResult.GetValueForOption(CommandHelpers.FactionOption);

            var repoRoot = RepoPaths.FindRepositoryRoot();
            var draftPath = RepoPaths.ResolveDraftOutput(
                repoRoot,
                output,
                runId,
                factionId);

            Console.WriteLine($"Mode:             {PlayModeParser.ToCliValue(mode)}");
            Console.WriteLine($"Draft output:     {draftPath}");
            Console.WriteLine("Inputs (Phase 3): isolated report, optional story.md, shared + faction RAG retrieval.");

            if (dryRun)
            {
                Console.WriteLine("Dry run: prompt pack only; no chat call.");
                return;
            }

            Console.WriteLine("Phase 0 stub: draft is not implemented until Phase 3.");
        });

        return command;
    }
}

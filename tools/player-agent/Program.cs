using System.CommandLine;
using SpaceAge.PlayerAgent.Commands;

return await BuildRootCommand().InvokeAsync(args);

static RootCommand BuildRootCommand()
{
    var root = new RootCommand(
        "SpaceAge player-agent — Ollama-backed order drafting outside Game.exe (ADR-0009).");

    root.AddCommand(ConfigCommand.Create());
    root.AddCommand(SmokeCommand.Create());
    root.AddCommand(IngestSharedCommand.Create());
    root.AddCommand(IngestFactionCommand.Create());
    root.AddCommand(IngestRunCommand.Create());
    root.AddCommand(RefreshSharedCommand.Create());
    root.AddCommand(RegenerateAllowlistCommand.Create());
    root.AddCommand(RetrieveCommand.Create());
    root.AddCommand(DraftCommand.Create());
    root.AddCommand(DraftRunCommand.Create());
    root.AddCommand(AuditIsolationCommand.Create());
    root.AddCommand(UsageCommand.Create());

    return root;
}

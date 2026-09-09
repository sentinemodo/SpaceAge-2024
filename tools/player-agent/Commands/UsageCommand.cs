using System.CommandLine;

namespace SpaceAge.PlayerAgent.Commands;

internal static class UsageCommand
{
    public static Command Create()
    {
        var command = new Command("usage", "RunPod usage ledger and reports (Phase 7).");

        command.AddCommand(CreateStub("start", "Begin a RunPod usage session."));
        command.AddCommand(CreateStub("stop", "End a RunPod usage session."));
        command.AddCommand(CreateStub("status", "Show the active usage session."));
        command.AddCommand(CreateStub("report", "Summarize usage for a period."));
        command.AddCommand(CreateStub("sync", "Reconcile local ledger with RunPod API (optional)."));

        return command;
    }

    private static Command CreateStub(string name, string description)
    {
        var command = new Command(name, description);
        command.SetHandler(() =>
        {
            Console.WriteLine($"Phase 0 stub: usage {name} is not implemented until Phase 7.");
        });
        return command;
    }
}

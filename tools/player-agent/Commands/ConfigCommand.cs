using System.CommandLine;
using SpaceAge.PlayerAgent.Configuration;

namespace SpaceAge.PlayerAgent.Commands;

internal static class ConfigCommand
{
    public static Command Create()
    {
        var command = new Command("config", "Show resolved player-agent configuration.");
        command.SetHandler(() =>
        {
            var settings = PlayerAgentSettings.Load(allowRunPodFlag: false);
            CommandHelpers.PrintSettings(settings);
        });
        return command;
    }
}

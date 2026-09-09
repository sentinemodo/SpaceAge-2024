using System.CommandLine;
using System.CommandLine.Invocation;
using SpaceAge.PlayerAgent.Configuration;

namespace SpaceAge.PlayerAgent.Commands;

internal static class CommandHelpers
{
    public static Option<bool> AllowRunPodOption { get; } = new("--allow-runpod")
    {
        Description = "Allow remote Ollama hosts (RunPod). Required when OLLAMA_HOST is not localhost.",
    };

    public static Option<string> ModeOption { get; } = new("--mode")
    {
        Description = "Play mode for manual corpus selection: test (SampleGame manuals) or campaign.",
        IsRequired = true,
    };

    public static Option<string> RunOption { get; } = new("--run")
    {
        Description = "Campaign run id under play/runs/<id>/.",
    };

    public static Option<int?> FactionOption { get; } = new("--faction")
    {
        Description = "Faction id (2–11) for campaign paths.",
    };

    public static Option<string> OutputOption { get; } = new("--output")
    {
        Description = "Explicit UTF-8 draft output path (dev/test).",
    };

    public static Option<bool> DryRunOption { get; } = new("--dry-run")
    {
        Description = "Build prompt/RAG inputs without calling chat or starting remote pods.",
    };

    public static PlayMode ParseRequiredMode(InvocationContext context)
    {
        var modeValue = context.ParseResult.GetValueForOption(ModeOption);
        if (!PlayModeParser.TryParse(modeValue, out var mode))
        {
            throw new InvalidOperationException("--mode is required and must be test or campaign.");
        }

        return mode;
    }

    public static PlayerAgentSettings LoadSettings(InvocationContext context, bool checkRemote = true)
    {
        var allowRunPod = context.ParseResult.GetValueForOption(AllowRunPodOption);
        var settings = PlayerAgentSettings.Load(allowRunPod);
        if (checkRemote)
        {
            RunPodGuard.EnsureRemoteAllowed(settings);
        }

        return settings;
    }

    public static void PrintSettings(PlayerAgentSettings settings)
    {
        Console.WriteLine($"Ollama host:      {settings.OllamaBaseUri}");
        Console.WriteLine($"OpenAI base:      {settings.OpenAiBaseUri}");
        Console.WriteLine($"Chat model:       {settings.ChatModel}");
        Console.WriteLine($"Embed model:      {settings.EmbedModel}");
        Console.WriteLine($"Index directory:  {settings.IndexDirectory}");
        Console.WriteLine($"Remote host:      {settings.IsRemoteHost}");
        Console.WriteLine($"Allow RunPod:     {settings.AllowRunPod}");
    }
}

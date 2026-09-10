using System.CommandLine;
using SpaceAge.PlayerAgent.Lint;
using SpaceAge.PlayerAgent.Paths;

namespace SpaceAge.PlayerAgent.Commands;

internal static class RegenerateAllowlistCommand
{
    public static Command Create()
    {
        var outputOption = new Option<string?>("--output")
        {
            Description = "Verb allowlist JSON path (default tools/player-agent/Lint/verb-allowlist.json).",
        };

        var command = new Command(
            "regenerate-allowlist",
            "Export the verb allowlist from player/rules.md (Phase 5).");
        command.AddOption(outputOption);

        command.SetHandler((context) =>
        {
            var outputPath = context.ParseResult.GetValueForOption(outputOption);
            var repoRoot = RepoPaths.FindRepositoryRoot();
            var rulesPath = Path.Combine(RepoPaths.PlayerDirectory(repoRoot), "rules.md");
            if (!File.Exists(rulesPath))
            {
                throw new InvalidOperationException($"Rules file not found: {rulesPath}");
            }

            var resolvedOutput = string.IsNullOrWhiteSpace(outputPath)
                ? OrderVerbAllowlistExporter.DefaultOutputPath(repoRoot)
                : Path.GetFullPath(outputPath);

            var document = OrderVerbAllowlistExporter.BuildFromRulesFile(
                rulesPath,
                "player/rules.md");
            OrderVerbAllowlistExporter.WriteJson(resolvedOutput, document);

            Console.WriteLine($"Rules source:     {document.SourcePath}");
            Console.WriteLine($"Rules hash:       {document.SourceContentHash}");
            Console.WriteLine($"Verb count:       {document.Verbs.Count}");
            Console.WriteLine($"Allowlist output: {resolvedOutput}");
            Console.WriteLine("Allowlist regenerated.");
        });

        return command;
    }
}

using System.Text.Json;
using NUnit.Framework;
using SpaceAge.PlayerAgent.Lint;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class OrderVerbAllowlistExporterTests
{
    [Test]
    public void Build_IncludesSortedVerbsAndSourceHash()
    {
        const string rules = """
            ## Immediate orders

            ### MOVE

            text

            ## Long orders

            ### USE

            text
            """;

        var document = OrderVerbAllowlistExporter.Build("player/rules.md", rules);

        Assert.That(document.SourcePath, Is.EqualTo("player/rules.md"));
        Assert.That(document.Verbs, Is.EqualTo(new[] { "MOVE", "USE" }));
        Assert.That(document.SourceContentHash, Has.Length.GreaterThan(10));
        Assert.That(document.GeneratedAtUtc, Is.Not.Empty);
    }

    [Test]
    public void WriteJson_RoundTripsThroughReadJson()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "player-agent-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var outputPath = Path.Combine(tempDir, "verb-allowlist.json");

        try
        {
            const string rules = """
                ## Immediate orders

                ### ACTIVE

                text

                ## Long orders

                ### TRAIN

                text
                """;

            var document = OrderVerbAllowlistExporter.Build("player/rules.md", rules);
            OrderVerbAllowlistExporter.WriteJson(outputPath, document);
            var loaded = OrderVerbAllowlistExporter.ReadJson(outputPath);

            Assert.That(loaded.Verbs, Is.EqualTo(document.Verbs));
            Assert.That(loaded.SourceContentHash, Is.EqualTo(document.SourceContentHash));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Test]
    public void WriteJson_UsesStablePropertyOrder()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "player-agent-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var outputPath = Path.Combine(tempDir, "verb-allowlist.json");

        try
        {
            var document = OrderVerbAllowlistExporter.Build("player/rules.md", "## Immediate orders\n\n### MOVE\n");
            OrderVerbAllowlistExporter.WriteJson(outputPath, document);
            var json = File.ReadAllText(outputPath);

            Assert.That(json, Does.Contain("\"sourcePath\""));
            Assert.That(json, Does.Contain("\"verbs\""));
            Assert.That(() => JsonDocument.Parse(json), Throws.Nothing);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}

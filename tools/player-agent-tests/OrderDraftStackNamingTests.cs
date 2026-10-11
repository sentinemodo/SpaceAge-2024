using NUnit.Framework;
using SpaceAge.PlayerAgent.Draft;
using SpaceAge.PlayerAgent.Paths;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class OrderDraftStackNamingTests
{
    [Test]
    public void InjectMissingNames_MilitaryTurn1_AddsNamesOnNewStacks()
    {
        const string orders = """
            #faction 7 "irnclad"
            #modulestack 250005
            use grndtr as new1 for 250001
            use armcbt as new2 for 250001
            use armcbt as new3 for 250001

            #modulestack new1
            move R00052

            #modulestack new2
            has 1 tanks

            #modulestack new3
            has 1 tanks
            #end
            """;

        var result = OrderDraftStackNaming.InjectMissingNames(orders, factionId: 7, "military");

        Assert.That(result, Does.Contain("#modulestack new1"));
        var scoutName = OrderDraftStackNaming.GenerateDisplayName("grndtr", "military", 7, "new1");
        var thirdTankName = OrderDraftStackNaming.GenerateDisplayName("armcbt", "military", 7, "new3");
        Assert.That(result, Does.Contain("NAME \"" + scoutName + "\""));
        Assert.That(result, Does.Contain("NAME \"" + thirdTankName + "\""));
        Assert.That(result, Does.Match(@"(?m)#modulestack new1\s*\r?\nNAME ""[^""]+""\s*\r?\nmove"));
    }

    [Test]
    public void InjectMissingNames_ResearcherMoblab_AddsScienceFlavorName()
    {
        const string orders = """
            #faction 10 "silicat"
            #modulestack 280005
            use msrvtm as new109 for 280001

            #modulestack new109
            move R00052
            @research R00052
            #end
            """;

        var result = OrderDraftStackNaming.InjectMissingNames(orders, factionId: 10, "researcher");

        var labName = OrderDraftStackNaming.GenerateDisplayName("msrvtm", "researcher", 10, "new109");
        Assert.That(result, Does.Contain("NAME \"" + labName + "\""));
    }

    [Test]
    public void InjectMissingNames_SkipsHumanFaction1()
    {
        const string orders = """
            #faction 1 "human"
            #modulestack 100005
            use grndtr as new1 for 100001
            #modulestack new1
            move R00001
            #end
            """;

        var result = OrderDraftStackNaming.InjectMissingNames(orders, factionId: 1, "military");

        Assert.That(result, Does.Not.Contain("NAME "));
    }

    [Test]
    public void InjectMissingNames_DoesNotReplaceExistingName()
    {
        const string orders = """
            #faction 4 "river"
            use grndtr as new1 for 240001
            #modulestack new1
            NAME "River explorer"
            move R00011
            #end
            """;

        var result = OrderDraftStackNaming.InjectMissingNames(orders, factionId: 4, "economic");

        Assert.That(result.Split("NAME \"River explorer\"").Length, Is.EqualTo(2));
    }

    [Test]
    public void PrepareForWrite_AppliesNamingForAiFactions()
    {
        const string generated = """
            #faction 7
            use grndtr as new1 for 250001
            #modulestack new1
            move R00052
            """;

        var prepared = OrderDraftWriter.PrepareForWrite(generated, factionId: 7, password: "irnclad", "military");

        var scoutName = OrderDraftStackNaming.GenerateDisplayName("grndtr", "military", 7, "new1");
        Assert.That(prepared, Does.Contain("NAME \"" + scoutName + "\""));
        Assert.That(prepared, Does.Contain("#faction 7 \"irnclad\""));
    }

    [Test]
    [Explicit("One-off maintenance: inject NAME into beta-1 AI order files that use newN aliases.")]
    public void RetrofitBeta1OrderFilesWithMissingNames()
    {
        var repoRoot = RepoPaths.FindRepositoryRoot();
        var factionsRoot = Path.Combine(repoRoot, "play", "runs", "beta-1", "factions");
        for (var factionId = 2; factionId <= 11; factionId++)
        {
            var folder = Path.Combine(factionsRoot, factionId.ToString("D2"));
            if (!Directory.Exists(folder))
            {
                continue;
            }

            var persona = ReadPersonaPreference(folder);
            foreach (var path in Directory.EnumerateFiles(folder, "orders*.txt"))
            {
                var text = File.ReadAllText(path);
                if (!text.Contains(" as new", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var updated = OrderDraftStackNaming.InjectMissingNames(text, factionId, persona);
                if (!string.Equals(text, updated, StringComparison.Ordinal))
                {
                    File.WriteAllText(path, updated);
                }
            }
        }
    }

    private static string? ReadPersonaPreference(string factionFolder)
    {
        var personaPath = Path.Combine(factionFolder, "persona.md");
        if (!File.Exists(personaPath))
        {
            return null;
        }

        foreach (var line in File.ReadAllLines(personaPath))
        {
            if (line.StartsWith("## Preference:", StringComparison.OrdinalIgnoreCase))
            {
                return line["## Preference:".Length..].Trim();
            }
        }

        return null;
    }
}

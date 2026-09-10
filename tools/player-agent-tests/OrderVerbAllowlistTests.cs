using NUnit.Framework;
using SpaceAge.PlayerAgent.Lint;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class OrderVerbAllowlistTests
{
    [Test]
    public void FromRules_IncludesImmediateAndLongVerbs()
    {
        var rulesPath = Path.Combine(TestRepoPaths.FindRepositoryRoot(), "player", "rules.md");
        var rules = File.ReadAllText(rulesPath);
        var allowlist = OrderVerbAllowlist.FromRulesMarkdown(rules);

        Assert.That(allowlist.Contains("MOVE"), Is.True);
        Assert.That(allowlist.Contains("USE"), Is.True);
        Assert.That(allowlist.Contains("STACK"), Is.True);
        Assert.That(allowlist.Contains("RESEARCH"), Is.True);
        Assert.That(allowlist.Contains("TRAIN"), Is.True);
    }

    [Test]
    public void FromRules_ExcludesNonOrderHeadings()
    {
        const string rules = """
            ## Immediate orders

            ACTIVE, MOVE.

            ### ACTIVE

            text

            ### File headers

            not a verb

            ## Long orders

            USE.

            ### USE

            text
            """;

        var allowlist = OrderVerbAllowlist.FromRulesMarkdown(rules);

        Assert.That(allowlist, Does.Contain("ACTIVE"));
        Assert.That(allowlist, Does.Contain("USE"));
        Assert.That(allowlist, Does.Not.Contain("FILE"));
    }
}

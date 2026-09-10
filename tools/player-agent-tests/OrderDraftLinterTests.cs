using NUnit.Framework;
using SpaceAge.PlayerAgent.Lint;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class OrderDraftLinterTests
{
    private static readonly HashSet<string> Allowlist =
    [
        "MOVE", "STACK", "GIVE", "USE", "SEE", "RESEARCH",
    ];

    [Test]
    public void Lint_ValidDraft_Passes()
    {
        const string draft = """
            #faction 2 "secret"
            #modulestack 101
            stack 112
            give 1 terran to 112
            -move O00001
            @research 200
            #end
            """;

        var result = OrderDraftLinter.Lint(draft, Allowlist);

        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void Lint_UnknownVerb_FailsWithLineNumber()
    {
        const string draft = """
            #faction 2
            #modulestack 101
            teleport O00001
            #end
            """;

        var result = OrderDraftLinter.Lint(draft, Allowlist);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("TELEPORT"));
        Assert.That(result.Errors, Has.Some.Contains("line 3"));
    }

    [Test]
    public void Lint_MissingEnd_Fails()
    {
        const string draft = """
            #faction 2
            #modulestack 101
            stack 112
            """;

        var result = OrderDraftLinter.Lint(draft, Allowlist);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("#end"));
    }
}

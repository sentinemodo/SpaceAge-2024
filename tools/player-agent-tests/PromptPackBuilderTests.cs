using NUnit.Framework;
using SpaceAge.PlayerAgent.Rag;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class PromptPackBuilderTests
{
    [Test]
    public void StripFactionPassword_RemovesQuotedPassword()
    {
        const string order = "#faction 2 \"secret\"\n#modulestack 101\n";
        var stripped = PromptPackBuilder.StripFactionPassword(order);
        Assert.That(stripped, Does.Contain("#faction 2"));
        Assert.That(stripped, Does.Not.Contain("secret"));
    }

    [Test]
    public void BuildFactionLine_StripsPasswordWhenRemote()
    {
        var line = PromptPackBuilder.BuildFactionLine(2, "secret", stripPassword: true);
        Assert.That(line, Is.EqualTo("#faction 2"));
    }
}

using NUnit.Framework;
using SpaceAge.PlayerAgent.Draft;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class OrderDraftWriterTests
{
    [Test]
    public void PrepareForWrite_StripsFenceAndInjectsPassword()
    {
        const string generated = """
            ```text
            #faction 2
            #modulestack 101
            stack 112
            ```
            """;

        var prepared = OrderDraftWriter.PrepareForWrite(generated, factionId: 2, password: "secret");

        Assert.That(prepared, Does.Contain("#faction 2 \"secret\""));
        Assert.That(prepared, Does.Contain("#end"));
        Assert.That(prepared, Does.Not.Contain("```"));
    }
}

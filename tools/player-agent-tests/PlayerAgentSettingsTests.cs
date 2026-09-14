using NUnit.Framework;
using SpaceAge.PlayerAgent.Configuration;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class PlayerAgentSettingsTests
{
    [Test]
    public void Load_ParsesChatTimeoutSeconds()
    {
        Environment.SetEnvironmentVariable("PLAYER_AGENT_CHAT_TIMEOUT_SECONDS", "900");
        try
        {
            var settings = PlayerAgentSettings.Load(allowRunPodFlag: false);
            Assert.That(settings.ChatTimeoutSeconds, Is.EqualTo(900));
        }
        finally
        {
            Environment.SetEnvironmentVariable("PLAYER_AGENT_CHAT_TIMEOUT_SECONDS", null);
        }
    }

    [Test]
    public void Load_DefaultChatTimeout_IsFifteenMinutes()
    {
        Environment.SetEnvironmentVariable("PLAYER_AGENT_CHAT_TIMEOUT_SECONDS", null);
        var settings = PlayerAgentSettings.Load(allowRunPodFlag: false);
        Assert.That(settings.ChatTimeoutSeconds, Is.EqualTo(PlayerAgentSettings.DefaultChatTimeoutSeconds));
        Assert.That(settings.ChatTimeoutSeconds, Is.EqualTo(900));
    }
}

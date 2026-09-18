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

    [Test]
    public void Load_LocalDefaults_UseSevenBandEightKContext()
    {
        ClearSettingsEnvironment();
        var settings = PlayerAgentSettings.Load(allowRunPodFlag: false);
        Assert.That(settings.ChatModel, Is.EqualTo(PlayerAgentSettings.LocalDefaultChatModel));
        Assert.That(settings.ChatContextTokens, Is.EqualTo(PlayerAgentSettings.LocalDefaultContextTokens));
        Assert.That(settings.ChatMaxOutputTokens, Is.EqualTo(PlayerAgentSettings.DefaultMaxOutputTokens));
        Assert.That(settings.DefaultTopK, Is.EqualTo(PlayerAgentSettings.LocalDefaultTopK));
    }

    [Test]
    public void Load_RemoteHost_UsesRunPodDefaults()
    {
        ClearSettingsEnvironment();
        Environment.SetEnvironmentVariable("OLLAMA_HOST", "https://runpod.example/v1/");
        try
        {
            var settings = PlayerAgentSettings.Load(allowRunPodFlag: true);
            Assert.That(settings.ChatModel, Is.EqualTo(PlayerAgentSettings.RunPodDefaultChatModel));
            Assert.That(settings.ChatContextTokens, Is.EqualTo(PlayerAgentSettings.RunPodDefaultContextTokens));
            Assert.That(settings.DefaultTopK, Is.EqualTo(PlayerAgentSettings.RunPodDefaultTopK));
            Assert.That(settings.IsRemoteHost, Is.True);
        }
        finally
        {
            Environment.SetEnvironmentVariable("OLLAMA_HOST", null);
        }
    }

    [Test]
    public void ResolveChatModel_Auto_UsesHostBasedDefault()
    {
        var localUri = new Uri("http://127.0.0.1:11434/");
        var remoteUri = new Uri("https://runpod.example/v1/");

        Assert.That(
            PlayerAgentSettings.ResolveChatModel(null, localUri),
            Is.EqualTo(PlayerAgentSettings.LocalDefaultChatModel));
        Assert.That(
            PlayerAgentSettings.ResolveChatModel("auto", localUri),
            Is.EqualTo(PlayerAgentSettings.LocalDefaultChatModel));
        Assert.That(
            PlayerAgentSettings.ResolveChatModel("AUTO", remoteUri),
            Is.EqualTo(PlayerAgentSettings.RunPodDefaultChatModel));
    }

    [Test]
    public void ResolveChatModel_ExplicitValue_IsPreserved()
    {
        var localUri = new Uri("http://127.0.0.1:11434/");
        Assert.That(
            PlayerAgentSettings.ResolveChatModel("custom-model:1", localUri),
            Is.EqualTo("custom-model:1"));
    }

    [Test]
    public void Load_ParsesContextOverrides()
    {
        ClearSettingsEnvironment();
        Environment.SetEnvironmentVariable("PLAYER_AGENT_CHAT_CONTEXT_TOKENS", "12288");
        Environment.SetEnvironmentVariable("PLAYER_AGENT_CHAT_MAX_OUTPUT_TOKENS", "2048");
        try
        {
            var settings = PlayerAgentSettings.Load(allowRunPodFlag: false);
            Assert.That(settings.ChatContextTokens, Is.EqualTo(12288));
            Assert.That(settings.ChatMaxOutputTokens, Is.EqualTo(2048));
        }
        finally
        {
            Environment.SetEnvironmentVariable("PLAYER_AGENT_CHAT_CONTEXT_TOKENS", null);
            Environment.SetEnvironmentVariable("PLAYER_AGENT_CHAT_MAX_OUTPUT_TOKENS", null);
        }
    }

    private static void ClearSettingsEnvironment()
    {
        Environment.SetEnvironmentVariable("OLLAMA_HOST", null);
        Environment.SetEnvironmentVariable("PLAYER_AGENT_CHAT_MODEL", null);
        Environment.SetEnvironmentVariable("PLAYER_AGENT_CHAT_CONTEXT_TOKENS", null);
        Environment.SetEnvironmentVariable("PLAYER_AGENT_CHAT_MAX_OUTPUT_TOKENS", null);
        Environment.SetEnvironmentVariable("PLAYER_AGENT_CHAT_TIMEOUT_SECONDS", null);
    }
}

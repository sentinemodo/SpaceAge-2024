using NUnit.Framework;
using SpaceAge.PlayerAgent.Configuration;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class RunPodBudgetSettingsTests
{
    [Test]
    public void LoadFromEnvironment_ParsesBudgetAndCaps()
    {
        Environment.SetEnvironmentVariable("PLAYER_AGENT_BUDGET_USD", "25");
        Environment.SetEnvironmentVariable("PLAYER_AGENT_BUDGET_SOFT_USD", "20");
        Environment.SetEnvironmentVariable("PLAYER_AGENT_MAX_POD_HOURS", "2");
        Environment.SetEnvironmentVariable("PLAYER_AGENT_MAX_CHAT_CALLS", "50");
        Environment.SetEnvironmentVariable("PLAYER_AGENT_IDLE_TIMEOUT_MINUTES", "12");
        Environment.SetEnvironmentVariable("PLAYER_AGENT_REQUIRE_CONFIRM", "0");

        try
        {
            var budget = RunPodBudgetSettings.LoadFromEnvironment(isRemoteHost: true);

            Assert.That(budget.HardBudgetUsd, Is.EqualTo(25));
            Assert.That(budget.SoftBudgetUsd, Is.EqualTo(20));
            Assert.That(budget.MaxPodHoursPerSession, Is.EqualTo(2));
            Assert.That(budget.MaxChatCallsPerSession, Is.EqualTo(50));
            Assert.That(budget.IdleTimeoutMinutes, Is.EqualTo(12));
            Assert.That(budget.RequireConfirm, Is.False);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PLAYER_AGENT_BUDGET_USD", null);
            Environment.SetEnvironmentVariable("PLAYER_AGENT_BUDGET_SOFT_USD", null);
            Environment.SetEnvironmentVariable("PLAYER_AGENT_MAX_POD_HOURS", null);
            Environment.SetEnvironmentVariable("PLAYER_AGENT_MAX_CHAT_CALLS", null);
            Environment.SetEnvironmentVariable("PLAYER_AGENT_IDLE_TIMEOUT_MINUTES", null);
            Environment.SetEnvironmentVariable("PLAYER_AGENT_REQUIRE_CONFIRM", null);
        }
    }

    [Test]
    public void LoadFromEnvironment_RemoteDefaultsRequireConfirm()
    {
        Environment.SetEnvironmentVariable("PLAYER_AGENT_REQUIRE_CONFIRM", null);

        var budget = RunPodBudgetSettings.LoadFromEnvironment(isRemoteHost: true);

        Assert.That(budget.RequireConfirm, Is.True);
    }
}

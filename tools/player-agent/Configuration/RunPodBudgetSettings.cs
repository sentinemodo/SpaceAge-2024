using Microsoft.Extensions.Configuration;

namespace SpaceAge.PlayerAgent.Configuration;

public sealed class RunPodBudgetSettings
{
    public const int DefaultIdleTimeoutMinutes = 15;

    public double? HardBudgetUsd { get; init; }
    public double? SoftBudgetUsd { get; init; }
    public double? MaxPodHoursPerSession { get; init; }
    public double? MaxPodHoursPerMonth { get; init; }
    public int? MaxChatCallsPerSession { get; init; }
    public int IdleTimeoutMinutes { get; init; } = DefaultIdleTimeoutMinutes;
    public bool RequireConfirm { get; init; } = true;

    public static RunPodBudgetSettings LoadFromEnvironment(bool isRemoteHost)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var hard = ParseDouble(configuration["PLAYER_AGENT_BUDGET_USD"]);
        var soft = ParseDouble(configuration["PLAYER_AGENT_BUDGET_SOFT_USD"]);
        if (hard is not null && soft is null)
        {
            soft = hard * 0.8;
        }

        var requireConfirm = ParseBool(configuration["PLAYER_AGENT_REQUIRE_CONFIRM"])
            ?? isRemoteHost;

        return new RunPodBudgetSettings
        {
            HardBudgetUsd = hard,
            SoftBudgetUsd = soft,
            MaxPodHoursPerSession = ParseDouble(configuration["PLAYER_AGENT_MAX_POD_HOURS"]),
            MaxPodHoursPerMonth = ParseDouble(configuration["PLAYER_AGENT_MAX_POD_HOURS_MONTH"]),
            MaxChatCallsPerSession = ParseInt(configuration["PLAYER_AGENT_MAX_CHAT_CALLS"]),
            IdleTimeoutMinutes = ParseInt(configuration["PLAYER_AGENT_IDLE_TIMEOUT_MINUTES"])
                ?? DefaultIdleTimeoutMinutes,
            RequireConfirm = requireConfirm,
        };
    }

    private static double? ParseDouble(string? value) =>
        double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    private static int? ParseInt(string? value) =>
        int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    private static bool? ParseBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (string.Equals(value, "1", StringComparison.Ordinal)
            || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(value, "0", StringComparison.Ordinal)
            || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return null;
    }
}

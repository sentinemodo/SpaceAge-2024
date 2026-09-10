using SpaceAge.PlayerAgent.Configuration;

namespace SpaceAge.PlayerAgent.Usage;

public sealed class RunPodGuardrails
{
    private readonly Func<DateTimeOffset> _clock;

    public RunPodGuardrails(Func<DateTimeOffset>? clock = null)
    {
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public GuardrailCheckResult ValidateBeforeSessionStart(
        PlayerAgentSettings settings,
        RunPodBudgetSettings budget,
        UsageLedger ledger,
        bool assumeYes,
        IUserPrompt prompt,
        RunPodGuardrailOverride? overrideFile)
    {
        if (!settings.IsRemoteHost)
        {
            return new GuardrailCheckResult(true);
        }

        if (ledger.LoadActiveSession() is not null)
        {
            return new GuardrailCheckResult(
                false,
                "An active usage session already exists from a prior run. "
                + "Run 'usage stop' or 'usage reclaim' before starting new remote work.");
        }

        if (budget.HardBudgetUsd is null && overrideFile?.AllowMissingBudget != true)
        {
            return new GuardrailCheckResult(
                false,
                "RunPod guardrail: PLAYER_AGENT_BUDGET_USD is unset. "
                + "Set a monthly hard cap or add a gitignored override at "
                + RunPodGuardrailOverride.OverridePath(settings.IndexDirectory) + ".");
        }

        var now = _clock();
        var sessions = ledger.LoadAllSessions();
        var monthlySpend = RunPodBudgetTracker.MonthlyRemoteSpend(sessions, now);

        if (budget.HardBudgetUsd is { } hard && monthlySpend >= hard)
        {
            return new GuardrailCheckResult(
                false,
                $"RunPod guardrail: hard budget ${hard:F2} already reached "
                + $"(spent ${monthlySpend:F2} this month). Run 'usage report' or raise PLAYER_AGENT_BUDGET_USD.");
        }

        if (budget.MaxPodHoursPerMonth is { } maxMonthHours)
        {
            var monthHours = RunPodBudgetTracker.MonthlyRemoteHours(sessions, now);
            if (monthHours >= maxMonthHours)
            {
                return new GuardrailCheckResult(
                    false,
                    $"RunPod guardrail: monthly pod-hours cap {maxMonthHours:F2} reached "
                    + $"(used {monthHours:F2} h).");
            }
        }

        if (budget.SoftBudgetUsd is { } soft && monthlySpend >= soft && !assumeYes)
        {
            var message =
                $"Soft budget ${soft:F2} reached (spent ${monthlySpend:F2} this month). Continue RunPod at "
                + $"{UsageReportFormatter.FormatUsd(settings.HourlyRateUsd)}/hr? [y/N] ";
            if (!prompt.Confirm(message))
            {
                return new GuardrailCheckResult(false, "RunPod start cancelled at soft budget prompt.");
            }
        }
        else if (budget.RequireConfirm && !assumeYes)
        {
            var gpu = settings.GpuClass ?? "remote GPU";
            var message =
                $"Start {gpu} at {UsageReportFormatter.FormatUsd(settings.HourlyRateUsd)}/hr "
                + $"on {settings.OllamaBaseUri.Host}? [y/N] ";
            if (!prompt.Confirm(message))
            {
                return new GuardrailCheckResult(false, "RunPod start cancelled at confirmation prompt.");
            }
        }

        return new GuardrailCheckResult(true);
    }

    public GuardrailViolation? AssertBeforeInferenceCall(
        PlayerAgentSettings settings,
        RunPodBudgetSettings budget,
        UsageLedger ledger,
        UsageActiveSession active,
        bool isChatCall)
    {
        if (!settings.IsRemoteHost)
        {
            return null;
        }

        var now = _clock();

        if (isChatCall
            && budget.MaxChatCallsPerSession is { } maxChat
            && active.ChatCalls >= maxChat)
        {
            return new GuardrailViolation(
                $"RunPod guardrail: session chat-call cap {maxChat} reached.",
                UsageStopReason.Guardrail);
        }

        if (active.ChatCalls + active.EmbedCalls > 0
            && (now - active.LastActivityAt).TotalMinutes > budget.IdleTimeoutMinutes)
        {
            return new GuardrailViolation(
                $"RunPod guardrail: idle timeout ({budget.IdleTimeoutMinutes} min) exceeded.",
                UsageStopReason.IdleTimeout);
        }

        if (budget.MaxPodHoursPerSession is { } maxSessionHours)
        {
            var elapsedHours = (now - active.StartedAt).TotalHours;
            if (elapsedHours > maxSessionHours)
            {
                return new GuardrailViolation(
                    $"RunPod guardrail: session wall clock cap {maxSessionHours:F2} h exceeded.",
                    UsageStopReason.Guardrail);
            }
        }

        var monthlySpend = RunPodBudgetTracker.ProjectedMonthlySpend(ledger.LoadAllSessions(), active, now);
        if (budget.HardBudgetUsd is { } hard && monthlySpend > hard)
        {
            return new GuardrailViolation(
                $"RunPod guardrail: hard budget ${hard:F2} would be exceeded "
                + $"(projected ${monthlySpend:F2}).",
                UsageStopReason.Guardrail);
        }

        return null;
    }

    public static string FormatGuardrailFailure(GuardrailViolation violation) =>
        violation.Message + Environment.NewLine
        + "Next steps: run 'usage report', raise PLAYER_AGENT_BUDGET_USD, switch to localhost, "
        + "or stop the RunPod pod in the console.";
}

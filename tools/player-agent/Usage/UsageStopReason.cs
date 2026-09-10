namespace SpaceAge.PlayerAgent.Usage;

public static class UsageStopReason
{
    public const string User = "user";
    public const string Guardrail = "guardrail";
    public const string Error = "error";
    public const string IdleTimeout = "idle-timeout";
    public const string Complete = "complete";
    public const string Cancelled = "cancelled";
}

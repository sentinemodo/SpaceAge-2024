namespace SpaceAge.PlayerAgent.Usage;

public sealed record GuardrailViolation(string Message, string StopReason);

public sealed record GuardrailCheckResult(bool Allowed, string? Message = null);

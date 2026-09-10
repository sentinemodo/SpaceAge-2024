using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpaceAge.PlayerAgent.Configuration;

public sealed class RunPodGuardrailOverride
{
    public bool AllowMissingBudget { get; init; }
    public DateTimeOffset? ExpiresAtUtc { get; init; }

    public static string OverridePath(string indexDirectory) =>
        Path.Combine(indexDirectory, "runpod-guardrail-override.json");

    public static RunPodGuardrailOverride? TryLoad(string indexDirectory, DateTimeOffset nowUtc)
    {
        var path = OverridePath(indexDirectory);
        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        var parsed = JsonSerializer.Deserialize<RunPodGuardrailOverride>(json, JsonOptions);
        if (parsed is null)
        {
            return null;
        }

        if (parsed.ExpiresAtUtc is { } expires && expires <= nowUtc)
        {
            return null;
        }

        return parsed;
    }

    public bool IsActive(DateTimeOffset nowUtc) =>
        ExpiresAtUtc is null || ExpiresAtUtc > nowUtc;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}

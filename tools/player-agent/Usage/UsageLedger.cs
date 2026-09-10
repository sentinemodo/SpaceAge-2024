using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpaceAge.PlayerAgent.Usage;

public sealed class UsageLedger
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    private readonly string _usageDirectory;

    public UsageLedger(string indexDirectory)
    {
        _usageDirectory = Path.Combine(indexDirectory, "usage");
    }

    public string UsageDirectory => _usageDirectory;
    public string ActiveSessionPath => Path.Combine(_usageDirectory, "active-session.json");
    public string SessionsPath => Path.Combine(_usageDirectory, "sessions.jsonl");

    public void EnsureLayout() => Directory.CreateDirectory(_usageDirectory);

    public UsageActiveSession? LoadActiveSession()
    {
        if (!File.Exists(ActiveSessionPath))
        {
            return null;
        }

        var json = File.ReadAllText(ActiveSessionPath);
        return JsonSerializer.Deserialize<UsageActiveSession>(json, JsonOptions);
    }

    public void SaveActiveSession(UsageActiveSession session)
    {
        EnsureLayout();
        var json = JsonSerializer.Serialize(session, JsonOptions);
        File.WriteAllText(ActiveSessionPath, json);
    }

    public void ClearActiveSession()
    {
        if (File.Exists(ActiveSessionPath))
        {
            File.Delete(ActiveSessionPath);
        }
    }

    public void AppendCompletedSession(UsageSessionRecord record)
    {
        EnsureLayout();
        var json = JsonSerializer.Serialize(record, JsonOptions);
        File.AppendAllText(SessionsPath, json + Environment.NewLine);
    }

    public IReadOnlyList<UsageSessionRecord> LoadAllSessions()
    {
        if (!File.Exists(SessionsPath))
        {
            return [];
        }

        var sessions = new List<UsageSessionRecord>();
        foreach (var line in File.ReadLines(SessionsPath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var record = JsonSerializer.Deserialize<UsageSessionRecord>(line, JsonOptions);
            if (record is not null)
            {
                sessions.Add(record);
            }
        }

        return sessions;
    }
}

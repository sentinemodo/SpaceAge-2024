using System.Text.RegularExpressions;

namespace SpaceAge.PlayerAgent.Paths;

public static partial class OrderFileNaming
{
    public static string FormatFileName(int factionId, int turn, int iteration) =>
        $"orders.{factionId}.{turn}.{iteration}.txt";

    public static int InferDraftTurnFromReportTurn(int reportTurn) => reportTurn + 1;

    public static bool TryParseReportFileName(string fileName, out int reportTurn, out int factionId)
    {
        reportTurn = 0;
        factionId = 0;
        var match = ReportFileRegex().Match(Path.GetFileName(fileName));
        if (!match.Success)
        {
            return false;
        }

        reportTurn = int.Parse(match.Groups[1].Value);
        factionId = int.Parse(match.Groups[2].Value);
        return true;
    }

    public static bool TryParseOrderFileName(string fileName, out int factionId, out int turn, out int iteration)
    {
        factionId = 0;
        turn = 0;
        iteration = 0;
        var match = OrderFileRegex().Match(Path.GetFileName(fileName));
        if (!match.Success)
        {
            return false;
        }

        factionId = int.Parse(match.Groups[1].Value);
        turn = int.Parse(match.Groups[2].Value);
        iteration = int.Parse(match.Groups[3].Value);
        return true;
    }

    public static int ResolveNextIteration(string factionDir, int factionId, int turn)
    {
        var maxIteration = 0;
        if (Directory.Exists(factionDir))
        {
            foreach (var path in Directory.GetFiles(factionDir, $"orders.{factionId}.{turn}.*.txt"))
            {
                if (TryParseOrderFileName(path, out _, out _, out var iteration))
                {
                    maxIteration = Math.Max(maxIteration, iteration);
                }
            }
        }

        return maxIteration + 1;
    }

    public static string? ResolveActiveOrderPath(string factionDir, int factionId, int turn)
    {
        if (!Directory.Exists(factionDir))
        {
            return null;
        }

        string? bestPath = null;
        var bestIteration = -1;
        foreach (var path in Directory.GetFiles(factionDir, $"orders.{factionId}.{turn}.*.txt"))
        {
            if (!TryParseOrderFileName(path, out _, out _, out var iteration))
            {
                continue;
            }

            if (iteration > bestIteration)
            {
                bestIteration = iteration;
                bestPath = path;
            }
        }

        return bestPath;
    }

    public static int InferDraftTurn(string? latestReportPath, int factionId)
    {
        if (!string.IsNullOrWhiteSpace(latestReportPath)
            && TryParseReportFileName(latestReportPath, out var reportTurn, out var reportFaction)
            && reportFaction == factionId)
        {
            return InferDraftTurnFromReportTurn(reportTurn);
        }

        throw new InvalidOperationException(
            "Could not infer draft turn from the latest report. Pass --turn explicitly.");
    }

    [GeneratedRegex(@"^report\.(\d+)\.(\d+)\.txt$", RegexOptions.IgnoreCase)]
    private static partial Regex ReportFileRegex();

    [GeneratedRegex(@"^orders\.(\d+)\.(\d+)\.(\d+)\.txt$", RegexOptions.IgnoreCase)]
    private static partial Regex OrderFileRegex();
}

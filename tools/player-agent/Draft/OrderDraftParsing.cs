namespace SpaceAge.PlayerAgent.Draft;

public static class OrderDraftParsing
{
    public sealed record ModuleStackBlock(string StackId, IReadOnlyList<string> Lines);

    public static IReadOnlyList<ModuleStackBlock> ParseModuleStackBlocks(string orderText)
    {
        var blocks = new List<ModuleStackBlock>();
        string? currentId = null;
        var lines = new List<string>();

        foreach (var rawLine in orderText.Replace("\r\n", "\n").Split('\n'))
        {
            var trimmed = rawLine.Trim();
            if (trimmed.StartsWith("#modulestack", StringComparison.OrdinalIgnoreCase))
            {
                if (currentId is not null)
                {
                    blocks.Add(new ModuleStackBlock(currentId, lines));
                }

                currentId = trimmed.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries)[1];
                lines = [];
                continue;
            }

            if (currentId is null)
            {
                continue;
            }

            if (trimmed.Equals("#end", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            lines.Add(rawLine);
        }

        if (currentId is not null)
        {
            blocks.Add(new ModuleStackBlock(currentId, lines));
        }

        return blocks;
    }

    public static string ExtractFactionPreamble(string orderText)
    {
        var kept = new List<string>();
        foreach (var rawLine in orderText.Replace("\r\n", "\n").Split('\n'))
        {
            if (rawLine.TrimStart().StartsWith("#modulestack", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (rawLine.TrimStart().Equals("#end", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            kept.Add(rawLine);
        }

        return string.Join(Environment.NewLine, kept).TrimEnd();
    }
}

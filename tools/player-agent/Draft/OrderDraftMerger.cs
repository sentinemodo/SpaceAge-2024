namespace SpaceAge.PlayerAgent.Draft;

public static class OrderDraftMerger
{
    /// <summary>Merge pass-2 modulestack sections into pass-1 bootstrap draft.</summary>
    public static string MergeBootstrapAndProduction(string bootstrapPass, string productionPass)
    {
        var preamble = OrderDraftParsing.ExtractFactionPreamble(bootstrapPass);
        if (string.IsNullOrWhiteSpace(preamble))
        {
            preamble = OrderDraftParsing.ExtractFactionPreamble(productionPass);
        }

        var blocks = new Dictionary<string, OrderDraftParsing.ModuleStackBlock>(StringComparer.OrdinalIgnoreCase);
        foreach (var block in OrderDraftParsing.ParseModuleStackBlocks(bootstrapPass))
        {
            blocks[block.StackId] = block;
        }

        foreach (var block in OrderDraftParsing.ParseModuleStackBlocks(productionPass))
        {
            if (blocks.TryGetValue(block.StackId, out var existing))
            {
                var mergedLines = existing.Lines.Concat(block.Lines).ToList();
                blocks[block.StackId] = new OrderDraftParsing.ModuleStackBlock(block.StackId, mergedLines);
            }
            else
            {
                blocks[block.StackId] = block;
            }
        }

        var builder = new System.Text.StringBuilder();
        builder.AppendLine(preamble.TrimEnd());
        builder.AppendLine();
        foreach (var block in blocks.Values.OrderBy(b => b.StackId, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"#modulestack {block.StackId}");
            foreach (var line in block.Lines)
            {
                builder.AppendLine(line);
            }

            builder.AppendLine();
        }

        builder.AppendLine("#end");
        return builder.ToString().TrimEnd() + Environment.NewLine;
    }
}

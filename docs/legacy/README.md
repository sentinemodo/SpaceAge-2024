# Legacy documentation

Material here is **historical or superseded**. For live behavior use [`player/rules.md`](../../player/rules.md) and [`architecture/overview.md`](../../architecture/overview.md).

| Path | What it was | Superseded by |
|------|-------------|---------------|
| [`prompts/visual-tool-brief.txt`](prompts/visual-tool-brief.txt) | Original visual-tool vision | [ADR-0010](../../architecture/adr/ADR-0010-visual-tool.md), [`visual-tool/README.md`](../../visual-tool/README.md) |
| [`prompts/website-brief.txt`](prompts/website-brief.txt) | Original website brief | [ADR-0007](../../architecture/adr/ADR-0007-public-campaign-website.md), [`delivery/website.md`](../../architecture/delivery/website.md) |
| [`alderson/`](alderson/) | Alderson-era design text from `Game/documentation/` | Engine code + `player/` manuals |

Do not implement features from legacy docs without verifying against current C# and an ADR if the boundary changes.

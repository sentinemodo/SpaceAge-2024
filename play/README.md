# Campaign play scripts

PBEM helpers live here. The engine stays batch file-in/file-out ([ADR-0003](../architecture/adr/ADR-0003-filesystem-pbem-batch.md)); these scripts orchestrate folders under `play/runs/<id>/`.

## RAG refresh after isolate (Phase 4)

After `Game.exe` (or reports-only) and isolate copies `report.{turn}.{faction}.txt` into each `play/runs/<id>/factions/NN/` folder, refresh per-seat indexes **before** drafting the next turn:

```powershell
# All AI seats (factions 2–11), incremental: latest report + story + last 3 order turns
.\play\ingest-rag.ps1 -Run smoke-test -Mode campaign

# Plan only (no Ollama calls)
.\play\ingest-rag.ps1 -Run smoke-test -Mode campaign -DryRun

# Single seat (manual fallback)
dotnet run --project tools/player-agent/PlayerAgent.csproj -- `
  ingest-faction --mode campaign --run smoke-test --faction 2
```

Do **not** run `ingest-shared` on every turn unless `player/rules.md`, tech manuals, or `player/battle.md` changed ([Phase 5](../architecture/delivery/local-player-agent.md)).

### Campaign-ai handoff

When `/campaign-ai` updates `story.md` for a seat, re-ingest that story chunk before `draft`:

```powershell
dotnet run --project tools/player-agent/PlayerAgent.csproj -- `
  ingest-faction --mode campaign --run <id> --faction <n> --story-only
```

`/campaign-ai` continues to own strategy text; the runner consumes `story.md` as RAG + prompt-pack input.

## Turn encoding

UTF-8 order drafts under faction folders are converted to Windows-1251 before `Game.exe` (GM script or future `turn.ps1`). See [ADR-0002](../architecture/adr/ADR-0002-windows-1251-io.md).

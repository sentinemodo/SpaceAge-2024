# Campaign play scripts

AI isolation play loop for the 10-player campaign (factions **2–11**). This is not the public lobby website. Engine **0.1.148**.

The **campaign-gm** agent (`.cursor/agents/campaign-gm.md`) **executes** these scripts and keeps this README accurate. It does **not** write or patch `play/*.ps1`, C#, or tests. Missing automation is listed under [Gaps](#gaps) — implementers add scripts; the GM only documents and runs them.

## Build Game.exe (Windows)

From the repo root:

```
nuget restore SpaceAge.sln
msbuild SpaceAge.sln /p:Configuration=Debug
```

Output: `Game\bin\Debug\Game.exe` (`bin/` is gitignored). Scripts that invoke the engine default to that path. Pass `-Exe path\to\Game.exe` to override.

## Layout

`play/runs/` is gitignored because it holds **passwords + gamein**. After `init-run`:

```
play/runs/<id>/
  data/                          Game.exe /data
    data.xml                     copy of campaign/data.xml
    gamein.xml                   copy of campaign/gamein.1.xml with factions 2–11 passwords patched
  turn/                          Game.exe /turn-dir  (empty after init)
  factions/
    02/  persona.md
    …
    11/  persona.md
```

Later:

| When | Path | Notes |
|------|------|--------|
| `/reports` | `turn/report.1.{1–13}.txt` and `.xml` | Seed `turn="1"`; campaign factions have `xml-report="True"` |
| isolate | `factions/NN/report.{T}.{id}.txt` only | NN = `02`–`11`; never `.xml`; never 1/12/13 |
| `/player` draft | `factions/NN/order.{id}.txt` | UTF-8 in the faction folder |
| campaign-ai | `factions/NN/story.md` | Plan: review + strategic (system, 4q) + tactical (planet/moon, 1q) + win (galaxy, T≥10) |
| `turn.ps1` | `turn/order.{id}.txt` | Windows-1251 copies of the ten player files |
| full exe | `data/gameout.{N}.xml`, `turn/report.{N}.*` | Seed 1 → N = **2** |
| `next.ps1` | overwrite `data/gamein.xml` from `data/gameout.{N}.xml` | Keep all `gameout.*` |

Never put `gamein.xml`, `gameout*.xml`, `campaign/gamein.1.xml`, or any `report.*.xml` in a faction folder. Never create `factions/01`, `12`, or `13`. Never point `/data` at `campaign/`.

## Exact Game.exe lines

From the repo root (`<id>` is the run id):

```
Game\bin\Debug\Game.exe /data play\runs\<id>\data /turn-dir play\runs\<id>\turn /reports
Game\bin\Debug\Game.exe /data play\runs\<id>\data /turn-dir play\runs\<id>\turn
Game\bin\Debug\Game.exe /data play\runs\<id>\data /turn-dir play\runs\<id>\turn /no-turn
```

- `/reports` — load + `GenerateReports`; no `Execute`; no `SaveGame`. Seed 1 → `report.1.*`.
- Full run — `turn++` first. Seed 1 → `report.2.*` + `data\gameout.2.xml`.
- `/no-turn` — between-turn `CONTRACT` / `PRESS` only (no `turn++`). Not wrapped by a `play/*.ps1` yet; GM may run this line. `/check` is unused.

## Script examples

From the repo root (`powershell -NoProfile -File` if execution policy blocks `.\`):

```
.\play\init-run.ps1 demo
.\play\init-run.ps1 demo -Seed 20260829 -Force
.\play\reports.ps1 demo
.\play\isolate.ps1 demo
.\play\turn.ps1 demo
.\play\next.ps1 demo
```

Optional `-Exe path\to\Game.exe` on `reports.ps1` / `turn.ps1`. `init-run.ps1` accepts `-Exe` for consistency but does not launch the engine.

Typical first quarter: `init-run` → `reports` → `isolate` → (ten `order.{id}.txt` in faction folders) → `turn` → `next`.

Shared helpers live in `play/_common.ps1` (dot-sourced; not invoked directly).

## GM operations

Invoke `/campaign-gm` to run a table. GM loop (same mermaid as [campaign-play.md](../architecture/delivery/campaign-play.md)):

```
init-run → reports → isolate → ten isolated campaign-ai (or /player) → turn → win check → next
```

Optional **between-turn** (UN contracts and press) uses `/no-turn` **before** collecting player orders or after `next`, never mixed with leftover `turn/order.{2–11}.txt` (the engine globs all `order.*`).

### Between-turn (`/no-turn`)

There is no `play/no-turn.ps1`. Until one exists, from repo root:

```
Game\bin\Debug\Game.exe /data play\runs\<id>\data /turn-dir play\runs\<id>\turn /no-turn
```

Stage only GM/UN orders (typically `turn\order.1.txt`, Windows-1251, `#faction 1 ""` then `CONTRACT` / `PRESS` per `player/rules.md`). Flavour and contract ids come from `/game-designer` + `/player` — the GM does not invent them. `/no-turn` does not increment the turn; `SaveGame` writes `data\gameout.{currentTurn}.xml`. Promote with `.\play\next.ps1 <id> -Turn <currentTurn>` when that file is the snapshot you want as the next `gamein.xml`.

New wrecks or receivers that are not live `CONTRACT` orders: `/game-designer` patches the **run** `data\gamein.xml` only, not committed `campaign/gamein.1.xml`.

### Win (crude, from text reports)

- **Solitary:** one player `corphq` left among factions 2–11.
- **Bloc:** surviving players have **mutual** `DECLARE FACTION <id> ALLY` (one-way does not count) and every other player HQ is gone.

NPC 1 / 12 / 13 do not decide this win.

### AI players (`/campaign-ai`)

One call per faction **2–11**. Workspace is `factions/NN/` plus `player/rules.md` (and `player/campaign/basic_technologies.md` when it exists). Each call reviews the previous `story.md` against this report, writes a new story (**strategic** = 4 quarters / system, **tactical** = next quarter / planet or moon, **win** = galaxy-wide when report turn ≥ 10), then invokes `/player` with the **tactical** objective (catalog **path** `campaign/data.xml` — campaign-ai does **not** open that XML). TDD or designer gaps wait for **human approval**. Do not feed catalog XML, `gamein.xml`, XML reports, or other factions’ files.

## Encoding

- Catalog / gamein / reports / `/turn-dir` orders: **Windows-1251**.
- `/player` drafts UTF-8 in `factions/NN/order.{id}.txt`; `turn.ps1` writes 1251 into `turn\`.
- `persona.md`: UTF-8.
- `story.md`: UTF-8 (campaign-ai).

Orders header: `#faction <id> "<password>"` (see `player/rules.md`).

## Isolation

Text reports only into `factions/02`–`11`. No XML reports. No faction 1/12/13 folders. No gamein/gameout in a faction folder.

## NPC orders

Factions **1 / 12 / 13** submit no `order.*` until a later GM/raid slice.

## Passwords

Generated at init (10 ASCII characters, no quotes or backslashes); stored in the **run** `gamein.xml` and each `persona.md`. Committed `campaign/gamein.1.xml` placeholders are **not** live. Do not publish run passwords in this README.

## Gaps

Not scripts yet (GM documents and may run the documented `Game.exe` line; GM does **not** add these files):

| Need | Notes |
|------|--------|
| `play/no-turn.ps1` | Wrap `/no-turn` + UN `order.1.txt` staging + optional `next` |
| NPC 12/13 orders on a full turn | `turn.ps1` copies factions 2–11 only (raids / hostility-flip later) |
| `website/public/status.json` | Lobby Phase 2; not this isolation path |

Do not point `/data` at `campaign/`. Do not commit `play/runs/`.

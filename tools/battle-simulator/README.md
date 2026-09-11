# SpaceAge Battle Simulator (standalone)

Local planning tool that configures two-sided rosters and runs combat through the real engine:

```text
Game.exe /battle-sim <sim-input.xml> [output.txt] /data campaign [/seed N]
```

Catalog presets use module and item ids from [`campaign/data.xml`](../../campaign/data.xml). The UI modules under `public/js/` are written to be embedded later in the visual tool client.

## Prerequisites

1. Build the engine: `Game/bin/Debug/Game.exe` (Debug configuration).
2. Node.js 18+.

## Run

```powershell
cd tools/battle-simulator
npm start
```

Open http://localhost:4173

## Layout

| Path | Purpose |
|------|---------|
| `public/index.html` | Two-side roster UI |
| `public/js/sim-xml.js` | XML builder (reuse in visual tool) |
| `public/js/storage.js` | Custom template persistence (`localStorage`) |
| `public/presets.json` | Preset catalog (keep in sync with `Game/battle/BattleSimulatorTemplates.cs`) |
| `server/bridge.mjs` | Local HTTP bridge to `Game.exe` |

## CLI without UI

```powershell
Game.exe /battle-sim Tests\fixtures\battle-sim\inftry-skirmish.xml /data campaign /seed 42
```

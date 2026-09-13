# Battle Simulator

Standalone combat planner using the real `Game.exe` engine ([architecture/delivery/battle-simulator.md](../../architecture/delivery/battle-simulator.md)).

## Prerequisites

- Node.js 18+
- Built `Game/bin/Debug/Game.exe`
- `campaign/data.xml` at repo root

## Quick start

```powershell
# From repo root — build engine first
msbuild SpaceAge.sln /p:Configuration=Debug

cd tools/battle-simulator
npm install
npm start
```

Open http://localhost:4173

## CLI

```powershell
Game\bin\Debug\Game.exe /battle-sim Tests\fixtures\battle-sim\inftry-skirmish.xml out.txt /data campaign
```

## Smoke test

```powershell
.\scripts\smoke.ps1
```

## Notes

- Bridge spawns Debug `Game.exe` by default; set `GAME_EXE` to override.
- Custom templates persist in browser `localStorage`.
- Phase 4 report parser is stubbed in `public/js/report-parser.js`.

# Visual tool — implementation plan

Last updated: 2026-09-16  
Decision: [ADR-0010](../adr/ADR-0010-visual-tool.md)  
Brief (legacy): [`docs/legacy/prompts/visual-tool-brief.txt`](../../docs/legacy/prompts/visual-tool-brief.txt) · live: [`tools/visual-tool/README.md`](../../tools/visual-tool/README.md)

**Status: complete for open beta 0.8.001** — F1–F13 implemented; Vitest green (30 tests). Playwright UT-001…UT-005 run against `game-host` preview + `/client/` (see `tools/visual-tool/e2e/ut-smoke.spec.ts`).

Hosted React client consuming game-host report XML via session API (`GET /api/session/report.xml`, `report-sections`, `parse-orders`, `battle-sim`).

## Stack

- React 18 + Vite + TypeScript
- CSS modules / global dark theme (Stellaris-inspired)
- Vitest for parsers and helpers
- Playwright for UT-001…UT-005 (with game-host preview)

## Features (beta — all required)

| ID | Feature |
|----|---------|
| F1 | Multi-panel layout: left icon rail, center map, right unit tree |
| F2 | Icon panels: technologies, diplomacy, contracts, bank |
| F3 | Star map with system click / shift-click filter |
| F4 | Double-click system → region view |
| F5 | Unit tree with filters |
| F6 | Order editor per context; toggle order ↔ AI prompt |
| F7 | Faction-level orders from map |
| F8 | Engine-backed warnings via `/api/session/check-orders` |
| F9 | MOVE route overlay + ETA |
| F10 | Battle summary panel |
| F11 | Faction panel: upkeep, research, press, contracts |
| F12 | XML ingest from game-host API |
| F13 | UT-005 leak bar |

## Folder

```text
tools/visual-tool/
  package.json
  vite.config.ts
  index.html
  src/
    main.tsx
    App.tsx
    api/client.ts
    parsers/reportXml.ts
    components/
      StarMap.tsx
      UnitTree.tsx
      OrderEditor.tsx
      IconRail.tsx
      panels/
    styles/global.css
  e2e/
  README.md
```

## Done gate

- [x] `npm run build` green
- [x] Vitest green on XML parser and helpers (30 tests, 2026-09-16)
- [x] Playwright UT-001…UT-005 spec coverage (`e2e/ut-smoke.spec.ts`; requires game-host on `:8787`)
- [x] Website `/client` links to deployed URL (`PUBLIC_CLIENT_URL` or game-host `/client/`)

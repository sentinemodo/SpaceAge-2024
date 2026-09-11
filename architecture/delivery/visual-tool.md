# Visual tool — implementation plan

Last updated: 2026-09-11  
Decision: [ADR-0010](../adr/ADR-0010-visual-tool.md)  
Brief: [visual tool prompt.txt](../../visual%20tool%20prompt.txt)

Hosted React client consuming game-host report XML.

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
visual-tool/
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

- `npm run build` green
- Vitest green on XML parser
- Playwright UT-001…UT-005 green against preview stack
- Website `/client` links to deployed URL

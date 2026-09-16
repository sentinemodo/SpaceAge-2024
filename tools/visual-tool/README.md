# SpaceAge Visual Tool

Complete hosted report client for open beta.

Decision: [docs/architecture/adr/ADR-0010-visual-tool.md](../docs/architecture/adr/ADR-0010-visual-tool.md)

## Development

```bash
cd visual-tool
npm install
npm run dev
```

Proxy targets game-host at `localhost:8787`.

## Production

Build and serve from game-host:

```bash
npm run build
cd ../game-host
npm start
# Open http://localhost:8787/client/
```

## Features

- Authenticated session (faction password from report `#faction` header)
- **XML** report ingest from `GET /api/session/report.xml` (map, units, orders)
- **Text report sections** from `GET /api/session/report-sections` (engine-formatted; client does not reformat)
- Star map with system filter (click / shift-click) and region drill-down
- Unit tree, order editor, AI prompt toggle
- Engine-backed order parse via `POST /api/session/parse-orders`
- Battle simulator via `POST /api/session/battle-sim` (Game.exe `/battle-sim`)
- MOVE route from XML orders + ETA estimate on selected unit

## Tests

```bash
npm test
npm run test:e2e   # requires game-host + playwright
```

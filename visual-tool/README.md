# SpaceAge Visual Tool

Complete hosted report client for open beta.

Decision: [architecture/adr/ADR-0010-visual-tool.md](../architecture/adr/ADR-0010-visual-tool.md)

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

- XML report ingest from game-host API
- Star map with system filter (click / shift-click)
- Unit tree, order editor, AI prompt toggle
- Engine-backed order warnings via `/api/session/check-orders`
- Technology, diplomacy, contracts, bank, battle, faction panels
- MOVE ETA estimate on selected unit

## Tests

```bash
npm test
npm run test:e2e   # requires game-host + playwright
```

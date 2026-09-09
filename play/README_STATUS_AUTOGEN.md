Status JSON generator

This document explains how to run the automated status.json generator and local tests.

Requirements
- PowerShell (Windows PowerShell or PowerShell Core/pwsh) — optional on Linux/macOS if using the Node fallback
- Node.js 18+ and npm 9+ (for website tests)

Generate status.json for a run
1. From repo root, run:
   powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File .\play\generate-status.ps1 <RunId>
   or
   pwsh -NoProfile -NonInteractive -ExecutionPolicy Bypass -File ./play/generate-status.ps1 <RunId>
   or (CI / Linux fallback)
   node website/scripts/generate-status.mjs <RunId>

   Example: pwsh -File ./play/generate-status.ps1 demo
   This writes website/public/status.json with the current turn, per-faction submission state, and lobby status.

   Reads:
   - play/runs/<RunId>/data/gamein.xml — turn from `<game turn="N">`
   - play/runs/<RunId>/factions/NN/order.{id}.txt — player draft submissions
   - play/runs/<RunId>/factions/NN/report.{T}.{id}.txt — isolated reports (`reports-out`)
   - play/runs/<RunId>/gm/schedule.json — optional `nextTurnAt` deadline

Automatic updates
- isolate.ps1, next.ps1, and turn.ps1 call generate-status at the end of each step.
- turn.ps1 removes consumed faction order drafts after isolate so status returns to reports-out.

Optional deadline
Create play/runs/<RunId>/gm/schedule.json:
```json
{ "nextTurnAt": "2026-09-15T23:59:59Z" }
```

Run website checks and tests
1. cd website
2. npm install
3. npm run check      # TypeScript + astro check
4. npm test           # Vitest unit tests (includes status-schema and status-producer integration test)
5. npm run build
6. npm run preview
7. npm run test:e2e   # Playwright e2e tests against preview (in separate terminal)

CI
- `.github/workflows/website.yml` runs Vitest on Ubuntu (Node fallback) and Windows (PowerShell producer).

Notes
- The integration test in website/tests/status-producer.test.ts uses PowerShell when available, otherwise the Node script.
- Keep play/generate-status.ps1 and website/scripts/generate-status.mjs logic in sync.

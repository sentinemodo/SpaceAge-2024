Status JSON generator

This document explains how to run the automated status.json generator and local tests.

Requirements
- PowerShell (Windows PowerShell or PowerShell Core/pwsh)
- Node.js 18+ and npm 9+ (for website tests)

Generate status.json for a run
1. From repo root, run:
   powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File .\play\generate-status.ps1 <RunId>
   or
   pwsh -NoProfile -NonInteractive -ExecutionPolicy Bypass -File ./play/generate-status.ps1 <RunId>

   Example: pwsh -File ./play/generate-status.ps1 test1
   This writes website/public/status.json with the current turn and per-faction submission state (reads play/runs/<RunId>/data/gamein.xml and play/runs/<RunId>/turn/order.<id>.txt).

Run website checks and tests
1. cd website
2. npm install
3. npm run check      # TypeScript + astro check
4. npm test           # Vitest unit tests (includes status-schema and status-producer integration test)
5. npm run build
6. npm run preview
7. npm run test:e2e   # Playwright e2e tests against preview (in separate terminal)

Notes
- The integration test in website/tests/status-producer.test.ts will skip if no PowerShell executable is available in PATH.
- If CI needs to run the PS script, ensure a PowerShell runner is available or adapt the test to use a Node-based generator for CI.

Changes introduced for Phase 2
- play/generate-status.ps1: new script that emits website/public/status.json
- website/tests/status-producer.test.ts: integration test that validates generation
- website/src/components/StatusDashboard.astro: client-side fetch of /status.json and live rendering


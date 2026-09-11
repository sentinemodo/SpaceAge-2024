# ADR-0010: Visual tool (hosted report client)

Date: 2026-09-11  
Status: **Accepted**  
Plan: [`../delivery/visual-tool.md`](../delivery/visual-tool.md)  
Product brief: [`../../visual tool prompt.txt`](../../visual%20tool%20prompt.txt)

## Context

Open beta requires a **complete** Stellaris-inspired client for browsing quarterly reports, editing orders, and submitting them to the game-host. [ADR-0007](ADR-0007-public-campaign-website.md) keeps the Astro lobby separate; this is a **fourth** bounded context (after Engine, Website, Game Host).

## Decision

1. **Folder:** `visual-tool/` at repo root. React 18 + Vite + TypeScript. Not inside `website/` or `Game/`.
2. **Deployment:** Built static assets served from game-host same origin (`/client/` proxy) or separate static host with CORS to game-host API.
3. **Data path:** Primary input is **`report.*.xml`** from game-host `GET /api/session/report.xml`. Text reports are debug fallback only.
4. **Validation:** Order warnings via game-host `POST /api/session/check-orders` (engine-backed), not client-side guesswork.
5. **Scope for beta:** Full checklist in [visual-tool.md](../delivery/visual-tool.md) § Features — star map, icon rail, unit tree, order/AI-prompt editors, MOVE routes, battle summary, faction panel, UT-001…UT-005.
6. **Mobile:** Responsive layout; touch-friendly filters and panels.
7. **Secrets:** No passwords in bundle; session via game-host auth only.

## Options considered

| Option | Decision | Why |
|--------|----------|-----|
| Implement inside Astro `website/` | Rejected | ADR-0007; lobby stays static |
| Text-report-only MVP | Rejected | Open beta requires XML + complete prompt |
| Local-only Electron app | Rejected | Beta requires hosted play |
| Parse full `gamein.xml` in browser | Rejected | UT-005; use report XML only |

## Consequences

- Website Phase 3 `/client` CTA links to deployed visual tool URL.
- Playwright UT-* scenarios run against `visual-tool` + `game-host` preview stack.
- Shared design tokens with lobby optional; not required for beta.

## Revision

- 2026-09-11: Accepted for open beta.

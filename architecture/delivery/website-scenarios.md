# Public lobby — acceptance scenario catalog (seed)

Last updated: 2026-08-29  
Owned by: **`/website-tester`** (defines, maintains, automates).  
Plan: [`website.md`](website.md) · Decision: [ADR-0007](../adr/ADR-0007-public-campaign-website.md)

This file is the **seed** catalog. Until `website/` exists it is the source of truth for acceptance. At Phase 1 the tester **moves or copies** it to **`website/e2e/scenarios.md`**. After that move, **`website/e2e/scenarios.md` is canonical**; this architecture file becomes a pointer plus a dated snapshot note (do not maintain two live catalogs).

**Manual browser exploration is not the acceptance path.** Every scenario below is automated: Vitest for schema/helpers, Playwright Chromium against `astro build` + `astro preview` for routes and copy. Green e2e is the done gate.

User-tool (visual-tool) scenarios are a **reserved section**. Do not implement that app from this catalog.

## Catalog conventions

Each scenario has:

| Field | Meaning |
|-------|---------|
| **id** | Stable id (`WS-###` lobby, `UT-###` user tools). Do not renumber; add or deprecate. |
| **user goal** | What a visitor or player is trying to do |
| **route(s)** | Public path(s) |
| **given / when / then** | Observable acceptance |
| **layer** | `Vitest` (schema/helpers) and/or `Playwright` (browser vs preview). Not NUnit. |
| **security** | Leak check if the surface could expose secrets |

Playwright specs **cite the scenario id** in the title or annotation (`WS-001`, …). If a spec has no catalog row, the tester adds the row **before** treating the spec as acceptance.

## Initial lobby scenarios

### WS-001 — Home flavour and required credits

| Field | Value |
|-------|--------|
| **id** | `WS-001` |
| **user goal** | Understand what SpaceAge is and that it sits in the Atlantis / Rise of Heroes / Archer family — without a rewritten origin myth |
| **route(s)** | `/` |
| **given** | Phase 1 site is built; `astro preview` is serving the static build |
| **when** | A visitor opens the home page |
| **then** | Visible **attribution** (not footer-only) names **Atlantis**, **Rise of Heroes**, and **Vincent Archer**; a link to [https://overlord.sourceforge.net/](https://overlord.sourceforge.net/) is present; flavour includes the Alderson lead-in, weak points / Drive / 750 light-years, Points shutdown, and **two Points to Earth stayed dark** (Rules.txt §§1, 1.1 condensed per [`website.md`](website.md)). Copy is an excerpt, not a new myth |
| **layer** | **Playwright** (string presence). Vitest is not required for prose. |
| **security** | Home HTML must not contain passwords, `gamein`, `order.`, or `report.` paths |

### WS-002 — Closed lobby (no join / signup)

| Field | Value |
|-------|--------|
| **id** | `WS-002` |
| **user goal** | See that this campaign is invitation-only; not a signup mill |
| **route(s)** | `/` (and shared nav/footer on `/client`, `/turns`, `/rules`) |
| **given** | Closed 10-player campaign (factions 2–11) |
| **when** | A visitor looks for a way to create an account or join |
| **then** | Home states the campaign is **invitation-only** / closed. **No** “Join Game Now”, sign-up form, password-reset form, or account-creation control. Primary CTA is Client (`/client`), not enroll |
| **layer** | **Playwright** (absent controls + closed-lobby sentence) |
| **security** | No auth cookies, no credential fields |

### WS-003 — Status dashboard from `/status.json`

| Field | Value |
|-------|--------|
| **id** | `WS-003` |
| **user goal** | See live-enough campaign status without calling `Game.exe` |
| **route(s)** | `/`, `/turns`; data `GET /status.json` |
| **given** | Committed placeholder `website/public/status.json` (Phase 1) matching schema v1 in [`website.md`](website.md) |
| **when** | Home and Turns load and fetch `/status.json` |
| **then** | Dashboard shows `status` (one of `not-started` \| `accepting-orders` \| `processing` \| `reports-out`), turn number, **10** player seats, and next-turn as countdown **or** “GM-scheduled” when `nextTurnAt` is null. Values come from the JSON file, not from parsed `gamein.xml` |
| **layer** | **Vitest**: schema (exactly factions 2–11, allowed `status` enum, **no** password/email/path keys). **Playwright**: dashboard widgets reflect the published JSON |
| **security** | JSON body and rendered page: no passwords, emails, `gamein`, `order.`, or `report.` paths |

### WS-004 — Players & Turns: ten seats, factions 2–11 only

| Field | Value |
|-------|--------|
| **id** | `WS-004` |
| **user goal** | See who has submitted orders this turn — players only |
| **route(s)** | `/turns` |
| **given** | Placeholder JSON lists factions **2–11** with `ordersSubmitted` booleans |
| **when** | A visitor opens Players & Turns |
| **then** | Exactly **ten** seats. Faction ids **2 through 11** only. NPC **1 / 12 / 13** do **not** appear as rows. Each row is submitted / missing only (no report bodies, no order text) |
| **layer** | **Vitest**: `factions` length 10, ids 2–11, no 1/12/13. **Playwright**: table/list shows ten seats and no NPC ids |
| **security** | No order-file contents, no report excerpts, no passwords |

### WS-005 — Game Client placeholder, then live href

| Field | Value |
|-------|--------|
| **id** | `WS-005` |
| **user goal** | Find the visual tool when it exists; until then, understand it is coming |
| **route(s)** | `/client` (CTA from `/`) |
| **given** | Phase 1–2: tool not built. Phase 3: href documented in `website/` README |
| **when** | A visitor opens `/client` or follows the home Client CTA |
| **then** | **Phases 1–2:** page explains purpose; “not built yet” / coming later; CTA is `#`, disabled, or an in-page anchor — **not a 404**. **Phase 3:** CTA is a live href (`/visual-tool/` same origin **or** the documented absolute URL). The lobby does **not** embed the Stellaris-style client |
| **layer** | **Playwright**. Phase 3 updates this scenario’s `then` when the href is chosen; do not invent a host |
| **security** | Client page does not host `gamein`, reports, or order files |

### WS-006 — Rules: short principles, not the rulebook

| Field | Value |
|-------|--------|
| **id** | `WS-006` |
| **user goal** | Learn Open PBEM / Interests / quarterly reports in this engine’s terms |
| **route(s)** | `/rules` |
| **given** | Flavour source is Rules.txt §2.1 rewritten in SpaceAge file-in/file-out terms ([`website.md`](website.md)) |
| **when** | A visitor opens `/rules` |
| **then** | Short principles only: Open PBEM, Interests (factions 2–11), one turn = one in-game quarter (13 weeks), reports then order files. **Must not** paste the full Rules.txt / `player/rules.md` book. May link out to `player/rules.md` when published |
| **layer** | **Playwright** (principles present; page is not a dump of the rulebook — e.g. no multi-thousand-word paste, no full order-syntax manual) |
| **security** | No live order files or report bodies |

### WS-007 — Mobile nav and cards

| Field | Value |
|-------|--------|
| **id** | `WS-007` |
| **user goal** | Use the lobby on a phone-width viewport |
| **route(s)** | `/`, `/client`, `/turns`, `/rules` |
| **given** | Shared dark nav, light cards, footer |
| **when** | Chromium viewport is **390px** wide (MVP mobile width) |
| **then** | Nav collapses (hamburger or equivalent); cards **stack** (not a cramped multi-column dashboard). All four routes remain reachable. Dashboard 2×2 becomes a single column |
| **layer** | **Playwright** (`page.setViewportSize({ width: 390, height: 844 })` or project config). Not a visual-regression suite |
| **security** | Same leak bar as WS-008 on the HTML |

### WS-008 — No secrets on public pages or status JSON

| Field | Value |
|-------|--------|
| **id** | `WS-008` |
| **user goal** | Observers can use the lobby without seeing foreign intel or credentials |
| **route(s)** | `/`, `/client`, `/turns`, `/rules`, `/status.json` |
| **given** | Placeholder or play-script `status.json`; static pages |
| **when** | Playwright fetches each route and the JSON |
| **then** | Response bodies **never** contain passwords, `gamein`, `order.`, or `report.` path/filename leaks. JSON is an allow-list (schema v1 only). NPC foreign reports are absent |
| **layer** | **Vitest** on fixture/schema (forbidden keys). **Playwright** on published HTML + `/status.json` |
| **security** | This **is** the security scenario. Fail closed |

### WS-009 — Four public routes and shared chrome

| Field | Value |
|-------|--------|
| **id** | `WS-009` |
| **user goal** | Reach Home, Client, Turns, and Rules from the shared nav |
| **route(s)** | `/`, `/client`, `/turns`, `/rules` |
| **given** | Phase 1 information architecture |
| **when** | A visitor uses the top nav (desktop) |
| **then** | All four routes return success (no 404). Shared dark nav + footer (engine version placeholder, start date, schedule). No extra public routes that serve game files |
| **layer** | **Playwright** smoke |
| **security** | Nav must not link to `gamein.xml`, `data.xml`, order files, or reports |

## Reserved — user tools (visual tool and later player-facing web apps)

**Do not implement** the visual tool from this section. Product brief: [`visual tool prompt.txt`](../../visual%20tool%20prompt.txt) (Stellaris-inspired report/XML client: star map, unit tree, order editing, warnings). It is a **separate folder and host**, not pages inside `website/`.

`/website-tester` **owns** these scenarios when that app exists: add `UT-###` rows, Playwright under that app’s `e2e/`, same architect-compliance gate as the lobby ([`website.md`](website.md) — Cursor agents and test pairing). Same rule: **no manual exploration as acceptance**.

| id | Placeholder user goal | Status |
|----|----------------------|--------|
| `UT-001` | Browse a report as a multi-window client (faction / map / unit) | Reserved — app not in repo |
| `UT-002` | See order warnings (wrong region, missing tech, inoperable move, …) | Reserved |
| `UT-003` | Star map + unit tree filters; route preview for a MOVE | Reserved |
| `UT-004` | Mobile-friendly tool chrome | Reserved |
| `UT-005` | Tool never uploads/serves `gamein` passwords or foreign reports on a public origin | Reserved |

When the architect names the tool folder, add its `e2e/**` glob to `.cursor/rules/website-tester.mdc` and move live `UT-*` rows into that app’s scenario file (or keep one catalog with a User tools chapter). Until then, keep this reserved table only.

## Reserved — Phase 4 lobby tools (`/eta`, `/battle`)

Do **not** implement these pages in Phase 1. Specs stay reserved until `/website-developer` ships the islands. Product brief: [`website.md`](website.md) **Phase 4 — Player tools**.

### WS-010 — Transit ETA from ship report + two AU

| Field | Value |
|-------|--------|
| **id** | `WS-010` |
| **user goal** | Estimate MOVE weeks from own-ship mass/thrust and two orbital radii |
| **route(s)** | `/eta` (CTA from home Tools card) |
| **given** | Phase 4 site is built; a sample paste includes `mass: 40000/4150`; origin AU 1.0 and destination AU 80; drive speed 1 |
| **when** | The visitor pastes the excerpt, sets the two AU-from-star fields, and calculates |
| **then** | The page shows ΔAU **79** and **14** weeks (default workshop frigate, speed 1). Scout paste `mass: 40000/2430` at the same hop shows **10** weeks. Parse failure (no `mass: thrust/mass`) shows an error and allows manual thrust/mass. Page states the next engine turn is authoritative |
| **layer** | **Vitest**: `DurationWeeks` + mass-factor clamp vs [`designer/au-transit.md`](../../designer/au-transit.md) locked table. **Playwright**: sample paste + two AU → 14 weeks |
| **security** | Paste is not submitted to a server. HTML + `/status.json` still have no `report.` / `gamein` / `order.` files |

### WS-011 — Two-side battle what-if

| Field | Value |
|-------|--------|
| **id** | `WS-011` |
| **user goal** | Play out a fight after entering units and tactics on both sides |
| **route(s)** | `/battle` |
| **given** | Phase 4 site; each side has at least one combatant (attack, defense, damage, HP, tactic) and a numeric seed |
| **when** | The visitor runs the simulation |
| **then** | A round log appears (fire / chance / hit or miss / wreck or capture) and an end line: attackers win, defenders win, or indecisive. Same seed + same roster reproduces the log. Page states this is a what-if, not `Game.exe` |
| **layer** | **Vitest**: to-hit / damage / evade-leave helpers from [`player/battle.md`](../../player/battle.md). **Playwright**: submit a minimal two-unit roster → finished log + end line |
| **security** | No upload of rosters. No `data.xml` / `gamein` on the origin |

### WS-012 — Phase 4 tools do not publish reports

| Field | Value |
|-------|--------|
| **id** | `WS-012` |
| **user goal** | Use the calculators without the site becoming a report host |
| **route(s)** | `/eta`, `/battle`, `/status.json` |
| **given** | Phase 4 pages exist |
| **when** | Playwright fetches the routes and JSON |
| **then** | No `report.` / `gamein` / `order.` / `data.xml` files are served. Default page HTML contains no sample password or `#faction` credential line. WS-008 still holds on the Phase 1 routes |
| **layer** | **Playwright** (and Vitest if a combat-stats excerpt JSON is added — allow-list keys only) |
| **security** | This is the tools leak bar. Fail closed |

WS-007 / WS-009 stay Phase 1 (four routes). When Phase 4 ships, extend those rows (or add a follow-up id) so nav + 390px cover `/eta` and `/battle` — do not renumber.

## Revision

- 2026-08-29: Seed catalog for `/website-tester`. Canonical path after Phase 1: `website/e2e/scenarios.md`.
- 2026-08-29: Reserved WS-010…WS-012 for Phase 4 `/eta` and `/battle`.

# SpaceAge Website

A static Astro site for the SpaceAge PBEM campaign lobby.

**Location:** `website/` at the repository root (separate from `Game/`, `Tests/`, `campaign/`, `play/`)

## Overview

This is the public-facing website for SpaceAge, a play-by-email galactic strategy game. The site provides:

- **Home page** — Campaign overview, flavour, and current status dashboard
- **`/client`** — Information about the visual tool client (Phase 3+)
- **`/turns`** — Faction orders-submission status and turn information
- **`/rules`** — Quick rules overview and link to full player rules

**Status data** is fed via a UTF-8 JSON file (`public/status.json`), never by calling `Game.exe`.

## Stack

- **Framework:** Astro 4.x (static output, `output: 'static'`)
- **Language:** HTML/Astro templates + hand-written CSS + TypeScript for small islands
- **Testing:**
  - `astro check` — TypeScript validation
  - **Vitest** — Unit tests (status schema, helpers)
  - **Playwright** — E2E tests against built site (`astro build` + `astro preview`)
- **Hosting:** GitHub Pages (or Cloudflare Pages / Netlify)

**No:** SSR, React/Vue (unless architect-approved Phase 4), `Game.exe` imports, Windows-1251 XML in Node, auth, cookies.

## Project Structure

```
website/
├── src/
│   ├── pages/           # Astro pages (/ /client /turns /rules)
│   │   ├── index.astro  # Home
│   │   ├── client.astro
│   │   ├── turns.astro
│   │   └── rules.astro
│   ├── layouts/
│   │   └── Layout.astro # Base layout + nav + footer
│   ├── components/
│   │   └── StatusDashboard.astro # Reads status.json
│   └── styles/
│       └── global.css   # Design tokens, dark/light theme
├── public/
│   └── status.json      # Faction status (UTF-8, schema-validated)
├── e2e/
│   └── smoke.spec.ts    # Playwright tests (Phase 1: WS-001…WS-004)
├── tests/
│   └── status-schema.test.ts # Vitest unit tests
├── astro.config.mjs     # Astro config (static output)
├── tsconfig.json        # TypeScript
├── vitest.config.ts     # Vitest
├── playwright.config.ts # Playwright (testWebServer: astro preview)
├── package.json         # npm scripts + dependencies
└── README.md            # This file
```

## Installation & Development

### Prerequisites

- Node.js 18+ and npm 9+
- PowerShell (for GM scripts that write `status.json`; not needed for dev)

### Setup

```bash
cd website
npm install
```

### Development Commands

| Command | What it does |
|---------|-------------|
| `npm run dev` | Start Astro dev server (hot reload) |
| `npm run build` | Build static site to `dist/` |
| `npm run preview` | Serve built site locally (for testing) |
| `npm run check` | TypeScript check + `astro check` |
| `npm test` | Run Vitest unit tests |
| `npm run test:watch` | Run Vitest in watch mode |
| `npm run test:e2e` | Run Playwright tests (requires built site) |

### Typical Workflow

```bash
# 1. Make changes
npm run dev           # Dev server watches for changes

# 2. Validate
npm run check         # TypeScript + Astro check
npm test              # Unit tests

# 3. Build & test
npm run build         # Build to dist/
npm run preview       # Serve dist/ locally
npm run test:e2e      # E2E tests against preview

# 4. Commit
git add .
git commit -m "feat: add new page"
git push origin copilot/website
```

## Design & Styling

**Theme:** Dark space aesthetic (deep navy bg, teal accents, readable light text on cards).

All styles are in `src/styles/global.css` and scoped `<style>` blocks in `.astro` components.

**Design tokens:**
- Colors: `--color-bg-dark`, `--color-card-bg`, `--color-accent-teal`, `--color-accent-amber`, etc.
- Spacing: `--space-sm`, `--space-md`, `--space-lg`, `--space-xl`, `--space-2xl`
- Typography: `--font-sans`, `--font-size-*`, `--font-weight-*`

Customize these in `global.css` `:root`.

## Status JSON

The file `public/status.json` drives the dashboard on the home page. Schema:

```json
{
  "status": "not-started|accepting-orders|processing|reports-out",
  "turn": 1,
  "nextTurnAt": "2026-09-06T23:59:59Z" or null,
  "factions": [
	{ "id": 2, "submitted": false, "name": "Faction 2" },
	{ "id": 3, "submitted": true, "name": "Faction 3" },
	...
  ]
}
```

**Rules:**
- Exactly 10 factions (ids 2–11). NPC factions (1, 12, 13) never appear.
- No passwords, emails, or paths.
- Updated by `play/` PowerShell scripts or manually by the GM.
- Validated by Vitest unit tests (`tests/status-schema.test.ts`).

## Testing

### Vitest (Unit Tests)

```bash
npm test
```

Tests live in `tests/` and validate:
- Status JSON schema (exactly factions 2–11, no secrets, valid enum)
- Countdown formatting
- Any TypeScript helpers

### Playwright (E2E Tests)

```bash
npm run build
npm run preview  # In one terminal
npm run test:e2e # In another terminal
```

**Phase 1 scenario catalog** (IDs in `e2e/smoke.spec.ts`):

- **WS-001:** Home page exists and is accessible
- **WS-002:** Home page includes Atlantis, Rise of Heroes, Vincent Archer credits
- **WS-003:** All routes (/, /client, /turns, /rules) exist and return 200
- **WS-004:** Navigation menu links work and pages load

Playwright runs against built site (`http://localhost:4321`) after `astro build` + `astro preview`.
Do **not** test against the dev server.

## Phase 1 Deliverables

✅ **Static pages:** `/`, `/client`, `/turns`, `/rules`  
✅ **Navigation:** Working links between pages  
✅ **Attribution:** Atlantis, Rise of Heroes, Vincent Archer credited on home  
✅ **Status dashboard:** Placeholder with 10 faction slots  
✅ **Vitest:** Schema validation (green)  
✅ **Playwright:** Four routes + credits + nav (green)  
✅ **Build:** `astro build` + `astro preview` + `npm run test:e2e` all green  

## Deployment

### URL (decision)

**Production:** GitHub **project Pages** at [https://sentinemodo.github.io/SpaceAge-2024/](https://sentinemodo.github.io/SpaceAge-2024/)

- Repo: `sentinemodo/SpaceAge-2024` — Astro `base` is `/SpaceAge-2024` in CI (`PUBLIC_BASE_PATH`).
- Local dev and Playwright preview use `base: '/'` (no env var).
- **Custom domain** (e.g. `spaceage.example.com`) is optional later: add a `CNAME` in repo Settings → Pages, set `PUBLIC_SITE_URL` in the workflow, and point DNS at GitHub Pages. Not required for Phase 1–2.

### GitHub Actions (`.github/workflows/website.yml`)

On every PR and push under `website/**` (and status producer scripts):

1. `npm ci`
2. `npm run check`
3. `npm test` (Vitest — schema + status producer)
4. `npm run build` (with `PUBLIC_BASE_PATH=/SpaceAge-2024` on CI)
5. **Deploy `website/dist/` to GitHub Pages** — only on push to `master`

**One-time repo setup:** Settings → Pages → Build and deployment → Source: **GitHub Actions**.

The Windows job runs the same Vitest suite with the PowerShell status producer (parity check).

### GM status publishing (Phase 2)

The dashboard fetches `/status.json` at runtime. The GM **does not** hand-edit JSON.

**Primary workflow — commit `status.json`:**

1. After `isolate`, `turn`, or `next` (these call `generate-status` automatically), or manually:
   ```powershell
   .\play\generate-status.ps1 <RunId>
   ```
2. Commit and push `website/public/status.json`:
   ```bash
   git add website/public/status.json
   git commit -m "status: turn N, accepting-orders"
   git push origin master
   ```
3. CI rebuilds and redeploys; the live site picks up the new file.

**Optional deadline:** `play/runs/<RunId>/gm/schedule.json` with `{ "nextTurnAt": "2026-09-15T23:59:59Z" }`, or `-NextTurnAt` on the script.

**Linux / CI fallback (no PowerShell):** `node website/scripts/generate-status.mjs <RunId>`

See also [`play/README_STATUS_AUTOGEN.md`](../play/README_STATUS_AUTOGEN.md) and [`play/README.md`](../play/README.md) (GM operations).

### Local production build (matches Pages)

```bash
cd website
PUBLIC_BASE_PATH=/SpaceAge-2024 PUBLIC_SITE_URL=https://sentinemodo.github.io npm run build
npm run preview   # open http://localhost:4321/SpaceAge-2024/
```

### Cloudflare Pages / Netlify (alternative)

Build command: `npm run build` with `PUBLIC_BASE_PATH` set to your mount path (or `/` on a dedicated subdomain). Output directory: `dist/`.

## Handoff to Testing

After changes:

1. Run `npm run check` — TypeScript passes
2. Run `npm test` — Vitest green
3. Notify `/website-tester` with:
   - Files changed / routes added
   - Scenario catalog ids (e.g., WS-001)
   - Copy notes (e.g., "credits moved to footer")
   - Known gaps

Tester runs `npm run build`, `npm run preview`, `npm run test:e2e` and confirms green before slice is done.

## Guidelines

- **Do:** Follow `architecture/delivery/website.md` and [ADR-0007](../architecture/adr/ADR-0007-public-campaign-website.md)
- **Do:** Keep files in `website/` only; do not import from `Game/` or `Tests/`
- **Do:** Validate `status.json` in Vitest before hand-off
- **Do:** Commit `astro.config.mjs`, `package.json`, `tsconfig.json`, and all source
- **Do not:** Add engine C# imports or parse Windows-1251 XML in Node
- **Do not:** Add SSR, React/Vue (unless architect-approved), auth, or cookies
- **Do not:** Host reports, orders, or `gamein.xml` on the public site
- **Do not:** Accept Phase 4 `/eta` `/battle` before Phase 1 is green

## Questions?

- Strategy / scope: See `architecture/delivery/website.md`
- Decision: See [ADR-0007](../architecture/adr/ADR-0007-public-campaign-website.md)
- Astro docs: https://docs.astro.build/
- Vitest: https://vitest.dev/
- Playwright: https://playwright.dev/

---

**Last Updated:** 2026-09-09  
**Phase:** 1–2 (lobby + status feed)  
**Status:** CI deploy to GitHub Pages on `master`

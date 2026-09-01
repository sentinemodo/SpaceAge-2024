# SpaceAge-2024 Codebase Review

**Repository:** SpaceAge-2024  
**Stack:** .NET Framework 4.8 (via Mono)  
**Branch:** cursor/campaign-load-play  
**Engine Version:** 0.1.157  
**Last reviewed:** 2025 (This Review)

---

## Executive Summary

**SpaceAge** is a sophisticated **Play-by-Email (PBEM) turn-based space strategy game** with a carefully architected **multi-agent Cursor workflow**. The codebase separates concerns across:

- **Game Engine** (C# / .NET 4.8) — turn execution, combat, economics
- **Campaign Infrastructure** (PowerShell scripts, XML catalogs, text reports)
- **Player Interface** (UTF-8 order files, markdown manuals)
- **Public Website** (Astro static site)
- **Designer Tools** (XML, markdown specs)

The `.cursor/` folder defines **six specialized Cursor agents** and **corresponding rulesets**, each with hard boundaries and explicit handoff points. This enables **coordinated AI workflow** without conflicts.

---

## Folder Structure

### Core Projects (Targets: .NET Framework 4.8)

```
Game/
├── battle/              # Combat system (Battle, BattleField, Tactics)
├── data structures/     # Domain model (Factions, Units, Resources, Tech, etc.)
├── effects/             # Effect system (Producing, Receiving, Training, etc.)
├── orders/              # Player-issued order parsing & execution
├── game/                # Main engine loop (Sequence, Market, XMLProcessing)
├── reports/             # Turn report generation
├── documentation/       # Rules, Concepts (outdated; superseded by player/*.md)
├── Program.cs           # Entry point; EngineVersion = "0.1.157"
└── Game.csproj          # Legacy project format (not SDK-style)

Tests/
├── SampleGame/          # Golden integration tests (turns 1–6)
├── fixtures/            # Isolated fixture catalogs
├── T*.cs                # Unit + integration tests (NUnit 4)
└── Tests.csproj         # Test harness
```

### Campaign Play Scripts & Data

```
campaign/
├── data.xml             # Live catalog (technologies, modules, items, skills, races)
├── gamein.1.xml         # Turn 1 seed (factions, galaxy, contracts)
└── _gen_gamein.py       # Regenerates gamein.1.xml from designer/galaxy.md

play/
├── README.md            # Operations manual (scripts, encoding, isolation rules)
├── init-run.ps1         # Create new run directory
├── reports.ps1          # Generate text reports
├── isolate.ps1          # Isolate reports to faction folders
├── turn.ps1             # Execute full turn
├── next.ps1             # Advance game state
└── runs/
	└── <id>/            # Live run (gitignored)
		├── data/        # gamein.xml, gameout.*.xml
		├── turn/        # UTF-8 order.*.txt (converted to 1251 on copy)
		├── gm/          # Optional GM workspace
		└── factions/
			├── 02/…11/  # Player folders (isolation boundary)
			│   ├── persona.md
			│   ├── report.{turn}.{id}.txt
			│   ├── order.{id}.txt (drafted by /player or campaign-ai)
			│   ├── story.md (current quarterly plan)
			│   └── story.{turn}.md (archived plans)
			└── 01/12/13/  # NPC only (never exists on disk)
```

### Designer Workspace

```
designer/
├── README.md
├── xml-schema.md        # XML token reference
├── galaxy.md            # 10-player seed spec (two occupied, eight empty systems)
├── environments.md      # Gravity, atmosphere, temperature
├── combat-balance.md    # Raid sizes, damage formulas, typed matchups
├── economy.md           # HQ cash, market seeding
├── technology.md        # Tech tree (L0–L10)
├── resources.md         # Canonical resource dictionary
├── catalog.md           # Module types, items, skills
├── contracts.md         # Contract vectors (injection triggers)
├── engine-wishlist.md   # Missing effects, orders, groups (no C# here)
```

### Player Interface & Manuals

```
player/
├── rules.md             # Live order syntax (immediate vs long, subjects)
├── basic_technologies.md  # L0–L1 techs (SampleGame source: Tests/data.xml)
├── advanced_technologies.md  # L2+ techs with requires graph
├── battle.md            # Diplomacy, sides, tactics, hit/damage rules
├── campaign/basic_technologies.md  # L0–L1 excerpt for campaign play (campaign/data.xml)
├── order_wishlist.md    # Suggested syntax improvements (player objectives)
├── technologies_wishlist.md  # Requested new techs or rebalance
└── drafts/              # UTF-8 order drafts (test helper writing)
```

### Public Website (Astro)

```
website/
├── src/
│   ├── pages/           # /, /client, /turns, /rules (+ Phase 4: /eta, /battle)
│   ├── components/      # Reusable UI
│   ├── styles/          # CSS (mobile-first, no CSS-in-JS)
│   └── lib/             # TS helpers (Vitest)
├── e2e/                 # Playwright scenarios (WS-001…WS-009)
├── public/status.json   # Placeholder schema (factions 2–11, no secrets)
├── astro.config.mjs     # static output, TypeScript
└── playwright.config.ts  # Chromium testing
```

### Architecture Documentation

```
architecture/
├── README.md            # Pointer to this tree
├── overview.md          # Principles, glossary
├── modules-and-integrations.md
├── technology.md        # Stack: .NET 4.8, NUnit 4, Astro, Playwright, Windows-1251 IO
├── docs-index.md        # Canonical library versions, external docs
├── adr/                 # Architecture Decision Records (ADR-0001…ADR-0007)
│   ├── ADR-0001-net48-legacy-csproj.md
│   ├── ADR-0002-windows-1251-io.md
│   ├── ADR-0003-filesystem-pbem-batch.md
│   ├── ADR-0004-test-layers.md
│   ├── ADR-0005-modulestack-partials.md
│   ├── ADR-0006-datafile-facade-and-xml-seams.md
│   └── ADR-0007-public-campaign-website.md
├── delivery/
│   ├── cicd-conventions.md
│   ├── campaign-play.md
│   ├── website.md       # Cursor agents + test pairing; Phase 1–4
│   ├── website-scenarios.md  # Scenario catalog (WS-001…WS-009)
│   ├── datafile-refactor.md
│   ├── saveload-quirks.md
│   ├── samplegame-turn4.md
│   ├── samplegame-turn5.md
├── diagrams/
└── future-work.md       # Deferred modernization backlog
```

### Cursor Agent Configuration

```
.cursor/
├── environment.json     # Workspace metadata
├── agents/              # Six agent definitions
│   ├── campaign-ai.md   # Plays one faction (2–11), writes story, calls /player
│   ├── campaign-gm.md   # Runs play/ scripts, maintains play/README.md
│   ├── game-designer.md  # Owns campaign XML, tech tree, contracts
│   ├── player.md        # Reads reports, drafts orders, updates manuals
│   ├── project-architect.md  # Maintains architecture/ docs
│   ├── website-developer.md  # Astro pages, Vitest, handoff to /website-tester
│   └── (website-tester.md and game-designer.md exist but are less central for code review)
├── rules/               # Rulesets that activate agents
│   ├── campaign-ai.mdc
│   ├── campaign-gm.mdc
│   ├── csharp-tdd.mdc   # **Central: all C# work goes here**
│   ├── game-designer.mdc
│   ├── website-astro.mdc
│   └── website-tester.mdc
├── install.sh           # Setup script (Mono, Nix, etc.)
├── run-tests.sh         # Test runner (NUnit console)
```

---

## Agent System (`.cursor/` Agents & Rules)

The repository uses **six specialized Cursor agents** with **hard handoff boundaries**. Each agent has:

1. **Agent definition** (`.cursor/agents/*.md`) — role, canonical files, hard rules, workflow
2. **Ruleset** (`.cursor/rules/*.mdc`) — glob patterns, brief reference, when to delegate

### 1. **csharp-tdd** (`csharp-tdd.mdc`)
- **Scope:** All C# (`**/*.cs`)
- **Role:** Senior C# engineer; test-first (Red–Green–Refactor)
- **Key Rules:**
  - No production change without a test (unless user opts out for a spike)
  - NUnit 4, namespace-based layers (UnitTests / IntegrationTests)
  - Calls `/player` when orders are needed, report validation is needed, or golden changes
  - Before every commit: run `/player` to refresh `player/rules.md`, `player/basic_technologies.md`, `player/advanced_technologies.md`, `player/battle.md`
  - Do **not** overwrite goldens without `/player` + explicit human approval
  - Handoff to `/project-architect` for major boundaries, integrations, multi-repo layout

### 2. **campaign-ai** (`campaign-ai.mdc`)
- **Scope:** `play/runs/**/factions/NN/` (faction folders 02–11)
- **Role:** Play one Interest per invocation; write strategic + tactical story, call `/player` for orders
- **Key Rules:**
  - One faction (2–11) per call; no mixing
  - Read only: `persona.md`, `report.{T}.{id}.txt`, `player/rules.md`, `player/campaign/basic_technologies.md`
  - **Forbidden:** `campaign/data.xml`, `data/data.xml`, `gamein.xml`, `gameout*.xml`, other faction folders, C#
  - Write `story.md` with Review (vs last quarter), Strategic (system, 4 quarters), Tactical (planet/moon, next quarter), Win objective (T ≥ 10)
  - **Never** draft orders; call `/player` with tactical objective
  - No TDD, no `/game-designer` — proposed gaps wait for human approval

### 3. **campaign-gm** (`campaign-gm.mdc`)
- **Scope:** `play/` scripts, `play/README.md`
- **Role:** Operate the campaign table; execute scripts, keep docs accurate, delegate AI / designer / player
- **Key Rules:**
  - **Execute** `play/*.ps1` scripts; **do not write or patch** them (document gaps instead)
  - Do **not** write C#, tests, contracts, or player orders (factions 2–11)
  - Isolation: text reports only into `factions/02`–`11`. No `gamein`, `gameout`, XML reports in faction folders
  - No `factions/01`, `12`, `13`; NPC files handled elsewhere
  - Handoff design / player / campaign-ai, wait for their files before next script
  - State: run id, scripts run, orders per faction (yes/no), contracts/press applied, win check, README changed, script gaps (one sentence)

### 4. **game-designer** (`game-designer.mdc`)
- **Scope:** `campaign/`, `designer/`
- **Role:** Own campaign XML (catalog, galaxy, contracts); never C# or test files
- **Key Rules:**
  - May read: `Tests/data.xml`, SampleGame `gamein.*`, `Game/game/DataFile.cs` (schema only), `player/*.md`
  - **Do not change:** `Tests/**`, `*.cs`, `*.csproj`
  - Edit: `campaign/data.xml`, `campaign/gamein.1.xml`, `campaign/_gen_gamein.py`, `designer/` markdown
  - Encoding: Windows-1251
  - Ids ≤ 6 characters
  - If a new module group or location-type crashes load → add to `designer/engine-wishlist.md`, **do not patch DataFile**
  - Design galaxy first in `designer/galaxy.md` (10 systems, 2 occupied, 8 empty; ten players on two starts; UN + militia NPCs; Alderson Gates)
  - Level 10 tech enables self-sufficient ark (thousands crew, inner-system weeks)
  - Inject contracts mid-game (alien threat, pirates, bombardment, fauna, anomalies)

### 5. **player** (agent: `player.md`; no separate ruleset yet)
- **Scope:** `player/`, `player/drafts/`, faction folders (text reports, orders)
- **Role:** Play the game; read reports, draft orders, maintain player manuals
- **Key Rules:**
  - **Do not write:** C#, test fixtures, engine XML catalogs
  - **May read:** order syntax from `Game/orders/`, catalog from `data.xml` / `Tests/data.xml`
  - Orders use **only implemented syntax** (`EOrderType`, `*Order.Parse`, `OrdersReader`)
  - Manuals: `player/rules.md` (order syntax, immediate vs long), `player/basic_technologies.md` (L0–1 per `Tests/data.xml`), `player/advanced_technologies.md` (L2+ with requires graph), `player/battle.md` (rules, tactics, diplomacy)
  - Campaign manuals: `player/campaign/basic_technologies.md` excerpt from `campaign/data.xml` (for campaign-ai)
  - **Never** copy SampleGame manuals from campaign or vice versa
  - Draft UTF-8 orders into `player/drafts/` (or TDD-named path); reminder to save 1251 if copied to GM
  - Wishlists (optional): `player/order_wishlist.md`, `player/technologies_wishlist.md`
  - Report review: **match** or **mismatch** with cited lines/ids (for TDD golden validation)
  - **Before commit:** refresh all four manuals (TDD asks for docs-only prompt)

### 6. **project-architect**
- **Scope:** `architecture/` tree
- **Role:** Design and document solution boundaries, integrations, tech stack, decisions
- **Key Rules:**
  - **Do not** implement production code (`*.cs`, `*.csproj`, runtime config)
  - **Do** design and maintain `architecture/` docs (README, overview, modules, technology, ADRs, delivery, diagrams, future-work)
  - Create ADRs when architecture must shift
  - If production code / test changes needed → return clear recommendations, do not edit them
  - Summaries: paths created/updated, what implementers should read first

### 7. **website-developer** (ruleset: `website-astro.mdc`)
- **Scope:** `website/src/**`, `website/public/`, `website/**/*.test.ts`, astro config
- **Role:** Implement Astro static site (closed PBEM lobby); Vitest for schema, helpers
- **Key Rules:**
  - **Do not write:** C#, Tests, Playwright, visual tool, engine XML in Node
  - Read before scaffolding: `architecture/delivery/website.md`, ADR-0007, `architecture/technology.md` (Website section)
  - Delegate `/project-architect` first for: new page beyond `/`, `/client`, `/turns`, `/rules`, Phase 4 `/eta` `/battle`; SSR / React / Vue / accounts; npm deps beyond Astro / TypeScript / `@astrojs/check` / Vitest
  - Divergence from architecture → stop, present gap, require **explicit human approval** (named, e.g. "approve SSR")
  - Modern Astro: content-first, `output: 'static'`, Islands only for interactivity, Typed `status.json` (factions 2–11, no secrets), accessible HTML, CSS files, no CSS-in-JS
  - Vitest for schema, helpers; Red → Green
  - **Handoff to `/website-tester`** — do not declare done from browser tour
	- Architecture still matches (or ADR + human yes for named deviation)
	- Routes and files touched
	- `astro check` green, `vitest run` green
	- `status.json` schema-valid, no secrets
	- Copy excerpted from Rules.txt
	- Known gaps listed

### 8. **website-tester** (ruleset: `website-tester.mdc`)
- **Scope:** `website/e2e/**`, `website/**/*.spec.ts`, Playwright config, scenario catalog
- **Role:** Define scenarios, automate them, Playwright acceptance testing
- **Key Rules:**
  - **Do not:** restyle pages, add routes, implement visual tool, manual exploration as acceptance
  - Scenario catalog (`architecture/delivery/website-scenarios.md`, then `website/e2e/scenarios.md`): id, goal, routes, given/when/then, layer, security
  - Seeded: WS-001…WS-009 (lobby). Reserved Phase 4: WS-010 `/eta`, WS-011 `/battle`
  - Playwright tests cite scenario id; no row → add before spec counts
  - Chromium MVP vs `astro build` + `astro preview` (not Vite)
  - WS-007: viewport 390×844 (mobile)
  - WS-008: never leak passwords, gamein, order, report paths
  - Failures → `/website-developer` with id + assertion

---

## Rulesets (`.cursor/rules/*.mdc`)

Each `.mdc` file is a **Cursor ruleset** that activates the corresponding agent when files matching the glob pattern are in scope.

| Ruleset | Glob | Agent | When Applies |
|---------|------|-------|--------------|
| `csharp-tdd.mdc` | `**/*.cs` | `/csharp-tdd` | Any C# file; TDD workflow |
| `campaign-ai.mdc` | `play/runs/**/factions/**` | `/campaign-ai` | Faction folders (AI isolated play) |
| `campaign-gm.mdc` | `play/README.md` | `/campaign-gm` | Campaign operations |
| `game-designer.mdc` | `designer/**,campaign/**` | `/game-designer` | XML, catalog, contracts |
| `website-astro.mdc` | `website/src/**,website/public/**,website/**/*.test.ts,website/astro.config.*,website/tsconfig*.json` | `/website-developer` | Astro pages, Vitest, config |
| `website-tester.mdc` | `website/e2e/**,website/**/*.spec.ts,website/playwright.config.*,architecture/delivery/website-scenarios.md` | `/website-tester` | Playwright, scenarios |

---

## Key Architectural Decisions (ADRs)

| ADR | Title | Impact |
|-----|-------|--------|
| **ADR-0001** | .NET 4.8 Legacy (non-SDK) | `Game.csproj` and `Tests.csproj` use `ToolsVersion="4.0"` csproj format, not SDK-style; targets net48 |
| **ADR-0002** | Windows-1251 I/O | All engine XML, catalog, reports, and turn orders on disk are Windows-1251; player drafts and persona are UTF-8 |
| **ADR-0003** | Filesystem PBEM Batch | Campaign runs live on disk (`play/runs/<id>/`); scripts (PowerShell, .NET exe) batch-process turns; no HTTP/DB/game server |
| **ADR-0004** | Test Layers (Namespaces) | Single `Tests.dll`, NUnit 4; layers by namespace: `UnitTests` (fixtures, no goldens), `IntegrationTests` (SampleGame turns, goldens); no `[Category]` attributes |
| **ADR-0005** | ModuleStack Partials | Separate file per concern: `ModuleStack.cs` (core), `ModuleStack.Ownership.cs`, `ModuleStack.Upkeep.cs` |
| **ADR-0006** | DataFile Facade & XML Seams | `DataFile` loads and exposes schema; engine XML is a contract surface — tests mock it |
| **ADR-0007** | Public Campaign Website | Astro static site (Phase 1–4); lobby shows factions 2–11, status, turns, rules; Phase 4 adds `/eta` and `/battle` previews |

---

## Core Systems (C# Domain Model)

### Domain Entities

**Cosmos (spatial):**
- `Galaxy` → `SpaceSystems` → `Stars`, `Planets`, `Moons`, `Belts`, `Orbits`
- `Region` (planet/moon surfaces; types: ground, sea, etc.)
- `Location` (named places, e.g., cities, spaceports)
- `Alderson` (paired FTL gates between systems)

**People & Factions:**
- `Faction` (player, NPC; attitude per pair)
- `Person` (officer; race, skills, equipment)
- `Race` (genetic background; skill bonuses)
- `Skill`, `SkillType`, `SkillEffects` (training, bonuses in combat/production)

**Modules & Cargo:**
- `ModuleType` (hull, engine, cargo, industry, etc.; groups, techs)
- `Module` (instance in a stack)
- `ModuleStack` (owner, quantity, upkeep, effects, partial separation)
- `ItemType`, `ItemStack` (raw goods, ship equipment)
- `Capacity` (hold, crew, power budget)

**Production & Research:**
- `Producable` (modules, items, skills; requires resources, techs, labor)
- `Technology`, `SkillBonusFormula` (tech tree; unlocks modules and items)
- `Production` (long order; weekly output per module type)
- `SkillPercentProduce`, `ProducingEffect` (bonus formulas)

**Orders & Execution:**
- `Order` (base; immediate or long)
- `*Order` subclasses (Move, Jump, Buy, Sell, Produce, Research, Train, etc.)
- `Tactic` (combat behavior; subclasses: Capture, Destroy, Evade, PrioritizeX)
- `Battle`, `BattleField` (turn-based combat with multiple sides)

**Economy & Trade:**
- `Bank` (faction cash, weekly income/upkeep)
- `Market` (faction buys/sells; prices fluctuate per availability)
- `Offer` (NPC market offers; types: buy, sell, trade)
- `Contract` (faction mission; triggers on event; rewards)

**Effects & State:**
- `Effect` (applied to module stack; types: Moving, Producing, Training, Damaged, etc.)
- `EDamageStatus` (light/medium/heavy/wrecked)
- `EProducableEffects`, `ENoUpkeepEffects` (effect groups)

**Reporting:**
- `EventReport` (turn summary; per faction)
- `ReportLine` (event detail; types: Order Executed, Battle, Tech Researched, etc.)
- `ReportWriter` (formats text report output)

---

## Test Strategy (NUnit 4, Namespace Layers)

### Test Organization

| Layer | Namespace | Scope | Files |
|-------|-----------|-------|-------|
| **Unit** | `Tests.UnitTests` | Single type; XML fixtures (`Tests/data.xml`, `Tests/gamein.xml`); no multi-turn goldens | `Tests/T*.cs` |
| **Integration** | `Tests.IntegrationTests` | Full SampleGame turns (1–6); report/XML goldens; program smoke | `Tests/SampleGame/SampleGame.cs`, etc. |

### Key Test Files

- **`SampleGame/SampleGame.cs`** — Harness; runs turns 1–6 with `ExecuteTurn` helper
- **`SampleGame/testreport.*.txt`** — Golden expected reports (txt, Windows-1251)
- **`SampleGame/gameout.*.xml`** — Golden expected game state (xml, Windows-1251)
- **`TTest.cs`** — Base class; helpers: `compareFiles`, `executeOrder`, `Game.ClearDictionaries()` (teardown)
- **`T*.cs`** — Individual unit tests (TAlderson, TBattle, TMarket, TMove, etc.)

### Important Test Pattern

```csharp
[TearDown]
public void Teardown()
{
	Game.ClearDictionaries();  // Reset static *.All registries between tests
}
```

### Running Tests

```bash
# Via .cursor/run-tests.sh (NUnit console, Mono, --inprocess)
bash .cursor/run-tests.sh

# Or: vstest.console (Windows)
vstest.console Tests\Tests.csproj

# Via VS NUnit adapter
# (no `dotnet test` — net48, non-SDK)
```

### Golden File Strategy

- **Integration tests** compare generated reports/XML to committed goldens
- **New or changed golden** → **must** pass through `/player` (validation) + **explicit human approval** before replacing the committed file
- **Exception:** Engine version line only (SpaceAge Engine Version: X.Y.Z) in `testreport` / `gameout` files → already approved for replacement (no player handoff)
- Do **not** commit golden diffs without this pair of gates

---

## Data Files & Encoding

### Campaign Catalog & Game State

| File | Format | Encoding | Ownership | Notes |
|------|--------|----------|-----------|-------|
| `campaign/data.xml` | XML | Windows-1251 | `/game-designer` | Live catalog (techs, modules, items, skills, races) |
| `campaign/gamein.1.xml` | XML | Windows-1251 | `/game-designer` | Turn 1 seed (factions, galaxy, contracts) |
| `campaign/_gen_gamein.py` | Python | UTF-8 | `/game-designer` | Regenerates `gamein.1.xml` from `designer/galaxy.md` |
| `play/runs/<id>/data/gamein.xml` | XML | Windows-1251 | Campaign script copy (gitignored) | Engine loads this |
| `play/runs/<id>/data/gameout.N.xml` | XML | Windows-1251 | Engine output | Turn N result |

### Turn Orders & Reports

| File | Format | Encoding | Ownership | Notes |
|------|--------|----------|-----------|-------|
| `player/drafts/order.{id}.txt` | UTF-8 text | UTF-8 | `/player` | Draft (repo); converted to 1251 for `turn.ps1` |
| `play/runs/<id>/turn/order.{id}.txt` | UTF-8 text | 1251 (via copy) | Campaign script (temp) | Engine reads these |
| `play/runs/<id>/factions/NN/report.{T}.{id}.txt` | UTF-8 text | Windows-1251 | Engine output → isolate script | Text report (player reads) |
| `play/runs/<id>/factions/NN/report.{T}.{id}.xml` | XML | Windows-1251 | Engine output (optional) | Full state snapshot |

### Player Manuals

| File | Format | Encoding | Ownership | Notes |
|------|--------|----------|-----------|-------|
| `player/rules.md` | Markdown | UTF-8 | `/player` | Order syntax (immediate vs long, subjects) |
| `player/basic_technologies.md` | Markdown | UTF-8 | `/player` | L0–1 techs from `Tests/data.xml` (SampleGame) |
| `player/advanced_technologies.md` | Markdown | UTF-8 | `/player` | L2+ techs from `Tests/data.xml` (SampleGame) |
| `player/campaign/basic_technologies.md` | Markdown | UTF-8 | `/player` | L0–1 excerpt from `campaign/data.xml` (campaign play) |
| `player/battle.md` | Markdown | UTF-8 | `/player` | Rules: diplomacy, sides, tactics, hit/damage |

### Test Fixtures

| File | Format | Encoding | Ownership | Notes |
|------|--------|----------|-----------|-------|
| `Tests/data.xml` | XML | Windows-1251 | Frozen test fixture | **Never** updated from campaign; SampleGame uses this |
| `Tests/gamein.xml` | XML | Windows-1251 | Frozen test fixture | SampleGame turn 1 seed |
| `Tests/SampleGame/gamein.*.xml` | XML | Windows-1251 | Generated from prior turn | Intermediate seeds (golden) |
| `Tests/fixtures/*/gamein.xml` | XML | Windows-1251 | Isolated test catalogs | Unit tests only |

---

## Player Interface (Orders & Manuals)

### Order Syntax

**Subjects:**
- `#faction <id> "<password>"` (must match report)
- `#modulestack <id>` (named stack from report or alias)
- `#person <id>`
- `#end` (terminator)

**Prefixes (condition chains):**
- `-` (immediate)
- `+` (long, repeat weekly)
- `@` (long, once at end of turn)
- `N` (repeat N times, then drop)

**Order verbs (implemented):**
- Immediate: `MOVE`, `JUMP`, `SEE`, `ATTACK`, `TACTIC`, `DECLARE`, `USE`, `NAME`, `GET`, `GIVE`, `TRANSFER`, `SELL`, `BUY`
- Long: `MOVE`, `JUMP`, `PRODUCE`, `RESEARCH`, `TRAIN`, `REPAIR`, `CAPTURE`

**Comments:**
- `;` (rest of line)
- `//` (rest of line)

**Example:**
```
#faction 2 "password2"
#modulestack Frigate
- SEE
- MOVE Solar System to Helios/1 north region
+ PRODUCE Fusion Reactor
#modulestack Explorer
#end
```

### Manual Format

**`player/rules.md`:**
- Order syntax reference
- Prefixes and subjects
- Immediate vs long execution
- Turn sequence (13-week loop)
- Order grouping: A–Z inside each immediate/long category

**`player/basic_technologies.md` / `player/advanced_technologies.md`:**
- Technologies grouped by level (0, 1, 2, 3, …)
- Alphabetical within level
- Associated modules and items per tech
- `requires` diagram (markdown, Mermaid) for L2+

**`player/battle.md`:**
- Sides (Attacker, Defender, Neutral per module type)
- Diplomacy: attitudes (Hostility, Neutral, Alliance)
- Weeks per turn; weeks per combat round
- Tactics: Capture, Destroy, Evade, PrioritizeArmed, PrioritizeCommand, PrioritizeCargo
- Initiative (Attacker first)
- Hit chance (modified by range, officer skill, equipment)
- Damage (typed per module; mitigation by armor/shield)

---

## Campaign Play Loop

### Scripts & Encoding

| Script | When | What | Encoding |
|--------|------|------|----------|
| `init-run.ps1 <id>` | New table | Copy `campaign/data.xml` + `campaign/gamein.1.xml` → `play/runs/<id>/data/`; patch passwords 2–11; write `persona.md` | 1251 → 1251 |
| `reports.ps1 <id>` | After init or turn | `Game.exe /reports` → text reports | Input 1251 → Output 1251 |
| `isolate.ps1 <id>` | After reports | Copy `report.{T}.{2–11}.txt` → `factions/NN/` (isolation) | 1251 → 1251 |
| `turn.ps1 <id>` | All ten `order.{id}.txt` exist | Clear `turn/order.*`, UTF-8 → 1251 copy, `Game.exe /turn`, then isolate | UTF-8 input → 1251 → 1251 output |
| `next.ps1 <id>` | After full turn | Copy `data/gameout.{N}.xml` → `data/gamein.xml` (advance turn) | 1251 → 1251 |

### Isolation Boundary

**What factions see (in their folder):**
- `persona.md` (id, password, preference, doctrine, win objective)
- `report.{T}.{id}.txt` (text report; this faction's view only)
- `order.{id}.txt` (draft or executed)
- `story.md`, `story.{T}.md` (AI quarter plans)

**What factions cannot see:**
- `campaign/data.xml`, run `data/data.xml` (catalog)
- `gamein.xml`, `gameout*.xml` (full game state)
- Other faction folders
- `campaign/gamein.1.xml`, `/turn` (shared XML and scripts)
- XML reports

---

## Website (Astro, Phase 1–4)

### Phase 1 (Seeded: WS-001…WS-009)

| Route | Page | Content | Scenario |
|-------|------|---------|----------|
| `/` | Lobby | Intro; credit Atlantis, Rise of Heroes, Vincent Archer; link to `/turns` and `/rules` | WS-001 |
| `/client` | Client setup | Install, run, connect to PBEM server (excerpt Rules.txt §1.1) | WS-002 |
| `/turns` | Turn list | Ten factions (2–11); status (roster, turn, date); link to turn reports (when live) | WS-004, WS-007 |
| `/rules` | Rules | Excerpt Rules.txt (§1, §2.1); hard science flavour | WS-006 |

### Phase 4 (Reserved: WS-010, WS-011)

| Route | Page | Content | Scenario |
|-------|------|---------|----------|
| `/eta` | ETA calculator | User-pasted text report → parse (browser, UTF-8 only); estimate arrival week (no POST, no persist) | WS-010 |
| `/battle` | Battle preview | User-pasted text report → parse (browser); visualize matchup, odds | WS-011 |

### Architecture & Tech Stack

- **Framework:** Astro (static output, `output: 'static'`)
- **CSS:** Mobile-first, CSS files under `website/src/styles/`; no CSS-in-JS
- **Testing (Vitest):** Unit tests for schema, helpers (TS); Red → Green
- **Acceptance (Playwright):** Chromium on `astro build` + `astro preview`; scenario catalog
- **No auth, no accounts, no HTTP engine** (Phase 1)
- **`status.json` schema** (placeholder; v1 factions 2–11, status enum, no secrets)

### Security

- Never leak passwords, `gamein`, `order.*`, `report.*` paths
- No engine XML in Node build
- Phase 4 `/eta` and `/battle` parse user-pasted UTF-8 **in browser only** (no Node, no POST, no persist)

---

## Handoff Checkpoints

### TDD → `/player` (Order Drafting)

When C# adds or changes turn orders:

1. **Prompt:** Faction, password, report path(s), tactical objective (planet/moon, what to build/fight)
2. **Player handoff:** Path(s) written to `player/drafts/` (or TDD-named path), any parser gaps
3. **TDD next:** Use the orders in test

### TDD → `/player` (Report Validation)

When C# runs a turn or golden comparison:

1. **Prompt:** Report path(s) (txt, xml), expected beats (ids, locations, cargo, events)
2. **Player handoff:** **Match** or **mismatch** with cited lines/ids
3. **TDD next:** If mismatch, fix engine; if match, proceed to golden approval

### TDD → `/player` (Golden Replacement)

**Two-gate approval process:**

1. Keep generated output as candidate (not overwriting committed golden yet)
2. **Prompt `/player`:** Candidate path(s), current golden (if any), expected beats
3. **Player handoff:** **Match** or **mismatch**
4. **If mismatch:** Do not update; fix and retry
5. **If match:** Show the golden path and **STOP** — wait for explicit human approval
6. Human: **Approve this golden** (e.g., "replace report.2.1.txt", "approve gameout.3.xml")
7. TDD: Replace committed file

**Exception:** Engine version line only → already approved (no `/player` pass needed)

### TDD → `/project-architect` (Architectural Questions)

When TDD faces new systems, major boundaries, integration choices, or multi-repo layout:

1. **Stop** and delegate
2. `/project-architect` designs and documents in `architecture/` (ADR, revised doc, diagrams)
3. **Architect handoff:** Architecture stable; what TDD should read first
4. **TDD continues:** Implement aligned to architecture

### `/player` → `/game-designer` (Wishlists)

When `/player` finds a gap in order syntax or catalog:

1. **`/player` writes:** `player/order_wishlist.md` (syntax), `player/technologies_wishlist.md` (techs / balance)
2. **One sentence per item:** objective vs current limitation
3. **Do not implement** (no C#, no XML changes) — wait for human review

### `/campaign-ai` → `/player` (Story to Orders)

When `/campaign-ai` writes a story:

1. **Story:** Review (vs last quarter), Strategic (system, 4Q, persona-tied), Tactical (planet/moon, next Q)
2. **Prompt `/player`:** Tactical objective (what to build, where to land, what to fight)
3. **Player handoff:** UTF-8 `order.{id}.txt` in faction folder, any parser gaps
4. **Campaign-ai:** Handoff to GM; folder is ready

### `/website-developer` → `/website-tester` (Handoff)

When `/website-developer` finishes a vertical slice:

1. **Checklist:**
   - Architecture still matches (or ADR + **explicit human yes** for named deviation)
   - Routes, files, catalog ids touched
   - `astro check` green; `vitest run` green
   - `status.json` schema-valid, no secrets
   - Copy excerpted from Rules.txt; no new myth
   - Known gaps listed
2. **Prompt `/website-tester`:** Update catalog if needed; write/adjust Playwright; run build + test
3. **Tester handoff:** Playwright tests green; CI ready

---

## Key Files to Know

| Path | Purpose | Owner |
|------|---------|-------|
| `Game/Program.cs` | Entry point; `EngineVersion`, `Main` | C# (TDD) |
| `Game/Game.cs` | Main game loop; `Sequence`, `Execute`, events | C# (TDD) |
| `Game/game/DataFile.cs` | XML schema facade; loads catalog & game state | C# (TDD) |
| `Game/orders/Order.cs`, `*Order.cs` | Order parsing & execution | C# (TDD) |
| `Game/battle/Battle.cs` | Combat simulation; tactics, initiative, damage | C# (TDD) |
| `Tests/TTest.cs` | Test base; helpers (`compareFiles`, `Game.ClearDictionaries`) | C# (TDD) |
| `Tests/SampleGame/SampleGame.cs` | Integration harness (turns 1–6) | C# (TDD) |
| `campaign/data.xml` | Live catalog | Designer |
| `campaign/gamein.1.xml` | Turn 1 seed | Designer |
| `designer/galaxy.md` | 10-system spec | Designer |
| `player/rules.md` | Order syntax manuals | Player |
| `player/basic_technologies.md`, `player/advanced_technologies.md`, `player/battle.md` | Game manuals | Player |
| `play/README.md` | Campaign ops manual | GM |
| `play/init-run.ps1`, `play/reports.ps1`, `play/isolate.ps1`, `play/turn.ps1`, `play/next.ps1` | Campaign scripts | (Execute; do not edit) |
| `website/src/pages/`, `website/src/components/` | Astro lobby | Website-Developer |
| `website/e2e/` | Playwright scenarios | Website-Tester |
| `architecture/overview.md`, `architecture/modules-and-integrations.md`, `architecture/adr/` | Architecture docs | Project-Architect |
| `.cursor/agents/`, `.cursor/rules/` | Cursor agent config | All (reference) |

---

## Technology Stack

| Layer | Technology | Version / Notes |
|-------|-----------|-----------------|
| **Engine** | C# / .NET Framework | 4.8 (via Mono); legacy csproj (non-SDK) |
| **Game State** | XML | Windows-1251 encoding |
| **Testing** | NUnit | 4 (namespace layers) |
| **Campaign Scripts** | PowerShell | `*.ps1` (cross-platform via Mono) |
| **Player Interface** | UTF-8 text orders | Windows-1251 on disk (conversion in scripts) |
| **Player Manuals** | Markdown | UTF-8 |
| **Website** | Astro | Static output; `output: 'static'` |
| **Website Testing** | Vitest + Playwright | Chromium; `astro build` + `astro preview` |
| **Architecture Docs** | Markdown + Mermaid | Version-controlled in `architecture/` |

---

## Encoding Rules (Critical)

| File Type | Encoding | When/Where | Notes |
|-----------|----------|-----------|-------|
| Engine XML (catalog, game state) | Windows-1251 | Disk: `campaign/data.xml`, `play/runs/<id>/data/gamein.xml`, `gameout*.xml` | C# reads natively; schema requires 1251 |
| Turn reports (text) | Windows-1251 | Disk: `play/runs/<id>/factions/NN/report.{T}.{id}.txt` | Engine writes 1251; player reads 1251 |
| Turn orders (submitted to engine) | 1251 on disk | Copy step: UTF-8 drafts → 1251 before `turn.ps1` | Player drafts UTF-8; `turn.ps1` converts |
| Player manuals, drafts | UTF-8 | Repo: `player/*.md`, `player/drafts/` | Human-readable; repo standard |
| Persona, story | UTF-8 | Repo: `play/runs/<id>/factions/NN/persona.md`, `story.md` | Human-readable; campaign notes |

**Critical:** If a file is Windows-1251 and you open it as UTF-8 (or vice versa), characters are corrupted. The scripts handle conversion in the PBEM loop; do not double-convert.

---

## Isolation Rules (Campaign Play)

### What Each Faction Sees

**Allowed in `play/runs/<id>/factions/NN/`:**
- `persona.md` (id, password, preference, doctrine, win objective)
- `report.{T}.{id}.txt` (text report; isolated view)
- `order.{id}.txt` (current or draft orders)
- `story.md`, `story.{T}.md` (quarter plans)

**Forbidden (never copy into faction folder):**
- `campaign/data.xml`, run `data/data.xml` (catalog)
- `gamein.xml`, `gameout*.xml`, `campaign/gamein.1.xml` (full state)
- `report.*.xml` (leaks other cargo, techs)
- Other `factions/NN/` folders
- `play/runs/<id>/turn/` (shared orders and XML)
- `play/runs/<id>/data/` (entire data dir)

### NPC Factions

- **Factions 1, 12, 13:** NPC only; **never** create folders `factions/01`, `factions/12`, `factions/13` on disk
- **Passwords:** Live only in gitignored `play/runs/<id>/`, never in `play/README.md` or committed files
- **Orders:** NPC factions do not submit `order.{id}.txt`; they are governed by contracts and reactions in XML

---

## Common Workflows

### Scenario 1: Add a new technology

1. **Designer** edits `designer/technology.md` (level, cost, unlocked modules/items)
2. **Designer** runs `campaign/_gen_gamein.py` or manually edits `campaign/data.xml`
3. **Player** updates `player/basic_technologies.md` or `player/advanced_technologies.md` (from new catalog)
4. **TDD** adds test (if engine behavior changed) and calls `/player` to update manuals
5. **Commit:** Human approves, manuals are current

### Scenario 2: Run a campaign turn

1. **GM** runs `play/reports.ps1 <id>` (no turn increment; reports only)
2. **GM** runs `play/isolate.ps1 <id>` (copy to faction folders)
3. **GM** launches **10 × `/campaign-ai`** (one faction each, 2–11)
   - Each reads `persona.md`, latest `report.{T}.{id}.txt`
   - Writes `story.md`
   - Calls `/player` with tactical objective
4. **Player** (per AI request) drafts `order.{id}.txt` into faction folder
5. **GM** collects all ten `order.{id}.txt` files
6. **GM** runs `play/turn.ps1 <id>` (UTF-8 → 1251, engine execute, isolate)
7. **GM** runs `play/next.ps1 <id>` (advance seed)
8. **TDD** or **Player** validates reports (match expected beats)

### Scenario 3: Update player manuals before commit

1. **TDD** proposes changes (new order, rebalanced tech, changed combat rule)
2. **TDD** calls `/player` with prompt: "Docs-only: update `player/rules.md`, `player/basic_technologies.md`, `player/advanced_technologies.md`, `player/battle.md` from current engine and catalog"
3. **Player** refreshes all four manuals from `Game/orders/`, `Game/battle/`, and `campaign/data.xml` (or `Tests/data.xml` for SampleGame)
4. **Player** handoff: paths changed (or "no changes needed")
5. **TDD** commits with updated manuals

### Scenario 4: Website vertical slice

1. **Website-Developer** reads `architecture/delivery/website.md`, ADR-0007, `architecture/technology.md` (Website section)
2. **Website-Developer** scaffolds page (e.g., `/turns`)
3. **Website-Developer** writes Vitest for schema / helpers; runs `vitest run` → green
4. **Website-Developer** runs `astro check` → green
5. **Website-Developer** hands off to **`/website-tester`** with checklist:
   - Routes, files, catalog ids
   - `astro check`, `vitest run` green
   - `status.json` schema-valid, no secrets
   - Copy excerpted
   - Known gaps
6. **Website-Tester** updates scenario catalog (`website-scenarios.md`), writes Playwright, runs `astro build` + `playwright test`
7. **Tester handoff:** Tests green; CI ready

---

## Review Checklist

Use this checklist when reviewing SpaceAge code or architecture:

### Code Changes (TDD)

- [ ] Test written first (Red–Green–Refactor)
- [ ] No production change without test (unless spike + note)
- [ ] Teardown calls `Game.ClearDictionaries()` to reset static registries
- [ ] `/player` updated if order syntax or battle rules changed
- [ ] Golden files validated by `/player` + human before commit
- [ ] Encoding preserved (1251 for engine XML, UTF-8 for manuals)

### XML Changes (Designer)

- [ ] `campaign/data.xml` or `campaign/gamein.1.xml` updated (encoding 1251)
- [ ] Module ids ≤ 6 characters
- [ ] Unknown module `group` or `location-type` → park in `designer/engine-wishlist.md` (no C#)
- [ ] `designer/` markdown spec updated first (galaxy, contracts, tech tree)
- [ ] Copy is hard-science (no magic, no FTL except Alderson JUMP)
- [ ] No test catalog edits (`Tests/data.xml` frozen)

### Campaign Play (GM)

- [ ] Scripts run in order: `init-run` → `reports` → `isolate` → (AI → `/player`) → `turn` → `next`
- [ ] Isolation boundary respected (no XML in faction folders)
- [ ] Text reports copied to `factions/NN/` (no `gamein`, `gameout`, `report.*.xml`)
- [ ] `play/README.md` kept accurate
- [ ] Script gaps documented (no new scripts written)

### Order Drafting (Player)

- [ ] Orders use only implemented syntax (`EOrderType`, `*Order.Parse`)
- [ ] `#faction`, `#modulestack`, `#person`, `#end` structure
- [ ] Conditions (prefixes) correct: `-` (immediate), `+` (long repeat), `@` (long once), `N` (repeat N)
- [ ] No invented unit ids (except aliases `/player` defined)
- [ ] UTF-8 draft → 1251 on copy to turn dir
- [ ] Manuals updated (rules, techs, battle)

### Player Manuals (Player)

- [ ] `player/rules.md` matches `EOrderType` + `*Order.Parse` (immediate vs long)
- [ ] `player/basic_technologies.md` L0–1 from `Tests/data.xml` (SampleGame), A–Z by level
- [ ] `player/advanced_technologies.md` L2+ from `Tests/data.xml`, with `requires` graph
- [ ] `player/campaign/basic_technologies.md` L0–1 from `campaign/data.xml` (campaign play), separate manual
- [ ] `player/battle.md` current (sides, diplomacy, tactics, hit/damage from `Battle.cs`)
- [ ] No SampleGame manual copied to campaign or vice versa
- [ ] Encoding UTF-8

### Architecture & Documents (Architect)

- [ ] `architecture/` docs aligned with code (no silent divergence)
- [ ] ADR written for major decisions (new boundary, new integration, new tech)
- [ ] Divergence from baseline requires explicit human approval (named)
- [ ] `architecture/docs-index.md` updated with new external references
- [ ] Diagrams current (Mermaid in markdown; date or version labeled)

### Website (Website-Developer + Website-Tester)

- **Developer:**
  - [ ] Read `architecture/delivery/website.md`, ADR-0007, `architecture/technology.md` (Website)
  - [ ] Pages within `/`, `/client`, `/turns`, `/rules` (Phase 1) or Phase 4 `/eta`, `/battle` only
  - [ ] `output: 'static'`, no SSR (unless architect + human approved)
  - [ ] CSS files; no CSS-in-JS
  - [ ] Vitest for schema, helpers (Red → Green)
  - [ ] `astro check` green
  - [ ] `status.json` placeholder schema (factions 2–11, no secrets)
  - [ ] Copy excerpted from Rules.txt; no new myth
  - [ ] No engine XML in Node

- **Tester:**
  - [ ] Scenario catalog updated (`website-scenarios.md` or `website/e2e/scenarios.md`)
  - [ ] Playwright tests cite scenario id (WS-* / UT-*)
  - [ ] `astro build` + `astro preview` + Chromium test
  - [ ] WS-007 viewport 390×844 (mobile)
  - [ ] WS-008: no passwords, gamein, order, report paths leaked
  - [ ] Failures traced to `/website-developer` with id + assertion

---

## Common Pitfalls to Avoid

| Pitfall | Impact | Fix |
|---------|--------|-----|
| Edit `Tests/data.xml` instead of `campaign/data.xml` | Test catalog diverges from campaign; golden mismatch | Keep `Tests/data.xml` frozen; edit campaign only |
| Write C# in `/game-designer` role | Wishlist items silently become code | Use `designer/engine-wishlist.md` for gaps; stop coding |
| Leak `gamein`, `gameout`, XML reports into faction folders | AI sees full state; isolation broken | Copy text reports to `factions/NN/` only; no XML |
| Overwrite golden without `/player` + human approval | Unvalidated changes to expected behavior | Two-gate process: (1) `/player` match/mismatch, (2) human approval |
| Double-convert encoding (UTF-8 ↔ 1251) | Corrupted files | Scripts handle conversion; drafts stay UTF-8, turn orders become 1251 |
| Silently diverge from architecture (SSR, React, DB, new auth) | Implementation at odds with baseline | Stop, present gap, require **explicit** human approval (named); then `/project-architect` records ADR |
| Declare website done from browser tour | Untested code ships | Vitest + Playwright + handoff to tester; no browser exploration as acceptance |
| Copy unimplemented verbs (JUMP, CONVERT, EMAIL) into `player/rules.md` | Players draft invalid orders | Use only `EOrderType` + `*Order.Parse` entries |
| Create NPC faction folders (01, 12, 13) | Isolation leaks; NPC folders in repo | Never commit these; NPC handled via contracts in XML |
| Submit `order.*` for NPC factions on `turn.ps1` | Script fails | NPC orders come from XML contracts and reactions; no `order.*.txt` |

---

## Future Work & Modernization

See `architecture/future-work.md` for deferred items:

- SDK-style csproj migration (from ADR-0001 baseline)
- Astro Phase 2–4 rollout (website enhancements)
- Visual battle tool (separate app, Electron or web)
- HTTP lobby + accounts (Phase 4+; architect-gated)
- SQL-backed game state (eventual; filesystem PBEM is current baseline)

All are gated behind ADR review and explicit human approval.

---

## Quick Reference: Agent Prompts

### When you need an agent, use one of these:

- **`/csharp-tdd`** — "Write a test for the new order. Red → Green → Refactor."
- **`/campaign-ai`** — "Play faction 5 for turn 3. Write the story and call /player."
- **`/campaign-gm`** — "Run turn.ps1 for run 2. Isolate reports."
- **`/game-designer`** — "Add three new technologies for L2. Update campaign/data.xml."
- **`/player`** — "Faction 3, turn 2. Objective: land on planet P00005. Draft orders."
- **`/project-architect`** — "We need SSR for the lobby. Design the new architecture."
- **`/website-developer`** — "Build the /turns page (faction roster, status, turn history)."
- **`/website-tester`** — "Test /turns with Playwright. Verify WS-004 (ten seats, factions 2–11)."

---

## Conclusion

**SpaceAge-2024** is a masterfully compartmentalized **game-and-campaign engine** with **six specialized Cursor agents** that coordinate through **explicit handoffs and isolation boundaries**. The codebase achieves:

- **Clean separation of concerns** (engine C#, XML catalog, player interface, website, architecture docs)
- **Non-overlapping agent roles** (TDD ↔ Player ↔ Designer ↔ GM ↔ Architect ↔ Website)
- **Hard rules and rulesets** that prevent conflicts and ensure quality gates
- **Encoding discipline** (Windows-1251 for engine XML, UTF-8 for manuals and player drafts)
- **Strong test strategy** (NUnit 4, namespace layers, golden file validation)
- **PBEM + campaign infrastructure** (filesystem-based, isolation, text-report driven)
- **Modern web presence** (Astro static site with Vitest + Playwright, Phase-gated rollout)

Use this review as a reference guide for **code navigation, contribution patterns, and agent handoffs**. When in doubt, **refer to the hard rules in the agent definitions and rulesets** — they are the source of truth for each role's boundaries and responsibilities.


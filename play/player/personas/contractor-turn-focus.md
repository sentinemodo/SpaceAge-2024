# Contractor turn focus doctrine

Charter contractors (persona **contractor**) choose **one primary focus per quarter** using these long-run weights — pick what the latest report and rumors make achievable, not a random roll:

| Weight | Focus | Meaning |
|--------|--------|---------|
| **50%** | **contract** | Advance or complete an open UN `CONTRACT` / `give-module` job (stage modules, `TRANSFER`, presence, research points). |
| **25%** | **defence** | Fauna lanes, escorts, orbit visibility; `DECLARE FACTION` when rumors confirm contact. |
| **15%** | **economy** | Grant loop (`@produce`, `@use`, staging iron/titani/food/oil/cash) for upcoming contracts. |
| **10%** | **research** | Moblab, `RESEARCH`, tech copies required for contract rewards or wreck charters. |

**Alignment:** a **contract** quarter often requires **economy** staging, **research**, or **defence** in the same quarter — name supporting work when it serves the active contract.

## Required in every `story.md`

Include **## Turn priority** immediately after the title (and after **## Review** when turn ≥ 2):

- **Focus this quarter:** `contract` | `defence` | `economy` | `research` (one primary)
- **Rationale:** one sentence tied to this report or rumor release
- **Active contract:** `CTxxxx` — **required when focus is contract** (title or id from report/rumors + next milestone); omit the line when not contract-focused
- **Supporting work:** optional one line when economy/research/defence aligns with the active contract

Tactical bullets must execute the chosen focus; narrative must mention the focus and named contract when applicable.

## Contract decomposition (orders drafting)

When **Active contract** is set, break the CT into **tactical objectives** sized for one quarter of order syntax:

- Name **scout hops** (region ids from report exits), **convoy stacks** (`grndtr` + `mobctr` / `[engtrk]`), **field `USE`** at the contract location, and **transfer** milestones — not a single story bullet “deliver four farms.”
- **Prerequisites chain**: activate **`cplant`** → max **`sdrill`** output (energy permitting) → **`@give all to cargob`** + **`2 use tminng` / `use iminng` / `10 use hcdril`** on one drill → second **`cplant`/`sdrill`** when carbon and iron support it. With bank **above ~5000**, **`grant item`** bootstrap for factory **`USE`** when the market does not sell inputs.
- **CEO `[exmgmt]`**: **`#person <ceo-id>`** + **`stack <factry|sdrill|cplant-id>`** for the quarter (+25% productivity); **`stack <hq-id>`** when done.
- **Oil before CT convoy**: **`grndtr`** scout to the **first neighboring region**; later **`[engtrk]`** (production group) field-builds power and drills at oil sites; parallel eng trucks for **`agrplx`** (oil-heavy).
- **Economy**: cap **sell** to market **buy**; **one-shot `get`** for cplant fuel; never haul completed **`[farms]`** on cargo trucks.

Player-agent RAG: `tools/player-agent/Draft/contractor-story-to-orders.md`.

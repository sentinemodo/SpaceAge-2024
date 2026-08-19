# Contracts (during play)

Live triggers: **`give-module`** (deliver N modules of a type to a receiver stack) and **`research`** (accumulate research points on a wreckage/anomaly stack at the same location). Rewards: **technology copy** or **unit**.

Always set `title` and `flavour`. Flavour is **hard science**: spectra, Δv, isotopes, epidemiology — not prophecy.

Issuer is usually faction `1` (**United Star Nations**). `location` is a **region** id (required by loader).

## Seed at turn 1

The map is **two occupied basins** (Arbor, Anvil) plus eight empty systems. Do **not** plant a wreck on every system at t=1, and do **not** put wrecks on Arbor or Anvil grids.

**Live at t=1 (no new trigger types):**

1. **UN markets** on the cities (not contracts): Assembly buy/sell food and labour; Slagport buys food at a premium. See `galaxy.md`.
2. **2–4 UN `give-module` jobs** that force local trade inside a starting system, e.g. deliver `farms` or `wnplnt` to **Slagport** / **Isotope**, or `cdrill` to **Tidewatch**. Receiver = that UN city or its nested garrison. `baseline` = current recursive count.
3. **At most one wreck rumour per occupied system**, and only **off** the habitable grids: Helios belt metal rock or an Aeolus ice moon; Fomal carbonaceous belt or an ice moon. `research`, L3–L4 reward, mixed branches. Flavour as a UN survey charter, not a free skip of the resource split.
4. **Empty systems:** no t=1 contracts. Wrecks there wait until someone can actually reach them (year 1+).

Do not put L10 rewards on the map at t=1. Item shipments (food tonnes to Anvil) are **markets**, not `give-module`.

## Vectors to inject later

Pick **one** new vector per mid-game turn unless the table is quiet. Prefer empty regions and existing NPC stacks over inventing new geography.

| Vector | Player beat | Live XML shape | Later (wishlist trigger) |
|--------|-------------|----------------|---------------------------|
| **UN charter / colony job** | Deliver `wnplnt`/`farms`/`cdrill` to a UN town for `ctypln` / `servic` | `give-module` as SampleGame `gamein.2_contract.xml` | automatic growth |
| **Alien threat** | Armed NPC stack in a surveyed orbit (empty system or outer occupied moon); deliver `inftry`/`tanks`/`alndrn` to a UN garrison **or** research the hulk. Optional later: UN asks for a **named group** (laser PD `pdltur`, EW `ewantn`, pbpd `ciwst`) | `give-module` to NPC `inftry`; or `research` on wreck | timed spawn, hostility flip |
| **Rogue / pirate colony** | NPC `city` or `smhabi` on a previously empty region (starting hinterland or a belt); buy/sell food and stolen ore; deliver `lawenf` / `inftry` | `give-module` `inftry` to NPC nested garrison | pirate `ATTACK` orders (GM/`/player` as NPC) |
| **Asteroid bombardment** | Impactor flavour on a **habitable** cell (Arbor or Anvil, or later Graph). Two forms: (A) **NPC-scripted impact** — timed event spawns destruction on a region, reduce `food`/`h2o2`, damage UN city, deliver `engshp`/`spare` to repair; (B) **Player-initiated** — L8 `impgde` tech lets a faction `TOW` + `IMPACT` a belt asteroid onto a target planet region. Weeks of visible approach; defenders intercept by destroying the tow stack. On impact: target region resources halved, capacity zeroed, all stacks take mass casualties. Strategic weapon of last resort | `give-module` `engshp` for NPC-scripted repair; `IMPACT` order + `Events` pipeline for player-initiated | region damage effect, timed impact, `TOW`+`IMPACT` orders, interception mechanics |
| **Space fauna** | Belt hazard, faction 1, no tech. Research for `exobio` **or** destroy | `research` + tech `exobio` | fauna consume/attack without DECLARE |
| **Anomaly** | Dust or ice region, small `survsc` wreck. `research` for `survts` / `deepsc` / `radtol` | `research` | SEE-only until approached |
| **Empty-system wreck** | First survey of SS0003–SS0010 finds L3–L6 wreckage (production, propulsion, research, military mixed) | `research` on faction-1 hulk | — |
| **NPC colony development** | UN `city` `quantity`++ or new `farms`; first UN flag on an empty system | `give-module` | automatic growth |

Wreck target stacks: faction 1, wrecked or inert module (`alnhul`, `robofc`, `he3aut`, or a unique `arkhul` only in SS0006/SS0010 as a **powered-down** hulk with `quantity` 0 or 1 and no crew). `points` 16–64.

## Writing a contract

1. Name `CTnnnn` unused.
2. Put the receiver or wreckage **in the galaxy XML** first (same `gamein`).
3. `baseline` for `give-module` = current recursive count of that module type on the receiver (SampleGame Sydney garrison `inftry` baseline 1).
4. Reward tech must exist in `campaign/data.xml`.
5. One-sentence handoff: who should notice it in the report (region event + faction event).

## Cadence

| Game year (4 turns) | Contracts in flight (whole map) |
|---------------------|----------------------------------|
| 0 (turns 1–4) | UN markets live; 2–4 UN jobs on Arbor/Anvil; 0–2 off-grid wreck rumours in the occupied pair; **no** empty-system wrecks yet |
| 1 | + survey/charter jobs toward SS0003–SS0010; + pirates in 1–2 **starting** belts; first empty-system wreck when a ship can be there |
| 2 | + fauna or anomaly; +1 alien-threat orbit (prefer an empty system or outer moon, not Assembly Basin) |
| 3+ | L8–L10 wreck rumours; contested UN colonies; bombardment scare on a habitable grid |

Never starve players of something to do **on Arbor or Anvil** (hinterland, local dust/belt, UN towns) while the eight empty systems wait. Inter-system trade is the mid-game prize, not the only verb on turn 1.

## Military threat cadence (turns 1-20)

Goal: force meaningful defensive buildup (garrisons + a mobile responder) without creating unavoidable faction wipes.

### Hostility flip rules (planet-linked)

- **Arbor First** starts neutral and tracks Arbor-only trigger state.
- **HCS** starts neutral and tracks Anvil-only trigger state.
- Faction flips to hostile when the first spaceship module is completed on its bound planet:
  - Arbor First flips on first spaceship module built on Arbor.
  - HCS flips on first spaceship module built on Anvil.
- Flip effect: immediate `DECLARE` hostility to the builder faction, and standing hostile posture to any faction with active spaceship production on that planet.
- Neutral factions can still trade with UN cities after flip; hostile factions cannot rely on city adjacency safety.

### Raid pressure profile (medium target)

| Turn band | Expected raid rhythm | Typical objective | Failure outcome ceiling |
|-----------|----------------------|-------------------|-------------------------|
| 1-6 | One hostile probe every 2-3 turns per active hostile NPC faction | Probe HQ perimeter, test garrison | Local module losses, not HQ deletion |
| 7-12 | One coordinated raid every 2 turns, occasional paired pressure (surface + orbit) | Disrupt production and logistics lanes | Temporary regional denial, recoverable in 1-2 turns |
| 13-20 | One major raid every 2 turns with one reinforcement chance | Force defended convoy/mobile response play | Severe attrition if ignored, still non-terminal |

### Threat target priorities

1. Exposed convoy / shuttle stacks.
2. Outlying extraction/production stacks.
3. HQ-adjacent static infrastructure.
4. Faction HQ core stack only if defender has already committed response and lost initiative.

### Contract templates (combat-forward)

Use these alongside economy contracts once hostile state is active.

1. **Emergency defense**: survive raid window, keep city/HQ module count above baseline.
2. **Punitive strike**: destroy a named hostile forward stack before reinforcements land.
3. **Interdiction**: deliver combat modules (`inftry`, `tanks`, `pdltur`, `ewantn`, `ciwst`) to a defensive receiver before a timer.
4. **Route clearance**: clear fauna/rogue AI blocker stack from a designated resource lane.

## Threat stat benchmarking (sample stacks vs expected defense)

Numbers are balancing anchors for campaign seeding and contract tuning, not hard engine laws.

### Stage A (turns 1-6)

| Profile | Sample composition | Attack | Damage | Effective HP |
|---------|--------------------|--------|--------|--------------|
| Expected player defense | HQ garrison + 1 militia stack (`inftry`, light support) | 18-24 | 16-22 | 160-220 |
| Arbor First raider patrol | 1 raider stack (`inftry`-lean) | 12-16 | 10-15 | 110-150 |
| HCS interdiction team | 1 raider stack + harassment support | 13-17 | 11-16 | 120-155 |
| Space fauna guard (L0-L2) | Single aggressive organism cluster | 10-14 | 12-18 | 90-130 |
| Rogue AI sentry | Damaged drone/tooling remnant | 11-15 | 11-15 | 100-140 |

Target ratios vs expected player defense:
- `attackRatio`: 0.60-0.75
- `damageRatio`: 0.60-0.75
- `ehpRatio`: 0.60-0.75

### Stage B (turns 7-12)

| Profile | Sample composition | Attack | Damage | Effective HP |
|---------|--------------------|--------|--------|--------------|
| Expected player defense (prepared) | HQ garrison + 1 mobile responder + one defense module lane | 32-42 | 30-40 | 300-420 |
| Hostile coordinated raid wave | 2 stacks (surface spear + support) | 24-33 | 24-34 | 240-340 |
| Fauna alpha nest guard | 1 heavy fauna stack + juvenile support | 22-30 | 26-34 | 210-300 |
| Rogue AI drill complex guard | 1 fortified AI core + 1 drone escort | 25-34 | 23-32 | 230-320 |

Target ratios vs prepared defense:
- `attackRatio`: 0.60-0.75
- `damageRatio`: 0.60-0.75
- `ehpRatio`: 0.60-0.75

Unprepared local static defense may face wave ratios up to `0.80-0.95`.

### Stage C (turns 13-20)

| Profile | Sample composition | Attack | Damage | Effective HP |
|---------|--------------------|--------|--------|--------------|
| Expected static player defense | HQ + fixed orbit/surface defenses | 44-58 | 42-56 | 440-620 |
| Expected defense with mobile response | Static defense + one responder stack | 56-72 | 54-70 | 560-760 |
| Major hostile raid wave | 2-3 stacks (lead + flank + optional orbit harasser) | 50-64 | 48-62 | 500-680 |
| Heavy rogue AI complex | Core stack + perimeter stack | 48-62 | 46-60 | 490-660 |

Target ratios:
- vs static defense: `1.00-1.15` allowed for strongest wave.
- vs static + mobile response: cap at `0.85-0.95`.

### Tuning caps and safety rails

- No single spawned wave should one-pass delete a faction HQ.
- If any sample exceeds ratio caps on `attack`, `damage`, and `ehp` simultaneously, reduce by:
  1. lowering stack quantity,
  2. reducing high-damage module share,
  3. delaying reinforcement timing.
- Successful defense should usually retain operational control with recoverable losses (not total force reset).

---

## Phase 1: Foundation (turns 1–10)

Early contracts that teach trade, off-world logistics, and military basics. All completable with L0–L1 tech and starting resources.

| Contract | Player delivers | Turns to complete | Reward | Design purpose |
|----------|----------------|-------------------|--------|----------------|
| **Feed Slagport** | 200 `food` over 4 turns (cargo shuttle runs Arbor→Anvil) | 4 | 3 000 `cash` | Arbor→Anvil food trade intro; teaches shuttle logistics and cross-system commerce |
| **Selene survey** | `cdrill` + 5 `terran` crew delivered to Selene surface | 6 (4 travel + 2 setup) | 5 000 `cash` + 15 `titani` | First moon mission; forces orbital launch and short-hop navigation |
| **Tidewatch garrison** | 2 `inftry` modules delivered to Tidewatch | 3 | 2 000 `cash` + 10 `iron` | Military intro; requires `frminf` USE and ground transport |
| **Scoria assay** | `shuttl` + `sdrill` to Scoria, return spectrometric report | 10 (8 travel + 2 work) | 8 000 `cash` + titani supply route (Scoria market opens) | First interplanetary; commits a shuttle to a multi-turn round trip |

## Phase 2: System industry (turns 10–20)

Major multi-turn projects requiring L2–L3 tech, orbital construction, and sustained logistics chains.

| Contract | Player delivers | Turns to complete | Reward | Design purpose |
|----------|----------------|-------------------|--------|----------------|
| **Selene NPC city** | `city` module (150 `iron`, 80 `titani`, 40 `copper`, 30 `silici`) + 500 `food` + 50 `terran` crew on Selene | 15–20 | 30 000 `cash` + L3 tech grant + moon trade route | Major multi-turn megaproject; proves industrial capacity |
| **Orbital research station** | `smhabi` + `crwqrt` ×2 + `cmplib` assembled in Arbor orbit | 10–12 | 20 000 `cash` + research bonus (output ×1.5 on that stack) | Orbital construction; requires habitat, crew quarters, and library in zero-g |
| **Scoria mining outpost** | `factry` + `cdrill` ×2 + `smhabi` + 100 `food` on Scoria | 12–15 | 15 000 `cash` + resource rights (Scoria extraction capacity doubles) | Industrial expansion to a barren world; sustained food supply chain |

## Phase 3: Inter-system expansion (turns 20–40)

Endgame contracts requiring L5+ tech, rare resources from multiple systems, and jump-drive or deep-space capability.

| Contract | Player delivers | Turns to complete | Reward | Design purpose |
|----------|----------------|-------------------|--------|----------------|
| **Large orbital habitat** (city-scale) | `whlhul` (400+ `iron`, 200+ `titani`, 100 `copper`, 50 `nickfe`, 30 `alumin`) + life support + `farms` ×10 + 2 000 `food` | 20–30 | 80 000 `cash` + 500 `terran` pop + orbital city rights | Requires L6+ centrifugal habitat tech and asteroid metals from Gleam/Spare |
| **Colony on Graph** | `city` + 500 `food` + 200 `iron` + 100 `titani` + habitats + 100 `terran` crew via jump drive | 25–35 | 100 000 `cash` + Graph system claim (exclusive exploitation zone) | Requires jump drive; first inter-system colony; proves self-sufficiency |
| **AP fortress** | military modules (200 `iron`, 100 `titani`, 60 `copper`, 30 `uraniu`) + `smhabi` ×2 + garrison | 15–25 | 60 000 `cash` + chokepoint toll rights (collect transit fees) | Strategic endgame; controls an Alderson point chokepoint |

## Wreckage research contracts (all tiers)

Each wreckage site uses `ResearchWreckageTrigger` — a faction-1 `ModuleStack` at the target region with a contract requiring N research points. First faction to complete wins. Full catalog: **[anomalies.md](anomalies.md)**.

### Contract XML template

```xml
<contract type="research" target="W90001" points="20"
          name-en="Crashed lander analysis"
          description="Direct research modules at the alien wreckage to unlock its secrets.">
  <reward type="technology" name="areact"/>
  <reward type="deposit" region="R..." item="iron" quantity="50" renewable="false"/>
</contract>
```

### Reward types by tier

| Tier | Typical rewards | Contract visibility |
|------|----------------|-------------------|
| Early (20–40 RP) | L1–L2 tech free, 1–2 modules, finite deposits (30–80) | Visible at game start |
| Mid (60–100 RP) | L3–L5 tech, military items (4–6), skill unlocks, deposits (20–60) | After body survey |
| Late (120–200 RP) | L6–L7 tech, advanced modules, rare deposits (40–100), skill chains | On SURVEY discovery |
| Deep (250–400 RP) | L8–L10 tech hints (-50% to -75%), ark components, unique items | GM injection or SURVEY |
| Stellar (180–350 RP) | L7–L9 tech (solar/coronal/drone), paired modules | Shield + survey 0.1 AU |
| Drone (160–220 RP) | L5–L7 drone tech, `wrkdrn` items (20–50), `drnfac` modules | With late tier |

### Engine reward types (wishlist)

- `<reward type="technology">` — grants tech to winner (skips research cost)
- `<reward type="technology-hint" discount="0.50">` — reduces cost by fraction
- `<reward type="deposit">` — seeds finite resource on wreckage region
- `<reward type="module">` — gives N modules to winner's stack
- `<reward type="item">` — gives N items to winner's stack
- `<reward type="skill-unlock">` — adds skill to faction Known Skills (bypasses tech gate)
- `<reward type="effect">` — unique per-site (e.g., permanent 2x research)

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
| **Asteroid bombardment** | Impactor flavour on a **habitable** cell (Arbor or Anvil, or later Graph). Reduce `food`/`h2o2`; optional damaged UN city. Deliver `engshp` or `spare` | `give-module` `engshp` or a repair module | region damage effect, timed impact |
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

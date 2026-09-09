# Contracts (during play)

Live triggers: **`give-module`** (deliver N modules of a type to a receiver stack) and **`research`** (accumulate research points on a wreckage/anomaly stack at the same location). Rewards: **technology copy** or **unit**.

Always set `title` and `flavour`. Flavour is **hard science**: spectra, Δv, isotopes, epidemiology — not prophecy.

Issuer is usually faction `1` (**United Star Nations**). `location` is a **region** id (required by loader).

## Seed at turn 1

The map is **two occupied basins** (Arbor, Anvil) plus eight empty systems. Do **not** plant a wreck on every system at t=1, and do **not** put wrecks on Arbor or Anvil grids.

**Live at t=1 (no new trigger types):**

1. **UN markets** on the cities (not contracts): Assembly / Slagport books in [`economy.md`](economy.md) (mirrored in `galaxy.md`). Item shipments stay markets, not `give-module`.
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

## Hostile faction bounties (Arbor First / HCS)

After a planet's anti-starflight faction flips hostile (first spaceship module built on Arbor → **Arbor First**; on Anvil → **HCS**), UN issues a **capture charter**. Seed locations and stack ids: [`galaxy.md`](galaxy.md) (Rootfast `120001` on `R00014`; Crusthold `130001` on `R00060`). Story: these factions opposed leaving the well, but they kept the best **planetary industry** manuals — self-sufficient farming (Arbor First) and crust/ore assay (HCS).

### Dual reward (UN pay + finders keepers)

| Layer | What the player gets | How |
|-------|----------------------|-----|
| **UN contract pay** | Cash (and optionally a unit) | Live `reward-type` cash/unit when trigger completes |
| **Finders keepers** | Technology copy consistent with that faction's archives | Same completion: grant tech to the completing faction (not shared with UN) |

Do **both**. Cash alone is weak motivation for a multi-turn military campaign; tech alone skips the UN story beat.

### Story → technology map

| Hostile faction | Planet | Archive flavour | Tech reward (grant copy) | Why it fits |
|-----------------|--------|-----------------|--------------------------|-------------|
| **Arbor First** | Arbor | Closed-loop soil/crop genetics; "never need another world" | `afrmng` **and** `hydrop` | Organic world specialists |
| **HCS** | Anvil | Crust assay, hard-rock extraction, metal beneficiation | `nminng` **and** `gminng` | Metal-world specialists; Fe-Ni + precious assay |

If the player already knows a listed tech, substitute the next unused production extract in that branch (`hydrop`→skip; `nminng`→`gminng` or a finite deposit reward on a named region).

### Contract shape (design)

1. **Trigger (wishlist until live):** `destroy-stack` or `capture-stack` on Rootfast `120001` / Crusthold `130001` (or deliver N `inftry`/`tanks` to a UN garrison *and* hold the hostile city region for N weeks — interim `give-module` + GM resolve).
2. **Issuer:** UN (faction 1). Flavour: charter of pacification / quarantine of anti-expansion militias.
3. **Rewards on complete:**
   - Cash band: 8 000–15 000 early; 20 000–40 000 if the raid is mid-game.
   - Technology: table above (finders keepers).
4. **Optional second contract:** "Seize the archive vault" nested stack (`research` wreckage on captured city) for a **second** tech if the first was already known.

### Live XML until new triggers land

Use a UN `give-module` war-supply contract (`inftry`/`tanks` to UN garrison) with `reward-type="technology"` for the first archive tech, and GM/event grant of the second tech when the hostile city is actually captured in play. Prefer migrating to a single multi-reward completion when TDD adds `destroy-stack` / multi-reward contracts (see `engine-wishlist.md`).

### Cadence

| Event | Contract |
|-------|----------|
| Hostility flip on Arbor | UN "Contain Arbor First" — farming archive rewards |
| Hostility flip on Anvil | UN "Contain HCS" — mining archive rewards |
| Hostile city captured | Archive finders-keepers resolves; raids stop or downgrade to remnants |

Do not put these techs on t=1 free wrecks on the same planet — they are the **payoff for fighting the hostility arc**.


# Stack visibility (stealth vs detection)

Designer intent for TDD. Aligns with [`docs/human/rules.md`](../../docs/human/rules.md) STACK / SEE paragraphs and replaces the ad‑hoc underwater-only hide in `ModuleStack.Visible`.

## Goal

Foreign module stacks (and nested children in reports / `SEE`) are shown only when **some friendly observer** can overcome the target’s **effective stealth**. Stealth and detection are **integer levels** (default 0 unless catalog says otherwise).

**Visibility rule (per observer–target pair):**

```text
effectiveStealth(target, observer) =
    target.baseStealth
  + target.nestDepth
  + target.factionStealthBonus        // e.g. fauna +1 vs orbital observers
  + (target.isUnderwaterStealthy ? 1 : 0)
  + geoStealth(observer, target)

observerDetection(O, target) =
    O.baseDetection
  + O.technologyDetectionBonus
  + (O.location is Orbit of target body ? 1 : 0)   // orbit sweep bonus
  + underwaterChannelBonus(O, target)             // see below

visible iff observerDetection(O, target) > effectiveStealth(target, observer)   // strict
```

**Faction visibility:** target is visible to faction F iff owner bypass / NPC bypass applies, or ∃ **formed** observer stack O owned by F with `O.Size > 0` satisfying the strict inequality. Use the **best** observer (maximum margin).

Own units remain always visible to their owner in the region (human rules); owner bypass stays in `Visible`.

### Default “day one” behaviour (campaign detection 1, orbit +1)

| Observer | Target | Typical outcome |
|----------|--------|-----------------|
| Ground stack, **same region**, detection 1 | Foreign **root** (nest 0), not underwater | **Seen** (1 > 0) |
| Ground stack, **same region**, detection 1 | Foreign **nested** once (nest 1) | **Not seen** (1 > 1 false) |
| **Orbit** on planet (detection 1 + orbit +1 = **2**) | Any **root** on that planet/moon regions, not underwater, within regional reach | **Seen** (2 > stealth) |
| Orbit sweep | **Underwater-stealthy** root | **Not seen** without extra channel (underwater +1 stealth; orbit +1 does **not** apply to underwater targets — see detection) |
| Ground, **adjacent region** (+1 regional stealth) | Foreign root | **Not seen** with detection 1 (1 > 1 false) |

Regional distance is what keeps “patrol in one tile” from seeing the next tile without better sensors or orbit.

## Stealth components (target)

| Component | Value | Notes |
|-----------|-------|--------|
| Base | `ModuleType.Stealth` (default 0) | Catalog + stack tech later |
| Nest depth | +1 per level under root | Root on region/orbit/belt = 0 |
| Underwater | +1 if `IsUnderwaterStealthy` | Same predicate as today |
| **Regional distance** | **+1 per region hop** | Shortest path on **same body** region graph (orthogonal exits); observer region → target region; 0 if same region; orbit↔surface on same body = 0 hops (body-level sweep) |
| Geography (body/system) | see below | Cumulative with regional hops |
| **Fauna owner** | **+1** when observer gets the **orbit +1** bonus | Factions **14–17** only; Arbor First / HCS (**12–13**) use standard stealth |

## Geography stealth `geoStealth(observer, target)` (body / system)

Resolve each stack to a **visibility anchor**:

- `SpaceSystemId`
- `PlanetId` (parent planet; belt inherits its parent planet when belt is `<belt>` under planet)
- `MoonId` (null on planet surface/orbit)
- `LocationKind`: `Region` | `Orbit` | `Belt` | …

Apply **cumulative** penalties (all that apply):

| Condition | Stealth |
|-----------|---------|
| Different `SpaceSystemId` | +10 |
| Different `PlanetId` (both resolved and not equal) | +1 |
| Different `MoonId` (both non-null and not equal) | +1 |
| Target on **`Belt`**, observer not on that belt | **+1 “belt orbit”** (asteroid / ring location) |

**Belt example (decided):** observer on **planet region**, target on **belt** around that planet → different planet anchor is **not** triggered (same parent planet), but **belt orbit +1** applies; if the engine also treats belt vs surface as cross-**planet** anchor for penalty, use **+1 planet +1 belt = +2** total as in playtest intent (“see asteroid from surface”). TDD fixture: one planet, one belt child, assert **+2** geo component (document exact anchor rules in `VisibilityGeo` tests so planet+belt matches designer +2).

Same **region** instance: regional hops = 0; geo may still apply for cross-body observers.

**Cross-layer same body:** observer in **orbit** of body B, target in **region** on B: regional hops **0**; orbit detection +1 applies.

## Detection `observerDetection(O, target)`

| Source | Value | Notes |
|--------|-------|--------|
| Base | `ModuleType.Detection` (default 0) | Campaign “standard” sensor units → **1** when attrs land |
| Stack technologies | sum `Technology.DetectionBonus` | Catalog |
| Modules / skills | e.g. `survsc` +1 same-region | Narrow scope |
| **Orbit bonus** | **+1** when O’s location is **`Orbit`** of the **target body** (planet/moon the target is on) | Enables planet-wide root sweep |
| Underwater channel | +1 when target is underwater-stealthy **and** (faction `HasUnderwaterPresence` in target **region** OR own `IsShipHullType` on that body’s orbit) | Replaces old boolean hide; **orbit +1 does not apply** when target is underwater-stealthy (surface orbital pass only) |

**Observers (decided):** **Unformed** stacks (`!IsFormed`) **never** observe. **Size ≤ 0** stacks do not observe.

Detection is **not** reduced by geography; distance adds **stealth** only.

## SEE order (engine)

**Syntax (extends existing person forms):**

```text
SEE <stack|newN> [AT <region-id>|ANYWHERE]
SEE PERSON <id> [AT <region-id>|ANYWHERE]
SEE <id> PERSON [AT <region-id>|ANYWHERE]
```

| Mode | Meaning |
|------|---------|
| *(default)* | **Current region:** target must be in the **observer subject’s region**, and `CanSee(subject, target)` must hold (subject is the observing stack; uses faction’s best observer among stacks **in that region** for the inequality, or subject-only — TDD pick **subject stack’s detection stats** with geography from **subject’s location**; if margin insufficient, order fails). |
| **`AT R00001`** | Same check, but observer location context is **that region** (subject must be able to “look into” AT region: same body; subject’s faction has presence on that body). Targets must lie **in AT region** (or nested under a root in AT region). |
| **`ANYWHERE`** | Succeeds if the **subject stack** can see the target **anywhere** the subject’s detection reaches: evaluate `CanSee(subject, target)` with geography from **subject’s current location** to **target’s location** (all regional/body/system stealth terms); no restriction that target shares subject’s region. |

On success: event `saw {target} in {location}.` (unchanged). On failure: silent (today’s behaviour).

`CanSee` shared with `ModuleStack.Visible(faction)` for report filtering (faction uses **best** observer anywhere in system after presence gate; SEE uses rules above).

XML: add optional `at-region="R00001"` or `anywhere="yes"` on `<see>` when TDD extends save format.

## Surfaces to wire

| Surface | Behaviour |
|---------|-----------|
| `ModuleStack.Visible(Faction)` | Best **formed** observer in faction after system presence gate |
| `ModuleStack.Report` nested children | Skip child subtree when `!child.Visible(faction)` |
| `SeeOrder` | Parse `AT` / `ANYWHERE`; call `Visibility.CanSee` |
| Exit hints | Unchanged |

## Catalog (campaign)

- `<module detection="1" stealth="0"/>`
- `<technology detection-bonus="1"/>`
- Fauna modules: optional `stealth="1"` redundant if faction rule hard-codes +1 vs orbit

Default workshop orbital / scout: **detection 1**.

## v1 vs v2

| | **v1 (this ship)** | **v2 (later, optional)** |
|---|---------------------|---------------------------|
| **Reports / STACK intent** | Foreign **town root** visible when detection wins; **nested** stacks omitted when `detection > stealth` — same as human “only report a town if units stacked under one” (garrison not listed separately) | **No extra obfuscation** beyond v1 report rules |
| **Also v2** | — | Galaxy home-system blur (`SpaceSystem.Visible` wishlist), person stealth, broad stealth/detection tech tree (`grokbot`) |

Comparison uses **`detection > stealth`** (strict).

## TDD plan (red → green)

1. **`VisibilityGeo`** — region BFS hops; planet/moon/system/belt penalties; fauna +1; strict `>`.
2. **`NestDepth`**, orbit +1, underwater channel, belt +2 fixture.
3. **`ModuleStack.Visible`** — migrate `TUnderwaterVisibility`.
4. **`SeeOrder`** — `AT`, `ANYWHERE`, parse + XML attrs.
5. **Report nesting** golden.
6. **Performance** — observer index (wishlist); profile env.

## Performance / load conservation

Unchanged from prior revision: system presence gate, per-faction observer index, `(stackId, factionId)` cache, avoid O(stacks²). Regional hop cache per (observerRegion, targetRegion) on same body.

## Migration

- Subsumes boolean underwater hide into stealth + channels.
- [`play/player/rules.md`](../player/rules.md) SEE + visibility updated for engine **0.8.003**.

## Decisions (closed)

1. **Unformed** units do **not** observe.
2. **Belt:** from planet **region** to unit on **belt** around that planet → **+2** geo stealth (planet/surface vs belt location + belt orbit term — implement as documented in belt fixture).
3. **Fauna 14–17:** **+1 stealth** when observer would receive **orbit +1** detection; **12–13** standard.
4. **v1 vs v2:** table above.

# Alien Wreckage and Anomaly Catalog

Canonical list of all discoverable wreckage sites and space anomalies. Each entry becomes a faction-1 `ModuleStack` on the target region with an associated `ResearchWreckageTrigger` contract.

**Discovery:** anomalies are hidden until the body is **surveyed** (`SURVEY` order, 1 RP per region). Once surveyed, wreckage stacks become visible to the surveying faction.

**Mechanics:** research stack co-located with wreckage contributes RP toward the contract trigger. First faction to reach required points wins the reward.

---

## Early tier (turns 1–10, starting system moons, 20–40 pts)

| # | Id | Name | Location | Flavour | RP | Reward |
|---|-----|------|----------|---------|-----|--------|
| 1 | `W90001` | Crashed lander | Scoria surface | Scorched hull half-buried in regolith, pre-human design | 20 | L1 `areact` tech free + 50 `iron` finite deposit |
| 2 | `W90002` | Signal repeater | Pyre polar region | Periodic microwave pulse from beneath sulphur crust | 25 | L2 `corpmg` tech + 30 `silici` deposit |
| 3 | `W90003` | Frozen probe | Selene ice field | Vitrified probe embedded in ancient ice shelf | 20 | L1 `hydstg` tech + 40 `h2o2` deposit |
| 4 | `W90004` | Mineral cache | Scoria highlands | Sealed container of refined metals, alien isotope markers | 30 | 80 `titani` + 40 `copper` finite deposits |
| 5 | `W90005` | Derelict shuttle | Aeolus outer ice | Small craft adrift near ice-giant ring, intact fuel cells | 25 | 2x `rctdrv` modules |
| 6 | `W90006` | Antenna array | Pyre equatorial | Grid of collapsed dish antennae, passive EM collector | 30 | L2 `spctrl` tech + `survsc` module |
| 7 | `W90007` | Reactor fragment | Selene crater | Shattered power core, residual decay heat detectable | 35 | L2 `urfiss` tech + 30 `uraniu` deposit |
| 8 | `W90008` | Biofilm sample | Aeolus subsurface | Silicate matrix preserving alien microbial remnants | 25 | Unlock `xenoB` skill |
| 9 | `W90009` | Navigation buoy | Fomal ice moon | Tumbling beacon broadcasting obsolete star charts | 20 | L1 `iondrv` tech + AP map reveal |
| 10 | `W90010` | Tool cache | Fomal rock moon | Crate of precision-machined implements, unknown alloys | 30 | 3x `indust` modules |
| 11 | `W90011` | Habitat pod | Anvil highlands | Pressurised capsule, atmosphere still viable, empty | 35 | 2x `crwqrt` modules + 20 `terair` deposit |
| 12 | `W90012` | Medical vault | Fomal ice moon 2 | Cryogenic storage of alien pharmaceutical compounds | 30 | Unlock `medcA` skill early + 10 `ammoni` |

---

## Mid tier (turns 10–20, Helios/Fomal non-starting bodies, 60–100 pts)

| # | Id | Name | Location | Flavour | RP | Reward |
|---|-----|------|----------|---------|-----|--------|
| 13 | `W90013` | Power station ruin | Helios belt large metallic | Half-melted solar collector array on a tumbling asteroid | 60 | L3 `he3fus` tech + 2x `energs` modules |
| 14 | `W90014` | Weapons locker | Scoria deep caves | Armoured vault behind collapsed tunnel, radiation hot | 70 | 4x `lasopt` items + L3 `lasres` tech |
| 15 | `W90015` | Engine test bed | Aeolus orbit | Orbital frame with exhaust-scarred nozzles, fuel residue | 80 | L4 `iondrv` tech + `ioneng` module |
| 16 | `W90016` | Drone swarm husk | Fomal belt | Cluster of inert drones, formation preserved by vacuum | 75 | L4 `drnhng` tech + 6x `drnswm` items |
| 17 | `W90017` | Refinery hulk | Helios belt carbonaceous | Industrial vessel, processing bays still pressurised | 90 | L4 `indprs` tech + 2x `factry` |
| 18 | `W90018` | Shield emitter | Pyre orbit | Curved plate generating faint EM field when powered | 80 | 4x `psnshd` items + `capshd` module |
| 19 | `W90019` | Star chart archive | Aeolus gas giant orbit | Data crystal array — alien mapping of local cluster | 60 | L3 `jmpdvr` tech + all AP connections revealed |
| 20 | `W90020` | Agricultural dome | Helios belt ice | Sealed greenhouse with alien crop genetics, still viable | 70 | L4 `afrmng` tech + unlock `farmA` skill |
| 21 | `W90021` | Comms relay station | Fomal gas giant orbit | Long-range transmitter, power cells drained | 85 | L5 `tacnet` tech + `comsat` module |
| 22 | `W90022` | Kinetic accelerator | Anvil deep crater | Rail-launcher buried under ejecta blanket | 90 | L4 `kntcgn` tech + 4x `prlgun` items |
| 23 | `W90023` | Cryogenics lab | Selene deep ice | Liquid-helium temperature maintained by passive radiators | 75 | L5 `crymed` tech early + 20 `heliu3` deposit |
| 24 | `W90024` | Fabrication matrix | Fomal belt metallic | Molecular assembler templates, partially degraded | 100 | L5 `nminng` tech + 60 `nickfe` deposit |
| 25 | `W90025` | Tow cable spool | Helios belt large | Kilometre-scale monofilament cable on a drum | 65 | L5 `asttow` tech hint (-50% cost) |
| 26 | `W90026` | EW jammer pod | Pyre terminator | Cloaked pod emitting broadband noise across EM spectrum | 80 | L4 `ewsens` tech + 4x `psnew` items |

---

## Late tier (turns 20–35, empty system bodies, 120–200 pts)

| # | Id | Name | Location | Flavour | RP | Reward |
|---|-----|------|----------|---------|-----|--------|
| 27 | `W90027` | He3 processing plant | Ember — Calx | Industrial-scale helium-3 extractor, cold-fused to regolith | 150 | L6 `he3ext` tech + 2x `he3plt` + 100 `heliu3` deposit |
| 28 | `W90028` | Drone hangar | Gleam — ice-dust planet | Cavernous bay carved into permafrost, launch rails intact | 140 | L6 `drnswm` tech + 8x drone items + `drnbay` module |
| 29 | `W90029` | Foundry complex | Cinder — Forge | Smelting chambers using volcanic heat, alien metallurgy | 160 | L6 `orbfnd` tech + 3x `factry` + 80 `tungst` deposit |
| 30 | `W90030` | Fusion containment core | Ash — Nieve | Toroidal magnetic bottle, superconducting coils intact | 180 | L7 `fusdrv` tech + `fuseng` module + 50 `xenon` deposit |
| 31 | `W90031` | Space fauna specimen | Shards — outer belt | Preserved organism in vacuum-sealed crystal, bioluminescent | 120 | Unlock `xenoA` skill + L6 `xbiolb` tech |
| 32 | `W90032` | Spinal weapon mount | Deep — Basalt | Massive linear accelerator carved through the moon's core | 200 | L7 `spngun` tech + `spnwpn` module |
| 33 | `W90033` | Habitat wheel segment | Graph — dust planet | Curved hull section, centrifugal gravity design | 170 | L7 `whlhbt` tech + 2x `whlhab` modules |
| 34 | `W90034` | Life support core | Spare — dust planet | Closed-loop ecosystem module, beryllium-shielded | 150 | L6 `eclss2` tech + 40 `berylm` deposit |
| 35 | `W90035` | Xenobiology archive | Deep — Haven | Library of alien species data, partially translatable | 130 | Unlock `xenoA` + `xenoE` chain + L7 `xnofrn` tech |
| 36 | `W90036` | Propulsion test frame | Ember — dust planet | Linear test track across 3 regions, magnetic rail scars | 160 | L6 `mpddrv` tech + `mpdeng` module |
| 37 | `W90037` | Armour forge | Cinder — Tuyere | Pressure-sintering chambers for ultra-dense composites | 140 | L6 `armhul` tech + `hvyarm` module + 60 `tungst` |
| 38 | `W90038` | Point-defense turret | Shards — dust planet | Rapid-fire interceptor mount, targeting optics shattered | 130 | L5 `proxpd` tech + 2x `pdturr` modules |
| 39 | `W90039` | Missile silo | Deep — Fathom | Vertical launch tubes cut into methane ice | 150 | L6 `misgde` tech + 8x `prxgrd` items |
| 40 | `W90040` | Shield generator | Ash — Crucis | Capital-scale shield bubble projector, overloaded | 170 | L7 `capshd` upgrade + `lgshld` module |
| 41 | `W90041` | Mining automaton | Gleam — belt | Self-replicating extraction drone, dormant | 120 | L5 `asttow` tech + `towmod` module + unlock `minrA` skill |
| 42 | `W90042` | Sensor nexus | Spare — Sliver | Distributed phased array covering the moon hemisphere | 140 | L7 `bolsen` tech + `sensrr` module |

---

## Deep tier (turns 35+, AP coronae / deep space, 250–400 pts)

| # | Id | Name | Location | Flavour | RP | Reward |
|---|-----|------|----------|---------|-----|--------|
| 43 | `W90043` | Alderson stabiliser | Cinder-Shards UAP corona | Device maintaining the unstable point — alien engineering | 300 | L8 `plsdv` tech + UAP becomes stable |
| 44 | `W90044` | Ark skeleton | Deep — outer giant (18 AU) | Kilometre-long hull frame, stripped but structurally sound | 400 | L9 `arkfnd` tech + `arkhul` module frame |
| 45 | `W90045` | Dark-matter collector | Helios-Cinder AP corona | Gossamer filaments trailing into the point throat | 350 | L9 `arkeng` tech hint (-75% cost) |
| 46 | `W90046` | Stasis vault | Graph — ocean deep | Submerged alien archive, living specimens in suspension | 250 | Unlock `xenoE` + L8 `xnofrn` tech + unique "alien crew" item |
| 47 | `W90047` | Antimatter trap | Ash — outer giant (18 AU) | Penning trap array accumulating positrons from solar wind | 350 | L9 `plsdv` tech + `plseng` module |
| 48 | `W90048` | Navigation AI core | Shards-Deep AP corona | Alien computer calculating Alderson point geometries | 300 | All system connections revealed + L8 `navint` tech |
| 49 | `W90049` | Gravity lens | Ember-Cinder AP corona | Focused gravitational wavefront emitter | 280 | L8 `magsns` tech + `grvlns` module |
| 50 | `W90050` | Colony seedship wreck | Graph — belt large carbonaceous | Ancient generation ship, hull breached, cargo scattered | 400 | L10 `arkcns` hint (-50%) + 200 `alumin` + 100 `nickfe` |
| 51 | `W90051` | Temporal anomaly | Fomal-Shards AP corona | Localised time dilation — instruments age decades in hours | 250 | Region permanent 2x research output |
| 52 | `W90052` | Dimensional rift | Cinder — dust planet deep | Space-time tear leaking exotic particles | 300 | L10 `arkdrv` hint (-50%) + 30 `platnm` deposit |

---

## Stellar close-orbit tier (late game, 0.1 AU, 180–350 pts)

Requires `capshd` or better shield to survive `radiation-damage="20"` at 0.1 AU.

| # | Id | Name | Location | Flavour | RP | Reward |
|---|-----|------|----------|---------|-----|--------|
| 53 | `W90053` | Solar tap prototype | Helios 0.1 AU | Alien energy collector drinking from the photosphere | 200 | L7 `soltap` tech + `solgen` module |
| 54 | `W90054` | Coronal research platform | Fomal 0.1 AU | Hardened lab studying stellar plasma physics at source | 250 | L8 `corlab` tech + 2x `reslb3` modules |
| 55 | `W90055` | Beam relay transmitter | Helios 0.1 AU | Paired laser emitter/receiver for power beaming across AU | 220 | L7 `lsrxmt` tech + `lsrtx` + `lsrrx` modules |
| 56 | `W90056` | Photon sail loom | Fomal 0.1 AU | Fabrication frame weaving kilometre-scale reflective sails | 280 | L8 `plsail` tech + `phtsil` module |
| 57 | `W90057` | Drone foundry | Cinder 0.1 AU | Automated factory producing worker drones from raw silicates | 300 | L5 `drncrw` tech + `drnfac` module + 20x `wrkdrn` |
| 58 | `W90058` | Coronal anomaly — plasma entity | Ember 0.1 AU | Self-organising plasma filament exhibiting directed behaviour | 350 | Unlock `xenoE` + "plasma specimen" item + L9 `plsbio` tech |

---

## Drone-related anomalies (scattered, 160–220 pts)

| # | Id | Name | Location | Flavour | RP | Reward |
|---|-----|------|----------|---------|-----|--------|
| 59 | `W90059` | Dormant drone swarm | Ash — outer giant orbit | Millions of inert micro-drones in stable Lagrange cloud | 180 | L5 `drncrw` tech + 50x `wrkdrn` items |
| 60 | `W90060` | AI personality matrix | Gleam — inner barren | Crystalline data store radiating structured EM pulses | 220 | L7 `advdrn` tech hint (-50%) + unlock `xenoA` |
| 61 | `W90061` | Self-repair lattice | Spare — belt large metallic | Mesh of nanoscale drones maintaining asteroid structural integrity | 160 | `drnfac` module + 30 `silici` deposit |

---

## Placement rules

1. **Not on Arbor/Anvil surface at t=1** — starting planets are clean.
2. **Early tier placed at game start** (`gamein.xml`) on starting-system moons.
3. **Mid tier placed at game start** on harder-to-reach locations (belts, gas giant orbits).
4. **Late tier NOT at t=1** — injected by GM at turn 10–15 when players reach those systems, or seeded hidden (faction 1 stack, no contract until discovered via `SURVEY`).
5. **Deep tier injected by GM at turn 25+** or on first player jump to the system.
6. **Stellar close-orbit** — seeded at game start but invisible without survey + inaccessible without shields.
7. **Drone anomalies** — seeded with late tier, on empty-system bodies.

## L0-L2 exploratory combat encounters (starting basin)

These are intentional early combat outcomes for exploration on initial planets/moons, tuned as challenge encounters rather than faction-killing events.

| Encounter type | Example seed | Typical placement | Visibility / trigger | Threat profile | Reward profile |
|---|---|---|---|---|---|
| Space fauna guard | Helios moon trench feeder colony | Moon polar trenches, belt-edge cold shadows | Hidden until survey or close approach | One medium fauna stack; high damage spikes, lower HP | L6 resource clue + small finite cache (`xenon`, `reeox`, or `berylm`) |
| Rogue AI sentry | Abandoned core drill supervisor | Collapsed industrial node, old factory apron | Visible as inert ruin; activates on approach or extraction attempt | One fortified core + one light escort | Extraction rights boost + 1-2 usable modules (`cdrill`, `factry`, `survsc`) |
| Destroyable asteroid hazard | Fragment stream on orbital crossing | Approach lanes near belts, non-HQ regions | Announced as incoming hazard; timer window for interception | Hazard object with moderate HP, low attack, high impact damage if ignored | Salvage metals + safety contract payout |

### Early combat tuning (non-lethal)

- Encounter strength should map to **turns 1-10** player baselines:
  - single local defense stack can win with attrition,
  - unprepared scout-only response should fail or withdraw,
  - ignored hazard should hurt infrastructure, not erase a faction.
- Use rough envelope:
  - fauna/AI `attackRatio` and `damageRatio` vs local defender: `0.60-0.85`,
  - fauna/AI `ehpRatio`: `0.60-0.80`,
  - asteroid hazard impact equivalent: one serious regional setback, not HQ wipe.

### Encounter outcomes and contract hooks

1. **Repel / clear**: defeat the guard stack, receive resource lead unlock or salvage cache.
2. **Secure node**: hold location for N weeks after combat to claim industrial reward.
3. **Intercept hazard**: destroy/redirect asteroid before impact for defense payout.
4. **Fail-forward**: if players lose the first fight, keep a follow-up contract path with lower reward and reduced risk.

## Drone crew technology branch

| Tech | Level | Name | Prereq | Produces | Notes |
|------|-------|------|--------|----------|-------|
| `drncrw` | 5 | drone crew | `drnhng` (L4) + `ewsens` (L4) | `wrkdrn` item | 1 crew-equivalent; requires power not food; immune to radiation; binary alive/destroyed |
| `drnfac` | 5 | drone factory | `drncrw` | `drnfac` module | Produces `wrkdrn`; consumes 2 `silici` + 1 `copper` + 5 energy per drone |
| `advdrn` | 7 | advanced drone AI | `drncrw` + `navint` (L6) | `advdrn` item | 2 crew-equivalent; +10% initiative vs basic |
| `drnark` | 8 | ark drone complement | `advdrn` + `arkcns` (L10) | enables full-drone ark | Ark crewed entirely by drones (no food/terair/medical) |

## Stellar close-orbit technologies

| Tech | Level | Name | Prereq | Produces | Notes |
|------|-------|------|--------|----------|-------|
| `soltap` | 7 | solar energy tap | `he3fus` (L3) + `capshd` (L6) | `solgen` module | Massive energy output at 0.1 AU; useless beyond 0.5 AU |
| `corlab` | 8 | coronal research lab | `soltap` + `reslb2` | `corlab` module | 3x research output at close orbit |
| `lsrxmt` | 7 | laser power transmission | `soltap` | `lsrtx` + `lsrrx` modules (pair) | Beam energy from transmitter to receiver across AU |
| `plsbio` | 9 | plasma biology | `xnofrn` (L8) + `corlab` | — | Enables study of coronal life forms; prerequisite for late xenobiology |

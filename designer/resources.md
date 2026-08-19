# Resource dictionary (seed + catalog)

Canonical list for campaign galaxy seeding and `campaign/data.xml` item entries. **Do not fork:** `designer/catalog.md` points here. Consume/fuel ids come from `designer/technology.md`. Galaxy occupancy (Arbor/Anvil split) is `designer/galaxy.md`; this file owns **which ids exist, rarity, environments, and quantities**.

Do not invent a resource that no tech, module fuel, farm, or upkeep uses. Do not leave a tech consume-id without a row and a seed (or a refine path).

## Quantity bands (`<resource quantity="">`)

| Band | Quantity | Use |
|------|----------|-----|
| trace | 3–8 | PGM, tungsten, gold on a few cells |
| uncommon | 10–25 | Secondary ores, modest hydrosphere |
| common | 10–40 | Iron/silica where the split allows |
| rich | 40–80 | Signature ore on a world |
| pocket | 50–150 | Uranium, helium-3, deuterium ices — few cells only |
| signature | 40–80 | L3+ empty-system fame ores (`lithia` `boron` `reeox` …) on a few cells |

Never stack the same `type` twice on one region. `terair` only on habitable cells. `heliu3` and `deutrm` **not** on habitable basins.

### Renewable vs finite

All current region resource quantities are **renewable yield caps** (max extraction rate/turn, never depletes). **Finite deposits** (deplete to zero) will be added via GM events, contract rewards, or `PROSPECT` discoveries once the engine supports the `renewable` attribute.

**Classification:** L0–L3 resources (`iron` through `terair`) are renewable everywhere. L4+ resources (`nickfe` through `volatl`) are renewable on moons and gas giants but finite on planets and asteroids. See `designer/engine-wishlist.md` for the `renewable` attr spec.

## Closed set

**Seeded on regions (or orbits when the loader allows):**  
`iron` `titani` `silici` `copper` `uraniu` `carbon` `oil` `gold` `heliu3` `h2o2` `water` `food` `terair` `nickfe` `tungst` `deutrm` `ammoni` `methn` `volatl` `kerogn` `alumin` `platnm` `lithia` `boron` `berylm` `xenon` `reeox` `grphit` `nitrat`

**Refined, not seeded:** `hydzn` (from `hydsyn` + `ammoni`).

**Not seeded on regions** (produced, upkeep, or cargo only):  
`cash` `spare` `wastes` `medici` `deadtn` `rctlnc` `prllsr` `prlgun` `psnarm` `psnshd` `psnew` `prxgrd` `spcsut` `hydzn`

**Dropped:** silica-aerogel *item* (`aerogl`) — insulation is module `insltn` from `silici`. Hydrazine is refined, not a ground ore.

## Military affinities

When a **military** tech or its module is improved, bias `use-consume` and ammunition toward the signature family. A little `iron` for structure is fine; do not make the signature reagent a different family.

| Family | Closed-set ids | Feeds |
|--------|----------------|-------|
| **Gases** | `terair` `xenon` `volatl` `ammoni` `methn` `h2o2` `hydzn` (refined) | **Lasers** and **shields** (working gas, coolant, plasma) |
| **Minerals** | `iron` `titani` `tungst` `nickfe` `alumin` `boron` `berylm` `grphit` (ceramic path) | **Armour** and **kinetics** (density, hardness, penetrators) |
| **Isotopes** | `uraniu` `heliu3` `deutrm` `nitrat` (energetic salts); `gold` traces only | **Missiles** and **point-blank defence** (warheads, proximity bursts, PD ammo) |
| **Silicons** | `silici` `reeox` `copper` (Si is the signature; Cu with it for interconnects) | **EW** and **drones** (processors, seekers, datalinks) |

No new resource ids for this pass. `reeox` stays L6 (`reemin`); L2–L4 drone/EW consume `silici`+`copper` only. Seed locations unchanged.

See `technology.md` Combat matchups for which techs sit in each group.

---

## Seeded items

| id | name-en | size/mass | How it enters play | First level that *needs* it | Rarity | Seed environments | Arbor | Anvil | Empty systems (rich deposits) |
|----|---------|-----------|--------------------|------------------------------|--------|-------------------|-------|-------|-------------------------------|
| `iron` | unit of iron | 5/10 | `iminng` (3); region ore | L0 (`indust`, `fossil`, most builds) | common | `grassl` `dust` `mountn` `barren` `smmast` `lrmast`; `dust`/`ocean` planets | **present** common–rich | present, not signature | SS0009 Graph; SS0010 Spare; Pyre extra |
| `titani` | unit of titanium | 10/10 | `tminng` (2); region ore | L0 (`areact`, `indust`, hulls) | uncommon–rich | `mountn` `dust` `lrmast`; ilmenite highlands | **absent** (pocket: Scoria, Selene) | **rich** highlands | SS0003 Ember; SS0010 Spare |
| `silici` | unit of silicium | 5/3 | `slcmlt` (1); region ore; consume EW/drone `psnew` `drnhng` `ewsens` `alnfgh` `drnswm` `misgde` `ewark` `arkew` | L0 (`corpmg`, `filidx`, `spctrl`) | common | `dust` `barren` `mountn` `grassl` (low); belts | **low** sediments | **rich** | SS0003 Ember; SS0009 Graph |
| `copper` | unit of copper | 5/8 | `cminng` (2); region ore | L0 (`corpmg`, `urfiss`, `airgen`) | uncommon–common | volcanic `dust` `mountn`; metal asteroids | **absent** (pocket: Scoria) | **common** arcs | SS0004 Gleam; SS0010 Spare |
| `uraniu` | unit of uranium | 1/8 | `uminng` (1); `fisrec` fuel | L0 (`urfiss` fuel; mining) | rare / pocket | few `mountn`; `lrmast` | **absent** (pocket: Helios belt) | **pockets** 2–3 peaks 50–150 | SS0004 Gleam; SS0005 Cinder |
| `carbon` | unit of carbon | 5/5 | `hcdril` (1); `cplant` fuel | L0 (`fossil` / plant fuel) | common on organics | `grassl` wetlands; `smcast` `lrcast` | **rich** peat/coal | **absent** (trace graphite only; pocket: Fomal belt) | SS0005 Cinder; SS0007 Shards; SS0009 Graph |
| `oil` | unit of oil | 4/5 | `oildwe` (2); `oplant` fuel | L0 (`oilbrn` / plant fuel) | uncommon | coastal `grassl`; `lrcast` | **present** Tidewatch coast | **absent** (pocket: Fomal belt) | SS0007 Shards carbonaceous |
| `gold` | unit of gold | 5/9 | `gminng` (1); contacts | L1 (`optins` consumes 1) | trace | hydrothermal `mountn`; rare `lrmast` | **absent** | **trace** mountains | SS0009 Graph |
| `heliu3` | unit of helium-3 | 1/1 | `he3min` (1); `he3ext`; `fusrec`/`he3aut`/`dhefrc`/`fuseng`/`plsdv`/`arkeng` fuel; build `he3fus` `fusdrv` `hiisp` `arkcns` `arkdrv`; pbpd `ciwssy` `arkpd` | L2 (`he3fus`) | rare / pocket | ice-moon `dust`/`barren`; outer `abelt`; **not** habitable basins | absent on Arbor (pocket: Aeolus ices) | absent on Anvil (pocket: Fomal ices) | SS0003 Ember ices; SS0006 Ash; SS0008 Deep ices |
| `h2o2` | unit of oxyhydro | 1/1 | `wtrdst` (3); `rctdrv`/`autdrv`/`tanker` fuel | L0 (`areact` fuel; distillation) | common on wet worlds | `ocean` `sea` ice `dust`; polar | **rich** | **modest** | SS0007 Shards ice; SS0008 Deep; SS0010 Spare ice |
| `water` | unit of water | 1/1 | region hydrosphere; `clslss` produce; `hydnoz`/`iondrv`/`ntreng`/`nepeng`/`vasmdr` fuel | L1 (`hydstg` fuel) | common / modest | `ocean` `sea` ice; not vacuum dust | **rich** | **modest** | SS0008 Deep hab moon; ice moons generally |
| `food` | unit of food | 1/1 | `farmng` (5), `afrmng` (8), `agrdek`; region biomass; city upkeep | L0 (`farmng`; city upkeep) | biosphere | `grassl` `sea` only (habitable) | **rich** 400–800 grassland | **poor** 80–150 | SS0008 Deep (tight); SS0009 Graph |
| `terair` | terran breathing gas | 1/1 | region biosphere; `lifsys` produce 10/wk; `clslss`; consume lasers/shields `lasopt` `lstrrt` `prllsr` `psnshd` `pdefls` `shplas` `uvltur` `bmdir` `arkdef` | L0 (`popcnt`/`agrplx`/`farmng` need atmosphere) | habitable only | habitable `grassl` `ocean` `sea` `mountn` (thin) | **present** habitable cells | **present** habitable cells | SS0008 Deep ice moon; SS0009 Graph. Never Scoria/Pyre/belts |
| `nickfe` | nickel-iron | 6/12 | `nminng` (2); consume `orbfnd` `msdrvr` `lghull` `arkcns`; kinetics `kntcgn` `gausgn` `spngun` `armhul` | L1 (`nminng`); L4 foundry | uncommon | `lrmast` `smmast`; metal moons | absent on Arbor (pocket: Helios belt) | absent (Anvil has iron/titani instead) | SS0004 Gleam |
| `tungst` | tungsten | 8/20 | `wminng` (1); consume `matlib` `armhul` `kpdgun` `gausgn` `spngun` `hiisp` `arkcns` | L5 (`wminng` / L6 `matlib`) | trace | `vulcan` moon `mountn`/`dust` | absent (pocket: Aeolus vulcan) | absent | SS0005 Cinder vulcan moons |
| `deutrm` | deuterium | 1/1 | `d2ext` (1); `dhefrc`/`plsdv`/`arkeng` fuel; consume `dhefus`; pbpd `proxpd` | L4 (`d2ext`); L6 fusion | rare / pocket | ice `dust`/`barren`; **not** habitable basins. Orbit of `gasgnt` is flavour until `atmosphere` location-type | absent on Arbor (pocket: Aeolus ices) | absent on Anvil (pocket: Fomal ices) | SS0003 Ember ice-giant moons; SS0006 Ash |
| `ammoni` | ammonia ice | 2/2 | `amnext` (2); consume `clslfe`; makeup `eclssx` | L5 (`clslfe`) | uncommon | outer ice moons `dust`/`barren` | absent (pocket: Aeolus ices) | absent (pocket: Fomal ices) | SS0006 Ash |
| `methn` | methane ice | 2/1 | `ch4min` (2); `mpddrv`/`vasmdr` fuel | L3 (`ch4min`); L5 MPD | uncommon | outer ice moons; Titan-class | absent (pocket: Aeolus ices) | absent (pocket: Fomal ices) | SS0008 Deep |
| `volatl` | mixed volatiles | 2/2 | `volext` (2); `isrplt` USE feedstock | L4 (`volext` / `isrurf`) | uncommon | `smcast` `lrcast` | absent (pocket: Helios belt `lrcast`) | absent (pocket: Fomal belt) | SS0006 Ash; SS0007 Shards |
| `kerogn` | kerogen organics | 2/2 | `krogen` (1); `isrplt` USE | L2 (`krogen`) | uncommon | `smcast` `lrcast` | absent (pocket: Helios belt carbonaceous) | absent (pocket: Fomal belt) | SS0007 Shards |
| `alumin` | aluminium | 5/8 | `alminn` (2); consume `whlhbt` `agrark` | L2 (`alminn`); L6 habitat | uncommon | anorthosite `barren`/`dust` highlands; rock moons | absent (pocket: Selene) | **uncommon** highlands | SS0008 Deep rock moons; SS0009 Graph |
| `platnm` | platinum-group | 1/4 | `ptminn` (1); consume `dhefus` `eclss2` `recyl2` `arkrec` | L6 (`dhefus`) | trace | with `nickfe` on `lrmast`; few cells | absent (trace Helios belt) | **trace** with uraninite (1–2 cells 3–8) | SS0004 Gleam |
| `lithia` | lithium ore | 2/2 | `liming` (1); fuel `limpd`; consume `mpdlth` | L4 (`liming`); L5 Li-MPD | uncommon | evaporite `dust`/`barren`; ice-moon brines | **absent** (no basin) | **absent** | **SS0003 Ember** (signature) |
| `boron` | boron | 3/4 | `bormin` (1); consume `ceramp` `armcml` `radshc` `shldsp` `arkshl` | L5 (`bormin` / `ceramp`) | uncommon | `vulcan` `mountn` fumaroles | **absent** (pocket: Aeolus vulcan trace 3–8) | **absent** | **SS0005 Cinder** vulcan moons |
| `berylm` | beryllium | 4/6 | `beming` (1); consume `bwinow` `ntdiag` `radshc` `crydet` `dosmtr` `arkshl` | L5 (`beming`) | rare | `dust` `mountn` on barren worlds | **absent** | **absent** | **SS0010 Spare** |
| `xenon` | xenon | 1/2 | `xeming` (1); fuel `hlthst` `xendrv` `trimth` `orbtug`; consume lasers/shields `pdefls` `shplas` `uvltur` `bmdir` `capshd` `arkdef` `arkshd` | L4 (`xeming` / Hall) | rare / pocket | outer ice `dust`/`barren`; **not** habitable basins | absent (pocket: Aeolus ices trace) | absent (pocket: Fomal ices trace) | **SS0006 Ash** |
| `reeox` | rare-earth oxides | 3/8 | `reemin` (1); consume `magsail` `magsns` `plsail` `bolsen` `navint` `arksail` `arknav`; military EW/drone from L6 `drnswm` `ewark` `arkew` | L6 (`reemin`) | rare | `lrmast` with `nickfe` | **absent** | **absent** | **SS0004 Gleam** (with nickfe) |
| `grphit` | nuclear graphite | 5/5 | `grmine` (1); consume `cccomp` `radshc` `shldsp` `brakch` `arkshl` | L6 (`grmine` / `cccomp`) | uncommon | high-T `carbon` worlds; `lrcast` baked | **absent** (not peat) | **absent** | **SS0005 Cinder** (with boron) |
| `nitrat` | nitrate salts | 2/2 | `ntmine` (2); consume missiles/pbpd `mslpod` `prxgrd` `proxpd` `crumis` `magzin` `minelr` `ciwssy` `arkpd` `arkmag` | L3 (`ntmine` / missiles) | uncommon | evaporite `dust` `barren` | **absent** | **absent** | **SS0007 Shards** |

Same-system pockets (Scoria, Helios belt, Selene, Pyre, Fomal belt, ice/vulcan moons) exist so a corp can finish a **L0–L2** bill without the Helios↔Fomal hop. **L3+ signature ores stay off Arbor/Anvil basins**; Helios vulcan/ice may hold **trace** boron/xenon only (3–8), not rich deposits.

---

## Not seeded (still catalog items)

| id | name-en | size/mass | How it enters play | First need | Seed |
|----|---------|-----------|--------------------|------------|------|
| `cash` | cash | — | cities, `corphq`; consume `ctypln` 500 | L1 | faction `balance`, city stacks — not `<resource>` |
| `spare` | spare part | 2/2 | `servic` (10); consume `repair` / `REPAIR` | L1 | cargo only |
| `wastes` | waste product | 5/5 | `fisrec` byproduct; consume `wastdp`; `clslss` feedstock | L0–1 | not a ground ore |
| `medici` | medicines | 1/1 | `medirf` (1) on **ocean** worlds (no cargo consume); `pharms` (1) from 1 `food` on a `sckbay`; weekly `wndtrn`/`madtrn` race consume; 1 per sick-bay conversion; consume `exobio` `crewmd` | L2 harvest / L3 synth | not a region type; ocean refine on Arbor/Anvil/Graph, or ferment aboard |
| `deadtn` | dead terran | 4/4 | combat/medical leftover; edible | — | never a region resource |
| `rctlnc` | rocket launchers | 100/100 | `rckter` | L1 | infantry/vehicle/military cargo; **missile** |
| `prllsr` | personal laser | 2/3 | `prllsr` | L1 | **laser** item |
| `prlgun` | personal rail gun | 3/4 | `prlgun` | L1 | **kinetic** item |
| `psnarm` | personal armour | 4/6 | `psnarm` | L1 | **armour** item |
| `psnshd` | personal plasma shield | 3/4 | `psnshd` | L2 | **shield** item |
| `psnew` | personal EW pack | 2/2 | `psnew` | L2 | **ew** item |
| `prxgrd` | proximity grenades | 3/3 | `prxgrd` | L3 | **pbpd** item |
| `spcsut` | space suit | 2/10 | catalog equipment | L0 flavour | cargo, not ground |
| `hydzn` | hydrazine | 2/2 | `hydsyn` (2 from `ammoni`); fuel `hypeng` `chmup2` `rcspod` `arkrcs`; consume `crumis` | L3 (`hydsyn`) | never a region resource; refine from ammonia ices |

`*` (anything) is a wildcard item, not seeded.

---

## Extraction techs (mirror `iminng`)

All `use-allowed-in` extraction + solid-surface unless noted. Campaign tag **production**.

| Tech | Lv | Requires | Produces | Where to USE |
|------|----|----------|----------|--------------|
| `iminng` | 0 | — | 3 `iron` | solid ore cells |
| `tminng` | 0 | — | 2 `titani` | |
| `slcmlt` | 0 | — | 1 `silici` | |
| `cminng` | 0 | — | 2 `copper` | |
| `uminng` | 0 | — | 1 `uraniu` | |
| `hcdril` | 0 | — | 1 `carbon` | solid-surface |
| `oildwe` | 0 | — | 2 `oil` | |
| `wtrdst` | 0 | — | 3 `h2o2` | water/ice cells |
| `nminng` | 1 | `iminng` | 2 `nickfe` | metal asteroids |
| `gminng` | 1 | `cminng` | 1 `gold` | vein `mountn` |
| `he3min` | 2 | `uminng` | 1 `heliu3` | ice/regolith, not grassland |
| `alminn` | 2 | `slcmlt` | 2 `alumin` | highlands |
| `krogen` | 2 | `hcdril` | 1 `kerogn` | carbonaceous |
| `amnext` | 3 | `wtrdst` | 2 `ammoni` | outer ices |
| `ch4min` | 3 | `oildwe` | 2 `methn` | outer ices |
| `ntmine` | 3 | `oildwe` | 2 `nitrat` | evaporite dust; SS0007 |
| `liming` | 4 | `wtrdst` | 1 `lithia` | brines; SS0003 |
| `xeming` | 4 | `volext` | 1 `xenon` | outer ices; SS0006 |
| `d2ext` | 4 | `wtrdst` | 1 `deutrm` | ice, not habitable basins |
| `volext` | 4 | `krogen` | 2 `volatl` | carbonaceous |
| `wminng` | 5 | `tminng` | 1 `tungst` | vulcan |
| `bormin` | 5 | `tminng` | 1 `boron` | vulcan; SS0005 |
| `beming` | 5 | `tminng` | 1 `berylm` | barren dust; SS0010 |
| `ptminn` | 6 | `wminng` | 1 `platnm` | metal asteroids / Anvil pockets |
| `reemin` | 6 | `nminng` | 1 `reeox` | metal asteroids; SS0004 |
| `grmine` | 6 | `hcdril` | 1 `grphit` | high-T carbon; SS0005 |
| `he3drl` | 3 | `he3min` | module `he3ext` (20 `heliu3`/13 wk) | build, not a USE-mine |

Farms: `farmng` / `afrmng` produce `food` (not a mine). `medirf` produces `medici` on ocean planets. `pharms` produces `medici` from 1 `food` on a sick bay. `hydsyn` produces `hydzn` from `ammoni` (not extraction).

## Module fuels (must exist in this dictionary)

| Module | Fuel items |
|--------|------------|
| `cplant` | `carbon` |
| `oplant` | `oil` |
| `fisrec` `nepeng` | `uraniu` |
| `rctdrv` `autdrv` `tanker` | `h2o2` |
| `hydnoz` `iondrv` `ntreng` `arcjet` | `water` (`hydnoz` also `h2o2`) |
| `mpddrv` `vasmdr` | `methn` (`vasmdr` also `water`) |
| `limpd` | `lithia` |
| `hlthst` `xendrv` | `xenon` |
| `trimth` `orbtug` | `xenon` or `water` |
| `hypeng` `chmup2` `rcspod` `arkrcs` `crumis` | `hydzn` |
| `h2cell` | `h2o2` |
| `fusrec` `he3aut` `fuseng` | `heliu3` |
| `dhefrc` `plsdv` `arkeng` | `heliu3` + `deutrm` |
| `slsmod` `mgsail` `plsail` `arksail` `hshld` | none (sails / heatshield) |

## Engine

Gas-giant **orbit** `deutrm`/`heliu3` as a cloud resource needs location-type `atmosphere` — already on `designer/engine-wishlist.md`. Until then, put those isotopes on **ice-moon surfaces** (`dust`/`barren` + ice flavour). No new location-type for this dictionary.

Moon-region exits still block Selene/Aeolus pockets; Helios **Scoria** and **abelt** remain the loadable titani/uraniu/nickfe path.

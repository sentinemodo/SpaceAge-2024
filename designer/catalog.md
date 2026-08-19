# Catalog proposals (resources, modules, items, skills, equipment)

Ids ≤ 6 characters. No item id equal to a module id. Descriptions: mechanism and environment, not mysticism.

Live catalog (`Tests/data.xml`) is the baseline. Below are **additions** for `campaign/data.xml`.

## Resources (item types)

Canonical dictionary (ids, size/mass, extraction techs, rarity, Arbor/Anvil, empty-system seeds): **[resources.md](resources.md)**. Do not keep a second table here.

Closed seed set: `iron` `titani` `silici` `copper` `uraniu` `carbon` `oil` `gold` `heliu3` `h2o2` `water` `food` `terair` `nickfe` `tungst` `deutrm` `ammoni` `methn` `volatl` `kerogn` `alumin` `platnm` `lithia` `boron` `berylm` `xenon` `reeox` `grphit` `nitrat`. Refined: `hydzn`.

## Module types (new)

Use live **groups**. Intended future group in parentheses → wishlist.

| Id | Group | Size/mass (order of) | Crew / energy | What it is |
|----|-------|----------------------|---------------|------------|
| `sckbay` | habitat | 380/250 | 3 / 4 | Sick bay (inpatient ward). Habitat 8, HP 32, tech-cap 2, cash upkeep 40, radiation −120. **Not** `medfac` (clinic: 300/200, crew 2, energy 2, habitat 5, HP 25). Stronger than the clinic: 2 `wndtrn`/4 wk without `medici`, 4/wk with 1 `medici` per conversion (wishlist). No `operation-allowed-in` — settlements, stations, hulls. `pharms` USE produces `medici` from 1 `food` |
| `hydnoz` | propulsion | 800/900 | 1 / 40 | Staged hydrolox; mass-capacity ~20000; fuel `water` or `h2o2` |
| `iondrv` | propulsion | 500/400 | 1 / 50 | Gridded ion; mass-capacity ~25000; fuel `water` |
| `nepeng` | propulsion | 1400/1600 | 2 / 70 | Nuclear-electric; mass-capacity ~35000; fuel `uraniu`+`water` |
| `optlab` | research | 250/60 | 2 / 6 | output 1, tech-cap 5 |
| `ntreng` | propulsion | 1200/1400 | 2 / 80 | Nuclear thermal; `move` space mass-capacity ~40000; fuel `water` |
| `orbfry` | production | 4000/800 | 8 / 40 | Vacuum foundry; `use` orbit |
| `radlab` | research | 300/80 | 4 / 8 | output 2, tech-cap 6 |
| `railgn` | military | 800/900 | 4 / 20 | **kinetic**; attack 8 defense 2 damage 8 |
| `clslss` | habitat | 400/200 | 2 / 12 | produce `terair`+`water` from `wastes`+energy |
| `isrplt` | extraction | 1500/1500 | 8 / 20 | consumes regional `volatl`/`dust` flavour via USE techs |
| `mpddrv` | propulsion | 900/800 | 2 / 60 | mass-capacity ~80000; fuel `methn` |
| `pdltur` | military | 200/150 | 1 / 15 | **laser** PD; attack 4, high initiative |
| `survsc` | research | 150/40 | 2 / 4 | output 1, tech-cap 4 |
| `dhefrc` | energy | 2000/1800 | 6 / 20 | produce energy ~250 / 13 wk; consume `heliu3`+`deutrm` |
| `whlhul` | habitat | 20000/8000 | 20 / 40 | spin habitat; population-maximum 2000 |
| `fuseng` | propulsion | 2500/2200 | 4 / 120 | mass-capacity ~2e5 |
| `drnctl` | command | 300/120 | 2 / 10 | tech-cap 4 |
| `matlab` | research | 400/100 | 6 / 12 | output 3, tech-cap 8 |
| `msdrst` | production | 8000/10000 | 12 / 80 | surface; operation solid-surface |
| `crytnk` | storage | 2000/400 | 1 / 8 | capacity 5000 |
| `vasmdr` | propulsion | 1800/1600 | 3 / 100 | fuel `water` or `methn` |
| `armplt` | frigate | 2000/3000 | 0 / 0 | **armour**; defense 15, hit-points 200 |
| `xbiolb` | research | 250/80 | 4 / 6 | |
| `lghul` | frigate | 25000/8000 | 0 / 20 | large hull; capacity 20000 |
| `tanker` | spacecraft | 6000/2000 | 6 / 30 | move space; cargo |
| `spnknc` | military | 4000/5000 | 12 / 40 | **kinetic**; attack 14 damage 16 |
| `dpsens` | research | 600/200 | 4 / 15 | |
| `eclssx` | habitat | 800/400 | 4 / 20 | near-closed loop |
| `agrdek` | agricultural | 2000/600 | 15 / 25 | produce food in space (`operation` orbit/space) |
| `plsdv` | propulsion | 4000/3500 | 6 / 180 | ark-class; mass-capacity ~8e5 |
| `ciwst` | military | 400/300 | 2 / 20 | **pbpd** only; attack 6 damage 8 |
| `arkmed` | research | 500/150 | 8 / 10 | |
| `arkhul` | frigate | 80000/25000 | 0 / 80 | L10 hull |
| `arkeng` | propulsion | 8000/7000 | 8 / 200 | mass-capacity ~2e6 |
| `arkbrg` | command | 2000/800 | 12 / 30 | tech-cap 16, research-output 4 |
| `arkdfn` | military | 3000/2500 | 10 / 50 | **laser** grid |
| `solthp` | energy | 800/600 | 2 / 4 | solar thermal; ~30 energy / 13 wk; inner-system |
| `slsmod` | propulsion | 400/80 | 1 / 2 | sail; mass-cap ~15000; no fuel |
| `msltub` | military | 300/400 | 2 / 8 | **missile**; attack 6 damage 8 |
| `ewantn` | military | 150/80 | 2 / 10 | **ew**; defense 4 initiative 4 |
| `hypeng` | propulsion | 700/800 | 1 / 25 | mass-cap ~18000; fuel `hydzn` |
| `hlthst` | propulsion | 600/500 | 1 / 55 | mass-cap ~30000; fuel `xenon` |
| `h2cell` | energy | 400/300 | 2 / 8 | ~40 energy / 13 wk; fuel `h2o2` |
| `crylab` | research | 280/90 | 3 / 8 | output 2, tech-cap 5 |
| `cmplab` | research | 300/100 | 4 / 8 | output 2, tech-cap 6 |
| `shplas` | military | 400/350 | 3 / 25 | **shield**: defense 12, attack 0 |
| `o2plt` | extraction | 1200/1200 | 6 / 18 | volatiles → `h2o2`/`terair` |
| `cerkil` | production | 1500/1200 | 6 / 30 | B4C/SiC kiln |
| `limpd` | propulsion | 950/850 | 2 / 70 | Li-MPD; mass-cap ~90000; fuel `lithia` |
| `arcjet` | propulsion | 500/450 | 1 / 45 | mass-cap ~22000; fuel `water` |
| `seissc` | research | 180/80 | 2 / 6 | output 1, tech-cap 5 |
| `bwinow` | research | 120/40 | 2 / 4 | x-ray windows |
| `kpdtur` | military | 250/280 | 2 / 12 | **kinetic** cannon; attack 5 initiative 6 |
| `cermpl` | frigate | 1200/1800 | 0 / 0 | **armour**; defense 12, HP 150 |
| `ccplnk` | production | 2000/1500 | 8 / 40 | C-C layup |
| `mgsail` | propulsion | 800/400 | 2 / 20 | mag-sail; mass-cap ~1e5; no fuel |
| `xendrv` | propulsion | 700/550 | 2 / 80 | xenon ion; mass-cap ~50000 |
| `coilgn` | military | 1200/1400 | 6 / 35 | **kinetic**; attack 10 damage 10 |
| `miscpu` | command | 200/80 | 2 / 8 | tech-cap 3 |
| `maglab` | research | 350/120 | 4 / 10 | output 2, tech-cap 6 |
| `ntdiag` | research | 300/100 | 4 / 10 | output 2 |
| `recykl` | production | 1000/800 | 6 / 20 | scrap → iron/spare |
| `bioplt` | production | 800/400 | 8 / 15 | cultured polymer |
| `chmup2` | propulsion | 900/1000 | 2 / 30 | insertion stage; fuel `hydzn` |
| `plsail` | propulsion | 1200/600 | 3 / 25 | plasma sail; no fuel |
| `crumis` | military | 500/600 | 0 / 10 | **missile**; attack 12 damage 14; fuel `hydzn`; spacecraft-capable |
| `uvltur` | military | 350/400 | 2 / 20 | **laser**; attack 8 damage 7 |
| `navcmp` | command | 400/150 | 4 / 12 | tech-cap 6 |
| `fatlab` | research | 350/120 | 5 / 10 | output 2, tech-cap 7 |
| `radshd` | habitat | 3000/2500 | 4 / 15 | storm shelter |
| `recylr` | production | 2500/2000 | 8 / 40 | closed recycle |
| `insltn` | habitat | 400/80 | 0 / 2 | aerogel MLI analogue |
| `rcspod` | propulsion | 200/180 | 1 / 8 | fuel `hydzn` |
| `trimth` | propulsion | 300/250 | 1 / 20 | fuel `xenon` or `water` |
| `bolsen` | research | 250/80 | 3 / 8 | output 2 |
| `crydet` | research | 280/90 | 3 / 10 | output 2, tech-cap 6 |
| `magzin` | storage | 4000/3000 | 2 / 5 | **missile** grain well |
| `bmdir` | military | 600/400 | 4 / 25 | **laser** steering optics |
| `stmshd` | habitat | 2500/2000 | 2 / 10 | SPE shelter |
| `wstplt` | production | 1500/1200 | 6 / 25 | mineralise wastes |
| `orbtug` | spacecraft | 4000/2500 | 4 / 40 | electric tug; fuel `xenon`/`water` |
| `hshld` | spacecraft | 3000/2000 | 0 / 0 | aerobrake shield |
| `ewark` | military | 500/300 | 4 / 20 | **ew**; area jamming |
| `knmine` | military | 200/400 | 0 / 0 | **missile** mine; attack 8 damage 12 |
| `psybrd` | research | 400/150 | 6 / 8 | isolation psych |
| `insnav` | command | 500/200 | 4 / 15 | tech-cap 8 |
| `arkshl` | habitat | 8000/6000 | 6 / 25 | ark SPE/GCR shelter |
| `arkrec` | production | 6000/5000 | 10 / 50 | ark mass loop |
| `arkrcs` | propulsion | 2000/1800 | 4 / 40 | fuel `hydzn` |
| `arksail` | propulsion | 4000/2000 | 4 / 30 | abort mag-sail |
| `arknav` | command | 2500/1000 | 8 / 25 | tech-cap 12 |
| `doslab` | research | 400/150 | 4 / 8 | dosimetry |
| `arkmag` | storage | 12000/8000 | 4 / 10 | **missile** ark grain |
| `arkew` | military | 2000/1500 | 8 / 40 | **ew** |
| `proxpd` | military | 250/200 | 2 / 15 | **pbpd**; attack 4 damage 6 initiative 8 |
| `capshd` | military | 800/600 | 4 / 40 | **shield**; defense 16, attack 0 |
| `arkshd` | military | 2500/1800 | 6 / 60 | **shield**; defense 20. Not habitat `arkshl` |
| `arkpd` | military | 1800/1400 | 6 / 35 | **pbpd**; attack 8 damage 10 |
| `lrmmod` | military | 1200/1400 | 4 / 30 | **LRM**; attack 20 damage 30. Strategic. `LAUNCH` order |
| `lrmhvy` | military | 2000/2400 | 6 / 50 | **LRM**; attack 30 damage 50. Multi-warhead (3) |
| `lrmark` | military | 4000/5000 | 8 / 80 | **LRM**; attack 40 damage 80. Ark strategic missile. 5 warheads |
| `mnlayr` | military | 400/300 | 2 / 8 | **mine**; mine layer. Produces `spmine` items (1/cycle) |
| `smnlay` | military | 600/500 | 3 / 12 | **mine**; smart mine layer. Produces `smmine` items |
| `hvmlay` | military | 900/800 | 4 / 20 | **mine**; heavy mine layer. Produces `hvmine` items |
| `sensor` | military | 200/100 | 1 / 6 | **detection**; basic sensor array. 1 AU visibility |
| `advsns` | military | 400/200 | 2 / 10 | **detection**; advanced sensors. 5 AU visibility |
| `syssns` | military | 800/400 | 3 / 20 | **detection**; system-wide sensors. Full system visibility |
| `apsns` | military | 1200/600 | 4 / 30 | **detection**; AP-spanning sensors. Cross-system detection |
| `clkdev` | military | 200/150 | 1 / 10 | **cloaking**; basic cloaking device. Counters L2 detection |
| `advclk` | military | 400/300 | 2 / 20 | **cloaking**; advanced cloaking. Counters L4 detection |
| `clkfld` | military | 600/500 | 3 / 35 | **cloaking**; cloaking field. Counters L6 detection. Covers stack |
| `arkclk` | military | 1200/1000 | 4 / 60 | **cloaking**; ark cloaking. Counters L8 detection |

Tune hit-points ≈ `(mass+size)/20` if omitted. Cash upkeep roughly `crew*10 + energy + size/200`.

## Military groups (same four at every scale)

Matchup table: `technology.md` Combat matchups. Until Battle is typed, put `attack`/`damage` on the **weapon**, `defense` on the **resist** module, and signature consume from `resources.md` Military affinities.

| Id | Group | Role | Size/mass | Crew / energy | Combat stats |
|----|-------|------|-----------|---------------|--------------|
| live `gunplc` | kinetic | weapon | live | live | attack/damage |
| live `bltlas` `laztrt` `xraylz` | laser | weapon | live | live | attack/damage |
| live `alndrn` | drone | weapon **and** fighter hull | live | live | attack/damage; may nest other-group items |
| live `inftry` | mixed | infantry platform | live | live | mounts items below |
| live `tanks` | kinetic+armour | vehicle platform | live | live | oil engines; mounts items |
| `railgn` | kinetic | weapon | 800/900 | 4 / 20 | attack 8 defense 2 damage 8 |
| `kpdtur` | kinetic | weapon | 250/280 | 2 / 12 | attack 5 initiative 6 |
| `coilgn` | kinetic | weapon | 1200/1400 | 6 / 35 | attack 10 damage 10 |
| `spnknc` | kinetic | weapon | 4000/5000 | 12 / 40 | attack 14 damage 16 |
| `pdltur` | laser | weapon | 200/150 | 1 / 15 | attack 4, high initiative |
| `uvltur` | laser | weapon | 350/400 | 2 / 20 | attack 8 damage 7 |
| `bmdir` | laser | weapon | 600/400 | 4 / 25 | attack 6 damage 5 initiative 8 |
| `arkdfn` | laser | ark beam grid | 3000/2500 | 10 / 50 | attack 10 damage 8 |
| `shplas` | shield | resist laser | 400/350 | 3 / 25 | **defense 12, attack 0** (not a battery) |
| `capshd` | shield | resist laser | 800/600 | 4 / 40 | defense 16, attack 0 |
| `arkshd` | shield | resist laser | 2500/1800 | 6 / 60 | defense 20, attack 0. Not habitat `arkshl` |
| `drnbay` | drone | hangar | live / campaign | — | nests `alndrn` |
| `drnctl` | drone | command | 300/120 | 2 / 10 | tech-cap 4 |
| `ewantn` | ew | resist drone | 150/80 | 2 / 10 | defense 4 initiative 4 |
| `ewark` | ew | resist drone | 500/300 | 4 / 20 | defense 8 initiative 6 |
| `arkew` | ew | resist drone | 2000/1500 | 8 / 40 | defense 12 initiative 8 |
| `msltub` | missile | weapon | 300/400 | 2 / 8 | attack 6 damage 8 |
| `crumis` | missile | weapon | 500/600 | 0 / 10 | attack 12 damage 14; fuel `hydzn` |
| `magzin` | missile | ammo well | 4000/3000 | 2 / 5 | storage; no attack |
| `knmine` | missile | mine-as-missile | 200/400 | 0 / 0 | attack 8 damage 12 |
| `arkmag` | missile | ark grain | 12000/8000 | 4 / 10 | storage |
| `proxpd` | pbpd | resist missile | 250/200 | 2 / 15 | attack 4 damage 6 initiative 8 (last-ditch burst) |
| `ciwst` | pbpd | resist missile | 400/300 | 2 / 20 | attack 6 damage 8 initiative 10. **Not** laser or kinetic PD |
| `arkpd` | pbpd | resist missile | 1800/1400 | 6 / 35 | attack 8 damage 10 |
| `cermpl` | armour | resist kinetic | 1200/1800 | 0 / 0 | defense 12, HP 150 |
| `armplt` | armour | resist kinetic | 2000/3000 | 0 / 0 | defense 15, HP 200 |

Fighter/drone stacks mount the **same item ids** as infantry (`use-allowed-by` infantry + vehicle + military). `alndrn` is the drone-group weapon; nested `prllsr`/`prlgun`/`rctlnc` are the other three groups at that scale.

## Equipment (items, not modules)

Live: `rctlnc` (infantry rocket, **missile**), `spcsut` (EVA). Live `smlarm` is L0 **kinetic** flavour.

`use-allowed-by` for the four-group kit: infantry **and** vehicle **and** military so tanks and fighters share the same items.

| Id | Group | Name | size/mass | `use-allowed-by` | Effect (live attrs or wishlist) |
|----|-------|------|-----------|------------------|----------------------------------|
| `rctlnc` | missile | rocket launchers | 100/100 | infantry (live); campaign also vehicle+military | attack 2 damage 2 — **not in `getChance` today** |
| `prllsr` | laser | personal laser | 2/3 | infantry vehicle military | attack 2 damage 2 |
| `prlgun` | kinetic | personal rail gun | 3/4 | infantry vehicle military | attack 2 damage 3 |
| `psnarm` | armour | personal armour | 4/6 | infantry vehicle military | defense 3 |
| `psnshd` | shield | personal plasma shield | 3/4 | infantry vehicle military | defense 3 (laser resist) |
| `psnew` | ew | personal EW pack | 2/2 | infantry vehicle military | defense 2 initiative 2 (drone resist) |
| `prxgrd` | pbpd | proximity grenades | 3/3 | infantry vehicle military | attack 2 damage 3 initiative 2 (missile resist) |
| `radvst` | — | radiation vest | 2/3 | officer+crew terran | `radiation="-40"` if loader ignores, still set it |
| `senpak` | — | field spectrometer | 2/2 | officer | flavour; wishlist SEE bonus |
| `smlarm` | kinetic | personal firearm | 1/2 | infantry | attack 1 damage 1 |
| `evakit` | — | EVA tool kit | 3/4 | crew terran | wishlist: repair chance |
| `dosim` | — | dosimeter | 1/1 | officer | flavour |
| `vacshd` | — | deployable sunshade | 8/6 | spacecraft stacks as cargo | flavour until effect exists |
| `spmine` | — | space mine | 50/80 | deployed at location | damage 15. Invisible. Stationary. Detonates on enemy entry |
| `smmine` | — | smart mine | 80/120 | deployed at location | damage 25. IFF (skip friendly). Invisible. Stationary |
| `hvmine` | — | heavy mine | 150/200 | deployed at location | damage 50. IFF. Invisible. Cracks small ships |

## Officer skills

### Design principles

1. **Percentage bonuses** — skills grant +X% to relevant stats, staying relevant as modules scale.
2. **Three ranks** (Basic → Advanced → Expert) — each replaces the previous. Retraining costs time + resources.
3. **Tech-gated availability** — ranks unlock when prerequisite technologies are researched. Unlocked skills appear in the player report (mirroring "Known Technologies").
4. **Trained at any research module** — no per-module skill restrictions. Module tech tier gates which rank is trainable (L0-L2 module → Basic; L4-L6 → Advanced; L8-L10 → Expert).
5. **Parallel slots, not speed** — multiple research modules on a stack increase simultaneous trainees (1 officer per module) but do NOT reduce training duration.
6. **Some skills hidden until discovery** — xenobiology requires alien wreckage; exoagriculture requires non-terran colony; ark helm requires L10 propulsion.

### Legacy skills (live, unchanged)

`frgplt` `sscmnd` `hmedic` `arpldr` `inbtcm` — kept as-is for backward compatibility until engine supports percentage semantics.

### Tiered skill catalog

| Skill family | Branch | Basic (B) | Advanced (A) | Expert (E) |
|---|---|---|---|---|
| **Pilot** `piltB/A/E` | Propulsion | -10% travel time | -20% travel time | -30% travel time |
| **Commander** `cmdrB/A/E` | Command | +10% stack defense | +20% stack defense | +30% stack defense |
| **Engineer** `engrB/A/E` | Production | +15% build rate | +25% build rate | +35% build rate |
| **Gunnery** `gnryB/A/E` | Military | +15% attack | +25% attack | +35% attack |
| **Tactics** `taktB/A/E` | Military | +10% initiative | +20% initiative | +30% initiative |
| **Medic** `medcB/A/E` | Medical | +15% cure chance | +25% cure chance | +35% cure chance |
| **Scout** `scutB/A/E` | Research | +15% scan/prospect speed | +25% scan/prospect | +35% scan/prospect |
| **Mining** `minrB/A/E` | Production | +10% extraction rate | +20% extraction | +30% extraction |
| **Naval** `navlB/A/E` | Command | +10% fleet evasion | +20% fleet evasion | +30% fleet evasion |
| **Farmer** `farmB/A/E` | Habitat | +15% food output | +25% food output | +35% food output |
| **Xenobiology** `xenoB/A/E` | Research | +10% alien research | +20% alien research | +30% alien research |
| **Ark Helm** `arkhB/A/E` | L10 Command | +10% ark defense | +20% ark defense | +30% ark defense |

### Training details

| Rank | Training weeks | Resource cost | Required research module tier |
|------|---------------|--------------|-------------------------------|
| Basic | 4–6 wk | none (opportunity cost only) | L0–L2 (`reslb1`) |
| Advanced | 8–12 wk | 1 tier-appropriate resource | L4–L6 (`reslb2`) |
| Expert | 14–20 wk | 2 tier-appropriate resources | L8–L10 (`reslb3`) |

Training resource by branch:

| Branch | Advanced consumes | Expert consumes |
|--------|-------------------|-----------------|
| Propulsion | 1 `h2o2` | 2 `deutrm` |
| Command | 1 `silici` | 2 `reeox` |
| Production | 1 `copper` | 2 `tungst` |
| Military | 1 `uraniu` | 2 `nitrat` |
| Medical | 1 `water` | 2 `ammoni` |
| Research | 1 `silici` | 2 `berylm` |
| Habitat | 1 `food` | 2 `alumin` |

### Tech gates and discovery requirements

| Skill | Basic gate | Advanced gate | Expert gate |
|--------|-----------|---------------|-------------|
| Pilot | — (start) | `jmpdvr` (L3) | `fusdrv` (L8) |
| Commander | — (start) | `tacnet` (L4) | `arkcns` (L10) |
| Engineer | — (start) | `indprs` (L4) | `arkfnd` (L9) |
| Gunnery | — (start) | `kntcgn` (L4) | `spngun` (L8) |
| Tactics | — (start) | `ewsens` (L4) | `arkew` (L9) |
| Medic | — (start) | `pharms` (L4) | `crymed` (L8) |
| Scout | — (start) | `astprp` (L3) | `bolsen` (L7) |
| Mining | — (start) | `tminng` (L2) | `asttow` (L7) |
| Naval | `hydstg` (L1) | `iondrv` (L5) | `plsdv` (L9) |
| Farmer | — (start) | `afrmng` (L4) | `agrark` (L9) |
| Xenobiology | *alien wreckage discovery* | `xbiolb` (L5) | `xnofrn` (L8) |
| Ark Helm | `arkfnd` (L9) | `arkcns` (L10) | `arkeng` (L10) |

### XML example

```xml
<entry name="engrA" name-en="advanced engineer"
       training-duration="10"
       requires-tech="indprs"
       replaces-skill="engrB">
  <usable-in module-group="production"/>
  <produce effect="build rate" target="stacked" value="0.25"/>
  <consume type="copper" quantity="1"/>
</entry>
```

## Naming collisions to avoid

Do not reuse: `repair` (tech), `lifsys` (tech **and** module already share? check campaign — live tech `airgen` produces `lifsys` module; tech id `lifsys` also exists in basic list). **Never** add an item named `lifsys`. Scan `campaign/data.xml` before minting an id.

Taken (this pass): `sckbay` (module), `sckcns` (tech), `pharms` (tech). Military items `prllsr` `prlgun` `psnarm` `psnshd` `psnew` `prxgrd`; modules `proxpd` `capshd` `arkshd` `arkpd`. Live `medfac` / `medtec` / `medirf` / `medici` / `wndtrn` kept. New ores `lithia` `boron` `berylm` `xenon` `reeox` `grphit` `nitrat`; refined `hydzn`. Do not mint an item named `boron` if a module ever uses that id — currently item-only.

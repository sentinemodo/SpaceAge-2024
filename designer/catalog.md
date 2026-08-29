# Catalog proposals (resources, modules, items, skills, equipment)

Ids ≤ 6 characters. No item id equal to a module id. Descriptions: mechanism and environment, not mysticism.

Live catalog (`Tests/data.xml`) is the baseline. Below are **additions** for `campaign/data.xml`.

Planet type **`adpnt`** remains in the campaign catalog but gamein Gates are `<alderson>` — see [`galaxy.md`](galaxy.md).

## Resources (item types)

Canonical dictionary (ids, size/mass, extraction techs, rarity, Arbor/Anvil, empty-system seeds): **[resources.md](resources.md)**. Do not keep a second table here.

Closed seed set: `iron` `titani` `silici` `copper` `uraniu` `carbon` `oil` `gold` `heliu3` `h2o2` `water` `food` `terair` `nickfe` `tungst` `deutrm` `ammoni` `methn` `volatl` `kerogn` `alumin` `platnm` `lithia` `boron` `berylm` `xenon` `reeox` `grphit` `nitrat`. Refined: `hydzn`.

## Module types (new)

Use live **groups**. Intended future group in parentheses → wishlist.

### Hull size classes (L0–L10)

All use `group="frigate"` until engine adds `corvette`/`cruiser`/`capital`/`ark`. See [`combat-balance.md`](combat-balance.md).

| Class | Id | Tech | Size / capacity | Nested combat | Hull HP |
|-------|-----|------|-----------------|---------------|---------|
| Patrol | `sshull` | L0–1 `ssassm` | 5k / 4.5k | 1–2 | 50 |
| Corvette | `corhul` | L2 `corvhl` | 8k / 7k | 2–4 | 80 |
| Frigate | `alnhul` | L4 (alien) / advanced hull | 12k / 10k | 4–6 | 120 |
| Destroyer | `deshul` | L6 `desthl` | 25k / 20k | 6–10 | 200 |
| Cruiser | `cruhul` | L8 `cruihl` | 45k / 35k | 10–16 | 350 |
| Ark | `arkhul` | L10 `arkcns` | 80k / 60k | 20–40 | 500 |

### Other modules

| Id | Group | Size/mass (order of) | Crew / energy | What it is |
|----|-------|----------------------|---------------|------------|
| `sckbay` | habitat | 380/250 | 3 / 4 | Sick bay (inpatient ward). Habitat 8, HP 32, tech-cap 2, cash upkeep 40, radiation −120. **Not** `medfac` (clinic: 300/200, crew 2, energy 2, habitat 5, HP 25). Stronger than the clinic: 2 `wndtrn`/4 wk without `medici`, 4/wk with 1 `medici` per conversion (wishlist). No `operation-allowed-in` — settlements, stations, hulls. `pharms` USE produces `medici` from 1 `food` |
| `coastr` | vehicle | 250/100 | 1 / 0 | Coastal transport. Naval MOVE speed 1. Solid+liquid (port + sea). Oil fuel. L0 `nvltrs` |
| `gunbot` | vehicle | 240/240 | 12 / 0 | Gunboat. Naval MOVE speed 1. Kinetic. Solid+liquid. L1 `nvlcbt` |
| `fshfrm` | agricultural | 1000/100 | 5 / 5 | Fishery. Liquid-surface + `terair` only (not grassland). Nets plus photic seaweed/algae. L0 `fshng`; harvest `fshhrv` (terair, not planet-type ocean) |
| `hydnoz` | propulsion | 800/900 | 1 / 40 | Staged hydrolox; mass-capacity ~20000; fuel `water` or `h2o2` |
| `iondrv` | propulsion | 500/400 | 1 / 50 | Gridded ion; mass-capacity ~25000; fuel `water` |
| `nepeng` | propulsion | 1400/1600 | 2 / 70 | Nuclear-electric; mass-capacity ~35000; fuel `uraniu`+`water` |
| `optlab` | research | 250/60 | 2 / 6 | output 1, tech-cap 5 |
| `ntreng` | propulsion | 1200/1400 | 2 / 80 | Nuclear thermal; `move` space mass-capacity ~40000; fuel `water` |
| `orbfry` | production | 4000/800 | 8 / 40 | Vacuum foundry; `use` orbit |
| `radlab` | research | 300/80 | 4 / 8 | output 2, tech-cap 6 |
| `railgn` | military | 800/900 | 4 / 20 | **kinetic** `weapon-group`; attack 14 defense 2 damage 16 HP 140 |
| `clslss` | habitat | 400/200 | 2 / 12 | produce `terair`+`water` from `wastes`+energy |
| `isrplt` | extraction | 1500/1500 | 8 / 20 | consumes regional `volatl`/`dust` flavour via USE techs |
| `mpddrv` | propulsion | 900/800 | 2 / 60 | mass-capacity ~80000; fuel `methn` |
| `pdltur` | military | 200/150 | 1 / 15 | **laser** PD; attack 8 damage 8 initiative 8 HP 90 |
| `survsc` | research | 150/40 | 2 / 4 | output 1, tech-cap 4 |
| `dhefrc` | energy | 2000/1800 | 6 / 20 | produce energy ~250 / 13 wk; consume `heliu3`+`deutrm` |
| `whlhul` | habitat | 20000/8000 | 20 / 40 | spin habitat; population-maximum 2000 |
| `fuseng` | propulsion | 2500/2200 | 4 / 120 | mass-capacity ~2e5 |
| `drnctl` | command | 300/120 | 2 / 10 | tech-cap 4 |
| `matlab` | research | 400/100 | 6 / 12 | output 3, tech-cap 8 |
| `msdrst` | production | 8000/10000 | 12 / 80 | surface; operation solid-surface |
| `crytnk` | storage | 2000/400 | 1 / 8 | capacity 5000 |
| `vasmdr` | propulsion | 1800/1600 | 3 / 100 | fuel `water` or `methn` |
| `armplt` | frigate | 2000/3000 | 0 / 0 | **armour** `resists`/`armor-module`; defense 15, HP 200 |
| `xbiolb` | research | 250/80 | 4 / 6 | |
| `lghul` | frigate | 25000/8000 | 0 / 20 | large hull alias; prefer `deshul` destroyer class |
| `tanker` | spacecraft | 6000/2000 | 6 / 30 | move space; cargo |
| `spnknc` | military | 4000/5000 | 12 / 40 | **kinetic**; attack 28 damage 32 HP 220 |
| `dpsens` | research | 600/200 | 4 / 15 | |
| `eclssx` | habitat | 800/400 | 4 / 20 | near-closed loop |
| `agrdek` | agricultural | 2000/600 | 15 / 25 | produce food in space (`operation` orbit/space) |
| `plsdv` | propulsion | 4000/3500 | 6 / 180 | ark-class; mass-capacity ~8e5 |
| `ciwst` | military | 400/300 | 2 / 20 | **pbpd** `resists`; attack 8 damage 10 defense 12 |
| `arkmed` | research | 500/150 | 8 / 10 | |
| `arkhul` | frigate | 80000/25000 | 0 / 80 | L10 ark hull; capacity 60000; HP 500; hosts 20–40 combat stacks |
| `arkeng` | propulsion | 8000/7000 | 8 / 200 | mass-capacity ~2e6 |
| `arkbrg` | command | 2000/800 | 12 / 30 | tech-cap 16, research-output 4 |
| `arkdfn` | military | 3000/2500 | 10 / 50 | **laser** grid |
| `solthp` | energy | 800/600 | 2 / 4 | solar thermal; ~30 energy / 13 wk; inner-system |
| `slsmod` | propulsion | 400/80 | 1 / 2 | sail; mass-cap ~15000; no fuel |
| `msltub` | military | 300/400 | 2 / 8 | **missile**; attack 12 damage 14 HP 100 |
| `ewantn` | military | 150/80 | 2 / 10 | **ew** `resists`; defense 8 initiative 4 |
| `hypeng` | propulsion | 700/800 | 1 / 25 | mass-cap ~18000; fuel `hydzn` |
| `hlthst` | propulsion | 600/500 | 1 / 55 | mass-cap ~30000; fuel `xenon` |
| `h2cell` | energy | 400/300 | 2 / 8 | ~40 energy / 13 wk; fuel `h2o2` |
| `crylab` | research | 280/90 | 3 / 8 | output 2, tech-cap 5 |
| `cmplab` | research | 300/100 | 4 / 8 | output 2, tech-cap 6 |
| `shplas` | military | 400/350 | 3 / 25 | **shield** `resists`; defense 12, attack 0, HP 120 |
| `o2plt` | extraction | 1200/1200 | 6 / 18 | volatiles → `h2o2`/`terair` |
| `cerkil` | production | 1500/1200 | 6 / 30 | B4C/SiC kiln |
| `limpd` | propulsion | 950/850 | 2 / 70 | Li-MPD; mass-cap ~90000; fuel `lithia` |
| `arcjet` | propulsion | 500/450 | 1 / 45 | mass-cap ~22000; fuel `water` |
| `seissc` | research | 180/80 | 2 / 6 | output 1, tech-cap 5 |
| `bwinow` | research | 120/40 | 2 / 4 | x-ray windows |
| `kpdtur` | military | 250/280 | 2 / 12 | **kinetic**; attack 10 damage 12 initiative 6 HP 110 |
| `cermpl` | frigate | 1200/1800 | 0 / 0 | **armour** `resists`/`armor-module`; defense 12, HP 150 |
| `ccplnk` | production | 2000/1500 | 8 / 40 | C-C layup |
| `mgsail` | propulsion | 800/400 | 2 / 20 | mag-sail; mass-cap ~1e5; no fuel |
| `xendrv` | propulsion | 700/550 | 2 / 80 | xenon ion; mass-cap ~50000 |
| `coilgn` | military | 1200/1400 | 6 / 35 | **kinetic**; attack 18 damage 20 HP 160 |
| `miscpu` | command | 200/80 | 2 / 8 | tech-cap 3 |
| `maglab` | research | 350/120 | 4 / 10 | output 2, tech-cap 6 |
| `ntdiag` | research | 300/100 | 4 / 10 | output 2 |
| `recykl` | production | 1000/800 | 6 / 20 | scrap → iron/spare |
| `bioplt` | production | 800/400 | 8 / 15 | cultured polymer |
| `chmup2` | propulsion | 900/1000 | 2 / 30 | insertion stage; fuel `hydzn` |
| `plsail` | propulsion | 1200/600 | 3 / 25 | plasma sail; no fuel |
| `crumis` | military | 500/600 | 0 / 10 | **missile**; attack 18 damage 22 HP 130; fuel `h2o2` |
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
| `proxpd` | military | 250/200 | 2 / 15 | **pbpd** `resists`; attack 6 damage 8 defense 10 initiative 8 |
| `capshd` | military | 800/600 | 4 / 40 | **shield** `resists`; defense 16, attack 0, HP 180 |
| `arkshd` | military | 2500/1800 | 6 / 60 | **shield**; defense 20. Not habitat `arkshl` |
| `arkpd` | military | 1800/1400 | 6 / 35 | **pbpd**; attack 8 damage 10 |

Tune hit-points ≈ `(mass+size)/20` if omitted. Cash upkeep: [`economy.md`](economy.md) — `size/200 + 4×crew + build-cost/8 + 10×level`, round to 5, minimum 5 except size ≤ 15.

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

## Officer skills

Live: `frgplt` `sscmnd` `hmedic` `arpldr` `inbtcm`. Loader stores `training-duration`, `attack`, `defense`, `initiative`. Extra children are ignored — still write `usable-in` for reports.

| Id | Name | Weeks | Bonuses | Usable with |
|----|------|-------|---------|-------------|
| `ntrplt` | nuclear thermal pilot | 6 | init 4 def 2 | propulsion / frigate |
| `astrog` | astrogation | 6 | init 6 | command, spacecraft |
| `chfeng` | chief engineer | 8 | def 3 | production, propulsion |
| `gunnry` | gunnery director | 6 | attack 6 init 3 | military; flavour: pick a weapon group |
| `arpldr` | armor platoon leader | live | attack 5 init 5 | vehicle; **kinetic**+**armour** |
| `inbtcm` | infantry battalion commander | live | attack 5 init 5 | infantry; mixed four groups |
| `snsroff` | sensor officer | 5 | init 4 | research, command |
| `logoff` | logistics officer | 5 | — | storage, settlement |
| `xenbio` | xenobiology | 8 | — | research (`xbiolb`) |
| `radmed` | radiation medicine | 6 | (cure-chance wishlist) | medical / `hmedic` line |
| `arkplt` | ark helm | 10 | def 8 init 4 | L10 hull / command |
| `exoagr` | exoagriculture | 6 | — | agricultural in vacuum |

## Naming collisions to avoid

Do not reuse: `repair` (tech), `lifsys` (tech **and** module already share? check campaign — live tech `airgen` produces `lifsys` module; tech id `lifsys` also exists in basic list). **Never** add an item named `lifsys`. Scan `campaign/data.xml` before minting an id.

Taken (this pass): `sckbay` (module), `sckcns` (tech), `pharms` (tech). Military items `prllsr` `prlgun` `psnarm` `psnshd` `psnew` `prxgrd`; modules `proxpd` `capshd` `arkshd` `arkpd`. Live `medfac` / `medtec` / `medirf` / `medici` / `wndtrn` kept. New ores `lithia` `boron` `berylm` `xenon` `reeox` `grphit` `nitrat`; refined `hydzn`. Do not mint an item named `boron` if a module ever uses that id — currently item-only.

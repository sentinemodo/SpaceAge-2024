# Campaign XML schema (live loader)

Source: `Game/game/DataFile.cs`, `Contract.LoadXml`. Encoding `windows-1251`. Ids (`name`) ≤ 6 characters. `ValidateTypeNameUniqueness`: an id must not be both an item/race and a module.

Unknown **attributes** are ignored. Unknown **module `group`**, **`location-type`**, or contract **`trigger`** throw on load — do not put those in `campaign/` until TDD lands the wishlist row.

## Catalog (`data.xml`) — `/data/.../entry`

| Section | Required on `entry` | Children / notes |
|---------|---------------------|------------------|
| `star` | `name`, `name-en` | `description` |
| `planet` | `name`, `name-en` | Live types: `ocean`, `gasgnt`, `dust`, `abelt` (unused in gamein — belts are `<belt>`), `adpnt` (unused in gamein — Gates are `<alderson>`) |
| `moon` | `name`, `name-en` | Live types: `ice`, `rock`, `vulcan`, `ring` |
| `region` | `name`, `name-en`, `location-type` | `orbit` \| `solid-surface` \| `liquid-surface` \| `space` |
| `item` | `name`, `name-en` | `name-en2`, `description`, `size`, `mass`, `attack`, `damage`; `upkeep`/`consume` `type`+`quantity`; `use-allowed-by` `module-type-group`; design attr `value` (nominal price, ignored until TDD — [`economy.md`](economy.md)) |
| `technology` | `name`, `name-en`, `level` | `tags`, `requires`, `use-time`, `cost`, combat bonuses; `use-allowed-in`; `use-consume` / `use-produce` (`item` \| `module` \| `effect`) |
| `module` | `name`, `name-en`, `group` | size/mass/capacity/crew/energy/`hit-points`/`technology-capacity`/`research-output`; combat: `attack`/`defense`/`damage`/`initiative`; live combat attrs: `weapon-group`, `resists`, `armor-module` (SampleGame omits them → flat dice); `upkeep`, `fuel`, `move`, `produce`, `operation-allowed-in`, `use` |
| `race` | `name`, `name-en` | Also creates an `ItemType` (crew). `officer-training-duration`; upkeep `crew-type` crew\|officer |
| `skill` | `name`, `name-en` | `training-duration`, `attack`, `defense`, `initiative` |

Research cost if `cost` omitted: `8 * 2^(level-1)` (L1=8, L10=4096). Level 0 is never researched.

### Module groups (crash if misspelled)

`agricultural` `command` `energy` `extraction` `frigate` `corvette` `destroyer` `cruiser` `capital` `ark` `habitat` `infantry` `military` `production` `propulsion` `research` `settlement` `spacecraft` `space station` `storage` `vehicle`

Parse accepts the extra hull groups. **Campaign catalog still emits `group="frigate"`** on `corhul`/`deshul`/`cruhul`/`arkhul` until the hull-group todo retags them **and** expands every `operation-allowed-in` / `usable-in` `module-group="frigate"` allow-list (drills, drives, weapons) to include the new groups. Do not retag without those allow-lists.

### Combat attributes (live; SampleGame omits them)

| Attr | Values | On |
|------|--------|-----|
| `weapon-group` | `laser` `kinetic` `missile` `drone` | weapons |
| `resists` | `shield` `armour` `ew` `pbpd` | defence modules |
| `armor-module` | `true` | armour plates (5× hit weight + no capture when engine lands) |

Balance ladders: [`combat-balance.md`](combat-balance.md). Cash upkeep: [`economy.md`](economy.md).

### Technology `use-allowed-in` / module `operation-allowed-in`

Attributes: `module-type-group`, `location-type`, `planet-type`, `planet-atmosphere` (item id, usually `terair`).

`use-produce effect` needs `target` and `change` or load throws. `UseOrder` may still not execute the effect — wishlist, do not drop the tech.

## Game state (`gamein.xml`) — `/game`

The loader hardcodes filename `gamein.xml` under `/data`. The committed seed is `campaign/gamein.1.xml`; copy it to the run `/data/gamein.xml` and copy `campaign/data.xml` to `/data/data.xml`. Never point `/data` at `campaign/` (would write `gameout` into the catalog tree). Never put `gamein.xml` in `/turn-dir`.

**Reports-only** (`/reports`): load + `GenerateReports`, no `Execute`. Seed `turn="1"` writes `report.1.{faction}.txt` in `/turn-dir` (plus `.xml` when `xml-report` is true).

```
<game turn="N">
  <faction name="1" name-en="United Star Nations" ... />
  <faction name="2" ... />                 <!-- players 2–11 -->
  <faction name="12" name-en="Arbor First" ... />
  <faction name="13" name-en="HCS" ... />
  <contracts>...</contracts>
  <galaxy>...</galaxy>
  <orders/>
</game>
```

Faction `name` **1** is still the unfiltered NPC in XML reports (`SaveGame` skips faction filter when the viewer is `1`). Players 2–11 and militias 12–13 are filtered.

### Galaxy

```
<system name="SS0001" name-en="..." X="0" Y="0" Z="0">
  <star name="S00001" name-en="..." type="M4" mass="1"/>
  <planet name="P00001" name-en="..." type="ocean" AU="1" surface-size-X="6" surface-size-Y="6">
    <race type="terran"/>
    <moon name="M00001" name-en="..." type="rock" AU="0.0026" surface-size-X="4" surface-size-Y="3">
      <orbit name="O00001"/>
      <region name="R00001" name-en="..." X="0" Y="0" type="dust">...</region>
    </moon>
    <orbit name="O00002"/>
    <region ...>
      <capacity group="settlement" quantity="8"/>
      <exit region="R00002"><exitmode mode="ground" duration="3"/></exit>
      <!-- live modes: ground | naval | space. sea/ocean (and any exit that touches one) use mode="naval" (port = adjacent land). no coastal region type -->
      <resource type="iron" quantity="20"/>
      <modulestack .../>
    </region>
  </planet>
</system>
```

- `<race type="terran"/>` is a child of `<planet>` or `<moon>` (native biosphere). `LoadGalaxy` also still loads `<race>` under `<orbit>` (SampleGame fallback).
- Asteroid belts are `<belt name="P00003" AU="2.7">` with `<composition><resource type="uraniu" quantity="80" probability="0.5"/></composition>`. No orbit, no child regions (asteroids spawn later). `MOVE P00003` occupies the belt (`location-type` space). Region `<exit belt="P00003">` carries space duration until AU×drive. Rings are `<belt>` children of a `<planet>` (gas giant), same as moons.
- Alderson Gates are `<alderson name="P00009" AU="80" pair="P00010">` with one `<orbit>` and **no** child regions and **no** region exits. Do not emit `<exit alderson=>` from planetary regions. `JUMP P00010` while at Helios Gate orbit is 1 week to the pair’s orbit (ships only). Reaching a Gate from a planet is AU×drive (`SpaceTransit` × `fustor` speed 1). See [au-transit.md](au-transit.md).
- Gas giants: orbit + moons; no solid-surface regions.
- `AU` is stored; system `X Y Z` are currently commented out in the loader — still set them for later.
- Design attrs on `<planet>` / `<moon>` (ignored until TDD): `gravity="low|normal|high"`, `temperature="habitable|cold|hot"`, `atmosphere="none|thin|terair|hostile"`. See [`environments.md`](environments.md).
- Moon `name` must be unique. The loader currently constructs moons with the **planet** id (known bug); still emit unique moon ids and wishlist the fix.
- Region exits: second pass walks **planet regions only**. Moon-region exits may not load; keep moon maps small or wishlist.
- `surface-size-X/Y` is not enforced as region count; still match the grid.

### Contracts

`give-module`: `location`, `issuer`, `trigger="give-module"`, `reward-type="technology"|"unit"`, `reward`, `quantity`, `module`, `receiver`, `baseline`, optional `title`, `flavour`.

`research`: `trigger="research"`, `target` (wreckage stack), `points`, optional `progress`, `winner`. Reward is a tech copy or a unit.

Faction **1** issuing a contract is the usual NPC patron. Militias 12/13 do not issue t=1 contracts.

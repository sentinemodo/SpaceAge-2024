# Campaign XML schema (live loader)

Source: `Game/game/DataFile.cs`, `Contract.LoadXml`. Encoding `windows-1251`. Ids (`name`) ≤ 6 characters. `ValidateTypeNameUniqueness`: an id must not be both an item/race and a module.

Unknown **attributes** are ignored. Unknown **module `group`**, **`location-type`**, or contract **`trigger`** throw on load — do not put those in `campaign/` until TDD lands the wishlist row.

## Catalog (`data.xml`) — `/data/.../entry`

| Section | Required on `entry` | Children / notes |
|---------|---------------------|------------------|
| `star` | `name`, `name-en` | `description` |
| `planet` | `name`, `name-en` | Live types: `ocean`, `gasgnt`, `dust`, `abelt`. Campaign adds `adpnt` (Alderson corona; no habitable surface). `to-system` / `to-point` attrs ignored until JUMP |
| `moon` | `name`, `name-en` | Live types: `ice`, `rock`, `vulcan`, `ring` |
| `region` | `name`, `name-en`, `location-type` | `orbit` \| `solid-surface` \| `liquid-surface` \| `space` |
| `item` | `name`, `name-en` | `name-en2`, `description`, `size`, `mass`, `attack`, `damage`; `upkeep`/`consume` `type`+`quantity`; `use-allowed-by` `module-type-group` |
| `technology` | `name`, `name-en`, `level` | `tags`, `requires`, `use-time`, `cost`, combat bonuses; `use-allowed-in`; `use-consume` / `use-produce` (`item` \| `module` \| `effect`) |
| `module` | `name`, `name-en`, `group` | size/mass/capacity/crew/energy/`hit-points`/`technology-capacity`/`research-output`; `upkeep`, `fuel`, `move`, `produce`, `operation-allowed-in`, `use` |
| `race` | `name`, `name-en` | Also creates an `ItemType` (crew). `officer-training-duration`; upkeep `crew-type` crew\|officer |
| `skill` | `name`, `name-en` | `training-duration`, `attack`, `defense`, `initiative`, `requires-tech`, `replaces-skill`; children: `<usable-in>`, `<produce>`, `<consume>` |

#### Skill attributes (new — engine wishlist)

| Attribute | Type | Description |
|-----------|------|-------------|
| `requires-tech` | tech id | Skill rank only available after this technology is researched. Omit for skills available from start. Appears in "Known Skills" report once gate satisfied |
| `replaces-skill` | skill id | On training completion, the named skill is removed from the officer. Used for rank upgrades (e.g. `engrA` replaces `engrB`) |
| `training-duration` | int (weeks) | Time to train. Multiple research modules on the same stack provide parallel slots (1 officer/module) but do not reduce duration |
| `<produce effect="..." target="stacked" value="0.XX"/>` | child element | Percentage bonus applied multiplicatively to the commanded stack's relevant stat. `value` is a decimal fraction (0.25 = +25%) |
| `<consume type="..." quantity="N"/>` | child element | Resource consumed when training completes (Advanced/Expert ranks). Deducted from stack inventory |
| `<usable-in module-group="..."/>` | child element | Restricts which module group benefits from the skill's produce effect. Officer must command a stack containing that group |

Research cost if `cost` omitted: `8 * 2^(level-1)` (L1=8, L10=4096). Level 0 is never researched.

### Module groups (crash if misspelled)

`agricultural` `command` `energy` `extraction` `frigate` `habitat` `infantry` `military` `production` `propulsion` `research` `settlement` `spacecraft` `space station` `storage` `vehicle`

### Technology `use-allowed-in` / module `operation-allowed-in`

Attributes: `module-type-group`, `location-type`, `planet-type`, `planet-atmosphere` (item id, usually `terair`).

`use-produce effect` needs `target` and `change` or load throws. `UseOrder` may still not execute the effect — wishlist, do not drop the tech.

## Game state (`gamein.xml`) — `/game`

```
<game turn="N">
  <faction name="1" name-en="NPC" ... />   <!-- name 1 is unfiltered NPC -->
  <faction name="2" ... />                 <!-- players 2–11 for a 10-player game -->
  <contracts>...</contracts>
  <galaxy>...</galaxy>
  <orders/>
</game>
```

### Galaxy

```
<system name="SS0001" name-en="..." X="0" Y="0" Z="0">
  <star name="S00001" name-en="..." type="M4" mass="1"/>
  <planet name="P00001" name-en="..." type="ocean" AU="1" surface-size-X="6" surface-size-Y="6">
    <moon name="M00001" name-en="..." type="rock" AU="0.0026" surface-size-X="4" surface-size-Y="3">
      <orbit name="O00001"/>
      <region name="R00001" name-en="..." X="0" Y="0" type="dust">...</region>
    </moon>
    <orbit name="O00002"><race type="terran"/></orbit>
    <region ...>
      <capacity group="settlement" quantity="8"/>
      <exit region="R00002"><exitmode mode="ground" duration="3"/></exit>
      <resource type="iron" quantity="20"/>
      <modulestack .../>
    </region>
  </planet>
</system>
```

- Asteroid belts are **planets** with `type="abelt"` (no separate belt element).
- Alderson points are **planets** with `type="adpnt"` at high AU; one `orbit` region. Pairing attrs ignored. `JUMP` is wishlist. Spaceport → corona uses planet-region `exit` `mode="space"`.
- Gas giants: orbit + moons; no solid-surface regions.
- `AU` is stored; system `X Y Z` are currently commented out in the loader — still set them for later.
- Moon `name` must be unique. The loader currently constructs moons with the **planet** id (known bug); still emit unique moon ids and wishlist the fix.
- Region exits: second pass walks **planet regions only**. Moon-region exits may not load; keep moon maps small or wishlist.
- `surface-size-X/Y` is not enforced as region count; still match the grid.

### Contracts

`give-module`: `location`, `issuer`, `trigger="give-module"`, `reward-type="technology"|"unit"`, `reward`, `quantity`, `module`, `receiver`, `baseline`, optional `title`, `flavour`.

`research`: `trigger="research"`, `target` (wreckage stack), `points`, optional `progress`, `winner`. Reward is a tech copy or a unit.

Faction **1** issuing a contract is the usual NPC patron.

# Level 0 and 1 technologies (campaign)

Catalog: `campaign/data.xml`, loaded by `Game/game/CatalogLoader.cs` (`DataFile.LoadConfigurationItems` delegates). Checked **30 Aug 2026** against engine **0.1.148**. Phase 1 catalog slice (optins, personal combat kit, ionthr) included.

This is the **campaign** L0–L1 excerpt for campaign-ai and campaign play. SampleGame manuals (`player/basic_technologies.md`, `player/advanced_technologies.md`) stay on `Tests/data.xml` and are not retargeted here.

This file lists **level 0 and level 1** technologies, then the **module types** and **item types** those technologies produce or consume. **Level 2+** are in [`player/campaign/advanced_technologies.md`](advanced_technologies.md). SampleGame manuals stay on `Tests/data.xml`.

Omitted `use-time` defaults to **1** week in `CatalogLoader`. Omitted consume/produce `quantity` defaults to **1**.

## Levels

**Level 0** technologies are always known and present. Any stack whose module group and location match `usable-in` can `USE` them. They do not occupy technology capacity, are not research breakthroughs, and do not need `COPY`.

**Level 1 and above** must exist as a **local copy** on the using stack (`UseOrder.HasTechnology`: `Producer.Technologies.Contains`). Get a copy by research (labs roll level 1 … faction max+1 into remaining capacity) or by `COPY <id> TO <stack>` from a same-location holder that already has it. Each copy uses `level` points of the stack’s technology capacity.

`USE` still needs matching module group, location, and consume items. Battle-only techs (no produce) are held as copies, not used as builds.

Campaign play starts on **Arbor** (Helios) or **Anvil** (Fomal). Arbor is organics-rich (`food`, `carbon`, `oil`, `water`) and metals-poor (`titani`, `copper` thin or pocket-only). Anvil is metals-rich (`iron`, `titani`, `silici`, `copper`) and organics-poor (`food`, `carbon`, `oil`). Neither start has the full industrial diet.

---

## Level 0

Always known. `USE` from a matching stack; no copy required.

**action and reaction [areact]**  
The construction of reaction-based drives, using cheap reaction mass heated by large energy systems.  
Works in: production. Use consumes: 10 iron `[iron]`, 10 titanium `[titani]`. Use produces: reaction drive `[rctdrv]`. Use-time: 2 weeks.

**agricultural complex [agrplx]**  
The placing of farms and others complexes on a planet for farming and ranching.  
Works in: production, solid-surface, terran atmosphere `[terair]`. Use consumes: 10 iron `[iron]`. Use produces: farming complex `[farms]`. Use-time: 4 weeks.

**breathing-gas generation [airgen]**  
Engineering for compact recyclers that produce canned terran breathing gas for habitats, ships and vacuum work.  
Works in: production. Use consumes: 2 iron `[iron]`, 1 copper `[copper]`. Use produces: life support system `[lifsys]`. Use-time: 2 weeks.

**copper mining [cminng]**  
The extraction, purification and refining of copper ores from a variety of locations. Copper mining is used on planetary surfaces and asteroids.  
Works in: extraction. Use produces: 2 copper `[copper]`. Use-time: 1 week.

**corporate management [corpmg]**  
Corporate governance and centralized assets management allow increase in cost-effectiveness of the entire branch and a knowledge sharing centre. Tag: `production`.  
Works in: production. Use consumes: 10 iron `[iron]`, 2 copper `[copper]`, 5 silicium `[silici]`. Use produces: corporate headquarters `[corphq]`. Use-time: 6 weeks.

**crew housing [crewhs]**  
Habitations for crew members in hostile environment require careful design to avoid cohabitation problems.  
Works in: production. Use consumes: 3 iron `[iron]`, 2 titanium `[titani]`. Use produces: crew quarters `[crwqrt]`. Use-time: 2 weeks.

**file indexing [filidx]**  
Digital library / indexing methods that support researchers (ingest, tag, and query large document sets). Tag: `research`.  
Works in: production. Use consumes: 1 iron `[iron]`, 3 silicium `[silici]`. Use produces: computer library `[cmplib]`. Use-time: 2 weeks.

**fishery construction [fshng]**  
Factory-built pontoon kits: nets, holds, and photic-zone seaweed or algae lines. Photosynthetic biomass in sunlit liquid on a terair world; tow the works to sea.  
Works in: production (factories; no sea-cell gate on the build). Use consumes: 10 iron `[iron]`. Use produces: fishery `[fshfrm]`. Use-time: 4 weeks.

**fishery harvest [fshhrv]**  
Haul fish plus photic-zone seaweed or algae in place. Photosynthetic biomass in sunlit liquid. Requires a fishery on liquid surface under terair.  
Works in: agricultural, on fishery `[fshfrm]`, **liquid-surface**, terran atmosphere `[terair]`. Not grassland. No `planet-type="ocean"` gate. Use produces: 5 food `[food]`. Use-time: 1 week (default).

**fossil use [fossil]**  
Use of fossil fuels allows one to produce huge and dirty plants that transform carbon-based resources into energy. Tag: `production`.  
Works in: production. Use consumes: 100 iron `[iron]`. Use produces: coal-burning plant `[cplant]`. Use-time: 8 weeks.

**ground transport [grndtr]**  
The most basic transportation means are large trucks, powered by oil consuming engines.  
Works in: production. Use consumes: 2 iron `[iron]`. Use produces: trucks `[trucks]`. Use-time: 2 weeks.

**hydrocarbons drilling [hcdril]**  
The extraction and refining of hydrocarbons, or fossil fuels, from a planetary surface.  
Works in: extraction, solid-surface. Use produces: 1 carbon `[carbon]`. Use-time: 1 week (default).

**ice mining [icemin]**  
Cut and melt water ice from polar caps, ice moons, and hydrated regolith. Produces bulk water for distillation and hydroponics. Tag: `production`.  
Works in: extraction, solid-surface. Use produces: 3 water `[water]`. Use-time: 1 week (default).

**industrial automation [indust]**  
Use of automated production management to reduce the workforce requirements. Tag: `production`.  
Works in: production. Use consumes: 15 iron `[iron]`, 10 titanium `[titani]`. Use produces: factory `[factry]`. Use-time: 4 weeks.

**intensive farming [farmng]**  
Long experience in exploitation techniques and breeds selection makes for intensive farming.  
Works in: agricultural, on ocean worlds with terran atmosphere `[terair]`, solid-surface. Use produces: 5 food `[food]`. Use-time: 1 week (default).

**iron mining [iminng]**  
The extraction, purification and refining of iron ores from a variety of locations.  
Works in: extraction, solid-surface. Use produces: 3 iron `[iron]`. Use-time: 1 week (default).

**mineral surface drilling [sdrill]**  
Mineral exploitation.  
Works in: production. Use consumes: 25 iron `[iron]`. Use produces: surface drill `[sdrill]`. Use-time: 3 weeks.

**naval transport [nvltrs]**  
Displacement hulls for cargo on liquid surfaces. Oil engines, hulls of iron. Same logistics as trucks, at sea.  
Works in: production. Use consumes: 2 iron `[iron]`. Use produces: coastal transport `[coastr]`. Use-time: 2 weeks.

**oil burning [oilbrn]**  
Use of oil fuels allows one to produce smaller plants that transform oil-based resources into energy.  
Works in: production. Use consumes: 80 iron `[iron]`. Use produces: oil-burning plant `[oplant]`. Use-time: 10 weeks.

**oil dwelling [oildwe]**  
The extraction, purification and refining of oil from variety of locations.  
Works in: extraction. Use produces: 2 oil `[oil]`. Use-time: 1 week (default).

**orbital complexes assembly [orassm]**  
The putting together of frames for basic orbital complexes.  
Works in: production, **orbit**. Use consumes: 2 iron `[iron]`. Use produces: orbital complex `[orcmpx]`. Use-time: 5 weeks.

**population center [popcnt]**  
Creation of basic infrastructure and housing for population.  
Works in: production, solid-surface, terran atmosphere `[terair]`. Use consumes: 100 iron `[iron]`. Use produces: city `[city]`. Use-time: 26 weeks.

**shuttles assembly [shtlas]**  
Most ships can never land, and rely on orbital shuttles for exploration, ferrying and construction. The construction of space-based hulls, bases and any other large objects is usually done in solar space above the planet surface.  
Works in: production. Use consumes: 2 iron `[iron]`, 1 titanium `[titani]`, 1 silicium `[silici]`. Use produces: space shuttle `[shuttl]`. Use-time: 4 weeks.

**silicium melting [slcmlt]**  
The extraction and refining of silicates into electronic-grade silicium for hi-tech elements.  
Works in: extraction, solid-surface. Use produces: 1 silicium `[silici]`. Use-time: 2 weeks.

**small scale transportation [strans]**  
Transport over interstellar distances and storage of freight poses logistics problems.  
Works in: production. Use consumes: 2 iron `[iron]`, 2 titanium `[titani]`. Use produces: small cargo bay `[cargob]`. Use-time: 2 weeks.

**space control [spctrl]**  
Any self-respecting space vessel requires a command structure to navigate and direct a ship.  
Works in: production. Use consumes: 1 iron `[iron]`, 4 titanium `[titani]`, 1 silicium `[silici]`. Use produces: command bridge `[cbridg]`. Use-time: 3 weeks.

**space ship assembly [ssassm]**  
Construction of spacecraft hulls in orbit: modular hull sections assembled for later fit-out (quarters, bridge, cargo, and so on).  
Works in: production, **orbit**. Use consumes: 6 iron `[iron]`, 4 titanium `[titani]`. Use produces: spaceship hull `[sshull]`. Use-time: 4 weeks.

**stationary defense [stnrdf]**  
The basic stationary defense. Gun emplacement and fortification to protect against ground based military. Tag: `military`.  
Works in: production. Use consumes: 8 iron `[iron]`, 4 titanium `[titani]`. Use produces: gun placement `[gunplc]`. Use-time: 8 weeks.

**titanium mining [tminng]**  
The extraction, purification and refining of titanium ores from a variety of locations.  
Works in: extraction, solid-surface. Use produces: 2 titanium `[titani]`. Use-time: 1 week (default).

**uranium fission [urfiss]**  
Heavy elements can be made to fission faster using heavy shielded structures to provide lasting power.  
Works in: production. Use consumes: 2 iron `[iron]`, 8 titanium `[titani]`, 5 copper `[copper]`. Use produces: fission reactor `[fisrec]`. Use-time: 6 weeks.

**uranium mining [uminng]**  
The extraction, purification and refining of pechblend, the basic uranium ore from a variety of locations. Uranium mining is used on planetary surfaces and asteroids.  
Works in: extraction (`usable-in="extraction"`). Use produces: 1 uranium `[uraniu]`. Use-time: 8 weeks.

**water distillation [wtrdst]**  
Electrolysis and distillation of liquid or melted ice water into oxyhydro propellant feedstock. Strategic: without regional water (or shipped water), fuel production stalls.  
Works in: extraction. Use consumes: 1 water `[water]`. Use produces: 3 oxyhydro `[h2o2]`. Use-time: 1 week (default).

**wind turbines [wndtrb]**  
Use the power of the wind as a method of generating power.  
Works in: production. Use consumes: 1 iron `[iron]`. Use produces: wind powerplant `[wnplnt]`. Use-time: 2 weeks.

---

## Level 1

Need a local copy on the using stack (`COPY` or research). Each copy uses 1 capacity.

**advanced farming [afrmng]**  
Advancing techniques and breeds selection allows increase of farms the output.  
Works in: agricultural, on ocean worlds with terran atmosphere `[terair]`, solid-surface. Use produces: 8 food `[food]`. Use-time: 1 week (default).

**armored combat [armcbt]**  
The basic mobile armor, powered by oil consuming engines. Tag: `military`.  
Works in: production. Use consumes: 8 iron `[iron]`, 2 titanium `[titani]`. Use produces: tanks `[tanks]`. Use-time: 10 weeks.

**city planning [ctypln]**  
Planning the area of the city allows more eficient use of the area.  
Works in: settlement, solid-surface, terran atmosphere `[terair]`. Use consumes: 500 cash `[cash]`, 26 iron `[iron]`, 1 city `[city]`. Use produces: metropoly `[mtrply]`. Use-time: 26 weeks.

**engineering shop [engshp]**  
Construction of a compact engineering shop that patches battle and maintenance damage using spare parts. Tags: `production`, `repair`. Research cost override: 4.  
Works in: production. Use consumes: 5 iron `[iron]`. Use produces: engineering shop `[engshp]`. Use-time: 2 weeks.

**form infantry battalion [frminf]**  
The basic infantry unit. Equipped with standard rifles. Tag: `military`.  
Works in: production. Use consumes: 4 iron `[iron]`. Use produces: infantry battalion `[inftry]`. Use-time: 13 weeks.

**gold recovery [gminng]**  
Trace precious-metal recovery from hydrothermal veins. Compact assay methods preserved in HCS crust manuals. Tag: `production`. **Requires:** copper mining `[cminng]`.  
Works in: extraction, solid-surface. Use produces: 1 gold `[gold]`. Use-time: 1 week (default).

**hydroponics [hydrop]**  
Closed-loop soilless agriculture fed by recycled water. Enables food production on ice moons and airless bases without grassland biosphere. Tag: `production`. **Requires:** agricultural complex `[agrplx]`.  
Works in: agricultural. Use consumes: 2 water `[water]`. Use produces: 3 food `[food]`. Use-time: 1 week.

**laser optics [lasopt]**  
The working of high-frequency lasers.  
Works in: production. Use consumes: 2 titanium `[titani]`, 6 copper `[copper]`, 2 silicium `[silici]`. Use produces: blue laser `[bltlas]`. Use-time: 10 weeks.

**laser turret [lstrrt]**  
A fixed laser emplacement for ground defense. Requires a power supply. Tag: `military`. **Requires:** laser optics `[lasopt]`.  
Works in: production. Use consumes: 6 iron `[iron]`, 4 titanium `[titani]`, 4 copper `[copper]`. Use produces: laser turret `[laztrt]`. Use-time: 10 weeks.

**law enforcement [lawenf]**  
The popular punishment for committed fellonies is to restrict ones' freedom of move. The question is to make the container durable and secure.  
Works in: production. Use consumes: 6 iron `[iron]`. Use produces: jail block `[jail]`. Use-time: 4 weeks.

**military tactics [miltac]**  
Elementary military tactics, which enable a higher level of combat proficiency. Tag: `military`. Initiative: 5.  
Works in: command. Use consumes: nothing. Use produces: nothing (battle tech). Use-time: 1 week (default).

**mineral core drilling [cdrill]**  
Mineral exploitation.  
Works in: production. Use consumes: 25 iron `[iron]`, 10 titanium `[titani]`. Use produces: core drill `[cdrill]`. Use-time: 3 weeks.

**naval combat [nvlcbt]**  
Gunboats: oil-fired displacement hulls with deck guns for liquid-surface combat. Same role as tanks, at sea. Tag: `military`.  
Works in: production. Use consumes: 8 iron `[iron]`, 2 titanium `[titani]`. Use produces: gunboat `[gunbot]`. Use-time: 10 weeks.

**nickel-iron extraction [nminng]**  
Fe-Ni alloy recovery from metal-rich crust and M-type rocks. HCS archive specialty — hard-rock beneficiation beyond simple iron pits. Tag: `production`. **Requires:** iron mining `[iminng]`.  
Works in: extraction, solid-surface. Use produces: 2 nickel-iron `[nickfe]`. Use-time: 1 week (default).

**orbital rocket launcher [orbrkt]**  
A rack of chemically boosted rockets sized to nest on a shuttle or station and fire in orbit. Built in a factory on the ground or assembled in space. Tag: `military`.  
Works in: production (no location-type limit). Use consumes: 4 iron `[iron]`. Use produces: orbital rocket launcher `[orbrkt]`. Use-time: 8 weeks.

**optical and IR instruments [optins]**  
Diffraction-limited telescopes, FTIR, and gold-coated contacts for survey and targeting research. Tag: `research`. **Requires:** file indexing `[filidx]`.  
Works in: production. Use consumes: 6 silicium `[silici]`, 4 copper `[copper]`, 2 iron `[iron]`, 1 gold `[gold]`. Use produces: optical lab `[optlab]`. Use-time: 4 weeks.

**personal armour [psnarm]**  
Ceramic-composite body armour resisting kinetics. Tag: `military`. **Requires:** stationary defense `[stnrdf]`.  
Works in: production. Use consumes: 2 iron `[iron]`, 1 titanium `[titani]`. Use produces: personal armour `[psnarm]` (item). Use-time: 3 weeks.

**personal laser [prllsr]**  
Compact man-portable high-frequency laser for infantry battalions. Tag: `military`. **Requires:** laser optics `[lasopt]`.  
Works in: production. Use consumes: 2 terran breathing gas `[terair]`, 1 copper `[copper]`, 1 oxyhydro `[h2o2]`. Use produces: personal laser `[prllsr]` (item). Use-time: 3 weeks.

**personal rail gun [prlgun]**  
Man-portable electromagnetic rail kinetic for infantry battalions. Tag: `military`. **Requires:** stationary defense `[stnrdf]`.  
Works in: production. Use consumes: 2 iron `[iron]`, 1 titanium `[titani]`. Use produces: personal rail gun `[prlgun]` (item). Use-time: 3 weeks.

**preventive servicing [servic]**  
Maintenance and repairs are best done in advance. Tag: `repair`.  
Works in: production. Use consumes: 1 titanium `[titani]`, 1 iron `[iron]`, 1 copper `[copper]`, 1 silicium `[silici]`. Use produces: 10 spare parts `[spare]`. Use-time: 1 week (default).

**repair and maintenance [repair]**  
Repair and maintenance allows you to recover from ship damage, either from military actions or neglect.  
Works in: production. Use consumes: 1 spare part `[spare]`. Use produces: effect `repair` on `module-damage` (`change="-1"`). Use-time: 2 weeks.  
`USE` of effect-producing techs throws “Not implemented”; issue **`REPAIR`** instead (see `player/rules.md`).

**rocket launcher production [rckter]**  
Manufacture of portable rocket launchers issued to infantry battalions. Tag: `military`. Research cost override: 4.  
Works in: production. Use consumes: 2 iron `[iron]`, 1 uranium `[uraniu]`. Use produces: rocket launchers `[rctlnc]` (item). Use-time: 6 weeks.

**staged hydrolox [hydstg]**  
Staged combustion of hydrogen and oxygen derived from water or stored oxyhydro. Vacuum Isp about 450 s. Fuel water or h2o2. Tag: `propulsion`. **Requires:** action and reaction `[areact]`.  
Works in: production. Use consumes: 8 iron `[iron]`, 4 titanium `[titani]`, 2 copper `[copper]`. Use produces: hydrolox stage `[hydnoz]`. Use-time: 5 weeks.

**waste disposal [wastdp]**  
This simple yet effective technique sends wastes into a sun, preventing accumulation of radio-active or unbreakable toxic wastes.  
Works in: spacecraft. Use consumes: 2 waste products `[wastes]`. Use produces: nothing. Use-time: 1 week.

---

## Module types

Catalog `<module><entry>` that a level 0 or 1 technology **produces** or **consumes**. Not every module in `campaign/data.xml`. A type produced at level 0 and consumed at level 1 is listed under level 0. Hulls `corhul` / `deshul` / `cruhul` / `arkhul` are L2+ products and are omitted here; they emit `corvette` / `destroyer` / `cruiser` / `ark` in the campaign catalog.

### Level 0

**city [city]**  
The city with basic infrastructure, underlying a settlement.  
Group `settlement`. Built by population center `[popcnt]`. Consumed (1) by city planning `[ctypln]` to make a metropoly. Size 25000, capacity 15000, energy 10, HP 1250, tech-cap 1, population max 10000. Cannot be owned; cannot hold item stacks. Upkeep 100 food (riot 25% if unpaid). Produces 1000 cash and 10 terran per 13 weeks. Operates on solid-surface worlds with terran atmosphere `[terair]`.

**coal-burning plant [cplant]**  
Provide energy by burining carbon.  
Group `energy`. Built by fossil use `[fossil]`. Size 1000, mass 1000, crew 2, energy 10, capacity 500, HP 100, attack 1. Upkeep 40 cash. Produces 40 energy / 13 weeks; burns 5 carbon. Fuel duration 13. Operates in settlement, solid-surface, terran atmosphere.

**coastal transport [coastr]**  
Displacement cargo hull for liquid surfaces. Oil engines. Can lie alongside a land region as a port.  
Group `vehicle`. Built by naval transport `[nvltrs]`. Size 250, mass 100, crew 1, capacity 150, HP 17. Upkeep 5 cash. Consumes 4 food and 4 terran air (damage 25% if not). Naval move speed 1. Fuel duration 13 (1 oil). Operates on **solid-surface and liquid-surface** with terran atmosphere `[terair]` (port + sea).

**command bridge [cbridg]**  
A command bridge is mandated for the control of any spaceship.  
Group `command`. Built by space control `[spctrl]`. Size 800, mass 300, crew 1, energy 5, capacity 200, HP 55, tech-cap 1. Upkeep 10 cash. Operates in frigate, space station.

**computer library [cmplib]**  
The efficiency of the computer library allows on to store and manipulate larger than usual files and technological reference documents.  
Group `research`. Built by file indexing `[filidx]`. Size 200, mass 50, energy 5, HP 50, tech-cap 4, research-output 1, value 1000. Upkeep 5 cash.

**optical lab [optlab]**  
Diffraction-limited optical and IR instruments for survey and targeting research.  
Group `research`. Built by optical and IR instruments `[optins]`. Size 250, mass 60, crew 2, energy 6, HP 35, tech-cap 5, research-output 1. Upkeep 35 cash.

**corporate headquarters [corphq]**  
A corporate headquarters allow centralized control of the corporation.  
Group `command`. Built by corporate management `[corpmg]`. Size 1000, mass 1000, crew 20, energy 10, capacity 750, HP 100, tech-cap 2, defense 5. Upkeep 90 cash. Produces 50 cash / week. Effects: 0.1 upkeep reduction and fast construction in the region. Operates in settlement.

**crew quarters [crwqrt]**  
These sealed and protected quarters house crew in the most hostile of the areas. They also allow slow recuperation of wounded crew members.  
Group `habitat`. Built by crew housing `[crewhs]`. Size 500, mass 400, crew 0, energy 1, capacity 250, HP 45, tech-cap 1, habitat 20, radiation -200. Upkeep 5 cash. Heal 0.1 on stacked.

**factory [factry]**  
Factories are everywhere since the dawn of Industral age, and even the Space age couldn't change the fact.  
Group `production`. Built by industrial automation `[indust]`. Size 1000, mass 750, crew 10, energy 15, capacity 500, HP 100, tech-cap 1. Upkeep 55 cash. Operates in settlement, frigate, space station.

**farming complex [farms]**  
A low-energy, low-technology food producing and harvesting complex.  
Group `agricultural`. Built by agricultural complex `[agrplx]`. Size 1000, mass 100, crew 5, energy 5, capacity 500, HP 55, tech-cap 1. Upkeep 30 cash. Operates in settlement, solid-surface, terran atmosphere.

**fishery [fshfrm]**  
Pontoon works on liquid surface: nets plus photic-zone seaweed or algae harvest. Photosynthetic biomass in sunlit water; needs terair and a sea or ocean cell.  
Group `agricultural`. Built by fishery construction `[fshng]`. Required in place (not consumed) by fishery harvest `[fshhrv]`. Size 1000, mass 100, crew 5, energy 5, capacity 500, HP 55, tech-cap 1. Upkeep 30 cash. Operates on **liquid-surface** with terran atmosphere `[terair]` only (not grassland; not a `planet-type="ocean"` gate).

**fission reactor [fisrec]**  
The basic nuclear power reactors, which works using heavy fissile elements to produce energy.  
Group `energy`. Built by uranium fission `[urfiss]`. Size 400, mass 140, crew 1, energy 10, capacity 100, HP 45, tech-cap 1, attack 2. Upkeep 15 cash. Produces 60 energy / 13 weeks; burns 1 uranium; produces 1 waste. Fuel duration 13. Unpaid upkeep: radiation 250 at 25%.

**gun placement [gunplc]**  
The gun placements are primary means of defense for the short range ground combat.  
Group `military`. Built by stationary defense `[stnrdf]`. `weapon-group` `kinetic`. Size 100, mass 500, crew 2, capacity 100, HP 60, tech-cap 1, attack 2, defense 2, damage 3. Upkeep 10 cash. Operates on solid region surface. Unpaid upkeep: damage 25%.

**life support system [lifsys]**  
Generates canned terran breathing gas for people and vehicles that cannot breathe the local atmosphere.  
Group `habitat`. Built by breathing-gas generation `[airgen]`. Size 100, mass 80, crew 0, energy 2, capacity 40, HP 20, tech-cap 1. Upkeep 5 cash. Produces 10 terran breathing gas `[terair]` / week. Operates in habitat, settlement, frigate.

**oil-burning plant [oplant]**  
Provide energy by burning oil.  
Group `energy`. Built by oil burning `[oilbrn]`. Size 800, mass 800, crew 2, energy 10, capacity 500, HP 80, attack 1. Upkeep 30 cash. Produces 30 energy / 13 weeks; burns 5 oil. Fuel duration 13. Operates in settlement, solid-surface, terran atmosphere.

**orbital complex [orcmpx]**  
An elongated set of struts, cables, frames and other interconnecting parts assembling modules in orbit. The orbital complex is cheap, simple and easy to build, but lacks sophistication.  
Group `space station`. Built by orbital complexes assembly `[orassm]`. Size 5000, mass 100, capacity 4500, energy 1, HP 50, radiation -50. Cannot hold item stacks. Upkeep 25 cash. Operates in orbit.

**reaction drive [rctdrv]**  
Chemical thermal rocket. Cheap oxyhydro for orbit and short inner-system burns. Not an AU torch: too little exhaust velocity for the Gate in a season.  
Group `propulsion`. Built by action and reaction `[areact]`. Size 600, mass 700, crew 1, energy 30, capacity 150, HP 65, tech-cap 1. Upkeep 15 cash. Space move, speed **0.5**, mass-capacity 10000. Fuel duration 1 (1 oxyhydro). Operates in frigate.

**small cargo bay [cargob]**  
The cargo bays may hold a wide variety of cargo for bulk transportations.  
Group `storage`. Built by small scale transportation `[strans]`. Size 2000, mass 200, crew 0, capacity 1800, HP 55, tech-cap 1. Upkeep 10 cash.

**space shuttle [shuttl]**  
Basic shuttle used for orbital constructions. It has basic construction facitilites, small fission reactor and is propelled by a small reaction drive.  
Group `production` (stays production so orbit `USE` still works). Built by shuttles assembly `[shtlas]`. Size 200, mass 30, crew 2, capacity 125, HP 17, tech-cap 1. Upkeep 10 cash. USE in orbit at 10× efficiency, requires fuel. Space move, speed 1 (omitted in catalog), mass-capacity 750. Fuel duration 13 (1 uranium, 1 oxyhydro). Operates on solid-surface, liquid-surface, orbit. Shuttle unit (`IsShuttleUnit` / hangar craft) by type id.

**spaceship hull [sshull]**  
The basic spaceship hull, it embodies the technology and experience in space travel.  
Group **`frigate`** (as emitted). Built by space ship assembly `[ssassm]`. Size 5000, mass 100, capacity 4500, energy 1, HP 50, defense 10, radiation -50. Cannot hold item stacks. Upkeep 30 cash. Operates in orbit and space. Campaign hulls `corhul` / `deshul` / `cruhul` / `arkhul` emit `corvette` / `destroyer` / `cruiser` / `ark` (L2+ products, not listed here).

**surface drill [sdrill]**  
Efficient mining system. A surface drill allows you to strip minerals and various resources out of the surface of any solid body.  
Group `extraction`. Built by mineral surface drilling `[sdrill]`. Size 500, mass 500, crew 6, energy 5, capacity 250, HP 50. Upkeep 35 cash. Operates on solid-surface, in settlement or frigate.

**trucks [trucks]**  
Group of large and slow ground moving trucks.  
Group `vehicle`. Built by ground transport `[grndtr]`. Size 250, mass 100, crew 1, capacity 150, HP 17. Upkeep 5 cash. Consumes 4 food and 4 terran air (damage 25% if not). Ground move speed 0.5. Fuel duration 13 (1 oil). Operates on solid-surface with terran atmosphere.

**wind powerplant [wnplnt]**  
Small serviceless energy system, utilising power of the wind.  
Group `energy`. Built by wind turbines `[wndtrb]`. Size 10, mass 10, energy 1, HP 1. Upkeep 1 cash. Produces 4 energy / 13 weeks. Operates in settlement with terran atmosphere on **both solid-surface and liquid-surface**.

### Level 1

**blue laser [bltlas]**  
A compact high-frequency laser suitable for ground combat.  
Group `military`. Built by laser optics `[lasopt]`. Requires technology laser optics `[lasopt]`. `weapon-group` `laser`. Size 100, mass 100, crew 1, energy 5, capacity 50, HP 50, tech-cap 2, attack 7, defense 1, damage 7. Upkeep 20 cash.

**core drill [cdrill]**  
Advanced mining system. A core drill allows you to strip minerals and various resources out of the core of any solid body.  
Group `extraction`. Built by mineral core drilling `[cdrill]`. Size 1000, mass 1000, crew 6, energy 5, capacity 750, HP 100, tech-cap 4. Upkeep 50 cash. Faster extraction on self. Operates on solid-surface, in settlement or frigate.

**engineering shop [engshp]**  
A small workshop that repairs the parent module stack and nested stacks. REPAIR restores 20 hit points per week and consumes 1 spare part, or 1 hit point with no parts.  
Group `production`. Built by engineering shop `[engshp]`. Size 25, mass 20, crew 2, energy 1, capacity 10, HP 15, tech-cap 1. Upkeep 20 cash. Operates in settlement, frigate, spacecraft, space station.

**gunboat [gunbot]**  
Oil-fired gunboat for liquid-surface combat. Deck guns against hulls and coastal modules.  
Group `vehicle`. Built by naval combat `[nvlcbt]`. `weapon-group` `kinetic`. Size 240, mass 240, crew 12, capacity 200, HP 70, attack 5, defense 3, damage 6. Upkeep 80 cash (rebel 10% if unpaid). Consumes 12 food and 12 terran air (damage 25% if not). Naval move speed 1. Fuel duration 13 (3 oil). Operates on **solid-surface and liquid-surface** with terran atmosphere `[terair]` (port + sea).

**hydrolox stage [hydnoz]**  
Staged hydrolox rocket; fuel water or oxyhydro. Better mass-capacity than a reaction drive, still chemical: not an AU torch.  
Group `propulsion`. Built by staged hydrolox `[hydstg]`. Size 800, mass 900, crew 1, energy 40, HP 45. Upkeep 25 cash. Space move, speed 0.5, mass-capacity 20000. Fuel duration 13 (2 oxyhydro). Operates in frigate.

**infantry battalion [inftry]**  
Infantry battalion used for claiming cities, and taking over hostile modules.  
Group `infantry`. Built by form infantry battalion `[frminf]`. `weapon-group` `kinetic`. Size 500, mass 500, capacity 300, HP 50, attack 2, defense 2, damage 2, can-convert, value 50. Upkeep 15 cash. Consumes 100 food and 100 terran air (damage 25% if not). Unpaid upkeep: rebel 10%. Ground move speed 0.1. Operates on solid surface with terran atmosphere.

**jail block [jail]**  
Secure and durable - cells you want to be when the criminals and prisoners are put there for containment.  
Group `habitat`. Built by law enforcement `[lawenf]`. Size 500, mass 800, crew 5, energy 2, capacity 250, HP 65, tech-cap 1, habitat 40. Upkeep 35 cash. Effect: immobile on stacked.

**laser turret [laztrt]**  
A fixed laser emplacement for ground defense. Requires a power supply.  
Group `military`. Built by laser turret `[lstrrt]`. Requires technology laser optics `[lasopt]`. `weapon-group` `laser`. Size 100, mass 500, crew 2, energy 5, capacity 100, HP 80, tech-cap 1, attack 7, defense 1, damage 7. Upkeep 25 cash. Operates on solid region surface.

**metropoly [mtrply]**  
Well designed city with advanced infrastructure, that allows building tall buildings and effective public transport.  
Group `settlement`. Built by city planning `[ctypln]` (consumes 1 city). Size 25000, capacity 20000, energy 20, HP 1250, tech-cap 2, population max 15000. Cannot be owned; cannot hold item stacks. Upkeep 150 food (riot 25% if unpaid). Produces 1500 cash and 15 terran per 13 weeks. Operates on solid-surface worlds with terran atmosphere.

**orbital rocket launcher [orbrkt]**  
A rack of chemically boosted rockets that only fires in orbit. Compact enough to nest in a shuttle with crew, fuel, food, and breathing mix.  
Group `military`. Built by orbital rocket launcher `[orbrkt]`. `weapon-group` `missile`. Size 40, mass 40, crew 0, capacity 0, HP 40, tech-cap 1, attack 3, defense 1, damage 3. Upkeep 10 cash. Operates only in orbit. Not a shuttle unit: stays nested on the shuttle, fires on the parent’s shot sequence, and is included in the standard hit-location roll.

**tanks [tanks]**  
Platoon of 4 armored vehicles suitable for destroying ground modules and infantry battalions.  
Group `vehicle`. Built by armored combat `[armcbt]`. `weapon-group` `kinetic`. Size 240, mass 240, crew 16, capacity 200, HP 80, attack 6, defense 4, damage 7. Upkeep 80 cash. Consumes 16 food and 16 terran air (damage 25% if not). Unpaid upkeep: rebel 10%. Ground move speed 0.5. Fuel duration 13 (4 oil). Operates on solid surface with terran atmosphere.

---

## Item types

Catalog `<item><entry>` that a level 0 or 1 technology **produces** or **consumes**. Not the full item list (no medicines, helium-3, space suits, `*`, dead terran, …). Size/mass omitted in the catalog for cash.

### Level 0

**unit of carbon [carbon]**  
In solid form or combined with hydrogen for organic basics, the carbon is used as a good source of both fuel and food enrichments.  
Size 5, mass 5. Produced by hydrocarbons drilling `[hcdril]` (1). Coal plants burn it for energy (module, not a tech consume).

**unit of copper [copper]**  
This very useful metal is the basis of most energy based or energy intensive structures.  
Size 5, mass 8. Produced by copper mining `[cminng]` (2). Consumed by L0: breathing-gas generation `[airgen]` (1), corporate management `[corpmg]` (2), uranium fission `[urfiss]` (5). Also L1: laser optics `[lasopt]` (6), laser turret `[lstrrt]` (4), preventive servicing `[servic]` (1), staged hydrolox `[hydstg]` (2).

**unit of food [food]**  
An carefully designed set of pastes, liquids and solids, lending itself to taste-satisfying preparations, yet a source of all essential minerals, vitamins and calories for human consumption.  
Size 1, mass 1. Produced by intensive farming `[farmng]` (5), fishery harvest `[fshhrv]` (5). Also L1: advanced farming `[afrmng]` (8), hydroponics `[hydrop]` (3). Cities/vehicles consume food as upkeep, not via these techs.

**unit of iron [iron]**  
Extracted, refined, and purified into industrial steels, iron is a basic construction material widely used in most structures.  
Size 5, mass 10. Produced by iron mining `[iminng]` (3). Consumed by most L0 builds (see technology entries). Also L1: city planning `[ctypln]` (26), mineral core drilling `[cdrill]` (25), armored combat `[armcbt]` (8), naval combat `[nvlcbt]` (8), orbital rocket launcher `[orbrkt]` (4), form infantry battalion `[frminf]` (4), rocket launcher production `[rckter]` (2), laser turret `[lstrrt]` (6), preventive servicing `[servic]` (1), engineering shop `[engshp]` (5), law enforcement `[lawenf]` (6), staged hydrolox `[hydstg]` (8).

**unit of oil [oil]**  
Black liquid carbon based used as fuel.  
Size 4, mass 5. Produced by oil dwelling `[oildwe]` (2). Oil-burning plants and naval/ground vehicles burn it as module fuel, not a tech consume.

**unit of oxyhydro [h2o2]**  
A very useful combination of two volatiles that react strongly.  
Size 1, mass 1. Produced by water distillation `[wtrdst]` (3) from 1 water. Reaction drive and hydrolox stage burn it as module fuel. Same-body surface↔orbit `MOVE` also consumes this as a launch surcharge when the body emitted environment attrs (see `player/rules.md` MOVE).

**unit of silicium [silici]**  
The silicium is a very common material in most areas, but high-grade siliciums are base components for smart systems and modules.  
Size 5, mass 3. Produced by silicium melting `[slcmlt]` (1). Consumed by L0: corporate management `[corpmg]` (5), shuttles assembly `[shtlas]` (1), space control `[spctrl]` (1), file indexing `[filidx]` (3). Also L1: laser optics `[lasopt]` (2), preventive servicing `[servic]` (1).

**unit of titanium [titani]**  
Due to its resistance to wear, titanium is a good construction material.  
Size 10, mass 10. Produced by titanium mining `[tminng]` (2). Consumed by L0: action and reaction `[areact]` (10), industrial automation `[indust]` (10), stationary defense `[stnrdf]` (4), shuttles assembly `[shtlas]` (1), space ship assembly `[ssassm]` (4), space control `[spctrl]` (4), uranium fission `[urfiss]` (8), small scale transportation `[strans]` (2), crew housing `[crewhs]` (2). Also L1: mineral core drilling `[cdrill]` (10), laser optics `[lasopt]` (2), laser turret `[lstrrt]` (4), preventive servicing `[servic]` (1), armored combat `[armcbt]` (2), naval combat `[nvlcbt]` (2), staged hydrolox `[hydstg]` (4).

**unit of uranium [uraniu]**  
With a half-life of million of years, this is one of the most stable of the radio-active elements, and one very easy to use in energy power modules.  
Size 1, mass 8. Produced by uranium mining `[uminng]` (1). Fission reactors burn it for energy (module, not a tech consume). Also L1: rocket launcher production `[rckter]` (1).

**unit of water [water]**  
Strategic hydrosphere resource: liquid water or melted ice. Feedstock for oxyhydro fuel (wtrdst) and hydroponic food (hydrop). Seed rich on ocean worlds; also as ice on homeworld moons.  
Size 1, mass 1. Produced by ice mining `[icemin]` (3). Consumed by water distillation `[wtrdst]` (1) and hydroponics `[hydrop]` (2).

### Level 1

**cash [cash]**  
The material token of wealth.  
No size/mass in the catalog. Consumed by city planning `[ctypln]` (500). Settlements and many modules produce or upkeep cash; that is not a technology USE.

**rocket launchers [rctlnc]**  
Infantry equipment.  
Size 100, mass 100, attack 2, damage 2. Produced by rocket launcher production `[rckter]` (1). Usable by module group infantry.

**personal laser [prllsr]**  
Compact man-portable high-frequency laser.  
Size 2, mass 3, attack 2, damage 2. Produced by personal laser `[prllsr]` (1). Usable by module group infantry.

**personal rail gun [prlgun]**  
Man-portable electromagnetic rail kinetic.  
Size 3, mass 4, attack 2, damage 3. Produced by personal rail gun `[prlgun]` (1). Usable by module group infantry.

**personal armour [psnarm]**  
Ceramic-composite body armour resisting kinetics.  
Size 4, mass 6, defense 3. Produced by personal armour `[psnarm]` (1). Usable by module group infantry.

**personal plasma shield [psnshd]**  
Wearable gas-fed plasma bottle resisting lasers.  
Size 3, mass 4, defense 3. Produced by personal plasma shield `[psnshd]` (1). Usable by module group infantry.

**personal EW pack [psnew]**  
Datalink spoof and jamming pack vs drones.  
Size 2, mass 2, defense 2, initiative 2. Produced by personal EW pack `[psnew]` (1). Usable by module group infantry.

**spare part [spare]**  
Spare part can be used to remove 10 points of damage.  
Size 2, mass 2. Produced by preventive servicing `[servic]` (10). Consumed by repair and maintenance `[repair]` (1); the working HP path is the `REPAIR` order.

**unit of gold [gold]**  
This very valuable metal usable in energy based or energy intensive structures.  
Size 5, mass 9. Produced by gold recovery `[gminng]` (1). No L0–L1 tech consume.

**unit of nickel-iron [nickfe]**  
Fe-Ni alloy from metal-rich crust and asteroids. Structural feedstock for foundries and heavy hulls. HCS mining archives specialise in its recovery.  
Size 6, mass 12. Produced by nickel-iron extraction `[nminng]` (2). No L0–L1 tech consume.

**waste product [wastes]**  
Industral wastes and radioactive decay products cause problems as they accumulate in modules.  
Size 5, mass 5, radiation 1. Consumed by waste disposal `[wastdp]` (2). Upkeep 1 cash (radiation 10% if unpaid). Fission reactors produce wastes as an energy byproduct.

Items also used at level 1 and already listed above: **copper**, **food**, **iron**, **silicium**, **titanium**, **uranium**, **water**.

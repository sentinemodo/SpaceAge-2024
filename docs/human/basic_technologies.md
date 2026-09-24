# Basic technologies

Level 0 and level 1 technologies for the open-beta campaign, then the **modules** (the units you build) and **items** (materials and gear) those technologies use. Checked September 2026.

A bracketed id such as [iron] jumps to that technology, module, or item. When two entries share an id, the link follows the name written next to it.

## How to use them

**Level 0** technologies are already known. Any unit of the right kind, in the right place, can use them. They do not fill technology capacity and do not need a copy.

**Level 1** technologies **or higher** must sit on the unit that uses them. Research can discover a copy, or another unit in the same place can copy one across. Each copy fills 1 point of technology capacity per technology level.

Using a technology still needs the right unit, the right location, and the materials listed under Needs. 

**Battle** technologies are held as copies; they do not build anything.

**Harvest** technologies pull goods from the site. They do not spend cargo, except water distillation, which spends 1 water.

| Id | Where | Produces |
|----|--------|----------|
| iron, titanium, silicium, hydrocarbons, ice mining | extraction on solid ground | see each entry |
| copper, uranium, oil | extraction, including asteroids | copper, uranium, or oil |
| water distillation | extraction | 3 oxyhydro from 1 water |
| intensive farming | farms on grassland, or an ocean world with breathable air | 5 food |
| fishery harvest | a fishery on open water with breathable air | 5 food |
| nickel-iron, gold | extraction on solid ground (level 1 copies) | 2 nickel-iron or 1 gold |

## Level 0

Already known. Use them from a matching unit. No copy required.

**action and reaction [areact]**  
The construction of reaction-based drives, using cheap reaction mass heated by large energy systems.  
Where: production. Needs: 10 iron `[iron]`, 10 titanium `[titani]`. Builds: reaction drive `[rctdrv]`. Time: 2 weeks.

**agricultural complex [agrplx]**  
The placing of farms and others complexes on a planet for farming and ranching.  
Where: production, solid-surface, terran atmosphere `[terair]`. Needs: 10 iron `[iron]`. Builds: farming complex `[farms]`. Time: 4 weeks.

**breathing-gas generation [airgen]**  
Engineering for compact recyclers that produce canned terran breathing gas for habitats, ships and vacuum work.  
Where: production. Needs: 2 iron `[iron]`, 1 copper `[copper]`. Builds: life support system `[lifsys]`. Time: 2 weeks.

**copper mining [cminng]**  
The extraction, purification and refining of copper ores from a variety of locations. Copper mining is used on planetary surfaces and asteroids.  
Where: extraction. Builds: 2 copper `[copper]`. Time: 1 week.

**corporate management [corpmg]**  
Corporate governance and centralized assets management allow increase in cost-effectiveness of the entire branch and a knowledge sharing centre. Tag: `production`.  
Where: production. Needs: 10 iron `[iron]`, 2 copper `[copper]`, 5 silicium `[silici]`. Builds: corporate headquarters `[corphq]`. Time: 6 weeks.

**crew housing [crewhs]**  
Habitations for crew members in hostile environment require careful design to avoid cohabitation problems.  
Where: production. Needs: 3 iron `[iron]`, 2 titanium `[titani]`. Builds: crew quarters `[crwqrt]`. Time: 2 weeks.

**file indexing [filidx]**  
Digital library / indexing methods that support researchers (ingest, tag, and query large document sets). Tag: `research`.  
Where: production. Needs: 1 iron `[iron]`, 3 silicium `[silici]`. Builds: computer library `[cmplib]`. Time: 2 weeks.

**fishery construction [fshng]**  
Factory-built pontoon kits: nets, holds, and photic-zone seaweed or algae lines. Photosynthetic biomass in sunlit liquid on a terair world; tow the works to sea.  
Where: production (factories; no sea-cell gate on the build). Needs: 10 iron `[iron]`. Builds: fishery `[fshfrm]`. Time: 4 weeks.

**fishery harvest [fshhrv]**  
Haul fish plus photic-zone seaweed or algae in place. Photosynthetic biomass in sunlit liquid. Requires a fishery on liquid surface under terair. Active harvest: no cargo consume.  
Where: agricultural, on fishery `[fshfrm]`, **liquid-surface**, terran atmosphere `[terair]`. Not grassland. No `planet-type="ocean"` gate — any terair sea cell qualifies on campaign maps. Builds: 5 food `[food]`. Time: 1 week.

**fossil use [fossil]**  
Use of fossil fuels allows one to produce huge and dirty plants that transform carbon-based resources into energy. Tag: `production`.  
Where: production. Needs: 100 iron `[iron]`. Builds: coal-burning plant `[cplant]`. Time: 8 weeks.

**ground transport [grndtr]**  
The most basic transportation means are large trucks, powered by oil consuming engines.  
Where: production. Needs: 2 iron `[iron]`. Builds: trucks `[trucks]`. Time: 2 weeks.

**hydrocarbons drilling [hcdril]**  
The extraction and refining of hydrocarbons, or fossil fuels, from a planetary surface.  
Where: extraction, solid-surface. Builds: 1 carbon `[carbon]`. Time: 1 week.

**ice mining [icemin]**  
Cut and melt water ice from polar caps, ice moons, and hydrated regolith. Active harvest: bulk water for distillation (`wtrdst`) and hydroponics (`hydrop`). Tag: `production`.  
Where: extraction, solid-surface. Builds: 3 water `[water]`. Time: 1 week.

**industrial automation [indust]**  
Use of automated production management to reduce the workforce requirements. Tag: `production`.  
Where: production. Needs: 15 iron `[iron]`, 10 titanium `[titani]`. Builds: factory `[factry]`. Time: 4 weeks.

**intensive farming [farmng]**  
Long experience in exploitation techniques and breeds selection makes for intensive farming. Active harvest on grassland biosphere cells.  
Where: agricultural, **ocean** planet-type with terran atmosphere `[terair]`, solid-surface. Builds: 5 food `[food]`. Time: 1 week.

**iron mining [iminng]**  
The extraction, purification and refining of iron ores from a variety of locations.  
Where: extraction, solid-surface. Builds: 3 iron `[iron]`. Time: 1 week.

**mineral surface drilling [sdrill]**  
Mineral exploitation.  
Where: production. Needs: 25 iron `[iron]`. Builds: surface drill `[sdrill]`. Time: 3 weeks.

**naval transport [nvltrs]**  
Displacement hulls for cargo on liquid surfaces. Oil engines, hulls of iron. Same logistics as trucks, at sea.  
Where: production. Needs: 2 iron `[iron]`. Builds: coastal transport `[coastr]`. Time: 2 weeks.

**oil burning [oilbrn]**  
Use of oil fuels allows one to produce smaller plants that transform oil-based resources into energy.  
Where: production. Needs: 80 iron `[iron]`. Builds: oil-burning plant `[oplant]`. Time: 10 weeks.

**oil dwelling [oildwe]**  
The extraction, purification and refining of oil from variety of locations.  
Where: extraction. Builds: 2 oil `[oil]`. Time: 1 week.

**orbital complexes assembly [orassm]**  
The putting together of frames for basic orbital complexes.  
Where: production, **orbit**. Needs: 2 iron `[iron]`. Builds: orbital complex `[orcmpx]`. Time: 5 weeks.

**population center [popcnt]**  
Creation of basic infrastructure and housing for population.  
Where: production, solid-surface, terran atmosphere `[terair]`. Needs: 100 iron `[iron]`. Builds: city `[city]`. Time: 26 weeks.

**town construction [twnbld]**  
Prefabricated civic shell: graded pads, trunk utilities, and light-frame housing for a minor market town.  
Where: production, solid-surface, terran atmosphere `[terair]`. Needs: 30 iron `[iron]`, 2 titanium `[titani]`. Builds: town `[town]`. Time: 10 weeks.

**shuttles assembly [shtlas]**  
Most ships can never land, and rely on orbital shuttles for exploration, ferrying and construction. The construction of space-based hulls, bases and any other large objects is usually done in solar space above the planet surface.  
Where: production. Needs: 2 iron `[iron]`, 1 titanium `[titani]`, 1 silicium `[silici]`. Builds: space shuttle `[shuttl]`. Time: 4 weeks.

**silicium melting [slcmlt]**  
The extraction and refining of silicates into electronic-grade silicium for hi-tech elements.  
Where: extraction, solid-surface. Builds: 1 silicium `[silici]`. Time: 2 weeks.

**small scale transportation [strans]**  
Transport over interstellar distances and storage of freight poses logistics problems.  
Where: production. Needs: 2 iron `[iron]`, 2 titanium `[titani]`. Builds: small cargo bay `[cargob]`. Time: 2 weeks.

**space control [spctrl]**  
Any self-respecting space vessel requires a command structure to navigate and direct a ship.  
Where: production. Needs: 1 iron `[iron]`, 4 titanium `[titani]`, 1 silicium `[silici]`. Builds: command bridge `[cbridg]`. Time: 3 weeks.

**space ship assembly [ssassm]**  
Construction of spacecraft hulls in orbit: modular hull sections assembled for later fit-out (quarters, bridge, cargo, and so on).  
Where: production, **orbit**. Needs: 6 iron `[iron]`, 4 titanium `[titani]`. Builds: spaceship hull `[sshull]`. Time: 4 weeks.

**stationary defense [stnrdf]**  
The basic stationary defense. Gun emplacement and fortification to protect against ground based military. Tag: `military`.  
Where: production. Needs: 8 iron `[iron]`, 4 titanium `[titani]`. Builds: gun placement `[gunplc]`. Time: 8 weeks.

**titanium mining [tminng]**  
The extraction, purification and refining of titanium ores from a variety of locations.  
Where: extraction, solid-surface. Builds: 2 titanium `[titani]`. Time: 1 week.

**uranium fission [urfiss]**  
Heavy elements can be made to fission faster using heavy shielded structures to provide lasting power.  
Where: production. Needs: 2 iron `[iron]`, 8 titanium `[titani]`, 5 copper `[copper]`. Builds: fission reactor `[fisrec]`. Time: 6 weeks.

**uranium mining [uminng]**  
The extraction, purification and refining of pechblend, the basic uranium ore from a variety of locations. Uranium mining is used on planetary surfaces and asteroids.  
Where: extraction. Builds: 1 uranium `[uraniu]`. Time: 8 weeks.

**water distillation [wtrdst]**  
Electrolysis and distillation of liquid or melted ice water into oxyhydro propellant feedstock. Strategic: without regional water (or shipped water), fuel production stalls.  
Where: extraction. Needs: 1 water `[water]`. Builds: 3 oxyhydro `[h2o2]`. Time: 1 week.

**wind turbines [wndtrb]**  
Use the power of the wind as a method of generating power.  
Where: production. Needs: 1 iron `[iron]`. Builds: wind powerplant `[wnplnt]`. Time: 2 weeks.

---

## Level 1

The unit that uses one of these must hold a copy (research or copy from a neighbour). Each copy uses 1 capacity.

**advanced farming [afrmng]**  
Advancing techniques and breeds selection allows increase of farms the output.  
Where: agricultural, on ocean worlds with terran atmosphere `[terair]`, solid-surface. Builds: 8 food `[food]`. Time: 1 week.

**armored combat [armcbt]**  
The basic mobile armor, powered by oil consuming engines. Tag: `military`.  
Where: production. Needs: 8 iron `[iron]`, 2 titanium `[titani]`. Builds: tanks `[tanks]`. Time: 10 weeks.

**branch office construction [brnofc]**  
Satellite HR and payroll offices use encrypted radio links to HQ ledgers, biometric screening booths, and local labour contracts. Not a second C-suite: no region-wide construction scheduling or upkeep optimization. Tag: `production`. **Requires:** corporate management `[corpmg]`.  
Where: production. Needs: 5 iron `[iron]`, 1 copper `[copper]`, 2 silicium `[silici]`. Builds: branch office `[brnofc]`. Time: 4 weeks.

**city planning [ctypln]**  
Planning the area of the city allows more eficient use of the area.  
Where: settlement, solid-surface, terran atmosphere `[terair]`. Needs: 500 cash `[cash]`, 26 iron `[iron]`, 1 city `[city]`. Builds: metropoly `[mtrply]`. Time: 26 weeks.

**engineering shop [engshp]**  
Construction of a compact engineering shop that patches battle and maintenance damage using spare parts. Tags: `production`, `repair`.  
Where: production. Needs: 5 iron `[iron]`. Builds: engineering shop `[engshp]`. Time: 2 weeks.

**form infantry battalion [frminf]**  
The basic infantry unit. Equipped with standard rifles. Tag: `military`.  
Where: production. Needs: 4 iron `[iron]`. Builds: infantry battalion `[inftry]`. Time: 13 weeks.

**gold recovery [gminng]**  
Trace precious-metal recovery from hydrothermal veins. Compact assay methods preserved in HCS crust manuals. Tag: `production`. **Requires:** copper mining `[cminng]`.  
Where: extraction, solid-surface. Builds: 1 gold `[gold]`. Time: 1 week.

**hydroponics [hydrop]**  
Closed-loop soilless agriculture fed by recycled water. Enables food production on ice moons and airless bases without grassland biosphere. Tag: `production`. **Requires:** agricultural complex `[agrplx]`.  
Where: agricultural. Needs: 2 water `[water]`. Builds: 3 food `[food]`. Time: 1 week.

**laser optics [lasopt]**  
The working of high-frequency lasers.  
Where: production. Needs: 2 titanium `[titani]`, 6 copper `[copper]`, 2 silicium `[silici]`. Builds: blue laser `[bltlas]`. Time: 10 weeks.

**laser turret [lstrrt]**  
A fixed laser emplacement for ground defense. Requires a power supply. Tag: `military`. **Requires:** laser optics `[lasopt]`.  
Where: production. Needs: 6 iron `[iron]`, 4 titanium `[titani]`, 4 copper `[copper]`. Builds: laser turret `[laztrt]`. Time: 10 weeks.

**law enforcement [lawenf]**  
The popular punishment for committed fellonies is to restrict ones' freedom of move. The question is to make the container durable and secure.  
Where: production. Needs: 6 iron `[iron]`. Builds: jail block `[jail]`. Time: 4 weeks.

**military tactics [miltac]**  
Elementary military tactics, which enable a higher level of combat proficiency. Tag: `military`. Initiative: 5.  
Where: command. Needs: nothing. Builds: nothing (battle tech). Time: 1 week.

**mineral core drilling [cdrill]**  
Core-body mineral exploitation. Detects subsurface **deep pockets** on regional exits when a `cdrill` technology copy is present on the observing grant; identifies ore types only when `cdrill` is on-site in the pocket region. Requires a core drill module to extract deep resources.  
Where: production. Needs: 25 iron `[iron]`, 10 titanium `[titani]`. Builds: core drill `[cdrill]`. Time: 3 weeks.

**naval combat [nvlcbt]**  
Gunboats: oil-fired displacement hulls with deck guns for liquid-surface combat. Same role as tanks, at sea. Tag: `military`.  
Where: production. Needs: 8 iron `[iron]`, 2 titanium `[titani]`. Builds: gunboat `[gunbot]`. Time: 10 weeks.

**nickel-iron extraction [nminng]**  
Fe-Ni alloy recovery from metal-rich crust and M-type rocks. HCS archive specialty — hard-rock beneficiation beyond simple iron pits. Tag: `production`. **Requires:** iron mining `[iminng]`.  
Where: extraction, solid-surface. Builds: 2 nickel-iron `[nickfe]`. Time: 1 week.

**orbital rocket launcher [orbrkt]**  
A rack of chemically boosted rockets sized to nest on a shuttle or station and fire in orbit. Built in a factory on the ground or assembled in space. Tag: `military`.  
Where: production. Needs: 4 iron `[iron]`. Builds: orbital rocket launcher `[orbrkt]`. Time: 8 weeks.

**mobile laboratory [moblib]**  
Truck-mounted FTIR, XRF sample prep, and a rugged field terminal for half-rate research away from the factory floor. Tag: `research`. **Requires:** file indexing `[filidx]`.  
Where: production. Needs: 2 iron `[iron]`, 2 silicium `[silici]`. Builds: mobile laboratory `[moblab]`. Time: 3 weeks.

**optical and IR instruments [optins]**  
Diffraction-limited telescopes, FTIR, and gold-coated contacts for survey and targeting research. Tag: `research`. **Requires:** file indexing `[filidx]`.  
Where: production. Needs: 6 silicium `[silici]`, 4 copper `[copper]`, 2 iron `[iron]`, 1 gold `[gold]`. Builds: optical lab `[optlab]`. Time: 4 weeks.

**personal armour [psnarm]**  
Ceramic-composite body armour resisting kinetics. Tag: `military`. **Requires:** stationary defense `[stnrdf]`.  
Where: production. Needs: 2 iron `[iron]`, 1 titanium `[titani]`. Builds: personal armour `[psnarm]` (item). Time: 3 weeks.

**personal laser [prllsr]**  
Compact man-portable high-frequency laser for infantry battalions. Tag: `military`. **Requires:** laser optics `[lasopt]`.  
Where: production. Needs: 2 terran breathing gas `[terair]`, 1 copper `[copper]`, 1 oxyhydro `[h2o2]`. Builds: personal laser `[prllsr]` (item). Time: 3 weeks.

**preventive servicing [servic]**  
Maintenance and repairs are best done in advance. Tag: `repair`.  
Where: production. Needs: 1 titanium `[titani]`, 1 iron `[iron]`, 1 copper `[copper]`, 1 silicium `[silici]`. Builds: 10 spare parts `[spare]`. Time: 1 week.

**rocket launcher production [rckter]**  
Manufacture of portable rocket launchers issued to infantry battalions. Tag: `military`.  
Where: production. Needs: 2 iron `[iron]`, 1 uranium `[uraniu]`. Builds: rocket launchers `[rctlnc]` (item). Time: 6 weeks.

**staged hydrolox [hydstg]**  
Staged combustion of hydrogen and oxygen derived from water or stored oxyhydro. Vacuum Isp about 450 s. Fuel water or h2o2. Tag: `propulsion`. **Requires:** action and reaction `[areact]`.  
Where: production. Needs: 8 iron `[iron]`, 4 titanium `[titani]`, 2 copper `[copper]`. Builds: hydrolox stage `[hydnoz]`. Time: 5 weeks.

**tidal power [tdlpwr]**  
Kinetic turbines in tidal streams and coastal currents. Needs a terair world with liquid surface; denser working fluid than wind. Tag: `production`. **Requires:** wind turbines `[wndtrb]`.  
Where: production. Needs: 8 iron `[iron]`, 2 copper `[copper]`. Builds: tidal powerplant `[tdlpln]`. Time: 4 weeks.

**underwater drilling [udrill]**  
Seafloor rotary drill and riser on a bottom skid. Strips crust, nodules, and soft sediment under liquid surfaces. Heavier than a surface drill. Tag: `production`. **Requires:** mineral surface drilling `[sdrill]`.  
Where: production. Needs: 35 iron `[iron]`, 15 titanium `[titani]`. Builds: underwater drill `[udrill]`. Time: 4 weeks. The drill works underwater and can hide.

**underwater transport [uwtrs]**  
Pressure-hull cargo submarine. Ballast and oil closed-cycle for liquid-surface transit. Costlier than coastal freighters; required to seat undersea colonies. Tag: `production`. **Requires:** naval transport `[nvltrs]`.  
Where: production. Needs: 6 iron `[iron]`, 4 titanium `[titani]`. Builds: underwater transport `[uwtruk]`. Time: 4 weeks. The boat works underwater.

**waste disposal [wastdp]**  
This simple yet effective technique sends wastes into a sun, preventing accumulation of radio-active or unbreakable toxic wastes.  
Where: spacecraft. Needs: 2 waste products `[wastes]`. Builds: nothing. Time: 1 week.

---

## Modules

Units you can build or spend with a level 0 or 1 technology. A module built at level 0 and later upgraded by a level 1 technology stays under level 0. Larger hulls (corvette and above) are later technologies.

### Level 0

**city [city]**  
The city with basic infrastructure, underlying a settlement.  
Group `settlement`. Built by population center `[popcnt]`. Consumed (1) by city planning `[ctypln]` to make a metropoly. Size 25000, capacity 15000, energy 10, HP 1250, tech-cap 1, population max 10000. Cannot be owned; cannot hold item stacks. Upkeep 100 food (riot 25% if unpaid). Produces 1000 cash and 10 terran per 13 weeks. Operates on solid-surface worlds with terran atmosphere `[terair]`.

**town [town]**  
A compact market town: trunk roads, a modest square, and light-frame housing under a shared utility spine.  
Group `settlement`. Built by town construction `[twnbld]`. Size 6000, capacity 4000, energy 4, HP 350, tech-cap 1, population max 750. Cannot be owned; cannot hold item stacks. Upkeep 30 food (riot 25% if unpaid). Produces 200 cash and 2 terran per 13 weeks. Operates on solid-surface worlds with terran atmosphere `[terair]`.

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
Group `military`. Built by stationary defense `[stnrdf]`. Weapon: kinetic. Size 100, mass 500, crew 2, capacity 100, HP 60, tech-cap 1, attack 2, defense 2, damage 3. Upkeep 10 cash. Operates on solid region surface. Unpaid upkeep: damage 25%.

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
Group `production`. Built by shuttles assembly `[shtlas]`. Size 200, mass 30, crew 2, capacity 125, HP 17, tech-cap 1. Upkeep 10 cash. USE in orbit at 10× efficiency, requires fuel. Space move, speed 1, mass-capacity 750. Fuel duration 13 (1 uranium, 1 oxyhydro). Operates on solid-surface, liquid-surface, orbit. Counts as a shuttle.

**spaceship hull [sshull]**  
The basic spaceship hull, it embodies the technology and experience in space travel.  
Group frigate. Built by space ship assembly `[ssassm]`. Size 5000, mass 100, capacity 4500, energy 1, HP 50, defense 10, radiation -50. Cannot hold item stacks. Upkeep 30 cash. Operates in orbit and space.

**surface drill [sdrill]**  
Efficient mining system. A surface drill allows you to strip minerals and various resources out of the surface of any solid body.  
Group `extraction`. Built by mineral surface drilling `[sdrill]`. Size 500, mass 500, crew 6, energy 5, capacity 250, HP 50. Upkeep 35 cash. Operates on solid-surface, in settlement or frigate.

**trucks [trucks]**  
Group of large and slow ground moving trucks.  
Group `vehicle`. Built by ground transport `[grndtr]`. Size 250, mass 100, crew 1, capacity 150, HP 17. Upkeep 5 cash. Consumes 4 food and 4 terran air (damage 25% if not). Ground move speed 0.5. Fuel duration 13 (1 oil). Operates on solid-surface with terran atmosphere.

**mobile laboratory [moblab]**  
Six-wheel flatbed carries FTIR, XRF, and a field terminal for half-rate research and anomaly investigation (spectral, radiometric).  
Group `research`. Built by mobile laboratory `[moblib]`. Size 220, mass 75, crew 1, capacity 50, energy 3, HP 30, tech-cap 2, research-output 1 (divisor 2), investigation-output 1. Upkeep 5 cash. Consumes 4 food and 4 terran air off-world (damage 25% if not). Ground move speed 0.5. **Fuel duration 13 (1 oil)** — same ground fuel rule as `trucks`. Operates on solid-surface with terran atmosphere.

**wind powerplant [wnplnt]**  
Small serviceless energy system, utilising power of the wind.  
Group `energy`. Built by wind turbines `[wndtrb]`. Size 10, mass 10, energy 1, HP 1. Upkeep 1 cash. Produces 4 energy / 13 weeks. Operates in settlement with terran atmosphere on **both solid-surface and liquid-surface**.

### Level 1

**blue laser [bltlas]**  
A compact high-frequency laser suitable for ground combat.  
Group `military`. Built by laser optics `[lasopt]`. Requires technology laser optics `[lasopt]`. Weapon: laser. Size 100, mass 100, crew 1, energy 5, capacity 50, HP 50, tech-cap 2, attack 7, defense 1, damage 7. Upkeep 20 cash.

**branch office [brnofc]**  
A mid-rise admin block with interview suites, a small vault, and a satellite uplink for payroll. Recruits local labour at half headquarters cadence; optional cash take is a thin franchise fee, not corporate treasury output.  
Group `command`. Built by branch office construction `[brnofc]`. Size 300, mass 300, crew 6, energy 4, capacity 150, HP 40, tech-cap 1, defense 2. Upkeep 40 cash. Produces 20 cash / 2 weeks and 1 terran / 2 weeks. No region upkeep-reduction or fast-construction effects. Operates in settlement.

**core drill [cdrill]**  
Advanced mining system. Strips core-body minerals and extracts **deep-pocket** deposits invisible to surface drills. Carry a `cdrill` technology copy in-region to read deep resource assays; pair with a mobile laboratory for field scouting.  
Group `extraction`. Built by mineral core drilling `[cdrill]`. Size 1000, mass 1000, crew 6, energy 5, capacity 750, HP 100, tech-cap 4. Upkeep 50 cash. Faster extraction on self. Operates on solid-surface, in settlement or frigate.

**engineering shop [engshp]**  
A small workshop that repairs the parent module stack and nested stacks. REPAIR restores 20 hit points per week and consumes 1 spare part, or 1 hit point with no parts.  
Group `production`. Built by engineering shop `[engshp]`. Size 25, mass 20, crew 2, energy 1, capacity 10, HP 15, tech-cap 1. Upkeep 20 cash. Operates in settlement, frigate, spacecraft, space station.

**gunboat [gunbot]**  
Oil-fired gunboat for liquid-surface combat. Deck guns against hulls and coastal modules.  
Group `vehicle`. Built by naval combat `[nvlcbt]`. Weapon: kinetic. Size 240, mass 240, crew 12, capacity 200, HP 70, attack 5, defense 3, damage 6. Upkeep 80 cash (rebel 10% if unpaid). Consumes 12 food and 12 terran air (damage 25% if not). Naval move speed 1. Fuel duration 13 (3 oil). Operates on **solid-surface and liquid-surface** with terran atmosphere `[terair]` (port + sea).

**hydrolox stage [hydnoz]**  
Staged hydrolox rocket; fuel water or oxyhydro. Better mass-capacity than a reaction drive, still chemical: not an AU torch.  
Group `propulsion`. Built by staged hydrolox `[hydstg]`. Size 800, mass 900, crew 1, energy 40, HP 45. Upkeep 25 cash. Space move, speed 0.5, mass-capacity 20000. Fuel duration 13 (2 oxyhydro). Operates in frigate.

**infantry battalion [inftry]**  
Infantry battalion used for claiming cities, and taking over hostile modules.  
Group `infantry`. Built by form infantry battalion `[frminf]`. Weapon: kinetic. Size 500, mass 500, capacity 300, HP 50, attack 2, defense 2, damage 2, can-convert, value 50. Upkeep 15 cash. Consumes 100 food and 100 terran air (damage 25% if not). Unpaid upkeep: rebel 10%. Ground move speed 0.1. Operates on solid surface with terran atmosphere.

**jail block [jail]**  
Secure and durable - cells you want to be when the criminals and prisoners are put there for containment.  
Group `habitat`. Built by law enforcement `[lawenf]`. Size 500, mass 800, crew 5, energy 2, capacity 250, HP 65, tech-cap 1, habitat 40. Upkeep 35 cash. Effect: immobile on stacked.

**laser turret [laztrt]**  
A fixed laser emplacement for ground defense. Requires a power supply.  
Group `military`. Built by laser turret `[lstrrt]`. Requires technology laser optics `[lasopt]`. Weapon: laser. Size 100, mass 500, crew 2, energy 5, capacity 100, HP 80, tech-cap 1, attack 7, defense 1, damage 7. Upkeep 25 cash. Operates on solid region surface.

**metropoly [mtrply]**  
Well designed city with advanced infrastructure, that allows building tall buildings and effective public transport.  
Group `settlement`. Built by city planning `[ctypln]` (consumes 1 city). Size 25000, capacity 20000, energy 20, HP 1250, tech-cap 2, population max 15000. Cannot be owned; cannot hold item stacks. Upkeep 150 food (riot 25% if unpaid). Produces 1500 cash and 15 terran per 13 weeks. Operates on solid-surface worlds with terran atmosphere.

**orbital rocket launcher [orbrkt]**  
A rack of chemically boosted rockets that only fires in orbit. Compact enough to nest in a shuttle with crew, fuel, food, and breathing mix.  
Group `military`. Built by orbital rocket launcher `[orbrkt]`. Weapon: missile. Size 40, mass 40, crew 0, capacity 0, HP 40, tech-cap 1, attack 3, defense 1, damage 3. Upkeep 10 cash. Operates only in orbit. Not a shuttle unit: stays nested on the shuttle, fires on the parent’s shot sequence, and is included in the standard hit-location roll.

**tanks [tanks]**  
Platoon of 4 armored vehicles suitable for destroying ground modules and infantry battalions.  
Group `vehicle`. Built by armored combat `[armcbt]`. Weapon: kinetic. Size 240, mass 240, crew 16, capacity 200, HP 80, attack 6, defense 4, damage 7. Upkeep 80 cash. Consumes 16 food and 16 terran air (damage 25% if not). Unpaid upkeep: rebel 10%. Ground move speed 0.5. Fuel duration 13 (4 oil). Operates on solid surface with terran atmosphere.

**tidal powerplant [tdlpln]**  
Kinetic turbines in tidal streams and coastal currents. Denser working fluid than wind; needs terair and liquid surface.  
Group `energy`. Built by tidal power `[tdlpwr]`. Size 80, mass 80, crew 1, energy 2, HP 8. Upkeep 15 cash. Produces 12 energy / 13 weeks. Operates in settlement on **liquid-surface** with terran atmosphere `[terair]`.

**underwater drill [udrill]**  
Seafloor rotary drill and riser on a bottom skid. Strips crust, nodules, and soft sediment under liquid surfaces.  
Group `extraction`. Built by underwater drilling `[udrill]`. Size 750, mass 750, crew 8, energy 8, capacity 400, HP 60. Underwater. Upkeep 60 cash. Operates on liquid-surface, in settlement or frigate. Hidden unless the watcher is underwater nearby or in a ship overhead.

**underwater transport [uwtruk]**  
Pressure-hull cargo submarine. Ballast tanks and oil closed-cycle. Liquid surface only; costlier than coastal freighters.  
Group `vehicle`. Built by underwater transport `[uwtrs]`. Size 350, mass 200, crew 2, capacity 120, HP 25. Underwater. Upkeep 25 cash. Consumes 8 food and 8 terran air (damage 25% if not). Naval move speed 1. Fuel duration 13 (2 oil). Operates on **liquid-surface** with terran atmosphere `[terair]` (no land port). Counts as underwater presence for stealth and seating under-surface cities / liquid-surface domes.

---

## Items

Materials and gear that a level 0 or 1 technology produces or spends. Cash has no size or mass.

### Level 0

**unit of carbon [carbon]**  
In solid form or combined with hydrogen for organic basics, the carbon is used as a good source of both fuel and food enrichments.  
Size 5, mass 5. Produced by hydrocarbons drilling `[hcdril]` (1). Coal plants burn it for energy (module, not a tech consume).

**unit of copper [copper]**  
This very useful metal is the basis of most energy based or energy intensive structures.  
Size 5, mass 8. Produced by copper mining `[cminng]` (2). Consumed by L0: breathing-gas generation `[airgen]` (1), corporate management `[corpmg]` (2), uranium fission `[urfiss]` (5). Also L1: branch office construction `[brnofc]` (1), laser optics `[lasopt]` (6), laser turret `[lstrrt]` (4), preventive servicing `[servic]` (1), staged hydrolox `[hydstg]` (2), tidal power `[tdlpwr]` (2).

**unit of food [food]**  
An carefully designed set of pastes, liquids and solids, lending itself to taste-satisfying preparations, yet a source of all essential minerals, vitamins and calories for human consumption.  
Size 1, mass 1. Produced by intensive farming `[farmng]` (5), fishery harvest `[fshhrv]` (5). Also L1: advanced farming `[afrmng]` (8), hydroponics `[hydrop]` (3). Cities/vehicles consume food as upkeep, not via these techs.

**unit of iron [iron]**  
Extracted, refined, and purified into industrial steels, iron is a basic construction material widely used in most structures.  
Size 5, mass 10. Produced by iron mining `[iminng]` (3). Consumed by most L0 builds (see technology entries). Also L1: branch office construction `[brnofc]` (5), city planning `[ctypln]` (26), mineral core drilling `[cdrill]` (25), armored combat `[armcbt]` (8), naval combat `[nvlcbt]` (8), orbital rocket launcher `[orbrkt]` (4), form infantry battalion `[frminf]` (4), rocket launcher production `[rckter]` (2), laser turret `[lstrrt]` (6), preventive servicing `[servic]` (1), engineering shop `[engshp]` (5), law enforcement `[lawenf]` (6), staged hydrolox `[hydstg]` (8), tidal power `[tdlpwr]` (8), underwater drilling `[udrill]` (35), underwater transport `[uwtrs]` (6).

**unit of oil [oil]**  
Black liquid carbon based used as fuel.  
Size 4, mass 5. Produced by oil dwelling `[oildwe]` (2). Oil-burning plants and naval/ground vehicles burn it as module fuel, not a tech consume.

**unit of oxyhydro [h2o2]**  
A very useful combination of two volatiles that react strongly.  
Size 1, mass 1. Produced by water distillation `[wtrdst]` (3) from 1 water. Reaction drive and hydrolox stage burn it as module fuel. Moving between a world and its orbit also spends oxyhydro.

**unit of silicium [silici]**  
The silicium is a very common material in most areas, but high-grade siliciums are base components for smart systems and modules.  
Size 5, mass 3. Produced by silicium melting `[slcmlt]` (1). Consumed by L0: corporate management `[corpmg]` (5), shuttles assembly `[shtlas]` (1), space control `[spctrl]` (1), file indexing `[filidx]` (3). Also L1: branch office construction `[brnofc]` (2), laser optics `[lasopt]` (2), preventive servicing `[servic]` (1).

**unit of titanium [titani]**  
Due to its resistance to wear, titanium is a good construction material.  
Size 10, mass 10. Produced by titanium mining `[tminng]` (2). Consumed by L0: action and reaction `[areact]` (10), industrial automation `[indust]` (10), stationary defense `[stnrdf]` (4), shuttles assembly `[shtlas]` (1), space ship assembly `[ssassm]` (4), space control `[spctrl]` (4), uranium fission `[urfiss]` (8), small scale transportation `[strans]` (2), crew housing `[crewhs]` (2). Also L1: mineral core drilling `[cdrill]` (10), laser optics `[lasopt]` (2), laser turret `[lstrrt]` (4), preventive servicing `[servic]` (1), armored combat `[armcbt]` (2), naval combat `[nvlcbt]` (2), staged hydrolox `[hydstg]` (4), underwater drilling `[udrill]` (15), underwater transport `[uwtrs]` (4).

**unit of uranium [uraniu]**  
With a half-life of million of years, this is one of the most stable of the radio-active elements, and one very easy to use in energy power modules.  
Size 1, mass 8. Produced by uranium mining `[uminng]` (1). Fission reactors burn it for energy (module, not a tech consume). Also L1: rocket launcher production `[rckter]` (1).

**unit of water [water]**  
Strategic hydrosphere resource: liquid water or melted ice. Feedstock for oxyhydro fuel (wtrdst) and hydroponic food (hydrop). Seed rich on ocean worlds; also as ice on homeworld moons.  
Size 1, mass 1. Produced by ice mining `[icemin]` (3). Consumed by water distillation `[wtrdst]` (1) and hydroponics `[hydrop]` (2).

### Level 1

**cash [cash]**  
The material token of wealth.  
Consumed by city planning `[ctypln]` (500). Settlements and many modules produce or upkeep cash; that is not a technology USE.

**rocket launchers [rctlnc]**  
Infantry equipment.  
Size 100, mass 100, attack 2, damage 2. Produced by rocket launcher production `[rckter]` (1). Usable by module group infantry.

**personal laser [prllsr]**  
Compact man-portable high-frequency laser.  
Size 2, mass 3, attack 2, damage 2. Produced by personal laser `[prllsr]` (1). Usable by module group infantry.

**personal armour [psnarm]**  
Ceramic-composite body armour resisting kinetics.  
Size 4, mass 6, defense 3. Produced by personal armour `[psnarm]` (1). Usable by module group infantry.

**personal EW pack [psnew]**  
Datalink spoof and jamming pack vs drones.  
Size 2, mass 2, defense 2, initiative 2. Produced by personal EW pack `[psnew]` (1). Usable by module group infantry.

**spare part [spare]**  
Spare part can be used to remove 10 points of damage.  
Size 2, mass 2. Produced by preventive servicing `[servic]` (10). Spent by the `REPAIR` order.

**unit of gold [gold]**  
This very valuable metal usable in energy based or energy intensive structures.  
Size 5, mass 9. Produced by gold recovery `[gminng]` (1). No L0–L1 tech consume.

**unit of nickel-iron [nickfe]**  
Fe-Ni alloy from metal-rich crust and asteroids. Structural feedstock for foundries and heavy hulls. HCS mining archives specialise in its recovery.  
Size 6, mass 12. Produced by nickel-iron extraction `[nminng]` (2). No L0–L1 tech consume.

**waste product [wastes]**  
Industral wastes and radioactive decay products cause problems as they accumulate in modules.  
Size 5, mass 5, radiation 1. Consumed by waste disposal `[wastdp]` (2). Upkeep 1 cash (radiation 10% if unpaid). Fission reactors produce wastes as an energy byproduct.

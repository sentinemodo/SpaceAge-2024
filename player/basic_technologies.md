# Level 0 and 1 technologies

Catalog: `Tests/data.xml`. Checked **19 Aug 2026**.

This file lists **level 0 and level 1** technologies, then the **module types** and **item types** those technologies produce or consume. Level 2 and above: `player/advanced_technologies.md`. Alphabetical by English `name-en` inside each level.

Omitted `use-time` defaults to **1** week in `DataFile`. Omitted consume/produce `quantity` defaults to **1**.

## Levels

**Level 0** technologies are always known and present. Any stack whose module group and location match `usable-in` can `USE` them. They do not occupy technology capacity, are not research breakthroughs, and do not need `COPY`.

**Level 1 and above** must exist as a **local copy** on the using stack (`UseOrder.HasTechnology`: `Producer.Technologies.Contains`). Get a copy by research (labs roll level 1 … faction max+1 into remaining capacity) or by `COPY <id> TO <stack>` from a same-location holder that already has it. Each copy uses `level` points of the stack’s technology capacity.

`USE` still needs matching module group, location, and consume items. Battle-only techs (no produce) are held as copies, not used as builds.

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

**fossil use [fossil]**  
Use of fossil fuels allows one to produce huge and dirty plants that transform carbon-based resources into energy. Tag: `production`.  
Works in: production. Use consumes: 100 iron `[iron]`. Use produces: coal-burning plant `[cplant]`. Use-time: 8 weeks.

**ground transport [grndtr]**  
The most basic transportation means are large trucks, powered by oil consuming engines.  
Works in: production. Use consumes: 2 iron `[iron]`. Use produces: trucks `[trucks]`. Use-time: 2 weeks.

**hydrocarbons drilling [hcdril]**  
The extraction and refining of hydrocarbons, or fossil fuels, from a planetary surface.  
Works in: extraction, solid-surface. Use produces: 1 carbon `[carbon]`. Use-time: 1 week (default).

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
Most ships can never land, and rely on orbital shuttles for exploration, ferrying and construction.  
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
Works in: production. Use consumes: 2 iron `[iron]`, 2 titanium `[titani]`. Use produces: gun placement `[gunplc]`. Use-time: 4 weeks.

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
The extraction of salts and minerals from water, making it suitable for industrial uses.  
Works in: extraction. Use produces: 3 oxyhydro `[h2o2]`. Use-time: 1 week (default).

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
Works in: production. Use consumes: 4 iron `[iron]`. Use produces: tanks `[tanks]`. Use-time: 4 weeks.

**city planning [ctypln]**  
Planning the area of the city allows more eficient use of the area.  
Works in: settlement, solid-surface, terran atmosphere `[terair]`. Use consumes: 500 cash `[cash]`, 26 iron `[iron]`, 1 city `[city]`. Use produces: metropoly `[mtrply]`. Use-time: 26 weeks.

**engineering shop [engshp]**  
Construction of a compact engineering shop that patches battle and maintenance damage using spare parts. Tags: `production`, `repair`. Research cost override: 4.  
Works in: production. Use consumes: 5 iron `[iron]`. Use produces: engineering shop `[engshp]`. Use-time: 2 weeks.

**form infantry battalion [frminf]**  
The basic infantry unit. Equipped with standard rifles. Tag: `military`.  
Works in: production. Use consumes: 1 iron `[iron]`. Use produces: infantry battalion `[inftry]`. Use-time: 13 weeks.

**laser optics [lasopt]**  
The working of high-frequency lasers.  
Works in: production. Use consumes: 1 titanium `[titani]`, 4 copper `[copper]`, 1 silicium `[silici]`. Use produces: blue laser `[bltlas]`. Use-time: 4 weeks.

**laser turret [lstrrt]**  
A fixed laser emplacement for ground defense. Requires a power supply. Tag: `military`. **Requires:** laser optics `[lasopt]`.  
Works in: production. Use consumes: 2 iron `[iron]`, 2 titanium `[titani]`, 2 copper `[copper]`. Use produces: laser turret `[laztrt]`. Use-time: 4 weeks.

**law enforcement [lawenf]**  
The popular punishment for committed fellonies is to restrict ones' freedom of move. The question is to make the container durable and secure.  
Works in: production. Use consumes: 6 iron `[iron]`. Use produces: jail block `[jail]`. Use-time: 4 weeks.

**military tactics [miltac]**  
Elementary military tactics, which enable a higher level of combat proficiency. Tag: `military`. Initiative: 5.  
Works in: command. Use consumes: nothing. Use produces: nothing (battle tech). Use-time: 1 week (default).

**mineral core drilling [cdrill]**  
Mineral exploitation.  
Works in: production. Use consumes: 25 iron `[iron]`, 10 titanium `[titani]`. Use produces: core drill `[cdrill]`. Use-time: 3 weeks.

**preventive servicing [servic]**  
Maintenance and repairs are best done in advance. Tag: `repair`.  
Works in: production. Use consumes: 1 titanium `[titani]`, 1 iron `[iron]`, 1 copper `[copper]`, 1 silicium `[silici]`. Use produces: 10 spare parts `[spare]`. Use-time: 1 week (default).

**repair and maintenance [repair]**  
Repair and maintenance allows you to recover from ship damage, either from military actions or neglect.  
Works in: production. Use consumes: 1 spare part `[spare]`. Use produces: effect `repair` on `module-damage` (`change="-1"`). Use-time: 2 weeks.  
`USE` of effect-producing techs throws “Not implemented”; issue **`REPAIR`** instead (see `player/rules.md`).

**rocket launcher production [rckter]**  
Manufacture of portable rocket launchers issued to infantry battalions. Tag: `military`. Research cost override: 4.  
Works in: production. Use consumes: 1 iron `[iron]`. Use produces: rocket launchers `[rctlnc]` (item). Use-time: 2 weeks.

**waste disposal [wastdp]**  
This simple yet effective technique sends wastes into a sun, preventing accumulation of radio-active or unbreakable toxic wastes.  
Works in: spacecraft. Use consumes: 2 waste products `[wastes]`. Use produces: nothing. Use-time: 1 week.

---

## Module types

Catalog `<module><entry>` that a level 0 or 1 technology **produces** or **consumes**. Not every module in `data.xml`. A type produced at level 0 and consumed at level 1 is listed under level 0.

### Level 0

**city [city]**  
The city with basic infrastructure, underlying a settlement.  
Group `settlement`. Built by population center `[popcnt]`. Consumed (1) by city planning `[ctypln]` to make a metropoly. Size 25000, capacity 15000, energy 10, HP 1250, tech-cap 1, population max 10000. Cannot be owned; cannot hold item stacks. Upkeep 100 food (riot 25% if unpaid). Produces 1000 cash and 10 terran per 13 weeks. Operates on solid-surface worlds with terran atmosphere `[terair]`.

**coal-burning plant [cplant]**  
Provide energy by burining carbon.  
Group `energy`. Built by fossil use `[fossil]`. Size 1000, mass 1000, crew 2, energy 10, capacity 500, HP 100. Upkeep 50 cash. Produces 40 energy / 13 weeks; burns 5 carbon. Fuel duration 13. Operates in settlement, solid-surface, terran atmosphere.

**command bridge [cbridg]**  
A command bridge is mandated for the control of any spaceship.  
Group `command`. Built by space control `[spctrl]`. Size 800, mass 300, crew 1, energy 5, capacity 200, HP 55, tech-cap 1. Upkeep 100 cash. Operates in frigate, space station.

**computer library [cmplib]**  
The efficiency of the computer library allows on to store and manipulate larger than usual files and technological reference documents.  
Group `research`. Built by file indexing `[filidx]`. Size 200, mass 50, energy 5, HP 50, tech-cap 4. Upkeep 100 cash (catalog attribute `quanity`).

**corporate headquarters [corphq]**  
A corporate headquarters allow centralized control of the corporation.  
Group `command`. Built by corporate management `[corpmg]`. Size 1000, mass 1000, crew 20, energy 10, capacity 750, HP 100, tech-cap 2. Upkeep 100 cash. Produces 100 cash / week. Effects: 0.1 upkeep reduction and fast construction in the region. Operates in settlement.

**crew quarters [crwqrt]**  
These sealed and protected quarters house crew in the most hostile of the areas. They also allow slow recuperation of wounded crew members.  
Group `habitat`. Built by crew housing `[crewhs]`. Size 500, mass 400, crew 0, energy 1, capacity 250, HP 45, tech-cap 1. Upkeep 20 cash. Heal 0.1 on stacked.

**factory [factry]**  
Factories are everywhere since the dawn of Industral age, and even the Space age couldn't change the fact.  
Group `production`. Built by industrial automation `[indust]`. Size 1000, mass 750, crew 10, energy 15, capacity 500, HP 100, tech-cap 1. Upkeep 60 cash. Operates in settlement, frigate, space station.

**farming complex [farms]**  
A low-energy, low-technology food producing and harvesting complex.  
Group `agricultural`. Built by agricultural complex `[agrplx]`. Size 1000, mass 100, crew 5, energy 5, capacity 500, HP 55, tech-cap 1. Upkeep 50 cash. Operates in settlement, solid-surface, terran atmosphere.

**fission reactor [fisrec]**  
The basic nuclear power reactors, which works using heavy fissile elements to produce energy.  
Group `energy`. Built by uranium fission `[urfiss]`. Size 400, mass 140, crew 1, energy 10, capacity 100, HP 45, tech-cap 1. Upkeep 90 cash. Produces 60 energy / 13 weeks; burns 1 uranium; produces 1 waste. Fuel duration 13. Unpaid upkeep: radiation 250 at 25%.

**gun placement [gunplc]**  
The gun placements are primary means of defense for the short range ground combat.  
Group `military`. Built by stationary defense `[stnrdf]`. Size 100, mass 500, crew 2, capacity 100, HP 100, tech-cap 1. Upkeep 10 cash. Operates on solid region surface. Unpaid upkeep: damage 25%.

**life support system [lifsys]**  
Generates canned terran breathing gas for people and vehicles that cannot breathe the local atmosphere.  
Group `habitat`. Built by breathing-gas generation `[airgen]`. Size 100, mass 80, crew 0, energy 2, capacity 40, HP 20, tech-cap 1. Upkeep 10 cash. Produces 10 terran breathing gas `[terair]` / week. Operates in habitat, settlement, frigate.

**oil-burning plant [oplant]**  
Provide energy by burning oil.  
Group `energy`. Built by oil burning `[oilbrn]`. Size 800, mass 800, crew 2, energy 10, capacity 500, HP 80. Upkeep 60 cash. Produces 30 energy / 13 weeks; burns 5 oil. Fuel duration 13. Operates in settlement, solid-surface, terran atmosphere.

**orbital complex [orcmpx]**  
An elongated set of struts, cables, frames and other interconnecting parts assembling modules in orbit. The orbital complex is cheap, simple and easy to build, but lacks sophistication.  
Group `space station`. Built by orbital complexes assembly `[orassm]`. Size 5000, mass 100, capacity 4500, energy 1, HP 50, radiation -50. Cannot hold item stacks. Upkeep 10 cash. Operates in orbit.

**reaction drive [rctdrv]**  
Long experience in fluid dynamics and combustion has gone into these drives.  
Group `propulsion`. Built by action and reaction `[areact]`. Size 600, mass 700, crew 1, energy 30, capacity 150, HP 65, tech-cap 1. Upkeep 100 cash. Space move, mass-capacity 10000. Fuel duration 1. Operates in frigate.

**small cargo bay [cargob]**  
The cargo bays may hold a wide variety of cargo for bulk transportations.  
Group `storage`. Built by small scale transportation `[strans]`. Size 2000, mass 200, crew 0, capacity 1800, HP 55, tech-cap 1. Upkeep 30 cash.

**space shuttle [shuttl]**  
Basic shuttle used for orbital constructions. It has basic construction facitilites, small fission reactor and is propelled by a small reaction drive.  
Group `production`. Built by shuttles assembly `[shtlas]`. Size 200, mass 30, crew 2, capacity 125, HP 17, tech-cap 1. Upkeep 100 cash. USE in orbit at 10× efficiency, requires fuel. Space move, mass-capacity 750. Fuel duration 13. Operates on solid-surface, liquid-surface, orbit.

**spaceship hull [sshull]**  
The basic spaceship hull, it embodies the technology and experience in space travel.  
Group `frigate`. Built by space ship assembly `[ssassm]`. Size 5000, mass 100, capacity 4500, energy 1, HP 50, defense 10, radiation -50. Cannot hold item stacks. Upkeep 20 cash. Operates in orbit and space.

**surface drill [sdrill]**  
Efficient mining system. A surface drill allows you to strip minerals and various resources out of the surface of any solid body.  
Group `extraction`. Built by mineral surface drilling `[sdrill]`. Size 500, mass 500, crew 6, energy 5, capacity 250, HP 50. Upkeep 20 cash. Operates on solid-surface, in settlement or frigate.

**trucks [trucks]**  
Group of large and slow ground moving trucks.  
Group `vehicle`. Built by ground transport `[grndtr]`. Size 250, mass 100, crew 1, capacity 150, HP 17. Upkeep 10 cash. Consumes 4 food and 4 terran air (damage 25% if not). Ground move speed 0.5. Fuel duration 13. Operates on solid-surface with terran atmosphere.

**wind powerplant [wnplnt]**  
Small serviceless energy system, utilising power of the wind.  
Group `energy`. Built by wind turbines `[wndtrb]`. Size 10, mass 10, energy 1, HP 1. Upkeep 10 cash. Produces 4 energy / 13 weeks. Operates in settlement with terran atmosphere.

### Level 1

**blue laser [bltlas]**  
A compact high-frequency laser suitable for ground combat.  
Group `military`. Built by laser optics `[lasopt]`. Requires technology laser optics `[lasopt]`. Size 100, mass 100, crew 1, energy 5, capacity 50, HP 40, tech-cap 2. Upkeep 50 cash.

**core drill [cdrill]**  
Advanced mining system. A core drill allows you to strip minerals and various resources out of the core of any solid body.  
Group `extraction`. Built by mineral core drilling `[cdrill]`. Size 1000, mass 1000, crew 6, energy 5, capacity 750, HP 100. Upkeep 40 cash. Faster extraction on self. Operates on solid-surface, in settlement or frigate.

**engineering shop [engshp]**  
A small workshop that repairs the parent module stack and nested stacks. REPAIR restores 20 hit points per week and consumes 1 spare part, or 1 hit point with no parts.  
Group `production`. Built by engineering shop `[engshp]`. Size 25, mass 20, crew 2, energy 1, capacity 10, HP 15, tech-cap 1. Upkeep 5 cash. Operates in settlement, frigate, spacecraft, space station.

**infantry battalion [inftry]**  
Infantry battalion used for claiming cities, and taking over hostile modules.  
Group `infantry`. Built by form infantry battalion `[frminf]`. Size 500, mass 500, capacity 300, HP 50. Upkeep 5 cash. Consumes 100 food and 100 terran air (damage 25% if not). Unpaid upkeep: rebel 10%. Ground move speed 0.1. Operates on solid surface with terran atmosphere.

**jail block [jail]**  
Secure and durable - cells you want to be when the criminals and prisoners are put there for containment.  
Group `habitat`. Built by law enforcement `[lawenf]`. Size 500, mass 800, crew 5, energy 2, capacity 250, HP 65, tech-cap 1. Upkeep 60 cash. Effect: immobile on stacked.

**laser turret [laztrt]**  
A fixed laser emplacement for ground defense. Requires a power supply.  
Group `military`. Built by laser turret `[lstrrt]`. Requires technology laser optics `[lasopt]`. Size 100, mass 500, crew 2, energy 5, capacity 100, HP 100, tech-cap 1. Upkeep 20 cash. Operates on solid region surface.

**metropoly [mtrply]**  
Well designed city with advanced infrastructure, that allows building tall buildings and effective public transport.  
Group `settlement`. Built by city planning `[ctypln]` (consumes 1 city). Size 25000, capacity 20000, energy 20, HP 1250, tech-cap 2, population max 15000. Cannot be owned; cannot hold item stacks. Upkeep 150 food (riot 25% if unpaid). Produces 1500 cash and 15 terran per 13 weeks. Operates on solid-surface worlds with terran atmosphere.

**tanks [tanks]**  
Platoon of 4 armored vehicles suitable for destroying ground modules and infantry battalions.  
Group `vehicle`. Built by armored combat `[armcbt]`. Size 240, mass 240, crew 16, capacity 200, HP 100. Upkeep 24 cash. Consumes 16 food and 16 terran air (damage 25% if not). Unpaid upkeep: rebel 10%. Ground move speed 0.5. Fuel duration 13. Operates on solid surface with terran atmosphere.

---

## Item types

Catalog `<item><entry>` that a level 0 or 1 technology **produces** or **consumes**. Not the full item list (no gold, medicines, helium-3, space suits, `*`, dead terran, …). Size/mass omitted in the catalog for cash.

### Level 0

**unit of carbon [carbon]**  
In solid form or combined with hydrogen for organic basics, the carbon is used as a good source of both fuel and food enrichments.  
Size 5, mass 5. Produced by hydrocarbons drilling `[hcdril]` (1). Coal plants burn it for energy (module, not a tech consume).

**unit of copper [copper]**  
This very useful metal is the basis of most energy based or energy intensive structures.  
Size 5, mass 8. Produced by copper mining `[cminng]` (2). Consumed by L0: breathing-gas generation `[airgen]` (1), corporate management `[corpmg]` (2), uranium fission `[urfiss]` (5). Also L1: laser optics `[lasopt]` (4), laser turret `[lstrrt]` (2), preventive servicing `[servic]` (1).

**unit of food [food]**  
An carefully designed set of pastes, liquids and solids, lending itself to taste-satisfying preparations, yet a source of all essential minerals, vitamins and calories for human consumption.  
Size 1, mass 1. Produced by intensive farming `[farmng]` (5). Also L1: advanced farming `[afrmng]` (8). Cities/vehicles consume food as upkeep, not via these techs.

**unit of iron [iron]**  
Extracted, refined, and purified into industrial steels, iron is a basic construction material widely used in most structures.  
Size 5, mass 10. Produced by iron mining `[iminng]` (3). Consumed by most L0 builds (see technology entries). Also L1: city planning `[ctypln]` (26), mineral core drilling `[cdrill]` (25), armored combat `[armcbt]` (4), form infantry battalion `[frminf]` (1), rocket launcher production `[rckter]` (1), laser turret `[lstrrt]` (2), preventive servicing `[servic]` (1), engineering shop `[engshp]` (5), law enforcement `[lawenf]` (6).

**unit of oil [oil]**  
Black liquid carbon based used as fuel.  
Size 4, mass 5. Produced by oil dwelling `[oildwe]` (2). Oil-burning plants burn it for energy (module, not a tech consume).

**unit of oxyhydro [h2o2]**  
A very useful combination of two volatiles that react strongly.  
Size 1, mass 1. Produced by water distillation `[wtrdst]` (3).

**unit of silicium [silici]**  
The silicium is a very common material in most areas, but high-grade siliciums are base components for smart systems and modules.  
Size 5, mass 3. Produced by silicium melting `[slcmlt]` (1). Consumed by L0: corporate management `[corpmg]` (5), shuttles assembly `[shtlas]` (1), space control `[spctrl]` (1), file indexing `[filidx]` (3). Also L1: laser optics `[lasopt]` (1), preventive servicing `[servic]` (1).

**unit of titanium [titani]**  
Due to its resistance to wear, titanium is a good construction material.  
Size 10, mass 10. Produced by titanium mining `[tminng]` (2). Consumed by L0: action and reaction `[areact]` (10), industrial automation `[indust]` (10), stationary defense `[stnrdf]` (2), shuttles assembly `[shtlas]` (1), space ship assembly `[ssassm]` (4), space control `[spctrl]` (4), uranium fission `[urfiss]` (8), small scale transportation `[strans]` (2), crew housing `[crewhs]` (2). Also L1: mineral core drilling `[cdrill]` (10), laser optics `[lasopt]` (1), laser turret `[lstrrt]` (2), preventive servicing `[servic]` (1).

**unit of uranium [uraniu]**  
With a half-life of million of years, this is one of the most stable of the radio-active elements, and one very easy to use in energy power modules.  
Size 1, mass 8. Produced by uranium mining `[uminng]` (1). Fission reactors burn it for energy (module, not a tech consume).

### Level 1

**cash [cash]**  
The material token of wealth.  
No size/mass in the catalog. Consumed by city planning `[ctypln]` (500). Settlements and many modules produce or upkeep cash; that is not a technology USE.

**rocket launchers [rctlnc]**  
Infantry equipment.  
Size 100, mass 100, attack 2, damage 2. Produced by rocket launcher production `[rckter]` (1). Usable by module group infantry.

**spare part [spare]**  
Spare part can be used to remove 10 points of damage.  
Size 2, mass 2. Produced by preventive servicing `[servic]` (10). Consumed by repair and maintenance `[repair]` (1); the working HP path is the `REPAIR` order.

**waste product [wastes]**  
Industral wastes and radioactive decay products cause problems as they accumulate in modules.  
Size 5, mass 5, radiation 1. Consumed by waste disposal `[wastdp]` (2). Upkeep 1 cash (radiation 10% if unpaid). Fission reactors produce wastes as an energy byproduct.

Items also used at level 1 and already listed above: **copper**, **food**, **iron**, **silicium**, **titanium**.

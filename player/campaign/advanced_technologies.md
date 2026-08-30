# Level 2 and above technologies (campaign)

Catalog: `campaign/data.xml`, loaded by `Game/game/CatalogLoader.cs`. Checked **30 Aug 2026** against engine **0.1.148** (Phases 1–6 complete through L10). Level 0–1: `player/campaign/basic_technologies.md`.

Lists **level 2+** technologies grouped by level, alphabetical by English `name-en`. Omitted `use-time` defaults to **1** week. Omitted consume/produce `quantity` defaults to **1**.

## Levels

Same copy rule as level 1: local copy on the using stack; capacity = `level`. Catalog `requires` is a **research preference** for `RESEARCH TECHNOLOGY`, not a USE gate. **`arkcns`:** both `cruihl` and `lghull` tech copies required before USE (XML `requires`=`lghull`).

---

## Level 2

**aluminium from anorthosite [alminn]**  
Tag: `production`.  **Requires:** `slcmlt`.
Hall-Heroult analogue: dissolve anorthosite in molten cryolite and reduce aluminium on highland dust and rock moons.
Works in: module-type-group=extraction, location-type=solid-surface. Use produces: unit of aluminium `[alumin]` (item, qty 2). Use-time: 2 weeks.

**automated fabrication [autfab]**  
Tag: `production`.
An unmanned alien fabrication plant. It has no crew requirement.
Works in: module-type-group=production. Use consumes: 15 unit of iron `[iron]`, 10 unit of silicium `[silici]`. Use produces: advanced robo-factory `[robofc]`. Use-time: 6 weeks.

**corvette hull [corvhl]**  
Tag: `propulsion`.  **Requires:** `ssassm`.
Corvette pressure hull; hosts 2-4 combat stacks.
Works in: module-type-group=production. Use consumes: 20 unit of iron `[iron]`, 12 unit of titanium `[titani]`. Use produces: corvette hull `[corhul]`. Use-time: 12 weeks.

**cryogenic instrumentation [cryres]**  
Tag: `research`.  **Requires:** `optins`.
Cryogenic sensors and materials for deep-cold environments. Prerequisite for cold-rated settlement domes.
Works in: module-type-group=production. Use consumes: 8 unit of copper `[copper]`, 6 unit of titanium `[titani]`, 4 unit of silicium `[silici]`. Use produces: cryogenic lab `[crylab]`. Use-time: 5 weeks.

**electrostatic ion thrust [ionthr]**  
Tag: `propulsion`.  **Requires:** `hydstg`.
Gridded ion drive; water electrolyzed to propellant. High Isp, millinewton thrust. Station-keeping and cargo hops, not the Gate unlock.
Works in: module-type-group=production. Use consumes: 8 unit of titanium `[titani]`, 10 unit of copper `[copper]`, 6 unit of silicium `[silici]`. Use produces: ion drive `[iondrv]`. Use-time: 6 weeks.

**fusion torch drive [fustch]**  
Tag: `propulsion`.  **Requires:** `he3fus`.
Magnetic-nozzle helium-3 fusion torch. Exhaust velocity high enough for AU hops at a fraction of a g: accelerate, coast, decelerate. Burns helium-3 faster than a reaction drive burns oxyhydro. Chemical stages cannot make the Gate in a season.
Works in: module-type-group=production. Use consumes: 20 unit of titanium `[titani]`, 8 unit of silicium `[silici]`, 6 unit of copper `[copper]`, 4 unit of helium-3 `[heliu3]`. Use produces: fusion torch `[fustor]`. Use-time: 6 weeks.

**helium-3 fusion [he3fus]**  
Tag: `production`.  **Requires:** `he3min`.
Controlled fusion of helium-3 for clean, abundant energy. Unlocks the fusion torch drive.
Works in: module-type-group=production. Use consumes: unit of helium-3 `[heliu3]`. Use produces: fusion reactor `[fusrec]`. Use-time: 1 week.

**helium-3 mining [he3min]**  
Tag: `production`.  **Requires:** `uminng`.
The extraction and refining of helium-3 from regolith and gas. Helium-3 mining can be carried out by any extraction module that has this technology loaded.
Works in: module-type-group=extraction. Use produces: unit of helium-3 `[heliu3]` (item). Use-time: 8 weeks.

**kerogen retorting [krogen]**  
Tag: `production`.  **Requires:** `hcdril`.
Slow pyrolysis of carbonaceous-chondrite organics into usable kerogen feedstock.
Works in: module-type-group=extraction, location-type=solid-surface. Use produces: unit of kerogen organics `[kerogn]` (item). Use-time: 2 weeks.

**medical services [medtec]**  
Tag: `research`.
Organized triage, isolation, and first aid. Sterile volume, filtered air, and trained crew raise the odds that casualties survive long enough to reach a ward. This builds a clinic, not an inpatient surgical bay.
Works in: module-type-group=production. Use consumes: unit of iron `[iron]`, 2 unit of copper `[copper]`, 2 unit of silicium `[silici]`. Use produces: medical facility `[medfac]`. Use-time: 6 weeks.

**medicines refining [medirf]**  
Tag: `research`.  **Requires:** `medtec`.
On ocean worlds, coastal and pelagic biomass, iodine, and dissolved organics are harvested and refined into sterile doses. No cargo feedstock: the water column is the resource. Without pharmaceuticals, infection and fluid loss kill even when a surgeon is present.
Works in: module-type-group=production. Use produces: medicines `[medici]` (item). Use-time: 1 week.

**personal EW pack [psnew]**  
Tag: `military`.  **Requires:** `optins`.
Datalink spoof and jamming pack vs drones.
Works in: module-type-group=production. Use consumes: 3 unit of silicium `[silici]`, 2 unit of copper `[copper]`. Use produces: personal EW pack `[psnew]` (item). Use-time: 3 weeks.

**personal plasma shield [psnshd]**  
Tag: `military`.  **Requires:** `airgen`.
Wearable gas-fed plasma bottle resisting lasers.
Works in: module-type-group=production. Use consumes: 3 unit of terran breathing gas mixture `[terair]`, 2 unit of oxyhydro `[h2o2]`, unit of copper `[copper]`. Use produces: personal plasma shield `[psnshd]` (item). Use-time: 4 weeks.

**small habitat construction [habcns]**  
Methods for raising a root-level pressure shell sized to hold one or two support modules. It is not crew quarters.
Works in: module-type-group=production, location-type=solid-surface. Use consumes: 20 unit of iron `[iron]`, 8 unit of titanium `[titani]`. Use produces: small habitat `[smhabi]`. Use-time: 6 weeks.

**x-ray laser optics [xraylo]**  
**Requires:** `lasopt`.
Advanced optics for penetrating x-ray lasers.
Works in: module-type-group=production. Use consumes: 4 unit of titanium `[titani]`, 6 unit of copper `[copper]`, 4 unit of silicium `[silici]`. Use produces: x-ray laser `[xraylz]`. Use-time: 12 weeks.

---

## Level 3

**advanced computing [advres]**  
Tag: `research`.  **Requires:** `filidx`.
Next-generation computing enabling far larger research complexes.
Works in: module-type-group=production. Use consumes: 4 unit of iron `[iron]`, 12 unit of silicium `[silici]`. Use produces: advanced research complex `[advlib]`. Use-time: 1 week.

**advanced hull construction [ahlcns]**  
Recovered alien hull geometry. Automated internals need no crew and no life support.
Works in: module-type-group=production. Use consumes: 20 unit of titanium `[titani]`. Use produces: alien vessel hull `[alnhul]`. Use-time: 8 weeks.

**ammonia ice mining [amnext]**  
Tag: `production`.  **Requires:** `wtrdst`.
Cut and sublime outer-moon NH3 ice as a nitrogen source for air mix and later hydrazine.
Works in: module-type-group=extraction, location-type=solid-surface. Use produces: unit of ammonia ice `[ammoni]` (item, qty 2). Use-time: 2 weeks.

**automated command systems [autctl]**  
A crewless command core that can wake nested alien systems.
Works in: module-type-group=production. Use consumes: 8 unit of silicium `[silici]`. Use produces: automated command module `[autcmd]`. Use-time: 5 weeks.

**automated propulsion [autprp]**  
A crewless reaction drive sized for an alien hull.
Works in: module-type-group=production. Use consumes: 10 unit of titanium `[titani]`. Use produces: automated propulsion module `[autdrv]`. Use-time: 5 weeks.

**cold dome construction [coldom]**  
Tag: `production`.  **Requires:** `cryres`.
Insulated settlement dome rated for cryogenic surface temperatures. Required to found cities on cold moons and outer worlds.
Works in: module-type-group=production, location-type=solid-surface. Use consumes: 50 unit of iron `[iron]`, 25 unit of titanium `[titani]`, 10 unit of silicium `[silici]`. Use produces: cold dome `[clddom]`. Use-time: 12 weeks.

**dedicated helium-3 drilling [he3drl]**  
Tag: `production`.  **Requires:** `he3min`.
Purpose-built helium-3 core drills that double extraction efficiency at the cost of versatility and price.
Works in: module-type-group=production. Use consumes: 30 unit of iron `[iron]`, 10 unit of titanium `[titani]`. Use produces: helium-3 core drill `[he3ext]`. Use-time: 1 week.

**dome city construction [dmecns]**  
Construction of a compact domed settlement that can operate on airless rock if it is supplied with breathing gas and food.
Works in: module-type-group=production, location-type=solid-surface. Use consumes: 40 unit of iron `[iron]`, 20 unit of titanium `[titani]`. Use produces: small dome city `[dmdcty]`. Use-time: 10 weeks.

**drone hangar construction [drnhng]**  
Tag: `military`.
A hangar that stores and launches a squad of fighter drones.
Works in: module-type-group=production. Use consumes: 12 unit of titanium `[titani]`, 8 unit of silicium `[silici]`. Use produces: fighter drone bay `[drnbay]`. Use-time: 14 weeks.

**electronic warfare suite [ewsens]**  
Tag: `military`.  **Requires:** `optins`.
Broadband jamming suite vs drones.
Works in: module-type-group=production. Use consumes: 8 unit of silicium `[silici]`, 6 unit of copper `[copper]`. Use produces: EW antenna `[ewantn]`. Use-time: 12 weeks.

**heat-resistant settlement [hotset]**  
Tag: `production`.  **Requires:** `dmecns`.
Refractory dome and heat-rejection systems for Venus-class, volcanic, and high-CO2 hot worlds.
Works in: module-type-group=production, location-type=solid-surface. Use consumes: 50 unit of iron `[iron]`, 30 unit of titanium `[titani]`, 10 unit of copper `[copper]`. Use produces: heat dome `[hotdom]`. Use-time: 12 weeks.

**hydrazine synthesis [hydsyn]**  
Tag: `production`.  **Requires:** `amnext`.
Raschig analogue: ammonia ice to storable N2H4 hypergolic fuel.
Works in: module-type-group=production. Use consumes: 2 unit of ammonia ice `[ammoni]`. Use produces: unit of hydrazine `[hydzn]` (item, qty 2). Use-time: 4 weeks.

**methane ice mining [ch4min]**  
Tag: `production`.  **Requires:** `oildwe`.
Mine Titan-class methane ice for MPD and storable carbon-hydrogen propellant.
Works in: module-type-group=extraction, location-type=solid-surface. Use produces: unit of methane ice `[methn]` (item, qty 2). Use-time: 2 weeks.

**missile tube [mslpod]**  
Tag: `military`.  **Requires:** `ntmine`.
Nitrate-analogue solid-motor missile launcher tube.
Works in: module-type-group=production. Use consumes: 8 unit of iron `[iron]`, 4 unit of uranium `[uraniu]`. Use produces: missile tube `[msltub]`. Use-time: 14 weeks.

**nitrate evaporites [ntmine]**  
Tag: `production`.  **Requires:** `oildwe`.
Leach dry-lake oxidizer salts (NO3) from evaporite dust. Signature extraction on Shards.
Works in: module-type-group=extraction, location-type=solid-surface. Use produces: unit of nitrate salts `[nitrat]` (item, qty 2). Use-time: 2 weeks.

**nuclear-electric propulsion [nucthr]**  
Tag: `propulsion`.  **Requires:** `ionthr`.
Fission heat to Brayton cycle driving gridded ion or MPD thrusters. Reactor burns uranium; propellant is water.
Works in: module-type-group=production. Use consumes: 12 unit of titanium `[titani]`, 6 unit of uranium `[uraniu]`, 8 unit of copper `[copper]`. Use produces: nuclear-electric engine `[nepeng]`. Use-time: 8 weeks.

**proximity grenades [prxgrd]**  
Tag: `military`.  **Requires:** `rckter`.
Last-ditch isotope and frag burst vs incoming missiles. Infantry and vehicle scale.
Works in: module-type-group=production. Use consumes: 2 unit of nitrate salts `[nitrat]`, unit of uranium `[uraniu]`, unit of iron `[iron]`. Use produces: proximity grenade `[prxgrd]` (item). Use-time: 3 weeks.

**shipboard pharmacy [pharms]**  
Tag: `research`.  **Requires:** `sckcns`.
Fermentation and sterile fill inside a sick bay. Sugars and amino acids from food grow antibiotic cultures; the ward energy budget runs a still for saline and antiseptic. No ocean harvest required. The feedstock is cargo rations, the constraint is sterility.
Works in: module-type-group=habitat, **module sick bay `[sckbay]`**. Use consumes: unit of food `[food]`. Use produces: medicines `[medici]` (item). Use-time: 1 week.

**sick bay construction [sckcns]**  
Tag: `research`.  **Requires:** `medtec`.
A pressurized recovery ward: isolation beds, an autoclave, filtered air, and a surgical table. Trauma care is heat, sterility, fluids, and time. Without pharmaceuticals, two casualties still granulate over a four-week rest; with packed doses, infection drops fast enough that four patients can leave the ward each week.
Works in: module-type-group=production. Use consumes: 8 unit of iron `[iron]`, 4 unit of titanium `[titani]`, 4 unit of copper `[copper]`, 3 unit of silicium `[silici]`. Use produces: sick bay `[sckbay]`. Use-time: 8 weeks.

**sick bay [sckbay]**  
Inpatient surgical and recovery ward. Isolation beds, autoclave, filtered air. Without medicines, two casualties granulate over four unmedicated weeks per bay; with one medicines `[medici]` per conversion, up to four wounded terran per bay return to duty each week (medici-funded weeks do not tick the unmedicated clock). Remaining wounded then pay weekly medicines and, on week 13, roll death/stay/recover. The clinic `[medfac]` does not run this cadence. `USE pharms` is restricted to this module type.  
Group `habitat`. Built by sick bay construction `[sckcns]`. Size 380, mass 250, crew 3, energy 4, capacity 180, HP 32, tech-cap 2, habitat 8, radiation −120. Upkeep 50 cash (campaign catalog).

**solar sail [slsail]**  
Tag: `propulsion`.  **Requires:** `areact`.
Micron aluminium film; thrust falls as 1/r squared. Inner-system only; no reaction mass.
Works in: module-type-group=production. Use consumes: 15 unit of aluminium `[alumin]`, 4 unit of silicium `[silici]`. Use produces: solar sail `[slsmod]`. Use-time: 6 weeks.

**solar thermal plant [solth]**  
Tag: `production`.  **Requires:** `wndtrb`.
Concentrated sunlight and heat engine. No combustion; best inner-system with radiators.
Works in: module-type-group=production. Use consumes: 12 unit of silicium `[silici]`, 8 unit of aluminium `[alumin]`, 4 unit of iron `[iron]`. Use produces: solar thermal plant `[solthp]`. Use-time: 6 weeks.

**unmanned helium plant [he3unc]**  
An unmanned helium-3 fusion plant with no crew stations.
Works in: module-type-group=production. Use consumes: 12 unit of titanium `[titani]`. Use produces: automated helium powerplant `[he3aut]`. Use-time: 6 weeks.

---

## Level 4

**alien fighter construction [alnfgh]**  
Tag: `military`.
A crewless alien fighter. It needs no life support.
Works in: module-type-group=production. Use consumes: 6 unit of titanium `[titani]`, 8 unit of silicium `[silici]`, 4 unit of copper `[copper]`. Use produces: alien fighter drone `[alndrn]`. Use-time: 16 weeks.

**alkaline fuel cell [fuelcl]**  
Tag: `production`.  **Requires:** `urfiss`.
H2/O2 from water or oxyhydro; no PGM required. Quiet baseload power.
Works in: module-type-group=production. Use consumes: 10 unit of silicium `[silici]`, 8 unit of copper `[copper]`, 6 unit of titanium `[titani]`. Use produces: alkaline fuel cell `[h2cell]`. Use-time: 5 weeks.

**composite layup methods [matcmp]**  
Tag: `research`.  **Requires:** `advres`.
Fibre and matrix characterisation for advanced hull and armour layups.
Works in: module-type-group=production. Use consumes: 10 unit of silicium `[silici]`, 8 unit of carbon `[carbon]`, 4 unit of aluminium `[alumin]`. Use produces: composite lab `[cmplab]`. Use-time: 5 weeks.

**deuterium from ices [d2ext]**  
Tag: `production`.  **Requires:** `wtrdst`.
Electrolysis and cryogenic distillation of D/H from outer ice. Not used on habitable basins.
Works in: module-type-group=extraction, location-type=solid-surface. Use produces: unit of deuterium `[deutrm]` (item). Use-time: 6 weeks.

**Hall-effect thruster [hlthrs]**  
Tag: `propulsion`.  **Requires:** `ionthr`.
ExB plasma thruster; higher thrust than water-ion, still electric. Fuel xenon.
Works in: module-type-group=production. Use consumes: 8 unit of titanium `[titani]`, 12 unit of copper `[copper]`, 4 unit of silicium `[silici]`. Use produces: Hall thruster `[hlthst]`. Use-time: 6 weeks.

**hypergolic upper stage [hypstg]**  
Tag: `propulsion`.  **Requires:** `hydsyn`.
Storable N2H4-class hypergolic stage for capture burns when NTR is off.
Works in: module-type-group=production. Use consumes: 10 unit of iron `[iron]`, 6 unit of titanium `[titani]`, 4 unit of copper `[copper]`. Use produces: hypergolic engine `[hypeng]`. Use-time: 5 weeks.

**kinetic gunnery [kntcgn]**  
Tag: `military`.  **Requires:** `stnrdf`.
Electromagnetic rail kinetic weapon.
Works in: module-type-group=production. Use consumes: 20 unit of iron `[iron]`, 10 unit of titanium `[titani]`, 8 unit of copper `[copper]`. Use produces: railgun `[railgn]`. Use-time: 16 weeks.

**lithium brines [liming]**  
Tag: `production`.  **Requires:** `wtrdst`.
Recover spodumene and brine lithium for MPD cathodes and batteries. Signature extraction on Ember.
Works in: module-type-group=extraction, location-type=solid-surface. Use produces: unit of lithium ore `[lithia]` (item). Use-time: 3 weeks.

**mixed-volatile ISRU [volext]**  
Tag: `production`.  **Requires:** `krogen`.
Heat carbonaceous fines and capture mixed H2O, CO2, and N2 as ISRU feedstock.
Works in: module-type-group=extraction, location-type=solid-surface. Use produces: unit of mixed volatiles `[volatl]` (item, qty 2). Use-time: 4 weeks.

**nuclear thermal propulsion [ntrdrv]**  
Tag: `propulsion`.  **Requires:** `nucthr`.
Solid-core UO2 heats hydrogen. Isp about 800-900 s; fuel water.
Works in: module-type-group=production. Use consumes: 20 unit of titanium `[titani]`, 8 unit of uranium `[uraniu]`, 10 unit of copper `[copper]`. Use produces: nuclear thermal engine `[ntreng]`. Use-time: 6 weeks.

**orbital foundry methods [orbfnd]**  
Tag: `production`.  **Requires:** `ssassm`.
Vacuum induction melting and electron-beam welding for orbit-side fabrication.
Works in: module-type-group=production, location-type=orbit. Use consumes: 40 unit of iron `[iron]`, 20 unit of titanium `[titani]`, 15 unit of silicium `[silici]`, 8 unit of nickel-iron `[nickfe]`. Use produces: orbital foundry `[orbfry]`. Use-time: 8 weeks.

**radiation-tolerant computing [radtol]**  
Tag: `research`.  **Requires:** `optins`.
Shielded labs, ECC, and SOI for high-radiation environments.
Works in: module-type-group=production. Use consumes: 8 unit of silicium `[silici]`, 4 unit of copper `[copper]`, 2 unit of uranium `[uraniu]`. Use produces: radiation lab `[radlab]`. Use-time: 4 weeks.

**ship plasma shield [shplas]**  
Tag: `military`.  **Requires:** `lasopt`.
Gas-fed plasma bottle resisting lasers.
Works in: module-type-group=production. Use consumes: 10 unit of copper `[copper]`, 8 unit of silicium `[silici]`, 6 unit of titanium `[titani]`. Use produces: ship plasma shield `[shplas]`. Use-time: 16 weeks.

**xenon from ices [xeming]**  
Tag: `production`.  **Requires:** `volext`.
Slow desorption of adsorbed xenon from outer ice. Signature extraction on Ash. Not used on habitable basins.
Works in: module-type-group=extraction, location-type=solid-surface. Use produces: unit of xenon `[xenon]` (item). Use-time: 8 weeks.

---

## Level 5

**beryllium extraction [beming]**  
Tag: `production`.  **Requires:** `tminng`.
Bertrandite and phenakite recovery on barren dust. Signature extraction on Spare.
Works in: module-type-group=extraction, location-type=solid-surface. Use produces: unit of beryllium `[berylm]` (item). Use-time: 8 weeks.

**beryllium x-ray windows [bwinow]**  
Tag: `research`.  **Requires:** `beming`.
Soft x-ray transparent beryllium windows for fusion and NTR diagnostics.
Works in: module-type-group=production. Use consumes: 4 unit of beryllium `[berylm]`, 6 unit of silicium `[silici]`, 2 unit of copper `[copper]`. Use produces: x-ray window `[bwinow]`. Use-time: 4 weeks.

**boron extraction [bormin]**  
Tag: `production`.  **Requires:** `tminng`.
Fumarole borate recovery on vulcan moons. Signature extraction on Cinder.
Works in: module-type-group=extraction, location-type=solid-surface. Use produces: unit of boron `[boron]` (item). Use-time: 6 weeks.

**ceramic applique armour [armcml]**  
Tag: `military`.  **Requires:** `ceramp`.
B4C applique armour plates resisting kinetics.
Works in: module-type-group=production. Use consumes: 12 unit of boron `[boron]`, 8 unit of titanium `[titani]`, 6 unit of aluminium `[alumin]`. Use produces: ceramic armour plate `[cermpl]`. Use-time: 6 weeks.

**closed-loop life support [clslfe]**  
Tag: `production`.  **Requires:** `airgen`.
Sabatier, electrolysis, and amine CO2 scrubbing. Recycles wastes to breathing gas and water in a sealed habitat loop.
Works in: module-type-group=production. Use consumes: 15 unit of titanium `[titani]`, 10 unit of copper `[copper]`, 8 unit of silicium `[silici]`, 4 unit of ammonia ice `[ammoni]`. Use produces: closed-loop ECLSS `[clslss]`. Use-time: 6 weeks.

**electrothermal arcjet [ethtst]**  
Tag: `propulsion`.  **Requires:** `ntrdrv`.
Arc-heated water propellant. Between chemical and ion performance.
Works in: module-type-group=production. Use consumes: 10 unit of titanium `[titani]`, 10 unit of copper `[copper]`. Use produces: arcjet thruster `[arcjet]`. Use-time: 5 weeks.

**engineering ceramics [ceramp]**  
Tag: `production`.  **Requires:** `bormin`.
B4C and SiC kiln for ceramic armour and high-temperature hardware.
Works in: module-type-group=production. Use consumes: 8 unit of boron `[boron]`, 12 unit of silicium `[silici]`, 6 unit of aluminium `[alumin]`. Use produces: ceramic kiln `[cerkil]`. Use-time: 8 weeks.

**gravimetry and seismics [seisns]**  
Tag: `research`.  **Requires:** `survts`.
Gravimetry and passive seismics for asteroid and moon interiors.
Works in: module-type-group=production. Use consumes: 8 unit of silicium `[silici]`, 6 unit of iron `[iron]`, 2 unit of gold `[gold]`. Use produces: seismic sensor `[seissc]`. Use-time: 5 weeks.

**ISRU metal refining [isrurf]**  
Tag: `production`.  **Requires:** `he3min`.
Regolith beneficiation to metals and slag. Parallel to oxygen ISRU, not a replacement foundry.
Works in: module-type-group=production. Use consumes: 30 unit of iron `[iron]`, 15 unit of titanium `[titani]`. Use produces: ISRU refinery `[isrplt]`. Use-time: 8 weeks.

**kinetic cannon [kpdgun]**  
Tag: `military`.  **Requires:** `kntcgn`.
Rapid mass-driver turret.
Works in: module-type-group=production. Use consumes: 14 unit of iron `[iron]`, 8 unit of titanium `[titani]`. Use produces: kinetic cannon `[kpdtur]`. Use-time: 18 weeks.

**lithium MPD thruster [mpdlth]**  
Tag: `propulsion`.  **Requires:** `mpdthr`.
Lithium cathode MPD; higher Isp than methane MPD. Consumes lithia electrode feedstock.
Works in: module-type-group=production. Use consumes: 18 unit of titanium `[titani]`, 12 unit of copper `[copper]`, 8 unit of lithium ore `[lithia]`. Use produces: lithium MPD thruster `[limpd]`. Use-time: 6 weeks.

**methane MPD thruster [mpdthr]**  
Tag: `propulsion`.  **Requires:** `ntrdrv`.
JĂ—B magnetoplasmadynamic thruster. Fuel methane ice.
Works in: module-type-group=production. Use consumes: 20 unit of titanium `[titani]`, 15 unit of copper `[copper]`, 5 unit of silicium `[silici]`. Use produces: MPD thruster `[mpddrv]`. Use-time: 6 weeks.

**oxygen ISRU [o2isru]**  
Tag: `production`.  **Requires:** `krogen`.
Ilmenite and chondrite reduction to stored O2 as oxyhydro and breathing gas. Extraction group, not a metal foundry.
Works in: module-type-group=extraction, location-type=solid-surface. Use consumes: 20 unit of iron `[iron]`, 10 unit of silicium `[silici]`, 8 unit of mixed volatiles `[volatl]`. Use produces: oxygen ISRU plant `[o2plt]`. Use-time: 6 weeks.

**point-defense lasers [pdefls]**  
Tag: `military`.  **Requires:** `lstrrt`.
Short-range beam PD.
Works in: module-type-group=production. Use consumes: 8 unit of copper `[copper]`, 6 unit of silicium `[silici]`. Use produces: point-defense laser `[pdltur]`. Use-time: 18 weeks.

**survey spectrometry [survts]**  
Tag: `research`.  **Requires:** `radtol`.
Reflectance and emission spectrometers for surface survey.
Works in: module-type-group=production. Use consumes: 6 unit of silicium `[silici]`, 4 unit of copper `[copper]`. Use produces: survey spectrometer `[survsc]`. Use-time: 4 weeks.

**tungsten extraction [wminng]**  
Tag: `production`.  **Requires:** `tminng`.
Scheelite recovery on vulcan vents. Dense refractory metal for armour and kinetics.
Works in: module-type-group=extraction, location-type=solid-surface. Use produces: unit of tungsten `[tungst]` (item). Use-time: 8 weeks.

---

## Level 6

**carbon-carbon composites [cccomp]**  
Tag: `production`.  **Requires:** `grmine`.
Three-dimensional C-C layup for throats, heatshields, and high-temperature structure.
Works in: module-type-group=production. Use consumes: 12 unit of nuclear graphite `[grphit]`, 6 unit of tungsten `[tungst]`, 10 unit of carbon `[carbon]`. Use produces: carbon-carbon layup `[ccplnk]`. Use-time: 8 weeks.

**centrifugal habitat [whlhbt]**  
Tag: `production`.  **Requires:** `dmecns`.
Spin habitat providing about one g at the rim. Population to two thousand.
Works in: module-type-group=production. Use consumes: 80 unit of iron `[iron]`, 40 unit of titanium `[titani]`, 20 unit of aluminium `[alumin]`. Use produces: centrifugal habitat `[whlhul]`. Use-time: 10 weeks.

**coilgun [gausgn]**  
Tag: `military`.  **Requires:** `kntcgn`.
Dense armature coilgun.
Works in: module-type-group=production. Use consumes: 24 unit of iron `[iron]`, 12 unit of copper `[copper]`, 10 unit of titanium `[titani]`. Use produces: coilgun `[coilgn]`. Use-time: 20 weeks.

**coordinated drones [drnswm]**  
Tag: `military`.  **Requires:** `drnhng`.
Command suite coordinating alien fighter drones as a swarm weapon.
Works in: module-type-group=production. Use consumes: 10 unit of silicium `[silici]`, 6 unit of copper `[copper]`, 4 unit of rare-earth oxides `[reeox]`. Use produces: drone command suite `[drnctl]`. Use-time: 5 weeks.

**destroyer hull [desthl]**  
Tag: `propulsion`.  **Requires:** `corvhl`.
Destroyer hull; hosts 6-10 combat stacks.
Works in: module-type-group=production. Use consumes: 80 unit of iron `[iron]`, 50 unit of titanium `[titani]`, 20 unit of silicium `[silici]`. Use produces: destroyer hull `[deshul]`. Use-time: 24 weeks.

**deuterium-helium-3 fusion [dhefus]**  
Tag: `production`.  **Requires:** `he3fus`.
D-He3 fusion plant. Burns deuterium and helium-3 for high baseload power.
Works in: module-type-group=production. Use consumes: unit of deuterium `[deutrm]`, unit of platinum-group metal `[platnm]`, 8 unit of titanium `[titani]`, 8 unit of silicium `[silici]`. Use produces: D-He3 fusion plant `[dhefrc]`. Use-time: 8 weeks.

**fusion drive [fusdrv]**  
Tag: `propulsion`.  **Requires:** `fustch`.
Improved helium-3 fusion torch. Higher exhaust velocity than the L2 torch; still burn-coast-burn on AU legs.
Works in: module-type-group=production. Use consumes: 30 unit of titanium `[titani]`, 10 unit of silicium `[silici]`, 5 unit of helium-3 `[heliu3]`. Use produces: fusion engine `[fuseng]`. Use-time: 8 weeks.

**high-power xenon ion [xengid]**  
Tag: `propulsion`.  **Requires:** `hlthrs`.
Gridded xenon ion drive; higher Isp than water-ion. Parallel electric line.
Works in: module-type-group=production. Use consumes: 12 unit of titanium `[titani]`, 15 unit of copper `[copper]`, 6 unit of silicium `[silici]`. Use produces: xenon ion drive `[xendrv]`. Use-time: 7 weeks.

**magnetic sail [magsail]**  
Tag: `propulsion`.  **Requires:** `fusdrv`.
Superconducting loop against the solar wind. No onboard propellant; outer-system braking.
Works in: module-type-group=production. Use consumes: 20 unit of copper `[copper]`, 8 unit of rare-earth oxides `[reeox]`, 10 unit of titanium `[titani]`. Use produces: magnetic sail `[mgsail]`. Use-time: 8 weeks.

**magnetometry [magsns]**  
Tag: `research`.  **Requires:** `reemin`.
SQUID and fluxgate magnetometry for sails, drives, and survey.
Works in: module-type-group=production. Use consumes: 6 unit of rare-earth oxides `[reeox]`, 8 unit of silicium `[silici]`, 4 unit of copper `[copper]`. Use produces: magnetometry lab `[maglab]`. Use-time: 5 weeks.

**materials characterisation [matlib]**  
Tag: `research`.  **Requires:** `advres`.
Diffraction, hardness, and fatigue labs for hull and armour materials.
Works in: module-type-group=production. Use consumes: 12 unit of silicium `[silici]`, 6 unit of tungsten `[tungst]`. Use produces: materials lab `[matlab]`. Use-time: 6 weeks.

**missile guidance [misgde]**  
Tag: `military`.  **Requires:** `mslpod`.
Silicon seeker brains for guided missiles and cruise weapons.
Works in: module-type-group=production. Use consumes: 8 unit of silicium `[silici]`, 4 unit of copper `[copper]`. Use produces: missile guidance computer `[miscpu]`. Use-time: 4 weeks.

**neutron diagnostics [ntdiag]**  
Tag: `research`.  **Requires:** `bwinow`.
Beryllium windows and He3 tubes for fusion and NTR diagnostics.
Works in: module-type-group=production. Use consumes: 4 unit of beryllium `[berylm]`, 6 unit of silicium `[silici]`, 2 unit of uranium `[uraniu]`. Use produces: neutron diagnostic rack `[ntdiag]`. Use-time: 6 weeks.

**nuclear graphite [grmine]**  
Tag: `production`.  **Requires:** `hcdril`.
High-purity baked carbon, not peat. Signature extraction on Cinder with boron.
Works in: module-type-group=extraction, location-type=solid-surface. Use produces: unit of nuclear graphite `[grphit]` (item). Use-time: 4 weeks.

**platinum-group recovery [ptminn]**  
Tag: `production`.  **Requires:** `wminng`.
Trace PGM recovered with nickel-iron on metal asteroids and Gleam crust.
Works in: module-type-group=extraction, location-type=solid-surface. Use produces: unit of platinum-group metal `[platnm]` (item). Use-time: 10 weeks.

**proximity burst PD [proxpd]**  
Tag: `military`.  **Requires:** `mslpod`.
Last-ditch isotope bursts vs missiles.
Works in: module-type-group=production. Use consumes: 6 unit of uranium `[uraniu]`, 8 unit of iron `[iron]`. Use produces: proximity burst PD `[proxpd]`. Use-time: 18 weeks.

**rare-earth oxides [reemin]**  
Tag: `production`.  **Requires:** `nminng`.
Monazite and bastnasite recovery on metal asteroids with nickel-iron. Signature extraction on Gleam.
Works in: module-type-group=extraction, location-type=solid-surface. Use produces: unit of rare-earth oxides `[reeox]` (item). Use-time: 8 weeks.

---

## Level 7

**astrogation computers [navast]**  
Tag: `research`.  **Requires:** `advres`.
N-body integrators for long interplanetary legs. Not FTL.
Works in: module-type-group=production. Use consumes: 12 unit of silicium `[silici]`, 8 unit of copper `[copper]`, 2 unit of gold `[gold]`. Use produces: astrogation computer `[navcmp]`. Use-time: 6 weeks.

**chemical insertion stage [orbins]**  
Tag: `propulsion`.  **Requires:** `hypstg`.
Restartable hypergolic stage for orbit capture when the main drive is off.
Works in: module-type-group=production. Use consumes: 12 unit of iron `[iron]`, 8 unit of titanium `[titani]`. Use produces: insertion stage `[chmup2]`. Use-time: 6 weeks.

**cruise missile [crumis]**  
Tag: `military`.  **Requires:** `mslpod`.
Standoff guided missile launcher. Flyout fuel hydrazine; signature is nitrate and uranium.
Works in: module-type-group=production. Use consumes: 12 unit of iron `[iron]`, 8 unit of nitrate salts `[nitrat]`, 4 unit of uranium `[uraniu]`, 4 unit of hydrazine `[hydzn]`. Use produces: cruise missile launcher `[crumis]`. Use-time: 8 weeks.

**cryo-fatigue lab [fatlab]**  
Tag: `research`.  **Requires:** `matlib`.
Thermal-cycle coupons for hull and tank materials.
Works in: module-type-group=production. Use consumes: 8 unit of silicium `[silici]`, 6 unit of tungsten `[tungst]`, 4 unit of aluminium `[alumin]`. Use produces: fatigue lab `[fatlab]`. Use-time: 6 weeks.

**cryogenic bulk storage [cryost]**  
Tag: `production`.  **Requires:** `clslfe`.
LH2, LOX, LHe3, and LCH4 cryogenic tank farms.
Works in: module-type-group=production. Use consumes: 20 unit of titanium `[titani]`, 10 unit of copper `[copper]`. Use produces: cryogenic tank farm `[crytnk]`. Use-time: 5 weeks.

**cultured structural polymer [biofab]**  
Tag: `production`.  **Requires:** `krogen`.
Microbial cellulose and PHA for mass-light fittings.
Works in: module-type-group=production. Use consumes: 10 unit of kerogen organics `[kerogn]`, 8 unit of food `[food]`, 6 unit of silicium `[silici]`. Use produces: polymer plant `[bioplt]`. Use-time: 8 weeks.

**electromagnetic mass driver [msdrvr]**  
Tag: `production`.  **Requires:** `orbfnd`.
kA rails for surface-to-orbit launch.
Works in: module-type-group=production. Use consumes: 60 unit of iron `[iron]`, 30 unit of copper `[copper]`, 20 unit of titanium `[titani]`, 15 unit of nickel-iron `[nickfe]`. Use produces: mass driver station `[msdrst]`. Use-time: 10 weeks.

**exobiology protocols [exobio]**  
Tag: `research`.  **Requires:** `medtec`.
Containment and PCR protocols for alien samples.
Works in: module-type-group=production. Use consumes: 8 unit of silicium `[silici]`, 4 medicines `[medici]`. Use produces: exobiology lab `[xbiolb]`. Use-time: 6 weeks.

**metals recycling [metrec]**  
Tag: `production`.  **Requires:** `orbfnd`.
Scrap and wastes back to iron or spare parts.
Works in: module-type-group=production. Use consumes: 15 unit of iron `[iron]`, 8 unit of silicium `[silici]`, 4 unit of copper `[copper]`. Use produces: metals recycler `[recykl]`. Use-time: 6 weeks.

**plasma magnet sail [plsail]**  
Tag: `propulsion`.  **Requires:** `vasimr`.
Inflated magnetosphere against the solar wind. Larger than a magnetic sail; no onboard propellant.
Works in: module-type-group=production. Use consumes: 15 unit of copper `[copper]`, 6 unit of rare-earth oxides `[reeox]`, 8 unit of titanium `[titani]`. Use produces: plasma magnet sail `[plsail]`. Use-time: 8 weeks.

**spaced armour [armhul]**  
Tag: `military`.  **Requires:** `ahlcns`.
Whipple and tungsten fibre spaced armour resisting kinetics.
Works in: module-type-group=production. Use consumes: 40 unit of titanium `[titani]`, 10 unit of tungsten `[tungst]`, 8 unit of nickel-iron `[nickfe]`. Use produces: spaced armour plate `[armplt]`. Use-time: 6 weeks.

**ultraviolet laser [uvltur]**  
Tag: `military`.  **Requires:** `lstrrt`.
Shorter-wavelength beam weapon; xenon and volatiles as working medium.
Works in: module-type-group=production. Use consumes: 8 unit of xenon `[xenon]`, 6 unit of mixed volatiles `[volatl]`, 4 unit of terran breathing gas mixture `[terair]`, 2 unit of silicium `[silici]`. Use produces: ultraviolet laser `[uvltur]`. Use-time: 6 weeks.

**variable-Isp plasma drive [vasimr]**  
Tag: `propulsion`.  **Requires:** `fusdrv`.
RF-heated plasma thruster. Fuel water or methane ice.
Works in: module-type-group=production. Use consumes: 25 unit of titanium `[titani]`, 15 unit of copper `[copper]`, 8 unit of silicium `[silici]`. Use produces: Vasimr engine `[vasmdr]`. Use-time: 8 weeks.

---

## Level 8

**beam director [bmdir]**  
Tag: `military`.  **Requires:** `pdefls`.
Fast-steering optics for fleet lasers; gases for the beam path.
Works in: module-type-group=production. Use consumes: 10 unit of xenon `[xenon]`, 8 unit of terran breathing gas mixture `[terair]`, 4 unit of oxyhydro `[h2o2]`, 4 unit of silicium `[silici]`. Use produces: beam director `[bmdir]`. Use-time: 8 weeks.

**bolometer arrays [bolsen]**  
Tag: `research`.  **Requires:** `survts`.
Sub-millimetre thermal sensors.
Works in: module-type-group=production. Use consumes: 8 unit of silicium `[silici]`, 4 unit of gold `[gold]`, 4 unit of rare-earth oxides `[reeox]`. Use produces: bolometer array `[bolsen]`. Use-time: 5 weeks.

**capital plasma shield [capshd]**  
Tag: `military`.  **Requires:** `shplas`.
Capital-scale plasma shield; xenon and volatiles fed.
Works in: module-type-group=production. Use consumes: 12 unit of xenon `[xenon]`, 8 unit of methane ice `[methn]`, 6 unit of mixed volatiles `[volatl]`. Use produces: capital plasma shield `[capshd]`. Use-time: 8 weeks.

**closed-loop recycling [recyl2]**  
Tag: `production`.  **Requires:** `clslfe`.
Metals, water, and air from wastes in a near-closed loop.
Works in: module-type-group=production. Use consumes: 20 unit of titanium `[titani]`, 10 unit of silicium `[silici]`, 8 unit of copper `[copper]`, 4 unit of platinum-group metal `[platnm]`. Use produces: closed-loop recycler `[recylr]`. Use-time: 8 weeks.

**cruiser hull [cruihl]**  
Tag: `propulsion`.  **Requires:** `desthl`.
Cruiser hull; hosts 10-16 combat stacks.
Works in: module-type-group=production. Use consumes: 140 unit of iron `[iron]`, 90 unit of titanium `[titani]`, 30 unit of silicium `[silici]`. Use produces: cruiser hull `[cruhul]`. Use-time: 32 weeks.

**cryogenic detectors [crydet]**  
Tag: `research`.  **Requires:** `cryres`.
TES and KID detectors at kelvin temperatures.
Works in: module-type-group=production. Use consumes: 8 unit of copper `[copper]`, 4 unit of beryllium `[berylm]`, 6 unit of silicium `[silici]`. Use produces: cryogenic detector `[crydet]`. Use-time: 6 weeks.

**interplanetary tanker [isptnk]**  
Tag: `propulsion`.  **Requires:** `cryost`.
Moves cryogenic volatiles between worlds. Fuel oxyhydro.
Works in: module-type-group=production. Use consumes: 50 unit of titanium `[titani]`, 20 unit of copper `[copper]`. Use produces: interplanetary tanker `[tanker]`. Use-time: 8 weeks.

**large pressure hull [lghull]**  
Tag: `production`.  **Requires:** `whlhbt`.
Thousands of cubic metres. Production megahull path alongside the military hull ladder.
Works in: module-type-group=production. Use consumes: 120 unit of iron `[iron]`, 80 unit of titanium `[titani]`, 20 unit of silicium `[silici]`, 20 unit of nickel-iron `[nickfe]`. Use produces: large pressure hull `[lghul]`. Use-time: 12 weeks.

**long-baseline sensing [deepsc]**  
Tag: `research`.  **Requires:** `survts`.
Interferometric arrays for deep-space survey. Not FTL.
Works in: module-type-group=production. Use consumes: 15 unit of silicium `[silici]`, 10 unit of copper `[copper]`, 2 unit of gold `[gold]`. Use produces: deep sensor array `[dpsens]`. Use-time: 6 weeks.

**missile magazine [magzin]**  
Tag: `military`.  **Requires:** `mslpod`.
Nested isotope grain storage; not rail slugs.
Works in: module-type-group=production. Use consumes: 20 unit of nitrate salts `[nitrat]`, 10 unit of uranium `[uraniu]`, 8 unit of iron `[iron]`. Use produces: missile magazine `[magzin]`. Use-time: 6 weeks.

**radiation shelter construction [radshc]**  
Tag: `production`.  **Requires:** `bormin`.
Boron, beryllium, and graphite storm shelter for habitats.
Works in: module-type-group=production. Use consumes: 15 unit of boron `[boron]`, 8 unit of beryllium `[berylm]`, 10 unit of nuclear graphite `[grphit]`, 20 unit of iron `[iron]`. Use produces: radiation shelter `[radshd]`. Use-time: 8 weeks.

**RCS cluster [rcsblk]**  
Tag: `propulsion`.  **Requires:** `hypstg`.
Attitude and translation pods. Storable when the main drive is fusion.
Works in: module-type-group=production. Use consumes: 8 unit of titanium `[titani]`, 6 unit of copper `[copper]`. Use produces: RCS pod `[rcspod]`. Use-time: 4 weeks.

**silica aerogel insulation [insltc]**  
Tag: `production`.  **Requires:** `slcmlt`.
Supercritical-dried silica sol; habitat MLI analogue.
Works in: module-type-group=production. Use consumes: 12 unit of silicium `[silici]`, 4 unit of aluminium `[alumin]`. Use produces: aerogel insulation `[insltn]`. Use-time: 6 weeks.

**spinal kinetic [spngun]**  
Tag: `military`.  **Requires:** `kntcgn`.
Ship-length spinal rail.
Works in: module-type-group=production. Use consumes: 80 unit of iron `[iron]`, 20 unit of tungsten `[tungst]`, 15 unit of nickel-iron `[nickfe]`. Use produces: spinal kinetic `[spnknc]`. Use-time: 10 weeks.

**trim and station-keeping [trimth]**  
Tag: `propulsion`.  **Requires:** `isptnk`.
Low-thrust xenon or water trim after main burns.
Works in: module-type-group=production. Use consumes: 10 unit of titanium `[titani]`, 8 unit of silicium `[silici]`. Use produces: trim thruster `[trimth]`. Use-time: 5 weeks.

---

## Level 9

**aerobrake heatshield [brakch]**  
Tag: `propulsion`.  **Requires:** `hiisp`.
Ablative carbon-carbon shield for gas-giant or thick-air capture.
Works in: module-type-group=production. Use consumes: 20 unit of silicium `[silici]`, 15 unit of carbon `[carbon]`, 8 unit of nuclear graphite `[grphit]`. Use produces: aerobrake heatshield `[hshld]`. Use-time: 8 weeks.

**area electronic warfare [ewark]**  
Tag: `military`.  **Requires:** `ewsens`.
Fleet-scale jamming as defense and initiative suppression.
Works in: module-type-group=production. Use consumes: 12 unit of silicium `[silici]`, 8 unit of copper `[copper]`, 4 unit of rare-earth oxides `[reeox]`. Use produces: area EW suite `[ewark]`. Use-time: 6 weeks.

**close-in weapon system [ciwssy]**  
Tag: `military`.  **Requires:** `proxpd`.
Proximity isotope burst CIWS; pbpd only.
Works in: module-type-group=production. Use consumes: 12 unit of nitrate salts `[nitrat]`, 8 unit of uranium `[uraniu]`, 4 unit of helium-3 `[heliu3]`. Use produces: close-in weapon system `[ciwst]`. Use-time: 5 weeks.

**high-Isp fusion pulse drive [hiisp]**  
Tag: `propulsion`.  **Requires:** `vasimr`.
Pellet fusion with magnetic nozzle. Fuel helium-3 and deuterium.
Works in: module-type-group=production. Use consumes: 40 unit of titanium `[titani]`, 10 unit of helium-3 `[heliu3]`, 10 unit of tungsten `[tungst]`. Use produces: fusion pulse drive `[plsdv]`. Use-time: 10 weeks.

**inertial and optical navigation [navint]**  
Tag: `research`.  **Requires:** `navast`.
Star cameras and IMU fusion. Not FTL.
Works in: module-type-group=production. Use consumes: 10 unit of silicium `[silici]`, 8 unit of copper `[copper]`, 4 unit of rare-earth oxides `[reeox]`. Use produces: inertial navigation suite `[insnav]`. Use-time: 6 weeks.

**isolation psychology [psysup]**  
Tag: `research`.  **Requires:** `crewmd`.
Circadian lighting and isolation support; not psionics.
Works in: module-type-group=production. Use consumes: 6 unit of silicium `[silici]`, 4 medicines `[medici]`, 8 unit of food `[food]`. Use produces: isolation psychology suite `[psybrd]`. Use-time: 6 weeks.

**long-duration medicine [crewmd]**  
Tag: `research`.  **Requires:** `exobio`.
Bone loss, radiation, and isolation medicine for multi-year crews.
Works in: module-type-group=production. Use consumes: 10 unit of silicium `[silici]`, 8 medicines `[medici]`. Use produces: long-duration medical bay `[arkmed]`. Use-time: 6 weeks.

**missile mines [minelr]**  
Tag: `military`.  **Requires:** `crumis`.
Coast-then-burst mines deployed in orbit and deep space.
Works in: module-type-group=production. Use consumes: 10 unit of nitrate salts `[nitrat]`, 6 unit of uranium `[uraniu]`, 4 unit of iron `[iron]`. Use produces: missile mine `[knmine]`. Use-time: 6 weeks.

**orbital tug [orbtug]**  
Tag: `propulsion`.  **Requires:** `ionthr`.
Workhorse electric tug for station-keeping and cargo hops.
Works in: module-type-group=production. Use consumes: 25 unit of titanium `[titani]`, 15 unit of copper `[copper]`. Use produces: orbital tug `[orbtug]`. Use-time: 8 weeks.

**regenerative ECLSS [eclss2]**  
Tag: `production`.  **Requires:** `clslfe`.
Near-closed C, N, and H2O loop with platinum catalysts. Makeup ammonia or water.
Works in: module-type-group=production. Use consumes: 25 unit of titanium `[titani]`, 15 unit of silicium `[silici]`, 10 unit of copper `[copper]`, 2 unit of platinum-group metal `[platnm]`. Use produces: regenerative ECLSS `[eclssx]`. Use-time: 8 weeks.

**shipboard agriculture [agrark]**  
Tag: `production`.  **Requires:** `afrmng`.
LED hydroponics and greywater recycling. Produces food in orbit and deep space.
Works in: module-type-group=production. Use consumes: 20 unit of iron `[iron]`, 10 unit of silicium `[silici]`, 8 unit of aluminium `[alumin]`. Use produces: shipboard farm deck `[agrdek]`. Use-time: 8 weeks.

**storm-particle shelter [shldsp]**  
Tag: `production`.  **Requires:** `radshc`.
Dedicated solar-particle-event shelter for large habitats.
Works in: module-type-group=production. Use consumes: 20 unit of boron `[boron]`, 10 unit of nuclear graphite `[grphit]`, 15 unit of iron `[iron]`. Use produces: storm-particle shelter `[stmshd]`. Use-time: 8 weeks.

**waste mineralisation [wstprc]**  
Tag: `production`.  **Requires:** `clslfe`.
Oxidises wastes to slag and recoverable water.
Works in: module-type-group=production. Use consumes: 12 unit of titanium `[titani]`, 8 unit of silicium `[silici]`. Use produces: waste mineralisation plant `[wstplt]`. Use-time: 6 weeks.

---

## Level 10

**ark abort sail [arksail]**  
Tag: `propulsion`.  **Requires:** `magsail`.
Emergency magnetic sail if the pulse drive is dark. No onboard propellant.
Works in: module-type-group=production. Use consumes: 40 unit of copper `[copper]`, 15 unit of rare-earth oxides `[reeox]`, 20 unit of titanium `[titani]`. Use produces: ark abort sail `[arksail]`. Use-time: 10 weeks.

**ark command and autonomy [arkcmd]**  
Tag: `research`.  **Requires:** `autctl`.
Fleet-scale command bridge with high research throughput.
Works in: module-type-group=production. Use consumes: 20 unit of silicium `[silici]`, 15 unit of copper `[copper]`. Use produces: ark command bridge `[arkbrg]`. Use-time: 8 weeks.

**ark EW grid [arkew]**  
Tag: `military`.  **Requires:** `ewark`.
Fleet-scale electronic warfare jamming and defense.
Works in: module-type-group=production. Use consumes: 20 unit of silicium `[silici]`, 12 unit of copper `[copper]`, 8 unit of rare-earth oxides `[reeox]`. Use produces: ark EW grid `[arkew]`. Use-time: 8 weeks.

**ark laser grid [arkdef]**  
Tag: `military`.  **Requires:** `pdefls`.
Area beam grid for a km-scale target. Laser group.
Works in: module-type-group=production. Use consumes: 20 unit of xenon `[xenon]`, 15 unit of terran breathing gas mixture `[terair]`, 10 unit of mixed volatiles `[volatl]`. Use produces: ark laser grid `[arkdfn]`. Use-time: 10 weeks.

**ark magazines [arkmag]**  
Tag: `military`.  **Requires:** `magzin`.
Deep isotope grain wells for nested missile weapons.
Works in: module-type-group=production. Use consumes: 40 unit of nitrate salts `[nitrat]`, 20 unit of uranium `[uraniu]`, 10 unit of iron `[iron]`. Use produces: ark missile magazine `[arkmag]`. Use-time: 8 weeks.

**ark navigation [arknav]**  
Tag: `research`.  **Requires:** `arkcmd`.
Long-baseline optical and IMU navigation for week-scale legs.
Works in: module-type-group=production. Use consumes: 18 unit of silicium `[silici]`, 12 unit of copper `[copper]`, 6 unit of rare-earth oxides `[reeox]`. Use produces: ark navigation suite `[arknav]`. Use-time: 8 weeks.

**ark plasma shield [arkshd]**  
Tag: `military`.  **Requires:** `capshd`.
Capital-scale gas-fed plasma bottle. Not ark storm habitat shielding.
Works in: module-type-group=production. Use consumes: 20 unit of xenon `[xenon]`, 12 unit of methane ice `[methn]`, 8 unit of mixed volatiles `[volatl]`. Use produces: ark plasma shield `[arkshd]`. Use-time: 8 weeks.

**ark point-blank PD [arkpd]**  
Tag: `military`.  **Requires:** `ciwssy`.
Last-ditch isotope bursts around the hull. pbpd only.
Works in: module-type-group=production. Use consumes: 20 unit of nitrate salts `[nitrat]`, 10 unit of uranium `[uraniu]`, 6 unit of helium-3 `[heliu3]`. Use produces: ark point-blank PD `[arkpd]`. Use-time: 8 weeks.

**ark propulsion integration [arkdrv]**  
Tag: `propulsion`.  **Requires:** `hiisp`.
Integrated pulse drive for a km-class ark. Gate crossing in about twelve weeks.
Works in: module-type-group=production. Use consumes: 80 unit of titanium `[titani]`, 20 unit of helium-3 `[heliu3]`, 20 unit of copper `[copper]`. Use produces: ark engine `[arkeng]`. Use-time: 13 weeks.

**ark RCS [arkrcs]**  
Tag: `propulsion`.  **Requires:** `rcsblk`.
Attitude control for a high-inertia ark hull. Fuel hydrazine.
Works in: module-type-group=production. Use consumes: 25 unit of titanium `[titani]`, 15 unit of copper `[copper]`. Use produces: ark RCS cluster `[arkrcs]`. Use-time: 8 weeks.

**ark recycler [arkrec]**  
Tag: `production`.  **Requires:** `recyl2`.
Near-closed mass loop for thousands of crew.
Works in: module-type-group=production. Use consumes: 30 unit of titanium `[titani]`, 15 unit of silicium `[silici]`, 10 unit of platinum-group metal `[platnm]`. Use produces: ark recycler `[arkrec]`. Use-time: 10 weeks.

**ark storm shielding [arkshl]**  
Tag: `production`.  **Requires:** `radshc`.
Scaled SPE and GCR shelter for a km-class hull.
Works in: module-type-group=production. Use consumes: 30 unit of boron `[boron]`, 15 unit of beryllium `[berylm]`, 20 unit of nuclear graphite `[grphit]`, 40 unit of iron `[iron]`. Use produces: ark storm shelter `[arkshl]`. Use-time: 10 weeks.

**fleet dosimetry [dosmtr]**  
Tag: `research`.  **Requires:** `crewmd`.
Tissue-equivalent radiation detectors for long voyages.
Works in: module-type-group=production. Use consumes: 8 unit of silicium `[silici]`, 4 unit of beryllium `[berylm]`, 6 medicines `[medici]`. Use produces: dosimetry lab `[doslab]`. Use-time: 6 weeks.

**self-sufficient ark construction [arkcns]**  
Tag: `production`.  **Requires:** `lghull`.
Pressure hull for thousands of crew. Requires both production megahull (lghull) and cruiser hull ladder (cruihl) tech copies before USE.
Works in: module-type-group=production. Use consumes: 200 unit of iron `[iron]`, 120 unit of titanium `[titani]`, 40 unit of silicium `[silici]`, 20 unit of tungsten `[tungst]`, 10 unit of helium-3 `[heliu3]`, 15 unit of nickel-iron `[nickfe]`. Use produces: ark hull `[arkhul]`. Use-time: 13 weeks.

---

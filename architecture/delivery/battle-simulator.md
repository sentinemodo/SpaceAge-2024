# Battle simulator — implementation plan

Last updated: 2026-09-10  
Decisions: [ADR-0001](../adr/ADR-0001-net48-legacy-csproj.md), [ADR-0003](../adr/ADR-0003-filesystem-pbem-batch.md), [ADR-0007](../adr/ADR-0007-public-campaign-website.md)  
Engine reference: `0.1.158` (net48 / Mono)

This is the **architectural design and phased implementation plan** for the SpaceAge **Battle Simulator**.

---

## 1. Architectural Strategy & Direct Engine Invocation

### 1.1 Single Source of Truth (`Game.exe`)
The battle simulator directly executes `Game.exe` to resolve combat simulations. Maintaining two independent combat resolution codebases (e.g. duplicating `Battle.cs`, `CombatMatchup.cs`, `ModuleStack.cs`, damage weighting, shield intercepts, and evade mechanics in TypeScript) is prohibited due to the maintenance burden and unavoidable divergence as combat rules evolve.

Advanced combat mechanics (typed weapon-group multipliers, shield intercepts, armor absorption/hit-weight, hangar launch sequences, electronic warfare, and evade-leave rules) evolve directly within `Game.exe`. Every enhancement made in the C# engine immediately and automatically benefits the battle simulator without secondary porting.

### 1.2 Execution Topology & Bounded Contexts
1. **Engine Layer (`Game/Program.cs`, `Game/battle/`):**
   - Headless CLI batch capability.
   - Adds an isolated simulation switch: `Game.exe /battle-sim <input-spec.xml> [output.txt]`.
   - Loads `data.xml` configuration, parses an isolated two-side roster specification, sets an optional deterministic random seed, constructs ephemeral combatants in a virtual environment without touching or advancing persistent game state (`gamein.xml` or `gameout.*.xml`), executes the 10-round combat loop, and emits structured output.
2. **Local / Runner Bridge Layer:**
   - A lightweight execution harness invoking `mono Game.exe` (or `Game.exe` on Windows).
   - Can be invoked locally by players/GMs via CLI scripts, or via a local runner bridging the client interface to the executable without embedding an unmanaged HTTP server into `Game.exe`.
3. **Client UI Surface:**
   - Dedicated simulator interface featuring attacker and defender rosters.
   - Preset catalog dropdown of common unit templates.
   - Client-side custom unit template designer.
   - Deterministic seed controller for reproducible combat runs.
   - Detailed round-by-round combat log inspector and casualty breakdown.
   - Post-MVP: Report paste parser importing known stacks directly from player turn reports (`report.*.txt`).

---

## 2. Engine Simulator Mode Specification (`Game.exe /battle-sim`)

### 2.1 CLI Interface
Extend `Game/Program.cs` argument parsing to recognize the simulator flag:

```
Game.exe /battle-sim <simulation-file.xml> [output-path] [/data <catalog-dir>] [/seed <int>]
```

- `/battle-sim <file>`: Specifies the isolated simulation XML defining the two rosters and combat conditions.
- `output-path` (optional): File destination for simulation results. If omitted, writes to stdout or `<simulation-file>.out.txt`.
- `/data <dir>`: Catalog directory containing `data.xml` (defaults to current working directory).
- `/seed <int>`: Explicit integer seed for `Sequence.Reset(seed)` to guarantee deterministic results.

### 2.2 Simulation Input XML Schema (`sim-input.xml`)
An isolated schema avoiding campaign state dependencies while reusing standard catalog item and module IDs:

```xml
<?xml version="1.0" encoding="windows-1251"?>
<battle-sim seed="12345" location-type="orbit">
  <attackers faction="2" name="Strike Force Alpha">
    <stack id="sim_a1" type="alnhul" name="Frigate Spear" tactic="destroy">
      <items>
        <item type="terran" quantity="20" />
      </items>
      <nested>
        <stack id="sim_a1_w1" type="railgn" quantity="2" />
        <stack id="sim_a1_s1" type="shplas" quantity="1" />
        <stack id="sim_a1_c1" type="cargob" quantity="1" />
      </nested>
    </stack>
    <stack id="sim_a2" type="alndrn" name="Drone Escort" quantity="4" tactic="evade">
      <items>
        <item type="heliu3" quantity="4" />
      </items>
    </stack>
  </attackers>
  <defenders faction="1" name="Garrison Beta">
    <stack id="sim_d1" type="corhul" name="Patrol Corvette" tactic="destroy">
      <items>
        <item type="terran" quantity="12" />
      </items>
      <nested>
        <stack id="sim_d1_w1" type="pdltur" quantity="2" />
        <stack id="sim_d1_a1" type="cermpl" quantity="2" />
      </nested>
    </stack>
  </defenders>
</battle-sim>
```

### 2.3 Ephemeral Execution Harness
1. **Catalog Load:** `DataFile.LoadConfiguration()` initializes static types (`ModuleType.All`, `ItemType.All`, `Technology.All`).
2. **Virtual Scope:** Creates temporary `Game`, `Location` (e.g. dummy `Orbit` or `Region`), and mock `Faction` objects (Attacker Faction and Defender Faction, mutually declared as `FactionAttitude.Enemy`).
3. **ModuleStack Construction:** Instantiates `ModuleStack` objects with nested weapons, armor, shields, and crew/cargo items without adding them to persistent `gamein` collections.
4. **Deterministic Seeding:** Resets the RNG with the provided seed via `Sequence.Reset(seed)`.
5. **Battle Execution:** Calls `Battle battle = new Battle(attackerRoot, defenderRoot); battle.Execute(1);`.
6. **Teardown & Isolation:** Cleans up static registries via `game.ClearDictionaries()` without touching disk save files.

### 2.4 Simulation Output Format
Emits human-readable round-by-round combat logs along with a machine-readable summary block:

```text
SpaceAge Battle Simulator v0.1.158
Seed: 12345
Location: Orbit

Round 1:
------------------------------------------------------------
Attackers:
  alien vessel hull [sim_a1] Frigate Spear (destroy)
    railgun [sim_a1_w1] (active, hp: 140/140)
    ship plasma shield [sim_a1_s1] (active, hp: 120/120)
  4 alien fighter drones [sim_a2] Drone Escort (evade)
Defenders:
  corvette hull [sim_d1] Patrol Corvette (destroy)
    2 point-defense lasers [sim_d1_w1] (active, hp: 90/90)
    2 ceramic armour plates [sim_d1_a1] (active, hp: 150/150)
------------------------------------------------------------
sim_a2 fires and hits sim_d1 (chance: 12/24).
  Shield absorbs 10 damage.
sim_d1 fires 2 point-defense lasers on sim_a1 and hits sim_a1_s1 (chance: 15/22).
  sim_a1_s1 takes 16 damage (hp: 104/120).
...
============================================================
SIMULATION RESULT: ATTACKERS_WON
Rounds: 4
Casualties Attackers: 1 drone wrecked (sim_a2)
Casualties Defenders: corvette hull [sim_d1] wrecked
Captured Modules: none
============================================================
```

---

## 3. Unit Templates & User Interface Design

### 3.1 Common Unit Templates Catalog (Preset Dropdown)
The simulator provides a curated catalog of standard operational unit templates populated directly from game definitions:

| Template Name | Role | Hull / Core | Key Nested Equipment & Systems | Common Tactic |
|---|---|---|---|---|
| **System Patrol Corvette** | Light Patrol | Corvette Hull (`corhul`) | 2× Point-Defense Lasers (`pdltur`), Ceramic Armor (`cermpl`), Crew Quarters (`crwqrt`) | `destroy` |
| **Escort Frigate** | Defense / Anti-Craft | Alien Hull (`alnhul`) | 2× Railguns (`railgn`), Plasma Shield (`shplas`), Cargo Bay (`cargob`) | `destroy` |
| **Drone Carrier Frigate** | Drone Swarm | Alien Hull (`alnhul`) | Fighter Drone Bay (`drnbay`), 6× Fighter Drones (`alndrn`), Shield (`shplas`) | `evade` |
| **Line Destroyer** | Fleet Combat | Destroyer Hull (`deshul`) | Coilgun (`coilgn`), Cruise Missiles (`crumis`), CIWS (`ciwst`), Spaced Armor (`armplt`) | `destroy` |
| **Planetary Defense Battery** | Static Ground | Gun Placement (`gunplc`) | 4× Heavy Guns, Hardened Bunker, Power Plant | `destroy` |
| **Laser Emplacement** | Static Laser | Laser Turret (`laztrt`) | Fixed beam laser, dedicated power supply | `destroy` |
| **Armored Tank Platoon** | Ground Armor | Tank Platoon (`tanks`) | 4× Armored vehicles, kinetic guns, oil fuel | `destroy` |
| **Infantry Battalion** | Garrison / Capture | Infantry (`inftry`) | Terran rifles, light armor, food/air rations | `capture` |

### 3.2 Client-Only Custom Template Definition
Users can define custom unit configurations entirely client-side without modifying backend catalog files:
- **Base Selection:** Choose base hull/chassis (Frigate, Corvette, Destroyer, Tank, City Emplacement).
- **Module Sockets:** Add, configure, and remove nested modules (Weapons, Shields, Armor Plates, Sensor/EW pods, Cargo Bays).
- **Operational Stance:** Select primary combat tactic (`destroy`, `capture`, `evade`, `prioritize armed`, `prioritize command`, `prioritize storage`).
- **Logistics & Crew:** Configure crew size, fuel items (e.g. `heliu3`, `oil`, `h2o2`), and cargo gear bonuses.
- **Local Persistence:** Save custom templates in browser `localStorage` for reuse across sessions without transmitting proprietary configurations to external servers.

### 3.3 Post-MVP: Turn Report Stack Importer (`report.*.txt`)
A dedicated text ingestion module parsing player turn reports:
- Extracts enemy and friendly ships identified in scan reports or previous combat encounters.
- Reconstructs module layouts (hull type, known active modules, observed weapon mounts, armor layers).
- Populates the simulator attacker or defender slots directly with one click, enabling instant tactical "what-if" rehearsals against known hostile fleets.

---

## 4. Advanced Combat Evolution & Mechanics Synchronization

Because the simulator directly invokes `Game.exe`, all combat sub-systems remain 100% in sync with core engine development:

```
+---------------------------------------------------------------------------------+
|                                 Game.exe Core                                   |
|                                                                                 |
|   +-----------------------+   +----------------------+   +------------------+   |
|   |   CombatMatchup.cs    |   |     Battle.cs        |   |  ModuleStack.cs  |   |
|   |  - Typed Multipliers  |   |  - 10-Round Loop     |   |  - Hit Weight    |   |
|   |  - Shield Intercept   |   |  - Initiative Order  |   |  - Armor Resist  |   |
|   |  - Armor Mitigation   |   |  - Evade Leave       |   |  - Active HP     |   |
|   +-----------------------+   +----------------------+   +------------------+   |
+---------------------------------------------------------------------------------+
                                        ^
                                        | (direct execution)
                     +------------------+------------------+
                     |                                     |
           [Turn PBEM Pipeline]                  [Battle Simulator CLI]
          Game.exe (13-week turn)               Game.exe /battle-sim <spec>
```

- **Typed Matchups (`CombatMatchup.cs`):** Weapon groups (`laser`, `kinetic`, `missile`, `drone`) automatically apply exact damage multipliers against target resistances (`shield`, `armour`/`armor`, `pbpd`, `ew`).
- **Initiative Sorting:** Stacks fire strictly in ascending initiative order using root maneuverability bonuses, pilot skills, module initiative, and tech attachments.
- **Shield Intercept:** Shields absorb 90% floor damage before hull/module penetration.
- **Armor Absorption:** Armor modules absorb kinetic damage and are immune to capture.
- **Tactical Logic:** Evaluates `destroy` vs `capture` (25% HP / 75% capture pool) and `evade` retreat triggers (2 consecutive unhit rounds).

---

## 5. Phased Implementation Roadmap

### Phase 1: Engine Headless Simulator Harness (`Game.exe /battle-sim`)
- Add `/battle-sim` CLI command parsing in `Game/Program.cs`.
- Implement `BattleSimulatorRunner` class to read input XML, construct ephemeral `ModuleStack` instances, execute the `Battle` loop, and serialize round logs and outcome statistics.
- Ensure strict isolation: verify `gamein.xml` and persistent turn data are never modified.
- **Verification:** Unit tests in `Tests/TBattleSim.cs` verifying round counts, deterministic seed reproducibility, and casualty outputs.

### Phase 2: Preset Templates & XML Serializer
- Define the default template library containing standard corvettes, frigates, destroyers, drone swarms, and ground batteries.
- Implement helper utilities to serialize and deserialize `sim-input.xml`.
- **Verification:** Golden-file tests comparing simulated battles with known combat outcomes.

### Phase 3: Client Interface & Local Runner Integration
- Build two-side roster configuration UI with template dropdowns.
- Implement client-side custom template builder with module slot constraints and local storage persistence.
- Provide deterministic seed control and round-by-round log viewer with casualty summaries.
- Connect interface to `Game.exe /battle-sim` via runner bridge.
- **Verification:** Automated scenario tests verifying template selection, execution invocation, and log display.

### Phase 4: Post-MVP Report Parser
- Implement regex parser for `report.*.txt` combat and scan sections.
- Enable automatic stack extraction from battle logs and fleet reports into the simulator roster.
- **Verification:** Ingestion test suite covering varied player report excerpts.

---

## 6. Testing Strategy & Acceptance Criteria

### 6.1 Automated C# Unit & Integration Tests (`Tests/TBattleSim.cs`)
1. **CLI Execution Test:** Verify invoking `/battle-sim` with valid XML produces an exit code of 0 and expected output log format.
2. **Determinism Test:** Running the same input XML with identical seed twice produces identical roll sequences, hit allocations, and combat results.
3. **Isolation & Side-Effect Test:** Confirm running `/battle-sim` leaves no temporary save files and does not modify persistent registries or global `All` state.
4. **Mechanics Fidelity Test:** Verify laser-vs-shield, kinetic-vs-armor, and drone evasion behaviors in simulation mode match standard in-game combat results.

### 6.2 Acceptance Checklist (Done Gate)
- [ ] `Game.exe /battle-sim <spec.xml>` executes cleanly under both .NET 4.8 and Mono.
- [ ] Common unit templates dropdown populated with core vehicle and vessel types.
- [ ] Client allows defining and saving custom unit templates without server-side storage.
- [ ] Combat round log displays hit chances, damage absorption, module wrecking, and final victory/defeat conditions.
- [ ] Post-MVP report stack parser path documented and architecturally isolated.
- [ ] All existing regression tests in `Tests/` continue to pass via `.cursor/run-tests.sh`.

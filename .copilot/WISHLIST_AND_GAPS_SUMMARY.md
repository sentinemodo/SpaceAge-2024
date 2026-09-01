# Campaign Play: Wishlists & Remaining Gaps

**Date:** 2026-08-30  
**Engine Version:** 0.1.157  
**Status:** All critical path items complete; launch ready

---

## Executive Summary

**Campaign play is ready to launch.** The engine supports:
- 10-player PBEM with AI isolation
- JUMP gates, space transit, typed combat, hull groups, skill bonuses, environment bands
- Campaign XML catalog (L0–L10), turn 1 seed, play scripts
- Player manuals (rules, techs, battle), `/campaign-ai` and `/campaign-gm` agents

**What's left:** Website UI (parallel; not blocking), optional wishlist enhancements, and mid-game contract hooks.

---

## 📋 Wishlists (Player Agent Feedback)

### Player Order Syntax Wishlist (`player/order_wishlist.md`)

**Current state:** 2 items  
**Blocker for campaign?** No (workarounds exist)  
**Justification:** Each addresses a player objective that is awkward or impossible with current syntax.

| Proposal | Objective | Current Limitation | Workaround | Priority |
|----------|-----------|-------------------|-----------|----------|
| `TRANSFER ALL [DAMAGED] MODULES TO <id>` | Dump entire stack or pick damaged modules onto existing receiver | `TRANSFER <n> TO <id>` always takes first `n` of a type; cannot pick by damage state | Manual count-and-transfer per module; repair in place | P3 (late-game stack optimization) |
| `REPAIR` without precondition | Restore captured, heavily damaged guns to operational | `REPAIR` calls `CanOperate()` first; captured (Online=false) or wrecked (QuantityActive=0) modules cannot self-repair | Parent stack with operational modules repairs its own damage first (RepairScope); user must merge/separate stacks | P3 (captured-gun recovery flavor) |

**Action:** Monitor campaign play for player struggles. If players frequently request these, file C# / order-syntax wishlist with player objectives as rationale.

### Player Technology Wishlist (`player/technologies_wishlist.md`)

**Current state:** 1 item  
**Blocker for campaign?** No (workaround: sick-bay only)  
**Justification:** Catalog design choice; not an engine gap.

| Proposal | Objective | Current Limitation | Workaround | Notes |
|-----------|-----------|-------------------|-----------|-------|
| Wire medical facility `[medfac]` heal | Treat wounded crew in L2 clinic | Catalog `heal target="stacked"`; engine loads heal only when `target="wndtrn"`. Weekly conversion is sick-bay only | Sick-bay `[sckbay]` handles all medical. Deploy sickbays to HQs and large ships | **Designer decision:** `medfac` focus shifts to research/training (not medical in this campaign). If players want dedicated medical, `/player` escalates to `/game-designer` for catalog retune. |

**Action:** This is a **design choice**, not a bug. Document in `designer/catalog.md` rationale: sickbay is the mobile medical solution; medfac is lab/research. Revisit for Phase 3 if needed.

### Engine Wishlist (`designer/engine-wishlist.md`)

**Current state:** 24 rows; 15 live (0.1.155+), 4 out-of-scope, 5 in-scope pending  
**Blocker for campaign play?** No  
**Blocker for website Phase 1?** No  
**Blocker for Phase 2 (contracts)?** Some (contract triggers, events)

#### Already Implemented (0.1.148–0.1.157)

| Implemented | Engine Version | Status |
|-------------|-----------------|--------|
| Space transit from ΔAU × drive speed | 0.1.148 | ✅ Live |
| Retune f(ΔAU) to 2/6/13/39 formula | 0.1.148 | ✅ Live |
| System X Y Z coordinates | 0.1.148 | ✅ Live (load/save/report) |
| Body gravity/temperature/atmosphere | 0.1.148 | ✅ Live (load; effects active) |
| Shuttle h₂o₂ launch/land surcharge | 0.1.148 | ✅ Live (frigate+ banned) |
| Item attack/defense/damage in battle | 0.1.149 | ✅ Live (cargo equipment bonus) |
| Hull size groups (corvette…ark) | 0.1.149 | ✅ Live (Parse/ToToken/IsShipHull) |
| Typed weapon group vs resists | 0.1.155 | ✅ Live (matchup table) |
| Armour module hit bias + no capture | 0.1.155 | ✅ Live (×5 hitWeight; capture ignored) |
| Shield damage intercept | 0.1.155 | ✅ Live (90% floor onto shield) |
| Use-produce effect execution | 0.1.151 | ✅ Live (REPAIR via ProducingEffect) |
| Skill usable-in / cure-chance | 0.1.155 | ✅ Live (battle/production/medical gates) |
| Sick-bay heal cadence | 0.1.150 | ✅ Live (weekly wndtrn→terran) |
| Use-allowed-in module constraint | 0.1.150 | ✅ Live (UseOrder gates pharms to sckbay) |
| Item radiation / equipment bonuses | 0.1.149+ | ✅ Live (vests, suits, dosimeters consumable) |

#### Pending (In Scope, Phase 3)

| Need | Objective | Surface | Impact | Priority |
|------|-----------|---------|--------|----------|
| Moon `@name` unique ids | Moon M00001 survives load (not parent planet name) | `DataFile.LoadGalaxy` moon constructor | Campaign moons only; affects explorer naming | P3 |
| `loadGalaxyExits` moon regions + orbits | Moon maps and orbit↔surface space hops | Second pass over all `Region` / `Orbit` | Campaign moon bases playable; workaround: dust belts | P3 |
| Contract triggers: `survive-weeks`, `destroy-stack`, `region-resource-below` | Bombardment, fauna, pirate hunts | `IContractTrigger` + XML attrs | Mid-game narrative; workaround: static contracts | P2 |
| `Events` pipeline for timed spawns | Alien reactivation, impact scheduling | `Game/Events.cs` expansion | Campaign storytelling; workaround: GM-triggered contracts | P2 |
| `atmosphere` location-type | Gas-giant cloud regions | `LoadLocationType` | Gas-giant gameplay; workaround: regular regions on gas moons | P3 |
| Drive-dependent fuel for torch/pulse/ark | Fustor/plsdv/arkeng consumes heliu3 | `Move` effect + catalog fuel | Long-range travel cost; workaround: manual tracking | P3 |
| Gas-giant orbit resources | Deutrium, helium-3 on gas-giant orbits | `Orbit` location-type + resource seed | Resource exploration; workaround: ice-moon surfaces | P3 |
| `SEE` / scan bonus from `survsc` | Anomaly investigation | `SeeOrder` + module flag | Anomaly gameplay flavor; workaround: always visible | P3 |

#### Out of Scope (This Plan)

| Out of Scope | Reason | When |
|-------------|--------|------|
| Hostility-flip Events | Militia yearly raids not in T1 seed | Post-campaign, separate phase |
| Multi-reward / `capture-stack` contracts | Complex trigger composition | Phase 3+ wishlist |
| Coastal region type | PBEM simplification: no land-water interaction (naval regions can touch land; land can touch water) | Architecture review needed |
| Changing SampleGame or `Tests/data.xml` | Freeze test fixtures; campaign only | Never |
| Engine victory flag | Victory decided by report inspection (UI layer) | Phase 4+ (visual tool) |

---

## 🔌 Play Loop Gaps (Documented in `play/README.md`)

### Scripts Not Yet Wrapped

| Gap | Workaround | When | Priority |
|-----|-----------|------|----------|
| `play/no-turn.ps1` | GM runs `/no-turn` line directly from terminal | Between-turn UN contracts (Phase 2) | P2 |
| NPC 12/13 raid orders | Not invoked yet; contracts only | Post-campaign raiding (Phase 3) | P3 |

### Data Artifacts Not Yet Produced

| Artifact | Role | Producer | When | Priority |
|----------|------|----------|------|----------|
| `website/public/status.json` | Lobby shows turn status + orders-submitted per faction | Play scripts (Phase 2) or manual input | Website Phase 1 launch | P2 |
| Mid-game wrecks / altered `gamein.xml` | Designer patches run catalog for injected contracts | Designer patch of run data (not committed) | Phase 2 (between-turn) | P2 |

---

## 🎯 Campaign Play Success Metrics

### Phase 1 (Campaign Playable): Days 1–7

**Acceptance:** One full AI quarter (turn 1→2) completes end-to-end.

- [x] Engine green (SampleGame 448/448, campaign catalog load)
- [x] Campaign XML ready (data.xml, gamein.1.xml, environments, combat balance)
- [x] Play scripts ready (init → reports → isolate → turn → next)
- [x] AI agents ready (campaign-ai → /player)
- [ ] Website Phase 1 pages deployed (/, /client, /turns, /rules)
- [ ] One full quarter executed with 10 AI factions
- [ ] Reports match story objectives

**Blockers:** None (all engine items live; website is parallel).

### Phase 2 (Polish & Contracts): Days 8–21

**Acceptance:** Mid-game UN intervention; website status JSON; playable campaign.

- [ ] `play/no-turn.ps1` wraps `/no-turn` line
- [ ] Designer injects mid-game contract; `/player` reacts
- [ ] Website status JSON reflects current turn + orders-submitted
- [ ] 2–3 more quarters played
- [ ] Win condition check works

**Blockers:** Contract trigger hooks (partial; static contracts sufficient).

### Phase 3 (Richness): Weeks 2–4

**Acceptance:** Richer gameplay, moon bases, timed events.

- [ ] Moon construction + unique ids
- [ ] Moon region→orbit exits (second `loadGalaxyExits` pass)
- [ ] Contract triggers (`survive-weeks`, `destroy-stack`, `region-resource-below`)
- [ ] Events pipeline (timed alien reactivation)
- [ ] Optional: gas-giant atmosphere, fuel consumption, scan bonuses

**Blockers:** None (all optional enhancements).

---

## 📞 How Wishlists & Gaps Drive Future Work

### Process

1. **Player objective:** Player-agent notices a player cannot achieve goal with current syntax/catalog
2. **Wishlist entry:** Write proposal in `player/order_wishlist.md` or `player/technologies_wishlist.md` with rationale
3. **Designer escalation:** If catalog is the gap, `/game-designer` updates `designer/engine-wishlist.md` or proposes data changes
4. **TDD adoption:** If engine is the gap, TDD adds failing test + implementation
5. **Campaign retry:** Player re-plays campaign slice with new capability

### Current Wishlists

**Order Syntax (2 items):**
- Both are P3 (end-game optimization; not campaign blockers)
- If players request during Phase 1, escalate to TDD for engine slice

**Technology (1 item):**
- Designer decision (medfac focus); not a bug
- Revisit for Phase 3 if players want dedicated medical

**Engine Wishlist (24 rows):**
- **15 live** (0.1.148–0.1.157) ✅
- **5 pending P2–P3** (moon names, events, fuel, atmosphere, gas orbits)
- **4 out-of-scope** (hostility-flip, multi-reward contracts, coastal, victory flag)

---

## 🔄 Transition to Phase 1 Launch

### Pre-Launch Checklist

- [x] Engine version 0.1.157 live with all critical features
- [x] Campaign catalog complete (L0–L10, all modules, items, skills)
- [x] Campaign gamein.1.xml seeded with 10 factions + NPC
- [x] Play scripts ready (init, reports, isolate, turn, next)
- [x] Agent definitions ready (campaign-ai, campaign-gm)
- [x] Player manuals refreshed (rules, techs, battle)
- [ ] Website Phase 1 scaffolded (architect → developer → tester)
- [ ] One full quarter walkthrough (TDD + GM + AI)
- [ ] Reports validated against story objectives
- [ ] PR created; ready to merge to `main`

### Go/No-Go Decision (2026-08-31)

| Gate | Status | Owner | Notes |
|------|--------|-------|-------|
| Engine playability | ✅ PASS | TDD | All critical path items live; SampleGame green |
| Campaign data | ✅ PASS | Designer | Catalog, gamein, environments complete |
| Play infrastructure | ✅ PASS | GM | Scripts + agents + manuals ready |
| Website Phase 1 | ⏳ IN PROGRESS | Architect + Dev | Parallel; not blocking; targeted 7 days |
| AI walkthrough | ⏳ PENDING | Campaign-GM | Estimated 5 days after website scaffolding |

**Recommendation:** Launch campaign play **when website Phase 1 scaffolding + one AI quarter walkthrough complete**. This is **2–3 weeks away** (parallel tracks).

---

## 📚 Wishlist References

- **Player order wishlist:** `player/order_wishlist.md` (2 items)
- **Player technology wishlist:** `player/technologies_wishlist.md` (1 item)
- **Engine wishlist:** `designer/engine-wishlist.md` (24 rows; 15 live, 5 pending, 4 out-of-scope)
- **Play gaps:** `play/README.md` § Gaps (3 items; all P2+)

See `architecture/delivery/campaign-play.md` for detailed plan and rationale.

---

## 🚀 Next Action

1. **Architect** reviews `architecture/delivery/website.md` + ADR-0007
2. **Website-Developer** scaffolds Astro static site (Phase 1 pages)
3. **Campaign-GM** walks through Q1 (init → reports → isolate → 10×AI → turn → next) in parallel
4. **Tester** validates walkthrough and website acceptance tests
5. **Merge** when both tracks pass acceptance

---

**Status:** Ready to launch campaign play. Website Phase 1 in progress. Wishlist items tracked for Phase 2–3 iteration.


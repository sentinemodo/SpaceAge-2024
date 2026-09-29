# UN colony ship hull (draft)

**Status:** draft only — not in `play/campaign/data.xml` or `gamein.1.xml`.

## Pattern sources (repo)

| Concern | Live reference |
|---------|----------------|
| Hull + nested stacks | `Tests/SampleGame/gamein.5.xml` (`sshull` → `cbridg`, `fisrec`, `rctdrv` children) |
| Hull catalog attrs | `play/campaign/data.xml` `sshull`, `lghul`, `arkhul` (`group="frigate"`, `cannot-hold-itemstacks="yes"`, orbit/space) |
| Cargo bays | `cargob` — capacity **1800** size units each |
| Libraries | `cmplib` — `technology-capacity="4"` each |
| Command | `cbridg` |
| Crew quarters | `crwqrt` — `habitat="20"` each |
| Life support | `lifsys` (`name-en="life support system"`) — ship-legal on frigate+ hull groups |
| Power / drive | `fisrec` + `rctdrv` (same pairing as SampleGame frigate) |
| Resource ids | `iron`, `carbon`, `oil`, `food`, `terair`, `uraniu`, `titani`, `silici` |
| GRANT behaviour | `Game/orders/GrantOrder.cs` — tech copies need host stack `technology-capacity`; items need cargo `capacity` on target stack |

## Draft files

| File | Role |
|------|------|
| [`un-colony-ship-catalog.xml`](un-colony-ship-catalog.xml) | New module **`uncolh`** (UN colony ship hull) |
| [`un-colony-ship-layout.xml`](un-colony-ship-layout.xml) | Pre-built stack tree, **`faction="1"`** |

## Structure summary

- **Root:** `uncolh` ×1 — size 26000, hull capacity **21000** (nested modules total **16700** size; **4300** margin).
- **Command / life:** 1× `cbridg`, 1× `lifsys`.
- **Crew:** 4× `crwqrt` (80 habitat berths total).
- **Propulsion / power:** 1× `fisrec`, 1× `rctdrv` (seed fuel items only in layout draft).
- **Libraries:** 7× `cmplib` → **28** technology capacity (campaign catalog has **26** level-1 technologies today; sum of levels = **26**).
- **Cargo:** 6× `cargob` with seeded itemstacks (see capacity table below).

### Cargo capacity check (item `size` × quantity)

| Resource | Qty | Unit size | Stack size |
|----------|-----|-----------|--------------|
| iron | 500 | 5 | 2500 |
| carbon | 500 | 5 | 2500 |
| oil | 500 | 4 | 2000 |
| food | 500 | 1 | 500 |
| terair | 500 | 1 | 500 |
| uraniu | 100 | 1 | 100 |
| titani | 100 | 10 | 1000 |
| silici | 100 | 5 | 500 |
| **Total** | | | **9600** |

Six bays × 1800 = **10800** cargo capacity (fits with headroom).

### Nested size budget (hull internal)

| Module | Count | Size each | Subtotal |
|--------|------:|----------:|---------:|
| cbridg | 1 | 800 | 800 |
| lifsys | 1 | 100 | 100 |
| crwqrt | 4 | 500 | 2000 |
| fisrec | 1 | 400 | 400 |
| rctdrv | 1 | 600 | 600 |
| cmplib | 7 | 200 | 1400 |
| cargob | 6 | 2000 | 12000 |
| **Total** | | | **16700** |

## IDs and names

| Id | English name |
|----|----------------|
| `uncolh` | UN colony ship hull |
| (nested) | Standard catalog modules only — no new component ids |

Layout stack ids: `UNC001` (hull), `UNC011`–`UNC056` (children). Rename before merge to match orbit naming in target system.

## Future GRANT integration (faction 1)

1. **Catalog merge:** `/game-designer` adds `uncolh` from `un-colony-ship-catalog.xml` into `play/campaign/data.xml` (Windows-1251). Optionally add a production tech later; not required for UN stock GRANTs.
2. **Patron stock:** Merge `un-colony-ship-layout.xml` under a UN reserve orbit in run `gamein` (or spawn via contract `give-module` once catalog live).
3. **Between-turn orders** (UN `#faction 1 ""`):
   - `GRANT module uncolh TO <receiver-stack>` — if issuing a bare hull; prefer transferring the pre-built `UNC001` tree via `TRANSFER` / contract if the whole colony kit must move intact.
   - `GRANT technology <tech-id> TO UNC001` (or any child id) — engine picks first child `cmplib` with room (`FindTechnologyCopyHost`). With seven empty libraries, all **26** L1 tech copies fit (level sum 26 ≤ 28).
   - `GRANT item <qty> <item-id> TO UNC05x` — target a **cargob** stack id; each bay has spare capacity after seed loads.
4. **Costs:** `play/designer/economy.md` GRANT pricing; module grants debit faction 1 bank.
5. **Player handoff:** After GRANT/TRANSFER, recipient faction re-issues orders on their `#faction` line; UN does not retain control unless stack stays `faction="1"`.

## Merge checklist (when seeding)

- [ ] Confirm `uncolh` id unused in catalog and ≤6 chars
- [ ] Re-validate L1 tech count if catalog changes (adjust cmplib count: `ceil(sum(L1 levels) / 4)`)
- [ ] Encode merged XML as Windows-1251
- [ ] Do **not** copy into `Tests/data.xml` without explicit TDD scope

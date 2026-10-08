# Player-facing documentation sync

Last updated: 2026-10-09  
Purpose: **done gate** when engine order syntax or catalog techs (level 0–1, and level 2+ when applicable) change. Prevents drift between `Game/`, the visual client, RAG, and the public website.

Decision context: [ADR-0004](../adr/ADR-0004-test-layers.md) (engine tests), [ADR-0007](../adr/ADR-0007-public-campaign-website.md) (website reads `docs/human/`), [ADR-0010](../adr/ADR-0010-visual-tool.md) (client technologies panel), [local-player-agent.md](local-player-agent.md) (verb allowlist).

## Roles

| Role | Owns |
|------|------|
| **`/tdd`** | Engine + NUnit; invokes **`/player`** docs refresh before commit when orders or L0–L1 catalog change |
| **`/player`** | `play/player/*.md`, `docs/human/rules.md`, `docs/human/basic_technologies.md` (sync from player manuals) |
| **`/website-developer`** | Vitest for rules index; no duplicate rules text in Astro — build reads `docs/human/` |
| **Game-host / visual-tool** | `game-host/lib/check-orders.mjs` verb list; Docker copies `docs/human/basic_technologies.md` into the client bundle |

Human players see **`docs/human/`** on the site (`/rules`, `/faq`). Agents and RAG use **`play/player/`** ( richer, code-oriented). **Behavior must match**; wording may differ.

---

## A. Order syntax changes

Trigger: new or changed verb in `EOrderType`, `OrderFactory`, `*Order.Parse`, `#faction` header rules, between-turn allowance, or client-side validation.

### Authoritative implementation

- `Game/orders/EOrderType.cs`
- `Game/orders/OrderFactory.cs`
- `Game/orders/*Order.cs`
- `Game/orders/OrdersReader.cs` (headers, password line)
- NUnit: factory registry test + verb-specific tests (e.g. `TOrder`, `TPasswordOrder`)

### Player and public copy (same pass via `/player`)

| File | Requirement |
|------|-------------|
| [`play/player/rules.md`](../../../play/player/rules.md) | `## Immediate orders` / `## Long orders`; `### VERB` per live verb; between-turn list; comma-separated verb roll call |
| [`docs/human/rules.md`](../../human/rules.md) | Human SSOT: turn basics, **Full orders dictionary** (`**VERB** —` lines), between-turn list; update verb counts in dictionary header if needed |
| Website [`/rules`](../../../website/src/pages/rules.astro) | **No edit** when only markdown changes — rebuild/redeploy site picks up `docs/human/rules.md` |

### Client and agent tooling (engine agent or `/player` handoff)

| File | Requirement |
|------|-------------|
| [`game-host/lib/check-orders.mjs`](../../../game-host/lib/check-orders.mjs) | Add verb to `KNOWN_VERBS` |
| [`game-host/test/check-orders.test.mjs`](../../../game-host/test/check-orders.test.mjs) | At least one parse sample for new faction-level or unusual syntax |
| [`tools/player-agent/Lint/OrderVerbAllowlist.cs`](../../../tools/player-agent/Lint/OrderVerbAllowlist.cs) | Add verb to `FallbackVerbs` when it has a `### VERB` heading in rules |
| [`tools/player-agent/Lint/verb-allowlist.json`](../../../tools/player-agent/Lint/verb-allowlist.json) | Regenerate: `dotnet run --project tools/player-agent -- regenerate-allowlist` |
| [`website/tests/humanRules.test.ts`](../../../website/tests/humanRules.test.ts) | Update expected immediate/long **counts** if dictionary size changed |

### Verification commands

```text
dotnet build Tests/Tests.csproj
node --test game-host/test/check-orders.test.mjs
npm test -- --run tests/humanRules.test.ts   # in website/
dotnet test tools/player-agent-tests --filter OrderVerbAllowlist
```

Production client parse also requires **`Game.exe`** in the game-host Docker image (rebuild after engine change).

---

## B. Basic and level-1 technologies

Trigger: `<technology level="0">` or `level="1"` (or modules/items they produce/consume) change in **`Tests/data.xml`** (SampleGame) and/or **`play/campaign/data.xml`** (open beta).

### Catalog sources

| Mode | Catalog |
|------|---------|
| SampleGame / NUnit | `Tests/data.xml` |
| Campaign / open beta | `play/campaign/data.xml` |

Do not point SampleGame player manuals at the campaign catalog unless an ADR explicitly retargets them ([campaign-play.md](campaign-play.md)).

### Markdown targets (level 0 + 1 only)

| File | Audience |
|------|----------|
| [`play/player/basic_technologies.md`](../../../play/player/basic_technologies.md) | Agents, RAG, `/player` (SampleGame catalog path in header) |
| [`play/player/campaign/basic_technologies.md`](../../../play/player/campaign/basic_technologies.md) | Campaign-ai / campaign play (regenerate when **campaign** L0–L1 changes) |
| [`docs/human/basic_technologies.md`](../../human/basic_technologies.md) | **Human + visual client** Technologies panel (Dockerfile copies this path into the game-host image) |

Level **2+** changes: update [`play/player/advanced_technologies.md`](../../../play/player/advanced_technologies.md) in the same pass (not the website rules page).

### Website and FAQ

| Surface | Source |
|---------|--------|
| [`/rules`](../../../website/src/pages/rules.astro) | Order syntax only (`docs/human/rules.md`) |
| [`/faq`](../../../website/src/pages/faq.astro) | [`docs/human/faq.md`](../../human/faq.md) — update when FAQ text references tech ids, drills, or L0–L1 behavior |
| Home / other Astro | No full tech dump; link to client or FAQ as today |

After editing `docs/human/*.md`, run `npm test` in `website/` before handoff to **`/website-tester`**.

### Designer cross-reference

When ids or level bands shift, check [`play/designer/technology.md`](../../../play/designer/technology.md) and wishlists under `play/designer/` — designer specs are not player SSOT but should not contradict live ids.

---

## C. Commit checklist (TDD)

Before committing engine work that touches **orders** or **L0–L1 tech**:

1. NUnit green for new/changed behavior.
2. Invoke **`/player`** with: “docs-only refresh per `docs/architecture/delivery/player-facing-docs-sync.md` sections A and/or B.”
3. Complete every row in section A and/or B that applies (including game-host + allowlist + website test counts).
4. If SampleGame goldens changed for non-version reasons: `/player` golden handoff + **human approval** (existing golden policy).

If the commit is **documentation-only**, `/player` still runs the same sync; no C# required.

---

## D. What not to duplicate

- Do not paste `play/player/rules.md` into Astro components — use `docs/human/rules.md` only.
- Do not maintain a second verb list in the visual tool; validation is game-host + `Game.exe`.
- Legacy [`docs/legacy/alderson/`](../../legacy/alderson/) is not live behavior.

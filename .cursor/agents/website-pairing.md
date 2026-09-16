# Website developer ↔ tester pairing

Shared contract for the Astro lobby (`website/`).

| Concern | Developer | Tester |
|---------|-----------|--------|
| `website/src/**` | implements | read-only |
| Vitest | ✓ | — |
| Playwright E2E | — | ✓ |
| Scenario catalog | proposes | owns `website/e2e/scenarios.md` |

**Done gate:** developer stops at Vitest + build; tester owns Playwright green.

**Constraints:** no `Game` reference; no serving game state from static lobby; `/eta` and `/battle` are browser-only.

Specs: [website-developer.md](website-developer.md) · [website-tester.md](website-tester.md)

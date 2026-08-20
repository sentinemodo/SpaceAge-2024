# Order syntax wishlist

Suggestions for **easier or missing order syntax**. The player agent adds entries only when a player objective is hard or impossible with live verbs in `rules.md`.

Do not copy these into `rules.md` until the engine parses them.

| Proposal | Objective | Why current syntax is insufficient |
|----------|-----------|-------------------------------------|
| `TRANSFER ALL [DAMAGED] MODULES TO <id>`; `TRANSFER MODULE <n> TO <id>` | Dump a whole stack, or hand-pick a damaged module, onto an existing receiver | Live `TRANSFER <n> TO <id>` always takes the first `n` modules of the subject’s type. It cannot dump ALL without a count, skip healthy modules, or pick index `n`. |
| `REPAIR` that does not require the repairer to already be operational | Restore captured, heavily damaged guns so they can fire | Live `REPAIR` calls `CanOperate` (`HasOperationalModules`). Heavily damaged modules have `QuantityActive` 0; captured guns are also `Online=false`. They cannot start self-repair. A parent that can operate will repair its own damage first (`RepairScope`). |

# Order syntax wishlist

Suggestions for **easier or missing order syntax**. The player agent adds entries only when a player objective is hard or impossible with live verbs in `rules.md`.

Do not copy these into `rules.md` until the engine parses them.

| Proposal | Objective | Why current syntax is insufficient |
|----------|-----------|-------------------------------------|
| `TRANSFER <n> TO <stack>` in `OrdersReader` (class already parses `TRANSFER <n> TO <id>`) | Move modules between existing stacks from a turn file | Text orders reject `TRANSFER` (“Unknown order”). XML can load it. `FORM NEW WITH n` only splits off a **new** stack, not an existing receiver. |
| `PRESS TITLE "…" FLAVOUR "…"` in the text (and XML) switches | Issue a press release from `#faction` | `PressOrder.Parse` exists; neither `OrdersReader` nor `DataFile.LoadOrders` routes the verb. |

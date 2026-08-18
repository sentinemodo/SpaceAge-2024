# Technology wishlist

Suggestions for **new technologies or balance changes**. The player agent adds entries only when a player objective needs catalog support that level 0–1 techs in `basic_technologies.md` do not provide.

Do not edit `data.xml` here.

| Proposal | Objective | Justification |
|----------|-----------|----------------|
| Wire `USE repair` (effect `repair` / `module-damage`) or drop the tech from the catalog | Recover hit points by using `[repair]` | Catalog level-1 **repair and maintenance** sets `EProductionType.Effects`; `UseOrder.Execute` throws “Not implemented”. The working path is the **`REPAIR`** order plus spare parts / engineering shop. |

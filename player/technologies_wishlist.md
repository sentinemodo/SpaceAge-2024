# Technology wishlist

Suggestions for **new technologies or balance changes**. The player agent adds entries only when a player objective needs catalog support that level 0–1 techs in `basic_technologies.md` do not provide.

Do not edit `data.xml` here.

| Proposal | Objective | Justification |
|----------|-----------|----------------|
| Wire medical facility `[medfac]` heal, or retarget it to `wndtrn` like sick bay | Treat wounded crew in the level-2 clinic | Catalog heal is `target="stacked"`; `DataFile` only loads heal when `target="wndtrn"`. Weekly conversion is sick bay `[sckbay]` only. |

# ADR-0007: Public campaign website as a new bounded context

Date: 2026-08-29  
Status: **Accepted**  
Plan: [`../delivery/website.md`](../delivery/website.md)

[ADR-0003](ADR-0003-filesystem-pbem-batch.md) stated that adding a web UI is a **new product surface** and needs its own ADR. This is that ADR. It does **not** change the engine host.

## Context

SpaceAge is a net48 console PBEM engine: file-in / file-out, Windows-1251 XML and orders, no HTTP, SMTP, or database ([ADR-0001](ADR-0001-net48-legacy-csproj.md), [ADR-0002](ADR-0002-windows-1251-io.md), [ADR-0003](ADR-0003-filesystem-pbem-batch.md)). Campaign play uses `play/runs/<id>/` and ten player factions (2–11).

Humans asked for a **public site**: home (Alderson flavour + lineage), a link to a future **visual tool**, and **orders-submission status**. The visual tool is a third product (report/XML client). Atlantis New Origins ([atlantis-pbem.com](https://atlantis-pbem.com/), reviewed 2026-08-29) is a reference for **information architecture** only.

Putting pages on `Game.exe` would violate the batch-processor decision and couple lobby hosting to the CLR.

## Decision

1. **New bounded context: Website.** Lives in `website/` at the repo root. Not a `Game` project reference. Not an assembly in `SpaceAge.sln`.
2. **Stack:** Astro with **static** output, hand-written CSS, TypeScript only for small islands (e.g. countdown). Host on GitHub Pages (or Cloudflare Pages / Netlify).
3. **Status without an engine API:** publish a UTF-8 `status.json` (schema in the plan). Phase 1 is a committed placeholder; Phase 2 is PowerShell in `play/` writing that file from order-file presence. The site never calls `Game.exe` and never serves `gamein.xml`.
4. **Visual tool:** out of scope for the website implementer. The site exposes `/client` and a placeholder href (`/visual-tool/` or a later absolute URL). Turning the link on is Phase 3.
5. **Closed lobby:** no open signup. Attribution (Atlantis, Rise of Heroes, Vincent Archer) is required on the home page.
6. **Phase 4 player tools (2026-08-29):** the same static origin may later add `/eta` (transit ETA from a **user-pasted text** ship report + two AU-from-star) and `/battle` (two-side what-if). Both are browser islands that port published formulas. They do **not** call `Game.exe`, do not serve `report.*` / `gamein.xml` / `data.xml`, and do not persist pastes. A full `Battle.cs` port or server-side report parse would need a new ADR.

## Options considered

| Option | Decision | Why |
|--------|----------|-----|
| Keep UI off the web; email-only | Rejected for this request | Players need a lobby + status + client link |
| ASP.NET / HttpListener on net48 next to `Game.exe` | **Rejected** | New surface on the batch host; GM would run a Windows web stack; contradicts ADR-0003 |
| Retarget engine to ASP.NET Core | **Rejected** | Product-wide CLR migration; [ADR-0001](ADR-0001-net48-legacy-csproj.md) |
| PHP / Laravel clone of Atlantis | **Rejected** | Wrong aesthetic and ops (PHP host); copies a different game’s stack |
| Next.js static export | Rejected | Heavier than a four-page lobby |
| Vite + vanilla, or plain HTML | Rejected as default | More DIY for shared layout and copy; Astro fits content-first static sites |
| CMS | Rejected | Few pages, no editorial org |
| Engine REST so the site polls live state | **Rejected** | Invents the HTTP API ADR-0003 forbids; would risk leaking `gamein` |

## Consequences

- Implementers add `website/` and static hosting only. Engine CI (Mono / `Game.exe`) stays independent. Website tests are **Vitest + Playwright** in `website/` ([Astro testing](https://docs.astro.build/en/guides/testing/)), not NUnit.
- GMs publish status by updating a file, not by opening a port on the turn machine.
- A future visual-tool app is a **third** context; it may use React and XML reports. Sharing design tokens is optional and later.
- Security bar: no passwords, no `gamein.xml`, no foreign reports on the public origin.
- Changing this stack (SSR, accounts, engine HTTP) needs a new ADR.

## Revision

- 2026-08-29: Accepted. Astro static lobby; file-published status JSON.
- 2026-08-29: Decision 6 — Phase 4 `/eta` and `/battle` as client-side planning tools on the same origin.

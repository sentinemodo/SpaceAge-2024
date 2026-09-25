# Hosted open beta — GM scenarios (Docker + Caddy)

Player client URL (HTTPS, same origin as API):

**https://spaceage-pbem.duckdns.org/client/**

Game-host and the visual tool run on the GM laptop at port **8787**. Caddy on that laptop (`C:\Users\akacz\caddy\Caddyfile`) terminates TLS for `spaceage-pbem.duckdns.org` and forwards to `127.0.0.1:8787`. DuckDNS points the name at WAN `91.220.222.102`. The Huawei HS8145V forwards TCP **80** and **443** to the laptop `192.168.100.17`. Port **8787** is not mapped on the router.

ngrok (`https://manatee-sabbath-kudos.ngrok-free.dev/client/`) remains the fallback when the WAN forward is unreachable. The GitHub Pages workflow still bakes `PUBLIC_CLIENT_URL` to that ngrok URL until the workflow is changed. Caddy does not start on reboot (`caddy start` from `C:\Users\akacz\caddy`).

See also: [`docker/README.md`](../docker/README.md), [`game-host/README.md`](../game-host/README.md).

---

## Prerequisites (once)

| Item | Notes |
|------|--------|
| Docker Desktop | Windows |
| `.env` at repo root | Copy from `.env.example`; set `GAME_HOST_GM_KEY` |
| ngrok | `npm install -g ngrok` then `ngrok config add-authtoken <token>` |
| Reserved domain | `manatee-sabbath-kudos.ngrok-free.dev` on your ngrok account |

Optional on GM host (player-agent, not in Docker): Ollama in Docker on port 11434, .NET 8 SDK.

---

## Scenario A — Start game-host (Docker)

**When:** Beginning a session, or after pulling engine/client changes.

From repo root:

```powershell
docker compose up --build -d
# or: .\play\docker-up.ps1 -Build
```

**Verify:**

```powershell
Invoke-RestMethod http://localhost:8787/health
start http://localhost:8787/client/
```

Campaign files on the host (survive container rebuilds):

| Host path | Purpose |
|-----------|---------|
| `play/runs/beta-1/` | Canonical run: `gamein.xml`, orders, reports, personas, GM notes |

---

## Scenario B — First-time campaign bootstrap

**When:** New run or empty `play/runs/beta-1/data/gamein.xml`.

```powershell
.\play\beta-launch.ps1 -GameHostUrl http://localhost:8787 -GmKey <GAME_HOST_GM_KEY>
```

This runs `init-run.ps1` if needed, calls GM init + reports, and updates `website/public/status.json`.

Faction passwords (invite only — never commit):

```powershell
Select-String -Path play\runs\beta-1\data\gamein.xml -Pattern 'password='
```

Commit and push lobby status after bootstrap:

```powershell
git add website/public/status.json
git commit -m "Update lobby status after bootstrap."
git push
```

---

## Scenario C — Expose game-host via ngrok (remote players)

**When:** Players need HTTPS access from outside your LAN.

1. Ensure Docker game-host is up (Scenario A).
2. Start the tunnel:

```powershell
.\play\expose-game-host-ngrok.ps1
```

Uses reserved domain **`manatee-sabbath-kudos.ngrok-free.dev`** by default.

**Verify (phone on cellular or another network):**

- https://manatee-sabbath-kudos.ngrok-free.dev/health → `"ok": true`
- https://manatee-sabbath-kudos.ngrok-free.dev/client/ → login screen

**Notes:**

- Keep the ngrok process running while players are online (or run it as a Windows service).
- ngrok free tier may show an interstitial on first visit — players click through once.
- Router port forwarding is **not** required; see [`router-port-forward.md`](router-port-forward.md) only if you prefer a static IP instead of ngrok.

---

## Scenario D — Player turn (players act, GM waits)

**When:** Turn is open; players log in and submit orders.

Players use:

- Lobby → **Open SpaceAge Client**, or
- Direct: https://manatee-sabbath-kudos.ngrok-free.dev/client/

They log in with faction id **2–11** and password from their invitation.

GM monitors submitted orders on disk:

```powershell
Get-ChildItem play\runs\beta-1\turn\order.*.txt
```

---

## Scenario E — Run GM turn (close turn, advance game)

**When:** All (or enough) orders are in; ready to execute the quarter.

```powershell
Invoke-RestMethod -Method Post `
  -Uri http://localhost:8787/api/gm/turn `
  -Headers @{ 'X-GM-Key' = '<GAME_HOST_GM_KEY>' }
```

Optional — refresh reports without full turn:

```powershell
Invoke-RestMethod -Method Post `
  -Uri http://localhost:8787/api/gm/reports `
  -Headers @{ 'X-GM-Key' = '<GAME_HOST_GM_KEY>' }
```

---

## Scenario F — Player-agent RAG (optional)

**When:** After Scenario E; you want local AI drafts from new reports.

Game-host `/reports` and `/turn` already write isolated text reports under `play/runs/beta-1/factions/`. Ingest directly:

```powershell
.\play\ollama-check.ps1
.\play\ingest-rag.ps1 -Run beta-1 -Mode campaign
.\play\draft-run.ps1 -Run beta-1 -Mode campaign -DryRun   # drop -DryRun to draft
```

Ollama runs on the GM host (Docker container on 11434), not inside game-host.

---

## Scenario G — Publish lobby turn status

**When:** After turn execution or schedule change; update the public dashboard.

```powershell
.\play\generate-status.ps1 beta-1
git add website/public/status.json
git commit -m "Update lobby turn status."
git push
```

GitHub Pages redeploys on push to `master` (website workflow).

---

## Scenario H — Rebuild after code changes

**When:** You changed `Game/`, `tools/visual-tool/`, or `game-host/`.

```powershell
docker compose up --build -d
# restart ngrok if it was running (Scenario C)
```

To rebuild only the visual tool on the host without full image rebuild:

```powershell
cd tools\visual-tool
npm run build
docker compose restart game-host
```

---

## Scenario I — Stop stack

**When:** End of session or maintenance.

```powershell
# Ctrl+C in the ngrok terminal, or close the tunnel window
docker compose down
# or: .\play\docker-up.ps1 -Down
```

---

## Troubleshooting

| Symptom | Check |
|---------|--------|
| ngrok script fails “game-host not reachable” | Scenario A — `docker compose ps` |
| `/client/` 404 locally | Rebuild image; visual tool must be in `tools/visual-tool/dist` inside container |
| GM API 403 | `X-GM-Key` must match `.env` `GAME_HOST_GM_KEY` |
| Remote health fails | ngrok running? Reserved domain on your account? |
| Lobby links to localhost | Website built without `PUBLIC_CLIENT_URL`; see `.github/workflows/website.yml` |

---

## Security

- Rotate `GAME_HOST_GM_KEY` before open beta; never share with players.
- Back up `play/runs/beta-1/` before each GM turn.
- Only port **8787** is exposed via ngrok; do not expose GM key in chat or commits.

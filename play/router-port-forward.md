# Expose the laptop on the public IP (Caddy)

**Current path:** HTTPS on the WAN address, TLS on the laptop, apps stay on localhost ports.

| Public URL | Upstream on `192.168.100.17` |
|------------|------------------------------|
| `https://spaceage-pbem.duckdns.org` | game-host `127.0.0.1:8787` |
| `https://goodplays.duckdns.org` | GoodPlays API `127.0.0.1:5280` |

Caddy config: `C:\Users\akacz\caddy\Caddyfile`. DuckDNS for both names is `91.220.222.102`.

## Router (Huawei HS8145V)

**Forward Rules → Port Mapping Configuration**, WAN `1_INTERNET_R_VID_100`, internal host `192.168.100.17`:

| External TCP | Internal TCP | Why |
|--------------|--------------|-----|
| 80 | 80 | HTTP and certificate checks |
| 443 | 443 | HTTPS |
| 5280 | 5280 | GoodPlays API without TLS (phone check). Pages uses 443. |

Do not map **8787** or **5432**. Game-host is reached only through Caddy. Postgres stays on Docker on the laptop.

ngrok remains the fallback: [`hosted-beta-gm.md`](hosted-beta-gm.md) (`https://manatee-sabbath-kudos.ngrok-free.dev/client/`).

## Your network

| Setting | Value |
|---------|-------|
| PC (GM laptop) | `192.168.100.17` |
| Router | `192.168.100.1` |
| Public IP | `91.220.222.102` |
| Game-host | localhost `8787` (not on the WAN) |

Reserve `192.168.100.17` in DHCP so the mapping survives a reboot.

## Verify

From a phone on **mobile data** (not the home Wi‑Fi; this router does not loop the public IP back onto the LAN):

- `https://spaceage-pbem.duckdns.org/health` → `"ok": true`
- `https://spaceage-pbem.duckdns.org/client/` → login screen
- `https://goodplays.duckdns.org/health` → `Healthy`

If those fail and `http://localhost:8787/health` works, Caddy is down or the WAN forward is not reaching the laptop. Fallback:

```powershell
.\play\expose-game-host-ngrok.ps1
```

# Expose game-host on your public IP (alternative to ngrok)

**Primary path for open beta:** ngrok HTTPS tunnel — see [`hosted-beta-gm.md`](hosted-beta-gm.md) Scenario C
(`https://manatee-sabbath-kudos.ngrok-free.dev/client/`).

Use this document only if you prefer a **static IP + router port forward** instead of ngrok.

Local Docker and Windows Firewall are already correct. If `http://localhost:8787/health` works but
`http://91.220.222.102:8787/health` does not from outside your LAN, the missing piece is **router
port forwarding** (NAT).

## Your network (current)

| Setting | Value |
|---------|-------|
| PC (GM laptop) | `192.168.100.17` |
| Router | `192.168.100.1` |
| Public IP | `91.220.222.102` |
| Game-host port | `8787` |

## Router setup

1. Open the router admin UI (usually `http://192.168.100.1`).
2. Find **Port forwarding** / **Virtual server** / **NAT**.
3. Add a rule:

   | Field | Value |
   |-------|-------|
   | External port | `8787` |
   | Internal IP | `192.168.100.17` |
   | Internal port | `8787` |
   | Protocol | TCP (or TCP/UDP) |

4. Save and apply. Some routers require a reboot.
5. Reserve `192.168.100.17` for this PC (DHCP static lease) so the rule does not break after restart.

## Verify

From a phone on **mobile data** (not Wi‑Fi):

- `http://91.220.222.102:8787/health` → should return JSON with `"ok": true`
- `http://91.220.222.102:8787/client/` → visual client

## If it still fails

- **ISP CGNAT**: some residential plans do not allow inbound port forwarding. Ask your ISP for a public IP or use ngrok instead.
- **ISP blocks port 8787**: try forwarding external `8080` → internal `8787` and use `http://91.220.222.102:8080/...`.
- **Double NAT**: modem + router both doing NAT; forward on both or put the router in bridge mode.

## Preferred: ngrok (no router changes)

```powershell
.\play\expose-game-host-ngrok.ps1
```

Client URL: **https://manatee-sabbath-kudos.ngrok-free.dev/client/** — see [`hosted-beta-gm.md`](hosted-beta-gm.md).

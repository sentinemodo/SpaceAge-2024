#Requires -Version 5.1
<#
.SYNOPSIS
  Expose game-host on the public internet via ngrok (no router port forwarding).

.DESCRIPTION
  Starts an HTTPS tunnel to localhost:8787. Use the printed URL for remote players.
  Requires ngrok authtoken in %LOCALAPPDATA%\ngrok\ngrok.yml (ngrok config add-authtoken).
#>
param(
	[int] $Port = 0,
	[string] $Domain = 'manatee-sabbath-kudos.ngrok-free.dev'
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\_common.ps1"

Import-RepoEnv
if ($Port -le 0) {
	$Port = if ($env:GAME_HOST_PORT) { [int]$env:GAME_HOST_PORT } else { 8787 }
}
if ($env:NGROK_DOMAIN -and -not $PSBoundParameters.ContainsKey('Domain')) {
	$Domain = $env:NGROK_DOMAIN
}

$ngrokExe = Join-Path $env:APPDATA 'npm\node_modules\ngrok\bin\ngrok.exe'
if (-not (Test-Path -LiteralPath $ngrokExe)) {
	$ngrokExe = (Get-Command ngrok.exe -ErrorAction SilentlyContinue).Source
}
if (-not $ngrokExe -or -not (Test-Path -LiteralPath $ngrokExe)) {
	throw "ngrok not found. Install: npm install -g ngrok, then: ngrok config add-authtoken <token>"
}

try {
	Invoke-RestMethod -Uri "http://127.0.0.1:$Port/health" -TimeoutSec 5 | Out-Null
}
catch {
	throw "Game-host is not reachable on http://127.0.0.1:$Port. Start Docker first: docker compose up -d"
}

$existing = $null
try {
	$existing = Invoke-RestMethod -Uri 'http://127.0.0.1:4040/api/tunnels' -TimeoutSec 3
}
catch {
	# no local ngrok web UI yet
}

$tunnel = $existing.tunnels | Where-Object { $_.config.addr -match ":$Port`$" -or $_.config.addr -match ":$Port/" } | Select-Object -First 1
if ($tunnel) {
	Write-Host "ngrok already forwarding to port $Port."
	Write-Host "  Client: $($tunnel.public_url)/client/"
	Write-Host "  Health: $($tunnel.public_url)/health"
	Write-Host "  Inspect: http://127.0.0.1:4040"
	exit 0
}

Write-Host "Starting ngrok http $Port (domain: $Domain) ..."
Write-Host "  Client: https://$Domain/client/"
Write-Host "Press Ctrl+C to stop the tunnel."
Write-Host ""
if ($Domain) {
	& $ngrokExe http $Port --domain=$Domain
}
else {
	& $ngrokExe http $Port
}

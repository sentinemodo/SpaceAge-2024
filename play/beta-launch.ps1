# Open beta bootstrap — init game-host run, generate reports, publish status
param(
	[string]$RunId = 'beta-1',
	[string]$GameHostUrl = 'http://localhost:8787',
	[string]$GmKey = 'dev-gm-key'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Write-Host "=== SpaceAge Open Beta Launch ($RunId) ==="

# 1. Optional: local play init (creates passwords in gitignored play/runs)
if (-not (Test-Path (Join-Path $root "game-host\runs\$RunId\data\gamein.xml"))) {
	Write-Host "Initializing play run..."
	& (Join-Path $PSScriptRoot 'init-run.ps1') $RunId -Force
}

# 2. Game-host GM API (requires: cd game-host; npm start)
Write-Host "Calling game-host init (from play run passwords)..."
$initBody = @{ source = 'play'; runId = $RunId } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri "$GameHostUrl/api/gm/init" -Headers @{ 'X-GM-Key' = $GmKey; 'Content-Type' = 'application/json' } -Body $initBody

Write-Host "Generating reports..."
Invoke-RestMethod -Method Post -Uri "$GameHostUrl/api/gm/reports" -Headers @{ 'X-GM-Key' = $GmKey }

# 3. Status JSON for lobby
Write-Host "Updating website status..."
& (Join-Path $PSScriptRoot 'generate-status.ps1') $RunId

Write-Host ""
Write-Host "Beta ready:"
Write-Host "  Lobby: https://sentinemodo.github.io/SpaceAge-2024/"
Write-Host "  Client:  $GameHostUrl/client/"
Write-Host "  GM turn: Invoke-RestMethod -Method Post -Uri '$GameHostUrl/api/gm/turn' -Headers @{ 'X-GM-Key' = '$GmKey' }"
Write-Host ""
Write-Host "Invite players with faction id + password from play/runs/$RunId/data/gamein.xml"

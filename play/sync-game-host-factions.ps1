#Requires -Version 5.1
<#
.SYNOPSIS
  Mirror game-host isolated reports into play/runs for player-agent RAG ingest.

.DESCRIPTION
  After a GM turn or /reports, game-host writes faction reports under
  game-host/runs/<id>/factions/. Player-agent ingest reads play/runs/<id>/factions/.
  Run this on the GM laptop between game-host turns and ingest-rag.ps1.
#>
param(
	[string] $Run = 'beta-1'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$src = Join-Path $root "game-host\runs\$Run\factions"
$dst = Join-Path $root "play\runs\$Run\factions"

if (-not (Test-Path $src)) {
	throw "Missing $src — run game-host /reports or /turn first"
}

New-Item -ItemType Directory -Force -Path $dst | Out-Null
Copy-Item -Path (Join-Path $src '*') -Destination $dst -Recurse -Force
Write-Host "Synced faction reports: $src -> $dst"

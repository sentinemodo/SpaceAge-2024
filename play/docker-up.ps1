#Requires -Version 5.1
<#
.SYNOPSIS
  Build and start the game-host Docker stack on the GM laptop.

.DESCRIPTION
  Game.exe runs inside the container (Mono). Ollama and player-agent stay on the
  host — pull models before drafting:

    ollama pull qwen2.5-coder:7b
    ollama pull nomic-embed-text
#>
param(
	[switch] $Build,
	[switch] $Down
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
	if ($Down) {
		docker compose down
		return
	}
	$args = @('compose', 'up', '-d')
	if ($Build) { $args += '--build' }
	docker @args
	$port = if ($env:GAME_HOST_PORT) { $env:GAME_HOST_PORT } else { '8787' }
	Write-Host ""
	Write-Host "Game-host:  http://localhost:$port/client/"
	Write-Host "Health:     http://localhost:$port/health"
	Write-Host "Bootstrap:  .\play\beta-launch.ps1 -GameHostUrl http://localhost:$port"
	Write-Host ""
	Write-Host "Ollama (host): ollama pull qwen2.5-coder:7b && ollama pull nomic-embed-text"
	Write-Host "Player-agent runs on this machine — not in Docker."
}
finally {
	Pop-Location
}

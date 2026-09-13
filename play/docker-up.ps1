#Requires -Version 5.1
<#
.SYNOPSIS
  Build and start the game-host Docker stack on the GM laptop.

.DESCRIPTION
  Game.exe runs inside the container (Mono). Player-agent runs on the host and
  calls Ollama in Docker (port 11434). Ensure the ollama container is running:

    docker start ollama
    docker exec ollama ollama list
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
	Write-Host "Ollama Docker: docker start ollama  (http://127.0.0.1:11434)"
	Write-Host "Verify:       .\play\ollama-check.ps1"
	Write-Host "Player-agent runs on this machine — not in Docker."
}
finally {
	Pop-Location
}

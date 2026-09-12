#Requires -Version 5.1
<#
.SYNOPSIS
  Verify Ollama Docker is up and has the player-agent models.
#>
. (Join-Path $PSScriptRoot '_common.ps1')
Test-OllamaDocker

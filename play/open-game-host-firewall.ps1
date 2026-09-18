#Requires -Version 5.1
<#
.SYNOPSIS
  Allow inbound TCP to the game-host port (default 8787) on the GM laptop.

.DESCRIPTION
  New-NetFirewallRule requires an elevated PowerShell session. This script
  re-launches itself with UAC when needed, then creates or updates the rule.
#>
param(
	[int] $Port = 0
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\_common.ps1"

Import-RepoEnv
if ($Port -le 0) {
	$Port = if ($env:GAME_HOST_PORT) { [int]$env:GAME_HOST_PORT } else { 8787 }
}

$ruleName = 'SpaceAge game-host'

function Test-IsAdmin {
	$principal = New-Object Security.Principal.WindowsPrincipal(
		[Security.Principal.WindowsIdentity]::GetCurrent()
	)
	return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if (-not (Test-IsAdmin)) {
	Write-Host "Administrator approval required to change Windows Firewall."
	$argList = @(
		'-NoProfile',
		'-ExecutionPolicy', 'Bypass',
		'-File', ('"' + $MyInvocation.MyCommand.Path + '"'),
		'-Port', $Port
	)
	Start-Process -FilePath 'powershell.exe' -Verb RunAs -ArgumentList ($argList -join ' ') -Wait
	exit $LASTEXITCODE
}

$existing = Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue
if ($existing) {
	$filter = Get-NetFirewallPortFilter -AssociatedNetFirewallRule $existing
	if ($filter.LocalPort -eq "$Port") {
		if ($existing.Enabled -ne 'True') {
			Enable-NetFirewallRule -DisplayName $ruleName | Out-Null
			Write-Host "Enabled existing firewall rule '$ruleName' for TCP $Port."
		}
		else {
			Write-Host "Firewall rule '$ruleName' already allows inbound TCP $Port."
		}
		exit 0
	}
	Remove-NetFirewallRule -DisplayName $ruleName
}

New-NetFirewallRule `
	-DisplayName $ruleName `
	-Direction Inbound `
	-Protocol TCP `
	-LocalPort $Port `
	-Action Allow `
	-Profile Domain,Private,Public | Out-Null

Write-Host "Firewall rule '$ruleName' created: inbound TCP $Port allowed."

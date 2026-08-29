# Load the run and write starting (or current-turn) reports without Execute.
param(
	[Parameter(Mandatory = $true, Position = 0)]
	[string]$RunId,

	[string]$Exe
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')

$paths = Get-RunPaths -RunId $RunId
$dataXml = Join-Path $paths.DataDir 'data.xml'
$gamein = Join-Path $paths.DataDir 'gamein.xml'
if (-not (Test-Path -LiteralPath $dataXml)) {
	throw "Missing $dataXml. Run init-run.ps1 first."
}
if (-not (Test-Path -LiteralPath $gamein)) {
	throw "Missing $gamein. Run init-run.ps1 first."
}
if (-not (Test-Path -LiteralPath $paths.TurnDir)) {
	New-Item -ItemType Directory -Path $paths.TurnDir -Force | Out-Null
}

Invoke-GameExe -Exe $Exe -GameArgs @('/data', $paths.DataDir, '/turn-dir', $paths.TurnDir, '/reports')
Write-Host "Reports written under $($paths.TurnDir). Run isolate.ps1 to copy text reports into factions/."

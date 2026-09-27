# Clear stale orders, copy ten faction drafts into /turn-dir, run a full turn, then isolate.
param(
	[Parameter(Mandatory = $true, Position = 0)]
	[string]$RunId,

	[string]$Exe,

	# Passed to Game.exe as /gm-log (e.g. all, or research,battles,market). Falls back to $env:GM_LOG when omitted.
	[string]$GmLog
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')

$paths = Get-RunPaths -RunId $RunId
$gamein = Join-Path $paths.DataDir 'gamein.xml'
if (-not (Test-Path -LiteralPath $gamein)) {
	throw "Missing $gamein. Run init-run.ps1 first."
}
if (-not (Test-Path -LiteralPath $paths.TurnDir)) {
	New-Item -ItemType Directory -Path $paths.TurnDir -Force | Out-Null
}

$missing = @()
foreach ($id in $script:PlayerFactionIds) {
	$src = Resolve-FactionOrderSourcePath -FactionId $id -Paths $paths
	if (-not $src) {
		$folder = Join-Path $paths.FactionsDir (Get-FactionFolderName -Id $id)
		$missing += "faction $id (no orders.*.{turn}.{version}.txt and no order.$id.txt under $folder)"
	}
}
if ($missing.Count -gt 0) {
	throw "Missing faction orders (need all ten for factions 2-11):`n$($missing -join "`n")"
}

Get-ChildItem -LiteralPath $paths.TurnDir -Filter 'order.*' -File -ErrorAction SilentlyContinue |
	Remove-Item -Force

foreach ($id in $script:PlayerFactionIds) {
	$src = Resolve-FactionOrderSourcePath -FactionId $id -Paths $paths
	$dst = Join-Path $paths.TurnDir ("order.{0}.txt" -f $id)
	$text = Read-Utf8Text -Path $src
	Write-Win1251Text -Path $dst -Text $text
	Write-Host ("  order.{0}.txt <= {1}" -f $id, (Split-Path -Leaf $src))
}

$gameArgs = @('/data', $paths.DataDir, '/turn-dir', $paths.TurnDir)
$gmLogChannels = $GmLog
if ([string]::IsNullOrWhiteSpace($gmLogChannels)) {
	$gmLogChannels = $env:GM_LOG
}
if (-not [string]::IsNullOrWhiteSpace($gmLogChannels)) {
	$gameArgs += @('/gm-log', $gmLogChannels.Trim())
}
Invoke-GameExe -Exe $Exe -GameArgs $gameArgs

& (Join-Path $PSScriptRoot 'isolate.ps1') -RunId $RunId

foreach ($id in $script:PlayerFactionIds) {
	$draft = Join-Path (Join-Path $paths.FactionsDir (Get-FactionFolderName -Id $id)) ("order.{0}.txt" -f $id)
	if (Test-Path -LiteralPath $draft) {
		Remove-Item -LiteralPath $draft -Force
	}
}

Update-WebsiteStatus -RunId $RunId
Write-Host "Full turn complete for run '$RunId'. Isolate copied the latest text reports."

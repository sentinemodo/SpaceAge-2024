# Clear stale orders, copy ten faction drafts into /turn-dir, run a full turn, then isolate.
param(
	[Parameter(Mandatory = $true, Position = 0)]
	[string]$RunId,

	# Processing turn N: copy data/gameout.(N-1).xml -> gamein, use orders.*.(N-1).*, emit report.N.*
	[Parameter(Position = 1)]
	[int]$Turn,

	[string]$Exe,

	# Passed to Game.exe as /gm-log (e.g. all, or research,battles,market). Falls back to $env:GM_LOG when omitted.
	[string]$GmLog
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')

$paths = Get-RunPaths -RunId $RunId
if (-not (Test-Path -LiteralPath $paths.DataDir)) {
	throw "Missing $($paths.DataDir). Run init-run.ps1 first."
}
if (-not (Test-Path -LiteralPath $paths.TurnDir)) {
	New-Item -ItemType Directory -Path $paths.TurnDir -Force | Out-Null
}

if ($PSBoundParameters.ContainsKey('Turn')) {
	Install-GameinForProcessingTurn -Paths $paths -ProcessingTurn $Turn
	$paths.ProcessingTurn = $Turn
}

$gamein = Join-Path $paths.DataDir 'gamein.xml'
if (-not (Test-Path -LiteralPath $gamein)) {
	throw "Missing $gamein. Run init-run.ps1 first, or pass -Turn N to restore from gameout.(N-1).xml."
}

$turnPlan = Write-TurnExecutionBanner -RunId $RunId -Paths $paths
if (-not $PSBoundParameters.ContainsKey('Turn')) {
	Write-Host ('  Tip: pass processing turn explicitly, e.g. turn.ps1 {0} {1}' -f $RunId, $turnPlan.ProcessingTurn) -ForegroundColor DarkGray
}
elseif ($turnPlan.ProcessingTurn -ne $Turn) {
	throw "Internal turn plan mismatch (requested $Turn, got $($turnPlan.ProcessingTurn))."
}

$missing = @()
foreach ($id in $script:PlayerFactionIds) {
	$src = Resolve-FactionOrderSourcePath -FactionId $id -Paths $paths
	if (-not $src) {
		$missing += Get-FactionOrderMissingMessage -FactionId $id -Paths $paths
	}
}
if ($missing.Count -gt 0) {
	throw ('Cannot run turn - missing or wrong-bucket faction orders (need all ten for factions 2-11):' + "`n`n" + ($missing -join "`n`n"))
}

Get-ChildItem -LiteralPath $paths.TurnDir -Filter 'order.*' -File -ErrorAction SilentlyContinue |
	Remove-Item -Force

foreach ($id in $script:PlayerFactionIds) {
	$src = Resolve-FactionOrderSourcePath -FactionId $id -Paths $paths
	if (-not (Test-VersionedOrderPathMatchesTurn -Path $src -FactionId $id -ExpectedOrdersTurn $turnPlan.ExpectedOrdersTurn)) {
		$leaf = Split-Path -Leaf $src
		throw ('Cannot run turn - faction {0} resolved {1} but processing turn {2} requires orders.{0}.{3}.{{version}}.txt (gamein turn {3}).' -f $id, $leaf, $turnPlan.ProcessingTurn, $turnPlan.ExpectedOrdersTurn)
	}
	$dst = Join-Path $paths.TurnDir ("order.{0}.txt" -f $id)
	$text = Read-Utf8Text -Path $src
	Write-Win1251Text -Path $dst -Text $text
	Write-Host ('  order.{0}.txt <= {1}' -f $id, (Split-Path -Leaf $src))
}

$unSrc = Resolve-FactionOrderSourcePath -FactionId 1 -Paths $paths
if ($unSrc) {
	$unDst = Join-Path $paths.TurnDir 'order.1.txt'
	$unText = Read-Utf8Text -Path $unSrc
	Write-Win1251Text -Path $unDst -Text $unText
	Write-Host ('  order.1.txt <= {0}' -f (Split-Path -Leaf $unSrc))
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

& (Join-Path $PSScriptRoot 'isolate.ps1') -RunId $RunId -Turn $turnPlan.OutputReportTurn

foreach ($id in $script:PlayerFactionIds) {
	$draft = Join-Path (Join-Path $paths.FactionsDir (Get-FactionFolderName -Id $id)) ("order.{0}.txt" -f $id)
	if (Test-Path -LiteralPath $draft) {
		Remove-Item -LiteralPath $draft -Force
	}
}

Update-WebsiteStatus -RunId $RunId
Write-Host ('Full turn complete for run ''{0}'' (processing turn {1}, report.{1}.* isolated).' -f $RunId, $turnPlan.ProcessingTurn)

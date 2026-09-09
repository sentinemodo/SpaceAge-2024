# Copy text reports for factions 2-11 into their isolated folders. Never copy XML.
param(
	[Parameter(Mandatory = $true, Position = 0)]
	[string]$RunId,

	[int]$Turn
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')

$paths = Get-RunPaths -RunId $RunId
if (-not (Test-Path -LiteralPath $paths.TurnDir)) {
	throw "Missing turn dir $($paths.TurnDir). Run reports.ps1 first."
}

function Get-TurnFromReports {
	param([string]$TurnDir)
	$maxTurn = $null
	Get-ChildItem -LiteralPath $TurnDir -Filter 'report.*.*.txt' -File -ErrorAction SilentlyContinue | ForEach-Object {
		if ($_.Name -match '^report\.(\d+)\.(\d+)\.txt$') {
			$t = [int]$Matches[1]
			if ($null -eq $maxTurn -or $t -gt $maxTurn) {
				$maxTurn = $t
			}
		}
	}
	return $maxTurn
}

$resolvedTurn = $null
if ($PSBoundParameters.ContainsKey('Turn')) {
	$resolvedTurn = $Turn
}
else {
	$resolvedTurn = Get-TurnFromReports -TurnDir $paths.TurnDir
	if ($null -eq $resolvedTurn) {
		$resolvedTurn = Get-TurnFromGamein -GameinPath (Join-Path $paths.DataDir 'gamein.xml')
	}
	if ($null -eq $resolvedTurn) {
		throw "No report.*.*.txt under $($paths.TurnDir) and no <game turn=...> in data/gamein.xml. Run reports.ps1 first."
	}
}

$missing = @()
foreach ($id in $script:PlayerFactionIds) {
	$src = Join-Path $paths.TurnDir ("report.{0}.{1}.txt" -f $resolvedTurn, $id)
	if (-not (Test-Path -LiteralPath $src)) {
		$missing += $src
	}
}
if ($missing.Count -gt 0) {
	throw "Missing text reports for turn $resolvedTurn (factions 2-11):`n$($missing -join "`n")"
}

foreach ($id in $script:PlayerFactionIds) {
	$folder = Join-Path $paths.FactionsDir (Get-FactionFolderName -Id $id)
	if (-not (Test-Path -LiteralPath $folder)) {
		New-Item -ItemType Directory -Path $folder -Force | Out-Null
	}
	$src = Join-Path $paths.TurnDir ("report.{0}.{1}.txt" -f $resolvedTurn, $id)
	$dst = Join-Path $folder ("report.{0}.{1}.txt" -f $resolvedTurn, $id)
	Copy-Item -LiteralPath $src -Destination $dst -Force
}

Write-Host "Isolated text reports for turn $resolvedTurn into factions/02 .. 11 (no XML)."

Update-WebsiteStatus -RunId $RunId

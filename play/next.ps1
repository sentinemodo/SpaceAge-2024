# Promote data/gameout.{N}.xml to data/gamein.xml for the next exe run.
param(
	[Parameter(Mandatory = $true, Position = 0)]
	[string]$RunId,

	[int]$Turn
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')

$paths = Get-RunPaths -RunId $RunId
if (-not (Test-Path -LiteralPath $paths.DataDir)) {
	throw "Missing $($paths.DataDir). Run init-run.ps1 first."
}

function Get-LatestGameoutTurn {
	param([string]$DataDir)
	$maxTurn = $null
	Get-ChildItem -LiteralPath $DataDir -Filter 'gameout.*.xml' -File -ErrorAction SilentlyContinue | ForEach-Object {
		if ($_.Name -match '^gameout\.(\d+)\.xml$') {
			$t = [int]$Matches[1]
			if ($null -eq $maxTurn -or $t -gt $maxTurn) {
				$maxTurn = $t
			}
		}
	}
	return $maxTurn
}

$n = $null
if ($PSBoundParameters.ContainsKey('Turn')) {
	$n = $Turn
}
else {
	$n = Get-LatestGameoutTurn -DataDir $paths.DataDir
	if ($null -eq $n) {
		throw "No data/gameout.{N}.xml under $($paths.DataDir). Run turn.ps1 first."
	}
}

$src = Join-Path $paths.DataDir ("gameout.{0}.xml" -f $n)
$dst = Join-Path $paths.DataDir 'gamein.xml'
if (-not (Test-Path -LiteralPath $src)) {
	throw "Missing $src (requested turn $n)."
}

Copy-Item -LiteralPath $src -Destination $dst -Force
Write-Host "Copied $src -> $dst (keep all gameout.*). After a full turn from seed 1, N is 2."

Update-WebsiteStatus -RunId $RunId

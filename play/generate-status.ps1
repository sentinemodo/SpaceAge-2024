param(
	[Parameter(Mandatory=$true, Position=0)][string]$RunId,
	[string]$OutPath
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')

if ([string]::IsNullOrWhiteSpace($OutPath)) {
	$OutPath = Join-Path $script:RepoRoot 'website\public\status.json'
}

$paths = Get-RunPaths -RunId $RunId
$gamein = Join-Path $paths.DataDir 'gamein.xml'
$status = 'not-started'
$turn = 0
$nextTurnAt = $null

if (Test-Path -LiteralPath $gamein) {
	$gameinText = Read-Win1251Text -Path $gamein
	$m = [System.Text.RegularExpressions.Regex]::Match($gameinText, '<turn>\s*(\d+)\s*</turn>')
	if ($m.Success) { $turn = [int]$m.Groups[1].Value }
	if ($turn -eq 0) { $turn = 1 }

	# Determine submission state from turn dir order files
	$factions = @()
	foreach ($id in $script:PlayerFactionIds) {
		$orderPath = Join-Path $paths.TurnDir ("order.$id.txt")
		$submitted = Test-Path -LiteralPath $orderPath
		$factions += @{ id = $id; submitted = $submitted; name = ("Faction {0}" -f $id) }
	}

	if ($factions | Where-Object { -not $_.submitted }) {
		$status = 'accepting-orders'
	}
	else {
		$status = 'processing'
	}
}
else {
	$factions = @()
	foreach ($id in $script:PlayerFactionIds) {
		$factions += @{ id = $id; submitted = $false; name = ("Faction {0}" -f $id) }
	}
}

# Build object
$obj = @{ 
	status = $status;
	turn = $turn;
	nextTurnAt = $nextTurnAt;
	factions = $factions
}

$json = $obj | ConvertTo-Json -Depth 4 -Compress

# Ensure output folder exists
$dir = Split-Path -Parent $OutPath
if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }

Write-Utf8Text -Path $OutPath -Text $json
Write-Host "Wrote status.json to $OutPath"

param(
	[Parameter(Mandatory=$true, Position=0)][string]$RunId,
	[string]$OutPath,
	[string]$NextTurnAt
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

$schedulePath = Join-Path $paths.RunRoot 'gm\schedule.json'
if (Test-Path -LiteralPath $schedulePath) {
	try {
		$sched = Get-Content -LiteralPath $schedulePath -Raw -Encoding UTF8 | ConvertFrom-Json
		if ($null -ne $sched.nextTurnAt) {
			$raw = $sched.nextTurnAt
			if ($raw -is [datetime]) {
				$nextTurnAt = $raw.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
			}
			else {
				$nextTurnAt = [string]$raw
			}
		}
	}
	catch {
		Write-Warning "Could not parse ${schedulePath}: $_"
	}
}

if ($PSBoundParameters.ContainsKey('NextTurnAt')) {
	$nextTurnAt = $NextTurnAt
}

$factions = @()
foreach ($id in $script:PlayerFactionIds) {
	$factions += @{ id = $id; submitted = $false; name = ("Faction {0}" -f $id) }
}

if (Test-Path -LiteralPath $gamein) {
	$parsedTurn = Get-TurnFromGamein -GameinPath $gamein
	if ($null -ne $parsedTurn -and $parsedTurn -gt 0) {
		$turn = $parsedTurn
	}
	else {
		$turn = 1
	}

	$draftCount = 0
	for ($i = 0; $i -lt $factions.Count; $i++) {
		$id = $factions[$i].id
		$draftPath = Join-Path (Join-Path $paths.FactionsDir (Get-FactionFolderName -Id $id)) ("order.{0}.txt" -f $id)
		$submitted = Test-Path -LiteralPath $draftPath
		$factions[$i].submitted = $submitted
		if ($submitted) { $draftCount++ }
	}

	$reportTurn = Get-LatestIsolatedReportTurn -FactionsDir $paths.FactionsDir
	$reportsOut = $null -ne $reportTurn -and (Test-AllIsolatedReportsPresent -FactionsDir $paths.FactionsDir -Turn $reportTurn)

	if ($draftCount -eq $script:PlayerFactionIds.Count) {
		$status = 'processing'
	}
	elseif ($draftCount -gt 0) {
		$status = 'accepting-orders'
	}
	elseif ($reportsOut) {
		$status = 'reports-out'
	}
	else {
		$status = 'not-started'
	}
}

$obj = @{
	status = $status
	turn = $turn
	nextTurnAt = $nextTurnAt
	factions = $factions
}

$json = $obj | ConvertTo-Json -Depth 4 -Compress

$dir = Split-Path -Parent $OutPath
if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }

Write-Utf8Text -Path $OutPath -Text $json
Write-Host "Wrote status.json to $OutPath (status=$status, turn=$turn)"

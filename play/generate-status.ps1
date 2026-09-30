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
		if ($null -ne $sched.nextTurnAt -and -not [string]::IsNullOrWhiteSpace([string]$sched.nextTurnAt)) {
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

if ($PSBoundParameters.ContainsKey('NextTurnAt') -and -not [string]::IsNullOrWhiteSpace($NextTurnAt)) {
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
		$submitted = Test-FactionOrdersSubmitted -FactionId $id -Turn $turn -Paths $paths
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

if ([string]::IsNullOrWhiteSpace([string]$nextTurnAt)) {
	$nextTurnAt = $null
}

function Get-PublicIssuesFromRun {
	param(
		[string]$RunRoot,
		[string]$GameinText
	)
	$issues = New-Object System.Collections.Generic.List[object]
	$gmDir = Join-Path $RunRoot 'gm'
	if (-not (Test-Path -LiteralPath $gmDir)) {
		return @()
	}
	Get-ChildItem -LiteralPath $gmDir -Filter 'issues-turn-*.json' | Sort-Object Name | ForEach-Object {
		$doc = Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8 | ConvertFrom-Json
		foreach ($item in $doc.issues) {
			$row = @{
				turn = [int]$doc.turn
				no = [int]$item.no
				kind = [string]$item.kind
				planetId = [string]$item.planetId
			}
			if ($item.kind -eq 'rumor') {
				$row.title = [string]$item.title
				$row.flavour = [string]$item.flavour
			}
			elseif ($item.kind -eq 'contract') {
				$cid = [string]$item.contractId
				$row.contractId = $cid
				if ($item.PSObject.Properties['issuer']) {
					$row.issuer = [int]$item.issuer
				}
				$pattern = '<contract\s+name="' + [regex]::Escape($cid) + '"[^>]*/>'
				$m = [regex]::Match($GameinText, $pattern)
				if ($m.Success) {
					$block = $m.Value
					$row.title = ([regex]::Match($block, 'title="([^"]*)"')).Groups[1].Value
					$row.flavour = ([regex]::Match($block, 'flavour="([^"]*)"')).Groups[1].Value
				}
			}
			$issues.Add($row)
		}
	}
	return $issues.ToArray()
}

$gameinText = ''
if (Test-Path -LiteralPath $gamein) {
	$gameinText = [System.IO.File]::ReadAllText($gamein, [System.Text.Encoding]::GetEncoding(1251))
}

$obj = @{
	status = $status
	turn = $turn
	nextTurnAt = $nextTurnAt
	factions = $factions
	# @() keeps ConvertTo-Json emitting [] when the function returns an empty array (otherwise {}).
	issues = @(Get-PublicIssuesFromRun -RunRoot $paths.RunRoot -GameinText $gameinText)
}

$json = $obj | ConvertTo-Json -Depth 4 -Compress

$dir = Split-Path -Parent $OutPath
if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }

Write-Utf8Text -Path $OutPath -Text $json
Write-Host "Wrote status.json to $OutPath (status=$status, turn=$turn)"

#Requires -Version 5.1
param(
	[Parameter(Mandatory = $true)][string]$Run,
	[int]$FromFaction = 4,
	[int]$ToFaction = 11,
	[switch]$DryRun,
	[switch]$RepairOnly,
	[switch]$ContractorOnly,
	[switch]$EconomicOnly,
	[switch]$MilitaryOnly,
	[switch]$RecreateStrategic,
	[int[]]$OnlyFaction = @(),
	[string]$LogFileName = 'draft-stories.log'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')
Initialize-PlayRunLog -RunId $Run -LogFileName $LogFileName
$script:StoryFactionIds = @()
if ($ContractorOnly) {
	$script:StoryFactionIds = Get-ContractorFactionIds -RunId $Run
	if ($script:StoryFactionIds.Count -eq 0) { throw "No contractor factions found under run $Run" }
}
elseif ($EconomicOnly) {
	$script:StoryFactionIds = Get-EconomicFactionIds -RunId $Run
	if ($script:StoryFactionIds.Count -eq 0) { throw "No economic factions found under run $Run" }
}
elseif ($MilitaryOnly) {
	$script:StoryFactionIds = Get-MilitaryFactionIds -RunId $Run
	if ($script:StoryFactionIds.Count -eq 0) { throw "No military factions found under run $Run" }
}
else {
	for ($i = $FromFaction; $i -le $ToFaction; $i++) { $script:StoryFactionIds += $i }
}
if ($OnlyFaction.Count -gt 0) {
	$script:StoryFactionIds = @($script:StoryFactionIds | Where-Object { $OnlyFaction -contains $_ })
	if ($script:StoryFactionIds.Count -eq 0) { throw "No factions left after -OnlyFaction filter ($($OnlyFaction -join ','))" }
}
Write-PlayRunLog "draft-stories-seq run=$Run factions=$($script:StoryFactionIds -join ',') dryRun=$DryRun repairOnly=$RepairOnly contractorOnly=$ContractorOnly economicOnly=$EconomicOnly militaryOnly=$MilitaryOnly recreateStrategic=$RecreateStrategic onlyFaction=$($OnlyFaction -join ',')"
if (-not $DryRun -and -not $RepairOnly) { Test-OllamaDocker }

$repoRoot = $script:RepoRoot
$project = Join-Path $repoRoot 'tools/player-agent/PlayerAgent.csproj'
$paths = Get-RunPaths -RunId $Run

function Get-StoryPathForTurn {
	param([string]$FactionDir, [int]$FactionId, [int]$Turn)
	Join-Path $FactionDir ("story.{0}.{1}.md" -f $FactionId, $Turn)
}

function Get-ReportTurnForFaction {
	param([string]$FactionDir, [int]$FactionId)
	$reportPath = Join-Path $FactionDir ("report.2.{0}.txt" -f $FactionId)
	if (-not (Test-Path -LiteralPath $reportPath)) { return 2 }
	$base = [System.IO.Path]::GetFileNameWithoutExtension($reportPath)
	if ($base -match '^report\.(\d+)\.') { return [int]$Matches[1] }
	return 2
}

function Get-PriorStoryPathForContinuity {
	param([string]$FactionDir, [int]$FactionId, [int]$ReportTurn)
	for ($t = $ReportTurn - 1; $t -ge 1; $t--) {
		$p = Get-StoryPathForTurn -FactionDir $FactionDir -FactionId $FactionId -Turn $t
		if (Test-Path -LiteralPath $p) { return $p }
	}
	$legacy = Join-Path $FactionDir 'story.md'
	if (Test-Path -LiteralPath $legacy) { return $legacy }
	return $null
}

function Get-StrategicBlock {
	param([string]$StoryPath)
	if (-not (Test-Path -LiteralPath $StoryPath)) { return $null }
	$text = Get-Content -LiteralPath $StoryPath -Raw -Encoding UTF8
	if ($text -match '(?s)(## Strategic objective[^\r\n]*\r?\n.*?)(?=\r?\n## )') {
		return $Matches[1].TrimEnd()
	}
	if ($text -match '(?s)(## Strategic Objective[^\r\n]*\r?\n.*?)(?=\r?\n## )') {
		return $Matches[1].TrimEnd()
	}
	return $null
}

function Get-PriorStoryExcerpt {
	param([string]$StoryPath)
	if (-not (Test-Path -LiteralPath $StoryPath)) { return '' }
	$text = Get-Content -LiteralPath $StoryPath -Raw -Encoding UTF8
	$text = $text -replace '(?s)## Strategic objective.*?(?=\r?\n## )', ''
	$text = $text -replace '(?s)## Strategic Objective.*?(?=\r?\n## )', ''
	$lines = @(
		($text -split '\r?\n') |
			Where-Object { $_.Trim().Length -gt 0 } |
			ForEach-Object { $_.TrimEnd() }
	)
	$take = [Math]::Min(55, $lines.Count)
	if ($take -le 0) { return '' }
	return ($lines | Select-Object -First $take) -join "`r`n"
}

function Build-MergedReportPath {
	param([string]$FactionDir, [int]$FactionId, [string]$StoryPath)
	$report = Join-Path $FactionDir ("report.2.{0}.txt" -f $FactionId)
	if (-not (Test-Path -LiteralPath $report)) {
		throw "Missing $report"
	}
	$enc = [System.Text.Encoding]::GetEncoding(1251)
	$reportLines = [System.IO.File]::ReadAllLines($report, $enc)
	$keyLines = $reportLines | Where-Object {
		$_ -match 'CT\d{4}|Rumors?:|Hostile fauna|Contract|cleared|destroy|\[\d{6}\]|UN colony|region-presence|Between-turn|Battles report|Battle has commenced|Battle won by|owned by.*Fauna'
	} | Select-Object -Unique
	$headLines = $reportLines | Select-Object -First 50

	$sb = New-Object System.Text.StringBuilder
	[void]$sb.AppendLine('Between-turn releases (faction-visible):')
	$rumors = Get-ChildItem -LiteralPath $FactionDir -Filter 'rumor.2.*.txt' -ErrorAction SilentlyContinue | Sort-Object { [int]($_.BaseName -replace '^rumor\.2\.', '') }
	foreach ($file in $rumors) {
		[void]$sb.AppendLine("--- $($file.Name) ---")
		[void]$sb.Append([System.IO.File]::ReadAllText($file.FullName, $enc))
		[void]$sb.AppendLine()
	}
	$prior = Get-PriorStoryExcerpt -StoryPath $StoryPath
	if ($prior) {
		[void]$sb.AppendLine('Prior story excerpt (continuity; do not replace four-quarter strategic):')
		[void]$sb.AppendLine($prior)
		[void]$sb.AppendLine()
	}
	[void]$sb.AppendLine('Report highlights:')
	foreach ($line in $keyLines) { [void]$sb.AppendLine($line) }
	[void]$sb.AppendLine('Report opening:')
	foreach ($line in $headLines) { [void]$sb.AppendLine($line) }

	$tempDir = Join-Path $env:TEMP "player-agent-$Run"
	if (-not (Test-Path -LiteralPath $tempDir)) {
		New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
	}
	$out = Join-Path $tempDir ("report.2.{0}.txt" -f $FactionId)
	[System.IO.File]::WriteAllText($out, $sb.ToString(), [System.Text.UTF8Encoding]::new($false))
	return $out
}

function Ensure-StoryTitle {
	param([string]$StoryPath, [int]$ReportTurn)
	if ($StoryPath -notmatch '\\factions\\(\d{2})\\') { return }
	$personaPath = Join-Path (Split-Path -Path $StoryPath -Parent) 'persona.md'
	$name = $null
	if (Test-Path -LiteralPath $personaPath) {
		$first = Get-Content -LiteralPath $personaPath -First 1 -Encoding UTF8
		if ($first -match '\(([^\)]+)\)\s*$') { $name = $Matches[1].Trim() }
	}
	if (-not $name) { return }
	$text = Get-Content -LiteralPath $StoryPath -Raw -Encoding UTF8
	if ($text -match '(?m)^#\s+.+\s+-\s+turn\s+\d+\s*$') { return }
	$title = "# $name - turn $ReportTurn"
	$text = $text -replace '(?m)^## Turn priority for turn \d+\s*$', '## Turn priority'
	[System.IO.File]::WriteAllText($StoryPath, ($title + "`r`n`r`n" + $text.TrimStart()), [System.Text.UTF8Encoding]::new($false))
}

function Repair-StoryHeadings {
	param([string]$StoryPath)
	$text = Get-Content -LiteralPath $StoryPath -Raw -Encoding UTF8
	$text = $text -replace '(?m)^## Turn priority for turn \d+\s*$', '## Turn priority'
	$text = $text -replace '(?m)^## Tactical objective\s*\r?\n\r?\n## Tactical objective', '## Tactical objective'
	$text = $text -replace '## Tactical objective \(bullet list[^\)]*\)', '## Tactical objective'
	$text = $text -replace '(?m)^## Tactical objective:\s*$', '## Tactical objective'
	if ($text -notmatch '(?m)^## Narrative\s*$') {
		$text = $text -replace '(?s)(\r?\n)((?:The|On) [A-Z][^\r\n]{40,}.*)$', "`r`n`r`n## Narrative`r`n`r`n`$2"
	}
	if ($text -match '(?s)(## Strategic[^\r\n]*\r?\n.*?)(\r?\n- [^\r\n]+(?:\r?\n- [^\r\n]+)+)(\r?\n\r?\n(?:## Narrative|The ))') {
		$mid = $Matches[2]
		if ($mid -notmatch '(?m)^## Tactical') {
			$text = $text -replace [regex]::Escape($mid), ("`r`n`r`n## Tactical objective" + $mid)
		}
	}
	[System.IO.File]::WriteAllText($StoryPath, $text.TrimEnd() + "`r`n", [System.Text.UTF8Encoding]::new($false))
}

if ($RepairOnly) {
	$repairIds = if ($ContractorOnly) { Get-ContractorFactionIds -RunId $Run }
		elseif ($EconomicOnly) { Get-EconomicFactionIds -RunId $Run }
		elseif ($MilitaryOnly) { Get-MilitaryFactionIds -RunId $Run }
		else { @($FromFaction..$ToFaction) }
	foreach ($factionId in $repairIds) {
		$factionDir = Join-Path $paths.FactionsDir ('{0:D2}' -f $factionId)
		$turn = Get-ReportTurnForFaction -FactionDir $factionDir -FactionId $factionId
		$toRepair = @(
			Get-StoryPathForTurn -FactionDir $factionDir -FactionId $factionId -Turn $turn
		)
		$legacy = Join-Path $factionDir 'story.md'
		if (Test-Path -LiteralPath $legacy) { $toRepair += $legacy }
		Get-ChildItem -LiteralPath $factionDir -Filter ("story.{0}.*.md" -f $factionId) -ErrorAction SilentlyContinue | ForEach-Object { $toRepair += $_.FullName }
		foreach ($storyPath in ($toRepair | Select-Object -Unique)) {
			if (-not (Test-Path -LiteralPath $storyPath)) { continue }
			Ensure-StoryTitle -StoryPath $storyPath -ReportTurn $turn
			Repair-StoryHeadings -StoryPath $storyPath
		}
	}
	Write-PlayRunLog "Repaired headings: factions $FromFaction..$ToFaction" -Color Green
	return
}

function Merge-PreservedStrategic {
	param([string]$StoryPath, [string]$StrategicBlock)
	$new = Get-Content -LiteralPath $StoryPath -Raw -Encoding UTF8
	if ([string]::IsNullOrWhiteSpace($StrategicBlock)) { return }
	if ($new -notmatch '(?s)^(?<head># .+?\r?\n\r?\n)(?<body>.*)$') { return }
	$body = $Matches['body'].TrimStart()
	$turnPriority = $null
	if ($body -match '(?s)^(## Turn priority.*?)(?=\r?\n## )') {
		$turnPriority = $Matches[1].TrimEnd()
		$body = ($body -replace '(?s)^## Turn priority.*?(?=\r?\n## )', '').TrimStart()
	}
	$merged = $Matches['head']
	if ($turnPriority) {
		$merged += $turnPriority + "`r`n`r`n"
	}
	$merged += $StrategicBlock + "`r`n`r`n" + $body
	[System.IO.File]::WriteAllText($StoryPath, $merged, [System.Text.UTF8Encoding]::new($false))
}

foreach ($factionId in $script:StoryFactionIds) {
	$factionDir = Join-Path $paths.FactionsDir ('{0:D2}' -f $factionId)
	$reportTurn = Get-ReportTurnForFaction -FactionDir $factionDir -FactionId $factionId
	$storyPath = Get-StoryPathForTurn -FactionDir $factionDir -FactionId $factionId -Turn $reportTurn
	$priorStoryPath = Get-PriorStoryPathForContinuity -FactionDir $factionDir -FactionId $factionId -ReportTurn $reportTurn
	$strategicSource = if ($priorStoryPath) { $priorStoryPath } elseif (Test-Path -LiteralPath $storyPath) { $storyPath } else { $null }
	$strategic = if ($strategicSource) { Get-StrategicBlock -StoryPath $strategicSource } else { $null }
	$excerptStoryPath = if ($priorStoryPath) { $priorStoryPath } else { $storyPath }
	$mergedReport = Build-MergedReportPath -FactionDir $factionDir -FactionId $factionId -StoryPath $excerptStoryPath

	Write-PlayRunLog "=== draft-story faction $factionId ===" -Color Cyan
	$args = @(
		'run', '--project', $project, '--',
		'draft-story',
		'--run', $Run,
		'--faction', $factionId,
		'--report', $mergedReport,
		'--chunked',
		'--yes'
	)
	if (-not $RecreateStrategic) { $args += '--omit-strategic' }
	if ($DryRun) { $args += '--dry-run' }

	& dotnet @args
	if ($LASTEXITCODE -ne 0) {
		throw "draft-story failed for faction $factionId (exit $LASTEXITCODE)"
	}

	if (-not $DryRun -and $strategic -and -not $RecreateStrategic) {
		Merge-PreservedStrategic -StoryPath $storyPath -StrategicBlock $strategic
		Write-PlayRunLog "Preserved turn-1 strategic block in $storyPath"
	}
	if (-not $DryRun) {
		Ensure-StoryTitle -StoryPath $storyPath -ReportTurn $reportTurn
		Repair-StoryHeadings -StoryPath $storyPath
	}
}

Write-PlayRunLog "Done: factions $($script:StoryFactionIds -join ', ')" -Color Green

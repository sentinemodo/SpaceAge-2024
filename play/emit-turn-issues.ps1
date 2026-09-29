param(
	[Parameter(Mandatory = $true, Position = 0)][string]$RunId,
	[int]$Turn = 0,
	[string]$IssuesPath
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')

$paths = Get-RunPaths -RunId $RunId
$gamein = Join-Path $paths.DataDir 'gamein.xml'

if ([string]::IsNullOrWhiteSpace($IssuesPath)) {
	$parsedTurn = Get-TurnFromGamein -GameinPath $gamein
	if ($Turn -gt 0) { $parsedTurn = $Turn }
	$IssuesPath = Join-Path $paths.RunRoot ("gm\issues-turn-{0}.json" -f $parsedTurn)
}

if (-not (Test-Path -LiteralPath $IssuesPath)) {
	throw "Issues file not found: $IssuesPath"
}

$issuesDoc = Get-Content -LiteralPath $IssuesPath -Raw -Encoding UTF8 | ConvertFrom-Json
$turn = [int]$issuesDoc.turn
if ($Turn -gt 0) { $turn = $Turn }

$enc1251 = [System.Text.Encoding]::GetEncoding(1251)
$gameinText = [System.IO.File]::ReadAllText($gamein, $enc1251)

function Get-ContractBlock([string]$contractId) {
	$pattern = '<contract\s+name="' + [regex]::Escape($contractId) + '"[^/]*/>'
	$m = [regex]::Match($gameinText, $pattern)
	if (-not $m.Success) {
		$pattern = '<contract\s+name="' + [regex]::Escape($contractId) + '"[\s\S]*?</contract>'
		$m = [regex]::Match($gameinText, $pattern)
	}
	if (-not $m.Success) { return $null }
	return $m.Value
}

function Format-RumorIssue([object]$issue) {
	$lines = New-Object System.Collections.Generic.List[string]
	$lines.Add('Rumors:')
	$lines.Add(('  {0}: {1}.' -f $issue.planetId, $issue.title))
	$lines.Add(('    {0}' -f $issue.flavour))
	return ($lines -join "`r`n")
}

function Format-ContractIssue([string]$contractXml) {
	$title = ([regex]::Match($contractXml, 'title="([^"]*)"')).Groups[1].Value
	$flavour = ([regex]::Match($contractXml, 'flavour="([^"]*)"')).Groups[1].Value
	$name = ([regex]::Match($contractXml, 'name="(CT[^"]*)"')).Groups[1].Value
	$lines = New-Object System.Collections.Generic.List[string]
	$lines.Add('A new contract has been published:')
	$lines.Add(("  {0}: {1}." -f $name, $title))
	if ($flavour) { $lines.Add(("    {0}" -f $flavour)) }
	return ($lines -join "`r`n")
}

foreach ($issue in $issuesDoc.issues) {
	$issueNo = [int]$issue.no
	$body = $null
	if ($issue.kind -eq 'rumor') {
		$body = Format-RumorIssue $issue
	}
	elseif ($issue.kind -eq 'contract') {
		$block = Get-ContractBlock $issue.contractId
		if (-not $block) { throw "Contract $($issue.contractId) not found in gamein.xml" }
		$body = Format-ContractIssue $block
	}
	else {
		throw "Unknown issue kind: $($issue.kind)"
	}

	foreach ($factionId in $issue.factions) {
		$dir = Join-Path $paths.FactionsDir ('{0:D2}' -f [int]$factionId)
		if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
		$outPath = Join-Path $dir ("rumor.{0}.{1}.txt" -f $turn, $issueNo)
		[System.IO.File]::WriteAllText($outPath, $body + "`r`n", $enc1251)
	}
}

Write-Host "Emitted turn $turn issues from $IssuesPath to faction rumor.* files."

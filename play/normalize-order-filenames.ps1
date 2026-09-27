#Requires -Version 5.1
<#
.SYNOPSIS
  Rename versioned order files to orders.{id}.{reportTurn}.{version}.txt using file timestamps for version order.

.DESCRIPTION
  Does not change file contents. Merges legacy off-by-one names (middle segment = reportTurn + 1 from
  player-agent) into the report turn bucket when that is the only report on disk. Renumbers versions 1..N
  by LastWriteTimeUtc within each (faction, reportTurn) group.

.PARAMETER Run
  Campaign run id under play/runs/<id>/.

.PARAMETER FromFaction
  First player faction (default 2).

.PARAMETER ToFaction
  Last player faction (default 11).

.PARAMETER DryRun
  Print planned renames only.
#>
[CmdletBinding()]
param(
	[Parameter(Mandatory = $true)]
	[string] $Run,

	[int] $FromFaction = 2,
	[int] $ToFaction = 11,
	[switch] $DryRun
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')
$paths = Get-RunPaths -RunId $Run

function Get-MaxReportTurn {
	param([string]$Folder, [int]$FactionId)
	$max = 0
	foreach ($file in Get-ChildItem -LiteralPath $Folder -Filter "report.*.$FactionId.txt" -File -ErrorAction SilentlyContinue) {
		if ($file.Name -match '^report\.(\d+)\.') {
			$t = [int]$Matches[1]
			if ($t -gt $max) { $max = $t }
		}
	}
	return $max
}

function Get-CanonicalReportTurn {
	param([int]$FileTurn, [int]$MaxReport)
	if ($MaxReport -le 0) { return $FileTurn }
	if ($FileTurn -eq ($MaxReport + 1)) { return $MaxReport }
	return $FileTurn
}

foreach ($factionId in $FromFaction..$ToFaction) {
	$folder = Join-Path $paths.FactionsDir (Get-FactionFolderName -Id $factionId)
	if (-not (Test-Path -LiteralPath $folder)) { continue }

	$maxReport = Get-MaxReportTurn -Folder $folder -FactionId $factionId
	$entries = @()
	foreach ($file in Get-ChildItem -LiteralPath $folder -Filter 'orders.*.txt' -File -ErrorAction SilentlyContinue) {
		if ($file.Name -notmatch '^orders\.(\d+)\.(\d+)\.(\d+)\.txt$') { continue }
		$fid = [int]$Matches[1]
		if ($fid -ne $factionId) { continue }
		$fileTurn = [int]$Matches[2]
		$canonical = Get-CanonicalReportTurn -FileTurn $fileTurn -MaxReport $maxReport
		$entries += [PSCustomObject]@{
			Path         = $file.FullName
			OldName      = $file.Name
			ReportTurn   = $canonical
			OldVersion   = [int]$Matches[3]
			SortTime     = $file.LastWriteTimeUtc
		}
	}

	if ($entries.Count -eq 0) { continue }

	$plans = @()
	foreach ($group in ($entries | Group-Object ReportTurn)) {
		$reportT = [int]$group.Name
		$sorted = $group.Group | Sort-Object SortTime, OldVersion, OldName
		$version = 0
		foreach ($item in $sorted) {
			$version++
			$newName = "orders.$factionId.$reportT.$version.txt"
			if ($item.OldName -ne $newName) {
				$plans += [PSCustomObject]@{
					From = $item.Path
					To   = Join-Path $folder $newName
					Old  = $item.OldName
					New  = $newName
				}
			}
		}
	}

	if ($plans.Count -eq 0) {
		Write-Host "Faction $factionId`: already normalized."
		continue
	}

	Write-Host "Faction $factionId`: $($plans.Count) rename(s)"
	foreach ($plan in $plans) {
		Write-Host "  $($plan.Old) -> $($plan.New)"
	}

	if ($DryRun) { continue }

	$stamp = Get-Date -Format 'yyyyMMddHHmmss'
	$temp = @()
	foreach ($i in 0..($plans.Count - 1)) {
		$tmp = Join-Path $folder "__norm_${stamp}_$i.txt"
		Move-Item -LiteralPath $plans[$i].From -Destination $tmp -Force
		$temp += [PSCustomObject]@{ Tmp = $tmp; Final = $plans[$i].To }
	}
	foreach ($row in $temp) {
		Move-Item -LiteralPath $row.Tmp -Destination $row.Final -Force
	}
}

Write-Host "Done."

#Requires -Version 5.1
<#
.SYNOPSIS
  Draft turn orders for AI contractor seats (Preference: contractor), excluding human seats.

.DESCRIPTION
  Refreshes RAG per contractor faction, then runs player-agent draft with staged + quality retries.
  Logs to play/runs/<id>/gm/draft-contractor-orders.log with timestamped lines.
#>
[CmdletBinding()]
param(
	[Parameter(Mandatory = $true)][string]$Run,
	[ValidateSet('test', 'campaign')][string]$Mode = 'campaign',
	[int[]]$ExcludeFaction = @(2, 3),
	[int]$FromFaction = 2,
	[int]$ToFaction = 11,
	[switch]$DryRun,
	[switch]$AllowRunPod,
	[switch]$SkipIngest,
	[int]$MaxQualityAttempts = 7,
	[switch]$Staged,
	[int]$Limit = 0,
	[string]$LogFileName = 'draft-contractor-orders.log'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')
Initialize-PlayRunLog -RunId $Run -LogFileName $LogFileName

$factionIds = Get-ContractorFactionIds -RunId $Run -ExcludeFaction $ExcludeFaction -FromFaction $FromFaction -ToFaction $ToFaction
if ($Limit -gt 0) {
	$factionIds = $factionIds | Select-Object -First $Limit
}
if ($factionIds.Count -eq 0) {
	Write-PlayRunLog "No contractor factions found (exclude: $($ExcludeFaction -join ','))." -Color Yellow
	exit 0
}

Write-PlayRunLog "Contractor order draft run=$Run mode=$Mode factions=$($factionIds -join ',') exclude=$($ExcludeFaction -join ',')"
if (-not $DryRun) { Test-OllamaDocker }

$repoRoot = $script:RepoRoot
$project = Join-Path $repoRoot 'tools/player-agent/PlayerAgent.csproj'
# Staged merge often exceeds local 7B time budgets; default off — use --Staged when on RunPod/larger models.

foreach ($factionId in $factionIds) {
	if (-not $SkipIngest) {
		Write-PlayRunLog "ingest-faction faction $factionId" -Color Cyan
		$ingestArgs = @(
			'run', '--project', $project, '--',
			'ingest-faction',
			'--mode', $Mode,
			'--run', $Run,
			'--faction', $factionId
		)
		if ($DryRun) { $ingestArgs += '--dry-run' }
		if ($AllowRunPod) { $ingestArgs += '--allow-runpod' }
		& dotnet @ingestArgs
		if ($LASTEXITCODE -ne 0) {
			throw "ingest-faction failed for faction $factionId (exit $LASTEXITCODE)"
		}
	}

	Write-PlayRunLog "draft orders faction $factionId (staged=$Staged maxQuality=$MaxQualityAttempts)" -Color Cyan
	$draftArgs = @(
		'run', '--project', $project, '--',
		'draft',
		'--mode', $Mode,
		'--run', $Run,
		'--faction', $factionId,
		'--max-quality-attempts', $MaxQualityAttempts,
		'--yes'
	)
	if ($DryRun) { $draftArgs += '--dry-run' }
	if ($AllowRunPod) { $draftArgs += '--allow-runpod' }
	if ($Staged) { $draftArgs += '--staged' }

	& dotnet @draftArgs 2>&1 | ForEach-Object {
		Write-PlayRunLog "  [f$factionId] $_"
	}
	if ($LASTEXITCODE -ne 0) {
		throw "draft failed for faction $factionId (exit $LASTEXITCODE)"
	}
	Write-PlayRunLog "draft complete faction $factionId" -Color Green
}

Write-PlayRunLog "Done: contractor orders for $($factionIds -join ', ')" -Color Green

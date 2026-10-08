#Requires -Version 5.1
<#
.SYNOPSIS
  Turn pipeline for AI military seats: story, knowledge (incl. battle intel), RAG ingest, orders.

.PARAMETER DraftOnlyFaction
  If set, only these factions get order drafts (story/knowledge/ingest still run for all military seats in -OnlyFaction).
#>
[CmdletBinding()]
param(
	[Parameter(Mandatory = $true)][string]$Run,
	[ValidateSet('test', 'campaign')][string]$Mode = 'campaign',
	[int[]]$ExcludeFaction = @(2, 3),
	[int[]]$OnlyFaction = @(),
	[int[]]$DraftOnlyFaction = @(),
	[int]$FromFaction = 2,
	[int]$ToFaction = 11,
	[switch]$DryRun,
	[switch]$AllowRunPod,
	[switch]$SkipStory,
	[switch]$SkipKnowledge,
	[switch]$SkipIngest,
	[switch]$SkipSharedRefresh,
	[int]$MaxQualityAttempts = 7,
	[switch]$Staged,
	[string]$LogFileName = 'draft-military-turn.log'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')
Initialize-PlayRunLog -RunId $Run -LogFileName $LogFileName

$factionIds = Get-MilitaryFactionIds -RunId $Run -ExcludeFaction $ExcludeFaction -FromFaction $FromFaction -ToFaction $ToFaction
if ($OnlyFaction.Count -gt 0) {
	$factionIds = @($factionIds | Where-Object { $OnlyFaction -contains $_ })
}
if ($factionIds.Count -eq 0) {
	Write-PlayRunLog 'No military factions to process.' -Color Yellow
	exit 0
}

$draftIds = if ($DraftOnlyFaction.Count -gt 0) {
	@($DraftOnlyFaction | Where-Object { $factionIds -contains $_ })
} else {
	$factionIds
}

Write-PlayRunLog "military turn pipeline run=$Run mode=$Mode factions=$($factionIds -join ',') draft=$($draftIds -join ',') exclude=$($ExcludeFaction -join ',')"
if (-not $DryRun) { Test-OllamaDocker }
Initialize-OllamaEnv
$env:PLAYER_AGENT_CHAT_MAX_OUTPUT_TOKENS = '1536'
$env:PLAYER_AGENT_CHAT_TIMEOUT_SECONDS = '2400'

if (-not $SkipSharedRefresh -and -not $DryRun) {
	Write-PlayRunLog 'refresh-shared-rag (campaign)' -Color Cyan
	& (Join-Path $PSScriptRoot 'refresh-shared-rag.ps1') -Mode $Mode
	if ($LASTEXITCODE -ne 0) { throw "refresh-shared-rag failed (exit $LASTEXITCODE)" }
}

$repoRoot = $script:RepoRoot
$project = Join-Path $repoRoot 'tools/player-agent/PlayerAgent.csproj'

if (-not $SkipStory) {
	Write-PlayRunLog 'draft-story (military, omit-strategic preserved)' -Color Cyan
	$storyParams = @{
		Run          = $Run
		MilitaryOnly = $true
		OnlyFaction  = [int[]]$factionIds
	}
	if ($DryRun) { $storyParams['DryRun'] = $true }
	& (Join-Path $PSScriptRoot 'draft-stories-seq.ps1') @storyParams
	if ($LASTEXITCODE -ne 0) { throw "draft-stories-seq failed (exit $LASTEXITCODE)" }
}

foreach ($factionId in $factionIds) {
	if (-not $SkipKnowledge) {
		Write-PlayRunLog "draft-knowledge faction $factionId" -Color Cyan
		$kArgs = @(
			'run', '--project', $project, '--',
			'draft-knowledge',
			'--run', $Run,
			'--faction', $factionId,
			'--yes'
		)
		if ($DryRun) { $kArgs += '--dry-run' }
		if ($AllowRunPod) { $kArgs += '--allow-runpod' }
		& dotnet @kArgs 2>&1 | ForEach-Object { Write-PlayRunLog "  [f$factionId] $_" }
		if ($LASTEXITCODE -ne 0) { throw "draft-knowledge failed for faction $factionId (exit $LASTEXITCODE)" }
	}

	if (-not $SkipIngest) {
		Write-PlayRunLog "ingest-faction faction $factionId" -Color Cyan
		$ingestArgs = @(
			'run', '--project', $project, '--',
			'ingest-faction',
			'--mode', $Mode,
			'--run', $Run,
			'--faction', $factionId,
			'--yes'
		)
		if ($DryRun) { $ingestArgs += '--dry-run' }
		if ($AllowRunPod) { $ingestArgs += '--allow-runpod' }
		& dotnet @ingestArgs
		if ($LASTEXITCODE -ne 0) { throw "ingest-faction failed for faction $factionId (exit $LASTEXITCODE)" }
	}
}

foreach ($factionId in $draftIds) {
	Write-PlayRunLog "draft orders faction $factionId (maxQuality=$MaxQualityAttempts staged=$Staged)" -Color Cyan
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

	& dotnet @draftArgs 2>&1 | ForEach-Object { Write-PlayRunLog "  [f$factionId] $_" }
	if ($LASTEXITCODE -ne 0) { throw "draft failed for faction $factionId (exit $LASTEXITCODE)" }
	Write-PlayRunLog "complete faction $factionId" -Color Green
}

Write-PlayRunLog "Done: military turn for $($factionIds -join ', '); drafts $($draftIds -join ', ')" -Color Green

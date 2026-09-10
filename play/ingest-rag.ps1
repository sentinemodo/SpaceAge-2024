#Requires -Version 5.1
<#
.SYNOPSIS
  Refresh per-faction RAG indexes after isolate copies new turn reports.

.DESCRIPTION
  Thin wrapper around player-agent ingest-run (Phase 4). Does not rebuild the
  shared manual index — run ingest-shared separately when rules/tech manuals change.

  Typical play loop:
    1. Game.exe turn (or reports-only)
    2. isolate copies report.*.txt into play/runs/<id>/factions/NN/
    3. .\play\ingest-rag.ps1 -Run <id> -Mode campaign
    4. draft orders per faction (player-agent draft or Cursor /player)

.PARAMETER Run
  Campaign run id under play/runs/<id>/.

.PARAMETER Mode
  test (SampleGame manuals) or campaign.

.PARAMETER FromFaction
  First AI seat to refresh (default 2).

.PARAMETER ToFaction
  Last AI seat to refresh (default 11).

.PARAMETER MaxOrderTurns
  Keep orders from the last N turns in each faction index (default 3).

.PARAMETER Full
  Phase 2 full ingest: every report and order file on disk.

.PARAMETER DryRun
  Print ingest/prune plan without calling Ollama.

.PARAMETER AllowRunPod
  Pass --allow-runpod when OLLAMA_HOST points at RunPod.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $Run,

    [Parameter(Mandatory = $true)]
    [ValidateSet('test', 'campaign')]
    [string] $Mode,

    [int] $FromFaction = 2,
    [int] $ToFaction = 11,
    [int] $MaxOrderTurns = 3,
    [switch] $Full,
    [switch] $DryRun,
    [switch] $AllowRunPod
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot 'tools/player-agent/PlayerAgent.csproj'

if (-not (Test-Path $project)) {
    throw "Player-agent project not found: $project"
}

$args = @(
    'run',
    '--project', $project,
    '--',
    'ingest-run',
    '--mode', $Mode,
    '--run', $Run,
    '--from-faction', $FromFaction,
    '--to-faction', $ToFaction,
    '--max-order-turns', $MaxOrderTurns
)

if ($Full) { $args += '--full' }
if ($DryRun) { $args += '--dry-run' }
if ($AllowRunPod) { $args += '--allow-runpod' }

Write-Host "player-agent ingest-run --mode $Mode --run $Run (factions $FromFaction..$ToFaction)"
& dotnet @args
if ($LASTEXITCODE -ne 0) {
    throw "ingest-rag failed with exit code $LASTEXITCODE"
}

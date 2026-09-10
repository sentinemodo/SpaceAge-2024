#Requires -Version 5.1
<#
.SYNOPSIS
  Draft orders for all AI seats after per-faction RAG refresh.

.DESCRIPTION
  Thin wrapper around player-agent draft-run (Phase 6). Runs an isolation audit,
  then queues one draft per faction sequentially against a single Ollama host.

  Typical play loop:
    1. Game.exe turn (or reports-only)
    2. isolate copies report.*.txt into play/runs/<id>/factions/NN/
    3. .\play\ingest-rag.ps1 -Run <id> -Mode campaign
    4. .\play\draft-run.ps1 -Run <id> -Mode campaign
    5. GM review + play/turn.ps1 (UTF-8 to Windows-1251) before the next turn

  Cursor /player remains valid for single-seat or human-in-the-loop drafting.

.PARAMETER Run
  Campaign run id under play/runs/<id>/.

.PARAMETER Mode
  test (SampleGame manuals) or campaign.

.PARAMETER FromFaction
  First AI seat to draft (default 2).

.PARAMETER ToFaction
  Last AI seat to draft (default 11).

.PARAMETER DryRun
  Build prompt packs without calling chat or writing order files.

.PARAMETER AllowRunPod
  Pass --allow-runpod when OLLAMA_HOST points at RunPod.

.PARAMETER SkipIsolationAudit
  Skip shared/faction index audit (not recommended).

.PARAMETER NoRecordAudit
  Do not append audit results to play/runs/<id>/gm/isolation-audit.md.
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
    [switch] $DryRun,
    [switch] $AllowRunPod,
    [switch] $SkipIsolationAudit,
    [switch] $NoRecordAudit
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
    'draft-run',
    '--mode', $Mode,
    '--run', $Run,
    '--from-faction', $FromFaction,
    '--to-faction', $ToFaction
)

if ($DryRun) { $args += '--dry-run' }
if ($AllowRunPod) { $args += '--allow-runpod' }
if ($SkipIsolationAudit) { $args += '--skip-isolation-audit' }
if ($NoRecordAudit) { $args += '--no-record-audit' }

Write-Host "player-agent draft-run --mode $Mode --run $Run (factions $FromFaction..$ToFaction)"
& dotnet @args
if ($LASTEXITCODE -ne 0) {
    throw "draft-run failed with exit code $LASTEXITCODE"
}

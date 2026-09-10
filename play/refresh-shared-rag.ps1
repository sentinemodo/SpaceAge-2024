#Requires -Version 5.1
<#
.SYNOPSIS
  Rebuild shared RAG indexes after engine, catalog, or player-manual updates.

.DESCRIPTION
  Thin wrapper around player-agent refresh-shared (Phase 5). Run after /player
  docs-only refresh updates rules.md, battle.md, or tech manuals — not after
  every turn (use play/ingest-rag.ps1 for per-faction report refresh).

  Typical workflow:
    1. Refresh player manuals (/player docs-only or human edit)
    2. .\play\refresh-shared-rag.ps1 -Mode campaign
    3. Optional: re-ingest factions only if report templates changed

.PARAMETER Mode
  test, campaign, or both (rebuilds shared-test and/or shared-campaign).

.PARAMETER SpotCheckVerb
  Verb heading to retrieve after ingest (default MOVE).

.PARAMETER SpotCheckTech
  Optional catalog tech id to retrieve after ingest.

.PARAMETER NoteRun
  Append a rebuild note to play/runs/<id>/README.md.

.PARAMETER CatalogRevision
  Optional catalog revision label for the run note.

.PARAMETER Clear
  Wipe the target shared index before ingest.

.PARAMETER DryRun
  Print checklist and chunk plan without calling Ollama.

.PARAMETER SkipIngest
  Regenerate allowlist only (no embed calls).

.PARAMETER SkipSpotCheck
  Skip post-ingest retrieve spot-checks.

.PARAMETER AllowRunPod
  Pass --allow-runpod when OLLAMA_HOST points at RunPod.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('test', 'campaign', 'both')]
    [string] $Mode,

    [string] $SpotCheckVerb = 'MOVE',
    [string] $SpotCheckTech,
    [string] $NoteRun,
    [string] $CatalogRevision,
    [switch] $Clear,
    [switch] $DryRun,
    [switch] $SkipIngest,
    [switch] $SkipSpotCheck,
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
    'refresh-shared',
    '--mode', $Mode,
    '--spot-check-verb', $SpotCheckVerb
)

if ($SpotCheckTech) { $args += @('--spot-check-tech', $SpotCheckTech) }
if ($NoteRun) { $args += @('--note-run', $NoteRun) }
if ($CatalogRevision) { $args += @('--catalog-revision', $CatalogRevision) }
if ($Clear) { $args += '--clear' }
if ($DryRun) { $args += '--dry-run' }
if ($SkipIngest) { $args += '--skip-ingest' }
if ($SkipSpotCheck) { $args += '--skip-spot-check' }
if ($AllowRunPod) { $args += '--allow-runpod' }

Write-Host "player-agent refresh-shared --mode $Mode"
& dotnet @args
if ($LASTEXITCODE -ne 0) {
    throw "refresh-shared-rag failed with exit code $LASTEXITCODE"
}

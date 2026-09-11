param(
    [int]$Port = 4173
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '../../..')
$gameExe = Join-Path $repoRoot 'Game/bin/Debug/Game.exe'
$fixture = Join-Path $repoRoot 'Tests/fixtures/battle-sim/inftry-skirmish.xml'
$campaignData = Join-Path $repoRoot 'campaign'

if (-not (Test-Path $gameExe)) {
    throw "Build Game.exe first: $gameExe"
}

Write-Host "CLI smoke: Game.exe /battle-sim"
& $gameExe /battle-sim $fixture /data $campaignData /seed 42 | Select-Object -First 5

Write-Host ""
Write-Host "Bridge smoke: POST /api/run"
$bridge = Start-Process -FilePath 'node' -ArgumentList @(Join-Path $repoRoot 'tools/battle-simulator/server/bridge.mjs') -PassThru -WindowStyle Hidden
Start-Sleep -Seconds 2
try {
    $xml = Get-Content $fixture -Raw
    $body = @{ xml = $xml; seed = 42 } | ConvertTo-Json
    $result = Invoke-RestMethod -Uri "http://localhost:$Port/api/run" -Method POST -Body $body -ContentType 'application/json'
    if ($result.output -notmatch 'SIMULATION RESULT:') {
        throw 'Bridge response missing SIMULATION RESULT block.'
    }
    Write-Host 'Bridge smoke passed.'
}
finally {
    Stop-Process -Id $bridge.Id -Force -ErrorAction SilentlyContinue
}

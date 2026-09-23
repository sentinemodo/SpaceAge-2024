#Requires -Version 5.1
<#
.SYNOPSIS
  SpaceAge local CI/CD helpers for the /cicd agent.

.DESCRIPTION
  restart-dev        — Astro lobby (4321) + visual tool (5173)
  restart-prod       — restart-dev + game-host Docker + ngrok tunnel
  restart-local-llm  — Ollama Docker, pull/warm qwen2.5-coder:7b, health check
  commit             — git commit working changes, then restart-dev
  push               — git commit, push branch, then restart-prod
  merge              — git commit, push, merge into default branch, push, restart-prod
  test               — run all component test suites and print a summary report
  test-e2e           — Playwright e2e only (visual-tool + website); saves report under .cursor/cicd-test-results/
#>
param(
	[Parameter(Mandatory = $true, Position = 0)]
	[ValidateSet('restart-dev', 'restart-prod', 'restart-local-llm', 'commit', 'push', 'merge', 'test', 'test-e2e')]
	[string] $Action,

	[string] $Message,
	[string] $MergeTarget,
	[switch] $SkipCommit,
	[switch] $IncludeE2e
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')

$script:WebsitePort = 4321
$script:VisualToolPort = 5173
$script:GameHostPort = if ($env:GAME_HOST_PORT) { [int]$env:GAME_HOST_PORT } else { 8787 }
$script:OllamaPort = 11434
$script:ChatModel = 'qwen2.5-coder:7b'
$script:DevLogDir = Join-Path $script:RepoRoot '.cursor\dev-logs'
$script:CicdTestResultsDir = Join-Path $script:RepoRoot '.cursor\cicd-test-results'

function Write-CicdStep {
	param([string]$Text)
	Write-Host ""
	Write-Host "==> $Text" -ForegroundColor Cyan
}

function Test-DockerInfrastructureProcess {
	param([Parameter(Mandatory = $true)][int] $ProcessId)
	try {
		$proc = Get-Process -Id $ProcessId -ErrorAction Stop
	}
	catch {
		return $false
	}
	# Port 8787 is published by Docker Desktop. Killing that process takes the engine down
	# before `docker compose down` can run.
	return $proc.ProcessName -match '^(com\.docker\.|Docker Desktop|docker-proxy|wslrelay|vpnkit)$'
}

function Stop-ListenerProcess {
	param(
		[Parameter(Mandatory = $true)][int] $ProcessId,
		[Parameter(Mandatory = $true)][int] $Port
	)
	if (Test-DockerInfrastructureProcess -ProcessId $ProcessId) {
		Write-Host "  left PID $ProcessId on port $Port (Docker infrastructure; compose down releases it)"
		return
	}
	Stop-Process -Id $ProcessId -Force -ErrorAction SilentlyContinue
	Write-Host "  stopped PID $ProcessId on port $Port"
}

function Stop-ListenersOnPort {
	param([Parameter(Mandatory = $true)][int] $Port)
	try {
		$connections = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
		foreach ($conn in $connections) {
			$ownerPid = $conn.OwningProcess
			if ($ownerPid -and $ownerPid -ne 0) {
				Stop-ListenerProcess -ProcessId $ownerPid -Port $Port
			}
		}
	}
	catch {
		# Fallback for older Windows without Get-NetTCPConnection admin rights
		$out = netstat -ano | Select-String ":$Port\s"
		foreach ($line in $out) {
			if ($line -notmatch 'LISTENING') { continue }
			$ownerPid = ($line -split '\s+')[-1]
			if ($ownerPid -and $ownerPid -ne '0') {
				Stop-ListenerProcess -ProcessId ([int]$ownerPid) -Port $Port
			}
		}
	}
	Start-Sleep -Milliseconds 400
}

function Stop-DevPorts {
	Stop-ListenersOnPort -Port $script:WebsitePort
	Stop-ListenersOnPort -Port $script:VisualToolPort
}

function Stop-GameHostPort {
	Stop-ListenersOnPort -Port $script:GameHostPort
}

function Stop-CicdStackPorts {
	Write-Host '  freeing ports 4321 (lobby), 5173 (visual tool), 8787 (game-host) ...'
	Stop-DevPorts
	Stop-GameHostPort
}

function Start-BackgroundNpmDev {
	param(
		[Parameter(Mandatory = $true)][string] $Label,
		[Parameter(Mandatory = $true)][string] $WorkingDirectory,
		[Parameter(Mandatory = $true)][int] $Port
	)
	if (-not (Test-Path -LiteralPath $WorkingDirectory)) {
		throw "Missing directory: $WorkingDirectory"
	}
	New-Item -ItemType Directory -Force -Path $script:DevLogDir | Out-Null
	$logPath = Join-Path $script:DevLogDir "$Label.log"
	$errPath = Join-Path $script:DevLogDir "$Label.err.log"

	$psi = New-Object System.Diagnostics.ProcessStartInfo
	$psi.FileName = 'cmd.exe'
	$psi.Arguments = "/c npm run dev > `"$logPath`" 2> `"$errPath`""
	$psi.WorkingDirectory = $WorkingDirectory
	$psi.UseShellExecute = $false
	$psi.CreateNoWindow = $true
	[void][System.Diagnostics.Process]::Start($psi)

	$deadline = (Get-Date).AddSeconds(90)
	do {
		Start-Sleep -Milliseconds 500
		$ready = $false
		foreach ($hostName in @('127.0.0.1', 'localhost')) {
			try {
				Invoke-WebRequest -Uri "http://${hostName}:$Port/" -UseBasicParsing -TimeoutSec 3 | Out-Null
				$ready = $true
				break
			}
			catch {
				# try next host or wait
			}
		}
		if (-not $ready -and (Test-Path -LiteralPath $logPath)) {
			$tail = Get-Content -LiteralPath $logPath -Tail 30 -ErrorAction SilentlyContinue
			if ($tail -match 'ready in') {
				$listening = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
				if ($listening) {
					$ready = $true
				}
			}
		}
		if ($ready) {
			Write-Host "  $Label listening on http://localhost:$Port/ (log: $logPath)"
			return
		}
		if ((Get-Date) -gt $deadline) {
			throw "$Label did not start on port $Port within 90s. See $logPath and $errPath"
		}
	} while ($true)
}

function Invoke-RestartDev {
	Write-CicdStep 'Restart dev - lobby website + visual tool'
	Stop-DevPorts

	$websiteDir = Join-Path $script:RepoRoot 'website'
	$visualDir = Join-Path $script:RepoRoot 'tools\visual-tool'

	Start-BackgroundNpmDev -Label 'website-dev' -WorkingDirectory $websiteDir -Port $script:WebsitePort
	Start-BackgroundNpmDev -Label 'visual-tool-dev' -WorkingDirectory $visualDir -Port $script:VisualToolPort

	Write-Host ""
	Write-Host "Dev URLs:"
	Write-Host "  Lobby:       http://localhost:$($script:WebsitePort)/"
	Write-Host "  Visual tool: http://localhost:$($script:VisualToolPort)/client/"
	Write-Host "  (Visual tool API proxy expects game-host on http://localhost:$($script:GameHostPort)/ - use restart-prod or game-host npm start.)"
}

function Test-GameHostHealth {
	param([int] $Port = $script:GameHostPort)
	Invoke-RestMethod -Uri "http://127.0.0.1:$Port/health" -TimeoutSec 15 | Out-Null
}

function Invoke-RestartGameHostDocker {
	Write-CicdStep 'Restart game-host Docker stack'
	Stop-GameHostPort
	$down = Invoke-ExternalCommandCapture -FilePath 'docker' -ArgumentList @('compose', 'down', '--remove-orphans')
	if ($down.ExitCode -ne 0) {
		throw "docker compose down failed (exit $($down.ExitCode))"
	}
	$up = Invoke-ExternalCommandCapture -FilePath 'docker' -ArgumentList @('compose', 'up', '-d', '--build')
	if ($up.ExitCode -ne 0) {
		throw "docker compose up failed (exit $($up.ExitCode))"
	}
	$deadline = (Get-Date).AddSeconds(120)
	do {
		try {
			Test-GameHostHealth
			Write-Host "  game-host healthy on http://localhost:$($script:GameHostPort)/"
			return
		}
		catch {
			if ((Get-Date) -gt $deadline) {
				throw "game-host not healthy on port $($script:GameHostPort) after 120s"
			}
			Start-Sleep -Seconds 3
		}
	} while ($true)
}

function Ensure-NgrokTunnel {
	Write-CicdStep 'Ensure ngrok tunnel to game-host'
	Import-RepoEnv
	$port = $script:GameHostPort
	$domain = if ($env:NGROK_DOMAIN) { $env:NGROK_DOMAIN } else { 'manatee-sabbath-kudos.ngrok-free.dev' }

	Test-GameHostHealth -Port $port

	$existing = $null
	try {
		$existing = Invoke-RestMethod -Uri 'http://127.0.0.1:4040/api/tunnels' -TimeoutSec 3
	}
	catch {
		# ngrok API not up yet
	}

	$tunnel = $null
	if ($existing -and $existing.tunnels) {
		$tunnel = $existing.tunnels | Where-Object {
			$_.config.addr -match ":$port`$" -or $_.config.addr -match ":$port/"
		} | Select-Object -First 1
	}

	if ($tunnel) {
		Write-Host "  ngrok already forwarding: $($tunnel.public_url)/client/"
		return
	}

	$ngrokExe = Join-Path $env:APPDATA 'npm\node_modules\ngrok\bin\ngrok.exe'
	if (-not (Test-Path -LiteralPath $ngrokExe)) {
		$ngrokExe = (Get-Command ngrok.exe -ErrorAction SilentlyContinue).Source
	}
	if (-not $ngrokExe -or -not (Test-Path -LiteralPath $ngrokExe)) {
		throw 'ngrok not found. Install: npm install -g ngrok, then: ngrok config add-authtoken <token>'
	}

	New-Item -ItemType Directory -Force -Path $script:DevLogDir | Out-Null
	$ngrokLog = Join-Path $script:DevLogDir 'ngrok.log'
	$args = @('http', [string]$port, "--domain=$domain", "--log=stdout")
	Start-Process -FilePath $ngrokExe -ArgumentList $args -RedirectStandardOutput $ngrokLog -WindowStyle Hidden | Out-Null

	$deadline = (Get-Date).AddSeconds(30)
	do {
		Start-Sleep -Seconds 1
		try {
			$existing = Invoke-RestMethod -Uri 'http://127.0.0.1:4040/api/tunnels' -TimeoutSec 3
			$tunnel = $existing.tunnels | Where-Object {
				$_.config.addr -match ":$port`$" -or $_.config.addr -match ":$port/"
			} | Select-Object -First 1
			if ($tunnel) {
				Write-Host "  ngrok started: $($tunnel.public_url)/client/"
				return
			}
		}
		catch {
			if ((Get-Date) -gt $deadline) {
				throw "ngrok did not expose port $port within 30s. See $ngrokLog"
			}
		}
	} while ($true)
}

function Invoke-RestartProd {
	Write-CicdStep 'Free stack ports before prod restart'
	Stop-CicdStackPorts
	Invoke-RestartDev
	Invoke-RestartGameHostDocker
	Ensure-NgrokTunnel
	Write-Host ""
	Write-Host "Prod stack ready (local dev + Docker game-host + ngrok)."
}

function Invoke-RestartLocalLlm {
	Write-CicdStep 'Restart local Ollama Docker'
	Initialize-OllamaEnv

	$running = docker ps --filter 'name=^/ollama$' --format '{{.Names}}' 2>$null
	if ($running -eq 'ollama') {
		Write-Host '  restarting existing ollama container'
		docker restart ollama | Out-Null
	}
	else {
		$exists = docker ps -a --filter 'name=^/ollama$' --format '{{.Names}}' 2>$null
		if ($exists -eq 'ollama') {
			Write-Host '  starting stopped ollama container'
			docker start ollama | Out-Null
		}
		else {
			Write-Host '  creating ollama container (port 11434)'
			docker run -d --name ollama -p "$($script:OllamaPort):11434" -v ollama:/root/.ollama ollama/ollama | Out-Null
		}
	}

	$deadline = (Get-Date).AddSeconds(60)
	do {
		try {
			Invoke-RestMethod -Uri "http://127.0.0.1:$($script:OllamaPort)/api/tags" -TimeoutSec 5 | Out-Null
			break
		}
		catch {
			if ((Get-Date) -gt $deadline) {
				throw "Ollama API not reachable on port $($script:OllamaPort) after 60s"
			}
			Start-Sleep -Seconds 2
		}
	} while ($true)

	Write-CicdStep "Pull and warm chat model ($($script:ChatModel))"
	docker exec ollama ollama pull $script:ChatModel
	$warmBody = @{
		model  = $script:ChatModel
		prompt = 'Reply with OK.'
		stream = $false
	} | ConvertTo-Json -Compress
	Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:$($script:OllamaPort)/api/generate" `
		-ContentType 'application/json' -Body $warmBody -TimeoutSec 300 | Out-Null

	Test-OllamaDocker
	Write-Host '  Ollama healthy with chat model ready.'
}

function Get-DefaultMergeTarget {
	if ($MergeTarget) { return $MergeTarget }
	$originHead = git symbolic-ref refs/remotes/origin/HEAD 2>$null
	if ($originHead -match 'origin/(.+)$') { return $Matches[1] }
	if (git show-ref --verify --quiet refs/heads/main) { return 'main' }
	return 'master'
}

function Invoke-GitCommitAll {
	if ($SkipCommit) {
		Write-Host '  skip commit (-SkipCommit)'
		return
	}

	$status = git status --porcelain
	if (-not $status) {
		Write-Host '  nothing to commit'
		return
	}

	if (-not $Message) {
		$Message = Read-Host 'Commit message'
	}
	if ([string]::IsNullOrWhiteSpace($Message)) {
		throw 'Commit message required (pass -Message or enter at prompt)'
	}

	git add -A
	git commit -m $Message
	Write-Host "  committed: $Message"
}

function Invoke-GitPushCurrent {
	$branch = git branch --show-current
	if (-not $branch) { throw 'Detached HEAD - checkout a branch before push' }
	git push -u origin $branch
	Write-Host "  pushed branch $branch"
}

function Invoke-GitMergeToDefault {
	$target = Get-DefaultMergeTarget
	$branch = git branch --show-current
	if ($branch -eq $target) {
		Write-Host "  already on $target - merge skipped"
		return
	}
	git checkout $target
	git pull origin $target
	git merge $branch --no-edit
	git push origin $target
	Write-Host "  merged $branch -> $target and pushed"
}

$script:TestResults = @()

function Add-TestResult {
	param(
		[Parameter(Mandatory = $true)][string] $Name,
		[Parameter(Mandatory = $true)][ValidateSet('PASS', 'FAIL', 'SKIP')]
		[string] $Status,
		[string] $Detail = '',
		[double] $DurationSec = 0,
		[string[]] $FailedTests = @()
	)
	$script:TestResults += [pscustomobject]@{
		Name        = $Name
		Status      = $Status
		Detail      = $Detail
		DurationSec = [math]::Round($DurationSec, 1)
		Failures    = @($FailedTests)
	}
}

function Invoke-ExternalCommand {
	param(
		[Parameter(Mandatory = $true)][string] $FilePath,
		[string[]] $ArgumentList = @(),
		[string] $WorkingDirectory = $script:RepoRoot
	)
	$result = Invoke-ExternalCommandCapture -FilePath $FilePath -ArgumentList $ArgumentList -WorkingDirectory $WorkingDirectory
	if ($null -ne $result.ExitCode -and $result.ExitCode -ne 0) {
		throw "exit code $($result.ExitCode)"
	}
}

function Invoke-ExternalCommandCapture {
	param(
		[Parameter(Mandatory = $true)][string] $FilePath,
		[string[]] $ArgumentList = @(),
		[string] $WorkingDirectory = $script:RepoRoot
	)
	Push-Location $WorkingDirectory
	$prevEap = $ErrorActionPreference
	$ErrorActionPreference = 'Continue'
	try {
		$raw = & $FilePath @ArgumentList 2>&1
		$lines = @($raw | ForEach-Object {
			if ($_ -is [System.Management.Automation.ErrorRecord]) {
				$_.Exception.Message
			}
			else {
				"$_"
			}
		})
		foreach ($line in $lines) {
			Write-Host $line
		}
		$exit = $LASTEXITCODE
		if ($null -eq $exit) {
			$exit = 0
		}
		return @{
			ExitCode = [int]$exit
			Lines    = $lines
		}
	}
	finally {
		$ErrorActionPreference = $prevEap
		Pop-Location
	}
}

function Get-NUnitFailedTestNames {
	param(
		[string[]] $Lines,
		[string] $XmlPath
	)
	$names = [System.Collections.Generic.List[string]]::new()
	foreach ($line in $Lines) {
		if ($line -match '^\d+\)\s+(?:Failed|Error)\s*:\s*(.+)$') {
			[void]$names.Add($Matches[1].Trim())
		}
	}
	if ($names.Count -eq 0 -and (Test-Path -LiteralPath $XmlPath)) {
		try {
			[xml]$doc = Get-Content -LiteralPath $XmlPath -Encoding UTF8
			$nodes = $doc.SelectNodes("//test-case[@result='Failed' or @result='Error']")
			foreach ($node in $nodes) {
				if ($node.fullname) {
					[void]$names.Add([string]$node.fullname)
				}
			}
		}
		catch {
			# ignore xml parse errors
		}
	}
	return @($names | Select-Object -Unique)
}

function Get-DotnetTestFailedTestNames {
	param([string[]] $Lines)
	$names = [System.Collections.Generic.List[string]]::new()
	foreach ($line in $Lines) {
		if ($line -match '^\s*(?:Failed|Niepowodzenie)\s+(\S+)') {
			[void]$names.Add($Matches[1].Trim())
		}
		elseif ($line -match '\s+at\s+([A-Za-z0-9_.]+\.[A-Za-z0-9_]+)\(\)\s+in\s+') {
			[void]$names.Add($Matches[1].Trim())
		}
	}
	return @($names | Select-Object -Unique)
}

function Get-VitestFailedTestNames {
	param([string[]] $Lines)
	$names = [System.Collections.Generic.List[string]]::new()
	foreach ($line in $Lines) {
		if ($line -match '^\s*(?:FAIL|×)\s+(.+)$') {
			[void]$names.Add($Matches[1].Trim())
		}
	}
	return @($names | Select-Object -Unique)
}

function Get-NodeTestFailedTestNames {
	param([string[]] $Lines)
	$names = [System.Collections.Generic.List[string]]::new()
	foreach ($line in $Lines) {
		if ($line -match '^\s*(?:✖|×)\s+(.+)$') {
			[void]$names.Add($Matches[1].Trim())
		}
	}
	return @($names | Select-Object -Unique)
}

function Get-PlaywrightFailedTestNames {
	param([string[]] $Lines)
	$names = [System.Collections.Generic.List[string]]::new()
	foreach ($line in $Lines) {
		if ($line -match '^\s*\d+\)\s+(?:\[[^\]]+\]\s+›\s+)?(.+?)\s*(?:─+|$)') {
			[void]$names.Add($Matches[1].Trim())
		}
		elseif ($line -match '^\s*(?:x|X)\s+\d+\)\s+(.+)$') {
			[void]$names.Add($Matches[1].Trim())
		}
	}
	return @($names | Select-Object -Unique)
}

function Format-TestFailureDetail {
	param(
		[string] $Detail,
		[string[]] $Failures
	)
	if (-not $Failures -or $Failures.Count -eq 0) {
		return $Detail
	}
	$lines = @($Detail)
	$lines += 'failed tests:'
	foreach ($name in $Failures) {
		$lines += "  - $name"
	}
	return ($lines -join [Environment]::NewLine)
}

function Ensure-NpmDependencies {
	param([Parameter(Mandatory = $true)][string] $RelativeDir)
	$dir = Join-Path $script:RepoRoot $RelativeDir
	if (Test-Path (Join-Path $dir 'node_modules')) {
		return
	}
	$lock = Join-Path $dir 'package-lock.json'
	if (Test-Path -LiteralPath $lock) {
		Write-Host "  npm ci in $RelativeDir ..."
		Invoke-ExternalCommand -FilePath 'npm' -ArgumentList @('ci') -WorkingDirectory $dir
	}
	else {
		Write-Host "  npm install in $RelativeDir (no package-lock.json) ..."
		Invoke-ExternalCommand -FilePath 'npm' -ArgumentList @('install') -WorkingDirectory $dir
	}
}

function Invoke-NpmScript {
	param(
		[Parameter(Mandatory = $true)][string] $RelativeDir,
		[Parameter(Mandatory = $true)][string] $ScriptName,
		[ValidateSet('default', 'vitest', 'node', 'playwright')]
		[string] $FailureParser = 'default'
	)
	Ensure-NpmDependencies -RelativeDir $RelativeDir
	$dir = Join-Path $script:RepoRoot $RelativeDir
	Write-Host "  npm run $ScriptName (in $RelativeDir)"
	$result = Invoke-ExternalCommandCapture -FilePath 'npm' -ArgumentList @('run', $ScriptName) -WorkingDirectory $dir
	if ($result.ExitCode -ne 0) {
		$failures = @()
		switch ($FailureParser) {
			'vitest' { $failures = Get-VitestFailedTestNames -Lines $result.Lines }
			'node' { $failures = Get-NodeTestFailedTestNames -Lines $result.Lines }
			'playwright' { $failures = Get-PlaywrightFailedTestNames -Lines $result.Lines }
		}
		$script:LastSuiteFailures = $failures
		throw "exit code $($result.ExitCode)"
	}
}

function Ensure-GameSolutionBuilt {
	$gameExe = Join-Path $script:RepoRoot 'Game\bin\Debug\Game.exe'
	$testsDll = Join-Path $script:RepoRoot 'Tests\bin\Debug\Tests.dll'
	if ((Test-Path -LiteralPath $gameExe) -and (Test-Path -LiteralPath $testsDll)) {
		return
	}
	Write-Host '  msbuild SpaceAge.sln (Debug) ...'
	$solution = Join-Path $script:RepoRoot 'SpaceAge.sln'
	Invoke-ExternalCommand -FilePath 'msbuild' -ArgumentList @(
		$solution,
		'/p:Configuration=Debug',
		'/v:m',
		'/nologo'
	)
	if (-not (Test-Path -LiteralPath $testsDll)) {
		throw 'Tests\bin\Debug\Tests.dll missing after build'
	}
}

function Invoke-GameEngineTests {
	Ensure-GameSolutionBuilt
	$testBin = Join-Path $script:RepoRoot 'Tests\bin\Debug'
	$nunit = Join-Path $script:RepoRoot '.cursor\tools\nunit-runner\NUnit.ConsoleRunner.3.18.3\tools\nunit3-console.exe'
	if (-not (Test-Path -LiteralPath $nunit)) {
		throw "NUnit console missing at $nunit - run: bash .cursor/install.sh"
	}
	Write-Host '  msbuild SpaceAge.sln (Debug) - Game.exe + Tests.dll'
	Write-Host '  nunit3-console Tests.dll --inprocess'
	$result = Invoke-ExternalCommandCapture -FilePath $nunit -ArgumentList @(
		'Tests.dll',
		'--inprocess',
		'--work=.',
		'--result=TestResult.xml'
	) -WorkingDirectory $testBin
	if ($result.ExitCode -ne 0) {
		$xml = Join-Path $testBin 'TestResult.xml'
		$script:LastSuiteFailures = Get-NUnitFailedTestNames -Lines $result.Lines -XmlPath $xml
		throw "exit code $($result.ExitCode)"
	}
}

function Invoke-PlayerAgentTests {
	$project = Join-Path $script:RepoRoot 'tools\player-agent-tests\PlayerAgent.Tests.csproj'
	if (-not (Test-Path -LiteralPath $project)) {
		throw "Missing $project"
	}
	Write-Host '  dotnet test tools/player-agent-tests (PlayerAgent + tests)'
	$result = Invoke-ExternalCommandCapture -FilePath 'dotnet' -ArgumentList @(
		'test',
		$project,
		'-c', 'Release',
		'--verbosity', 'normal'
	)
	if ($result.ExitCode -ne 0) {
		$script:LastSuiteFailures = Get-DotnetTestFailedTestNames -Lines $result.Lines
		throw "exit code $($result.ExitCode)"
	}
}

function Invoke-TestSuite {
	param(
		[Parameter(Mandatory = $true)][string] $Name,
		[Parameter(Mandatory = $true)][scriptblock] $Block
	)
	Write-CicdStep "Test: $Name"
	$sw = [System.Diagnostics.Stopwatch]::StartNew()
	$script:LastSuiteFailures = @()
	try {
		& $Block
		Add-TestResult -Name $Name -Status 'PASS' -DurationSec $sw.Elapsed.TotalSeconds
		Write-Host '  PASS' -ForegroundColor Green
	}
	catch {
		$detail = $_.Exception.Message
		if ($_.ErrorDetails -and $_.ErrorDetails.Message) {
			$detail = $_.ErrorDetails.Message
		}
		$failures = @($script:LastSuiteFailures)
		Add-TestResult -Name $Name -Status 'FAIL' -Detail $detail -DurationSec $sw.Elapsed.TotalSeconds -FailedTests $failures
		Write-Host "  FAIL - $detail" -ForegroundColor Red
		foreach ($failName in $failures) {
			Write-Host "    - $failName" -ForegroundColor Red
		}
	}
}

function Save-TestResultsReport {
	param(
		[Parameter(Mandatory = $true)][string] $Scope,
		[Parameter(Mandatory = $true)][string[]] $SummaryLines,
		[int] $Passed,
		[int] $Failed,
		[int] $Skipped
	)
	New-Item -ItemType Directory -Force -Path $script:CicdTestResultsDir | Out-Null
	$stamp = Get-Date -Format 'yyyy-MM-ddTHHmmss'
	$archived = Join-Path $script:CicdTestResultsDir "$stamp-$Scope.txt"
	$latest = Join-Path $script:CicdTestResultsDir 'latest.txt'
	$header = @(
		"SpaceAge cicd test report",
		"scope: $Scope",
		"started: $($script:TestRunStartedUtc)",
		"finished: $((Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ'))",
		"repo: $($script:RepoRoot)",
		''
	)
	$footer = @(
		'',
		"Total: $Passed passed, $Failed failed, $Skipped skipped",
		"overall: $(if ($Failed -gt 0) { 'FAIL' } else { 'PASS' })"
	)
	$body = $header + $SummaryLines + $footer
	$text = ($body -join [Environment]::NewLine)
	$text | Set-Content -LiteralPath $archived -Encoding UTF8
	$text | Set-Content -LiteralPath $latest -Encoding UTF8
	Write-Host ''
	Write-Host "Report saved: $archived" -ForegroundColor Cyan
	Write-Host "Latest copy:  $latest" -ForegroundColor Cyan
}

function Write-TestSummary {
	param([Parameter(Mandatory = $true)][string] $Scope)
	Write-Host ''
	Write-Host '=== Test summary ===' -ForegroundColor Cyan
	$maxName = ($script:TestResults | ForEach-Object { $_.Name.Length } | Measure-Object -Maximum).Maximum
	$summaryLines = @()
	foreach ($row in $script:TestResults) {
		$color = switch ($row.Status) {
			'PASS' { 'Green' }
			'FAIL' { 'Red' }
			default { 'Yellow' }
		}
		$padded = $row.Name.PadRight($maxName)
		$line = "$padded  [$($row.Status)]  ($($row.DurationSec)s)"
		if ($row.Detail) {
			$line += "  $([string]$row.Detail)"
		}
		$summaryLines += $line
		Write-Host $line -ForegroundColor $color
		if ($row.Failures -and $row.Failures.Count -gt 0) {
			foreach ($failName in $row.Failures) {
				$failLine = "  - $failName"
				$summaryLines += $failLine
				Write-Host $failLine -ForegroundColor Red
			}
		}
	}
	$passed = @($script:TestResults | Where-Object { $_.Status -eq 'PASS' }).Count
	$failed = @($script:TestResults | Where-Object { $_.Status -eq 'FAIL' }).Count
	$skipped = @($script:TestResults | Where-Object { $_.Status -eq 'SKIP' }).Count
	Write-Host ''
	Write-Host "Total: $passed passed, $failed failed, $skipped skipped"
	Save-TestResultsReport -Scope $Scope -SummaryLines $summaryLines -Passed $passed -Failed $failed -Skipped $skipped
	if ($failed -gt 0) {
		throw "$failed test suite(s) failed"
	}
}

function Initialize-E2eEnv {
	Import-RepoEnv
	if ([string]::IsNullOrWhiteSpace($env:GAME_HOST_GM_KEY)) {
		$env:GAME_HOST_GM_KEY = 'dev-gm-key'
	}
}

function Invoke-RunE2eTests {
	$script:TestResults = @()
	$script:TestRunStartedUtc = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')

	Initialize-E2eEnv
	Ensure-GameSolutionBuilt

	Invoke-TestSuite -Name 'visual-tool (playwright e2e)' -Block {
		Invoke-NpmScript -RelativeDir 'tools\visual-tool' -ScriptName 'test:e2e' -FailureParser 'playwright'
	}
	Invoke-TestSuite -Name 'website (build for preview)' -Block {
		Invoke-NpmScript -RelativeDir 'website' -ScriptName 'build'
	}
	Invoke-TestSuite -Name 'website (playwright e2e)' -Block {
		Invoke-NpmScript -RelativeDir 'website' -ScriptName 'test:e2e' -FailureParser 'playwright'
	}

	Write-TestSummary -Scope 'e2e'
}

function Invoke-RunAllTests {
	$script:TestResults = @()
	$script:TestRunStartedUtc = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')

	Invoke-TestSuite -Name 'game-engine (Tests.dll)' -Block { Invoke-GameEngineTests }
	Invoke-TestSuite -Name 'player-agent (dotnet test)' -Block { Invoke-PlayerAgentTests }
	Invoke-TestSuite -Name 'game-host (node --test)' -Block {
		Invoke-NpmScript -RelativeDir 'game-host' -ScriptName 'test' -FailureParser 'node'
	}
	Invoke-TestSuite -Name 'visual-tool (vitest)' -Block {
		Invoke-NpmScript -RelativeDir 'tools\visual-tool' -ScriptName 'test' -FailureParser 'vitest'
	}
	Invoke-TestSuite -Name 'website (astro check)' -Block { Invoke-NpmScript -RelativeDir 'website' -ScriptName 'check' }
	Invoke-TestSuite -Name 'website (vitest)' -Block {
		Invoke-NpmScript -RelativeDir 'website' -ScriptName 'test' -FailureParser 'vitest'
	}

	if ($IncludeE2e) {
		Initialize-E2eEnv
		Ensure-GameSolutionBuilt
		Invoke-TestSuite -Name 'visual-tool (playwright e2e)' -Block {
			Invoke-NpmScript -RelativeDir 'tools\visual-tool' -ScriptName 'test:e2e' -FailureParser 'playwright'
		}
		Invoke-TestSuite -Name 'website (build for preview)' -Block {
			Invoke-NpmScript -RelativeDir 'website' -ScriptName 'build'
		}
		Invoke-TestSuite -Name 'website (playwright e2e)' -Block {
			Invoke-NpmScript -RelativeDir 'website' -ScriptName 'test:e2e' -FailureParser 'playwright'
		}
	}
	else {
		Add-TestResult -Name 'visual-tool (playwright e2e)' -Status 'SKIP' -Detail 'run test-e2e or pass -IncludeE2e'
		Add-TestResult -Name 'website (playwright e2e)' -Status 'SKIP' -Detail 'run test-e2e or pass -IncludeE2e'
		Write-Host ''
		Write-Host 'Skipped Playwright e2e (use test-e2e or pass IncludeE2e).' -ForegroundColor Yellow
	}

	Write-TestSummary -Scope 'all'
}

switch ($Action) {
	'restart-dev' { Invoke-RestartDev }
	'restart-prod' { Invoke-RestartProd }
	'restart-local-llm' { Invoke-RestartLocalLlm }
	'commit' {
		Invoke-GitCommitAll
		Invoke-RestartDev
	}
	'push' {
		Invoke-GitCommitAll
		Invoke-GitPushCurrent
		Invoke-RestartProd
	}
	'merge' {
		Invoke-GitCommitAll
		Invoke-GitPushCurrent
		Invoke-GitMergeToDefault
		Invoke-RestartProd
	}
	'test' { Invoke-RunAllTests }
	'test-e2e' { Invoke-RunE2eTests }
}

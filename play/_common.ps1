# Shared helpers for play/*.ps1. Dot-source from the same folder.
$ErrorActionPreference = 'Stop'

$script:RepoRoot = Split-Path -Parent $PSScriptRoot
$script:Encoding1251 = [System.Text.Encoding]::GetEncoding(1251)
$script:PlayerFactionIds = 2..11

# Default inference: Ollama in Docker on localhost:11434 (container name "ollama").
$script:DefaultOllamaHost = 'http://127.0.0.1:11434'
$script:LocalDefaultChatModel = 'qwen2.5-coder:7b'
$script:RunPodDefaultChatModel = 'qwen3-coder:30b'
$script:DefaultEmbedModel = 'nomic-embed-text'

function Test-RemoteOllamaHost {
	param([Parameter(Mandatory = $true)][string]$HostUrl)
	if ([string]::IsNullOrWhiteSpace($HostUrl)) {
		return $false
	}
	try {
		$uri = [Uri]$HostUrl.TrimEnd('/')
		if ($uri.Scheme -ne 'http' -and $uri.Scheme -ne 'https') {
			return $true
		}
		$hostName = $uri.Host
		return -not (
			$hostName -eq 'localhost' -or
			$hostName -eq '127.0.0.1' -or
			$hostName -eq '::1'
		)
	}
	catch {
		return $true
	}
}

function Resolve-PlayerAgentChatModel {
	param(
		[string]$Configured,
		[string]$OllamaHost
	)
	if ([string]::IsNullOrWhiteSpace($Configured) -or $Configured.Trim().ToLowerInvariant() -eq 'auto') {
		if (Test-RemoteOllamaHost -HostUrl $OllamaHost) {
			return $script:RunPodDefaultChatModel
		}
		return $script:LocalDefaultChatModel
	}
	return $Configured.Trim()
}

function Import-RepoEnv {
	$envFile = Join-Path $script:RepoRoot '.env'
	if (-not (Test-Path -LiteralPath $envFile)) {
		return
	}
	Get-Content -LiteralPath $envFile | ForEach-Object {
		if ($_ -match '^\s*([^#=]+)=(.*)$') {
			$name = $Matches[1].Trim()
			$value = $Matches[2].Trim().Trim('"').Trim("'")
			if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name, 'Process'))) {
				Set-Item -Path "Env:$name" -Value $value
			}
		}
	}
}

function Initialize-OllamaEnv {
	Import-RepoEnv
	if ([string]::IsNullOrWhiteSpace($env:OLLAMA_HOST)) {
		$env:OLLAMA_HOST = $script:DefaultOllamaHost
	}
	$env:PLAYER_AGENT_CHAT_MODEL = Resolve-PlayerAgentChatModel `
		-Configured $env:PLAYER_AGENT_CHAT_MODEL `
		-OllamaHost $env:OLLAMA_HOST
	if ([string]::IsNullOrWhiteSpace($env:PLAYER_AGENT_EMBED_MODEL)) {
		$env:PLAYER_AGENT_EMBED_MODEL = $script:DefaultEmbedModel
	}
}

function Test-OllamaDocker {
	param(
		[switch] $Quiet
	)
	Initialize-OllamaEnv
	$base = $env:OLLAMA_HOST.TrimEnd('/')
	try {
		$tags = Invoke-RestMethod -Uri "$base/api/tags" -TimeoutSec 10
	}
	catch {
		throw "Ollama not reachable at $base. Start the Docker container: docker start ollama (image ollama/ollama, port 11434)."
	}
	$names = @($tags.models | ForEach-Object { $_.name })
	$chat = $env:PLAYER_AGENT_CHAT_MODEL
	$embed = $env:PLAYER_AGENT_EMBED_MODEL
	$chatOk = $names | Where-Object { $_ -eq $chat -or $_ -eq "${chat}:latest" -or $_ -like "$chat*" }
	$embedOk = $names | Where-Object { $_ -eq $embed -or $_ -eq "${embed}:latest" -or $_ -like "$embed*" }
	if (-not $chatOk) {
		throw "Chat model '$chat' not in Ollama Docker. Pull inside container: docker exec -it ollama ollama pull $chat"
	}
	if (-not $embedOk) {
		throw "Embed model '$embed' not in Ollama Docker. Pull inside container: docker exec -it ollama ollama pull $embed"
	}
	if (-not $Quiet) {
		Write-Host "Ollama Docker OK at $base (chat=$chat, embed=$embed)"
	}
}

function Test-RunId {
	param([Parameter(Mandatory = $true)][string]$RunId)
	if ($RunId -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]{0,31}$') {
		throw "Invalid -RunId '$RunId'. Must match ^[A-Za-z0-9][A-Za-z0-9._-]{0,31}$ (no path separators or '..')."
	}
}

function Get-RunRoot {
	param([Parameter(Mandatory = $true)][string]$RunId)
	Test-RunId -RunId $RunId
	return Join-Path (Join-Path $script:RepoRoot 'play\runs') $RunId
}

function Get-RunPaths {
	param([Parameter(Mandatory = $true)][string]$RunId)
	$root = Get-RunRoot -RunId $RunId
	return @{
		RunRoot     = $root
		DataDir     = Join-Path $root 'data'
		TurnDir     = Join-Path $root 'turn'
		FactionsDir = Join-Path $root 'factions'
	}
}

function Get-FactionFolderName {
	param([Parameter(Mandatory = $true)][int]$Id)
	return ('{0:D2}' -f $Id)
}

function Get-FactionOrdersFolder {
	param(
		[Parameter(Mandatory = $true)][int]$FactionId,
		[Parameter(Mandatory = $true)][hashtable]$Paths
	)
	if ($FactionId -eq 1) {
		return Join-Path $Paths.RunRoot 'gm'
	}
	return Join-Path $Paths.FactionsDir (Get-FactionFolderName -Id $FactionId)
}

function Get-DefaultGameExe {
	return Join-Path $script:RepoRoot 'Game\bin\Debug\Game.exe'
}

function Resolve-GameExe {
	param([string]$Exe)
	if ([string]::IsNullOrWhiteSpace($Exe)) {
		$Exe = Get-DefaultGameExe
	}
	if (-not (Test-Path -LiteralPath $Exe)) {
		throw "Game.exe not found at '$Exe'. Build with ``nuget restore SpaceAge.sln`` then ``msbuild SpaceAge.sln /p:Configuration=Debug``."
	}
	return (Resolve-Path -LiteralPath $Exe).Path
}

function Invoke-GameExe {
	param(
		[string]$Exe,
		[Parameter(Mandatory = $true)][string[]]$GameArgs
	)
	$resolved = Resolve-GameExe -Exe $Exe
	& $resolved @GameArgs
	$code = $LASTEXITCODE
	if ($null -eq $code -or $code -ne 0) {
		throw "Game.exe exited with code $code ($resolved $($GameArgs -join ' '))"
	}
}

function Read-Win1251Text {
	param([Parameter(Mandatory = $true)][string]$Path)
	return [System.IO.File]::ReadAllText($Path, $script:Encoding1251)
}

function Write-Win1251Text {
	param(
		[Parameter(Mandatory = $true)][string]$Path,
		[Parameter(Mandatory = $true)][string]$Text
	)
	[System.IO.File]::WriteAllText($Path, $Text, $script:Encoding1251)
}

function Read-Utf8Text {
	param([Parameter(Mandatory = $true)][string]$Path)
	$reader = New-Object System.IO.StreamReader($Path, [System.Text.Encoding]::UTF8, $true)
	try {
		return $reader.ReadToEnd()
	}
	finally {
		$reader.Close()
	}
}

function Write-Utf8Text {
	param(
		[Parameter(Mandatory = $true)][string]$Path,
		[Parameter(Mandatory = $true)][string]$Text
	)
	$utf8 = New-Object System.Text.UTF8Encoding $false
	[System.IO.File]::WriteAllText($Path, $Text, $utf8)
}

function Get-TurnFromGamein {
	param([Parameter(Mandatory = $true)][string]$GameinPath)
	if (-not (Test-Path -LiteralPath $GameinPath)) {
		return $null
	}
	$text = Read-Win1251Text -Path $GameinPath
	$m = [regex]::Match($text, '<game\b[^>]*\bturn="(\d+)"')
	if ($m.Success) {
		return [int]$m.Groups[1].Value
	}
	return $null
}

function Get-LatestIsolatedReportTurn {
	param([Parameter(Mandatory = $true)][string]$FactionsDir)
	$maxTurn = $null
	foreach ($id in $script:PlayerFactionIds) {
		$folder = Join-Path $FactionsDir (Get-FactionFolderName -Id $id)
		if (-not (Test-Path -LiteralPath $folder)) {
			continue
		}
		Get-ChildItem -LiteralPath $folder -Filter 'report.*.*.txt' -File -ErrorAction SilentlyContinue | ForEach-Object {
			if ($_.Name -match '^report\.(\d+)\.\d+\.txt$') {
				$t = [int]$Matches[1]
				if ($null -eq $maxTurn -or $t -gt $maxTurn) {
					$maxTurn = $t
				}
			}
		}
	}
	return $maxTurn
}

function Test-NonEmptyOrderFile {
	param([Parameter(Mandatory = $true)][string]$Path)
	if (-not (Test-Path -LiteralPath $Path)) {
		return $false
	}
	try {
		$text = Read-Utf8Text -Path $Path
	}
	catch {
		$text = Read-Win1251Text -Path $Path
	}
	return -not [string]::IsNullOrWhiteSpace($text)
}

function Get-LatestVersionedOrderPath {
	param(
		[Parameter(Mandatory = $true)][int]$FactionId,
		[Parameter(Mandatory = $true)][int]$OrderTurn,
		[Parameter(Mandatory = $true)][string]$FactionFolder
	)
	if (-not (Test-Path -LiteralPath $FactionFolder)) {
		return $null
	}
	$pattern = "^orders\.$FactionId\.$OrderTurn\.(\d+)\.txt$"
	$bestPath = $null
	$bestVersion = -1
	foreach ($file in Get-ChildItem -LiteralPath $FactionFolder -Filter 'orders.*.txt' -File -ErrorAction SilentlyContinue) {
		if ($file.Name -match $pattern) {
			$version = [int]$Matches[1]
			if ($version -gt $bestVersion) {
				$bestVersion = $version
				$bestPath = $file.FullName
			}
		}
	}
	return $bestPath
}

function Resolve-FactionOrderSourcePath {
	param(
		[Parameter(Mandatory = $true)][int]$FactionId,
		[Parameter(Mandatory = $true)][hashtable]$Paths
	)
	$gamein = Join-Path $Paths.DataDir 'gamein.xml'
	$turn = Get-TurnFromGamein -GameinPath $gamein
	if ($null -eq $turn -or $turn -le 0) {
		$turn = 1
	}
	$folder = Get-FactionOrdersFolder -FactionId $FactionId -Paths $Paths

	$reportTurn = 0
	foreach ($reportFile in Get-ChildItem -LiteralPath $folder -Filter "report.*.$FactionId.txt" -File -ErrorAction SilentlyContinue) {
		if ($reportFile.Name -match '^report\.(\d+)\.') {
			$rt = [int]$Matches[1]
			if ($rt -gt $reportTurn) { $reportTurn = $rt }
		}
	}
	$tryTurns = @($turn)
	if ($reportTurn -gt 0 -and $reportTurn -ne $turn) { $tryTurns += $reportTurn }
	if ($reportTurn -gt 0) { $tryTurns += ($reportTurn + 1) }
	if ($turn -gt 0) { $tryTurns += ($turn + 1) }
	$tryTurns = $tryTurns | Select-Object -Unique
	foreach ($orderTurn in $tryTurns) {
		$versioned = Get-LatestVersionedOrderPath -FactionId $FactionId -OrderTurn $orderTurn -FactionFolder $folder
		if ($versioned) {
			return $versioned
		}
	}

	$legacyPath = Join-Path $folder ("order.{0}.txt" -f $FactionId)
	if (Test-NonEmptyOrderFile -Path $legacyPath) {
		return $legacyPath
	}

	return $null
}

function Test-FactionOrdersSubmitted {
	param(
		[Parameter(Mandatory = $true)][int]$FactionId,
		[Parameter(Mandatory = $true)][int]$Turn,
		[Parameter(Mandatory = $true)][hashtable]$Paths
	)
	$folder = Get-FactionOrdersFolder -FactionId $FactionId -Paths $Paths
	$draftPath = Join-Path $folder ("order.{0}.txt" -f $FactionId)
	if (Test-NonEmptyOrderFile -Path $draftPath) {
		return $true
	}

	if (-not (Test-Path -LiteralPath $folder)) {
		return $false
	}

	foreach ($orderTurn in @($Turn, ($Turn + 1))) {
		$pattern = "^orders\.$FactionId\.$orderTurn\.(\d+)\.txt$"
		foreach ($file in Get-ChildItem -LiteralPath $folder -Filter 'orders.*.txt' -File -ErrorAction SilentlyContinue) {
			if ($file.Name -match $pattern -and (Test-NonEmptyOrderFile -Path $file.FullName)) {
				return $true
			}
		}
	}

	return $false
}

function Test-AllIsolatedReportsPresent {
	param(
		[Parameter(Mandatory = $true)][string]$FactionsDir,
		[Parameter(Mandatory = $true)][int]$Turn
	)
	foreach ($id in $script:PlayerFactionIds) {
		$path = Join-Path (Join-Path $FactionsDir (Get-FactionFolderName -Id $id)) ("report.{0}.{1}.txt" -f $Turn, $id)
		if (-not (Test-Path -LiteralPath $path)) {
			return $false
		}
	}
	return $true
}

function Update-WebsiteStatus {
	param(
		[Parameter(Mandatory = $true)][string]$RunId,
		[string]$OutPath,
		[string]$NextTurnAt
	)
	$args = @($RunId)
	if (-not [string]::IsNullOrWhiteSpace($OutPath)) {
		$args += @('-OutPath', $OutPath)
	}
	if ($PSBoundParameters.ContainsKey('NextTurnAt')) {
		$args += @('-NextTurnAt', $NextTurnAt)
	}
	& (Join-Path $PSScriptRoot 'generate-status.ps1') @args
}

$script:PlayRunLogPath = $null

function Initialize-PlayRunLog {
	param(
		[Parameter(Mandatory = $true)][string]$RunId,
		[Parameter(Mandatory = $true)][string]$LogFileName
	)
	$paths = Get-RunPaths -RunId $RunId
	$gmDir = Join-Path $paths.RunRoot 'gm'
	if (-not (Test-Path -LiteralPath $gmDir)) {
		New-Item -ItemType Directory -Path $gmDir -Force | Out-Null
	}
	$script:PlayRunLogPath = Join-Path $gmDir $LogFileName
	$started = Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz'
	"=== log started $started ===" | Out-File -LiteralPath $script:PlayRunLogPath -Encoding utf8 -Force
}

function Write-PlayRunLog {
	param(
		[Parameter(Mandatory = $true)][string]$Message,
		[ConsoleColor] $Color = 'Gray'
	)
	$ts = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
	$line = "[$ts] $Message"
	if ($Color -eq 'Gray') {
		Write-Host $line
	}
	else {
		Write-Host $line -ForegroundColor $Color
	}
	if ($script:PlayRunLogPath) {
		Add-Content -LiteralPath $script:PlayRunLogPath -Value $line -Encoding UTF8
	}
}

function Get-ContractorFactionIds {
	param(
		[Parameter(Mandatory = $true)][string]$RunId,
		[int[]]$ExcludeFaction = @(2, 3),
		[int]$FromFaction = 2,
		[int]$ToFaction = 11
	)
	$paths = Get-RunPaths -RunId $RunId
	$ids = [System.Collections.Generic.List[int]]::new()
	for ($id = $FromFaction; $id -le $ToFaction; $id++) {
		if ($ExcludeFaction -contains $id) { continue }
		$persona = Join-Path (Join-Path $paths.FactionsDir (Get-FactionFolderName -Id $id)) 'persona.md'
		if (-not (Test-Path -LiteralPath $persona)) { continue }
		$text = Get-Content -LiteralPath $persona -Raw -Encoding UTF8
		if ($text -match '(?m)^##\s*Preference:\s*contractor\s*$') {
			$ids.Add($id)
		}
	}
	return $ids.ToArray()
}

function Get-EconomicFactionIds {
	param(
		[Parameter(Mandatory = $true)][string]$RunId,
		[int[]]$ExcludeFaction = @(2, 3),
		[int]$FromFaction = 2,
		[int]$ToFaction = 11
	)
	$paths = Get-RunPaths -RunId $RunId
	$ids = [System.Collections.Generic.List[int]]::new()
	for ($id = $FromFaction; $id -le $ToFaction; $id++) {
		if ($ExcludeFaction -contains $id) { continue }
		$persona = Join-Path (Join-Path $paths.FactionsDir (Get-FactionFolderName -Id $id)) 'persona.md'
		if (-not (Test-Path -LiteralPath $persona)) { continue }
		$text = Get-Content -LiteralPath $persona -Raw -Encoding UTF8
		if ($text -match '(?m)^##\s*Preference:\s*economic\s*$') {
			$ids.Add($id)
		}
	}
	return $ids.ToArray()
}

function Get-MilitaryFactionIds {
	param(
		[Parameter(Mandatory = $true)][string]$RunId,
		[int[]]$ExcludeFaction = @(2, 3),
		[int]$FromFaction = 2,
		[int]$ToFaction = 11
	)
	$paths = Get-RunPaths -RunId $RunId
	$ids = [System.Collections.Generic.List[int]]::new()
	for ($id = $FromFaction; $id -le $ToFaction; $id++) {
		if ($ExcludeFaction -contains $id) { continue }
		$persona = Join-Path (Join-Path $paths.FactionsDir (Get-FactionFolderName -Id $id)) 'persona.md'
		if (-not (Test-Path -LiteralPath $persona)) { continue }
		$text = Get-Content -LiteralPath $persona -Raw -Encoding UTF8
		if ($text -match '(?m)^##\s*Preference:\s*military\s*$') {
			$ids.Add($id)
		}
	}
	return $ids.ToArray()
}

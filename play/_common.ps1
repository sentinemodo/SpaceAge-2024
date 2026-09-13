# Shared helpers for play/*.ps1. Dot-source from the same folder.
$ErrorActionPreference = 'Stop'

$script:RepoRoot = Split-Path -Parent $PSScriptRoot
$script:Encoding1251 = [System.Text.Encoding]::GetEncoding(1251)
$script:PlayerFactionIds = 2..11

# Default inference: Ollama in Docker on localhost:11434 (container name "ollama").
$script:DefaultOllamaHost = 'http://127.0.0.1:11434'
$script:DefaultChatModel = 'qwen2.5-coder:7b'
$script:DefaultEmbedModel = 'nomic-embed-text'

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
	if ([string]::IsNullOrWhiteSpace($env:PLAYER_AGENT_CHAT_MODEL)) {
		$env:PLAYER_AGENT_CHAT_MODEL = $script:DefaultChatModel
	}
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
	$args = @('-RunId', $RunId)
	if (-not [string]::IsNullOrWhiteSpace($OutPath)) {
		$args += @('-OutPath', $OutPath)
	}
	if ($PSBoundParameters.ContainsKey('NextTurnAt')) {
		$args += @('-NextTurnAt', $NextTurnAt)
	}
	& (Join-Path $PSScriptRoot 'generate-status.ps1') @args
}

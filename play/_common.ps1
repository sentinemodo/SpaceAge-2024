# Shared helpers for play/*.ps1. Dot-source from the same folder.
$ErrorActionPreference = 'Stop'

$script:RepoRoot = Split-Path -Parent $PSScriptRoot
$script:Encoding1251 = [System.Text.Encoding]::GetEncoding(1251)
$script:PlayerFactionIds = 2..11

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

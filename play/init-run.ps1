# Copy campaign catalog + seed into a new run; assign passwords and personas.
param(
	[Parameter(Mandatory = $true, Position = 0)]
	[string]$RunId,

	[string]$Exe,

	[int]$Seed,

	[switch]$Force
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')
# -Exe is accepted for a consistent CLI with reports.ps1 / turn.ps1; init does not launch Game.exe.
$null = $Exe

$FactionMeta = @{
	2  = @{ Name = 'Northwind'; System = 'Helios'; World = 'Arbor' }
	3  = @{ Name = 'Greenwell'; System = 'Helios'; World = 'Arbor' }
	4  = @{ Name = 'Rivermark'; System = 'Helios'; World = 'Arbor' }
	5  = @{ Name = 'Sundock'; System = 'Helios'; World = 'Arbor' }
	6  = @{ Name = 'Copse'; System = 'Helios'; World = 'Arbor' }
	7  = @{ Name = 'Ironclad'; System = 'Fomal'; World = 'Anvil' }
	8  = @{ Name = 'Oreline'; System = 'Fomal'; World = 'Anvil' }
	9  = @{ Name = 'Basalt'; System = 'Fomal'; World = 'Anvil' }
	10 = @{ Name = 'Silicate'; System = 'Fomal'; World = 'Anvil' }
	11 = @{ Name = 'Fission'; System = 'Fomal'; World = 'Anvil' }
}

$PreferenceMeans = @{
	military   = 'Build and move `inftry` and `tanks`. Use `ATTACK`, `CAPTURE`, and `DECLARE FACTION <id> ENEMY`. Cross Helios Gate `P00009` <-> Fomal Gate `P00010` with `JUMP` once you have a ship on the Gate orbit.'
	economic   = 'USE extractors and farms on local mass and calories. `BUY` / `SELL` at UN markets (Assembly on Arbor, Slagport on Anvil). Push surplus through spaceports when you have hulls.'
	researcher = '`RESEARCH` at labs. Take UN wreck charters (`CONTRACT` / `research` on belt hulks). `SEE` foreign tech; `COPY` onto a receiver at the same location.'
	contractor = 'File UN `CONTRACT` / `give-module` jobs first (food, wind, drills). Spend rewards on trade and the same live verbs as economic.'
}

$PasswordCharset = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789'
$Preferences = @('military', 'economic', 'researcher', 'contractor')

function New-RunPassword {
	param([Parameter(Mandatory = $true)][System.Random]$Rng)
	$chars = New-Object char[] 10
	for ($i = 0; $i -lt 10; $i++) {
		$chars[$i] = $PasswordCharset[$Rng.Next($PasswordCharset.Length)]
	}
	return -join $chars
}

function Get-PersonaMarkdown {
	param(
		[Parameter(Mandatory = $true)][int]$Id,
		[Parameter(Mandatory = $true)][string]$Password,
		[Parameter(Mandatory = $true)][string]$Preference
	)
	$meta = $FactionMeta[$Id]
	$means = $PreferenceMeans[$Preference]
	return @"
# $($meta.Name) (faction $Id)

You are the charter board of **$($meta.Name)**, Interest $Id.
Home: **$($meta.World)** in **$($meta.System)** (Helios factions 2-6, Fomal factions 7-11).
United Star Nations is faction 1. Arbor First is 12 (Arbor). HCS is 13 (Anvil).
Those three are NPC this slice; they file no ``order.*``.

## Credentials

- Faction id: ``$Id``
- Password: ``$Password``
- Orders header (Windows-1251 in ``/turn-dir``): ``#faction $Id "$Password"``

Do not publish this password. Do not put it in ``play/README.md``.

## Preference: $Preference

$means

## Doctrine

Mass and energy, not myth. No FTL except Alderson ``JUMP`` between the paired Gates.
No psionics. Alien wrecks are materials, closed-cycle hardware, and high-Isp physics.

1. **Explore** the local hinterland on $($meta.World), then the Gate orbit.
2. **Exploit** the complementary diet: Arbor organics (food, carbon, oil) vs Anvil metals (titani, copper, uraniu). Neither start holds a full industrial slate -- trade, ``CONTRACT``, or fly.
3. **Conquer** militia cities after they flip (wishlist / GM later), then other corps -- or **ally**.

## Win

- **Solitary:** every other player ``corphq`` (factions 2-11 except you) is gone.
- **Bloc:** a surviving set of Interests where **each pair** has mutual ``DECLARE FACTION <id> ALLY``. Alliance is one-way until both sides declare.

## Isolation

You may read only this folder: ``persona.md``, your ``report.*.$Id.txt``, and your ``order.$Id.txt``.

Do **not** open other ``factions/NN/`` reports, ``campaign/data.xml``, ``data/data.xml``, ``data/gamein.xml``, ``data/gameout.*.xml``, ``campaign/gamein.1.xml``, or any ``report.*.xml`` (XML leaks foreign cargo and techs).

## Catalog

Do **not** open ``campaign/data.xml`` or this run's ``data/data.xml``. Tell ``/player`` the catalog path is ``campaign/data.xml`` (not ``Tests/data.xml``). Use the text report, this persona, ``player/rules.md``, and ``player/campaign/basic_technologies.md`` when that excerpt exists.
"@
}

$campaignData = Join-Path $script:RepoRoot 'campaign\data.xml'
$campaignGamein = Join-Path $script:RepoRoot 'campaign\gamein.1.xml'
if (-not (Test-Path -LiteralPath $campaignData)) {
	throw "Missing campaign catalog: $campaignData"
}
if (-not (Test-Path -LiteralPath $campaignGamein)) {
	throw "Missing campaign seed: $campaignGamein"
}

$paths = Get-RunPaths -RunId $RunId
if (Test-Path -LiteralPath $paths.RunRoot) {
	if (-not $Force) {
		throw "Run '$RunId' already exists at '$($paths.RunRoot)'. Pass -Force to replace it."
	}
	Remove-Item -LiteralPath $paths.RunRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $paths.DataDir -Force | Out-Null
New-Item -ItemType Directory -Path $paths.TurnDir -Force | Out-Null
foreach ($id in $script:PlayerFactionIds) {
	$folder = Join-Path $paths.FactionsDir (Get-FactionFolderName -Id $id)
	New-Item -ItemType Directory -Path $folder -Force | Out-Null
}

Copy-Item -LiteralPath $campaignData -Destination (Join-Path $paths.DataDir 'data.xml')
Copy-Item -LiteralPath $campaignGamein -Destination (Join-Path $paths.DataDir 'gamein.xml')

if ($PSBoundParameters.ContainsKey('Seed')) {
	$rng = New-Object System.Random $Seed
}
else {
	$rng = New-Object System.Random
}

$usedPasswords = @{}
$passwords = @{}
foreach ($id in $script:PlayerFactionIds) {
	$pw = $null
	for ($attempt = 0; $attempt -lt 100; $attempt++) {
		$candidate = New-RunPassword -Rng $rng
		if (-not $usedPasswords.ContainsKey($candidate)) {
			$pw = $candidate
			break
		}
	}
	if ($null -eq $pw) {
		throw "Could not generate a unique password for faction $id."
	}
	$usedPasswords[$pw] = $true
	$passwords[$id] = $pw
}

$gameinPath = Join-Path $paths.DataDir 'gamein.xml'
$gameinText = Read-Win1251Text -Path $gameinPath
foreach ($id in $script:PlayerFactionIds) {
	$pattern = '(<faction\b(?=[^>]*\bname="' + $id + '")[^>]*\bpassword=")[^"]*(")'
	$regex = New-Object System.Text.RegularExpressions.Regex($pattern)
	if ($regex.Matches($gameinText).Count -ne 1) {
		throw "Password regex missed faction $id in $gameinPath."
	}
	$replacement = '${1}' + $passwords[$id] + '${2}'
	$gameinText = $regex.Replace($gameinText, $replacement, 1)
}
Write-Win1251Text -Path $gameinPath -Text $gameinText

foreach ($id in $script:PlayerFactionIds) {
	$preference = $Preferences[$rng.Next($Preferences.Length)]
	$persona = Get-PersonaMarkdown -Id $id -Password $passwords[$id] -Preference $preference
	$personaPath = Join-Path (Join-Path $paths.FactionsDir (Get-FactionFolderName -Id $id)) 'persona.md'
	Write-Utf8Text -Path $personaPath -Text $persona
}

Write-Host "Initialized run '$RunId' at $($paths.RunRoot)"
Write-Host "Passwords are in factions/NN/persona.md (not in play/README.md)."

# Copy campaign catalog + seed into a new run; assign passwords and personas.
param(
	[Parameter(Mandatory = $true, Position = 0)]
	[string]$RunId,

	[string]$Exe,

	[int]$Seed,

	[switch]$Force,

	[hashtable]$PreferenceOverrides
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
	military   = @'
Startup: factory copy of **`armcbt`** (armored combat) costs **1000 balance** at init (9000 cash on hand). Do **not** blanket-declare fauna factions (14-17) hostile before contact — you have not met them yet. After a rumor or scout sighting, **`DECLARE FACTION 14 ENEMY`** (or the local fauna id) then engage. Oil is strategic - secure the nearest **oil** pocket with a defendable outpost before the grant thins.

**Priority queue:** (1) **`use armcbt`** to build a **tanks** squad; (2) build **trucks** and scout *safe* adjacent grants; (3) after fauna contact, send **tanks** to clear pockets and escort columns; (4) **`cplant` / `fossil` energy complexes** to feed **barracks**; (5) deploy **barracks** and **`frminf`** infantry from them.

Defend every region you occupy. Escort logistics except the first lone scouting truck. Scout contested ground with **tanks**, not trucks. Fauna culls yield battle loot; UN tier-1 bounties (**CT0016** Arbor, **CT0019** Anvil) pay **1000 cash** (default ladder 1000/2000/4000 by tier if UN posts more later).

Use `ATTACK`, `CAPTURE`, and `DECLARE FACTION <id> ENEMY` when diplomacy warrants. Cross Helios Gate `P00009` <-> Fomal Gate `P00010` with `JUMP` once you have a ship on the Gate orbit.
'@
	economic   = 'Startup: **surface drill only** on HQ; factory copy of **`cdrill`** costs **1000 balance** at init (9000 cash on hand). **`use cdrill`** to build the first **core drill**, then stack more **core drills** and **`agrplx` farms** on the grant. **Energy first:** keep **`cplant`** (or add **`fossil`**) running before scaling extraction. Scout with **`moblib` to `moblab`** carrying a **`cdrill` technology copy** (not trucks): adjacent exits show **deep pocket of resources detected** once the factory copy is present; move the lab into the pocket cell to read **Deep resources:** assays. Defer UN town charters until the home grant is production-maxed. Trade at UN markets when local mass is thin.'
	researcher = 'Build **`moblib` to `moblab` first** on the factory copy, `@get` crew, food, and **oil** from HQ cargo, then `@move` to the adjacent grant anomaly and `@research` it (8 pt / +20 RP). HQ cargo seeds **5 oil** for ground fuel (same as `trucks`). Defer town charters until the survey column moves. Later: `filidx`, `frminf` escort, silici scouting. Wreck charters (`CONTRACT` / `research` on belt hulks) when staged.'
	contractor = 'File UN `CONTRACT` / `give-module` jobs first (food, wind, drills). Spend rewards on trade and the same live verbs as economic.'
}

$PasswordCharset = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789'
$PreferencePool = @('military', 'economic', 'researcher', 'contractor')

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
Fauna factions 14-17 (wildlife per planet) are NPC; they file no ``order.*`` and start neutral until you declare or fight them.

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

function Get-FactoryStackId {
	param([Parameter(Mandatory = $true)][int]$FactionId)
	return [string](200000 + ($FactionId - 2) * 10000 + 5)
}

function Get-CargoStackId {
	param([Parameter(Mandatory = $true)][int]$FactionId)
	return [string](200000 + ($FactionId - 2) * 10000 + 3)
}

function Add-EconomicStartupToGamein {
	param(
		[Parameter(Mandatory = $true)][string]$GameinText,
		[Parameter(Mandatory = $true)][int]$FactionId
	)
	$text = $GameinText
	$balancePattern = '(<faction\b(?=[^>]*\bname="' + $FactionId + '")[^>]*\bbalance=")10000(")'
	$text = [regex]::Replace($text, $balancePattern, '${1}9000${2}', 1)

	$stackId = Get-FactoryStackId -FactionId $FactionId
	if ($text -match ('<modulestack name="' + [regex]::Escape($stackId) + '"[\s\S]*?<technology name="cdrill"')) {
		return $text
	}

	$stackOpen = '(<modulestack name="' + [regex]::Escape($stackId) + '" type="factry" quantity="2" faction="' + $FactionId + '">)'
	$replacement = '${1}' + "`n`t`t`t`t`t`t<technology name=`"cdrill`" name-en=`"mineral core drilling`" />"
	$text = [regex]::Replace($text, $stackOpen, $replacement, 1)
	if ($text -eq $GameinText) {
		throw "Economic startup: factory stack $stackId not found for faction $FactionId."
	}

	$cargoId = Get-CargoStackId -FactionId $FactionId
	$titaniPattern = '(<modulestack name="' + [regex]::Escape($cargoId) + '"[\s\S]*?<itemstack type="titani" quantity=")2(" />)'
	$text = [regex]::Replace($text, $titaniPattern, '${1}10${2}', 1)
	return $text
}

function Add-MilitaryStartupToGamein {
	param(
		[Parameter(Mandatory = $true)][string]$GameinText,
		[Parameter(Mandatory = $true)][int]$FactionId
	)
	$text = $GameinText
	$balancePattern = '(<faction\b(?=[^>]*\bname="' + $FactionId + '")[^>]*\bbalance=")10000(")'
	$text = [regex]::Replace($text, $balancePattern, '${1}9000${2}', 1)

	$stackId = Get-FactoryStackId -FactionId $FactionId
	if ($text -match ('<modulestack name="' + [regex]::Escape($stackId) + '"[\s\S]*?<technology name="armcbt"')) {
		return $text
	}

	$stackOpen = '(<modulestack name="' + [regex]::Escape($stackId) + '" type="factry" quantity="2" faction="' + $FactionId + '">)'
	$replacement = '${1}' + "`n`t`t`t`t`t`t<technology name=`"armcbt`" name-en=`"armored combat`" />"
	$text = [regex]::Replace($text, $stackOpen, $replacement, 1)
	if ($text -eq $GameinText) {
		throw "Military startup: factory stack $stackId not found for faction $FactionId."
	}
	return $text
}

function Add-ResearcherStartupToGamein {
	param(
		[Parameter(Mandatory = $true)][string]$GameinText,
		[Parameter(Mandatory = $true)][int]$FactionId
	)
	$text = $GameinText
	$balancePattern = '(<faction\b(?=[^>]*\bname="' + $FactionId + '")[^>]*\bbalance=")10000(")'
	$text = [regex]::Replace($text, $balancePattern, '${1}9000${2}', 1)

	$stackId = Get-FactoryStackId -FactionId $FactionId
	if ($text -match ('<modulestack name="' + [regex]::Escape($stackId) + '"[\s\S]*?<technology name="moblib"')) {
		return $text
	}

	$stackOpen = '(<modulestack name="' + [regex]::Escape($stackId) + '" type="factry" quantity="2" faction="' + $FactionId + '">)'
	$replacement = '${1}' + "`n`t`t`t`t`t`t<technology name=`"moblib`" name-en=`"mobile laboratory`" />"
	$text = [regex]::Replace($text, $stackOpen, $replacement, 1)
	if ($text -eq $GameinText) {
		throw "Researcher startup: factory stack $stackId not found for faction $FactionId."
	}
	return $text
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

$preferences = @{}
foreach ($id in $script:PlayerFactionIds) {
	$preferences[$id] = $PreferencePool[$rng.Next($PreferencePool.Length)]
}
if ($null -ne $PreferenceOverrides) {
	foreach ($entry in $PreferenceOverrides.GetEnumerator()) {
		$overrideId = [int]$entry.Key
		$overridePref = [string]$entry.Value
		if ($overridePref -notin $PreferencePool) {
			throw "PreferenceOverrides[$overrideId] must be one of: $($PreferencePool -join ', ')."
		}
		if ($overrideId -notin $script:PlayerFactionIds) {
			throw "PreferenceOverrides key $overrideId is not a player faction (2-11)."
		}
		$preferences[$overrideId] = $overridePref
	}
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
foreach ($id in $script:PlayerFactionIds) {
	if ($preferences[$id] -eq 'economic') {
		$gameinText = Add-EconomicStartupToGamein -GameinText $gameinText -FactionId $id
	}
	elseif ($preferences[$id] -eq 'researcher') {
		$gameinText = Add-ResearcherStartupToGamein -GameinText $gameinText -FactionId $id
	}
	elseif ($preferences[$id] -eq 'military') {
		$gameinText = Add-MilitaryStartupToGamein -GameinText $gameinText -FactionId $id
	}
}
Write-Win1251Text -Path $gameinPath -Text $gameinText

foreach ($id in $script:PlayerFactionIds) {
	$persona = Get-PersonaMarkdown -Id $id -Password $passwords[$id] -Preference $preferences[$id]
	$personaPath = Join-Path (Join-Path $paths.FactionsDir (Get-FactionFolderName -Id $id)) 'persona.md'
	Write-Utf8Text -Path $personaPath -Text $persona
}

Write-Host "Initialized run '$RunId' at $($paths.RunRoot)"
Write-Host "Passwords are in factions/NN/persona.md (not in play/README.md)."

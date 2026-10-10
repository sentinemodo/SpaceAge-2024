export type BattleOutcome = 'attackers' | 'defenders' | 'indecisive' | 'unknown';

export interface CombatantStats {
  id: string;
  label: string;
  hits: number;
  misses: number;
  damageDealt: number;
  damageReceived: number;
  captureDamageDealt: number;
}

export interface BattleCapture {
  subject: string;
  captor: string;
}

export interface BattleStats {
  combatants: CombatantStats[];
  totalHits: number;
  totalMisses: number;
  captures: BattleCapture[];
  destroyed: string[];
  wrecked: string[];
  loot: string[];
}

export interface ParsedBattle {
  id: string;
  week: number;
  locationLabel: string;
  regionId?: string;
  outcome: BattleOutcome;
  commenceLine: string;
  bodyText: string;
  stats: BattleStats;
}

export interface BattleFilter {
  week?: number | 'all';
  locationKey?: string | 'all';
}

export interface BattleLocationOption {
  key: string;
  label: string;
  regionId?: string;
}

function stripBattlesHeader(text: string): string {
  const trimmed = text.trim();
  const header = /^Battles report:\s*/i;
  return trimmed.replace(header, '').trim();
}

function parseOutcome(body: string): BattleOutcome {
  if (/Battle won by attackers/i.test(body)) return 'attackers';
  if (/Battle won by defenders/i.test(body)) return 'defenders';
  if (/Battle ended indecisively/i.test(body)) return 'indecisive';
  return 'unknown';
}

function lastBracketId(segment: string): string | undefined {
  const matches = [...segment.matchAll(/\[([^\]]+)\]/g)];
  if (!matches.length) return undefined;
  return matches[matches.length - 1][1];
}

function combatantLabel(attackerSegment: string): string {
  const trimmed = attackerSegment.trim();
  const bracket = trimmed.indexOf('[');
  if (bracket > 0) return trimmed.slice(0, bracket).trim();
  return trimmed;
}

function ensureCombatant(map: Map<string, CombatantStats>, id: string, label: string): CombatantStats {
  let row = map.get(id);
  if (!row) {
    row = {
      id,
      label: label || id,
      hits: 0,
      misses: 0,
      damageDealt: 0,
      damageReceived: 0,
      captureDamageDealt: 0,
    };
    map.set(id, row);
  } else if (label && row.label === row.id) {
    row.label = label;
  }
  return row;
}

/** Join wrapped (chance: …) fire lines into single logical lines. */
export function normalizeFireLines(body: string): string[] {
  const raw = body.split('\n');
  const merged: string[] = [];

  for (const line of raw) {
    const trimmed = line.trim();
    if (!trimmed) continue;

    const prev = merged.length ? merged[merged.length - 1] : '';
    const prevNeedsChanceClose =
      prev.includes('(chance:') && !/\)\s+and (hits|misses)/i.test(prev);

    const isContinuation =
      /^\d+\/\d+\)\s*(and (hits|misses))?/i.test(trimmed) || /^and (hits|misses)\b/i.test(trimmed);

    if (prev && (prevNeedsChanceClose || isContinuation)) {
      merged[merged.length - 1] = `${prev} ${trimmed}`;
      continue;
    }

    merged.push(trimmed);
  }

  return merged;
}

function isShotLine(line: string): boolean {
  return /\band (hits|misses)\b/i.test(line);
}

export function computeBattleStats(body: string): BattleStats {
  const combatantMap = new Map<string, CombatantStats>();
  const captures: BattleCapture[] = [];
  const destroyed: string[] = [];
  const wrecked: string[] = [];
  const loot: string[] = [];

  let totalHits = 0;
  let totalMisses = 0;

  for (const line of normalizeFireLines(body)) {
    if (isShotLine(line)) {
      const miss = /\band misses\.?\s*$/i.test(line);
      const hit = /\band hits\b/i.test(line);

      let attackerSeg = '';
      const firesWeapon = line.match(/^(.+?)\s+fires\s+.+\s+on\s+/i);
      const flatFire = line.match(/^(.+?)\s+fire\s+on\s+/i);
      const attacks = line.match(/^(.+?)\s+attacks\s+/i);
      if (firesWeapon) attackerSeg = firesWeapon[1];
      else if (flatFire) attackerSeg = flatFire[1];
      else if (attacks) attackerSeg = attacks[1];
      else continue;

      const attackerId = lastBracketId(attackerSeg) ?? attackerSeg.trim();
      const label = combatantLabel(attackerSeg);
      const row = ensureCombatant(combatantMap, attackerId, label);

      let targetSeg = '';
      const onTarget = line.match(/\s+on\s+(.+?)\s+\(chance:/i) ||
        line.match(/\s+on\s+(.+?)\s+and (hits|misses)/i) ||
        line.match(/\s+attacks\s+(.+?)\s+\(chance:/i) ||
        line.match(/\s+attacks\s+(.+?)\s+and (hits|misses)/i);
      if (onTarget) targetSeg = onTarget[1];

      const targetId = targetSeg ? lastBracketId(targetSeg) : undefined;

      if (miss) {
        row.misses += 1;
        totalMisses += 1;
      } else if (hit) {
        row.hits += 1;
        totalHits += 1;

        const hp = line.match(/doing\s+(\d+)\s+damage/i);
        const cap = line.match(/doing\s+\d+\s+damage\s+and\s+(\d+)\s+capture damage/i) ||
          line.match(/(\d+)\s+capture damage/i);
        const hpOnly = line.match(/doing\s+(\d+)\s+damage/i);
        if (hpOnly) {
          const dmg = parseInt(hpOnly[1], 10);
          row.damageDealt += dmg;
          if (targetId) {
            const def = ensureCombatant(combatantMap, targetId, targetSeg.trim());
            def.damageReceived += dmg;
          }
        }
        if (cap && hp) {
          const capDmg = parseInt(cap[1], 10);
          row.captureDamageDealt += capDmg;
        }
      }
      continue;
    }

    const moduleCapture = line.match(/^(.+?)\s+module captured by\s+(.+?)\.\s*$/i);
    if (moduleCapture) {
      captures.push({ subject: moduleCapture[1].trim(), captor: moduleCapture[2].trim() });
      continue;
    }

    const stackCapture = line.match(/^(.+?)\s+captured by\s+(.+?)\.\s*$/i);
    if (stackCapture && !/module captured/i.test(line)) {
      captures.push({ subject: stackCapture[1].trim(), captor: stackCapture[2].trim() });
      continue;
    }

    const destroyedMatch = line.match(/^(.+?)\s+is destroyed\.\s*$/i);
    if (destroyedMatch) {
      destroyed.push(destroyedMatch[1].trim());
      continue;
    }

    const wreckedMatch = line.match(/^(.+?)\s+is wrecked\.\s*$/i);
    if (wreckedMatch) {
      wrecked.push(wreckedMatch[1].trim());
      continue;
    }

    const lootMatch = line.match(/^Fauna wreckage yielded\s+(.+?)\.\s*$/i);
    if (lootMatch) {
      loot.push(lootMatch[1].trim());
    }
  }

  const combatants = [...combatantMap.values()].sort(
    (a, b) => b.damageDealt - a.damageDealt || b.hits - a.hits,
  );

  return {
    combatants,
    totalHits,
    totalMisses,
    captures,
    destroyed,
    wrecked,
    loot,
  };
}

function extractCommenceAndLocation(block: string): { commenceLine: string; locationLabel: string; regionId?: string } {
  const commenceIdx = block.search(/Battle has commenced at/i);
  if (commenceIdx < 0) {
    return { commenceLine: '', locationLabel: 'Unknown location' };
  }

  const after = block.slice(commenceIdx);
  const lines = after.split('\n');
  let commenceParts: string[] = [];
  for (const line of lines) {
    commenceParts.push(line.trim());
    if (/\.\s*$/.test(line.trim()) && commenceParts.join(' ').includes('Battle has commenced at')) {
      break;
    }
    if (/^-{3,}/.test(line.trim())) break;
  }

  const commenceLine = commenceParts.join(' ').replace(/\s+/g, ' ').trim();
  const locMatch = commenceLine.match(/Battle has commenced at\s+(.+)\.\s*$/i);
  const locationLabel = locMatch ? locMatch[1].trim() : 'Unknown location';
  const regionMatch = locationLabel.match(/\[(R[^\]]+)\]/i);
  const regionId = regionMatch ? regionMatch[1] : undefined;

  return { commenceLine, locationLabel, regionId };
}

export function parseBattlesReport(text: string): ParsedBattle[] {
  const body = stripBattlesHeader(text);
  if (!body) return [];

  const weekRe = /^\s*Week\s+(\d+)\.\s*$/gim;
  const starts: { week: number; index: number }[] = [];
  let m: RegExpExecArray | null;
  while ((m = weekRe.exec(body)) !== null) {
    starts.push({ week: parseInt(m[1], 10), index: m.index });
  }

  if (!starts.length) return [];

  const battles: ParsedBattle[] = [];
  for (let i = 0; i < starts.length; i += 1) {
    const start = starts[i];
    const end = i + 1 < starts.length ? starts[i + 1].index : body.length;
    const block = body.slice(start.index, end).trim();
    const { commenceLine, locationLabel, regionId } = extractCommenceAndLocation(block);
    const outcome = parseOutcome(block);
    const stats = computeBattleStats(block);

    battles.push({
      id: `battle-${start.week}-${i}`,
      week: start.week,
      locationLabel,
      regionId,
      outcome,
      commenceLine,
      bodyText: block,
      stats,
    });
  }

  return battles;
}

export function battleLocationKey(battle: ParsedBattle): string {
  return battle.regionId ?? battle.locationLabel;
}

export function uniqueBattleLocations(battles: ParsedBattle[]): BattleLocationOption[] {
  const map = new Map<string, BattleLocationOption>();
  for (const b of battles) {
    const key = battleLocationKey(b);
    if (!map.has(key)) {
      const short =
        b.regionId != null
          ? b.locationLabel.split(' on ')[0]?.trim() || b.locationLabel
          : b.locationLabel;
      map.set(key, { key, label: short, regionId: b.regionId });
    }
  }
  return [...map.values()].sort((a, b) => a.label.localeCompare(b.label));
}

export function filterBattles(battles: ParsedBattle[], filter: BattleFilter): ParsedBattle[] {
  return battles.filter((b) => {
    if (filter.week != null && filter.week !== 'all' && b.week !== filter.week) return false;
    if (filter.locationKey != null && filter.locationKey !== 'all') {
      if (battleLocationKey(b) !== filter.locationKey) return false;
    }
    return true;
  });
}

export function hitRatioPercent(hits: number, misses: number): string {
  const total = hits + misses;
  if (total === 0) return '—';
  return `${Math.round((100 * hits) / total)}%`;
}

export function battleOutcomeLabel(outcome: BattleOutcome): string {
  switch (outcome) {
    case 'attackers':
      return 'attackers won';
    case 'defenders':
      return 'defenders won';
    case 'indecisive':
      return 'indecisive';
    default:
      return 'unknown';
  }
}

export function battleSelectLabel(battle: ParsedBattle): string {
  const loc =
    battle.regionId != null
      ? battle.locationLabel.split(' on ')[0]?.trim() || battle.locationLabel
      : battle.locationLabel.slice(0, 60);
  return `Week ${battle.week} · ${loc} · ${battleOutcomeLabel(battle.outcome)}`;
}

export function battleSummaryText(battle: ParsedBattle, stats: BattleStats): string {
  const top = stats.combatants.find((c) => c.damageDealt > 0 || c.hits > 0);
  const lines = [
    `Week ${battle.week}.`,
    battle.commenceLine || `Battle at ${battle.locationLabel}.`,
    `Outcome: ${battleOutcomeLabel(battle.outcome)}.`,
    `Shots: ${stats.totalHits} hits, ${stats.totalMisses} misses (${hitRatioPercent(stats.totalHits, stats.totalMisses)} hit rate).`,
  ];

  if (top) {
    lines.push(
      `Top damage: ${top.label} [${top.id}] — ${top.damageDealt} HP damage (${top.hits} hits, ${top.misses} misses).`,
    );
  } else if (stats.totalHits + stats.totalMisses > 0) {
    lines.push('Damage totals not reported (legacy hit lines without HP amounts).');
  }

  lines.push(
    `Captures: ${stats.captures.length}. Destroyed: ${stats.destroyed.length}. Wrecked: ${stats.wrecked.length}. Loot lines: ${stats.loot.length}.`,
  );

  return lines.join('\n');
}

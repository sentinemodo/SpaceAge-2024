import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { describe, expect, it } from 'vitest';
import {
  battleSelectLabel,
  computeBattleStats,
  normalizeFireLines,
  parseBattlesReport,
} from './battleReport';

const repoRoot = join(dirname(fileURLToPath(import.meta.url)), '../../../..');

function loadSample(name: string): string {
  return readFileSync(join(repoRoot, 'Tests/SampleGame', name), 'utf8');
}

describe('battleReport', () => {
  it('joins wrapped chance lines', () => {
    const body = `
  Surrender or die! [109] fire on coal-burning plant [000015] (chance:
  15/20) and hits doing 1 damage and 3 capture damage.
  space shuttle [117] fires orbital rocket launcher [orbrkt] on spaceship hull [101] (chance: 1/13)
  and hits spaceship hull [101] doing 3 damage.
`;
    const lines = normalizeFireLines(body);
    expect(lines.some((l) => /109.*15\/20.*doing 1 damage/.test(l))).toBe(true);
    expect(lines.some((l) => /117.*doing 3 damage/.test(l))).toBe(true);
  });

  it('counts hits, misses, and damage from modern report snippet', () => {
    const snippet = `
  Surrender or die! [109] fire on coal-burning plant [000015] (chance:
  15/20) and hits doing 1 damage and 3 capture damage.
  Surrender or die! [109] fire on coal-burning plant [000015] (chance:
  15/20) and hits doing 1 damage and 3 capture damage.
  Surrender or die! [109] fire on coal-burning plant [000015] (chance:
  15/20) and hits doing 1 damage.
    coal-burning plant [cplant] module captured by Gelvaren [3].
  3 gun placements [111] fire on Surrender or die! [109] and misses.
`;
    const stats = computeBattleStats(snippet);
    const tanks = stats.combatants.find((c) => c.id === '109');
    expect(tanks?.hits).toBe(3);
    expect(tanks?.damageDealt).toBe(3);
    expect(tanks?.captureDamageDealt).toBe(6);
    expect(stats.totalMisses).toBe(1);
    expect(stats.captures).toHaveLength(1);
    expect(stats.captures[0].captor).toContain('Gelvaren');
  });

  it('counts legacy hits without HP damage amounts', () => {
    const snippet = `
  3 gun placements [111] fire on Surrender or die! [109] and misses.
  Surrender or die! [109] fire on gun placement [111] and hits #2 gun placement [111].
`;
    const stats = computeBattleStats(snippet);
    const guns = stats.combatants.find((c) => c.id === '111');
    const tanks = stats.combatants.find((c) => c.id === '109');
    expect(guns?.misses).toBe(1);
    expect(tanks?.hits).toBe(1);
    expect(tanks?.damageDealt).toBe(0);
    expect(stats.totalHits).toBe(1);
  });

  it('parses multiple weekly battles from SampleGame testreport.5.3', () => {
    const full = loadSample('testreport.5.3.txt');
    const battlesIdx = full.indexOf('Battles report:');
    expect(battlesIdx).toBeGreaterThan(0);
    const section = full.slice(battlesIdx);
    const battles = parseBattlesReport(section);
    expect(battles.length).toBeGreaterThanOrEqual(13);
    expect(battles[0].week).toBe(1);
    expect(battles[0].regionId).toBe('R00002');
    expect(battles[0].locationLabel).toContain('Northern Hemisphere');
    expect(battleSelectLabel(battles[0])).toContain('Week 1');
  });

  it('parses composite fire damage from testreport.6.2', () => {
    const full = loadSample('testreport.6.2.txt');
    const battlesIdx = full.indexOf('Battles report:');
    const section = full.slice(battlesIdx);
    const battles = parseBattlesReport(section);
    const withShuttle = battles.find((b) => /117.*doing 3 damage/.test(b.bodyText) || b.stats.combatants.some((c) => c.id === '117' && c.damageDealt > 0));
    expect(withShuttle).toBeDefined();
    const shuttle = withShuttle!.stats.combatants.find((c) => c.id === '117');
    expect(shuttle?.damageDealt).toBeGreaterThan(0);
  });
});

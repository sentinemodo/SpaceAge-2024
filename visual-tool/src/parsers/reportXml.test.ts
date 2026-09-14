// @vitest-environment jsdom
import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { describe, it, expect } from 'vitest';
import {
  parseReportXml,
  filterStacksBySystem,
  estimateMoveWeeks,
  flattenStacks,
  findOrderForStack,
} from './reportXml';

const fixturePath = join(dirname(fileURLToPath(import.meta.url)), '../fixtures/faction-report.xml');
const fixtureXml = readFileSync(fixturePath, 'utf8');

describe('parseReportXml', () => {
  it('reads galaxy/system coordinates from engine XML', () => {
    const report = parseReportXml(fixtureXml);
    const sol = report.systems.find((s) => s.id === 'SS0001');
    expect(sol).toBeDefined();
    expect(sol?.name).toBe('Sol');
    expect(sol?.x).toBeGreaterThanOrEqual(0);
    expect(sol?.y).toBeGreaterThanOrEqual(0);
    expect(report.systems.some((s) => s.id === 'S00002')).toBe(true);
  });

  it('assigns stacks to regions and systems', () => {
    const report = parseReportXml(fixtureXml);
    const frigate = flattenStacks(report.stacks).find((s) => s.id === '100011');
    expect(frigate).toBeDefined();
    expect(frigate?.systemId).toBe('SS0001');
    expect(frigate?.locationId).toBe('O00003');
    const hq = flattenStacks(report.stacks).find((s) => s.id === '000112');
    expect(hq?.locationId).toBe('R00002');
  });

  it('parses regions for drill-down', () => {
    const report = parseReportXml(fixtureXml);
    expect(report.regions.some((r) => r.id === 'R00001' && r.name === 'Western Europe')).toBe(true);
    expect(report.regions.every((r) => r.systemId === 'SS0001')).toBe(true);
  });

  it('parses MOVE orders from XML', () => {
    const report = parseReportXml(fixtureXml);
    const move = findOrderForStack(report.orders, '100002');
    expect(move?.moveDestinations).toEqual(['R00001', 'R00002']);
  });
});

describe('estimateMoveWeeks', () => {
  it('returns weeks from mass line', () => {
    expect(estimateMoveWeeks('40000/4150')).toBeTypeOf('number');
  });
});

describe('filterStacksBySystem', () => {
  it('filters by system id', () => {
    const stacks = [
      { id: '1', name: 'a', systemId: 'Helios', children: [] },
      { id: '2', name: 'b', systemId: 'Fomal', children: [] },
    ];
    expect(filterStacksBySystem(stacks, 'Helios')).toHaveLength(1);
  });
});

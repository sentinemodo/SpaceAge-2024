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
  formatOrderEntry,
  resolveMoveRouteSystems,
  getSystemDetail,
  topLevelBodies,
  groupSystemOrbitBands,
  collectBodyLocationIds,
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

  it('resolves MOVE route between systems via regions', () => {
    const report = parseReportXml(fixtureXml);
    const stack = flattenStacks(report.stacks).find((s) => s.id === '100002');
    const move = findOrderForStack(report.orders, '100002');
    expect(stack).toBeDefined();
    expect(move).toBeDefined();
    const route = resolveMoveRouteSystems(stack!, move!, report.regions);
    expect(route).toBeNull();
  });

  it('spreads galaxy Y when all systems share the same coordinate', () => {
    const xml = `<?xml version="1.0"?><report turn="1"><galaxy>
      <system name="A" name-en="Alpha" X="1" Y="5"/>
      <system name="B" name-en="Beta" X="2" Y="5"/>
      <system name="C" name-en="Gamma" X="3" Y="5"/>
    </galaxy></report>`;
    const report = parseReportXml(xml);
    const ys = report.systems.map((s) => s.y);
    expect(new Set(ys).size).toBeGreaterThan(1);
  });

  it('groups Alderson gates into the rightmost orbit band', () => {
    const xml = `<?xml version="1.0"?><report turn="1"><galaxy>
      <system name="SS0001" name-en="Helios" X="5" Y="5">
        <planet name="P00001" name-en="Arbor" AU="1.0" surface-size-X="6" surface-size-Y="6"/>
        <planet name="P00002" name-en="Scoria" AU="1.5" surface-size-X="6" surface-size-Y="4"/>
        <belt name="P00003" name-en="Helios Belt" AU="2.7"/>
        <alderson name="P00009" name-en="Helios Fomal Gate" AU="80" pair="P00010"/>
        <alderson name="P00041" name-en="Helios Ember Gate" AU="80" pair="P00042"/>
      </system>
    </galaxy></report>`;
    const report = parseReportXml(xml);
    const layout = groupSystemOrbitBands(getSystemDetail(report, 'SS0001')!);
    expect(layout.bands.map((b) => b.body.name)).toEqual(['Arbor', 'Scoria', 'Helios Belt']);
    expect(layout.gates.map((g) => g.name)).toEqual(['Helios Ember Gate', 'Helios Fomal Gate']);
  });

  it('orders top-level bodies by AU for system lanes', () => {
    const report = parseReportXml(fixtureXml);
    const sol = getSystemDetail(report, 'SS0001');
    expect(sol).toBeDefined();
    const lanes = topLevelBodies(sol!);
    expect(lanes.length).toBeGreaterThan(0);
    for (let i = 1; i < lanes.length; i++) {
      expect(lanes[i].au).toBeGreaterThanOrEqual(lanes[i - 1].au);
    }
  });

  it('uses quantity attribute for module count not child modulestacks', () => {
    const xml = `<?xml version="1.0"?><report turn="1"><faction name="2"/>
      <galaxy><system name="SS0001"><planet name="P1" AU="1">
        <region name="R1" X="0" Y="0">
          <modulestack name="HQ" type="corphq" quantity="1" faction="2">
            <modulestack name="bay" type="cargob" quantity="2" faction="2"/>
          </modulestack>
        </region>
      </planet></system></galaxy></report>`;
    const report = parseReportXml(xml);
    const hq = report.stacks[0];
    expect(hq.moduleCount).toBe(1);
    expect(hq.children[0].moduleCount).toBe(2);
  });

  it('parses persons under modulestacks', () => {
    const report = parseReportXml(fixtureXml);
    const hq = flattenStacks(report.stacks).find((s) => s.id === '000112');
    expect(hq?.persons.some((p) => p.id === '000101')).toBe(true);
  });

  it('parses region resources exits and stack faction', () => {
    const report = parseReportXml(fixtureXml);
    const region = report.regions.find((r) => r.id === 'R00001');
    expect(region?.terrainType).toBe('grassl');
    expect(region?.resources?.some((r) => r.type === 'food')).toBe(true);
    expect(region?.exits?.length).toBeGreaterThan(0);
    const frigate = flattenStacks(report.stacks).find((s) => s.id === '100011');
    expect(frigate?.faction).toBe('2');
    expect(frigate?.moduleCount).toBe(1);
    expect(frigate?.persons.length).toBe(0);
    const hq = flattenStacks(report.stacks).find((s) => s.id === '000112');
    expect(hq?.moduleCount).toBe(1);
    expect(report.factionId).toBe('2');
  });

  it('collects region and orbit ids for body filtering', () => {
    const report = parseReportXml(fixtureXml);
    const sol = getSystemDetail(report, 'SS0001');
    const earth = sol?.bodies.find((b) => b.name === 'Earth');
    expect(earth).toBeDefined();
    const ids = collectBodyLocationIds(sol!, earth!.id);
    expect(ids).toContain(earth!.id);
    earth!.regions.forEach((r) => expect(ids).toContain(r.id));
  });

  it('parses system bodies and regions for system view', () => {
    const report = parseReportXml(fixtureXml);
    const sol = getSystemDetail(report, 'SS0001');
    expect(sol?.name).toBe('Sol');
    expect(sol?.bodies.some((b) => b.name === 'Earth' && b.regions.length > 0)).toBe(true);
  });

  it('parses Alderson gate pairs between systems', () => {
    const xml = `<?xml version="1.0"?><report turn="1"><galaxy>
      <system name="SS0001" name-en="Helios" X="5" Y="5">
        <alderson name="P00009" pair="P00010"/>
        <alderson name="P00041" pair="P00042"/>
      </system>
      <system name="SS0002" name-en="Fomal" X="6" Y="5">
        <alderson name="P00010" pair="P00009"/>
      </system>
      <system name="SS0003" name-en="Ember" X="4" Y="7">
        <alderson name="P00042" pair="P00041"/>
      </system>
    </galaxy></report>`;
    const report = parseReportXml(xml);
    expect(report.aldersonLinks).toHaveLength(2);
    const home = report.aldersonLinks.find(
      (l) => l.fromSystemId === 'SS0001' && l.toSystemId === 'SS0002'
    );
    expect(home?.stable).toBe(true);
    expect(home?.homePair).toBe(true);
    const outbound = report.aldersonLinks.find(
      (l) => l.fromSystemId === 'SS0001' && l.toSystemId === 'SS0003'
    );
    expect(outbound?.stable).toBe(true);
    expect(outbound?.homePair).toBe(false);
  });

  it('omits unstable Alderson pair when gate missing from faction report', () => {
    const xml = `<?xml version="1.0"?><report turn="1"><galaxy>
      <system name="SS0004" name-en="Ember" X="4" Y="7">
        <alderson name="P00057" pair="P00058" stability="unstable"/>
      </system>
      <system name="SS0005" name-en="Gleam" X="8" Y="7"/>
    </galaxy></report>`;
    const report = parseReportXml(xml);
    expect(report.aldersonLinks).toHaveLength(0);
  });

  it('includes unstable Alderson pair when both gates are in report', () => {
    const xml = `<?xml version="1.0"?><report turn="1"><galaxy>
      <system name="SS0004" name-en="Ember" X="4" Y="7">
        <alderson name="P00057" pair="P00058" stability="unstable"/>
      </system>
      <system name="SS0005" name-en="Gleam" X="8" Y="7">
        <alderson name="P00058" pair="P00057" stability="unstable"/>
      </system>
    </galaxy></report>`;
    const report = parseReportXml(xml);
    expect(report.aldersonLinks).toHaveLength(1);
    expect(report.aldersonLinks[0].stable).toBe(false);
  });

  it('formats produce orders from report XML', () => {
    const report = parseReportXml(fixtureXml);
    const order = findOrderForStack(report.orders, '000011');
    expect(order?.verbs.some((v) => v.kind === 'produce')).toBe(true);
    expect(formatOrderEntry(order!)).toMatch(/PRODUCE: energy/);
  });

  it('resolves inter-system MOVE route endpoints', () => {
    const stacks = [{ id: '1', name: 'ship', systemId: 'SS0001', children: [], upkeep: [], moduleCount: 1, persons: [] }];
    const order = { subject: 'modulestack', targetId: '1', moveDestinations: ['R00099'], verbs: [{ kind: 'move' }] };
    const regions = [{ id: 'R00099', name: 'Far', systemId: 'S00002' }];
    expect(resolveMoveRouteSystems(stacks[0], order, regions)).toEqual({
      fromSystemId: 'SS0001',
      toSystemId: 'S00002',
    });
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

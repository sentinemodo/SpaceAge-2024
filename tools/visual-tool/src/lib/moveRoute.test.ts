import { describe, expect, it } from 'vitest';
import type { ParsedReport } from '../parsers/reportXml';
import {
  arrowPath,
  buildMoveSegments,
  indexRouteLocations,
  moveDestinationsForSelection,
  planVisibleArrows,
  segmentTimeLabel,
  type MoveSegment,
} from './moveRoute';

function heliosReport(): ParsedReport {
  return {
    systemDetails: [
      {
        id: 'SS0001',
        name: 'Helios',
        bodies: [
          {
            id: 'P00001',
            name: 'Arbor',
            kind: 'planet',
            au: 1,
            regions: [
              {
                id: 'R00008',
                name: 'Northwind Grant',
                systemId: 'SS0001',
                bodyId: 'P00001',
                exits: [{ targetId: 'R00009', targetKind: 'region', duration: 3 }],
              },
              {
                id: 'R00009',
                name: 'Mid Vale',
                systemId: 'SS0001',
                bodyId: 'P00001',
                exits: [
                  { targetId: 'R00008', targetKind: 'region', duration: 3 },
                  { targetId: 'R00010', targetKind: 'region', duration: 2 },
                ],
              },
              {
                id: 'R00010',
                name: 'Greenwell Grant',
                systemId: 'SS0001',
                bodyId: 'P00001',
                exits: [{ targetId: 'R00009', targetKind: 'region', duration: 2 }],
              },
            ],
            orbitIds: ['O00001'],
            orbits: [{ id: 'O00001', bodyId: 'P00001', systemId: 'SS0001', resources: [], exits: [] }],
          },
          {
            id: 'M00001',
            name: 'Selene',
            kind: 'moon',
            au: 0.003,
            parentId: 'P00001',
            regions: [],
            orbitIds: ['O00002'],
            orbits: [{ id: 'O00002', bodyId: 'M00001', systemId: 'SS0001', resources: [], exits: [] }],
          },
        ],
      },
    ],
  } as ParsedReport;
}

describe('moveDestinationsForSelection', () => {
  const template = [
    '#faction 2 "northwnd"',
    '#modulestack 200001',
    'MOVE R00009 R00010 O00001 O00002',
    '#end',
  ].join('\n');

  it('reads a combined MOVE line for the selected stack', () => {
    expect(moveDestinationsForSelection(template, '200001', null)).toEqual([
      'R00009',
      'R00010',
      'O00001',
      'O00002',
    ]);
  });

  it('returns an empty list when the stack block has no MOVE', () => {
    const parked = '#modulestack 200001\nBUILD farms\n#end\n';
    expect(moveDestinationsForSelection(parked, '200001', null)).toEqual([]);
  });

  it('returns null when the stack is absent', () => {
    expect(moveDestinationsForSelection(template, '999999', null)).toBeNull();
  });
});

describe('buildMoveSegments', () => {
  const locations = indexRouteLocations(heliosReport());

  it('Northwind Grant to Selene orbit is two straight hops and two angled hops, all timed', () => {
    const segments = buildMoveSegments('R00008', ['R00009', 'R00010', 'O00001', 'O00002'], locations, null);

    expect(segments.map((segment) => segment.geometry)).toEqual(['straight', 'straight', 'angled', 'angled']);
    expect(segments.every((segment) => segment.solid)).toBe(true);
    expect(segments.map((segment) => segment.weeks)).toEqual([3, 2, 1, 1]);
    expect(segments.map((segment) => segment.estimate)).toEqual(['exit', 'exit', 'implicit', 'calculated']);
    expect(segmentTimeLabel(segments[0])).toBe('3 weeks');
    expect(segmentTimeLabel(segments[3])).toBe('~1 week');
  });

  it('same-body regions without an exit stay dashed', () => {
    const segments = buildMoveSegments('R00008', ['R00010'], locations, null);
    expect(segments).toEqual([
      expect.objectContaining({ geometry: 'straight', solid: false, weeks: null, estimate: 'unknown' }),
    ]);
    expect(segmentTimeLabel(segments[0])).toBe('Time unknown');
  });

  it('uses the moon offset when timing orbit to orbit', () => {
    const selene = locations.get('O00002');
    const arbor = locations.get('O00001');
    expect(arbor?.au).toBe(1);
    expect(selene?.au).toBeCloseTo(1.003);
  });
});

describe('planVisibleArrows', () => {
  const locations = indexRouteLocations(heliosReport());
  const segments = buildMoveSegments('R00008', ['R00009', 'R00010', 'O00001', 'O00002'], locations, null);

  function keys(anchor: (id: string) => string | null) {
    return planVisibleArrows(segments, anchor).map((arrow) => ({
      from: arrow.fromKey,
      to: arrow.toKey,
      geometry: arrow.segment.geometry,
    }));
  }

  it('Arbor map shows four arrows', () => {
    const region = new Set(['R00008', 'R00009', 'R00010']);
    const planned = keys((id) => {
      if (region.has(id)) return `region:${id}`;
      if (id === 'O00001') return 'orbit:O00001';
      if (id === 'O00002' || id === 'M00001') return 'body:M00001';
      return null;
    });
    expect(planned).toHaveLength(4);
    expect(planned.filter((arrow) => arrow.geometry === 'straight')).toHaveLength(2);
    expect(planned.filter((arrow) => arrow.geometry === 'angled')).toHaveLength(2);
  });

  it('Selene map shows the hop from Arbor onto the moon orbit', () => {
    const planned = keys((id) => {
      if (id === 'O00001' || id === 'P00001' || id.startsWith('R000')) return 'body:P00001';
      if (id === 'O00002') return 'orbit:O00002';
      return null;
    });
    expect(planned).toEqual([
      { from: 'body:P00001', to: 'orbit:O00002', geometry: 'angled' },
    ]);
  });

  it('Helios map shows the hop from Arbor to Selene', () => {
    const planned = keys((id) => {
      if (id === 'O00001' || id.startsWith('R000')) return 'body:P00001';
      if (id === 'O00002' || id === 'M00001') return 'body:M00001';
      return null;
    });
    expect(planned).toEqual([
      { from: 'body:P00001', to: 'body:M00001', geometry: 'angled' },
    ]);
  });
});

describe('arrowPath', () => {
  it('draws region hops as a straight center line', () => {
    const path = arrowPath({ x: 10, y: 20 }, { x: 80, y: 20 }, 'straight', []);
    expect(path?.d).toBe('M 10 20 L 80 20');
    expect(path?.labelAt).toEqual({ x: 45, y: 20 });
  });

  it('bends space hops off icons that sit on the straight line', () => {
    const path = arrowPath({ x: 0, y: 0 }, { x: 200, y: 0 }, 'angled', [{ x: 100, y: 0, r: 12 }]);
    expect(path?.d).toContain('Q');
    expect(Math.abs(path!.labelAt.y)).toBeGreaterThan(12);
  });
});

describe('segment list shape', () => {
  it('keeps the chain starting at the unit location', () => {
    const segments: MoveSegment[] = buildMoveSegments(
      'R00008',
      ['R00009'],
      indexRouteLocations(heliosReport()),
      null,
    );
    expect(segments[0].fromId).toBe('R00008');
    expect(segments[0].toId).toBe('R00009');
  });
});

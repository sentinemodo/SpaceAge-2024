import { describe, expect, it } from 'vitest';
import type { ParsedReport, StackNode } from '../parsers/reportXml';
import { movementRowsFromOrders } from './movementReport';

function stack(partial: Partial<StackNode> & Pick<StackNode, 'id' | 'name'>): StackNode {
  return {
    children: [],
    upkeep: [],
    moduleCount: 1,
    persons: [],
    ...partial,
  };
}

function report(stacks: StackNode[]): ParsedReport {
  return {
    turn: 1,
    factionId: '2',
    factionName: 'Test',
    stacks,
    systems: [],
    aldersonLinks: [],
    regions: [{ id: 'R00009', name: 'Port', systemId: 'S1', x: 0, y: 0 }],
    systemDetails: [],
    orders: [],
    technologies: [],
    battles: [],
    faction: { press: [], contracts: [] },
    bank: [],
    diplomacy: [],
    regionHints: [],
    regionReportLines: {},
  };
}

describe('movementRowsFromOrders', () => {
  const parsed = report([
    stack({ id: '200001', name: 'Hauler', locationName: 'Dock', mass: '40000/4150' }),
    stack({ id: '200002', name: 'Yard', locationName: 'Shipyard' }),
  ]);

  it('lists only module stacks whose current orders contain MOVE', () => {
    const orders = [
      '#modulestack 200001',
      'MOVE R00009',
      '',
      '#modulestack 200002',
      'BUILD farms',
      '',
      '#person 300001',
      'MOVE R00009',
      '#end',
    ].join('\n');

    const rows = movementRowsFromOrders(parsed, orders);
    expect(rows.map((row) => row.id)).toEqual(['200001']);
    expect(rows[0]).toMatchObject({
      name: 'Hauler',
      location: 'Dock',
      mass: '40000/4150',
      route: 'Port',
      hasMove: true,
    });
  });

  it('reads a MOVE entered after the report was loaded', () => {
    const orders = '#modulestack 200002\n5 MOVE R00009\n#end';
    const rows = movementRowsFromOrders(parsed, orders);
    expect(rows).toHaveLength(1);
    expect(rows[0].name).toBe('Yard');
    expect(rows[0].repeat).toBe('5');
    expect(rows[0].route).toBe('Port');
  });
});

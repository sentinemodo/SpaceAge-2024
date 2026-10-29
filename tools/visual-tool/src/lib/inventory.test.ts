import { describe, expect, it } from 'vitest';
import type { StackNode } from '../parsers/reportXml';
import { aggregateInventory, parseCapacity, parseReportedItems } from './inventory';

function stack(partial: Partial<StackNode> & Pick<StackNode, 'id' | 'name'>): StackNode {
  return { children: [], upkeep: [], moduleCount: 1, persons: [], ...partial };
}

describe('parseReportedItems', () => {
  it('parses quantity, label, type, size and mass, ignoring upkeep inside parentheses', () => {
    const items = parseReportedItems(
      'size: 4000, mass: 1890 (400), capacity: 3600/1315.\n'
      + 'items: 400 units of food [food] (size: 400, mass: 400), 20 terrans [terran] (size: 80, mass: 80, upkeep: 20 cash [cash]), unit of carbon [carbon] (size: 5, mass: 5).',
    );
    expect(items).toEqual([
      { type: 'food', label: 'food', quantity: 400, size: 400, mass: 400 },
      { type: 'terran', label: 'terrans', quantity: 20, size: 80, mass: 80 },
      { type: 'carbon', label: 'carbon', quantity: 1, size: 5, mass: 5 },
    ]);
  });

  it('returns empty when there is no items line', () => {
    expect(parseReportedItems('size: 500.')).toEqual([]);
    expect(parseReportedItems(undefined)).toEqual([]);
  });
});

describe('parseCapacity', () => {
  it('reads capacity total/used from a detail block', () => {
    expect(parseCapacity('size: 1, capacity: 3600/1315, upkeep: 1 cash.')).toEqual({ total: 3600, used: 1315 });
    expect(parseCapacity('size: 1.')).toBeNull();
  });
});

describe('aggregateInventory', () => {
  const root = stack({
    id: '1',
    name: 'HQ',
    items: [{ type: 'cash', quantity: 100 }],
    reportDetail: 'capacity: 1000/250.\nitems: 100 cash [cash] (size: 100, mass: 100).',
    children: [
      stack({
        id: '2',
        name: 'Cargo bay',
        items: [
          { type: 'food', quantity: 40 },
          { type: 'cash', quantity: 5 },
        ],
      }),
      stack({ id: '3', name: 'Empty bay' }),
    ],
  });

  it('direct scope only counts the selected stack and prefers reported size/mass', () => {
    const inv = aggregateInventory(root, 'direct');
    expect(inv.rows).toEqual([
      expect.objectContaining({ type: 'cash', quantity: 100, size: 100, mass: 100, holderCount: 1 }),
    ]);
    expect(inv.totals).toEqual({ quantity: 100, size: 100, mass: 100, types: 1, holders: 1 });
    expect(inv.capacity).toEqual({ total: 1000, used: 250 });
  });

  it('subtree scope merges the same type across descendants and falls back to XML items', () => {
    const inv = aggregateInventory(root, 'subtree');
    expect(inv.rows.map((r) => [r.type, r.quantity])).toEqual([['cash', 105], ['food', 40]]);
    // XML-only child contributes quantity but unknown size/mass.
    expect(inv.rows[0]).toMatchObject({ size: 100, mass: 100, holderCount: 2 });
    expect(inv.rows[1]).toMatchObject({ size: null, mass: null, holderCount: 1 });
    expect(inv.totals).toMatchObject({ quantity: 145, size: 100, mass: 100, types: 2, holders: 2 });
  });

  it('handles a stack with no inventory', () => {
    const inv = aggregateInventory(stack({ id: '9', name: 'Scout' }), 'subtree');
    expect(inv.rows).toEqual([]);
    expect(inv.totals).toEqual({ quantity: 0, size: 0, mass: 0, types: 0, holders: 0 });
  });
});

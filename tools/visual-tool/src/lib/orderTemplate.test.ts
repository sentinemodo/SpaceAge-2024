import { describe, it, expect } from 'vitest';
import { applyFocusedOrderEdits, filterOrdersTemplate, ownedFactionOrdersTemplate, parseOrdersTemplate } from './orderTemplate';
import type { StackNode } from '../parsers/reportXml';

const sample = `Orders Template:
#faction 8 "oreline"
#modulestack 260001
; + Oreline Headquarters [260001]

#modulestack 260003
MOVE R00001

#modulestack 999999
; NPC stack

#person 260010
; + Oreline CEO [260010]

#end`;

describe('orderTemplate', () => {
  it('parses template blocks', () => {
    const { blocks } = parseOrdersTemplate(sample);
    expect(blocks.map((b) => b.id)).toEqual(['260001', '260003', '999999', '260010']);
  });

  it('filters to owned stack ids in order format', () => {
    const stacks: StackNode[] = [
      { id: '260001', faction: '8', children: [], upkeep: [], moduleCount: 1, persons: [] },
      { id: '260003', faction: '8', children: [], upkeep: [], moduleCount: 1, persons: [] },
    ];
    const text = ownedFactionOrdersTemplate(sample, stacks, '8');
    expect(text).toContain('#faction 8 "oreline"');
    expect(text).toContain('#modulestack 260001');
    expect(text).toContain('#modulestack 260003');
    expect(text).not.toContain('#modulestack 999999');
    expect(text).toContain('#end');
  });

  it('writes a unit-view edit back into the full order file', () => {
    const full = [
      '#faction 2 "northwnd"',
      '#modulestack 200001',
      '; + Northwind Headquarters [200001]',
      'move R00009',
      '#modulestack 200002',
      'MOVE R00001',
      '#end',
    ].join('\n');
    const edited = [
      '#modulestack 200001',
      '; + Northwind Headquarters [200001]',
      'move R00010',
    ].join('\n');
    const next = applyFocusedOrderEdits(full, edited, {
      stackIds: new Set(['200001']),
      personIds: new Set<string>(),
    });
    expect(next).toContain('move R00010');
    expect(next).not.toContain('move R00009');
    expect(next).toContain('#modulestack 200002');
    expect(next).toContain('MOVE R00001');
    expect(next).toContain('#faction 2 "northwnd"');
  });

  it('focus filter preserves template lines', () => {
    const text = filterOrdersTemplate(sample, { stackIds: new Set(['260003']), includeHeader: false });
    expect(text).toContain('#modulestack 260003');
    expect(text).toContain('MOVE R00001');
  });
});

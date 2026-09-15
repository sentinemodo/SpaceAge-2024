import { describe, it, expect } from 'vitest';
import { filterOrdersTemplate, ownedFactionOrdersTemplate, parseOrdersTemplate } from './orderTemplate';
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

  it('focus filter preserves template lines', () => {
    const text = filterOrdersTemplate(sample, { stackIds: new Set(['260003']), includeHeader: false });
    expect(text).toContain('#modulestack 260003');
    expect(text).toContain('MOVE R00001');
  });
});

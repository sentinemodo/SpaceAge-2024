import { describe, it, expect } from 'vitest';
import { ordersTemplateText, ordersSummaryForFocus } from './orderDisplay';
import type { StackNode } from '../parsers/reportXml';

describe('orderDisplay', () => {
  it('extracts orders template section', () => {
    const text = ordersTemplateText([
      { id: 'orders', title: 'Orders', text: 'Orders Template:\n#modulestack ABC\nMOVE R00001' },
    ]);
    expect(text).toContain('#modulestack ABC');
  });

  it('formats focus orders from template', () => {
    const template = 'Orders Template:\n#modulestack 230003\nMOVE R00001\n#end';
    const roots: StackNode[] = [
      { id: '230003', faction: '2', children: [], upkeep: [], moduleCount: 1, persons: [] },
    ];
    const text = ordersSummaryForFocus(template, roots, null);
    expect(text).toContain('#modulestack 230003');
    expect(text).toContain('MOVE R00001');
  });
});

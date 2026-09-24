import { describe, it, expect } from 'vitest';
import { factionOrdersPane, ordersTemplateText, ordersSummaryForFocus } from './orderDisplay';
import type { StackNode } from '../parsers/reportXml';

describe('factionOrdersPane', () => {
  it('shows report orders and a parse note when nothing was submitted', () => {
    const pane = factionOrdersPane(null, '#modulestack 200001\nMOVE R00001');
    expect(pane.ordersText).toContain('#modulestack 200001');
    expect(pane.parseOutput).toBe('No orders submitted yet.');
  });

  it('shows the submitted orders and leaves parse output empty', () => {
    const pane = factionOrdersPane('#faction 2 "pw"\n#end\n', '#modulestack 200001');
    expect(pane.ordersText).toContain('#faction 2');
    expect(pane.parseOutput).toBeNull();
  });
});

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
    const text = ordersSummaryForFocus(template, roots, null, null);
    expect(text).toContain('#modulestack 230003');
    expect(text).toContain('MOVE R00001');
  });

  it('shows person orders only when person is selected', () => {
    const template = [
      'Orders Template:',
      '#modulestack 230003',
      'MOVE R00001',
      '#person 260010',
      'TRAIN skill',
      '#end',
    ].join('\n');
    const roots: StackNode[] = [
      { id: '230003', faction: '2', children: [], upkeep: [], moduleCount: 1, persons: [] },
    ];
    expect(ordersSummaryForFocus(template, roots, '230003', null)).not.toContain('#person');
    expect(ordersSummaryForFocus(template, roots, null, '260010')).toContain('#person 260010');
    expect(ordersSummaryForFocus(template, roots, null, '260010')).not.toContain('#modulestack');
  });
});

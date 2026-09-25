import { describe, expect, it } from 'vitest';

import {
  injectOrderAnchors,
  loadHumanRulesMarkdown,
  orderAnchorId,
  parseOrdersIndex,
  renderHumanRulesHtml,
} from '../src/lib/humanRules';

describe('human rules loader', () => {
  it('loads docs/human/rules.md from repo root', () => {
    const md = loadHumanRulesMarkdown();
    expect(md).toMatch(/PBEM/);
    expect(md).toMatch(/#modulestack/);
    expect(md).toMatch(/0\.8\.001/);
  });

  it('renders markdown to HTML', () => {
    const html = renderHumanRulesHtml();
    expect(html).toMatch(/<h2[^>]*>.*Order file format/i);
    expect(html).toMatch(/13 weeks/);
  });

  it('indexes all dictionary orders as immediate vs long', () => {
    const md = loadHumanRulesMarkdown();
    const index = parseOrdersIndex(md);
    expect(index.immediate).toHaveLength(26);
    expect(index.long).toHaveLength(7);
    expect(index.immediate[0]).toBe('ACTIVE');
    expect(index.long.map((v) => v.toUpperCase())).toEqual([
      'JUMP',
      'MOVE',
      'PRODUCE',
      'REPAIR',
      'RESEARCH',
      'TRAIN',
      'USE',
    ]);
  });

  it('injects scroll anchors for dictionary verbs only', () => {
    const md = loadHumanRulesMarkdown();
    const withAnchors = injectOrderAnchors(md);
    expect(withAnchors).toContain(`id="${orderAnchorId('GET')}"`);
    expect(withAnchors.indexOf(`id="${orderAnchorId('GET')}"`)).toBeGreaterThan(
      md.indexOf('## Full orders dictionary'),
    );
    const html = renderHumanRulesHtml();
    expect(html).toMatch(new RegExp(`id="${orderAnchorId('MOVE')}"`));
  });
});

import { describe, expect, it } from 'vitest';

import { loadHumanRulesMarkdown, renderHumanRulesHtml } from '../src/lib/humanRules';

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
});

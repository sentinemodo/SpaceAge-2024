import { describe, expect, it } from 'vitest';

import { loadBasicTechnologiesMarkdown, renderBasicTechnologiesHtml } from '../src/lib/basicTechnologies';

describe('basic technologies loader', () => {
  it('loads play/player/campaign/basic_technologies.md from repo root', () => {
    const md = loadBasicTechnologiesMarkdown();
    expect(md).toMatch(/Level 0 and 1 technologies/);
    expect(md).toMatch(/\[farmng\]/);
    expect(md).toMatch(/0\.8\.001/);
  });

  it('renders cross-linked catalog HTML', () => {
    const html = renderBasicTechnologiesHtml();
    expect(html).toMatch(/class="technology-catalog"/);
    expect(html).toMatch(/id="tech-farmng"/);
    expect(html).toMatch(/class="tech-crosslink" href="#tech-iron"/);
  });
});

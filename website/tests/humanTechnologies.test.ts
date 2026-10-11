import { describe, expect, it } from 'vitest';

import {
  catalogAnchor,
  collectCatalogEntries,
  loadHumanTechnologiesMarkdown,
  parseTechnologiesIndex,
  renderHumanTechnologiesHtml,
  resolveCatalogLink,
} from '../src/lib/humanTechnologies';

describe('human technologies loader', () => {
  it('loads docs/human/basic_technologies.md from repo root', () => {
    const md = loadHumanTechnologiesMarkdown();
    expect(md).toMatch(/Level 0/);
    expect(md).toMatch(/## Modules/);
    expect(md).toMatch(/\*\*copper mining \[cminng\]\*\*/);
  });

  it('renders catalog HTML with anchors and cross-links', () => {
    const html = renderHumanTechnologiesHtml();
    expect(html).toMatch(/id="tech-cminng"/);
    expect(html).toMatch(/id="item-iron"/);
    expect(html).toMatch(/class="tech-crosslink" href="#item-iron"/);
    expect(html).toMatch(/<h2 id="tech-level-0"/);
  });

  it('indexes technologies, modules, and items', () => {
    const md = loadHumanTechnologiesMarkdown();
    const groups = parseTechnologiesIndex(md);
    const byLabel = Object.fromEntries(groups.map((g) => [g.label, g.entries.length]));
    expect(byLabel['Technologies · L0']).toBe(35);
    expect(byLabel['Technologies · L1']).toBe(28);
    expect(byLabel['Module stack']).toBe(42);
    expect(byLabel.Items).toBe(19);
    expect(groups.reduce((sum, g) => sum + g.entries.length, 0)).toBe(124);
  });

  it('resolves shared ids like the visual client catalog', () => {
    const entries = collectCatalogEntries(loadHumanTechnologiesMarkdown());
    const moduleHit = resolveCatalogLink('Builds: surface drill ', 'sdrill', entries);
    expect(moduleHit?.kind).toBe('module');
    expect(catalogAnchor(moduleHit!.kind, moduleHit!.id)).toBe('module-sdrill');

    const itemHit = resolveCatalogLink('Needs: 10 iron ', 'iron', entries);
    expect(itemHit?.kind).toBe('item');
  });
});

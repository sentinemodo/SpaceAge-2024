import { readFileSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { describe, expect, it } from 'vitest';

import {
  catalogAnchor,
  collectCatalogEntries,
  isMarkdownTableSeparator,
  markdownTableCells,
  parseCatalogDocument,
  resolveCatalogLink,
  visibleCatalogBlocks,
} from './techCatalog';

const humanPath = path.resolve(
  path.dirname(fileURLToPath(import.meta.url)),
  '../../../../docs/human/basic_technologies.md'
);
const text = readFileSync(humanPath, 'utf8');

describe('human technology catalog', () => {
  const entries = collectCatalogEntries(text);

  it('lists technologies, modules, and items', () => {
    expect(entries.some((e) => e.kind === 'tech' && e.id === 'iminng')).toBe(true);
    expect(entries.some((e) => e.kind === 'module' && e.id === 'factry')).toBe(true);
    expect(entries.some((e) => e.kind === 'item' && e.id === 'iron')).toBe(true);
  });

  it('sends a shared id to the module when the name is the module', () => {
    const hit = resolveCatalogLink('Builds: surface drill ', 'sdrill', entries);
    expect(hit?.kind).toBe('module');
    expect(catalogAnchor(hit!.kind, hit!.id)).toBe('module-sdrill');
  });

  it('sends a shared id to the technology when the name is the technology', () => {
    const hit = resolveCatalogLink('Built by mineral surface drilling ', 'sdrill', entries);
    expect(hit?.kind).toBe('tech');
  });

  it('splits a markdown table row and skips the rule line', () => {
    expect(markdownTableCells('| Id | Where | Produces |')).toEqual(['Id', 'Where', 'Produces']);
    expect(isMarkdownTableSeparator('|----|--------|----------|')).toBe(true);
    expect(markdownTableCells('| water distillation | extraction | 3 oxyhydro from 1 water |')[0]).toBe(
      'water distillation'
    );
  });

  it('filters entries by description text, level, and tag', () => {
    const blocks = parseCatalogDocument(text);
    const copper = blocks.find((block) => block.type === 'entry' && block.id === 'cminng');
    expect(copper?.tags).toContain('extraction');
    expect(copper?.text.toLowerCase()).toContain('planetary surfaces');

    const byText = visibleCatalogBlocks(blocks, { query: 'planetary surfaces', level: null, tag: null });
    expect(byText.some((block) => block.id === 'cminng')).toBe(true);
    expect(byText.some((block) => block.id === 'iron')).toBe(false);

    const military = visibleCatalogBlocks(blocks, { query: '', level: 1, tag: 'military' });
    expect(military.every((block) => block.type !== 'entry' || (block.kind === 'tech' && block.level === 1))).toBe(
      true
    );
    expect(military.some((block) => block.id === 'armcbt')).toBe(true);
    expect(military.some((block) => block.id === 'factry')).toBe(false);
  });

  it('links iron from a technology line to the item', () => {
    const hit = resolveCatalogLink('Needs: 10 iron ', 'iron', entries);
    expect(hit?.kind).toBe('item');
  });
});

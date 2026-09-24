import { readFileSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { describe, expect, it } from 'vitest';

import {
  catalogAnchor,
  collectCatalogEntries,
  resolveCatalogLink,
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

  it('links iron from a technology line to the item', () => {
    const hit = resolveCatalogLink('Needs: 10 iron ', 'iron', entries);
    expect(hit?.kind).toBe('item');
  });
});

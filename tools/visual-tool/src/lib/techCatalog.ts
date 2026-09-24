import catalogText from '../../../../docs/human/basic_technologies.md?raw';

export const humanCatalogText = catalogText;

export type CatalogKind = 'tech' | 'module' | 'item';

export interface CatalogEntry {
  id: string;
  name: string;
  kind: CatalogKind;
}

const HEADING_RE = /^\*\*(.+?) \[([a-z0-9]+)\]\*\*\s*$/i;

export function catalogAnchor(kind: CatalogKind, id: string): string {
  return `${kind}-${id.toLowerCase()}`;
}

/** Collect named entries. Level sections before Modules are technologies. */
export function collectCatalogEntries(text: string): CatalogEntry[] {
  const entries: CatalogEntry[] = [];
  let kind: CatalogKind = 'tech';
  for (const rawLine of text.split('\n')) {
    const trimmed = rawLine.trim();
    const section = trimmed.match(/^##\s+(.+)$/);
    if (section) {
      const title = section[1].toLowerCase();
      if (title.startsWith('module')) kind = 'module';
      else if (title.startsWith('item')) kind = 'item';
      else if (title.startsWith('level')) kind = 'tech';
      continue;
    }
    const heading = trimmed.match(HEADING_RE);
    if (heading) {
      entries.push({
        name: heading[1].trim().toLowerCase(),
        id: heading[2].toLowerCase(),
        kind,
      });
    }
  }
  return entries;
}

export const humanCatalogEntries = collectCatalogEntries(humanCatalogText);

/**
 * Pick which entry a `[id]` refers to.
 * Shared ids (a technology and the module it builds) follow the name written just before the brackets.
 */
export function resolveCatalogLink(
  before: string,
  id: string,
  entries: CatalogEntry[]
): CatalogEntry | null {
  const matches = entries.filter((e) => e.id === id.toLowerCase());
  if (matches.length === 0) return null;
  if (matches.length === 1) return matches[0];

  const window = before.slice(-120).toLowerCase();
  const hits = matches
    .map((entry) => ({ entry, at: window.lastIndexOf(entry.name) }))
    .filter((hit) => hit.at >= 0);
  const outermost = hits.filter(
    (hit) =>
      !hits.some(
        (other) =>
          other.entry.name.length > hit.entry.name.length &&
          hit.at >= other.at &&
          hit.at + hit.entry.name.length <= other.at + other.entry.name.length
      )
  );
  outermost.sort((a, b) => b.at - a.at || b.entry.name.length - a.entry.name.length);
  return outermost[0]?.entry ?? matches[0];
}

export function catalogEntry(
  entries: CatalogEntry[],
  id: string,
  prefer?: CatalogKind
): CatalogEntry | null {
  const matches = entries.filter((e) => e.id === id.toLowerCase());
  if (matches.length === 0) return null;
  if (prefer) {
    const hit = matches.find((e) => e.kind === prefer);
    if (hit) return hit;
  }
  return matches[0];
}

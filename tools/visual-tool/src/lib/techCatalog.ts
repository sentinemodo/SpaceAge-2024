import catalogText from '../../../../docs/human/basic_technologies.md?raw';

export const humanCatalogText = catalogText;

export type CatalogKind = 'tech' | 'module' | 'item';

export interface CatalogEntry {
  id: string;
  name: string;
  kind: CatalogKind;
}

const HEADING_RE = /^\*\*(.+?) \[([a-z0-9]+)\]\*\*\s*$/i;

export function isMarkdownTableLine(line: string): boolean {
  return line.trim().startsWith('|');
}

export function isMarkdownTableSeparator(line: string): boolean {
  return /^\|?\s*:?-{3,}/.test(line.trim()) && /^[\s|:-]+$/.test(line.trim());
}

export function markdownTableCells(line: string): string[] {
  return line
    .trim()
    .replace(/^\|/, '')
    .replace(/\|$/, '')
    .split('|')
    .map((cell) => cell.trim());
}

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

const ENTRY_HEADING_RE = /^\*\*(.+?) \[([a-z0-9]+)\]\*\*\s*$/i;
const WHERE_GROUPS = new Set([
  'production',
  'extraction',
  'agricultural',
  'command',
  'spacecraft',
  'settlement',
  'habitat',
  'military',
  'research',
  'repair',
  'propulsion',
]);

export interface CatalogDocumentBlock {
  type: 'title' | 'section' | 'subsection' | 'entry' | 'prose' | 'table';
  level: number | null;
  kind: CatalogKind | null;
  id?: string;
  name?: string;
  tags: string[];
  /** Name plus description text, used by the search box. */
  text: string;
  lines: string[];
  table?: string[][];
}

export interface CatalogFilter {
  query: string;
  level: number | null;
  tag: string | null;
}

export function tagsInEntry(lines: string[]): string[] {
  const tags = new Set<string>();
  for (const line of lines) {
    const tagged = line.match(/Tags?:\s*([^.]+)/i);
    if (tagged) {
      for (const match of tagged[1].matchAll(/`([a-z0-9-]+)`/gi)) {
        tags.add(match[1].toLowerCase());
      }
    }
    const where = line.match(/^Where:\s*(.+)/i);
    if (where) {
      for (const chunk of where[1].split(/[,.]/)) {
        const word = chunk.trim().toLowerCase().match(/^[a-z]+/)?.[0];
        if (word && WHERE_GROUPS.has(word)) tags.add(word);
      }
    }
  }
  return [...tags].sort();
}

export function parseCatalogDocument(text: string): CatalogDocumentBlock[] {
  const blocks: CatalogDocumentBlock[] = [];
  let kind: CatalogKind | null = null;
  let level: number | null = null;
  let entry: CatalogDocumentBlock | null = null;

  const flushEntry = () => {
    if (!entry) return;
    entry.tags = entry.kind === 'tech' ? tagsInEntry(entry.lines) : [];
    entry.text = [entry.name, ...entry.lines].join('\n');
    blocks.push(entry);
    entry = null;
  };

  for (const rawLine of text.split('\n')) {
    const trimmed = rawLine.trim();
    if (!trimmed || trimmed === '---') continue;
    if (isMarkdownTableLine(trimmed)) {
      flushEntry();
      const last = blocks[blocks.length - 1];
      if (last?.type === 'table' && last.table) {
        if (!isMarkdownTableSeparator(trimmed)) last.table.push(markdownTableCells(trimmed));
      } else if (!isMarkdownTableSeparator(trimmed)) {
        blocks.push({
          type: 'table',
          level,
          kind,
          tags: [],
          text: '',
          lines: [],
          table: [markdownTableCells(trimmed)],
        });
      }
      continue;
    }
    const levelMatch = trimmed.match(/^#{2,3}\s+Level\s+(\d+)/i);
    const section = trimmed.match(/^##\s+(.+)$/);
    const sub = trimmed.match(/^###\s+(.+)$/);
    const heading = trimmed.match(ENTRY_HEADING_RE);
    if (section || sub || heading || trimmed.startsWith('# ')) flushEntry();

    if (trimmed.startsWith('# ')) {
      blocks.push({ type: 'title', level: null, kind: null, tags: [], text: '', lines: [trimmed.slice(2)] });
      continue;
    }
    if (section) {
      const title = section[1].toLowerCase();
      if (title.startsWith('module')) kind = 'module';
      else if (title.startsWith('item')) kind = 'item';
      else if (title.startsWith('level')) kind = 'tech';
      if (levelMatch) level = Number(levelMatch[1]);
      blocks.push({ type: 'section', level, kind, tags: [], text: '', lines: [section[1]] });
      continue;
    }
    if (sub) {
      if (levelMatch) level = Number(levelMatch[1]);
      blocks.push({ type: 'subsection', level, kind, tags: [], text: '', lines: [sub[1]] });
      continue;
    }
    if (heading) {
      entry = {
        type: 'entry',
        level,
        kind,
        id: heading[2].toLowerCase(),
        name: heading[1].trim(),
        tags: [],
        text: '',
        lines: [],
      };
      continue;
    }
    if (entry) entry.lines.push(trimmed);
    else blocks.push({ type: 'prose', level, kind, tags: [], text: trimmed, lines: [trimmed] });
  }
  flushEntry();
  return blocks;
}

export function visibleCatalogBlocks(
  blocks: CatalogDocumentBlock[],
  filter: CatalogFilter
): CatalogDocumentBlock[] {
  const query = filter.query.trim().toLowerCase();
  const active = query.length > 0 || filter.level != null || filter.tag != null;
  const shown: CatalogDocumentBlock[] = [];
  let pending: CatalogDocumentBlock[] = [];

  for (const block of blocks) {
    if (block.type === 'title') {
      shown.push(block);
      continue;
    }
    if (block.type === 'section') {
      pending = [block];
      continue;
    }
    if (block.type === 'subsection') {
      const section = pending.find((item) => item.type === 'section');
      pending = section ? [section, block] : [block];
      continue;
    }
    if (block.type !== 'entry') {
      if (!active) shown.push(block);
      continue;
    }
    if (query && !block.text.toLowerCase().includes(query)) continue;
    if (filter.level != null && block.level !== filter.level) continue;
    if (filter.tag && (block.kind !== 'tech' || !block.tags.includes(filter.tag))) continue;
    shown.push(...pending, block);
    pending = [];
  }
  return shown;
}

export const humanCatalogBlocks = parseCatalogDocument(humanCatalogText);

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

/** Repo root (parent of `website/`). */
const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../..');

const technologiesPath = path.join(repoRoot, 'docs/human/basic_technologies.md');

export type CatalogKind = 'tech' | 'module' | 'item';

export interface CatalogEntry {
  id: string;
  name: string;
  kind: CatalogKind;
}

const HEADING_RE = /^\*\*(.+?) \[([a-z0-9]+)\]\*\*\s*$/i;
const ENTRY_HEADING_RE = /^\*\*(.+?) \[([a-z0-9]+)\]\*\*\s*$/i;

export function catalogAnchor(kind: CatalogKind, id: string): string {
  return `${kind}-${id.toLowerCase()}`;
}

export function loadHumanTechnologiesMarkdown(): string {
  return fs.readFileSync(technologiesPath, 'utf-8');
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
        name: heading[1].trim(),
        id: heading[2].toLowerCase(),
        kind,
      });
    }
  }
  return entries;
}

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
    .map((entry) => ({ entry, at: window.lastIndexOf(entry.name.toLowerCase()) }))
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

function isMarkdownTableLine(line: string): boolean {
  return line.trim().startsWith('|');
}

function isMarkdownTableSeparator(line: string): boolean {
  return /^\|?\s*:?-{3,}/.test(line.trim()) && /^[\s|:-]+$/.test(line.trim());
}

function markdownTableCells(line: string): string[] {
  return line
    .trim()
    .replace(/^\|/, '')
    .replace(/\|$/, '')
    .split('|')
    .map((cell) => cell.trim());
}

export interface CatalogDocumentBlock {
  type: 'title' | 'section' | 'subsection' | 'entry' | 'prose' | 'table';
  level: number | null;
  kind: CatalogKind | null;
  id?: string;
  name?: string;
  lines: string[];
  table?: string[][];
}

export function parseCatalogDocument(text: string): CatalogDocumentBlock[] {
  const blocks: CatalogDocumentBlock[] = [];
  let kind: CatalogKind | null = null;
  let level: number | null = null;
  let entry: CatalogDocumentBlock | null = null;

  const flushEntry = () => {
    if (!entry) return;
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
      blocks.push({ type: 'title', level: null, kind: null, lines: [trimmed.slice(2)] });
      continue;
    }
    if (section) {
      const title = section[1].toLowerCase();
      if (title.startsWith('module')) kind = 'module';
      else if (title.startsWith('item')) kind = 'item';
      else if (title.startsWith('level')) kind = 'tech';
      if (levelMatch) level = Number(levelMatch[1]);
      blocks.push({ type: 'section', level, kind, lines: [section[1]] });
      continue;
    }
    if (sub) {
      if (levelMatch) level = Number(levelMatch[1]);
      blocks.push({ type: 'subsection', level, kind, lines: [sub[1]] });
      continue;
    }
    if (heading) {
      entry = {
        type: 'entry',
        level,
        kind,
        id: heading[2].toLowerCase(),
        name: heading[1].trim(),
        lines: [],
      };
      continue;
    }
    if (entry) entry.lines.push(trimmed);
    else blocks.push({ type: 'prose', level, kind, lines: [trimmed] });
  }
  flushEntry();
  return blocks;
}

export type TechnologiesIndexGroup = {
  label: string;
  entries: Array<{ name: string; id: string; kind: CatalogKind; anchor: string }>;
};

export function parseTechnologiesIndex(text: string): TechnologiesIndexGroup[] {
  const blocks = parseCatalogDocument(text);
  const level0: TechnologiesIndexGroup['entries'] = [];
  const level1: TechnologiesIndexGroup['entries'] = [];
  const modules: TechnologiesIndexGroup['entries'] = [];
  const items: TechnologiesIndexGroup['entries'] = [];

  for (const block of blocks) {
    if (block.type !== 'entry' || !block.kind || !block.id || !block.name) continue;
    const row = {
      name: block.name,
      id: block.id,
      kind: block.kind,
      anchor: catalogAnchor(block.kind, block.id),
    };
    if (block.kind === 'tech' && block.level === 0) level0.push(row);
    else if (block.kind === 'tech' && block.level === 1) level1.push(row);
    else if (block.kind === 'module') modules.push(row);
    else if (block.kind === 'item') items.push(row);
  }

  const sortByName = (a: { name: string }, b: { name: string }) =>
    a.name.localeCompare(b.name, undefined, { sensitivity: 'base' });

  level0.sort(sortByName);
  level1.sort(sortByName);
  modules.sort(sortByName);
  items.sort(sortByName);

  return [
    { label: 'Technologies · L0', entries: level0 },
    { label: 'Technologies · L1', entries: level1 },
    { label: 'Module stack', entries: modules },
    { label: 'Items', entries: items },
  ].filter((group) => group.entries.length > 0);
}

function escapeHtml(text: string): string {
  return text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

function renderInlineHtml(text: string, entries: CatalogEntry[]): string {
  const re = /\[([a-z0-9]+)\]|\*\*(.+?)\*\*/gi;
  let last = 0;
  let html = '';
  let m: RegExpExecArray | null;
  while ((m = re.exec(text)) !== null) {
    if (m.index > last) html += escapeHtml(text.slice(last, m.index));
    if (m[2] != null) {
      html += `<strong>${escapeHtml(m[2])}</strong>`;
    } else {
      const id = m[1].toLowerCase();
      const hit = resolveCatalogLink(text.slice(0, m.index), id, entries);
      if (hit) {
        const anchor = catalogAnchor(hit.kind, hit.id);
        html += `<a class="tech-crosslink" href="#${anchor}">[${escapeHtml(m[1])}]</a>`;
      } else {
        html += escapeHtml(`[${m[1]}]`);
      }
    }
    last = m.index + m[0].length;
  }
  if (last < text.length) html += escapeHtml(text.slice(last));
  return html;
}

function sectionSlug(title: string): string {
  return `tech-${title.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '')}`;
}

function renderCatalogBlocksToHtml(blocks: CatalogDocumentBlock[], entries: CatalogEntry[]): string {
  const parts: string[] = [];
  for (const block of blocks) {
    if (block.type === 'title') {
      parts.push(`<h1 class="tech-page-title">${escapeHtml(block.lines[0] ?? '')}</h1>`);
      continue;
    }
    if (block.type === 'section') {
      const title = block.lines[0] ?? '';
      parts.push(`<h2 id="${sectionSlug(title)}" class="tech-section">${escapeHtml(title)}</h2>`);
      continue;
    }
    if (block.type === 'subsection') {
      parts.push(`<h3 class="tech-subsection">${escapeHtml(block.lines[0] ?? '')}</h3>`);
      continue;
    }
    if (block.type === 'table' && block.table?.length) {
      const [head, ...body] = block.table;
      parts.push('<div class="tech-table-wrap"><table class="tech-table"><thead><tr>');
      for (const cell of head) {
        parts.push(`<th>${renderInlineHtml(cell, entries)}</th>`);
      }
      parts.push('</tr></thead><tbody>');
      for (const row of body) {
        parts.push('<tr>');
        for (const cell of row) {
          parts.push(`<td>${renderInlineHtml(cell, entries)}</td>`);
        }
        parts.push('</tr>');
      }
      parts.push('</tbody></table></div>');
      continue;
    }
    if (block.type === 'entry' && block.kind && block.id && block.name) {
      const anchor = catalogAnchor(block.kind, block.id);
      parts.push(`<article class="tech-entry">`);
      parts.push(`<span id="${anchor}" class="tech-anchor"></span>`);
      parts.push(
        `<h3 class="tech-entry-title">${escapeHtml(block.name)} <span class="tech-id">[${escapeHtml(block.id)}]</span></h3>`
      );
      for (const line of block.lines) {
        parts.push(`<p class="tech-paragraph">${renderInlineHtml(line, entries)}</p>`);
      }
      parts.push('</article>');
      continue;
    }
    if (block.type === 'prose' && block.lines[0]) {
      parts.push(`<p class="tech-paragraph">${renderInlineHtml(block.lines[0], entries)}</p>`);
    }
  }
  return parts.join('\n');
}

export function renderHumanTechnologiesHtml(): string {
  const markdown = loadHumanTechnologiesMarkdown();
  const entries = collectCatalogEntries(markdown);
  const blocks = parseCatalogDocument(markdown);
  return renderCatalogBlocksToHtml(blocks, entries);
}

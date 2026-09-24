import { useEffect, useMemo, type ReactNode } from 'react';

import {
  catalogAnchor,
  humanCatalogEntries,
  humanCatalogText,
  resolveCatalogLink,
  type CatalogEntry,
  type CatalogKind,
} from '../lib/techCatalog';

const HEADING_RE = /^\*\*(.+?) \[([a-z0-9]+)\]\*\*\s*$/i;
const SECTION_RE = /^##\s+(.+)$/;
const SUB_RE = /^###\s+(.+)$/;

function sectionKind(title: string, current: CatalogKind): CatalogKind {
  const lower = title.toLowerCase();
  if (lower.startsWith('module')) return 'module';
  if (lower.startsWith('item')) return 'item';
  if (lower.startsWith('level')) return 'tech';
  return current;
}

function renderInline(text: string, entries: CatalogEntry[]): ReactNode[] {
  const parts: ReactNode[] = [];
  const re = /\[([a-z0-9]+)\]|\*\*(.+?)\*\*/gi;
  let last = 0;
  let m: RegExpExecArray | null;
  while ((m = re.exec(text)) !== null) {
    if (m.index > last) parts.push(text.slice(last, m.index));
    if (m[2] != null) {
      parts.push(<strong key={`b-${m.index}`}>{m[2]}</strong>);
    } else {
      const id = m[1].toLowerCase();
      const hit = resolveCatalogLink(text.slice(0, m.index), id, entries);
      if (hit) {
        const anchor = catalogAnchor(hit.kind, hit.id);
        parts.push(
          <a
            key={`${m.index}-${anchor}`}
            className="tech-crosslink"
            href={`#${anchor}`}
            onClick={(event) => {
              event.preventDefault();
              document.getElementById(anchor)?.scrollIntoView({ block: 'start' });
            }}
          >
            [{m[1]}]
          </a>
        );
      } else {
        parts.push(`[${m[1]}]`);
      }
    }
    last = m.index + m[0].length;
  }
  if (last < text.length) parts.push(text.slice(last));
  return parts;
}

export function TechnologyCatalog({
  focusAnchor,
  focusNonce,
}: {
  focusAnchor?: string | null;
  focusNonce?: number;
}) {
  const entries = humanCatalogEntries;

  const body = useMemo(() => {
    const nodes: ReactNode[] = [];
    let kind: CatalogKind = 'tech';
    let row = 0;
    for (const rawLine of humanCatalogText.split('\n')) {
      const line = rawLine.trimEnd();
      const trimmed = line.trim();
      row += 1;
      if (!trimmed || trimmed === '---') continue;
      if (trimmed.startsWith('|')) {
        nodes.push(
          <p key={`row-${row}`} className="tech-paragraph tech-table-row">
            {trimmed}
          </p>
        );
        continue;
      }
      const section = trimmed.match(SECTION_RE);
      if (section) {
        kind = sectionKind(section[1], kind);
        const slug = section[1].toLowerCase().replace(/[^a-z0-9]+/g, '-');
        nodes.push(
          <h2 key={`h2-${row}`} id={`tech-${slug}`} className="tech-section">
            {section[1]}
          </h2>
        );
        continue;
      }
      const sub = trimmed.match(SUB_RE);
      if (sub) {
        nodes.push(
          <h3 key={`h3s-${row}`} className="tech-subsection">
            {sub[1]}
          </h3>
        );
        continue;
      }
      const heading = trimmed.match(HEADING_RE);
      if (heading) {
        const id = heading[2].toLowerCase();
        nodes.push(
          <h3 key={`h3-${row}`} id={catalogAnchor(kind, id)} className="tech-entry-title">
            {heading[1]} <span className="tech-id">[{heading[2]}]</span>
          </h3>
        );
        continue;
      }
      if (trimmed.startsWith('# ')) {
        nodes.push(
          <h1 key={`h1-${row}`} className="tech-title">
            {trimmed.slice(2)}
          </h1>
        );
        continue;
      }
      nodes.push(
        <p key={`p-${row}`} className="tech-paragraph">
          {renderInline(line, entries)}
        </p>
      );
    }
    return nodes;
  }, [entries]);

  useEffect(() => {
    if (!focusAnchor) return;
    document.getElementById(focusAnchor)?.scrollIntoView({ block: 'start' });
  }, [focusAnchor, focusNonce]);

  return (
    <div className="technology-catalog">
      {body}
    </div>
  );
}

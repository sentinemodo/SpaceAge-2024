import { useMemo, type ReactNode } from 'react';

import catalogText from '../../../play/player/campaign/basic_technologies.md?raw';

const HEADING_RE = /^\*\*(.+?) \[([a-z0-9]+)\]\*\*\s*$/i;
const SECTION_RE = /^##\s+(.+)$/;

function collectAnchors(text: string): Set<string> {
  const anchors = new Set<string>();
  for (const line of text.split('\n')) {
    const heading = line.trim().match(HEADING_RE);
    if (heading) anchors.add(heading[2].toLowerCase());
  }
  return anchors;
}

function renderInline(text: string, anchors: Set<string>): ReactNode[] {
  const parts: ReactNode[] = [];
  let last = 0;
  const re = /\[([a-z0-9]+)\]/gi;
  let m: RegExpExecArray | null;
  while ((m = re.exec(text)) !== null) {
    if (m.index > last) parts.push(text.slice(last, m.index));
    const id = m[1].toLowerCase();
    if (anchors.has(id)) {
      parts.push(
        <a key={`${m.index}-${id}`} className="tech-crosslink" href={`#tech-${id}`}>
          [{m[1]}]
        </a>
      );
    } else {
      parts.push(`[${m[1]}]`);
    }
    last = m.index + m[0].length;
  }
  if (last < text.length) parts.push(text.slice(last));
  return parts;
}

export function TechnologyCatalog() {
  const anchors = useMemo(() => collectAnchors(catalogText), []);

  const body = useMemo(() => {
    const nodes: ReactNode[] = [];
    for (const rawLine of catalogText.split('\n')) {
      const line = rawLine.trimEnd();
      const trimmed = line.trim();
      if (!trimmed) {
        nodes.push(<br key={`br-${nodes.length}`} />);
        continue;
      }
      const section = trimmed.match(SECTION_RE);
      if (section) {
        const slug = section[1].toLowerCase().replace(/[^a-z0-9]+/g, '-');
        nodes.push(
          <h2 key={`h2-${nodes.length}`} id={`tech-${slug}`} className="tech-section">
            {section[1]}
          </h2>
        );
        continue;
      }
      const heading = trimmed.match(HEADING_RE);
      if (heading) {
        const id = heading[2].toLowerCase();
        nodes.push(
          <h3 key={`h3-${nodes.length}`} id={`tech-${id}`} className="tech-entry-title">
            {heading[1]} [{heading[2]}]
          </h3>
        );
        continue;
      }
      if (trimmed.startsWith('# ')) {
        nodes.push(<h1 key={`h1-${nodes.length}`} className="tech-title">{trimmed.slice(2)}</h1>);
        continue;
      }
      nodes.push(
        <p key={`p-${nodes.length}`} className="tech-paragraph">
          {renderInline(line, anchors)}
        </p>
      );
    }
    return nodes;
  }, [anchors]);

  return <div className="technology-catalog">{body}</div>;
}

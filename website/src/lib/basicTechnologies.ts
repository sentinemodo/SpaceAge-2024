import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

/** Repo root (parent of `website/`). */
const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../..');

const catalogPath = path.join(repoRoot, 'play/player/campaign/basic_technologies.md');

const HEADING_RE = /^\*\*(.+?) \[([a-z0-9]+)\]\*\*\s*$/i;
const SECTION_RE = /^##\s+(.+)$/;

function escapeHtml(text: string): string {
  return text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

function collectAnchors(text: string): Set<string> {
  const anchors = new Set<string>();
  for (const line of text.split('\n')) {
    const heading = line.trim().match(HEADING_RE);
    if (heading) anchors.add(heading[2].toLowerCase());
  }
  return anchors;
}

function renderInline(text: string, anchors: Set<string>): string {
  const parts: string[] = [];
  let last = 0;
  const re = /\[([a-z0-9]+)\]/gi;
  let match: RegExpExecArray | null;
  while ((match = re.exec(text)) !== null) {
    if (match.index > last) parts.push(escapeHtml(text.slice(last, match.index)));
    const id = match[1].toLowerCase();
    if (anchors.has(id)) {
      parts.push(
        `<a class="tech-crosslink" href="#tech-${escapeHtml(id)}">[${escapeHtml(match[1])}]</a>`,
      );
    } else {
      parts.push(escapeHtml(match[0]));
    }
    last = match.index + match[0].length;
  }
  if (last < text.length) parts.push(escapeHtml(text.slice(last)));
  return parts.join('');
}

export function loadBasicTechnologiesMarkdown(): string {
  return fs.readFileSync(catalogPath, 'utf-8');
}

/** Same cross-linked rendering as the visual-tool TechnologyCatalog. */
export function renderBasicTechnologiesHtml(): string {
  const catalogText = loadBasicTechnologiesMarkdown();
  const anchors = collectAnchors(catalogText);
  const nodes: string[] = [];

  for (const rawLine of catalogText.split('\n')) {
    const line = rawLine.trimEnd();
    const trimmed = line.trim();
    if (!trimmed) {
      nodes.push('<br />');
      continue;
    }

    const section = trimmed.match(SECTION_RE);
    if (section) {
      const slug = section[1].toLowerCase().replace(/[^a-z0-9]+/g, '-');
      nodes.push(`<h2 id="tech-${escapeHtml(slug)}" class="tech-section">${escapeHtml(section[1])}</h2>`);
      continue;
    }

    const heading = trimmed.match(HEADING_RE);
    if (heading) {
      const id = heading[2].toLowerCase();
      nodes.push(
        `<h3 id="tech-${escapeHtml(id)}" class="tech-entry-title">${escapeHtml(heading[1])} [${escapeHtml(heading[2])}]</h3>`,
      );
      continue;
    }

    if (trimmed.startsWith('# ')) {
      nodes.push(`<h1 class="tech-title">${escapeHtml(trimmed.slice(2))}</h1>`);
      continue;
    }

    nodes.push(`<p class="tech-paragraph">${renderInline(line, anchors)}</p>`);
  }

  return `<div class="technology-catalog">${nodes.join('\n')}</div>`;
}

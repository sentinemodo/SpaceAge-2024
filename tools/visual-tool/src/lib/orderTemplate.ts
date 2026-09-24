import type { StackNode } from '../parsers/reportXml';
import { flattenStacks } from '../parsers/reportXml';

export interface OrderTemplateBlock {
  kind: 'modulestack' | 'person' | 'other';
  id: string;
  lines: string[];
  /** Inclusive start and exclusive end in the source text. */
  start?: number;
  end?: number;
}

function stripOrdersHeader(text: string): string {
  return text.replace(/^Orders Template:\s*/i, '').trim();
}

/** Split an orders template into header, per-target blocks, and footer. */
export function parseOrdersTemplate(text: string): {
  header: string[];
  blocks: OrderTemplateBlock[];
  footer: string[];
} {
  const withoutLabel = text.replace(/^Orders Template:\s*/i, '');
  const leadingTrim = withoutLabel.length - withoutLabel.trimStart().length;
  const source = withoutLabel.trim();
  const lines = source.split('\n');
  const header: string[] = [];
  const footer: string[] = [];
  const blocks: OrderTemplateBlock[] = [];
  let current: OrderTemplateBlock | null = null;
  let offset = text.length - withoutLabel.length + leadingTrim;

  for (const line of lines) {
    const lineStart = offset;
    const lineEnd = offset + line.length;
    offset = lineEnd + 1;
    const trimmed = line.trim();
    if (/^#end\b/i.test(trimmed)) {
      if (current) {
        blocks.push(current);
        current = null;
      }
      footer.push(line);
      continue;
    }
    const stackMatch = trimmed.match(/^#modulestack\s+(\S+)/i);
    const personMatch = trimmed.match(/^#person\s+(\S+)/i);
    if (stackMatch || personMatch) {
      if (current) blocks.push(current);
      current = {
        kind: stackMatch ? 'modulestack' : 'person',
        id: (stackMatch || personMatch)![1],
        lines: [line],
        start: lineStart,
        end: lineEnd,
      };
      continue;
    }
    if (current) {
      current.lines.push(line);
      current.end = lineEnd;
      continue;
    }
    if (/^#faction\b/i.test(trimmed) || trimmed) {
      header.push(line);
    }
  }
  if (current) blocks.push(current);
  return { header, blocks, footer };
}

export function ownedStackIds(stacks: StackNode[], factionId: string): Set<string> {
  return new Set(
    flattenStacks(stacks)
      .filter((s) => s.faction === factionId)
      .map((s) => s.id)
  );
}

export function ownedPersonIds(stacks: StackNode[], factionId: string): Set<string> {
  const ids = new Set<string>();
  for (const s of flattenStacks(stacks)) {
    if (s.faction !== factionId) continue;
    for (const p of s.persons) ids.add(p.id);
  }
  return ids;
}

export function filterOrdersTemplate(
  template: string,
  opts: { stackIds?: Set<string>; personIds?: Set<string>; includeHeader?: boolean }
): string {
  const trimmed = template.trim();
  if (!trimmed) return '';
  const { header, blocks, footer } = parseOrdersTemplate(trimmed);
  const filtered = blocks.filter((block) => {
    if (block.kind === 'modulestack') {
      return opts.stackIds ? opts.stackIds.has(block.id) : false;
    }
    if (block.kind === 'person') {
      return opts.personIds ? opts.personIds.has(block.id) : false;
    }
    return false;
  });
  if (filtered.length === 0 && !opts.includeHeader) {
    return opts.stackIds?.size
      ? [...opts.stackIds].map((id) => `#modulestack ${id}\n; (no order in report)`).join('\n\n')
      : '';
  }
  const parts: string[] = [];
  if (opts.includeHeader !== false && header.length) parts.push(header.join('\n'));
  for (const block of filtered) parts.push(block.lines.join('\n'));
  if (footer.length) parts.push(footer.join('\n'));
  else if (filtered.length) parts.push('#end');
  return parts.join('\n\n').trim();
}

export function ownedFactionOrdersTemplate(
  template: string,
  stacks: StackNode[],
  factionId: string
): string {
  return filterOrdersTemplate(template, {
    stackIds: ownedStackIds(stacks, factionId),
    personIds: ownedPersonIds(stacks, factionId),
    includeHeader: true,
  });
}

function blockKey(block: OrderTemplateBlock): string {
  return `${block.kind}:${block.id}`;
}

function blockInFocus(
  block: OrderTemplateBlock,
  focus: { stackIds: Set<string>; personIds: Set<string> },
): boolean {
  if (block.kind === 'modulestack') return focus.stackIds.has(block.id);
  if (block.kind === 'person') return focus.personIds.has(block.id);
  return false;
}

/** Replace the focused unit or person blocks in the full order file with the unit-view text. */
export function applyFocusedOrderEdits(
  fullText: string,
  editedFocusText: string,
  focus: { stackIds: Set<string>; personIds: Set<string> },
): string {
  const full = parseOrdersTemplate(fullText);
  const edited = parseOrdersTemplate(editedFocusText);
  const incoming = new Map(edited.blocks.map((block) => [blockKey(block), block]));
  const replaced = new Set<string>();
  const edits: { start: number; end: number; text: string }[] = [];

  for (const block of full.blocks) {
    if (!blockInFocus(block, focus) || block.start == null || block.end == null) continue;
    const key = blockKey(block);
    const next = incoming.get(key);
    replaced.add(key);
    edits.push({
      start: block.start,
      end: block.end,
      text: next ? next.lines.join('\n') : '',
    });
  }

  edits.sort((a, b) => b.start - a.start);
  let result = fullText;
  for (const edit of edits) {
    let start = edit.start;
    let end = edit.end;
    let text = edit.text;
    if (!text) {
      const after = result.slice(end, end + 2);
      if (after.startsWith('\n\n')) end += 2;
      else if (after.startsWith('\n')) end += 1;
      else if (start > 0 && result[start - 1] === '\n') start -= 1;
    }
    result = result.slice(0, start) + text + result.slice(end);
  }

  const fresh = edited.blocks.filter((block) => !replaced.has(blockKey(block)));
  if (!fresh.length) return result;
  const insertion = fresh.map((block) => block.lines.join('\n')).join('\n\n');
  const endAt = result.search(/^#end\b/im);
  if (endAt >= 0) {
    const before = result.slice(0, endAt).replace(/\s*$/, '');
    return `${before}\n\n${insertion}\n\n${result.slice(endAt)}`;
  }
  const trimmed = result.replace(/\s*$/, '');
  return trimmed ? `${trimmed}\n\n${insertion}` : insertion;
}

export function focusOrdersTemplate(template: string, stackIds: string[]): string {
  if (stackIds.length === 0) return 'No units in current focus.';
  return filterOrdersTemplate(template, {
    stackIds: new Set(stackIds),
    includeHeader: false,
  }) || stackIds.map((id) => `#modulestack ${id}\n; (no order in report)`).join('\n\n');
}

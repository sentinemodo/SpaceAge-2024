import type { StackNode } from '../parsers/reportXml';
import { flattenStacks } from '../parsers/reportXml';

export interface OrderTemplateBlock {
  kind: 'modulestack' | 'person' | 'other';
  id: string;
  lines: string[];
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
  const lines = stripOrdersHeader(text).split('\n');
  const header: string[] = [];
  const footer: string[] = [];
  const blocks: OrderTemplateBlock[] = [];
  let current: OrderTemplateBlock | null = null;

  for (const line of lines) {
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
      };
      continue;
    }
    if (current) {
      current.lines.push(line);
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
      return !opts.stackIds || opts.stackIds.has(block.id);
    }
    if (block.kind === 'person') {
      return !opts.personIds || opts.personIds.has(block.id);
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

export function focusOrdersTemplate(template: string, stackIds: string[]): string {
  if (stackIds.length === 0) return 'No units in current focus.';
  return filterOrdersTemplate(template, {
    stackIds: new Set(stackIds),
    includeHeader: false,
  }) || stackIds.map((id) => `#modulestack ${id}\n; (no order in report)`).join('\n\n');
}

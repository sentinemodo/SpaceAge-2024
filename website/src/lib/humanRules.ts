import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

import { marked } from 'marked';

/** Repo root (parent of `website/`). */
const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../..');

const rulesPath = path.join(repoRoot, 'docs/human/rules.md');

const ORDERS_DICTIONARY_HEADING = '## Full orders dictionary';

const ORDER_DEF_LINE = /^\*\*([A-Z]+)\*\* —/gm;

export function orderAnchorId(verb: string): string {
  return `order-${verb.toLowerCase()}`;
}

export type OrdersIndex = {
  immediate: string[];
  long: string[];
};

/** Parse immediate vs long verbs from the orders dictionary in rules.md. */
export function parseOrdersIndex(markdown: string): OrdersIndex {
  const dictStart = markdown.indexOf(ORDERS_DICTIONARY_HEADING);
  if (dictStart === -1) {
    return { immediate: [], long: [] };
  }

  const dict = markdown.slice(dictStart);
  const immediateBlock = dict.match(/### Immediate\s+([\s\S]*?)(?=\n### Long\s*\n)/)?.[1] ?? '';
  const longBlock =
    dict.match(/### Long\s+([\s\S]*?)(?=\n---\s*\n\s*\n## Combat overview)/)?.[1] ?? '';

  const extractVerbs = (section: string): string[] => {
    const verbs: string[] = [];
    for (const match of section.matchAll(/^\*\*([A-Z]+)\*\* —/gm)) {
      verbs.push(match[1]);
    }
    return verbs;
  };

  return {
    immediate: extractVerbs(immediateBlock),
    long: extractVerbs(longBlock),
  };
}

/** Insert scroll targets before each dictionary verb (dictionary section only). */
export function injectOrderAnchors(markdown: string): string {
  const dictStart = markdown.indexOf(ORDERS_DICTIONARY_HEADING);
  if (dictStart === -1) {
    return markdown;
  }

  const before = markdown.slice(0, dictStart);
  const dict = markdown.slice(dictStart);

  const withAnchors = dict.replace(ORDER_DEF_LINE, (line, verb: string) => {
    const id = orderAnchorId(verb);
    return `<span id="${id}" class="order-anchor"></span>\n${line}`;
  });

  return before + withAnchors;
}

export function loadHumanRulesMarkdown(): string {
  return fs.readFileSync(rulesPath, 'utf-8');
}

export function renderHumanRulesHtml(): string {
  const markdown = injectOrderAnchors(loadHumanRulesMarkdown());
  return marked.parse(markdown, { gfm: true, async: false }) as string;
}

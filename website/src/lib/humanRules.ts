import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

import { marked } from 'marked';

/** Repo root (parent of `website/`). */
const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../..');

const rulesPath = path.join(repoRoot, 'docs/human/rules.md');

export function loadHumanRulesMarkdown(): string {
  return fs.readFileSync(rulesPath, 'utf-8');
}

export function renderHumanRulesHtml(): string {
  const markdown = loadHumanRulesMarkdown();
  return marked.parse(markdown, { gfm: true, async: false }) as string;
}

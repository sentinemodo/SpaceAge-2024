import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

import { marked } from 'marked';

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../..');
const faqPath = path.join(repoRoot, 'docs/human/faq.md');

export function loadFaqMarkdown(): string {
  return fs.readFileSync(faqPath, 'utf-8');
}

export function renderFaqHtml(): string {
  const markdown = loadFaqMarkdown();
  return marked.parse(markdown, { gfm: true, async: false }) as string;
}

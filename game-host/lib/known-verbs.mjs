import fs from 'node:fs';
import path from 'node:path';
import { repoRoot } from './paths.mjs';

/** Registered in OrderFactory but may lack a `### VERB` heading in rules export. */
const ENGINE_VERB_EXTRAS = ['JUMP'];

let cached = null;

export function knownVerbs() {
  if (cached) return cached;
  const allowlistPath = path.join(
    repoRoot(),
    'tools',
    'player-agent',
    'Lint',
    'verb-allowlist.json',
  );
  if (!fs.existsSync(allowlistPath)) {
    throw new Error(`Missing verb allowlist: ${allowlistPath}`);
  }
  const raw = fs.readFileSync(allowlistPath, 'utf8').replace(/^\uFEFF/, '');
  const doc = JSON.parse(raw);
  const fromRules = (doc.verbs || []).map((v) => String(v).toUpperCase());
  cached = new Set([...fromRules, ...ENGINE_VERB_EXTRAS]);
  return cached;
}

/** @internal test hook */
export function resetKnownVerbsCache() {
  cached = null;
}

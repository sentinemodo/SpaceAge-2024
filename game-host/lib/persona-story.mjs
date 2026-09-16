import fs from 'node:fs';
import path from 'node:path';
import { factionsDir, repoRoot, runRoot } from './paths.mjs';

export const PERSONA_IDS = ['military', 'economic', 'researcher', 'contractor'];

const DEFAULT_PERSONA_BLURBS = {
  military: 'Prioritize defense, tanks, and fauna clearance. Do not sell food early.',
  economic: 'Prioritize drills, farms, and energy before expansion.',
  researcher: 'Prioritize mobile lab survey and research anomalies first.',
  contractor: 'Prioritize UN contracts and module delivery jobs.',
};

function sharedPersonasDir(forRunId) {
  return path.join(runRoot(forRunId), 'shared', 'personas');
}

function personaPath(forRunId, personaId) {
  return path.join(sharedPersonasDir(forRunId), `${personaId}.md`);
}

function factionFolderName(factionId) {
  return String(factionId).padStart(2, '0');
}

export function storyPath(forRunId, factionId) {
  return path.join(factionsDir(forRunId), factionFolderName(factionId), 'story.md');
}

export function factionPersonaPath(forRunId, factionId) {
  return path.join(factionsDir(forRunId), factionFolderName(factionId), 'persona.md');
}

function ensurePersonaSeed(forRunId, personaId) {
  const dir = sharedPersonasDir(forRunId);
  fs.mkdirSync(dir, { recursive: true });
  const p = personaPath(forRunId, personaId);
  if (!fs.existsSync(p)) {
    const blurb = DEFAULT_PERSONA_BLURBS[personaId] || 'SpaceAge faction persona.';
    fs.writeFileSync(
      p,
      `# Persona: ${personaId}\n\n${blurb}\n`,
      'utf8'
    );
  }
}

export function listPersonas(forRunId) {
  PERSONA_IDS.forEach((id) => ensurePersonaSeed(forRunId, id));
  return PERSONA_IDS.map((id) => ({
    id,
    label: id.charAt(0).toUpperCase() + id.slice(1),
    path: personaPath(forRunId, id),
  }));
}

export function readPersona(forRunId, personaId) {
  if (!PERSONA_IDS.includes(personaId)) {
    throw new Error(`Unknown persona: ${personaId}`);
  }
  ensurePersonaSeed(forRunId, personaId);
  return fs.readFileSync(personaPath(forRunId, personaId), 'utf8');
}

export function savePersona(forRunId, personaId, text) {
  if (!PERSONA_IDS.includes(personaId)) {
    throw new Error(`Unknown persona: ${personaId}`);
  }
  fs.mkdirSync(sharedPersonasDir(forRunId), { recursive: true });
  fs.writeFileSync(personaPath(forRunId, personaId), text, 'utf8');
  return { ok: true };
}

export function readStory(forRunId, factionId) {
  const p = storyPath(forRunId, factionId);
  if (!fs.existsSync(p)) return '';
  return fs.readFileSync(p, 'utf8');
}

export function saveStory(forRunId, factionId, text) {
  const p = storyPath(forRunId, factionId);
  fs.mkdirSync(path.dirname(p), { recursive: true });
  fs.writeFileSync(p, text, 'utf8');
  return { ok: true };
}

/** Merge shared persona description into faction persona.md for draft-story. */
export function writeFactionPersonaFromShared(forRunId, factionId, personaId) {
  const sharedText = readPersona(forRunId, personaId);
  const factionPath = factionPersonaPath(forRunId, factionId);
  let credentialsBlock = '';
  if (fs.existsSync(factionPath)) {
    const existing = fs.readFileSync(factionPath, 'utf8');
    const credMatch = existing.match(/## Credentials[\s\S]*?(?=\n## |\z)/);
    if (credMatch) credentialsBlock = credMatch[0].trim() + '\n\n';
  }

  const merged = `# Faction ${factionId} persona\n\n${credentialsBlock}## Shared persona (${personaId})\n\n${sharedText.trim()}\n`;
  fs.mkdirSync(path.dirname(factionPath), { recursive: true });
  fs.writeFileSync(factionPath, merged, 'utf8');
  return factionPath;
}

export function latestFactionReportPath(forRunId, factionId) {
  const dir = path.join(factionsDir(forRunId), factionFolderName(factionId));
  if (!fs.existsSync(dir)) return null;
  const reports = fs.readdirSync(dir)
    .filter((f) => f.match(/^report\.\d+\.\d+\.txt$/))
    .sort()
    .reverse();
  return reports.length ? path.join(dir, reports[0]) : null;
}

export function playerRulesPath() {
  return path.join(repoRoot(), 'player', 'rules.md');
}

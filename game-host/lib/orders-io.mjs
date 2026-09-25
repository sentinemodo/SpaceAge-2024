import fs from 'node:fs';
import path from 'node:path';
import { factionsDir, readTurnFromGamein, runId, turnDir } from './paths.mjs';

/** @returns {string} e.g. orders.2.5.3.txt */
export function orderVersionFileName(factionId, turn, version) {
  return `orders.${factionId}.${turn}.${version}.txt`;
}

export function factionOrderFolder(factionId, forRunId = runId()) {
  return path.join(factionsDir(forRunId), String(factionId).padStart(2, '0'));
}

/** Sorted version numbers for this faction and turn (may be empty). */
export function listOrderVersions(factionId, turn, forRunId = runId()) {
  const folder = factionOrderFolder(factionId, forRunId);
  if (!fs.existsSync(folder)) return [];
  const re = new RegExp(`^orders\\.${factionId}\\.${turn}\\.(\\d+)\\.txt$`);
  const versions = [];
  for (const file of fs.readdirSync(folder)) {
    const m = file.match(re);
    if (m) versions.push(parseInt(m[1], 10));
  }
  return versions.sort((a, b) => a - b);
}

function readLegacyDraft(factionId, forRunId) {
  const legacyPath = path.join(factionOrderFolder(factionId, forRunId), `order.${factionId}.txt`);
  if (!fs.existsSync(legacyPath)) return null;
  const text = fs.readFileSync(legacyPath, 'utf8');
  return text.trim() ? text : null;
}

/** Latest submitted orders for this turn, or null when none exist. */
export function readSubmittedOrder(factionId, turn, forRunId = runId()) {
  let versions = listOrderVersions(factionId, turn, forRunId);
  if (!versions.length && turn === readTurnFromGamein(forRunId)) {
    const legacy = readLegacyDraft(factionId, forRunId);
    if (legacy) {
      const folder = factionOrderFolder(factionId, forRunId);
      fs.mkdirSync(folder, { recursive: true });
      fs.writeFileSync(
        path.join(folder, orderVersionFileName(factionId, turn, 1)),
        legacy,
        'utf8',
      );
      versions = [1];
    }
  }
  if (!versions.length) return null;
  const version = versions[versions.length - 1];
  const filePath = path.join(
    factionOrderFolder(factionId, forRunId),
    orderVersionFileName(factionId, turn, version),
  );
  const text = fs.readFileSync(filePath, 'utf8');
  return text.trim() ? text : null;
}

/** Persist next iteration for this turn plus latest draft/turn copies for processing. */
export function saveOrder(factionId, body, turn, forRunId = runId()) {
  const folder = factionOrderFolder(factionId, forRunId);
  fs.mkdirSync(folder, { recursive: true });

  const versions = listOrderVersions(factionId, turn, forRunId);
  const nextVersion = versions.length ? Math.max(...versions) + 1 : 1;
  const versionPath = path.join(folder, orderVersionFileName(factionId, turn, nextVersion));
  fs.writeFileSync(versionPath, body, 'utf8');

  const draftPath = path.join(folder, `order.${factionId}.txt`);
  fs.writeFileSync(draftPath, body, 'utf8');

  const turnPath = path.join(turnDir(forRunId), `order.${factionId}.txt`);
  fs.mkdirSync(turnDir(forRunId), { recursive: true });
  fs.writeFileSync(turnPath, Buffer.from(body, 'utf8'));

  return nextVersion;
}

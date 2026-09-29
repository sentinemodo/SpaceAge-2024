import fs from 'node:fs';
import path from 'node:path';
import { factionsDir, readTurnFromGamein, runId, turnDir } from './paths.mjs';

/**
 * Versioned faction order file name.
 * @returns {string} e.g. orders.2.1.3.txt — faction 2, report turn 1, version 3.
 */
export function orderVersionFileName(factionId, reportTurn, version) {
  return `orders.${factionId}.${reportTurn}.${version}.txt`;
}

/** Latest report.{turn}.{factionId}.txt turn in the faction folder (0 if none). */
export function latestReportTurnForFaction(factionId, forRunId = runId()) {
  const folder = factionOrderFolder(factionId, forRunId);
  if (!fs.existsSync(folder)) return 0;
  const re = new RegExp(`^report\\.(\\d+)\\.${factionId}\\.txt$`);
  let max = 0;
  for (const file of fs.readdirSync(folder)) {
    const m = file.match(re);
    if (m) max = Math.max(max, parseInt(m[1], 10));
  }
  return max;
}

function orderReportTurnLookupSequence(factionId, forRunId) {
  const gameinTurn = readTurnFromGamein(forRunId);
  const reportTurn = latestReportTurnForFaction(factionId, forRunId);
  const seq = [];
  if (gameinTurn > 0) seq.push(gameinTurn);
  if (reportTurn > 0 && reportTurn !== gameinTurn) seq.push(reportTurn);
  // Legacy player-agent off-by-one (reportTurn + 1 in the middle segment).
  if (reportTurn > 0) seq.push(reportTurn + 1);
  if (gameinTurn > 0 && gameinTurn !== reportTurn + 1) seq.push(gameinTurn + 1);
  return [...new Set(seq)];
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

function nonEmptyFile(filePath) {
  if (!fs.existsSync(filePath)) return false;
  const text = fs.readFileSync(filePath, 'utf8');
  return Boolean(text.trim());
}

/** True when the faction has orders for the active collection window (gamein turn or next). */
export function hasSubmittedOrders(factionId, turn, forRunId = runId()) {
  for (const orderTurn of [turn, turn + 1]) {
    const versions = listOrderVersions(factionId, orderTurn, forRunId);
    if (!versions.length) continue;
    const version = versions[versions.length - 1];
    const filePath = path.join(
      factionOrderFolder(factionId, forRunId),
      orderVersionFileName(factionId, orderTurn, version),
    );
    if (nonEmptyFile(filePath)) return true;
  }
  if (turn !== readTurnFromGamein(forRunId)) {
    return false;
  }
  const legacyPath = path.join(factionOrderFolder(factionId, forRunId), `order.${factionId}.txt`);
  return nonEmptyFile(legacyPath);
}

/** Path to the order file that should run for this faction (versioned latest, else legacy draft). */
export function resolveSubmittedOrderPathForTurnRun(factionId, forRunId = runId()) {
  for (const orderTurn of orderReportTurnLookupSequence(factionId, forRunId)) {
    const versions = listOrderVersions(factionId, orderTurn, forRunId);
    if (versions.length) {
      const version = versions[versions.length - 1];
      return path.join(
        factionOrderFolder(factionId, forRunId),
        orderVersionFileName(factionId, orderTurn, version),
      );
    }
  }
  const legacyPath = path.join(factionOrderFolder(factionId, forRunId), `order.${factionId}.txt`);
  return nonEmptyFile(legacyPath) ? legacyPath : null;
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

/** Copy each player faction's resolved order into turn-dir as order.{id}.txt for Game.exe. */
export function stageSubmittedOrdersForTurnRun(forRunId = runId()) {
  const tDir = turnDir(forRunId);
  fs.mkdirSync(tDir, { recursive: true });
  for (const file of fs.readdirSync(tDir)) {
    if (/^order\.\d+\.txt$/.test(file)) {
      fs.unlinkSync(path.join(tDir, file));
    }
  }
  for (let id = 2; id <= 11; id += 1) {
    const src = resolveSubmittedOrderPathForTurnRun(id, forRunId);
    if (!src) {
      throw new Error(`Missing orders for faction ${id} under ${factionsDir(forRunId)}`);
    }
    const body = fs.readFileSync(src, 'utf8');
    fs.writeFileSync(path.join(tDir, `order.${id}.txt`), body, 'utf8');
  }
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

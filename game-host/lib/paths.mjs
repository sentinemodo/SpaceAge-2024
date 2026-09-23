import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

export function repoRoot() {
  if (process.env.REPO_ROOT) {
    return path.resolve(process.env.REPO_ROOT);
  }
  return path.resolve(__dirname, '..', '..');
}

export function runId() {
  return process.env.GAME_HOST_RUN_ID || 'beta-1';
}

/** Canonical campaign run directory (shared by play scripts, game-host, player-agent). */
export function runsRoot() {
  return path.join(repoRoot(), 'play', 'runs');
}

export function runRoot(forRunId = runId()) {
  return path.join(runsRoot(), forRunId);
}

export function dataDir(forRunId = runId()) {
  return path.join(runRoot(forRunId), 'data');
}

export function turnDir(forRunId = runId()) {
  return path.join(runRoot(forRunId), 'turn');
}

export function factionsDir(forRunId = runId()) {
  return path.join(runRoot(forRunId), 'factions');
}

export function campaignDataXml() {
  return path.join(repoRoot(), 'play', 'campaign', 'data.xml');
}

export function ensureRunLayout(forRunId = runId()) {
  for (const dir of [dataDir(forRunId), turnDir(forRunId), factionsDir(forRunId)]) {
    fs.mkdirSync(dir, { recursive: true });
  }
}

export function gameinPath(forRunId = runId()) {
  return path.join(dataDir(forRunId), 'gamein.xml');
}

export function readTurnFromGamein(forRunId = runId()) {
  const xmlPath = gameinPath(forRunId);
  if (!fs.existsSync(xmlPath)) return 1;
  const xml = fs.readFileSync(xmlPath, 'utf8');
  const m = xml.match(/<game\s+turn="(\d+)"/);
  return m ? parseInt(m[1], 10) : 1;
}

export function listRuns() {
  const ids = new Set();
  const root = runsRoot();
  if (!fs.existsSync(root)) return [];
  for (const d of fs.readdirSync(root)) {
    try {
      if (fs.statSync(path.join(root, d)).isDirectory()) ids.add(d);
    } catch {
      /* skip */
    }
  }
  return [...ids].sort().map((id) => ({ id, label: id }));
}

function scanReportTurns(dir, turns) {
  if (!fs.existsSync(dir)) return;
  for (const file of fs.readdirSync(dir)) {
    const m = file.match(/^report\.(\d+)\./);
    if (m) turns.add(parseInt(m[1], 10));
  }
}

export function listTurns(forRunId = runId()) {
  const turns = new Set();
  scanReportTurns(turnDir(forRunId), turns);
  const fRoot = factionsDir(forRunId);
  scanReportTurns(fRoot, turns);
  if (fs.existsSync(fRoot)) {
    for (const sub of fs.readdirSync(fRoot)) {
      try {
        const subPath = path.join(fRoot, sub);
        if (fs.statSync(subPath).isDirectory()) scanReportTurns(subPath, turns);
      } catch {
        /* skip */
      }
    }
  }
  turns.add(readTurnFromGamein(forRunId));
  return [...turns].sort((a, b) => b - a);
}

export function latestTurn(forRunId = runId()) {
  const turns = listTurns(forRunId);
  return turns.length ? turns[0] : readTurnFromGamein(forRunId);
}

export function reportPaths(factionId, forRunId = runId(), turn = null) {
  const resolvedTurn = turn ?? latestTurn(forRunId);
  const base = `report.${resolvedTurn}.${factionId}`;
  const tDir = turnDir(forRunId);
  const xmlPath = path.join(tDir, `${base}.xml`);
  const txtPath = path.join(tDir, `${base}.txt`);
  const isoTxt = path.join(factionsDir(forRunId), String(factionId).padStart(2, '0'), `${base}.txt`);
  return { turn: resolvedTurn, runId: forRunId, xmlPath, txtPath, isoTxt };
}

/** @deprecated use reportPaths */
export function latestReportPaths(factionId, forRunId = runId(), turn = null) {
  return reportPaths(factionId, forRunId, turn);
}

export function gameExe() {
  return process.env.GAME_EXE || path.join(repoRoot(), 'Game', 'bin', 'Debug', 'Game.exe');
}

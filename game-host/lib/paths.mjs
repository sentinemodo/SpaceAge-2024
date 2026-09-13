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

export function runRoot() {
  return path.join(repoRoot(), 'game-host', 'runs', runId());
}

export function dataDir() {
  return path.join(runRoot(), 'data');
}

export function turnDir() {
  return path.join(runRoot(), 'turn');
}

export function factionsDir() {
  return path.join(runRoot(), 'factions');
}

export function gameExe() {
  return process.env.GAME_EXE || path.join(repoRoot(), 'Game', 'bin', 'Debug', 'Game.exe');
}

export function campaignDataXml() {
  return path.join(repoRoot(), 'campaign', 'data.xml');
}

export function ensureRunLayout() {
  for (const dir of [dataDir(), turnDir(), factionsDir()]) {
    fs.mkdirSync(dir, { recursive: true });
  }
}

export function gameinPath() {
  return path.join(dataDir(), 'gamein.xml');
}

export function readTurnFromGamein() {
  const xml = fs.readFileSync(gameinPath(), 'utf8');
  const m = xml.match(/<game\s+turn="(\d+)"/);
  return m ? parseInt(m[1], 10) : 1;
}

export function latestReportPaths(factionId) {
  const turn = readTurnFromGamein();
  const base = `report.${turn}.${factionId}`;
  const turnPath = turnDir();
  const xmlPath = path.join(turnPath, `${base}.xml`);
  const txtPath = path.join(turnPath, `${base}.txt`);
  const isoTxt = path.join(factionsDir(), String(factionId).padStart(2, '0'), `${base}.txt`);
  return { turn, xmlPath, txtPath, isoTxt };
}

/**
 * Node fallback for play/generate-status.ps1 (CI on Linux/macOS).
 * Keep logic in sync with the PowerShell producer.
 */
import * as fs from 'node:fs';
import * as path from 'node:path';
import { fileURLToPath } from 'node:url';

const PLAYER_FACTION_IDS = [2, 3, 4, 5, 6, 7, 8, 9, 10, 11];

export function factionFolderName(id) {
  return String(id).padStart(2, '0');
}

export function getTurnFromGamein(text) {
  const match = text.match(/<game\b[^>]*\bturn="(\d+)"/);
  return match ? Number.parseInt(match[1], 10) : null;
}

export function getLatestIsolatedReportTurn(factionsDir) {
  let maxTurn = null;
  for (const id of PLAYER_FACTION_IDS) {
    const folder = path.join(factionsDir, factionFolderName(id));
    if (!fs.existsSync(folder)) continue;
    for (const name of fs.readdirSync(folder)) {
      const match = name.match(/^report\.(\d+)\.\d+\.txt$/);
      if (!match) continue;
      const turn = Number.parseInt(match[1], 10);
      if (maxTurn === null || turn > maxTurn) maxTurn = turn;
    }
  }
  return maxTurn;
}

export function allIsolatedReportsPresent(factionsDir, turn) {
  return PLAYER_FACTION_IDS.every((id) =>
    fs.existsSync(path.join(factionsDir, factionFolderName(id), `report.${turn}.${id}.txt`)),
  );
}

export function readScheduleNextTurnAt(schedulePath) {
  if (!fs.existsSync(schedulePath)) return null;
  try {
    const schedule = JSON.parse(fs.readFileSync(schedulePath, 'utf8'));
    return schedule.nextTurnAt ?? null;
  } catch {
    return null;
  }
}

export function buildStatusPayload(runRoot, options = {}) {
  const dataDir = path.join(runRoot, 'data');
  const factionsDir = path.join(runRoot, 'factions');
  const gameinPath = path.join(dataDir, 'gamein.xml');
  const schedulePath = path.join(runRoot, 'gm', 'schedule.json');

  let status = 'not-started';
  let turn = 0;
  let nextTurnAt = readScheduleNextTurnAt(schedulePath);

  if (options.nextTurnAt !== undefined) {
    nextTurnAt = options.nextTurnAt;
  }

  const factions = PLAYER_FACTION_IDS.map((id) => ({
    id,
    submitted: false,
    name: `Faction ${id}`,
  }));

  if (fs.existsSync(gameinPath)) {
    const gameinText = fs.readFileSync(gameinPath, 'utf8');
    const parsedTurn = getTurnFromGamein(gameinText);
    turn = parsedTurn && parsedTurn > 0 ? parsedTurn : 1;

    let draftCount = 0;
    for (const faction of factions) {
      const draftPath = path.join(
        factionsDir,
        factionFolderName(faction.id),
        `order.${faction.id}.txt`,
      );
      faction.submitted = fs.existsSync(draftPath);
      if (faction.submitted) draftCount += 1;
    }

    const reportTurn = getLatestIsolatedReportTurn(factionsDir);
    const reportsOut =
      reportTurn !== null && allIsolatedReportsPresent(factionsDir, reportTurn);

    if (draftCount === PLAYER_FACTION_IDS.length) {
      status = 'processing';
    } else if (draftCount > 0) {
      status = 'accepting-orders';
    } else if (reportsOut) {
      status = 'reports-out';
    } else {
      status = 'not-started';
    }
  }

  return { status, turn, nextTurnAt, factions };
}

function parseArgs(argv) {
  const args = { runId: null, outPath: null, nextTurnAt: undefined };
  const positional = [];

  for (let i = 0; i < argv.length; i += 1) {
    const arg = argv[i];
    if (arg === '--out' || arg === '-OutPath') {
      args.outPath = argv[i + 1];
      i += 1;
    } else if (arg === '--next-turn-at' || arg === '-NextTurnAt') {
      args.nextTurnAt = argv[i + 1];
      i += 1;
    } else if (!arg.startsWith('-')) {
      positional.push(arg);
    }
  }

  args.runId = positional[0] ?? null;
  return args;
}

export function main(argv = process.argv.slice(2)) {
  const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..', '..');
  const args = parseArgs(argv);

  if (!args.runId) {
    console.error('Usage: node generate-status.mjs <RunId> [--out path] [--next-turn-at ISO8601|null]');
    process.exit(1);
  }

  const runRoot = path.join(repoRoot, 'play', 'runs', args.runId);
  const outPath = args.outPath ?? path.join(repoRoot, 'website', 'public', 'status.json');
  const payload = buildStatusPayload(runRoot, { nextTurnAt: args.nextTurnAt });

  fs.mkdirSync(path.dirname(outPath), { recursive: true });
  fs.writeFileSync(outPath, `${JSON.stringify(payload)}\n`, 'utf8');
  console.log(`Wrote status.json to ${outPath} (status=${payload.status}, turn=${payload.turn})`);
}

const __filename = fileURLToPath(import.meta.url);
const isMain = process.argv[1] && path.resolve(process.argv[1]) === __filename;
if (isMain) {
  main();
}

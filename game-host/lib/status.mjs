import fs from 'node:fs';
import path from 'node:path';
import { factionsDir, gameinPath, readTurnFromGamein, repoRoot, turnDir } from './paths.mjs';
import { loadFactionCredentials } from './auth.mjs';

const PLAYER_IDS = [2, 3, 4, 5, 6, 7, 8, 9, 10, 11];

export function buildStatusJson() {
  let turn = 1;
  let status = 'not-started';
  let nextTurnAt = null;

  const schedulePath = path.join(path.dirname(factionsDir()), 'gm', 'schedule.json');
  if (fs.existsSync(schedulePath)) {
    try {
      const sched = JSON.parse(fs.readFileSync(schedulePath, 'utf8'));
      const raw = sched.nextTurnAt ?? null;
      nextTurnAt = raw && String(raw).trim() ? raw : null;
    } catch { /* ignore */ }
  }

  const factions = PLAYER_IDS.map((id) => {
    const creds = fs.existsSync(gameinPath()) ? loadFactionCredentials() : new Map();
    const row = creds.get(id);
    const orderPath = path.join(turnDir(), `order.${id}.txt`);
    const draftPath = path.join(factionsDir(), String(id).padStart(2, '0'), `order.${id}.txt`);
    const submitted = fs.existsSync(orderPath) || fs.existsSync(draftPath);
    return { id, submitted, name: row?.name || `Faction ${id}` };
  });

  if (fs.existsSync(gameinPath())) {
    turn = readTurnFromGamein();
    const submittedCount = factions.filter((f) => f.submitted).length;
    if (submittedCount === PLAYER_IDS.length) {
      status = 'processing';
    } else if (submittedCount > 0) {
      status = 'accepting-orders';
    } else {
      const reportTurn = turn;
      const allReports = PLAYER_IDS.every((id) =>
        fs.existsSync(path.join(turnDir(), `report.${reportTurn}.${id}.txt`))
      );
      status = allReports ? 'reports-out' : 'accepting-orders';
    }
  }

  return { status, turn, nextTurnAt, factions };
}

/** Write lobby status.json for the static site (same schema as play/generate-status.ps1). */
export function syncLobbyStatusFile() {
  if (process.env.GAME_HOST_SKIP_LOBBY_SYNC === '1') return;
  const outPath =
    process.env.GAME_HOST_LOBBY_STATUS_PATH ||
    path.join(repoRoot(), 'website', 'public', 'status.json');
  const payload = buildStatusJson();
  fs.mkdirSync(path.dirname(outPath), { recursive: true });
  fs.writeFileSync(outPath, `${JSON.stringify(payload)}\n`, 'utf8');
}

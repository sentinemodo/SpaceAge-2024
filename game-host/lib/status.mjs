import fs from 'node:fs';
import path from 'node:path';
import { factionsDir, gameinPath, readTurnFromGamein, turnDir } from './paths.mjs';
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
      nextTurnAt = sched.nextTurnAt ?? null;
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

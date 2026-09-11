import fs from 'node:fs';
import path from 'node:path';
import { factionsDir, readTurnFromGamein, turnDir } from './paths.mjs';

const PLAYER_IDS = [2, 3, 4, 5, 6, 7, 8, 9, 10, 11];

export function isolateReports() {
  const turn = readTurnFromGamein();
  for (const id of PLAYER_IDS) {
    const folder = path.join(factionsDir(), String(id).padStart(2, '0'));
    fs.mkdirSync(folder, { recursive: true });
    const src = path.join(turnDir(), `report.${turn}.${id}.txt`);
    const dst = path.join(folder, `report.${turn}.${id}.txt`);
    if (fs.existsSync(src)) {
      fs.copyFileSync(src, dst);
    }
  }
}

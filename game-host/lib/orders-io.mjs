import fs from 'node:fs';
import path from 'node:path';
import { factionsDir, turnDir } from './paths.mjs';

/** Write player UTF-8 draft and Windows-1251 turn copy. */
export function saveOrder(factionId, body) {
  const folder = path.join(factionsDir(), String(factionId).padStart(2, '0'));
  fs.mkdirSync(folder, { recursive: true });
  const draftPath = path.join(folder, `order.${factionId}.txt`);
  fs.writeFileSync(draftPath, body, 'utf8');

  const turnPath = path.join(turnDir(), `order.${factionId}.txt`);
  fs.mkdirSync(turnDir(), { recursive: true });
  const buf = Buffer.from(body, 'utf8');
  fs.writeFileSync(turnPath, buf);
}

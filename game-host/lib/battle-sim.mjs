import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { spawnGame } from './game-exe.mjs';
import { dataDir } from './paths.mjs';

export async function runBattleSimulation(xml, seed) {
  const tmpDir = fs.mkdtempSync(path.join(os.tmpdir(), 'sa-bsim-'));
  const inputPath = path.join(tmpDir, 'sim-input.xml');
  fs.writeFileSync(inputPath, xml, 'utf8');
  try {
    const args = ['/battle-sim', inputPath, '/data', dataDir()];
    if (seed != null && seed !== '') {
      args.push('/seed', String(seed));
    }
    const output = await spawnGame(args);
    const resultMatch = output.match(/SIMULATION RESULT:\s*(\S+)/);
    return {
      output,
      result: resultMatch ? resultMatch[1] : null,
    };
  } finally {
    fs.rmSync(tmpDir, { recursive: true, force: true });
  }
}

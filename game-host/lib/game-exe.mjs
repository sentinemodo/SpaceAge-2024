import { spawn } from 'node:child_process';
import fs from 'node:fs';
import path from 'node:path';
import { campaignDataXml, dataDir, gameExe, turnDir } from './paths.mjs';

export function spawnGame(args, { cwd } = {}) {
  const exe = gameExe();
  if (!fs.existsSync(exe)) {
    return Promise.reject(new Error(`Game.exe not found at ${exe}. Build Game project first.`));
  }
  return new Promise((resolve, reject) => {
    const child = spawn(exe, args, {
      cwd: cwd || path.dirname(exe),
      windowsHide: true,
    });
    let stdout = '';
    let stderr = '';
    child.stdout.on('data', (d) => { stdout += d; });
    child.stderr.on('data', (d) => { stderr += d; });
    child.on('error', reject);
    child.on('close', (code) => {
      if (code !== 0) {
        reject(new Error(stderr || stdout || `Game.exe exited ${code}`));
      } else {
        resolve(stdout);
      }
    });
  });
}

export async function bootstrapFromCampaign() {
  fs.copyFileSync(campaignDataXml(), path.join(dataDir(), 'data.xml'));
  const seed = path.join(path.dirname(campaignDataXml()), 'gamein.1.xml');
  if (!fs.existsSync(seed)) {
    throw new Error('campaign/gamein.1.xml missing');
  }
  fs.copyFileSync(seed, path.join(dataDir(), 'gamein.xml'));
}

/** Copy live passwords from play/runs/{id}/data after init-run.ps1 */
export function bootstrapFromPlayRun(sourceRunId) {
  const srcData = path.join(repoRoot(), 'play', 'runs', sourceRunId, 'data');
  if (!fs.existsSync(path.join(srcData, 'gamein.xml'))) {
    throw new Error(`play/runs/${sourceRunId}/data/gamein.xml missing — run play/init-run.ps1 first`);
  }
  fs.copyFileSync(path.join(srcData, 'data.xml'), path.join(dataDir(), 'data.xml'));
  fs.copyFileSync(path.join(srcData, 'gamein.xml'), path.join(dataDir(), 'gamein.xml'));
}

export async function runReports() {
  return spawnGame(['/data', dataDir(), '/turn-dir', turnDir(), '/reports']);
}

export async function runTurn() {
  return spawnGame(['/data', dataDir(), '/turn-dir', turnDir()]);
}

export async function runNoTurn() {
  return spawnGame(['/data', dataDir(), '/turn-dir', turnDir(), '/no-turn']);
}

export function promoteGameout(turn) {
  const out = path.join(dataDir(), `gameout.${turn}.xml`);
  if (!fs.existsSync(out)) {
    throw new Error(`Missing ${out}`);
  }
  fs.copyFileSync(out, path.join(dataDir(), 'gamein.xml'));
}

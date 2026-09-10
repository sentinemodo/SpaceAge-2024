import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import * as fs from 'node:fs';
import * as path from 'node:path';
import { promisify } from 'node:util';
import { exec as _exec } from 'node:child_process';
import { validateStatusJson } from '../src/lib/statusSchema';
import { buildStatusPayload } from '../scripts/generate-status.mjs';

const exec = promisify(_exec);

const RepoRoot = path.resolve(import.meta.dirname, '..', '..');
const PlayDir = path.join(RepoRoot, 'play');
const RunsDir = path.join(PlayDir, 'runs');
const TestRunId = 'test-gen';
const TestRunPaths = {
  root: path.join(RunsDir, TestRunId),
  data: path.join(RunsDir, TestRunId, 'data'),
  turn: path.join(RunsDir, TestRunId, 'turn'),
  factions: path.join(RunsDir, TestRunId, 'factions'),
  gm: path.join(RunsDir, TestRunId, 'gm'),
};
const GeneratedStatusOut = path.join(import.meta.dirname, '.tmp', 'generated-status.json');
const NodeScript = path.join(import.meta.dirname, '..', 'scripts', 'generate-status.mjs');

async function whichShell() {
  try {
    await exec('pwsh -v');
    return 'pwsh';
  } catch {
    try {
      await exec('powershell.exe -v');
      return 'powershell.exe';
    } catch {
      return null;
    }
  }
}

function writeGamein(turn: number) {
  const gamein = `<game turn="${turn}">\n</game>`;
  fs.writeFileSync(path.join(TestRunPaths.data, 'gamein.xml'), gamein, { encoding: 'utf8' });
}

function writeFactionDraft(id: number) {
  const folder = path.join(TestRunPaths.factions, String(id).padStart(2, '0'));
  fs.mkdirSync(folder, { recursive: true });
  fs.writeFileSync(
    path.join(folder, `order.${id}.txt`),
    `#faction ${id} \nnoop`,
    { encoding: 'utf8' },
  );
}

function writeIsolatedReports(turn: number) {
  for (let id = 2; id <= 11; id += 1) {
    const folder = path.join(TestRunPaths.factions, String(id).padStart(2, '0'));
    fs.mkdirSync(folder, { recursive: true });
    fs.writeFileSync(
      path.join(folder, `report.${turn}.${id}.txt`),
      `Report for faction ${id}`,
      { encoding: 'utf8' },
    );
  }
}

async function runNodeProducer(outPath: string) {
  const cmd = `node "${NodeScript}" ${TestRunId} --out "${outPath}"`;
  await exec(cmd, { cwd: RepoRoot });
  return 'node';
}

async function runProducer(outPath: string) {
  // Linux CI: Node fallback (pwsh on ubuntu-latest is slow/flaky). Windows job tests PowerShell.
  if (process.platform !== 'win32') {
    return runNodeProducer(outPath);
  }

  const shell = await whichShell();
  if (shell) {
    const script = path.join(RepoRoot, 'play', 'generate-status.ps1');
    const cmd = `${shell} -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "${script}" ${TestRunId} -OutPath "${outPath}"`;
    const { stderr } = await exec(cmd, { cwd: RepoRoot });
    if (stderr && stderr.length > 0) {
      console.warn('generate-status.ps1 stderr:', stderr);
    }
    return 'powershell';
  }

  return runNodeProducer(outPath);
}

describe('Status producer script', () => {
  beforeEach(() => {
    if (!fs.existsSync(RunsDir)) fs.mkdirSync(RunsDir, { recursive: true });
    if (fs.existsSync(TestRunPaths.root)) fs.rmSync(TestRunPaths.root, { recursive: true, force: true });
    fs.mkdirSync(TestRunPaths.data, { recursive: true });
    fs.mkdirSync(TestRunPaths.turn, { recursive: true });
    fs.mkdirSync(path.dirname(GeneratedStatusOut), { recursive: true });
    if (fs.existsSync(GeneratedStatusOut)) fs.unlinkSync(GeneratedStatusOut);
  });

  afterEach(() => {
    if (fs.existsSync(TestRunPaths.root)) fs.rmSync(TestRunPaths.root, { recursive: true, force: true });
    if (fs.existsSync(GeneratedStatusOut)) fs.unlinkSync(GeneratedStatusOut);
  });

  it('generates status.json with partial order submissions (accepting-orders)', async () => {
    writeGamein(3);
    writeIsolatedReports(3);
    for (let id = 2; id <= 6; id += 1) {
      writeFactionDraft(id);
    }

    const runner = await runProducer(GeneratedStatusOut);
    expect(fs.existsSync(GeneratedStatusOut)).toBe(true);

    const data = validateStatusJson(JSON.parse(fs.readFileSync(GeneratedStatusOut, { encoding: 'utf8' })));
    expect(data.turn).toBe(3);
    expect(data.status).toBe('accepting-orders');
    expect(data.factions).toHaveLength(10);

    const idsSubmitted = data.factions.filter((f) => f.submitted).map((f) => f.id).sort();
    expect(idsSubmitted).toEqual([2, 3, 4, 5, 6]);
    expect(runner).toMatch(/powershell|node/);
  });

  it('generates reports-out when reports are isolated and no orders submitted', async () => {
    writeGamein(2);
    writeIsolatedReports(2);

    await runProducer(GeneratedStatusOut);
    const data = validateStatusJson(JSON.parse(fs.readFileSync(GeneratedStatusOut, { encoding: 'utf8' })));
    expect(data.status).toBe('reports-out');
    expect(data.turn).toBe(2);
  });

  it('generates processing when all ten faction drafts exist', async () => {
    writeGamein(4);
    writeIsolatedReports(4);
    for (let id = 2; id <= 11; id += 1) {
      writeFactionDraft(id);
    }

    await runProducer(GeneratedStatusOut);
    const data = validateStatusJson(JSON.parse(fs.readFileSync(GeneratedStatusOut, { encoding: 'utf8' })));
    expect(data.status).toBe('processing');
  });

  it('reads nextTurnAt from gm/schedule.json', async () => {
    writeGamein(1);
    writeIsolatedReports(1);
    fs.mkdirSync(TestRunPaths.gm, { recursive: true });
    fs.writeFileSync(
      path.join(TestRunPaths.gm, 'schedule.json'),
      JSON.stringify({ nextTurnAt: '2026-09-15T23:59:59Z' }),
      'utf8',
    );

    await runProducer(GeneratedStatusOut);
    const data = validateStatusJson(JSON.parse(fs.readFileSync(GeneratedStatusOut, { encoding: 'utf8' })));
    expect(data.nextTurnAt).toBe('2026-09-15T23:59:59Z');
  });

  it('Node buildStatusPayload matches producer integration scenarios', () => {
    writeGamein(3);
    writeIsolatedReports(3);
    for (let id = 2; id <= 6; id += 1) {
      writeFactionDraft(id);
    }

    const payload = buildStatusPayload(TestRunPaths.root);
    const data = validateStatusJson(payload);
    expect(data.turn).toBe(3);
    expect(data.status).toBe('accepting-orders');
  });
});

import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import * as fs from 'node:fs';
import * as path from 'node:path';
import { promisify } from 'node:util';
import { exec as _exec } from 'node:child_process';
import { validateStatusJson } from '../src/lib/statusSchema';

const exec = promisify(_exec);

const RepoRoot = path.resolve(import.meta.dirname, '..', '..');
const PlayDir = path.join(RepoRoot, 'play');
const RunsDir = path.join(PlayDir, 'runs');
const TestRunId = 'test-gen';
const TestRunPaths = {
  root: path.join(RunsDir, TestRunId),
  data: path.join(RunsDir, TestRunId, 'data'),
  turn: path.join(RunsDir, TestRunId, 'turn'),
};
const GeneratedStatusOut = path.join(import.meta.dirname, '.tmp', 'generated-status.json');

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

describe('Status producer script', () => {
  beforeEach(() => {
    if (!fs.existsSync(RunsDir)) fs.mkdirSync(RunsDir, { recursive: true });
    if (fs.existsSync(TestRunPaths.root)) fs.rmSync(TestRunPaths.root, { recursive: true, force: true });
    fs.mkdirSync(TestRunPaths.data, { recursive: true });
    fs.mkdirSync(TestRunPaths.turn, { recursive: true });
    fs.mkdirSync(path.dirname(GeneratedStatusOut), { recursive: true });

    const gamein = '<game>\n  <turn>3</turn>\n</game>';
    fs.writeFileSync(path.join(TestRunPaths.data, 'gamein.xml'), gamein, { encoding: 'utf8' });

    for (let id = 2; id <= 6; id += 1) {
      fs.writeFileSync(
        path.join(TestRunPaths.turn, `order.${id}.txt`),
        `#faction ${id} \nnoop`,
        { encoding: 'utf8' },
      );
    }

    if (fs.existsSync(GeneratedStatusOut)) fs.unlinkSync(GeneratedStatusOut);
  });

  afterEach(() => {
    if (fs.existsSync(TestRunPaths.root)) fs.rmSync(TestRunPaths.root, { recursive: true, force: true });
    if (fs.existsSync(GeneratedStatusOut)) fs.unlinkSync(GeneratedStatusOut);
  });

  it('generates a status.json file with expected schema', async () => {
    const shell = await whichShell();
    if (!shell) {
      console.warn('No PowerShell executable found; skipping status producer integration test.');
      return;
    }

    const script = path.join(RepoRoot, 'play', 'generate-status.ps1');
    const cmd = `${shell} -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "${script}" ${TestRunId} -OutPath "${GeneratedStatusOut}"`;
    const { stderr } = await exec(cmd, { cwd: RepoRoot });
    if (stderr && stderr.length > 0) {
      console.warn('generate-status.ps1 stderr:', stderr);
    }

    expect(fs.existsSync(GeneratedStatusOut)).toBe(true);
    const data = validateStatusJson(JSON.parse(fs.readFileSync(GeneratedStatusOut, { encoding: 'utf8' })));

    expect(data.turn).toBe(3);
    expect(data.factions).toHaveLength(10);

    const idsSubmitted = data.factions.filter((f) => f.submitted).map((f) => f.id).sort();
    expect(idsSubmitted).toEqual([2, 3, 4, 5, 6]);
  });
});

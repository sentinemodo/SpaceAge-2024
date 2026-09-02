import { describe, it, expect, beforeEach, afterEach } from 'vitest'
import * as fs from 'fs'
import * as path from 'path'
import { promisify } from 'util'
import { exec as _exec } from 'child_process'
const exec = promisify(_exec)

const RepoRoot = path.resolve(__dirname, '..', '..')
const PlayDir = path.join(RepoRoot, 'play')
const RunsDir = path.join(PlayDir, 'runs')
const TestRunId = 'test-gen'
const TestRunPaths = {
  root: path.join(RunsDir, TestRunId),
  data: path.join(RunsDir, TestRunId, 'data'),
  turn: path.join(RunsDir, TestRunId, 'turn'),
}
const StatusOut = path.join(RepoRoot, 'website', 'public', 'status.json')

async function whichShell() {
  // Prefer pwsh (PowerShell Core) then fallback to powershell.exe
  try {
    await exec('pwsh -v')
    return 'pwsh'
  } catch {
    try {
      await exec('powershell.exe -v')
      return 'powershell.exe'
    } catch {
      return null
    }
  }
}

describe('Status producer script', () => {
  beforeEach(() => {
    // Ensure clean dirs
    if (!fs.existsSync(RunsDir)) fs.mkdirSync(RunsDir, { recursive: true })
    if (fs.existsSync(TestRunPaths.root)) fs.rmSync(TestRunPaths.root, { recursive: true, force: true })
    fs.mkdirSync(TestRunPaths.data, { recursive: true })
    fs.mkdirSync(TestRunPaths.turn, { recursive: true })

    // Write a minimal gamein.xml with turn 3
    const gamein = '<game>\n  <turn>3</turn>\n</game>'
    fs.writeFileSync(path.join(TestRunPaths.data, 'gamein.xml'), gamein, { encoding: 'utf8' })

    // Create some order files: mark factions 2..6 submitted, others not
    for (let id = 2; id <= 6; id++) {
      fs.writeFileSync(path.join(TestRunPaths.turn, `order.${id}.txt`), `#faction ${id} \nnoop`, { encoding: 'utf8' })
    }

    // Remove any existing status.json
    if (fs.existsSync(StatusOut)) fs.unlinkSync(StatusOut)
  })

  afterEach(() => {
    // cleanup
    if (fs.existsSync(TestRunPaths.root)) fs.rmSync(TestRunPaths.root, { recursive: true, force: true })
    if (fs.existsSync(StatusOut)) fs.unlinkSync(StatusOut)
  })

  it('generates a status.json file with expected schema', async () => {
    const shell = await whichShell()
    if (!shell) {
      // Skip if no PowerShell available in environment
      console.warn('No PowerShell executable found; skipping status producer integration test.')
      return
    }

    const script = path.join(RepoRoot, 'play', 'generate-status.ps1')
    const cmd = `${shell} -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "${script}" ${TestRunId}`
    const { stdout, stderr } = await exec(cmd, { cwd: RepoRoot })
    if (stderr && stderr.length > 0) {
      console.warn('generate-status.ps1 stderr:', stderr)
    }

    expect(fs.existsSync(StatusOut)).toBe(true)
    const raw = fs.readFileSync(StatusOut, { encoding: 'utf8' })
    const data = JSON.parse(raw)

    expect(data).toHaveProperty('status')
    expect(data).toHaveProperty('turn')
    expect(data.turn).toBe(3)
    expect(Array.isArray(data.factions)).toBe(true)
    expect(data.factions).toHaveLength(10)

    // factions 2..6 should be submitted
    const idsSubmitted = data.factions.filter((f: any) => f.submitted).map((f: any) => f.id).sort()
    expect(idsSubmitted).toEqual([2,3,4,5,6])
  })
})

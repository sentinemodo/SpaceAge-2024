import { spawn } from 'node:child_process';
import fs from 'node:fs';
import path from 'node:path';
import { repoRoot } from './paths.mjs';

const PROJECT = path.join(repoRoot(), 'tools', 'player-agent', 'PlayerAgent.csproj');

export function playerAgentProject() {
  return process.env.GAME_HOST_PLAYER_AGENT_PROJECT || PROJECT;
}

function parseJsonMarker(stdout) {
  const marker = '---JSON---';
  const idx = stdout.indexOf(marker);
  if (idx < 0) return null;
  const jsonText = stdout.slice(idx + marker.length).trim();
  try {
    return JSON.parse(jsonText.split('\n')[0]);
  } catch {
    return null;
  }
}

export function spawnPlayerAgent(args, { env = {}, timeoutMs = 600_000 } = {}) {
  const project = playerAgentProject();
  if (!fs.existsSync(project)) {
    return Promise.reject(new Error(`player-agent project not found: ${project}`));
  }

  const mergedEnv = {
    ...process.env,
    ...env,
    PLAYER_AGENT_ALLOW_RUNPOD: env.PLAYER_AGENT_ALLOW_RUNPOD ?? process.env.PLAYER_AGENT_ALLOW_RUNPOD ?? '1',
  };

  return new Promise((resolve, reject) => {
    const child = spawn(
      process.env.DOTNET_EXE || 'dotnet',
      ['run', '--project', project, '--', ...args],
      {
        cwd: repoRoot(),
        env: mergedEnv,
        windowsHide: true,
      }
    );
    let stdout = '';
    let stderr = '';
    const timer = timeoutMs > 0
      ? setTimeout(() => {
        child.kill('SIGTERM');
        reject(new Error(`player-agent timed out after ${timeoutMs}ms`));
      }, timeoutMs)
      : null;

    child.stdout.on('data', (d) => { stdout += d; });
    child.stderr.on('data', (d) => { stderr += d; });
    child.on('error', (err) => {
      if (timer) clearTimeout(timer);
      reject(err);
    });
    child.on('close', (code) => {
      if (timer) clearTimeout(timer);
      if (code !== 0) {
        reject(new Error(stderr.trim() || stdout.trim() || `player-agent exited ${code}`));
        return;
      }
      resolve({ stdout, stderr, json: parseJsonMarker(stdout) });
    });
  });
}

export async function buildPlayerAgent() {
  const project = playerAgentProject();
  return new Promise((resolve, reject) => {
    const child = spawn(
      process.env.DOTNET_EXE || 'dotnet',
      ['build', project, '-c', 'Release', '-v', 'q'],
      { cwd: repoRoot(), windowsHide: true }
    );
    let stderr = '';
    child.stderr.on('data', (d) => { stderr += d; });
    child.on('error', reject);
    child.on('close', (code) => {
      if (code !== 0) reject(new Error(stderr || `dotnet build failed ${code}`));
      else resolve(true);
    });
  });
}

export async function runAiQuery({
  runId,
  factionId,
  query,
  includeStory,
  reportPath,
  storyPath,
  personaPath,
  ollamaHost,
}) {
  const args = [
    'query',
    '--mode', 'campaign',
    '--run', runId,
    '--faction', String(factionId),
    '--query', query,
    '--json',
    '--allow-runpod',
    '--yes',
  ];
  if (includeStory) args.push('--include-story');
  if (reportPath) args.push('--report', reportPath);
  if (storyPath) args.push('--story-path', storyPath);
  if (personaPath) args.push('--persona-path', personaPath);

  const env = {};
  if (ollamaHost) env.OLLAMA_HOST = ollamaHost;

  const result = await spawnPlayerAgent(args, { env });
  if (result.json) return result.json;
  return { ok: true, query, output: result.stdout.trim(), hits: 0 };
}

export async function runDraftStory({
  runId,
  factionId,
  reportPath,
  personaPath,
  outputPath,
  ollamaHost,
}) {
  const args = [
    'draft-story',
    '--run', runId,
    '--faction', String(factionId),
    '--allow-runpod',
    '--yes',
  ];
  if (reportPath) args.push('--report', reportPath);
  if (personaPath) args.push('--persona-path', personaPath);
  if (outputPath) args.push('--output', outputPath);

  const env = {};
  if (ollamaHost) env.OLLAMA_HOST = ollamaHost;

  const result = await spawnPlayerAgent(args, { env, timeoutMs: 900_000 });
  return { ok: true, output: result.stdout.trim() };
}

export async function warmupOllama(ollamaHost) {
  const args = ['smoke', '--allow-runpod', '--yes'];
  const result = await spawnPlayerAgent(args, {
    env: { OLLAMA_HOST: ollamaHost },
    timeoutMs: 300_000,
  });
  return { ok: true, output: result.stdout.trim() };
}

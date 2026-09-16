import {
  latestFactionReportPath,
  readStory,
  saveStory,
  storyPath,
  writeFactionPersonaFromShared,
} from './persona-story.mjs';
import { runAiQuery, runDraftStory } from './player-agent.mjs';
import { currentOllamaHost, loadRunPodState } from './runpod.mjs';

export function runPodStatusPayload() {
  const state = loadRunPodState();
  return {
    phase: state.phase || 'stopped',
    stage: state.stage || (state.phase === 'running' ? 'ready' : 'idle'),
    podId: state.podId || null,
    ollamaHost: state.ollamaHost || null,
    model: state.model || process.env.PLAYER_AGENT_CHAT_MODEL || 'qwen3-coder:30b',
    message: state.message || '',
    ollamaReady: !!state.ollamaReady,
    gpuAttempt: state.gpuAttempt || null,
    podStatus: state.podStatus || null,
    logs: state.logs || [],
  };
}

export async function executeAiQuery({
  runId,
  factionId,
  prompt,
  includeStory,
  reportPath,
}) {
  const state = loadRunPodState();
  if (state.phase !== 'running' && !process.env.OLLAMA_HOST) {
    throw new Error('RunPod is not running. Start RunPod before querying.');
  }

  const resolvedReport = reportPath || latestFactionReportPath(runId, factionId);
  const result = await runAiQuery({
    runId,
    factionId,
    query: prompt,
    includeStory,
    reportPath: resolvedReport,
    storyPath: storyPath(runId, factionId),
    personaPath: null,
    ollamaHost: currentOllamaHost(),
  });

  return {
    ok: !!result.ok,
    query: prompt,
    output: result.output || '',
    hits: result.hits ?? 0,
  };
}

export async function regenerateStory({
  runId,
  factionId,
  personaId,
  reportPath,
}) {
  const state = loadRunPodState();
  if (state.phase !== 'running' && !process.env.OLLAMA_HOST) {
    throw new Error('RunPod is not running. Start RunPod before regenerating story.');
  }

  const personaFile = writeFactionPersonaFromShared(runId, factionId, personaId);
  const resolvedReport = reportPath || latestFactionReportPath(runId, factionId);
  if (!resolvedReport) {
    throw new Error('No faction report found for story draft');
  }

  const outPath = storyPath(runId, factionId);
  const result = await runDraftStory({
    runId,
    factionId,
    reportPath: resolvedReport,
    personaPath: personaFile,
    outputPath: outPath,
    ollamaHost: currentOllamaHost(),
  });

  const text = readStory(runId, factionId);
  saveStory(runId, factionId, text);
  return {
    ok: true,
    output: result.output,
    story: text,
  };
}

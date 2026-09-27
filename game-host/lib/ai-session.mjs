import {
  latestFactionReportPath,
  readStory,
  saveStory,
  storyPath,
  writeFactionPersonaFromShared,
} from './persona-story.mjs';
import { runAiQuery, runDraftStory } from './player-agent.mjs';
import { currentOllamaHost, loadRunPodState, prepareOllamaModels } from './runpod.mjs';

async function resolveInferenceHost() {
  const state = loadRunPodState();
  if (state.podId) {
    if (!state.ollamaHost) {
      throw new Error('RunPod pod is attached but Ollama URL is missing. Stop and start RunPod again.');
    }
    if (!state.ollamaReady) {
      throw new Error('RunPod is still warming up (pulling models). Wait until status shows ready.');
    }
    await prepareOllamaModels(state.ollamaHost);
    return state.ollamaHost;
  }
  const host = currentOllamaHost();
  if (!host) {
    throw new Error('RunPod is not running and OLLAMA_HOST is unset.');
  }
  return host;
}

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
  const ollamaHost = await resolveInferenceHost();

  const resolvedReport = reportPath || latestFactionReportPath(runId, factionId);
  const result = await runAiQuery({
    runId,
    factionId,
    query: prompt,
    includeStory,
    reportPath: resolvedReport,
    storyPath: storyPath(runId, factionId),
    personaPath: null,
    ollamaHost,
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
  const ollamaHost = await resolveInferenceHost();

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
    ollamaHost,
  });

  const text = (result.story || readStory(runId, factionId)).trim();
  if (text) {
    saveStory(runId, factionId, text);
  }
  return {
    ok: true,
    output: result.output,
    story: text,
  };
}

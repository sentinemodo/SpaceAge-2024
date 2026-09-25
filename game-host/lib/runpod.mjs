import './env-bootstrap.mjs';
import fs from 'node:fs';
import path from 'node:path';
import { repoEnvFilePath, runpodApiKey } from './load-env.mjs';
import { repoRoot } from './paths.mjs';
import { warmupOllama } from './player-agent.mjs';

const API_BASE = process.env.RUNPOD_API_BASE || 'https://api.runpod.io/v2';
const GPU_ALTERNATES = ['NVIDIA GeForce RTX 4090', 'NVIDIA GeForce RTX 5090'];
const GPU_RETRY_MS = 4000;
const MAX_GPU_ATTEMPTS = 90;
const MAX_LOG_ENTRIES = 80;
const OLLAMA_PROBE_TIMEOUT_MS = 10_000;
const OLLAMA_WARMUP_ATTEMPTS = 12;
const OLLAMA_WARMUP_DELAY_MS = 3000;

let startJob = null;

function statePath() {
  return path.join(repoRoot(), 'game-host', '.runpod-state.json');
}

export function isGpuCapacityError(err) {
  const msg = String(err?.message || err).toLowerCase();
  return (
    msg.includes('no longer any instances')
    || msg.includes('no instances available')
    || msg.includes('out of stock')
    || msg.includes('not available in')
    || msg.includes('capacity')
  );
}

export function isRunPodAuthError(err) {
  const msg = String(err?.message || err).toLowerCase();
  return msg.includes('unauthorized') || msg.includes('invalid api key');
}

export function gpuAlternationList(envGpuId) {
  if (envGpuId && !GPU_ALTERNATES.includes(envGpuId)) {
    return [envGpuId];
  }
  return [...GPU_ALTERNATES];
}

function trimLogs(logs = []) {
  return logs.slice(-MAX_LOG_ENTRIES);
}

function appendRunPodLog(state, level, message, detail = null) {
  const entry = {
    at: new Date().toISOString(),
    level,
    message,
    ...(detail != null ? { detail } : {}),
  };
  return trimLogs([...(state.logs || []), entry]);
}

export function loadRunPodState() {
  const p = statePath();
  if (!fs.existsSync(p)) {
    return {
      phase: 'stopped',
      stage: 'idle',
      podId: null,
      ollamaHost: null,
      message: null,
      logs: [],
      updatedAt: null,
    };
  }
  try {
    const state = JSON.parse(fs.readFileSync(p, 'utf8'));
    if (state.phase !== 'stopped' && !state.podId) {
      return {
        phase: 'stopped',
        stage: state.stage === 'failed' ? 'failed' : 'idle',
        podId: null,
        ollamaHost: null,
        ollamaReady: false,
        message: state.message || 'Previous start failed (no pod id)',
        logs: state.logs || [],
        updatedAt: state.updatedAt || null,
      };
    }
    return {
      stage: 'idle',
      logs: [],
      ...state,
    };
  } catch {
    return {
      phase: 'stopped',
      stage: 'idle',
      podId: null,
      ollamaHost: null,
      message: null,
      logs: [],
      updatedAt: null,
    };
  }
}

function saveRunPodState(state) {
  const next = {
    ...state,
    logs: trimLogs(state.logs || []),
    updatedAt: new Date().toISOString(),
  };
  fs.mkdirSync(path.dirname(statePath()), { recursive: true });
  fs.writeFileSync(statePath(), JSON.stringify(next, null, 2), 'utf8');
  return next;
}

function logState(level, message, detail = null) {
  const current = loadRunPodState();
  return saveRunPodState({
    ...current,
    logs: appendRunPodLog(current, level, message, detail),
  });
}

function extractPodId(pod) {
  const id = pod?.id || pod?.podId;
  if (!id || typeof id !== 'string') {
    throw new Error(`RunPod create response missing pod id (${JSON.stringify(pod)?.slice(0, 240) || 'empty'})`);
  }
  return id;
}

function podStatus(pod) {
  return String(pod?.status || pod?.desiredStatus || pod?.lastStatus || '').toUpperCase();
}

const TERMINAL_POD_STATUSES = ['EXITED', 'TERMINATED', 'DELETED', 'FAILED', 'ERROR'];

export function isActivePodStatus(status) {
  const s = String(status || '').toUpperCase();
  return s && !TERMINAL_POD_STATUSES.includes(s);
}

export function pickManagedPod(pods, { volumeId, namePrefix = 'spaceage-' } = {}) {
  if (!Array.isArray(pods)) return null;
  const active = pods.filter((pod) => {
    if (!isActivePodStatus(podStatus(pod))) return false;
    const name = String(pod.name || '');
    const hasVolume = volumeId && pod.mounts?.network?.some((m) => m.volumeId === volumeId);
    return name.startsWith(namePrefix) || hasVolume;
  });
  if (!active.length) return null;
  active.sort((a, b) => new Date(b.createdAt || 0) - new Date(a.createdAt || 0));
  return active[0];
}

async function listRunPodPods() {
  const body = await runpodFetch('/pods');
  return body?.pods || body || [];
}

export async function discoverExistingPod() {
  const volumeId = process.env.RUNPOD_NETWORK_VOLUME_ID || 'v41h4fkn1b';
  const pods = await listRunPodPods();
  return pickManagedPod(pods, { volumeId });
}

async function runpodFetch(route, init = {}) {
  const key = runpodApiKey();
  if (!key) {
    throw new Error(
      `RUNPOD_API_KEY is not set (add it to ${repoEnvFilePath()} and restart game-host)`,
    );
  }
  const res = await fetch(`${API_BASE}${route}`, {
    ...init,
    headers: {
      Authorization: `Bearer ${key}`,
      'Content-Type': 'application/json',
      ...(init.headers || {}),
    },
  });
  const text = await res.text();
  let body = null;
  try {
    body = text ? JSON.parse(text) : null;
  } catch {
    body = { raw: text };
  }
  if (!res.ok) {
    const msg = body?.message || body?.error || body?.detail || text || res.statusText;
    const hint = res.status === 400 && String(msg).toLowerCase().includes('unauthorized')
      ? ' (API key may be read-only — create a RunPod key with Pod Create permission at runpod.io/console/user/settings)'
      : '';
    throw new Error(`RunPod ${res.status}: ${msg}${hint}`);
  }
  return body;
}

function ollamaProxyUrl(podId, port = 11434) {
  return `https://${podId}-${port}.proxy.runpod.net`;
}

function mapPodPhase(pod, local) {
  if (!pod) return local.stage === 'warming' ? 'preparing' : 'stopped';
  const status = podStatus(pod);
  if (['EXITED', 'TERMINATED', 'DELETED', 'FAILED', 'ERROR'].includes(status)) return 'stopped';
  if (local.ollamaReady && status === 'RUNNING') return 'running';
  if (['CREATING', 'STARTING', 'PENDING', 'INITIALIZING', 'PROVISIONING'].includes(status)) {
    return 'preparing';
  }
  if (status === 'RUNNING') {
    if (local.stage === 'warming' && isRunPodStartInProgress()) return 'preparing';
    if (!local.ollamaReady) return 'preparing';
    return 'running';
  }
  return local.phase === 'stopped' ? 'stopped' : 'preparing';
}

export async function probeOllamaReady(ollamaHost) {
  if (!ollamaHost) return false;
  try {
    const res = await fetch(`${ollamaHost}/api/tags`, {
      signal: AbortSignal.timeout(OLLAMA_PROBE_TIMEOUT_MS),
    });
    return res.ok;
  } catch {
    return false;
  }
}

function attachDiscoveredPod(local, pod) {
  const podId = extractPodId(pod);
  const status = podStatus(pod);
  const ollamaHost = ollamaProxyUrl(podId);
  const running = status === 'RUNNING';
  return saveRunPodState({
    ...local,
    phase: local.ollamaReady && running ? 'running' : 'preparing',
    stage: local.ollamaReady && running ? 'ready' : (running ? 'attached' : 'starting'),
    podId,
    ollamaHost,
    ollamaReady: !!local.ollamaReady && running,
    gpuAttempt: pod.gpu?.id || local.gpuAttempt,
    podStatus: status,
    podName: pod.name,
    model: local.model || process.env.PLAYER_AGENT_CHAT_MODEL || 'qwen3-coder:30b',
    message: running
      ? (local.ollamaReady ? 'RunPod ready' : `Existing pod ${podId} — click Start to connect`)
      : `Existing pod ${podId} (${status})`,
    logs: appendRunPodLog(local, 'info', `Discovered pod ${podId}`, { podId, status }),
  });
}

export async function syncRunPodState() {
  let local = loadRunPodState();
  if (!local.podId) {
    try {
      const pod = await discoverExistingPod();
      if (pod) {
        local = attachDiscoveredPod(local, pod);
        process.env.OLLAMA_HOST = local.ollamaHost;
        process.env.PLAYER_AGENT_ALLOW_RUNPOD = '1';
      }
    } catch (err) {
      return saveRunPodState({
        ...local,
        logs: appendRunPodLog(local, 'warn', `Pod discovery failed: ${err.message || err}`),
      });
    }
  }
  if (!local.podId) {
    return saveRunPodState({
      phase: 'stopped',
      stage: local.stage === 'failed' ? 'failed' : 'idle',
      podId: null,
      ollamaHost: null,
      ollamaReady: false,
      message: local.message || 'Not running',
      logs: local.logs || [],
    });
  }
  return refreshRunPodStatus(local);
}

export async function refreshRunPodStatus(initialLocal = null) {
  const local = initialLocal || loadRunPodState();
  if (!local.podId) {
    return syncRunPodState();
  }

  try {
    const pod = await runpodFetch(`/pods/${local.podId}`);
    let phase = mapPodPhase(pod, local);
    const ollamaHost = local.ollamaHost || ollamaProxyUrl(local.podId);
    const status = podStatus(pod);
    let stage = local.stage || 'starting';
    let message = local.message || 'RunPod preparing…';

    let ollamaReady = !!local.ollamaReady;
    let logs = local.logs || [];

    if (status === 'RUNNING' && !ollamaReady && ollamaHost && !isRunPodStartInProgress()) {
      if (await probeOllamaReady(ollamaHost)) {
        ollamaReady = true;
        stage = 'ready';
        phase = 'running';
        message = 'RunPod ready';
        logs = appendRunPodLog(local, 'success', 'Ollama reachable — pod ready', { ollamaHost });
        process.env.OLLAMA_HOST = ollamaHost;
        process.env.PLAYER_AGENT_ALLOW_RUNPOD = '1';
      }
    }

    if (ollamaReady && status === 'RUNNING') {
      stage = 'ready';
      phase = 'running';
      message = 'RunPod ready';
    } else if (isRunPodStartInProgress() && local.stage === 'warming') {
      stage = 'warming';
      message = 'Warming up model…';
    } else if (status === 'RUNNING') {
      stage = 'attached';
      message = `Pod ${local.podId} running — click Connect RunPod`;
    } else if (phase === 'preparing') {
      stage = 'starting';
      message = `Starting up — pod status ${status || 'unknown'}`;
    }

    const prevStatus = local.podStatus;
    if (status && status !== prevStatus) {
      logs = appendRunPodLog({ ...local, logs }, 'info', `Pod status: ${status}`, { podId: local.podId });
    }

    return saveRunPodState({
      ...local,
      phase,
      stage,
      ollamaHost,
      ollamaReady,
      podName: pod.name,
      podStatus: status,
      message,
      logs,
    });
  } catch (err) {
    const keepPod = !!local.podId;
    return saveRunPodState({
      ...local,
      phase: keepPod ? 'preparing' : 'stopped',
      stage: 'failed',
      podId: keepPod ? local.podId : null,
      ollamaHost: keepPod ? local.ollamaHost : null,
      message: String(err.message || err),
      ollamaReady: false,
      logs: appendRunPodLog(local, 'error', String(err.message || err)),
    });
  }
}

async function reconnectToPod(podOrId) {
  const podId = typeof podOrId === 'string' ? podOrId : extractPodId(podOrId);
  const pod = typeof podOrId === 'string' ? await runpodFetch(`/pods/${podId}`) : podOrId;
  const ollamaHost = ollamaProxyUrl(podId);
  const status = podStatus(pod);

  if (status === 'RUNNING' && await probeOllamaReady(ollamaHost)) {
    saveRunPodState({
      ...loadRunPodState(),
      phase: 'running',
      stage: 'ready',
      podId,
      ollamaHost,
      ollamaReady: true,
      gpuAttempt: pod.gpu?.id || loadRunPodState().gpuAttempt,
      podStatus: status,
      podName: pod.name,
      model: process.env.PLAYER_AGENT_CHAT_MODEL || 'qwen3-coder:30b',
      message: 'RunPod ready',
      logs: appendRunPodLog(loadRunPodState(), 'success', `Connected to pod ${podId}`, { podId }),
    });
    process.env.OLLAMA_HOST = ollamaHost;
    process.env.PLAYER_AGENT_ALLOW_RUNPOD = '1';
    return refreshRunPodStatus();
  }

  saveRunPodState({
    ...loadRunPodState(),
    phase: 'preparing',
    stage: status === 'RUNNING' ? 'warming' : 'starting',
    podId,
    ollamaHost,
    ollamaReady: false,
    gpuAttempt: pod.gpu?.id || loadRunPodState().gpuAttempt,
    podStatus: status,
    podName: pod.name,
    model: process.env.PLAYER_AGENT_CHAT_MODEL || 'qwen3-coder:30b',
    message: status === 'RUNNING' ? 'Connecting to existing pod…' : `Waiting for pod (${status})…`,
    logs: appendRunPodLog(loadRunPodState(), 'info', `Reconnecting to pod ${podId}`, { podId, status }),
  });

  process.env.OLLAMA_HOST = ollamaHost;
  process.env.PLAYER_AGENT_ALLOW_RUNPOD = '1';

  if (status !== 'RUNNING') {
    await pollUntilRunning(podId);
  } else {
    saveRunPodState({
      ...loadRunPodState(),
      stage: 'warming',
      message: 'Warming up model…',
    });
  }

  await warmupAndMarkReady(ollamaHost);
  return refreshRunPodStatus();
}

async function createPodWithGpuAlternation(bodyBase) {
  const gpus = gpuAlternationList(process.env.RUNPOD_GPU_ID);
  let gpuIndex = 0;

  for (let attempt = 1; attempt <= MAX_GPU_ATTEMPTS; attempt += 1) {
    const gpuId = gpus[gpuIndex % gpus.length];
    gpuIndex += 1;

    const current = loadRunPodState();
    saveRunPodState({
      ...current,
      stage: 'starting',
      gpuAttempt: gpuId,
      message: gpus.length > 1
        ? `Starting up — trying ${gpuId} (attempt ${attempt})…`
        : `Starting up — trying ${gpuId}…`,
      logs: appendRunPodLog(
        current,
        'info',
        `POST /pods — GPU ${gpuId}`,
        { gpuId, attempt, dataCenter: bodyBase.dataCenterIds?.[0] },
      ),
    });

    try {
      const pod = await runpodFetch('/pods', {
        method: 'POST',
        body: JSON.stringify({ ...bodyBase, gpu: { id: gpuId, count: 1 } }),
      });
      logState('success', `Pod created on ${gpuId}`, {
        podId: pod?.id || pod?.podId,
        gpuId,
      });
      return { pod, gpuId };
    } catch (err) {
      const message = String(err.message || err);
      logState('error', message, { gpuId, attempt });

      if (isRunPodAuthError(err)) {
        throw err;
      }
      if (!isGpuCapacityError(err)) {
        throw err;
      }

      const nextGpu = gpus[gpuIndex % gpus.length];
      logState(
        'warn',
        gpus.length > 1
          ? `No GPU for ${gpuId}; retrying with ${nextGpu} in ${GPU_RETRY_MS / 1000}s…`
          : `No GPU for ${gpuId}; retrying in ${GPU_RETRY_MS / 1000}s…`,
        { nextGpu, waitMs: GPU_RETRY_MS },
      );
      await new Promise((r) => setTimeout(r, GPU_RETRY_MS));
    }
  }

  throw new Error(`No GPU available after ${MAX_GPU_ATTEMPTS} attempts (${gpus.join(' / ')})`);
}

async function createNewRunPodJob() {
  const templateId = process.env.RUNPOD_TEMPLATE_ID || 'e2wsrsjbjq';
  const volumeId = process.env.RUNPOD_NETWORK_VOLUME_ID || 'v41h4fkn1b';
  const dataCenter = process.env.RUNPOD_DATA_CENTER || 'EU-RO-1';

  saveRunPodState({
    ...loadRunPodState(),
    podId: null,
    ollamaHost: null,
    ollamaReady: false,
    stage: 'starting',
    message: 'Creating new pod…',
  });
  logState('info', 'No existing pod found — creating a new one');

  const bodyBase = {
    name: `spaceage-${Date.now()}`,
    templateId,
    cloud: process.env.RUNPOD_CLOUD || 'SECURE',
    dataCenterIds: [dataCenter],
    mounts: {
      network: [{ volumeId, path: process.env.RUNPOD_VOLUME_MOUNT || '/runpod-volume' }],
    },
  };

  const { pod, gpuId } = await createPodWithGpuAlternation(bodyBase);
  const podId = extractPodId(pod);
  const ollamaHost = ollamaProxyUrl(podId);
  saveRunPodState({
    ...loadRunPodState(),
    phase: 'preparing',
    stage: 'starting',
    podId,
    ollamaHost,
    ollamaReady: false,
    gpuAttempt: gpuId,
    message: 'Pod created; waiting for RUNNING…',
    templateId,
    volumeId,
    model: process.env.PLAYER_AGENT_CHAT_MODEL || 'qwen3-coder:30b',
    logs: appendRunPodLog(loadRunPodState(), 'success', `Pod ${podId} provisioning`, { podId, gpuId }),
  });

  process.env.OLLAMA_HOST = ollamaHost;
  process.env.PLAYER_AGENT_ALLOW_RUNPOD = '1';

  await pollUntilRunning(podId);
  await warmupAndMarkReady(ollamaHost);
  return refreshRunPodStatus();
}

function recordStartFailure(err) {
  const current = loadRunPodState();
  const keepPod = !!current.podId;
  saveRunPodState({
    ...current,
    phase: keepPod ? 'preparing' : 'stopped',
    stage: 'failed',
    podId: keepPod ? current.podId : null,
    ollamaHost: keepPod ? current.ollamaHost : null,
    ollamaReady: false,
    message: String(err.message || err),
    logs: appendRunPodLog(current, 'error', String(err.message || err)),
  });
}

async function startRunPodJob() {
  logState('info', 'RunPod start requested');
  saveRunPodState({
    ...loadRunPodState(),
    phase: 'preparing',
    stage: 'starting',
    ollamaReady: false,
    message: 'Starting up…',
  });

  const local = loadRunPodState();
  if (local.podId) {
    try {
      const pod = await runpodFetch(`/pods/${local.podId}`);
      if (isActivePodStatus(podStatus(pod))) {
        return reconnectToPod(pod);
      }
    } catch {
      // tracked pod gone — discover or create
    }
  }

  const discovered = await discoverExistingPod();
  if (discovered) {
    return reconnectToPod(discovered);
  }

  return createNewRunPodJob();
}

export function startRunPodBackground() {
  if (startJob) return startJob;

  startJob = startRunPodJob()
    .catch((err) => {
      recordStartFailure(err);
      throw err;
    })
    .finally(() => {
      startJob = null;
    });

  return startJob;
}

/** @deprecated prefer startRunPodBackground for HTTP handlers */
export async function startRunPod() {
  return startRunPodBackground();
}

async function pollUntilRunning(podId, attempts = 72, delayMs = 5000) {
  let lastStatus = '';
  for (let i = 0; i < attempts; i += 1) {
    const pod = await runpodFetch(`/pods/${podId}`);
    const status = podStatus(pod);
    if (status !== lastStatus) {
      logState('info', `Pod status: ${status}`, { podId });
      lastStatus = status;
    }
    if (status === 'RUNNING') {
      saveRunPodState({
        ...loadRunPodState(),
        phase: 'preparing',
        stage: 'warming',
        message: 'Warming up model…',
        podStatus: status,
      });
      return;
    }
    if (['EXITED', 'TERMINATED', 'FAILED', 'DELETED', 'ERROR'].includes(status)) {
      throw new Error(`RunPod entered ${status}`);
    }
    await new Promise((r) => setTimeout(r, delayMs));
  }
  throw new Error('RunPod did not reach RUNNING in time (6 min)');
}

async function warmupOllamaHttp(ollamaHost, {
  attempts = OLLAMA_WARMUP_ATTEMPTS,
  delayMs = OLLAMA_WARMUP_DELAY_MS,
} = {}) {
  for (let i = 0; i < attempts; i += 1) {
    if (await probeOllamaReady(ollamaHost)) {
      logState('success', 'Ollama responding on pod');
      return;
    }
    logState('info', `Waiting for Ollama… (${i + 1}/${attempts})`, { ollamaHost });
    await new Promise((r) => setTimeout(r, delayMs));
  }
  throw new Error(`Ollama not reachable on pod proxy after ${attempts} attempts`);
}

async function warmupAndMarkReady(ollamaHost) {
  saveRunPodState({
    ...loadRunPodState(),
    phase: 'preparing',
    stage: 'warming',
    message: 'Warming up model…',
    logs: appendRunPodLog(loadRunPodState(), 'info', 'Warming up Ollama model…', { ollamaHost }),
  });

  await warmupOllamaHttp(ollamaHost);

  if (process.env.PLAYER_AGENT_BUDGET_USD) {
    try {
      await warmupOllama(ollamaHost);
    } catch (err) {
      logState('warn', `Model smoke skipped: ${err.message || err}`);
    }
  } else {
    logState('info', 'Set PLAYER_AGENT_BUDGET_USD in .env for full player-agent smoke warmup');
  }

  saveRunPodState({
    ...loadRunPodState(),
    phase: 'running',
    stage: 'ready',
    ollamaReady: true,
    message: 'RunPod ready',
    logs: appendRunPodLog(loadRunPodState(), 'success', 'RunPod ready — Ollama warmed up'),
  });
  process.env.OLLAMA_HOST = ollamaHost;
}

export async function stopRunPod() {
  if (startJob) {
    saveRunPodState({
      phase: 'stopped',
      stage: 'idle',
      podId: null,
      ollamaHost: null,
      ollamaReady: false,
      message: 'Start cancelled',
      logs: appendRunPodLog(loadRunPodState(), 'warn', 'Start cancelled'),
    });
  }

  const local = loadRunPodState();
  if (!local.podId) {
    return saveRunPodState({
      phase: 'stopped',
      stage: 'idle',
      podId: null,
      ollamaHost: null,
      ollamaReady: false,
      message: 'Not running',
      logs: local.logs || [],
    });
  }

  try {
    logState('info', `DELETE /pods/${local.podId}`);
    await runpodFetch(`/pods/${local.podId}`, { method: 'DELETE' });
    logState('success', 'RunPod terminated');
  } catch (err) {
    saveRunPodState({
      phase: 'stopped',
      stage: 'idle',
      podId: null,
      ollamaHost: null,
      ollamaReady: false,
      message: `Terminated locally (${err.message || err})`,
      logs: appendRunPodLog(local, 'warn', String(err.message || err)),
    });
    return loadRunPodState();
  }

  return saveRunPodState({
    phase: 'stopped',
    stage: 'idle',
    podId: null,
    ollamaHost: null,
    ollamaReady: false,
    message: 'RunPod terminated',
    logs: appendRunPodLog(loadRunPodState(), 'info', 'RunPod stopped'),
  });
}

export function startRunPodStatusPolling(intervalMs = 30_000) {
  syncRunPodState().catch(() => {});
  return setInterval(() => {
    syncRunPodState().catch(() => {});
  }, intervalMs);
}

export function currentOllamaHost() {
  const state = loadRunPodState();
  if (state.phase === 'running' && state.ollamaHost) return state.ollamaHost;
  return process.env.OLLAMA_HOST || null;
}

export function isRunPodStartInProgress() {
  return !!startJob;
}

import { repoEnvFilePath, runpodApiKey } from '../lib/load-env.mjs';
import { json } from '../lib/http.mjs';
import { requireSession } from '../lib/auth.mjs';
import { runPodStatusPayload } from '../lib/ai-session.mjs';
import {
  isRunPodStartInProgress,
  startRunPodBackground,
  stopRunPod,
  syncRunPodState,
} from '../lib/runpod.mjs';

export async function handleRunPod(req, res, url) {
  if (req.method === 'GET' && url.pathname === '/api/session/runpod/status') {
    const session = requireSession(req, res);
    if (!session) return true;
    try {
      await syncRunPodState();
      json(res, 200, runPodStatusPayload());
    } catch (err) {
      json(res, 200, { ...runPodStatusPayload(), message: String(err.message || err) });
    }
    return true;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/runpod/start') {
    const session = requireSession(req, res);
    if (!session) return true;
    if (!runpodApiKey()) {
      json(res, 500, {
        error: `RUNPOD_API_KEY is not set. Add it to ${repoEnvFilePath()} and restart game-host.`,
        ...runPodStatusPayload(),
      });
      return true;
    }
    try {
      if (!isRunPodStartInProgress()) {
        startRunPodBackground().catch((err) => {
          console.error('[runpod] start failed:', err.message || err);
        });
      }
      json(res, 202, {
        ...runPodStatusPayload(),
        message: 'RunPod start requested — poll status for progress',
      });
    } catch (err) {
      json(res, 500, { error: String(err.message || err), ...runPodStatusPayload() });
    }
    return true;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/runpod/stop') {
    const session = requireSession(req, res);
    if (!session) return true;
    try {
      await stopRunPod();
      json(res, 200, runPodStatusPayload());
    } catch (err) {
      json(res, 500, { error: String(err.message || err) });
    }
    return true;
  }

  return false;
}

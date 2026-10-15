import { requireGm } from '../lib/auth.mjs';
import {
  bootstrapFromCampaign,
  bootstrapFromPlayRun,
  promoteGameout,
  runReports,
  runTurn,
} from '../lib/game-exe.mjs';
import { json, readBody } from '../lib/http.mjs';
import { isolateReports } from '../lib/isolate.mjs';
import { readTurnFromGamein } from '../lib/paths.mjs';
import { buildStatusJson, syncLobbyStatusFile } from '../lib/status.mjs';

export async function handleGm(req, res, url) {
  if (req.method === 'GET' && url.pathname === '/api/gm/status') {
    if (!requireGm(req, res)) return true;
    json(res, 200, buildStatusJson());
    return true;
  }

  if (req.method === 'POST' && url.pathname === '/api/gm/sync-lobby-status') {
    if (!requireGm(req, res)) return true;
    try {
      syncLobbyStatusFile();
    } catch (err) {
      json(res, 500, { error: String(err.message || err) });
      return true;
    }
    json(res, 200, buildStatusJson());
    return true;
  }

  if (req.method === 'POST' && url.pathname === '/api/gm/init') {
    if (!requireGm(req, res)) return true;
    let body = {};
    try {
      const raw = await readBody(req);
      if (raw.trim()) body = JSON.parse(raw);
    } catch { /* empty body ok */ }
    if (body.source === 'play' && body.runId) {
      bootstrapFromPlayRun(body.runId);
    } else {
      await bootstrapFromCampaign();
    }
    syncLobbyStatusFile();
    json(res, 200, { ok: true, turn: readTurnFromGamein() });
    return true;
  }

  if (req.method === 'POST' && url.pathname === '/api/gm/reports') {
    if (!requireGm(req, res)) return true;
    await runReports();
    isolateReports();
    syncLobbyStatusFile();
    json(res, 200, { ok: true, turn: readTurnFromGamein() });
    return true;
  }

  if (req.method === 'POST' && url.pathname === '/api/gm/turn') {
    if (!requireGm(req, res)) return true;
    const before = readTurnFromGamein();
    await runTurn();
    promoteGameout(before + 1);
    isolateReports();
    syncLobbyStatusFile();
    json(res, 200, { ok: true, turn: readTurnFromGamein() });
    return true;
  }

  if (req.method === 'POST' && url.pathname === '/api/gm/isolate') {
    if (!requireGm(req, res)) return true;
    isolateReports();
    syncLobbyStatusFile();
    json(res, 200, { ok: true });
    return true;
  }

  return false;
}

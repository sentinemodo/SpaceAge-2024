import './lib/env-bootstrap.mjs';
import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { repoEnvFilePath, runpodApiKey } from './lib/load-env.mjs';
import {
  ensureRunLayout,
  gameinPath,
  listRuns,
  listTurns,
  latestTurn,
  readTurnFromGamein,
  reportPaths,
  repoRoot,
  runId,
} from './lib/paths.mjs';
import {
  login,
  logout,
  loadFactionCredentials,
  listFactions,
  getFaction,
  effectiveFactionId,
  factionDisplayName,
  requireGm,
  requireSession,
  sessionFromRequest,
  sessionTokenFromRequest,
} from './lib/auth.mjs';
import { bootstrapFromCampaign, promoteGameout, runReports, runTurn } from './lib/game-exe.mjs';
import { buildStatusJson, syncLobbyStatusFile } from './lib/status.mjs';
import { validateOrderText } from './lib/check-orders.mjs';
import { parseOrders } from './lib/parse-orders.mjs';
import { runBattleSimulation } from './lib/battle-sim.mjs';
import { splitReportSections } from './lib/report-sections.mjs';
import { listOrderVersions, readSubmittedOrder, saveOrder } from './lib/orders-io.mjs';
import { isolateReports } from './lib/isolate.mjs';
import {
  executeAiQuery,
  regenerateStory,
  runPodStatusPayload,
} from './lib/ai-session.mjs';
import {
  listPersonas,
  readPersona,
  readStory,
  savePersona,
  saveStory,
} from './lib/persona-story.mjs';
import {
  syncRunPodState,
  startRunPodBackground,
  startRunPodStatusPolling,
  stopRunPod,
  isRunPodStartInProgress,
} from './lib/runpod.mjs';

const PORT = parseInt(process.env.GAME_HOST_PORT || '8787', 10);
const VISUAL_DIST = path.join(repoRoot(), 'tools', 'visual-tool', 'dist');

ensureRunLayout();

function playerVisibleRunId() {
  const marked = listRuns().find((run) => run.playerVisible);
  return marked?.id || runId();
}

function sessionRunId(session) {
  if (!session?.admin) return playerVisibleRunId();
  return session.viewRunId || runId();
}

function runsForSession(session) {
  const runs = listRuns();
  if (session.admin) return runs;
  const visible = runs.filter((run) => run.playerVisible);
  if (visible.length) return visible;
  const fallback = runs.find((run) => run.id === runId());
  return fallback ? [fallback] : [];
}

function sessionTurn(session) {
  return session.viewTurn ?? null;
}

function reportPathsForSession(session, factionId) {
  return reportPaths(factionId, sessionRunId(session), sessionTurn(session));
}

function orderContextForSession(session) {
  const run = sessionRunId(session);
  const factionId = effectiveFactionId(session);
  const { turn } = reportPaths(factionId, run, sessionTurn(session));
  return { run, factionId, turn };
}

function readBody(req) {
  return new Promise((resolve, reject) => {
    const chunks = [];
    req.on('data', (c) => chunks.push(c));
    req.on('end', () => resolve(Buffer.concat(chunks).toString('utf8')));
    req.on('error', reject);
  });
}

function json(res, status, obj) {
  res.writeHead(status, { 'Content-Type': 'application/json; charset=utf-8' });
  res.end(JSON.stringify(obj));
}

function cors(res) {
  res.setHeader('Access-Control-Allow-Origin', process.env.GAME_HOST_CORS || '*');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization, X-GM-Key');
  res.setHeader('Access-Control-Allow-Methods', 'GET, POST, PUT, OPTIONS');
}

function serveStatic(req, res, urlPath) {
  if (!fs.existsSync(VISUAL_DIST)) {
    return false;
  }
  let file = urlPath === '/' || urlPath === '/client' ? '/index.html' : urlPath;
  const full = path.join(VISUAL_DIST, file.replace(/^\//, ''));
  if (!full.startsWith(VISUAL_DIST)) {
    return false;
  }
  if (fs.existsSync(full) && fs.statSync(full).isFile()) {
    const ext = path.extname(full);
    const types = { '.html': 'text/html', '.js': 'application/javascript', '.css': 'text/css', '.svg': 'image/svg+xml' };
    res.writeHead(200, { 'Content-Type': types[ext] || 'application/octet-stream' });
    fs.createReadStream(full).pipe(res);
    return true;
  }
  if (!path.extname(full)) {
    const index = path.join(VISUAL_DIST, 'index.html');
    if (fs.existsSync(index)) {
      res.writeHead(200, { 'Content-Type': 'text/html' });
      fs.createReadStream(index).pipe(res);
      return true;
    }
  }
  return false;
}

const server = http.createServer(async (req, res) => {
  cors(res);
  if (req.method === 'OPTIONS') {
    res.writeHead(204);
    res.end();
    return;
  }

  const url = new URL(req.url || '/', `http://${req.headers.host}`);

  if (url.pathname.startsWith('/client') || url.pathname.startsWith('/assets')) {
    const p = url.pathname.replace(/^\/client/, '') || '/';
    if (serveStatic(req, res, p)) return;
  }

  if (req.method === 'GET' && url.pathname === '/health') {
    json(res, 200, {
      ok: true,
      runId: runId(),
      features: { adminBrowse: true, viewAsFaction: true, ai: true, runpod: true },
    });
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/auth/login') {
    const body = JSON.parse(await readBody(req));
    const gmKey = req.headers['x-gm-key'] || body.gmKey || '';
    const token = login(parseInt(body.factionId, 10), body.password || '', gmKey);
    if (!token) {
      json(res, 401, { error: 'invalid credentials' });
      return;
    }
    const session = sessionFromRequest({ headers: { authorization: `Bearer ${token}` } });
    json(res, 200, {
      token,
      factionId: session.factionId,
      viewAsFactionId: session.viewAsFactionId,
      name: session.name,
      admin: !!session.admin,
    });
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/auth/logout') {
    const token = sessionTokenFromRequest(req);
    if (token) logout(token);
    json(res, 200, { ok: true });
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/meta') {
    const session = requireSession(req, res);
    if (!session) return;
    const rid = sessionRunId(session);
    const resolvedTurn = sessionTurn(session) ?? latestTurn(rid);
    json(res, 200, {
      factionId: session.factionId,
      viewAsFactionId: effectiveFactionId(session),
      name: factionDisplayName(session),
      admin: !!session.admin,
      factions: session.admin ? listFactions() : undefined,
      runId: rid,
      viewRunId: session.viewRunId || rid,
      viewTurn: resolvedTurn,
      runs: runsForSession(session),
      turns: listTurns(rid),
      turn: fs.existsSync(gameinPath(rid)) ? readTurnFromGamein(rid) : resolvedTurn,
      engineVersion: '0.1.159',
    });
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/runs') {
    const session = requireSession(req, res);
    if (!session) return;
    json(res, 200, { runs: runsForSession(session) });
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/turns') {
    const session = requireSession(req, res);
    if (!session) return;
    const requested = url.searchParams.get('runId');
    const rid = session.admin && requested ? requested : sessionRunId(session);
    json(res, 200, { runId: rid, turns: listTurns(rid) });
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/context') {
    const session = requireSession(req, res);
    if (!session) return;
    const body = JSON.parse(await readBody(req));
    if (body.runId) {
      if (!session.admin) {
        json(res, 403, { error: 'admin only' });
        return;
      }
      session.viewRunId = String(body.runId);
    }
    if (body.turn != null) session.viewTurn = parseInt(body.turn, 10);
    const rid = sessionRunId(session);
    json(res, 200, {
      runId: rid,
      viewTurn: sessionTurn(session) ?? latestTurn(rid),
      turns: listTurns(rid),
    });
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/view-as') {
    const session = requireSession(req, res);
    if (!session) return;
    if (!session.admin) {
      json(res, 403, { error: 'admin only' });
      return;
    }
    const body = JSON.parse(await readBody(req));
    const viewAs = parseInt(body.factionId, 10);
    const row = getFaction(viewAs);
    if (!row) {
      json(res, 400, { error: 'unknown faction' });
      return;
    }
    session.viewAsFactionId = viewAs;
    json(res, 200, { viewAsFactionId: viewAs, name: row.name });
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/report.xml') {
    const session = requireSession(req, res);
    if (!session) return;
    const { xmlPath } = reportPathsForSession(session, effectiveFactionId(session));
    const pick = fs.existsSync(xmlPath) ? xmlPath : null;
    if (!pick) {
      json(res, 404, { error: 'report xml not found; GM may need to run /reports' });
      return;
    }
    res.writeHead(200, { 'Content-Type': 'application/xml; charset=utf-8' });
    fs.createReadStream(pick).pipe(res);
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/static/basic-technologies') {
    const session = requireSession(req, res);
    if (!session) return;
    const mdPath = path.join(repoRoot(), 'player', 'basic_technologies.md');
    if (!fs.existsSync(mdPath)) {
      json(res, 404, { error: 'basic_technologies.md not found' });
      return;
    }
    res.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
    fs.createReadStream(mdPath).pipe(res);
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/report.txt') {
    const session = requireSession(req, res);
    if (!session) return;
    const { txtPath, isoTxt } = reportPathsForSession(session, effectiveFactionId(session));
    const pick = fs.existsSync(txtPath) ? txtPath : isoTxt;
    if (!fs.existsSync(pick)) {
      json(res, 404, { error: 'report not found' });
      return;
    }
    res.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
    fs.createReadStream(pick).pipe(res);
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/report-sections') {
    const session = requireSession(req, res);
    if (!session) return;
    const factionId = effectiveFactionId(session);
    const { txtPath, isoTxt } = reportPathsForSession(session, factionId);
    const pick = fs.existsSync(txtPath) ? txtPath : isoTxt;
    if (!fs.existsSync(pick)) {
      json(res, 404, { error: 'report not found' });
      return;
    }
    const text = fs.readFileSync(pick, 'utf8');
    json(res, 200, { sections: splitReportSections(text) });
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/orders') {
    const session = requireSession(req, res);
    if (!session) return;
    const { run, factionId, turn } = orderContextForSession(session);
    const text = readSubmittedOrder(factionId, turn, run);
    json(res, 200, { submitted: text != null, text: text ?? '', turn, version: listOrderVersions(factionId, turn, run).at(-1) ?? null });
    return;
  }

  if (req.method === 'PUT' && url.pathname === '/api/session/orders') {
    const session = requireSession(req, res);
    if (!session) return;
    const body = await readBody(req);
    const { run, factionId, turn } = orderContextForSession(session);
    const version = saveOrder(factionId, body, turn, run);
    try {
      syncLobbyStatusFile();
    } catch (err) {
      console.warn('Lobby status sync failed:', err);
    }
    json(res, 200, { ok: true, turn, version });
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/check-orders') {
    const session = requireSession(req, res);
    if (!session) return;
    const body = JSON.parse(await readBody(req));
    const creds = loadFactionCredentials().get(session.factionId);
    const warnings = validateOrderText(body.text || '', session.factionId, creds?.password || '');
    json(res, 200, { warnings });
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/parse-orders') {
    const session = requireSession(req, res);
    if (!session) return;
    const body = JSON.parse(await readBody(req));
    const creds = loadFactionCredentials().get(session.factionId);
    const result = await parseOrders(body.text || '', session.factionId, creds?.password || '');
    json(res, 200, result);
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/runpod/status') {
    const session = requireSession(req, res);
    if (!session) return;
    try {
      await syncRunPodState();
      json(res, 200, runPodStatusPayload());
    } catch (err) {
      json(res, 200, { ...runPodStatusPayload(), message: String(err.message || err) });
    }
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/runpod/start') {
    const session = requireSession(req, res);
    if (!session) return;
    if (!runpodApiKey()) {
      json(res, 500, {
        error: `RUNPOD_API_KEY is not set. Add it to ${repoEnvFilePath()} and restart game-host.`,
        ...runPodStatusPayload(),
      });
      return;
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
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/runpod/stop') {
    const session = requireSession(req, res);
    if (!session) return;
    try {
      await stopRunPod();
      json(res, 200, runPodStatusPayload());
    } catch (err) {
      json(res, 500, { error: String(err.message || err) });
    }
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/personas') {
    const session = requireSession(req, res);
    if (!session) return;
    json(res, 200, { personas: listPersonas(sessionRunId(session)) });
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/persona') {
    const session = requireSession(req, res);
    if (!session) return;
    const personaId = url.searchParams.get('id');
    if (!personaId) {
      json(res, 400, { error: 'missing id' });
      return;
    }
    try {
      json(res, 200, { personaId, text: readPersona(sessionRunId(session), personaId) });
    } catch (err) {
      json(res, 400, { error: String(err.message || err) });
    }
    return;
  }

  if (req.method === 'PUT' && url.pathname === '/api/session/persona') {
    const session = requireSession(req, res);
    if (!session) return;
    if (!session.admin) {
      json(res, 403, { error: 'admin/gm only' });
      return;
    }
    const body = JSON.parse(await readBody(req));
    if (!body.personaId || body.text == null) {
      json(res, 400, { error: 'personaId and text required' });
      return;
    }
    try {
      savePersona(sessionRunId(session), body.personaId, body.text);
      json(res, 200, { ok: true });
    } catch (err) {
      json(res, 400, { error: String(err.message || err) });
    }
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/story') {
    const session = requireSession(req, res);
    if (!session) return;
    const text = readStory(sessionRunId(session), effectiveFactionId(session));
    json(res, 200, { text });
    return;
  }

  if (req.method === 'PUT' && url.pathname === '/api/session/story') {
    const session = requireSession(req, res);
    if (!session) return;
    const body = await readBody(req);
    saveStory(sessionRunId(session), effectiveFactionId(session), body);
    json(res, 200, { ok: true });
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/ai/query') {
    const session = requireSession(req, res);
    if (!session) return;
    const body = JSON.parse(await readBody(req));
    if (!body.prompt?.trim()) {
      json(res, 400, { error: 'missing prompt' });
      return;
    }
    try {
      const result = await executeAiQuery({
        runId: sessionRunId(session),
        factionId: effectiveFactionId(session),
        prompt: body.prompt.trim(),
        includeStory: !!body.includeStory,
      });
      json(res, 200, result);
    } catch (err) {
      json(res, 500, { ok: false, error: String(err.message || err) });
    }
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/ai/draft-story') {
    const session = requireSession(req, res);
    if (!session) return;
    const body = JSON.parse(await readBody(req));
    if (!body.personaId) {
      json(res, 400, { error: 'missing personaId' });
      return;
    }
    try {
      const result = await regenerateStory({
        runId: sessionRunId(session),
        factionId: effectiveFactionId(session),
        personaId: body.personaId,
      });
      json(res, 200, result);
    } catch (err) {
      json(res, 500, { ok: false, error: String(err.message || err) });
    }
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/battle-sim') {
    const session = requireSession(req, res);
    if (!session) return;
    const body = JSON.parse(await readBody(req));
    if (!body.xml) {
      json(res, 400, { error: 'missing xml' });
      return;
    }
    try {
      const result = await runBattleSimulation(body.xml, body.seed);
      json(res, 200, result);
    } catch (err) {
      json(res, 500, { error: String(err.message || err) });
    }
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/gm/status') {
    if (!requireGm(req, res)) return;
    json(res, 200, buildStatusJson());
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/gm/init') {
    if (!requireGm(req, res)) return;
    let body = {};
    try {
      const raw = await readBody(req);
      if (raw.trim()) body = JSON.parse(raw);
    } catch { /* empty body ok */ }
    if (body.source === 'play' && body.runId) {
      const { bootstrapFromPlayRun } = await import('./lib/game-exe.mjs');
      bootstrapFromPlayRun(body.runId);
    } else {
      await bootstrapFromCampaign();
    }
    syncLobbyStatusFile();
    json(res, 200, { ok: true, turn: readTurnFromGamein() });
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/gm/reports') {
    if (!requireGm(req, res)) return;
    await runReports();
    isolateReports();
    syncLobbyStatusFile();
    json(res, 200, { ok: true, turn: readTurnFromGamein() });
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/gm/turn') {
    if (!requireGm(req, res)) return;
    const before = readTurnFromGamein();
    await runTurn();
    promoteGameout(before + 1);
    isolateReports();
    syncLobbyStatusFile();
    json(res, 200, { ok: true, turn: readTurnFromGamein() });
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/gm/isolate') {
    if (!requireGm(req, res)) return;
    isolateReports();
    syncLobbyStatusFile();
    json(res, 200, { ok: true });
    return;
  }

  json(res, 404, { error: 'not found' });
});

async function reportPortConflict() {
  try {
    const res = await fetch(`http://127.0.0.1:${PORT}/health`);
    if (res.ok) {
      console.log(`Game-host already running on http://localhost:${PORT}`);
      console.log(`Visual tool (if built): http://localhost:${PORT}/client/`);
      process.exit(0);
    }
  } catch {
    /* not our server */
  }
  console.error(`Port ${PORT} is already in use.`);
  console.error(`  Windows: netstat -ano | findstr :${PORT}`);
  console.error(`  Then:    taskkill /PID <pid> /F`);
  console.error(`Or set GAME_HOST_PORT to use a different port.`);
  process.exit(1);
}

server.on('error', (err) => {
  if (err.code === 'EADDRINUSE') {
    reportPortConflict();
    return;
  }
  throw err;
});

server.listen(PORT, () => {
  console.log(`SpaceAge game-host run=${runId()} http://localhost:${PORT}`);
  console.log(`Visual tool (if built): http://localhost:${PORT}/client/`);
  startRunPodStatusPolling(parseInt(process.env.RUNPOD_STATUS_POLL_MS || '30000', 10));
});

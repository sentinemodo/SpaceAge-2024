import fs from 'node:fs';
import path from 'node:path';
import { json, readBody } from '../lib/http.mjs';
import {
  gameinPath,
  latestTurn,
  listTurns,
  readTurnFromGamein,
  repoRoot,
} from '../lib/paths.mjs';
import {
  effectiveFactionId,
  factionDisplayName,
  getFaction,
  isPlayerFaction,
  listFactions,
  loadFactionCredentials,
  login,
  logout,
  requireAdmin,
  requireSession,
  sessionFromRequest,
  sessionTokenFromRequest,
} from '../lib/auth.mjs';
import { validateOrderText } from '../lib/check-orders.mjs';
import { parseOrders } from '../lib/parse-orders.mjs';
import { runBattleSimulation } from '../lib/battle-sim.mjs';
import { splitReportSections } from '../lib/report-sections.mjs';
import { listOrderVersions, readSubmittedOrder, saveOrder } from '../lib/orders-io.mjs';
import { buildStatusJson, syncLobbyStatusFile } from '../lib/status.mjs';
import {
  orderContextForFactionId,
  orderContextForSession,
  parseFactionIdParam,
  reportPathsForSession,
  runsForSession,
  sessionRunId,
  sessionTurn,
} from '../lib/session-view.mjs';

export async function handleSession(req, res, url) {
  if (req.method === 'POST' && url.pathname === '/api/auth/login') {
    const body = JSON.parse(await readBody(req));
    const gmKey = req.headers['x-gm-key'] || body.gmKey || '';
    const token = login(parseInt(body.factionId, 10), body.password || '', gmKey);
    if (!token) {
      json(res, 401, { error: 'invalid credentials' });
      return true;
    }
    const session = sessionFromRequest({ headers: { authorization: `Bearer ${token}` } });
    json(res, 200, {
      token,
      factionId: session.factionId,
      viewAsFactionId: session.viewAsFactionId,
      name: session.name,
      admin: !!session.admin,
    });
    return true;
  }

  if (req.method === 'POST' && url.pathname === '/api/auth/logout') {
    const token = sessionTokenFromRequest(req);
    if (token) logout(token);
    json(res, 200, { ok: true });
    return true;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/meta') {
    const session = requireSession(req, res);
    if (!session) return true;
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
      engineVersion: '0.8.002',
    });
    return true;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/runs') {
    const session = requireSession(req, res);
    if (!session) return true;
    json(res, 200, { runs: runsForSession(session) });
    return true;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/turns') {
    const session = requireSession(req, res);
    if (!session) return true;
    const requested = url.searchParams.get('runId');
    const rid = session.admin && requested ? requested : sessionRunId(session);
    json(res, 200, { runId: rid, turns: listTurns(rid) });
    return true;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/context') {
    const session = requireSession(req, res);
    if (!session) return true;
    const body = JSON.parse(await readBody(req));
    if (body.runId) {
      if (!session.admin) {
        json(res, 403, { error: 'admin only' });
        return true;
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
    return true;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/view-as') {
    const session = requireSession(req, res);
    if (!session) return true;
    if (!session.admin) {
      json(res, 403, { error: 'admin only' });
      return true;
    }
    const body = JSON.parse(await readBody(req));
    const viewAs = parseInt(body.factionId, 10);
    const row = getFaction(viewAs);
    if (!row) {
      json(res, 400, { error: 'unknown faction' });
      return true;
    }
    session.viewAsFactionId = viewAs;
    json(res, 200, { viewAsFactionId: viewAs, name: row.name });
    return true;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/report.xml') {
    const session = requireSession(req, res);
    if (!session) return true;
    const { xmlPath } = reportPathsForSession(session, effectiveFactionId(session));
    const pick = fs.existsSync(xmlPath) ? xmlPath : null;
    if (!pick) {
      json(res, 404, { error: 'report xml not found; GM may need to run /reports' });
      return true;
    }
    res.writeHead(200, { 'Content-Type': 'application/xml; charset=utf-8' });
    fs.createReadStream(pick).pipe(res);
    return true;
  }

  if (req.method === 'GET' && url.pathname === '/api/static/basic-technologies') {
    const session = requireSession(req, res);
    if (!session) return true;
    const mdPath = path.join(repoRoot(), 'player', 'basic_technologies.md');
    if (!fs.existsSync(mdPath)) {
      json(res, 404, { error: 'basic_technologies.md not found' });
      return true;
    }
    res.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
    fs.createReadStream(mdPath).pipe(res);
    return true;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/report.txt') {
    const session = requireSession(req, res);
    if (!session) return true;
    const { txtPath, isoTxt } = reportPathsForSession(session, effectiveFactionId(session));
    const pick = fs.existsSync(txtPath) ? txtPath : isoTxt;
    if (!fs.existsSync(pick)) {
      json(res, 404, { error: 'report not found' });
      return true;
    }
    res.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
    fs.createReadStream(pick).pipe(res);
    return true;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/report-sections') {
    const session = requireSession(req, res);
    if (!session) return true;
    const factionId = effectiveFactionId(session);
    const { txtPath, isoTxt } = reportPathsForSession(session, factionId);
    const pick = fs.existsSync(txtPath) ? txtPath : isoTxt;
    if (!fs.existsSync(pick)) {
      json(res, 404, { error: 'report not found' });
      return true;
    }
    const text = fs.readFileSync(pick, 'utf8');
    json(res, 200, { sections: splitReportSections(text) });
    return true;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/orders') {
    const session = requireSession(req, res);
    if (!session) return true;
    const { run, factionId, turn } = orderContextForSession(session);
    const text = readSubmittedOrder(factionId, turn, run);
    json(res, 200, { submitted: text != null, text: text ?? '', turn, version: listOrderVersions(factionId, turn, run).at(-1) ?? null });
    return true;
  }

  if (req.method === 'PUT' && url.pathname === '/api/session/orders') {
    const session = requireSession(req, res);
    if (!session) return true;
    const body = await readBody(req);
    const { run, factionId, turn } = orderContextForSession(session);
    const version = saveOrder(factionId, body, turn, run);
    try {
      syncLobbyStatusFile();
    } catch (err) {
      console.warn('Lobby status sync failed:', err);
    }
    json(res, 200, { ok: true, turn, version });
    return true;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/check-orders') {
    const session = requireSession(req, res);
    if (!session) return true;
    const body = JSON.parse(await readBody(req));
    const creds = loadFactionCredentials().get(session.factionId);
    const warnings = validateOrderText(body.text || '', session.factionId, creds?.password || '');
    json(res, 200, { warnings });
    return true;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/parse-orders') {
    const session = requireSession(req, res);
    if (!session) return true;
    const body = JSON.parse(await readBody(req));
    const creds = loadFactionCredentials().get(session.factionId);
    const result = await parseOrders(body.text || '', session.factionId, creds?.password || '');
    json(res, 200, result);
    return true;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/battle-sim') {
    const session = requireSession(req, res);
    if (!session) return true;
    const body = JSON.parse(await readBody(req));
    if (!body.xml) {
      json(res, 400, { error: 'missing xml' });
      return true;
    }
    try {
      const result = await runBattleSimulation(body.xml, body.seed);
      json(res, 200, result);
    } catch (err) {
      json(res, 500, { error: String(err.message || err) });
    }
    return true;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/admin/lobby-status') {
    const session = requireAdmin(req, res);
    if (!session) return true;
    json(res, 200, buildStatusJson());
    return true;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/admin/sync-lobby-status') {
    const session = requireAdmin(req, res);
    if (!session) return true;
    try {
      syncLobbyStatusFile();
    } catch (err) {
      json(res, 500, { error: String(err.message || err) });
      return true;
    }
    json(res, 200, buildStatusJson());
    return true;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/admin/orders') {
    const session = requireAdmin(req, res);
    if (!session) return true;
    const factionId = parseFactionIdParam(url);
    if (factionId == null) {
      json(res, 400, { error: 'factionId query required (2–11)' });
      return true;
    }
    const { run, turn } = orderContextForFactionId(session, factionId);
    const text = readSubmittedOrder(factionId, turn, run);
    json(res, 200, {
      submitted: text != null,
      text: text ?? '',
      turn,
      factionId,
      version: listOrderVersions(factionId, turn, run).at(-1) ?? null,
    });
    return true;
  }

  if (req.method === 'PUT' && url.pathname === '/api/session/admin/orders') {
    const session = requireAdmin(req, res);
    if (!session) return true;
    const factionId = parseFactionIdParam(url);
    if (factionId == null) {
      json(res, 400, { error: 'factionId query required (2–11)' });
      return true;
    }
    const body = await readBody(req);
    const { run, turn } = orderContextForFactionId(session, factionId);
    const version = saveOrder(factionId, body, turn, run);
    try {
      syncLobbyStatusFile();
    } catch (err) {
      console.warn('Lobby status sync failed:', err);
    }
    json(res, 200, { ok: true, turn, factionId, version });
    return true;
  }

  if (req.method === 'POST' && url.pathname === '/api/session/admin/parse-orders') {
    const session = requireAdmin(req, res);
    if (!session) return true;
    const body = JSON.parse(await readBody(req));
    const factionId = parseInt(body.factionId, 10);
    if (!Number.isFinite(factionId) || !isPlayerFaction(factionId)) {
      json(res, 400, { error: 'factionId required (2–11)' });
      return true;
    }
    const creds = loadFactionCredentials().get(factionId);
    const result = await parseOrders(body.text || '', factionId, creds?.password || '');
    json(res, 200, result);
    return true;
  }

  return false;
}

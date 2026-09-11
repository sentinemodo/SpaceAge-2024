import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import {
  ensureRunLayout,
  gameinPath,
  latestReportPaths,
  readTurnFromGamein,
  repoRoot,
  runId,
} from './lib/paths.mjs';
import { login, logout, loadFactionCredentials, requireGm, requireSession, sessionFromRequest } from './lib/auth.mjs';
import { bootstrapFromCampaign, promoteGameout, runReports, runTurn } from './lib/game-exe.mjs';
import { buildStatusJson } from './lib/status.mjs';
import { validateOrderText } from './lib/check-orders.mjs';
import { saveOrder } from './lib/orders-io.mjs';
import { isolateReports } from './lib/isolate.mjs';

const PORT = parseInt(process.env.GAME_HOST_PORT || '8787', 10);
const VISUAL_DIST = path.join(repoRoot(), 'visual-tool', 'dist');

ensureRunLayout();

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
    json(res, 200, { ok: true, runId: runId() });
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/auth/login') {
    const body = JSON.parse(await readBody(req));
    const token = login(parseInt(body.factionId, 10), body.password || '');
    if (!token) {
      json(res, 401, { error: 'invalid credentials' });
      return;
    }
    json(res, 200, {
      token,
      factionId: body.factionId,
      name: loadFactionCredentials().get(parseInt(body.factionId, 10))?.name,
    });
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/auth/logout') {
    const session = sessionFromRequest(req);
    if (session) logout(req.headers.authorization?.slice(7));
    json(res, 200, { ok: true });
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/meta') {
    const session = requireSession(req, res);
    if (!session) return;
    json(res, 200, {
      factionId: session.factionId,
      name: session.name,
      turn: fs.existsSync(gameinPath()) ? readTurnFromGamein() : 0,
      engineVersion: '0.1.159',
    });
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/report.xml') {
    const session = requireSession(req, res);
    if (!session) return;
    const { xmlPath, isoTxt, txtPath } = latestReportPaths(session.factionId);
    const pick = fs.existsSync(xmlPath) ? xmlPath : null;
    if (!pick) {
      json(res, 404, { error: 'report xml not found; GM may need to run /reports' });
      return;
    }
    res.writeHead(200, { 'Content-Type': 'application/xml; charset=utf-8' });
    fs.createReadStream(pick).pipe(res);
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/session/report.txt') {
    const session = requireSession(req, res);
    if (!session) return;
    const { txtPath, isoTxt } = latestReportPaths(session.factionId);
    const pick = fs.existsSync(isoTxt) ? isoTxt : txtPath;
    if (!fs.existsSync(pick)) {
      json(res, 404, { error: 'report not found' });
      return;
    }
    res.writeHead(200, { 'Content-Type': 'text/plain; charset=utf-8' });
    fs.createReadStream(pick).pipe(res);
    return;
  }

  if (req.method === 'PUT' && url.pathname === '/api/session/orders') {
    const session = requireSession(req, res);
    if (!session) return;
    const body = await readBody(req);
    saveOrder(session.factionId, body);
    json(res, 200, { ok: true });
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
    json(res, 200, { ok: true, turn: readTurnFromGamein() });
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/gm/reports') {
    if (!requireGm(req, res)) return;
    await runReports();
    isolateReports();
    json(res, 200, { ok: true, turn: readTurnFromGamein() });
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/gm/turn') {
    if (!requireGm(req, res)) return;
    const before = readTurnFromGamein();
    await runTurn();
    promoteGameout(before + 1);
    isolateReports();
    json(res, 200, { ok: true, turn: readTurnFromGamein() });
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/gm/isolate') {
    if (!requireGm(req, res)) return;
    isolateReports();
    json(res, 200, { ok: true });
    return;
  }

  json(res, 404, { error: 'not found' });
});

server.listen(PORT, () => {
  console.log(`SpaceAge game-host run=${runId()} http://localhost:${PORT}`);
  console.log(`Visual tool (if built): http://localhost:${PORT}/client/`);
});

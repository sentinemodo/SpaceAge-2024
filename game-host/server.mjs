import './lib/env-bootstrap.mjs';
import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { ensureRunLayout, repoRoot, runId } from './lib/paths.mjs';
import { buildStatusJson } from './lib/status.mjs';
import { cors, json } from './lib/http.mjs';
import { startRunPodStatusPolling } from './lib/runpod.mjs';
import { handleAi } from './routes/ai.mjs';
import { handleGm } from './routes/gm.mjs';
import { handleRunPod } from './routes/runpod.mjs';
import { handleSession } from './routes/session.mjs';

const PORT = parseInt(process.env.GAME_HOST_PORT || '8787', 10);
const VISUAL_DIST = path.join(repoRoot(), 'tools', 'visual-tool', 'dist');

ensureRunLayout();

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

  if (req.method === 'GET' && url.pathname === '/api/public/lobby-status') {
    json(res, 200, buildStatusJson());
    return;
  }

  if (req.method === 'GET' && url.pathname === '/health') {
    json(res, 200, {
      ok: true,
      runId: runId(),
      features: { adminBrowse: true, viewAsFaction: true, ai: true, runpod: true },
    });
    return;
  }

  if (await handleSession(req, res, url)) return;
  if (await handleRunPod(req, res, url)) return;
  if (await handleAi(req, res, url)) return;
  if (await handleGm(req, res, url)) return;

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

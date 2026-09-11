import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { spawn } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(__dirname, '../../..');
const publicDir = path.resolve(__dirname, '../public');
const gameExe = path.join(repoRoot, 'Game', 'bin', 'Debug', 'Game.exe');
const campaignData = path.join(repoRoot, 'campaign');
const port = Number(process.env.BATTLE_SIM_PORT || 4173);

function contentType(filePath) {
  if (filePath.endsWith('.html')) return 'text/html; charset=utf-8';
  if (filePath.endsWith('.css')) return 'text/css; charset=utf-8';
  if (filePath.endsWith('.js')) return 'text/javascript; charset=utf-8';
  if (filePath.endsWith('.json')) return 'application/json; charset=utf-8';
  return 'application/octet-stream';
}

function serveStatic(request, response) {
  const url = new URL(request.url, `http://${request.headers.host}`);
  let filePath = path.join(publicDir, url.pathname === '/' ? 'index.html' : url.pathname);
  if (!filePath.startsWith(publicDir)) {
    response.writeHead(403);
    response.end('Forbidden');
    return;
  }
  if (!fs.existsSync(filePath) || fs.statSync(filePath).isDirectory()) {
    response.writeHead(404);
    response.end('Not found');
    return;
  }
  response.writeHead(200, { 'Content-Type': contentType(filePath) });
  fs.createReadStream(filePath).pipe(response);
}

function runSimulation(xml, seedOverride) {
  return new Promise((resolve, reject) => {
    const tempDir = fs.mkdtempSync(path.join(process.env.TEMP || '/tmp', 'spaceage-bsim-'));
    const inputPath = path.join(tempDir, 'sim-input.xml');
    const outputPath = path.join(tempDir, 'sim-output.txt');
    fs.writeFileSync(inputPath, xml, { encoding: 'utf8' });

    const args = ['/battle-sim', inputPath, outputPath, '/data', campaignData];
    if (seedOverride != null) {
      args.push('/seed', String(seedOverride));
    }

    const child = spawn(gameExe, args, { cwd: repoRoot, windowsHide: true });
    let stderr = '';
    child.stderr.on('data', (chunk) => {
      stderr += chunk.toString();
    });
    child.on('close', (code) => {
      try {
        if (code !== 0) {
          reject(new Error(stderr || `Game.exe exited with code ${code}`));
          return;
        }
        const output = fs.existsSync(outputPath)
          ? fs.readFileSync(outputPath, 'utf8')
          : '';
        resolve(output);
      } finally {
        fs.rmSync(tempDir, { recursive: true, force: true });
      }
    });
  });
}

const server = http.createServer(async (request, response) => {
  const url = new URL(request.url, `http://${request.headers.host}`);

  if (request.method === 'POST' && url.pathname === '/api/run') {
    let body = '';
    request.on('data', (chunk) => {
      body += chunk.toString();
    });
    request.on('end', async () => {
      try {
        if (!fs.existsSync(gameExe)) {
          throw new Error(`Game.exe not found at ${gameExe}. Build the Game project first.`);
        }
        if (!fs.existsSync(path.join(campaignData, 'data.xml'))) {
          throw new Error(`campaign/data.xml not found at ${campaignData}`);
        }
        const payload = JSON.parse(body || '{}');
        if (!payload.xml) {
          throw new Error('Missing xml in request body.');
        }
        const output = await runSimulation(payload.xml, payload.seed);
        response.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
        response.end(JSON.stringify({ output }));
      } catch (error) {
        response.writeHead(500, { 'Content-Type': 'application/json; charset=utf-8' });
        response.end(JSON.stringify({ error: error.message }));
      }
    });
    return;
  }

  if (request.method === 'GET') {
    serveStatic(request, response);
    return;
  }

  response.writeHead(405);
  response.end('Method not allowed');
});

server.listen(port, () => {
  console.log(`Battle simulator UI: http://localhost:${port}`);
  console.log(`Game.exe: ${gameExe}`);
  console.log(`Catalog: ${campaignData}`);
});

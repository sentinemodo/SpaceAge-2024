import { execSync } from 'node:child_process';
import { spawn } from 'node:child_process';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const PORT = parseInt(process.env.GAME_HOST_PORT || '8787', 10);
const __dirname = path.dirname(fileURLToPath(import.meta.url));
const hostRoot = path.resolve(__dirname, '..');

function pidsOnPort(port) {
  try {
    const out = execSync(`netstat -ano | findstr :${port}`, { encoding: 'utf8' });
    const pids = new Set();
    for (const line of out.split('\n')) {
      if (!line.includes('LISTENING')) continue;
      const pid = line.trim().split(/\s+/).pop();
      if (pid && pid !== '0') pids.add(pid);
    }
    return [...pids];
  } catch {
    return [];
  }
}

for (const pid of pidsOnPort(PORT)) {
  try {
    execSync(`taskkill /PID ${pid} /F`, { stdio: 'ignore' });
    console.log(`Stopped process ${pid} on port ${PORT}`);
  } catch {
    /* already gone */
  }
}

const child = spawn(process.execPath, ['server.mjs'], {
  cwd: hostRoot,
  stdio: 'inherit',
  env: process.env,
});

child.on('exit', (code) => process.exit(code ?? 0));

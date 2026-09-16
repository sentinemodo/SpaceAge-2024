import { loadRepoEnv } from '../lib/load-env.mjs';

loadRepoEnv();
const key = process.env.RUNPOD_API_KEY || '';
console.log('RUNPOD_API_KEY set:', !!key);
console.log('RUNPOD_API_KEY length:', key.length);
console.log('RUNPOD_API_KEY prefix:', key.slice(0, 4));
if (key.startsWith('"') || key.startsWith("'")) {
  console.log('WARNING: key appears to include quote characters — remove quotes in .env');
}
if (key.includes(' ')) {
  console.log('WARNING: key contains spaces');
}

const listRes = await fetch('https://api.runpod.io/v2/pods', {
  headers: { Authorization: `Bearer ${key}` },
});
const listText = await listRes.text();
console.log('GET /v2/pods status:', listRes.status);
console.log('response preview:', listText.slice(0, 120));

if (!listRes.ok) {
  process.exit(1);
}

console.log('Key can list pods. Restart game-host after changing .env (npm run restart in game-host/).');

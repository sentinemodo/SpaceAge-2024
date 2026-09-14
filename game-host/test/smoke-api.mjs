import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const BASE = process.env.GAME_HOST_URL || 'http://localhost:8787';
const __dirname = path.dirname(fileURLToPath(import.meta.url));
const battleXml = fs.readFileSync(
  path.join(__dirname, '../../Tests/fixtures/battle-sim/inftry-skirmish.xml'),
  'utf8'
);

async function req(pathname, init = {}) {
  const res = await fetch(`${BASE}${pathname}`, init);
  const ct = res.headers.get('content-type') || '';
  const body = ct.includes('json') ? await res.json() : await res.text();
  return { status: res.status, body };
}

let token;

console.log('1. Login faction 2…');
{
  const { status, body } = await req('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ factionId: 2, password: 'northwnd' }),
  });
  assert.equal(status, 200, JSON.stringify(body));
  assert.ok(body.token);
  token = body.token;
  console.log('   OK', body.name);
}

const auth = { Authorization: `Bearer ${token}` };

console.log('2. Fetch report.xml…');
{
  const res = await fetch(`${BASE}/api/session/report.xml`, { headers: auth });
  assert.equal(res.status, 200);
  const xml = await res.text();
  assert.match(xml, /<game\s+turn="1"/);
  assert.match(xml, /<galaxy>/);
  console.log('   OK', `${xml.length} bytes`);
}

console.log('3. Fetch report-sections…');
{
  const { status, body } = await req('/api/session/report-sections', { headers: auth });
  assert.equal(status, 200);
  assert.ok(Array.isArray(body.sections));
  assert.ok(body.sections.some((s) => s.id === 'galaxy'));
  assert.ok(body.sections.some((s) => s.id === 'orders'));
  console.log('   OK', body.sections.map((s) => s.id).join(', '));
}

console.log('4. Parse orders (bad password)…');
{
  const orderText = '#faction 2 "wrongpass"\n#modulestack 200001\nMOVE R99999\n#end\n';
  const { status, body } = await req('/api/session/parse-orders', {
    method: 'POST',
    headers: { ...auth, 'Content-Type': 'application/json' },
    body: JSON.stringify({ text: orderText }),
  });
  assert.equal(status, 200);
  assert.equal(body.ok, false);
  assert.ok(body.errors.length > 0 || body.warnings.length > 0);
  assert.notEqual(body.engineUnavailable, true, 'engine should be available');
  console.log('   OK', body.errors[0] || body.warnings[0]);
}

console.log('5. Battle sim…');
{
  const { status, body } = await req('/api/session/battle-sim', {
    method: 'POST',
    headers: { ...auth, 'Content-Type': 'application/json' },
    body: JSON.stringify({ xml: battleXml, seed: 42 }),
  });
  assert.equal(status, 200);
  assert.match(body.output, /SIMULATION RESULT:/);
  assert.equal(body.result, 'INDECISIVE');
  console.log('   OK', body.result);
}

console.log('\nAll smoke checks passed.');

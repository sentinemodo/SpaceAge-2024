import assert from 'node:assert/strict';
import test from 'node:test';
import { handleAi } from '../routes/ai.mjs';
import { handleGm } from '../routes/gm.mjs';
import { handleRunPod } from '../routes/runpod.mjs';
import { handleSession } from '../routes/session.mjs';

function fakeRes() {
  return {
    writeHead() {},
    end() {},
    setHeader() {},
  };
}

test('route modules leave unknown paths unclaimed', async () => {
  const req = { method: 'GET', headers: {} };
  const res = fakeRes();
  const url = new URL('http://localhost/api/nope');
  assert.equal(await handleSession(req, res, url), false);
  assert.equal(await handleRunPod(req, res, url), false);
  assert.equal(await handleAi(req, res, url), false);
  assert.equal(await handleGm(req, res, url), false);
});

test('GM status without a key is handled as forbidden', async () => {
  let status = 0;
  let body = '';
  const res = {
    writeHead(code) { status = code; },
    end(payload) { body = String(payload || ''); },
    setHeader() {},
  };
  const url = new URL('http://localhost/api/gm/status');
  const handled = await handleGm({ method: 'GET', headers: {} }, res, url);
  assert.equal(handled, true);
  assert.equal(status, 403);
  assert.match(body, /forbidden/);
});

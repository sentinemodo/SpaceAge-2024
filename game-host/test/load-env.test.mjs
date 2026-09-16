import { test } from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { loadRepoEnv } from '../lib/load-env.mjs';

test('loadRepoEnv reads repo-root .env and overrides inherited shell vars', () => {
  const tmp = fs.mkdtempSync(path.join(os.tmpdir(), 'sa-env-'));
  fs.writeFileSync(path.join(tmp, '.env'), 'SA_TEST_ENV_KEY=from-file\n', 'utf8');
  const prevRoot = process.env.REPO_ROOT;
  const prevVal = process.env.SA_TEST_ENV_KEY;
  process.env.SA_TEST_ENV_KEY = 'from-shell';
  process.env.REPO_ROOT = tmp;
  try {
    loadRepoEnv();
    assert.equal(process.env.SA_TEST_ENV_KEY, 'from-file');
  } finally {
    if (prevRoot === undefined) delete process.env.REPO_ROOT;
    else process.env.REPO_ROOT = prevRoot;
    if (prevVal === undefined) delete process.env.SA_TEST_ENV_KEY;
    else process.env.SA_TEST_ENV_KEY = prevVal;
    fs.rmSync(tmp, { recursive: true, force: true });
  }
});

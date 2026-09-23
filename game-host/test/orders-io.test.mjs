import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const testRunId = `test-orders-${process.pid}`;

process.env.REPO_ROOT = path.resolve(__dirname, '..', '..');
process.env.GAME_HOST_RUN_ID = testRunId;

const { ensureRunLayout, factionsDir, turnDir } = await import('../lib/paths.mjs');
const { saveOrder } = await import('../lib/orders-io.mjs');

describe('saveOrder', () => {
  it('writes faction draft and turn order files with submitted body', () => {
    ensureRunLayout();
    const body = '#faction 2 "secret"\n#modulestack 200001\n+get 2 titani from 200003\n#end\n';
    saveOrder(2, body);

    const draftPath = path.join(factionsDir(), '02', 'order.2.txt');
    const turnPath = path.join(turnDir(), 'order.2.txt');

    assert.ok(fs.existsSync(draftPath), `missing draft: ${draftPath}`);
    assert.ok(fs.existsSync(turnPath), `missing turn copy: ${turnPath}`);
    assert.equal(fs.readFileSync(draftPath, 'utf8'), body);
    assert.equal(fs.readFileSync(turnPath, 'utf8'), body);
  });
});

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
const {
  listOrderVersions,
  orderVersionFileName,
  readSubmittedOrder,
  saveOrder,
} = await import('../lib/orders-io.mjs');

describe('saveOrder', () => {
  it('writes versioned faction drafts and turn order files', () => {
    ensureRunLayout();
    const turn = 3;
    const body = '#faction 2 "secret"\n#modulestack 200001\n+get 2 titani from 200003\n#end\n';
    const v1 = saveOrder(2, body, turn);
    assert.equal(v1, 1);

    const folder = path.join(factionsDir(), '02');
    const v1Path = path.join(folder, orderVersionFileName(2, turn, 1));
    const draftPath = path.join(folder, 'order.2.txt');
    const turnPath = path.join(turnDir(), 'order.2.txt');

    assert.ok(fs.existsSync(v1Path), `missing version file: ${v1Path}`);
    assert.ok(fs.existsSync(draftPath), `missing draft: ${draftPath}`);
    assert.ok(fs.existsSync(turnPath), `missing turn copy: ${turnPath}`);
    assert.equal(fs.readFileSync(v1Path, 'utf8'), body);
    assert.equal(readSubmittedOrder(2, turn), body);
    assert.deepEqual(listOrderVersions(2, turn), [1]);
  });

  it('increments version on each submit for the same turn', () => {
    ensureRunLayout();
    const turn = 4;
    saveOrder(2, '#faction 2 "a"\n#end\n', turn);
    saveOrder(2, '#faction 2 "b"\n#end\n', turn);
    assert.deepEqual(listOrderVersions(2, turn), [1, 2]);
    assert.equal(readSubmittedOrder(2, turn), '#faction 2 "b"\n#end\n');
  });

  it('scopes reads to the requested turn', () => {
    ensureRunLayout();
    saveOrder(2, '#faction 2 "turn5"\n#end\n', 5);
    assert.equal(readSubmittedOrder(2, 5)?.includes('turn5'), true);
    assert.equal(readSubmittedOrder(2, 6), null);
  });

  it('returns null when the faction has not submitted orders for the turn', () => {
    ensureRunLayout();
    assert.equal(readSubmittedOrder(9, 1), null);
  });
});

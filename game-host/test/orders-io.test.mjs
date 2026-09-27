import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const testRunId = `test-orders-${process.pid}`;

process.env.REPO_ROOT = path.resolve(__dirname, '..', '..');
process.env.GAME_HOST_RUN_ID = testRunId;

const { ensureRunLayout, factionsDir, gameinPath, turnDir } = await import('../lib/paths.mjs');
const {
  hasSubmittedOrders,
  listOrderVersions,
  orderVersionFileName,
  readSubmittedOrder,
  resolveSubmittedOrderPathForTurnRun,
  saveOrder,
  stageSubmittedOrdersForTurnRun,
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

  it('resolveSubmittedOrderPathForTurnRun prefers orders for gamein report turn', () => {
    ensureRunLayout();
    fs.mkdirSync(path.dirname(gameinPath()), { recursive: true });
    fs.writeFileSync(gameinPath(), '<?xml version="1.0"?><game turn="2"></game>', 'utf8');
    const folder = path.join(factionsDir(), '05');
    fs.mkdirSync(folder, { recursive: true });
    fs.writeFileSync(
      path.join(folder, orderVersionFileName(5, 1, 1)),
      '#faction 5 "t1"\n#end\n',
      'utf8',
    );
    fs.writeFileSync(
      path.join(folder, orderVersionFileName(5, 2, 1)),
      '#faction 5 "t2"\n#end\n',
      'utf8',
    );
    assert.equal(path.basename(resolveSubmittedOrderPathForTurnRun(5)), 'orders.5.2.1.txt');
  });

  it('resolveSubmittedOrderPathForTurnRun prefers latest version over legacy draft', () => {
    ensureRunLayout();
    fs.mkdirSync(path.dirname(gameinPath()), { recursive: true });
    fs.writeFileSync(gameinPath(), '<?xml version="1.0"?><game turn="1"></game>', 'utf8');
    const folder = path.join(factionsDir(), '03');
    fs.mkdirSync(folder, { recursive: true });
    fs.writeFileSync(path.join(folder, 'order.3.txt'), '#faction 3 "stale"\n#end\n', 'utf8');
    fs.writeFileSync(
      path.join(folder, orderVersionFileName(3, 1, 10)),
      '#faction 3 "v10"\n#end\n',
      'utf8',
    );
    const resolved = resolveSubmittedOrderPathForTurnRun(3);
    assert.equal(path.basename(resolved), 'orders.3.1.10.txt');
    const stagedBody = fs.readFileSync(resolved, 'utf8');
    assert.match(stagedBody, /v10/);
    assert.doesNotMatch(stagedBody, /stale/);
  });

  it('stageSubmittedOrdersForTurnRun copies resolved paths for all player factions', () => {
    ensureRunLayout();
    const turn = 1;
    for (let id = 2; id <= 11; id += 1) {
      saveOrder(id, `#faction ${id} "all"\n#end\n`, turn);
    }
    saveOrder(3, '#faction 3 "rev2"\n#end\n', turn);
    stageSubmittedOrdersForTurnRun();
    assert.match(fs.readFileSync(path.join(turnDir(), 'order.3.txt'), 'utf8'), /rev2/);
    for (let id = 2; id <= 11; id += 1) {
      fs.unlinkSync(path.join(turnDir(), `order.${id}.txt`));
    }
  });

  it('hasSubmittedOrders is true for LLM version file without order.{id}.txt draft', () => {
    ensureRunLayout();
    const turn = 6;
    const folder = path.join(factionsDir(), '04');
    fs.mkdirSync(folder, { recursive: true });
    const versionPath = path.join(folder, orderVersionFileName(4, turn, 1));
    fs.writeFileSync(versionPath, '#faction 4 "llm"\n#end\n', 'utf8');
    assert.equal(readSubmittedOrder(4, turn)?.includes('llm'), true);
    assert.equal(hasSubmittedOrders(4, turn), true);
    assert.equal(hasSubmittedOrders(4, turn + 1), false);
  });
});

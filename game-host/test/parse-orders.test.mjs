import test from 'node:test';
import assert from 'node:assert/strict';
import { parseOrders } from '../lib/parse-orders.mjs';

test('parseOrders flags syntax issues when engine unavailable', async () => {
  const body = 'MOVE O00001';
  const result = await parseOrders(body, 2, 'secret');
  assert.equal(result.ok, false);
  assert.ok(result.warnings.some((w) => w.includes('#faction')));
  assert.ok(result.output?.includes('ok:'));
});

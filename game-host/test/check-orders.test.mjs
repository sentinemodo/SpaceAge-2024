import test from 'node:test';
import assert from 'node:assert/strict';
import { validateOrderText } from '../lib/check-orders.mjs';

test('validateOrderText requires faction header', () => {
  const w = validateOrderText('MOVE O00001', 2, 'secret');
  assert.ok(w.some((x) => x.includes('#faction')));
});

test('validateOrderText accepts valid header', () => {
  const body = '#faction 2 "secret"\nMOVE O00001';
  const w = validateOrderText(body, 2, 'secret');
  assert.equal(w.length, 0);
});

test('validateOrderText flags wrong password', () => {
  const body = '#faction 2 "wrong"\nMOVE O00001';
  const w = validateOrderText(body, 2, 'secret');
  assert.ok(w.some((x) => x.includes('password')));
});

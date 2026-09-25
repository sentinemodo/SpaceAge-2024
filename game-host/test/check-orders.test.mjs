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

test('validateOrderText accepts @use and #end', () => {
  const body = [
    '#faction 2 "secret"',
    '#modulestack 200001',
    '@use farmng',
    '#end',
  ].join('\n');
  const w = validateOrderText(body, 2, 'secret');
  assert.equal(w.length, 0);
});

test('validateOrderText accepts +get conditional prefix', () => {
  const body = [
    '#faction 2 "secret"',
    '#modulestack 200001',
    '+get 2 titani from 200003',
    '#end',
  ].join('\n');
  const w = validateOrderText(body, 2, 'secret');
  assert.equal(w.length, 0);
});

test('validateOrderText includes full line in warnings', () => {
  const body = '#faction 2 "secret"\nBADVERB O00001';
  const w = validateOrderText(body, 2, 'secret');
  assert.ok(w.some((x) => x.startsWith('BADVERB O00001:')));
});

test('validateOrderText accepts numeric repeat before verb', () => {
  const body = [
    '#faction 2 "secret"',
    '#modulestack 200001',
    '3 use iminng',
    '10 use hcdril',
    '-3 use armcbt as new4 for 000025',
  ].join('\n');
  const w = validateOrderText(body, 2, 'secret');
  assert.equal(w.length, 0);
});

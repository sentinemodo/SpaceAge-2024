import test from 'node:test';
import assert from 'node:assert/strict';
import { resolveGameSpawn } from '../lib/game-exe.mjs';

test('resolveGameSpawn uses mono on linux by default', () => {
  const r = resolveGameSpawn('linux', {});
  assert.equal(r.useMono, true);
  assert.equal(r.command, 'mono');
  assert.equal(r.argvPrefix.length, 1);
  assert.match(r.exePath, /Game\.exe$/);
});

test('resolveGameSpawn skips mono on win32', () => {
  const r = resolveGameSpawn('win32', {});
  assert.equal(r.useMono, false);
  assert.equal(r.command, r.exePath);
  assert.deepEqual(r.argvPrefix, []);
});

test('resolveGameSpawn honors GAME_USE_MONO=1 on win32', () => {
  const r = resolveGameSpawn('win32', { GAME_USE_MONO: '1', MONO_EXE: '/usr/bin/mono' });
  assert.equal(r.useMono, true);
  assert.equal(r.command, '/usr/bin/mono');
});

test('resolveGameSpawn honors GAME_USE_MONO=0 on linux', () => {
  const r = resolveGameSpawn('linux', { GAME_USE_MONO: '0' });
  assert.equal(r.useMono, false);
  assert.equal(r.command, r.exePath);
});

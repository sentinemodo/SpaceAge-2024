import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
process.env.REPO_ROOT = path.resolve(__dirname, '..', '..');
process.env.GAME_HOST_RUN_ID = 'beta-1';

const { listFactions, getFaction, login } = await import('../lib/auth.mjs');

describe('auth factions', () => {
  it('lists player and NPC factions for admin browse', () => {
    const factions = listFactions();
    assert.ok(factions.some((f) => f.id === 2 && !f.npc && f.name === 'Northwind'));
    assert.ok(factions.some((f) => f.id === 17 && f.npc && f.name === 'Graph Fauna'));
    assert.ok(factions.some((f) => f.id === 1 && f.npc));
  });

  it('allows GM login as NPC faction', () => {
    const token = login(17, '', 'dev-gm-key');
    assert.ok(token);
    assert.ok(getFaction(17)?.name === 'Graph Fauna');
  });

  it('rejects NPC login without GM key', () => {
    assert.equal(login(17, '', ''), null);
  });
});

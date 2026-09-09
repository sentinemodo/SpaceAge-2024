import { readFileSync, existsSync } from 'node:fs';
import { resolve } from 'node:path';
import { describe, it, expect, beforeAll } from 'vitest';
import { assertNoSecretLeaks, validateStatusJson } from '../src/lib/statusSchema';

const publicStatusPath = resolve(process.cwd(), 'public/status.json');
const fixtureStatusPath = resolve(process.cwd(), 'tests/fixtures/status.json');

describe('Status JSON Schema', () => {
  let raw = '';
  let parsed: unknown;

  beforeAll(() => {
    const path = existsSync(publicStatusPath) ? publicStatusPath : fixtureStatusPath;
    raw = readFileSync(path, 'utf8');
    parsed = JSON.parse(raw);
  });

  it('public/status.json validates against schema v1', () => {
    const status = validateStatusJson(parsed);
    expect(status.factions).toHaveLength(10);
    expect(status.factions.map((f) => f.id)).toEqual([2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
  });

  it('public/status.json has a valid status enum value', () => {
    const status = validateStatusJson(parsed);
    expect(['not-started', 'accepting-orders', 'processing', 'reports-out']).toContain(status.status);
  });

  it('public/status.json has turn as a number >= 0', () => {
    const status = validateStatusJson(parsed);
    expect(typeof status.turn).toBe('number');
    expect(status.turn).toBeGreaterThanOrEqual(0);
  });

  it('public/status.json does not contain forbidden top-level keys', () => {
    const keys = Object.keys(parsed as object);
    expect(keys).not.toContain('password');
    expect(keys).not.toContain('email');
    expect(keys).not.toContain('path');
  });

  it('public/status.json factions do not leak secrets', () => {
    validateStatusJson(parsed).factions.forEach((faction) => {
      const factionKeys = Object.keys(faction);
      expect(factionKeys).not.toContain('password');
      expect(factionKeys).not.toContain('email');
      expect(factionKeys).not.toContain('path');
      expect(factionKeys).not.toContain('report');
      expect(factionKeys).not.toContain('orders');
    });
  });

  it('public/status.json each faction has id and submitted', () => {
    validateStatusJson(parsed).factions.forEach((faction) => {
      expect(typeof faction.id).toBe('number');
      expect(typeof faction.submitted).toBe('boolean');
    });
  });

  it('public/status.json nextTurnAt is a string or null if provided', () => {
    const status = validateStatusJson(parsed);
    if (status.nextTurnAt !== undefined) {
      expect(status.nextTurnAt === null || typeof status.nextTurnAt === 'string').toBe(true);
    }
  });

  it('public/status.json raw text passes leak bar (WS-008)', () => {
    expect(() => assertNoSecretLeaks(raw)).not.toThrow();
  });
});

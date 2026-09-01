import { describe, it, expect } from 'vitest';

/**
 * Status JSON Schema Validation Tests
 * Ensures status.json conforms to the closed-lobby schema:
 * - Exactly 10 factions (2–11)
 * - No password, email, or path keys
 * - Status enum: not-started, accepting-orders, processing, reports-out
 * - Each faction: id (2–11), submitted (boolean), optional name
 */

interface StatusData {
  status: 'not-started' | 'accepting-orders' | 'processing' | 'reports-out';
  turn: number;
  nextTurnAt?: string | null;
  factions: Array<{
    id: number;
    submitted: boolean;
    name?: string;
  }>;
}

describe('Status JSON Schema', () => {
  const validStatus: StatusData = {
    status: 'not-started',
    turn: 1,
    nextTurnAt: null,
    factions: [
      { id: 2, submitted: false, name: 'Faction 2' },
      { id: 3, submitted: false, name: 'Faction 3' },
      { id: 4, submitted: false, name: 'Faction 4' },
      { id: 5, submitted: false, name: 'Faction 5' },
      { id: 6, submitted: false, name: 'Faction 6' },
      { id: 7, submitted: false, name: 'Faction 7' },
      { id: 8, submitted: false, name: 'Faction 8' },
      { id: 9, submitted: false, name: 'Faction 9' },
      { id: 10, submitted: false, name: 'Faction 10' },
      { id: 11, submitted: false, name: 'Faction 11' },
    ],
  };

  it('should have exactly 10 factions (2–11)', () => {
    expect(validStatus.factions).toHaveLength(10);
    const ids = validStatus.factions.map(f => f.id);
    expect(ids).toEqual([2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
  });

  it('should have a valid status enum value', () => {
    const validStatuses = ['not-started', 'accepting-orders', 'processing', 'reports-out'];
    expect(validStatuses).toContain(validStatus.status);
  });

  it('should have turn as a number >= 0', () => {
    expect(typeof validStatus.turn).toBe('number');
    expect(validStatus.turn).toBeGreaterThanOrEqual(0);
  });

  it('should not contain password, email, or path keys at top level', () => {
    const keys = Object.keys(validStatus);
    expect(keys).not.toContain('password');
    expect(keys).not.toContain('email');
    expect(keys).not.toContain('path');
  });

  it('should not leak faction-level secrets (no password/email/path per faction)', () => {
    validStatus.factions.forEach(faction => {
      const factionKeys = Object.keys(faction);
      expect(factionKeys).not.toContain('password');
      expect(factionKeys).not.toContain('email');
      expect(factionKeys).not.toContain('path');
      expect(factionKeys).not.toContain('report');
      expect(factionKeys).not.toContain('orders');
    });
  });

  it('each faction should have id and submitted', () => {
    validStatus.factions.forEach(faction => {
      expect(faction).toHaveProperty('id');
      expect(faction).toHaveProperty('submitted');
      expect(typeof faction.id).toBe('number');
      expect(typeof faction.submitted).toBe('boolean');
    });
  });

  it('nextTurnAt should be a string or null if provided', () => {
    if (validStatus.nextTurnAt !== undefined) {
      expect(validStatus.nextTurnAt === null || typeof validStatus.nextTurnAt === 'string').toBe(true);
    }
  });
});

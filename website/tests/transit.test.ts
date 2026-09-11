import { describe, it, expect } from 'vitest';
import { durationWeeks, parseMassLine } from '../src/lib/transit';

describe('transit WS-010', () => {
  it('Arbor to Gate ~39 weeks at default speed', () => {
    const weeks = durationWeeks(79, 1, 40000, 4150);
    expect(weeks).toBe(39);
  });

  it('parses mass line from report paste', () => {
    expect(parseMassLine('mass: 40000/4150')).toEqual({ thrust: 40000, mass: 4150 });
  });
});

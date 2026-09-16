import { describe, it, expect } from 'vitest';
import { parseStackDetails } from './stackDetailsParser';

describe('stackDetailsParser', () => {
  it('parses indented stack detail blocks from galaxy text', () => {
    const text = `
+ Gelvaren complex [000026], city [city], immobile.
  size: 25000, mass: 5396 (0), capacity: 15000/9240, upkeep: 100 units of food [food].
  items: 1000 cash [cash], 10 terrans [terran] (size: 40, mass: 40).
- Sydney garrison [000007], infantry battalion [inftry], immobile, owned by NPC [1].
  size: 500.
Orders Template:
  #modulestack 000026
`;
    const details = parseStackDetails(text);
    expect(details.get('000026')).toMatch(/size: 25000/);
    expect(details.get('000026')).toMatch(/items: 1000 cash/);
    expect(details.get('000007')).toBe('size: 500.');
  });
});

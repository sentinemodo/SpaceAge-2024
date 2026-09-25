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

  it('joins wrapped item lines that continue with digits or punctuation', () => {
    const text = `
+ small cargo bay [210003], 2 small cargo bays [cargob], immobile.
  size: 4000, mass: 1890 (400), capacity: 3600/1315, upkeep: 20 cash [cash].
  items: 400 units of food [food] (size: 400, mass: 400), 200 units of terran breathing gas
  mixture [terair] (size: 200, mass: 200), 200 units of oxyhydro [h2o2] (size: 200, mass: 200),
  40 units of iron [iron] (size: 200, mass: 400), 5 units of oil [oil] (size: 20, mass: 25).
Orders Template:
`;
    const details = parseStackDetails(text);
    const block = details.get('210003') ?? '';
    expect(block).toMatch(/40 units of iron/);
    expect(block).toMatch(/5 units of oil/);
  });
});

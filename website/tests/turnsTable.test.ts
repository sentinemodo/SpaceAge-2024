import { describe, expect, it } from 'vitest';
import { renderTurnsTableHtml } from '../src/lib/turnsTable';

describe('renderTurnsTableHtml', () => {
  it('marks faction 2 submitted in the Arbor column', () => {
    const html = renderTurnsTableHtml([
      { id: 2, submitted: true, name: 'Faction 2' },
      { id: 3, submitted: false },
      { id: 4, submitted: false },
      { id: 5, submitted: false },
      { id: 6, submitted: false },
      { id: 7, submitted: false },
      { id: 8, submitted: false },
      { id: 9, submitted: false },
      { id: 10, submitted: false },
      { id: 11, submitted: false },
    ]);

    expect(html).toContain('data-faction-id="2"');
    expect(html).toContain('Northwind (2)');
    expect(html).toMatch(/data-faction-id="2"[\s\S]*✓ Submitted/);
    expect(html).toContain('Arbor (Helios)');
    expect(html).toContain('Anvil (Fomal)');
  });
});

import { describe, expect, it } from 'vitest';
import { fullFactionReportText, sectionText } from './reportSections';

describe('reportSections', () => {
  it('maps legacy quarterEvents id to events', () => {
    const sections = [{ id: 'quarterEvents', title: 'Events', text: 'Events this quarter:\n  week 1: foo.' }];
    expect(sectionText(sections, 'events', '')).toContain('Events this quarter');
  });

  it('prefers full text file content for the faction panel', () => {
    const sections = [{ id: 'galaxy', title: 'Galaxy', text: 'Galaxy report:\npartial' }];
    const full = 'SpaceAge report for Test [2].\nGalaxy report:\nfull body';
    expect(fullFactionReportText(sections, full)).toBe(full);
  });

  it('joins all sections when full text is missing', () => {
    const sections = [
      { id: 'events', title: 'Events', text: 'Events this quarter:\n  a.' },
      { id: 'bank', title: 'Bank', text: 'Bank report:\n  balance 1.' },
    ];
    expect(fullFactionReportText(sections, '')).toContain('Bank report');
    expect(fullFactionReportText(sections, '')).toContain('Events this quarter');
  });
});

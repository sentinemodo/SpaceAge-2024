import type { ReportSection } from '../api/client';

export function sectionText(sections: ReportSection[], id: string, fallback = 'No section in report.'): string {
  const hit = sections.find((s) => s.id === id);
  if (hit?.text?.trim()) return hit.text;
  return fallback;
}

/** Extract a section body when splitReportSections missed it (e.g. older host). */
export function extractSectionFromFullText(fullText: string, header: string): string {
  const re = new RegExp(`^${header.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}`, 'im');
  const start = fullText.search(re);
  if (start < 0) return '';
  const rest = fullText.slice(start);
  const next = rest.slice(header.length).search(/^(Events|Bank|Survey|Technology|Battles?|Press|Contracts|Galaxy|Orders) /im);
  const body = next >= 0 ? rest.slice(0, header.length + next) : rest;
  return body.trim();
}

export function bankReportText(sections: ReportSection[], fullText = ''): string {
  const fromSection = sectionText(sections, 'bank', '');
  if (fromSection && !fromSection.startsWith('No ')) return fromSection;
  const extracted = extractSectionFromFullText(fullText, 'Bank report:');
  return extracted || 'No bank report section.';
}

export function filterReportLines(text: string, query: string): string {
  const q = query.trim().toLowerCase();
  if (!q) return text;
  const lines = text.split('\n');
  const filtered = lines.filter((line) => line.toLowerCase().includes(q));
  return filtered.length > 0 ? filtered.join('\n') : 'No matching lines.';
}

function extractStancesText(...sources: string[]): string {
  for (const source of sources) {
    const lines = source.split('\n');
    const idx = lines.findIndex((l) => /^Declared stances:/i.test(l.trim()));
    if (idx < 0) continue;
    const body: string[] = ['Declared stances:'];
    for (let i = idx + 1; i < lines.length; i++) {
      const line = lines[i];
      const trimmed = line.trim();
      if (!trimmed) break;
      if (/^Rumors:/i.test(trimmed)) break;
      if (/^[A-Z][a-z]+ report:/i.test(trimmed)) break;
      if (/^Contract reports:/i.test(trimmed)) break;
      body.push(line);
    }
    if (body.length > 1) return body.join('\n');
  }
  return 'Declared stances:\n  default: neutral.\n  unknown: neutral.';
}

function extractRumorsBlock(...sources: string[]): string {
  for (const source of sources) {
    const lines = source.split('\n');
    const start = lines.findIndex((l) => /^Rumors:\s*$/i.test(l.trim()));
    if (start >= 0) {
      const body: string[] = [];
      for (let i = start + 1; i < lines.length; i++) {
        const line = lines[i];
        if (/^[-=]{3,}/.test(line.trim())) break;
        if (/^\* [A-Za-z].*\[P\d/i.test(line.trim())) break;
        if (/^[A-Z][a-z]+ report:/i.test(line.trim())) break;
        if (/^Contract reports:/i.test(line.trim())) break;
        if (line.trim()) body.push(line.trim());
      }
      if (body.length) return body.join('\n');
    }
  }
  for (const source of sources) {
    const fauna = source.split('\n').filter(
      (l) => /hostile fauna|has hostile fauna/i.test(l) && l.trim().length > 20
    );
    if (fauna.length) return fauna.map((l) => l.trim()).join('\n');
  }
  return '';
}

/** Stances, press, and rumors from report sections. */
export function extractDiplomacyText(sections: ReportSection[], fullText = ''): string {
  const parts: string[] = [];
  const events = sectionText(sections, 'events', '');
  const galaxy = sectionText(sections, 'galaxy', '');
  const press = sectionText(sections, 'press', '');

  parts.push(extractStancesText(events, fullText, galaxy));

  if (press.trim() && !/^Press releases:\s*$/i.test(press.trim())) {
    parts.push(press.trim());
  }

  const rumors = extractRumorsBlock(events, galaxy);
  if (rumors.trim()) {
    parts.push(`Rumors:\n${rumors.trim()}`);
  }

  return parts.join('\n\n');
}

export function splitTechnologyReport(text: string): { known: string; breakthrough: string } {
  if (!text.trim()) {
    return { known: '', breakthrough: 'No breakthrough this turn.' };
  }
  const lines = text.split('\n');
  const header = lines.findIndex((l) => /^Technology reports:/i.test(l));
  if (header < 0) return { known: '', breakthrough: text.trim() };
  const body = lines.slice(header + 1);
  const breakthroughIdx = body.findIndex((l) => /^\+/.test(l.trim()));
  if (breakthroughIdx < 0) {
    return { known: '', breakthrough: 'No breakthrough this turn.' };
  }
  return {
    known: '',
    breakthrough: body.slice(breakthroughIdx).join('\n').trim(),
  };
}

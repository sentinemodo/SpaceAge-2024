const SECTION_MARKERS = [
  { id: 'header', title: 'Report header', match: (line) => /^SpaceAge report for /i.test(line) || /^Turn \d+/i.test(line) },
  { id: 'events', title: 'Events during turn', match: (line) => /^Events during turn:/i.test(line) },
  { id: 'bank', title: 'Bank report', match: (line) => /^Bank report:/i.test(line) },
  { id: 'survey', title: 'Survey reports', match: (line) => /^Survey reports:/i.test(line) },
  { id: 'technology', title: 'Technology reports', match: (line) => /^Technology reports:/i.test(line) },
  { id: 'battles', title: 'Battle reports', match: (line) => /^Battles? during turn:/i.test(line) || /^Battle report:/i.test(line) || /^Battles report:/i.test(line) },
  { id: 'press', title: 'Press releases', match: (line) => /^Press releases:/i.test(line) },
  { id: 'contracts', title: 'Contracts', match: (line) => /^A new contract|^Contracts:/i.test(line) },
  { id: 'galaxy', title: 'Galaxy report', match: (line) => /^Galaxy report:/i.test(line) },
  { id: 'orders', title: 'Orders template', match: (line) => /^Orders Template:/i.test(line) },
];

function stripEmailHeaders(lines) {
  const start = lines.findIndex((line) => /^SpaceAge report for /i.test(line));
  if (start >= 0) return lines.slice(start);
  const engine = lines.findIndex((line) => /^SpaceAge Engine Version:/i.test(line));
  return engine >= 0 ? lines.slice(engine) : lines;
}

export function splitReportSections(text) {
  const lines = stripEmailHeaders(text.split(/\r?\n/));
  const sections = [];
  let current = { id: 'preamble', title: 'Preamble', lines: [] };

  for (const line of lines) {
    const marker = SECTION_MARKERS.find((entry) => entry.match(line));
    if (marker) {
      if (current.lines.length > 0) {
        sections.push({
          id: current.id,
          title: current.title,
          text: current.lines.join('\n').trimEnd(),
        });
      }
      current = { id: marker.id, title: marker.title, lines: [line] };
      continue;
    }
    current.lines.push(line);
  }

  if (current.lines.length > 0) {
    sections.push({
      id: current.id,
      title: current.title,
      text: current.lines.join('\n').trimEnd(),
    });
  }

  return sections.filter((section) => section.text.length > 0);
}

const STACK_HEADER = /\[([A-Za-z0-9]+)\]/;
const SECTION_BREAK =
  /^(?:\* |[A-Z][a-z]+ report:|Rumors:|Orders Template:|Events during turn:|Bank report:|-{3,}|\s*\+?\s*[A-Za-z].*\[[SP]\d)/;

/** Parse indented detail blocks from galaxy report text keyed by stack id. */
export function parseStackDetails(galaxyText: string): Map<string, string> {
  const details = new Map<string, string>();
  if (!galaxyText.trim()) return details;

  const lines = galaxyText.split('\n');
  let currentId: string | null = null;
  let buffer: string[] = [];

  const flush = () => {
    if (currentId && buffer.length) {
      details.set(currentId, buffer.join('\n').trim());
    }
    buffer = [];
  };

  for (const rawLine of lines) {
    const line = rawLine.trimEnd();
    const trimmed = line.trim();
    if (!trimmed) continue;

    const stackIdMatch = trimmed.match(/^[+\-]?\s*.*?\[(\d+)\]/);
    if (stackIdMatch && (trimmed.includes('immobile') || trimmed.includes('owned by') || trimmed.startsWith('+'))) {
      flush();
      currentId = stackIdMatch[1];
      continue;
    }

    if (currentId && /^\s{2,}/.test(line) && !SECTION_BREAK.test(trimmed)) {
      if (/^\s*(size:|mass:|capacity:|upkeep:|items:|technologies:|energy:|crew:|consume:)/i.test(trimmed)) {
        buffer.push(trimmed);
      } else if (buffer.length > 0 && /^\s+[a-z]/i.test(line)) {
        buffer[buffer.length - 1] += ` ${trimmed}`;
      }
      continue;
    }

    if (SECTION_BREAK.test(trimmed) || (stackIdMatch && currentId)) {
      flush();
      currentId = null;
    }
  }
  flush();
  return details;
}

export function attachStackDetails<T extends { id: string; children: T[]; reportDetail?: string }>(
  stacks: T[],
  details: Map<string, string>
): T[] {
  return stacks.map((s) => ({
    ...s,
    reportDetail: details.get(s.id) || s.reportDetail,
    children: attachStackDetails(s.children, details),
  }));
}

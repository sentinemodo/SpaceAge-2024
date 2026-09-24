import type { ParsedReport } from '../parsers/reportXml';
import { flattenStacks } from '../parsers/reportXml';
import { parseOrdersTemplate } from './orderTemplate';

const MOVE_LINE = /^(?:\+\+|--|[+-])?\s*(?:(\d+)\s+)?move\b\s*(.*)$/i;

export type MovementRow = {
  id: string;
  name: string;
  location?: string;
  mass?: string;
  route: string;
  repeat?: string;
  hasMove: boolean;
};

function regionLabel(report: ParsedReport, id: string): string {
  const region = report.regions.find((r) => r.id === id);
  if (region) return region.name;
  return id;
}

function moveFromLines(lines: string[]): { destinations: string[]; repeat?: string } | null {
  const destinations: string[] = [];
  let repeat: string | undefined;
  let found = false;
  for (const line of lines) {
    const match = line.trim().split(';')[0].trim().match(MOVE_LINE);
    if (!match) continue;
    found = true;
    if (!repeat && match[1]) repeat = match[1];
    for (const token of match[2].split(/\s+/)) {
      if (token) destinations.push(token);
    }
  }
  return found ? { destinations, repeat } : null;
}

/** Module stacks whose current order text contains a MOVE line. */
export function movementRowsFromOrders(report: ParsedReport, ordersText: string): MovementRow[] {
  const flat = flattenStacks(report.stacks);
  const rows: MovementRow[] = [];

  for (const block of parseOrdersTemplate(ordersText).blocks) {
    if (block.kind !== 'modulestack') continue;
    const move = moveFromLines(block.lines);
    if (!move) continue;
    const stack = flat.find((entry) => entry.id === block.id);
    const routeLabels = move.destinations.map((id) => regionLabel(report, id));
    rows.push({
      id: block.id,
      name: stack?.name || block.id,
      location: stack?.locationName || stack?.locationId,
      mass: stack?.mass,
      route: routeLabels.length ? routeLabels.join(' → ') : '—',
      repeat: move.repeat,
      hasMove: true,
    });
  }

  rows.sort((a, b) => a.name.localeCompare(b.name));
  return rows;
}

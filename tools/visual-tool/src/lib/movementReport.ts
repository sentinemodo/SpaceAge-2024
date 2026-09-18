import type { ParsedReport } from '../parsers/reportXml';
import { findOrderForStack, flattenStacks } from '../parsers/reportXml';

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

export function movementRowsFromReport(report: ParsedReport): MovementRow[] {
  const flat = flattenStacks(report.stacks).filter(
    (s) => !report.factionId || s.faction === report.factionId,
  );
  const seen = new Set<string>();
  const rows: MovementRow[] = [];

  for (const stack of flat) {
    seen.add(stack.id);
    const order = findOrderForStack(report.orders, stack.id);
    const routeIds = order?.moveDestinations ?? [];
    const routeLabels = routeIds.map((id) => regionLabel(report, id));
    rows.push({
      id: stack.id,
      name: stack.name,
      location: stack.locationName || stack.locationId,
      mass: stack.mass,
      route: routeLabels.length ? routeLabels.join(' → ') : '—',
      repeat: order?.repeat,
      hasMove: routeIds.length > 0,
    });
  }

  for (const order of report.orders) {
    if (order.moveDestinations.length === 0) continue;
    if (order.subject !== 'modulestack' || seen.has(order.targetId)) continue;
    const routeLabels = order.moveDestinations.map((id) => regionLabel(report, id));
    rows.push({
      id: order.targetId,
      name: order.targetId,
      route: routeLabels.join(' → '),
      repeat: order.repeat,
      hasMove: true,
    });
  }

  rows.sort((a, b) => {
    if (a.hasMove !== b.hasMove) return a.hasMove ? -1 : 1;
    return a.name.localeCompare(b.name);
  });

  return rows;
}

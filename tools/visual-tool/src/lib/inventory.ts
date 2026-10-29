import type { StackNode } from '../parsers/reportXml';

export type InventoryScope = 'direct' | 'subtree';

export interface ReportedItem {
  type: string;
  label: string;
  quantity: number;
  size: number | null;
  mass: number | null;
}

export interface InventoryRow extends ReportedItem {
  /** How many visited stacks hold this item type. */
  holderCount: number;
}

export interface InventoryTotals {
  quantity: number;
  size: number;
  mass: number;
  types: number;
  /** Stacks (within scope) holding at least one item. */
  holders: number;
}

export interface Inventory {
  rows: InventoryRow[];
  totals: InventoryTotals;
  /** Capacity of the selected stack itself (`capacity: total/used`), if reported. */
  capacity: { total: number; used: number } | null;
}

/** Split on commas that are not inside parentheses (size/mass/upkeep details). */
function splitTopLevel(text: string): string[] {
  const parts: string[] = [];
  let depth = 0;
  let start = 0;
  for (let i = 0; i < text.length; i += 1) {
    const ch = text[i];
    if (ch === '(') depth += 1;
    else if (ch === ')') depth = Math.max(0, depth - 1);
    else if (ch === ',' && depth === 0) {
      parts.push(text.slice(start, i));
      start = i + 1;
    }
  }
  parts.push(text.slice(start));
  return parts.map((p) => p.trim()).filter(Boolean);
}

function parseItemEntry(entry: string): ReportedItem | null {
  const trimmed = entry.replace(/\.$/, '').trim();
  const match = trimmed.match(/^(.*?)\s*\[([^\]]+)\]\s*(?:\((.*)\))?$/);
  if (!match) return null;
  const [, namePart, type, details] = match;

  let quantity = 1;
  let label = namePart.trim();
  const units = label.match(/^(\d+)\s+units?\s+of\s+(.+)$/i);
  const counted = label.match(/^(\d+)\s+(.+)$/);
  if (units) {
    quantity = parseInt(units[1], 10);
    label = units[2].trim();
  } else if (/^unit\s+of\s+/i.test(label)) {
    label = label.replace(/^unit\s+of\s+/i, '').trim();
  } else if (counted) {
    quantity = parseInt(counted[1], 10);
    label = counted[2].trim();
  }

  const sizeMatch = details?.match(/size:\s*([\d.]+)/i);
  const massMatch = details?.match(/mass:\s*([\d.]+)/i);
  return {
    type: type.trim(),
    label,
    quantity,
    size: sizeMatch ? Number(sizeMatch[1]) : null,
    mass: massMatch ? Number(massMatch[1]) : null,
  };
}

/**
 * Parse the `items:` line of a stack's galaxy-report detail block, e.g.
 * `items: 400 units of food [food] (size: 400, mass: 400), 20 terrans [terran] (size: 80, mass: 80).`
 * Size/mass are totals for that item stack as printed by the engine.
 */
export function parseReportedItems(reportDetail?: string): ReportedItem[] {
  if (!reportDetail) return [];
  const line = reportDetail.split('\n').find((l) => /^\s*items:/i.test(l));
  if (!line) return [];
  const body = line.replace(/^\s*items:/i, '').trim();
  if (!body) return [];
  return splitTopLevel(body)
    .map(parseItemEntry)
    .filter((item): item is ReportedItem => item != null);
}

export function parseCapacity(reportDetail?: string): { total: number; used: number } | null {
  const match = reportDetail?.match(/capacity:\s*([\d.]+)\s*\/\s*([\d.]+)/i);
  if (!match) return null;
  return { total: Number(match[1]), used: Number(match[2]) };
}

function collectStacks(root: StackNode, scope: InventoryScope): StackNode[] {
  if (scope === 'direct') return [root];
  const out: StackNode[] = [];
  const walk = (node: StackNode) => {
    out.push(node);
    node.children.forEach(walk);
  };
  walk(root);
  return out;
}

function itemsOf(stack: StackNode): ReportedItem[] {
  const reported = parseReportedItems(stack.reportDetail);
  if (reported.length > 0) return reported;
  // XML `<itemstack>` gives quantity only — size/mass stay unknown (null).
  return (stack.items ?? []).map((item) => ({
    type: item.type,
    label: item.type,
    quantity: item.quantity,
    size: null,
    mass: null,
  }));
}

/**
 * Flat ledger for a stack: one row per item type. `direct` counts only the
 * selected stack; `subtree` recursively merges all descendant module stacks.
 */
export function aggregateInventory(root: StackNode, scope: InventoryScope): Inventory {
  const byType = new Map<string, InventoryRow>();
  let holders = 0;

  for (const stack of collectStacks(root, scope)) {
    const items = itemsOf(stack);
    if (items.length > 0) holders += 1;
    for (const item of items) {
      const row = byType.get(item.type) ?? {
        type: item.type,
        label: item.label,
        quantity: 0,
        size: null,
        mass: null,
        holderCount: 0,
      };
      row.quantity += item.quantity;
      // Sum known size/mass; stays null only if no holder reported one.
      if (item.size != null) row.size = (row.size ?? 0) + item.size;
      if (item.mass != null) row.mass = (row.mass ?? 0) + item.mass;
      row.holderCount += 1;
      byType.set(item.type, row);
    }
  }

  const rows = [...byType.values()].sort(
    (a, b) => b.quantity - a.quantity || a.type.localeCompare(b.type),
  );
  return {
    rows,
    totals: {
      quantity: rows.reduce((sum, r) => sum + r.quantity, 0),
      size: rows.reduce((sum, r) => sum + (r.size ?? 0), 0),
      mass: rows.reduce((sum, r) => sum + (r.mass ?? 0), 0),
      types: rows.length,
      holders,
    },
    capacity: parseCapacity(root.reportDetail),
  };
}

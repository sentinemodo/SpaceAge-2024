export interface StackNode {
  id: string;
  name: string;
  type?: string;
  locationId?: string;
  locationName?: string;
  systemId?: string;
  quantity?: number;
  children: StackNode[];
  hp?: string;
  mass?: string;
}

export interface SystemNode {
  id: string;
  name: string;
  x: number;
  y: number;
}

export interface RegionNode {
  id: string;
  name: string;
  systemId: string;
  x?: number;
  y?: number;
}

export interface ParsedOrderEntry {
  subject: string;
  targetId: string;
  repeat?: string;
  moveDestinations: string[];
}

export interface BattleSummary {
  id: string;
  text: string;
}

export interface FactionSummary {
  upkeep?: string;
  research?: string;
  press: string[];
  contracts: string[];
}

export interface ParsedReport {
  turn: number;
  factionName: string;
  stacks: StackNode[];
  systems: SystemNode[];
  regions: RegionNode[];
  orders: ParsedOrderEntry[];
  technologies: string[];
  battles: BattleSummary[];
  faction: FactionSummary;
  bank: string[];
  diplomacy: string[];
}

interface LocationContext {
  systemId: string;
  systemName?: string;
  locationId?: string;
  locationName?: string;
}

function text(el: Element | null): string {
  return el?.textContent?.trim() || '';
}

function attr(el: Element | null, ...names: string[]): string | undefined {
  if (!el) return undefined;
  for (const name of names) {
    const value = el.getAttribute(name);
    if (value) return value;
  }
  return undefined;
}

function parseStackEl(el: Element, ctx: LocationContext): StackNode {
  const id = attr(el, 'name', 'id') || '';
  return {
    id,
    name: attr(el, 'name-en') || attr(el, 'type') || id,
    type: el.getAttribute('type') || undefined,
    locationId: ctx.locationId,
    locationName: ctx.locationName,
    systemId: ctx.systemId,
    quantity: parseInt(el.getAttribute('quantity') || '1', 10),
    hp: text(el.querySelector(':scope > hit-points')) || undefined,
    mass: text(el.querySelector(':scope > mass')) || undefined,
    children: [...el.querySelectorAll(':scope > modulestack')].map((child) =>
      parseStackEl(child, ctx)
    ),
  };
}

function walkLocation(
  el: Element,
  ctx: LocationContext,
  stacks: StackNode[],
  regions: RegionNode[]
) {
  const tag = el.tagName.toLowerCase();

  if (tag === 'system') {
    const systemId = attr(el, 'name', 'id') || '';
    ctx = {
      systemId,
      systemName: attr(el, 'name-en') || systemId,
      locationId: systemId,
      locationName: attr(el, 'name-en') || systemId,
    };
  }

  if (tag === 'region') {
    const regionId = attr(el, 'name', 'id') || '';
    const regionName = attr(el, 'name-en') || regionId;
    regions.push({
      id: regionId,
      name: regionName,
      systemId: ctx.systemId,
      x: parseOptionalInt(el.getAttribute('X')),
      y: parseOptionalInt(el.getAttribute('Y')),
    });
    ctx = { ...ctx, locationId: regionId, locationName: regionName };
    for (const ms of el.querySelectorAll(':scope > modulestack')) {
      stacks.push(parseStackEl(ms, ctx));
    }
  }

  if (tag === 'orbit') {
    const orbitId = attr(el, 'name', 'id') || '';
    const orbitCtx = {
      ...ctx,
      locationId: orbitId,
      locationName: orbitId ? `orbit ${orbitId}` : 'orbit',
    };
    for (const ms of el.querySelectorAll(':scope > modulestack')) {
      stacks.push(parseStackEl(ms, orbitCtx));
    }
  }

  for (const child of el.children) {
    const childTag = child.tagName.toLowerCase();
    if (['system', 'planet', 'moon', 'orbit', 'region'].includes(childTag)) {
      walkLocation(child, ctx, stacks, regions);
    }
  }
}

function parseOptionalInt(value: string | null): number | undefined {
  if (value == null || value === '') return undefined;
  const n = parseInt(value, 10);
  return Number.isNaN(n) ? undefined : n;
}

function normalizeSystems(raw: { id: string; name: string; rawX: number; rawY: number }[]): SystemNode[] {
  if (raw.length === 0) return [];
  const xs = raw.map((s) => s.rawX);
  const ys = raw.map((s) => s.rawY);
  const minX = Math.min(...xs);
  const maxX = Math.max(...xs);
  const minY = Math.min(...ys);
  const maxY = Math.max(...ys);
  const spanX = maxX - minX || 1;
  const spanY = maxY - minY || 1;

  return raw.map((s) => ({
    id: s.id,
    name: s.name,
    x: 15 + (70 * (s.rawX - minX)) / spanX,
    y: 15 + (70 * (s.rawY - minY)) / spanY,
  }));
}

function parseSystems(root: Element): SystemNode[] {
  const galaxy = root.querySelector('galaxy');
  if (!galaxy) return [];

  const raw: { id: string; name: string; rawX: number; rawY: number }[] = [];
  galaxy.querySelectorAll(':scope > system').forEach((el, i) => {
    raw.push({
      id: attr(el, 'name', 'id') || `S${i}`,
      name: attr(el, 'name-en') || attr(el, 'name') || `System ${i}`,
      rawX: parseFloat(attr(el, 'X', 'x') || String(i * 30)),
      rawY: parseFloat(attr(el, 'Y', 'y') || String(20 + (i % 3) * 15)),
    });
  });
  return normalizeSystems(raw);
}

function parseOrders(root: Element): ParsedOrderEntry[] {
  const orders: ParsedOrderEntry[] = [];
  root.querySelectorAll('orders > order').forEach((el) => {
    const moveEl = el.querySelector('move');
    const destinations = moveEl
      ? [...moveEl.querySelectorAll('destination')]
          .map((d) => d.getAttribute('destination') || '')
          .filter(Boolean)
      : [];
    orders.push({
      subject: el.getAttribute('subject') || 'modulestack',
      targetId: el.getAttribute('name') || '',
      repeat: el.getAttribute('repeat') || undefined,
      moveDestinations: destinations,
    });
  });
  return orders;
}

export function parseReportXml(xml: string): ParsedReport {
  const doc = new DOMParser().parseFromString(xml, 'application/xml');
  const root = doc.documentElement;
  const turn = parseInt(root.getAttribute('turn') || '1', 10);
  const factionEl = root.querySelector('faction');
  const factionName =
    text(root.querySelector('faction-name')) ||
    attr(factionEl, 'name-en') ||
    text(root.querySelector('faction')) ||
    '';

  const systems = parseSystems(root);
  const stacks: StackNode[] = [];
  const regions: RegionNode[] = [];
  const galaxy = root.querySelector('galaxy');
  if (galaxy) {
    for (const systemEl of galaxy.querySelectorAll(':scope > system')) {
      const systemId = attr(systemEl, 'name', 'id') || '';
      walkLocation(systemEl, {
        systemId,
        systemName: attr(systemEl, 'name-en') || systemId,
        locationId: systemId,
        locationName: attr(systemEl, 'name-en') || systemId,
      }, stacks, regions);
    }
  }

  const technologies: string[] = [];
  root.querySelectorAll('technology, tech').forEach((el) => {
    const t = attr(el, 'name-en') || text(el) || el.getAttribute('name') || '';
    if (t) technologies.push(t);
  });

  const battles: BattleSummary[] = [];
  root.querySelectorAll('battle, combat').forEach((el, i) => {
    battles.push({ id: `b${i}`, text: el.textContent?.trim().slice(0, 500) || '' });
  });

  const press: string[] = [];
  root.querySelectorAll('press, press-release').forEach((el) => {
    if (el.textContent?.trim()) press.push(el.textContent.trim());
  });

  const contracts: string[] = [];
  root.querySelectorAll('contract').forEach((el) => {
    contracts.push(el.textContent?.trim() || el.getAttribute('id') || '');
  });

  const bank: string[] = [];
  root.querySelectorAll('bank, bank-report').forEach((el) => {
    bank.push(el.textContent?.trim() || '');
  });

  const diplomacy: string[] = [];
  root.querySelectorAll('diplomacy, attitude, declare').forEach((el) => {
    diplomacy.push(el.textContent?.trim() || '');
  });

  const upkeep = text(root.querySelector('upkeep, maintenance'));
  const research = text(root.querySelector('research, research-output'));

  return {
    turn,
    factionName,
    stacks,
    systems,
    regions,
    orders: parseOrders(root),
    technologies,
    battles,
    faction: { upkeep, research, press, contracts },
    bank,
    diplomacy,
  };
}

export function flattenStacks(stacks: StackNode[]): StackNode[] {
  const out: StackNode[] = [];
  function walk(n: StackNode) {
    out.push(n);
    n.children.forEach(walk);
  }
  stacks.forEach(walk);
  return out;
}

export function filterStacksBySystem(stacks: StackNode[], systemId: string | null): StackNode[] {
  if (!systemId) return stacks;
  return stacks.filter((s) => s.systemId === systemId || s.locationId === systemId);
}

export function findOrderForStack(orders: ParsedOrderEntry[], stackId: string): ParsedOrderEntry | undefined {
  return orders.find((o) => o.targetId === stackId);
}

export function estimateMoveWeeks(massLine?: string): number | null {
  if (!massLine) return null;
  const m = massLine.match(/(\d+)\s*\/\s*(\d+)/);
  if (!m) return null;
  const thrust = parseInt(m[1], 10);
  const mass = parseInt(m[2], 10);
  const load = thrust / Math.max(mass, 1);
  const ref = 40000 / 4150;
  const factor = Math.min(1.5, Math.max(0.67, load / ref));
  const speed = 1 * factor;
  const deltaAu = 1;
  const weeksAtOne = 8 + 6 * (deltaAu / (deltaAu + 0.8));
  return Math.ceil(weeksAtOne / speed);
}

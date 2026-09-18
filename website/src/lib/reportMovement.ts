/** Minimal XML report parse for movement listing (browser-safe subset of visual-tool parser). */

export type StackRow = {
  id: string;
  name: string;
  location?: string;
  mass?: string;
  faction?: string;
};

export type MovementRow = StackRow & {
  route: string;
  repeat?: string;
  hasMove: boolean;
};

export type MovementReport = {
  turn: number;
  factionId: string;
  factionName: string;
  rows: MovementRow[];
};

type ParsedOrder = {
  subject: string;
  targetId: string;
  repeat?: string;
  moveDestinations: string[];
};

function attr(el: Element | null, ...names: string[]): string | undefined {
  if (!el) return undefined;
  for (const name of names) {
    const value = el.getAttribute(name);
    if (value) return value;
  }
  return undefined;
}

function text(el: Element | null): string {
  return el?.textContent?.trim() || '';
}

type LocationContext = {
  systemId: string;
  locationId?: string;
  locationName?: string;
};

function parseStackEl(el: Element, ctx: LocationContext): StackRow & { children: (StackRow & { children: unknown[] })[] } {
  const id = attr(el, 'name', 'id') || '';
  const childEls = [...el.querySelectorAll(':scope > modulestack')];
  return {
    id,
    name: attr(el, 'name-en') || attr(el, 'type') || id,
    location: ctx.locationName || ctx.locationId,
    mass: text(el.querySelector(':scope > mass')) || undefined,
    faction: el.getAttribute('faction') || undefined,
    children: childEls.map((child) => parseStackEl(child, ctx)),
  };
}

function walkLocation(el: Element, ctx: LocationContext, stacks: (StackRow & { children: unknown[] })[]) {
  const tag = el.tagName.toLowerCase();

  if (tag === 'system') {
    const systemId = attr(el, 'name', 'id') || '';
    ctx = {
      systemId,
      locationId: systemId,
      locationName: attr(el, 'name-en') || systemId,
    };
  }

  if (tag === 'region') {
    const regionId = attr(el, 'name', 'id') || '';
    const regionName = attr(el, 'name-en') || regionId;
    ctx = { ...ctx, locationId: regionId, locationName: regionName };
    for (const ms of el.querySelectorAll(':scope > modulestack')) {
      stacks.push(parseStackEl(ms, ctx));
    }
  }

  if (tag === 'orbit') {
    const orbitId = attr(el, 'name', 'id') || '';
    ctx = {
      ...ctx,
      locationId: orbitId,
      locationName: orbitId ? `orbit ${orbitId}` : 'orbit',
    };
    for (const ms of el.querySelectorAll(':scope > modulestack')) {
      stacks.push(parseStackEl(ms, ctx));
    }
  }

  for (const child of el.children) {
    const childTag = child.tagName.toLowerCase();
    if (['system', 'planet', 'moon', 'belt', 'alderson', 'orbit', 'region'].includes(childTag)) {
      walkLocation(child, ctx, stacks);
    }
  }
}

function flattenStacks(
  stacks: (StackRow & { children: unknown[] })[],
): StackRow[] {
  const out: StackRow[] = [];
  for (const stack of stacks) {
    const { children, ...row } = stack;
    out.push(row);
    out.push(...flattenStacks(children as (StackRow & { children: unknown[] })[]));
  }
  return out;
}

function parseOrders(root: Element): ParsedOrder[] {
  const orders: ParsedOrder[] = [];
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

function regionLabel(regions: Map<string, string>, id: string): string {
  return regions.get(id) || id;
}

function buildRegionMap(root: Element): Map<string, string> {
  const map = new Map<string, string>();
  root.querySelectorAll('region').forEach((el) => {
    const id = attr(el, 'name', 'id');
    if (!id) return;
    map.set(id, attr(el, 'name-en') || id);
  });
  root.querySelectorAll('orbit').forEach((el) => {
    const id = attr(el, 'name', 'id');
    if (!id) return;
    map.set(id, `orbit ${id}`);
  });
  return map;
}

export function parseMovementReport(xml: string): MovementReport {
  const doc = new DOMParser().parseFromString(xml, 'application/xml');
  const root = doc.documentElement;
  if (root.querySelector('parsererror')) {
    throw new Error('Invalid XML report');
  }

  const turn = parseInt(root.getAttribute('turn') || '1', 10);
  const factionEl = root.querySelector('faction');
  const factionId = attr(factionEl, 'name', 'id') || '';
  const factionName =
    text(root.querySelector('faction-name')) ||
    attr(factionEl, 'name-en') ||
    factionId;

  const stackTree: (StackRow & { children: unknown[] })[] = [];
  const galaxy = root.querySelector('galaxy');
  if (galaxy) {
    for (const systemEl of galaxy.querySelectorAll(':scope > system')) {
      const systemId = attr(systemEl, 'name', 'id') || '';
      walkLocation(systemEl, {
        systemId,
        locationId: systemId,
        locationName: attr(systemEl, 'name-en') || systemId,
      }, stackTree);
    }
  }

  const orders = parseOrders(root);
  const moveOrders = orders.filter((o) => o.moveDestinations.length > 0);
  const orderByStack = new Map(
    moveOrders.filter((o) => o.subject === 'modulestack').map((o) => [o.targetId, o]),
  );

  const regionMap = buildRegionMap(root);
  const allStacks = flattenStacks(stackTree).filter(
    (s) => !factionId || s.faction === factionId,
  );

  const seen = new Set<string>();
  const rows: MovementRow[] = [];

  for (const stack of allStacks) {
    seen.add(stack.id);
    const order = orderByStack.get(stack.id);
    const routeIds = order?.moveDestinations ?? [];
    const routeLabels = routeIds.map((id) => regionLabel(regionMap, id));
    rows.push({
      ...stack,
      route: routeLabels.length ? routeLabels.join(' → ') : '—',
      repeat: order?.repeat,
      hasMove: routeLabels.length > 0,
    });
  }

  for (const order of moveOrders) {
    if (order.subject !== 'modulestack' || seen.has(order.targetId)) continue;
    const routeLabels = order.moveDestinations.map((id) => regionLabel(regionMap, id));
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

  return { turn, factionId, factionName, rows };
}

export function parseMassFromLine(mass?: string): { thrust: number; mass: number } | null {
  if (!mass) return null;
  const m = mass.match(/(\d+)\s*\/\s*(\d+)/);
  if (!m) return null;
  return { thrust: parseInt(m[1], 10), mass: parseInt(m[2], 10) };
}

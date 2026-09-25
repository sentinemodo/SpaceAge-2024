export interface ResourceEntry {
  type: string;
  quantity: number;
}

export interface ExitTargetStub {
  id: string;
  name?: string;
  x?: number;
  y?: number;
  terrainType?: string;
  discovered?: 'partial' | 'yes' | 'no';
  capacities?: CapacityEntry[];
  resources?: ResourceEntry[];
  deepResources?: ResourceEntry[];
  anomaly?: RegionAnomaly;
}

export interface ExitEntry {
  targetId: string;
  targetKind: 'region' | 'belt' | 'alderson' | 'orbit';
  mode?: string;
  duration?: number;
  target?: ExitTargetStub;
}

export interface CapacityEntry {
  group: string;
  quantity: number;
}

export interface RegionAnomaly {
  type: string;
  points?: number;
  description?: string;
  /** Faction ids that have finished investigating this anomaly. */
  resolvedFactions?: string[];
}

export interface PersonNode {
  id: string;
  name: string;
  race?: string;
  faction?: string;
  upkeep: { type: string; quantity: number }[];
}

export interface StackNode {
  id: string;
  name: string;
  type?: string;
  locationId?: string;
  locationName?: string;
  systemId?: string;
  quantity?: number;
  faction?: string;
  children: StackNode[];
  hp?: string;
  mass?: string;
  upkeep: { type: string; quantity: number }[];
  producesEnergy?: boolean;
  /** Installed module count (`quantity` or `<module>` elements), not child modulestacks. */
  moduleCount: number;
  persons: PersonNode[];
  /** Galaxy report text line, e.g. `+ small cargo bay [230003], 2 small cargo bays [cargob], immobile.` */
  reportLine?: string;
  /** Indented stats/items block from galaxy report text. */
  reportDetail?: string;
}

export interface StarNode {
  id: string;
  name: string;
  starType?: string;
  unexplored?: boolean;
  description?: string;
}

export interface SystemNode {
  id: string;
  name: string;
  x: number;
  y: number;
}

export interface AldersonLink {
  fromSystemId: string;
  toSystemId: string;
  /** Solid line (false = unstable wormhole, dashed) */
  stable: boolean;
  /** Helios↔Fomal home pair — brightest styling */
  homePair: boolean;
}

export interface RegionNode {
  id: string;
  name: string;
  systemId: string;
  bodyId?: string;
  x?: number;
  y?: number;
  terrainType?: string;
  description?: string;
  resources?: ResourceEntry[];
  exits?: ExitEntry[];
  capacities?: CapacityEntry[];
  deepResources?: ResourceEntry[];
  anomaly?: RegionAnomaly;
}

export interface OrbitDetail {
  id: string;
  bodyId: string;
  systemId: string;
  resources: ResourceEntry[];
  exits: ExitEntry[];
}

export interface SystemBodyNode {
  id: string;
  name: string;
  kind: 'planet' | 'moon' | 'belt' | 'alderson';
  au: number;
  surfaceW?: number;
  surfaceH?: number;
  parentId?: string;
  planetType?: string;
  gravity?: string;
  temperature?: string;
  atmosphere?: string;
  description?: string;
  unexplored?: boolean;
  diameter?: number;
  regions: RegionNode[];
  orbitIds: string[];
  orbits: OrbitDetail[];
}

export interface RegionMapCell extends RegionNode {
  hasPresence: boolean;
  reachableViaExit: boolean;
}

export interface SystemDetail {
  id: string;
  name: string;
  starName?: string;
  star?: StarNode;
  surveyFlavor?: string;
  bodies: SystemBodyNode[];
}

export interface OrderVerb {
  kind: string;
  detail?: string;
}

export interface ParsedOrderEntry {
  subject: string;
  targetId: string;
  repeat?: string;
  moveDestinations: string[];
  verbs: OrderVerb[];
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
  factionId: string;
  factionName: string;
  stacks: StackNode[];
  systems: SystemNode[];
  aldersonLinks: AldersonLink[];
  regions: RegionNode[];
  systemDetails: SystemDetail[];
  orders: ParsedOrderEntry[];
  technologies: string[];
  battles: BattleSummary[];
  faction: FactionSummary;
  bank: string[];
  diplomacy: string[];
  /** Region coords/names parsed from galaxy report text (exit neighbours, etc.) */
  regionHints: RegionNode[];
  /** Full galaxy-report lines keyed by region id. */
  regionReportLines: Record<string, string>;
}

interface LocationContext {
  systemId: string;
  systemName?: string;
  bodyId?: string;
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

function parseResources(parent: Element): ResourceEntry[] {
  return [...parent.querySelectorAll(':scope > resource')].map((r) => ({
    type: r.getAttribute('type') || '',
    quantity: parseInt(r.getAttribute('quantity') || '0', 10),
  }));
}

function parseCapacities(parent: Element): CapacityEntry[] {
  return [...parent.querySelectorAll(':scope > capacity')].map((c) => ({
    group: c.getAttribute('group') || '',
    quantity: parseInt(c.getAttribute('quantity') || '0', 10),
  }));
}

function parseAnomaly(el: Element | null): RegionAnomaly | undefined {
  if (!el) return undefined;
  const type = el.getAttribute('type') || '';
  if (!type) return undefined;
  const resolvedFactions = [...el.querySelectorAll(':scope > resolved')]
    .map((node) => node.getAttribute('faction') || '')
    .filter(Boolean);
  return {
    type,
    points: parseOptionalInt(el.getAttribute('points')),
    description: el.getAttribute('description') || undefined,
    resolvedFactions: resolvedFactions.length ? resolvedFactions : undefined,
  };
}

function parseExitTarget(ex: Element): ExitTargetStub | undefined {
  const targetEl = ex.querySelector(':scope > target');
  if (!targetEl) return undefined;
  const id = attr(targetEl, 'name', 'id') || ex.getAttribute('region') || '';
  if (!id) return undefined;
  const discovered = targetEl.getAttribute('discovered') as ExitTargetStub['discovered'] | null;
  const deepEl = targetEl.querySelector(':scope > deep-pocket');
  return {
    id,
    name: attr(targetEl, 'name-en') || undefined,
    x: parseOptionalInt(targetEl.getAttribute('X')),
    y: parseOptionalInt(targetEl.getAttribute('Y')),
    terrainType: targetEl.getAttribute('type') || undefined,
    discovered: discovered || undefined,
    capacities: parseCapacities(targetEl),
    resources: parseResources(targetEl),
    deepResources: deepEl ? parseResources(deepEl) : undefined,
    anomaly: parseAnomaly(targetEl.querySelector(':scope > anomaly')),
  };
}

function parseExits(parent: Element): ExitEntry[] {
  const exits: ExitEntry[] = [];
  for (const ex of parent.querySelectorAll(':scope > exit')) {
    const modeEl = ex.querySelector('exitmode');
    const target = parseExitTarget(ex);
    const base = {
      mode: modeEl?.getAttribute('mode') || undefined,
      duration: parseOptionalInt(modeEl?.getAttribute('duration') || null),
      target,
    };
    if (ex.hasAttribute('region')) {
      exits.push({ targetId: ex.getAttribute('region')!, targetKind: 'region', ...base });
    } else if (ex.hasAttribute('belt')) {
      exits.push({ targetId: ex.getAttribute('belt')!, targetKind: 'belt', ...base });
    } else if (ex.hasAttribute('alderson')) {
      exits.push({ targetId: ex.getAttribute('alderson')!, targetKind: 'alderson', ...base });
    } else if (ex.hasAttribute('orbit')) {
      exits.push({ targetId: ex.getAttribute('orbit')!, targetKind: 'orbit', ...base });
    }
  }
  return exits;
}

function parseUpkeep(el: Element): { type: string; quantity: number }[] {
  return [...el.querySelectorAll(':scope > upkeep')].map((u) => ({
    type: u.getAttribute('type') || '',
    quantity: parseInt(u.getAttribute('quantity') || '0', 10),
  }));
}

function parseDeepPocket(el: Element): ResourceEntry[] {
  const pocket = el.querySelector(':scope > deep-pocket');
  return pocket ? parseResources(pocket) : [];
}

function parseAnomalyEl(el: Element): RegionAnomaly | undefined {
  return parseAnomaly(el.querySelector(':scope > anomaly'));
}

function parsePersonEl(el: Element): PersonNode {
  return {
    id: attr(el, 'name', 'id') || '',
    name: attr(el, 'name-en') || attr(el, 'name') || '',
    race: el.getAttribute('race') || undefined,
    faction: el.getAttribute('faction') || undefined,
    upkeep: parseUpkeep(el),
  };
}

function moduleCountFromEl(el: Element): number {
  const moduleEls = el.querySelectorAll(':scope > module');
  if (moduleEls.length > 0) return moduleEls.length;
  return parseInt(el.getAttribute('quantity') || '1', 10);
}

function parseRegionEl(el: Element, systemId: string, bodyId?: string): RegionNode {
  return {
    id: attr(el, 'name', 'id') || '',
    name: attr(el, 'name-en') || attr(el, 'name') || '',
    systemId,
    bodyId,
    x: parseOptionalInt(el.getAttribute('X')),
    y: parseOptionalInt(el.getAttribute('Y')),
    terrainType: el.getAttribute('type') || undefined,
    description: el.getAttribute('description') || undefined,
    resources: parseResources(el),
    exits: parseExits(el),
    capacities: parseCapacities(el),
    deepResources: parseDeepPocket(el),
    anomaly: parseAnomalyEl(el),
  };
}

function parseOrbitEl(el: Element, systemId: string, bodyId: string): OrbitDetail {
  return {
    id: attr(el, 'name', 'id') || '',
    bodyId,
    systemId,
    resources: parseResources(el),
    exits: parseExits(el),
  };
}

function parseStackEl(el: Element, ctx: LocationContext): StackNode {
  const id = attr(el, 'name', 'id') || '';
  const childEls = [...el.querySelectorAll(':scope > modulestack')];
  return {
    id,
    name: attr(el, 'name-en') || attr(el, 'type') || id,
    type: el.getAttribute('type') || undefined,
    locationId: ctx.locationId,
    locationName: ctx.locationName,
    systemId: ctx.systemId,
    quantity: parseInt(el.getAttribute('quantity') || '1', 10),
    faction: el.getAttribute('faction') || undefined,
    hp: text(el.querySelector(':scope > hit-points')) || undefined,
    mass: text(el.querySelector(':scope > mass')) || undefined,
    upkeep: parseUpkeep(el),
    producesEnergy: el.querySelector(':scope > produce[produce-type="energy"]') != null,
    moduleCount: moduleCountFromEl(el),
    persons: [...el.querySelectorAll(':scope > person')].map(parsePersonEl),
    children: childEls.map((child) => parseStackEl(child, ctx)),
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
    const region = parseRegionEl(el, ctx.systemId, ctx.bodyId);
    regions.push(region);
    ctx = { ...ctx, locationId: region.id, locationName: region.name };
    for (const ms of el.querySelectorAll(':scope > modulestack')) {
      stacks.push(parseStackEl(ms, ctx));
    }
  }

  if (tag === 'planet' || tag === 'moon' || tag === 'belt' || tag === 'alderson') {
    const bodyId = attr(el, 'name', 'id');
    if (bodyId) ctx = { ...ctx, bodyId };
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
    if (['system', 'planet', 'moon', 'belt', 'alderson', 'orbit', 'region'].includes(childTag)) {
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
  const spanYRaw = maxY - minY;
  const midY = 50;

  if (spanYRaw === 0 && raw.length > 1) {
    const sorted = [...raw].sort((a, b) => a.rawX - b.rawX || a.id.localeCompare(b.id));
    const yById = new Map<string, number>();
    sorted.forEach((s, i) => {
      yById.set(s.id, 10 + (80 * i) / (sorted.length - 1));
    });
    return raw.map((s) => ({
      id: s.id,
      name: s.name,
      x: 10 + (80 * (s.rawX - minX)) / spanX,
      y: yById.get(s.id) ?? midY,
    }));
  }

  return raw.map((s) => ({
    id: s.id,
    name: s.name,
    x: 10 + (80 * (s.rawX - minX)) / spanX,
    y: spanYRaw === 0 ? midY : 10 + (80 * (s.rawY - minY)) / spanYRaw,
  }));
}

function parseBodyElement(
  el: Element,
  kind: SystemBodyNode['kind'],
  systemId: string,
  parentId?: string
): SystemBodyNode {
  const id = attr(el, 'name', 'id') || '';
  const body: SystemBodyNode = {
    id,
    name: attr(el, 'name-en') || id,
    kind,
    au: parseFloat(attr(el, 'AU', 'au') || '0'),
    surfaceW: parseOptionalInt(el.getAttribute('surface-size-X')),
    surfaceH: parseOptionalInt(el.getAttribute('surface-size-Y')),
    parentId,
    planetType: el.getAttribute('type') || undefined,
    gravity: el.getAttribute('gravity') || undefined,
    temperature: el.getAttribute('temperature') || undefined,
    atmosphere: el.getAttribute('atmosphere') || undefined,
    description: el.getAttribute('description') || undefined,
    regions: [],
    orbitIds: [],
    orbits: [],
  };

  for (const child of el.children) {
    const childTag = child.tagName.toLowerCase();
    if (childTag === 'region') {
      body.regions.push(parseRegionEl(child, systemId, id));
    } else if (childTag === 'orbit') {
      const orbit = parseOrbitEl(child, systemId, id);
      if (orbit.id) {
        body.orbitIds.push(orbit.id);
        body.orbits.push(orbit);
      }
    }
  }

  return body;
}

function parseSystemDetail(systemEl: Element): SystemDetail {
  const id = attr(systemEl, 'name', 'id') || '';
  const starEl = systemEl.querySelector(':scope > star');
  const starId = attr(starEl, 'name', 'id');
  const detail: SystemDetail = {
    id,
    name: attr(systemEl, 'name-en') || id,
    starName: attr(starEl, 'name-en'),
    star: starId
      ? {
          id: starId,
          name: attr(starEl, 'name-en') || starId,
          starType: starEl?.getAttribute('type') || undefined,
          unexplored: starEl?.getAttribute('explored') === 'false',
        }
      : undefined,
    bodies: [],
  };

  function walkBodies(parentEl: Element, parentBodyId?: string) {
    for (const child of parentEl.children) {
      const tag = child.tagName.toLowerCase();
      if (tag === 'planet' || tag === 'moon' || tag === 'belt' || tag === 'alderson') {
        const body = parseBodyElement(child, tag, id, parentBodyId);
        detail.bodies.push(body);
        walkBodies(child, body.id);
      }
    }
  }

  walkBodies(systemEl);
  return detail;
}

function parseSystemDetails(root: Element): SystemDetail[] {
  const galaxy = root.querySelector('galaxy');
  if (!galaxy) return [];
  return [...galaxy.querySelectorAll(':scope > system')].map(parseSystemDetail);
}

export function getSystemDetail(report: ParsedReport, systemId: string): SystemDetail | undefined {
  return report.systemDetails.find((s) => s.id === systemId);
}

export function getBodyDetail(
  report: ParsedReport,
  systemId: string,
  bodyId: string
): SystemBodyNode | undefined {
  return getSystemDetail(report, systemId)?.bodies.find((b) => b.id === bodyId);
}

export interface OrbitBand {
  body: SystemBodyNode;
  children: SystemBodyNode[];
}

export interface SystemOrbitLayout {
  bands: OrbitBand[];
  gates: SystemBodyNode[];
}

export function topLevelBodies(detail: SystemDetail): SystemBodyNode[] {
  return detail.bodies.filter((b) => !b.parentId).sort((a, b) => a.au - b.au || a.name.localeCompare(b.name));
}

/** One horizontal band per top-level body; Alderson gates share the rightmost column. */
export function groupSystemOrbitBands(detail: SystemDetail): SystemOrbitLayout {
  const top = topLevelBodies(detail);
  const gates = top
    .filter((b) => b.kind === 'alderson')
    .sort((a, b) => a.name.localeCompare(b.name) || a.id.localeCompare(b.id));
  const bands = top
    .filter((b) => b.kind !== 'alderson')
    .map((body) => ({ body, children: childBodies(detail, body.id) }));
  return { bands, gates };
}

export function childBodies(detail: SystemDetail, parentId: string): SystemBodyNode[] {
  return detail.bodies
    .filter((b) => b.parentId === parentId)
    .sort((a, b) => a.au - b.au || a.name.localeCompare(b.name));
}

export function collectBodyLocationIds(detail: SystemDetail, bodyId: string): string[] {
  const body = detail.bodies.find((b) => b.id === bodyId);
  if (!body) return [];
  const ids = new Set<string>([body.id, ...body.orbitIds, ...body.regions.map((r) => r.id)]);
  for (const child of childBodies(detail, bodyId)) {
    for (const id of collectBodyLocationIds(detail, child.id)) ids.add(id);
  }
  return [...ids];
}

export function bodyKindIcon(body: SystemBodyNode): string {
  const t = (body.planetType || '').toLowerCase();
  if (body.kind === 'planet' || body.kind === 'moon') {
    if (t.includes('gas')) return '◉';
    if (t === 'ocean') return '●';
    if (t === 'dust' || t === 'desert') return '◌';
    if (t === 'ice') return '◦';
    if (t === 'lava') return '▲';
    if (t === 'rock') return '◆';
    return '●';
  }
  switch (body.kind) {
    case 'belt':
      return '◎';
    case 'alderson':
      return '⎈';
    default:
      return '○';
  }
}

export function filterStacksByLocations(stacks: StackNode[], locationIds: Set<string>): StackNode[] {
  return stacks.filter((s) => s.locationId != null && locationIds.has(s.locationId));
}

export function formatStackLabel(stack: StackNode): string {
  if (stack.reportLine) {
    return stack.reportLine.replace(/^\+\s*/, '').replace(/\.\s*$/, '');
  }
  const label = stack.name || stack.type || stack.id;
  const typ = stack.type || '';
  const modWord = stack.moduleCount === 1 ? 'module' : 'modules';
  return [label, typ, stack.id, `${stack.moduleCount} ${modWord}`].filter(Boolean).join(', ');
}

export function formatPersonLabel(person: PersonNode): string {
  const race = person.race ? ` ${person.race}` : '';
  return `${person.name}, person${race}, ${person.id}`;
}

export function describeExitTarget(
  exit: ExitEntry,
  catalog: Map<string, RegionNode>,
  regionReportLines?: Record<string, string>
): { label: string; missingCoords: boolean } {
  if (exit.targetKind !== 'region') {
    return { label: `${exit.targetKind} ${exit.targetId}`, missingCoords: false };
  }
  const reportLine = regionReportLines?.[exit.targetId];
  if (reportLine) {
    const r = catalog.get(exit.targetId);
    return {
      label: reportLine,
      missingCoords: r ? r.x == null || r.y == null : false,
    };
  }
  const r = catalog.get(exit.targetId);
  if (!r) {
    const stub = exit.target;
    if (stub?.discovered === 'no') {
      return { label: `${exit.targetId} (unknown region)`, missingCoords: true };
    }
    if (stub && stub.x != null && stub.y != null) {
      let label = `${stub.name || exit.targetId} [${exit.targetId}] (${stub.x},${stub.y})`;
      if (stub.terrainType) label += `, ${stub.terrainType} region`;
      if (exit.mode) label += `, ${exit.mode} travel duration ${exit.duration ?? '?'} weeks`;
      if (stub.discovered === 'partial') label += ', adjacent (unvisited)';
      return { label, missingCoords: false };
    }
    return {
      label: `${exit.targetId} (region not in report XML)`,
      missingCoords: true,
    };
  }
  let label = `${r.name} [${exit.targetId}]`;
  if (r.x != null && r.y != null) label += ` (${r.x},${r.y})`;
  if (r.terrainType) label += `, ${r.terrainType} region`;
  if (exit.mode) label += `, ${exit.mode} travel duration ${exit.duration ?? '?'} weeks`;
  return {
    label,
    missingCoords: r.x == null || r.y == null,
  };
}

export function formatUpkeep(upkeep: { type: string; quantity: number }[]): string {
  if (upkeep.length === 0) return '—';
  return upkeep.map((u) => `${u.type} ${u.quantity}`).join(', ');
}

export function formatOrderEntry(order: ParsedOrderEntry): string {
  const lines = [`subject: ${order.subject}`, `target: ${order.targetId}`];
  if (order.repeat) lines.push(`repeat: ${order.repeat}`);
  for (const verb of order.verbs) {
    if (verb.kind === 'move' && order.moveDestinations.length) {
      lines.push(`MOVE: ${order.moveDestinations.join(' → ')}`);
    } else if (verb.detail) {
      lines.push(`${verb.kind.toUpperCase()}: ${verb.detail}`);
    } else {
      lines.push(verb.kind.toUpperCase());
    }
  }
  return lines.join('\n');
}

export function findPersonInStacks(stacks: StackNode[], personId: string): PersonNode | undefined {
  for (const stack of flattenStacks(stacks)) {
    const person = stack.persons.find((p) => p.id === personId);
    if (person) return person;
  }
  return undefined;
}

export function findOrderForTarget(
  orders: ParsedOrderEntry[],
  subject: string,
  targetId: string
): ParsedOrderEntry | undefined {
  return orders.find((o) => o.subject === subject && o.targetId === targetId);
}

export function regionCatalog(report: ParsedReport): Map<string, RegionNode> {
  const map = new Map<string, RegionNode>();
  const put = (r: RegionNode) => {
    const prev = map.get(r.id);
    map.set(r.id, prev ? { ...prev, ...r, x: r.x ?? prev.x, y: r.y ?? prev.y } : r);
  };
  for (const r of report.regions) put(r);
  for (const r of report.regionHints) put(r);
  for (const sys of report.systemDetails) {
    for (const body of sys.bodies) {
      for (const r of body.regions) put(r);
      for (const r of body.regions) {
        for (const ex of r.exits || []) {
          if (ex.targetKind !== 'region' || !ex.target) continue;
          const stub = ex.target;
          if (stub.discovered === 'no' || stub.x == null || stub.y == null) continue;
          put({
            id: stub.id,
            name: stub.name || stub.id,
            systemId: sys.id,
            bodyId: body.id,
            x: stub.x,
            y: stub.y,
            terrainType: stub.terrainType,
            capacities: ex.target.capacities,
            resources: ex.target.resources,
            deepResources: ex.target.deepResources,
            anomaly: ex.target.anomaly,
          });
        }
      }
    }
  }
  return map;
}

export function buildRegionMapCells(
  body: SystemBodyNode,
  catalog: Map<string, RegionNode>,
  ownedRegionIds: Set<string>
): RegionMapCell[] {
  const cells = new Map<string, RegionMapCell>();
  const addCell = (r: RegionNode, reachableViaExit: boolean) => {
    if (r.x == null || r.y == null) return;
    if (r.bodyId && r.bodyId !== body.id) return;
    cells.set(r.id, {
      ...r,
      hasPresence: ownedRegionIds.has(r.id),
      reachableViaExit,
    });
  };
  for (const r of body.regions) addCell(r, false);
  for (const r of body.regions) {
    for (const ex of r.exits || []) {
      if (ex.targetKind !== 'region') continue;
      let target = catalog.get(ex.targetId);
      if (!target && ex.target && ex.target.x != null && ex.target.y != null) {
        target = {
          id: ex.targetId,
          name: ex.target.name || ex.targetId,
          systemId: body.id,
          bodyId: body.id,
          x: ex.target.x,
          y: ex.target.y,
          terrainType: ex.target.terrainType,
        };
      }
      if (!target || (target.bodyId && target.bodyId !== body.id)) continue;
      if (!cells.has(ex.targetId)) addCell(target, true);
    }
  }
  return [...cells.values()];
}

export function regionMapGridSize(
  cells: RegionMapCell[]
): { gridW: number; gridH: number; minX: number; minY: number } {
  if (cells.length === 0) return { gridW: 1, gridH: 1, minX: 0, minY: 0 };
  const xs = cells.map((c) => c.x ?? 0);
  const ys = cells.map((c) => c.y ?? 0);
  const minX = Math.min(...xs);
  const minY = Math.min(...ys);
  const maxX = Math.max(...xs);
  const maxY = Math.max(...ys);
  return {
    minX,
    minY,
    gridW: maxX - minX + 1,
    gridH: maxY - minY + 1,
  };
}

export type UnitSortKey = 'id' | 'type' | 'name';

export function sortRootStacks(stacks: StackNode[], key: UnitSortKey): StackNode[] {
  const sorted = [...stacks];
  sorted.sort((a, b) => {
    if (key === 'id') return a.id.localeCompare(b.id, undefined, { numeric: true });
    if (key === 'type') return (a.type || '').localeCompare(b.type || '');
    return (a.name || '').localeCompare(b.name || '');
  });
  return sorted;
}

export function partitionOwnedStacks(
  stacks: StackNode[],
  factionId: string
): { owned: StackNode[]; other: StackNode[] } {
  const owned: StackNode[] = [];
  const other: StackNode[] = [];
  for (const s of stacks) {
    if (s.faction === factionId) owned.push(s);
    else other.push(s);
  }
  return { owned, other };
}

/** Unit name from a galaxy line, e.g. `+ small cargo bay [200003], …` → `small cargo bay`. */
function reportUnitName(stack: StackNode): string {
  const line = stack.reportLine;
  if (!line) return '';
  const match = line.match(/^\+?\s*(.+?)\s*\[/);
  return match?.[1]?.trim() ?? '';
}

/** Human-readable module type from a galaxy line, e.g. `2 small cargo bays [cargob]` → `small cargo bays`. */
export function moduleTypeName(stack: StackNode): string {
  const line = stack.reportLine;
  if (!line) return '';
  const match = line.match(/,\s*(.+?)\s*\[[^\]]+\]/);
  if (!match?.[1]) return '';
  return match[1].replace(/^\d+\s+/, '').trim();
}

function stackQueryHay(stack: StackNode): string {
  return [
    stack.id,
    stack.name,
    stack.type,
    reportUnitName(stack),
    moduleTypeName(stack),
    stack.reportLine,
    formatStackLabel(stack),
  ]
    .filter(Boolean)
    .join(' ')
    .toLowerCase();
}

function stackMatchesQuery(stack: StackNode, q: string, includeLocation: boolean): boolean {
  if (stackQueryHay(stack).includes(q)) return true;
  if (!includeLocation) return false;
  const loc = `${stack.locationId || ''} ${stack.locationName || ''}`.toLowerCase();
  return loc.includes(q);
}

function personMatchesQuery(person: PersonNode, q: string): boolean {
  return `${person.id} ${person.name}`.toLowerCase().includes(q);
}

export function filterStacksByQuery(
  stacks: StackNode[],
  query: string,
  options?: { includeLocation?: boolean },
): StackNode[] {
  const q = query.trim().toLowerCase();
  if (!q) return stacks;
  const includeLocation = options?.includeLocation === true;
  const matched: StackNode[] = [];
  for (const stack of stacks) {
    const self = stackMatchesQuery(stack, q, includeLocation);
    if (self) {
      matched.push(stack);
      continue;
    }
    const persons = stack.persons.filter((person) => personMatchesQuery(person, q));
    const children = filterStacksByQuery(stack.children, query, options);
    if (persons.length === 0 && children.length === 0) continue;
    matched.push({ ...stack, persons, children });
  }
  return matched;
}

export function groupStacksByLocation(
  stacks: StackNode[]
): { locationId: string; locationName: string; stacks: StackNode[] }[] {
  const groups = new Map<string, { locationName: string; stacks: StackNode[] }>();
  for (const s of stacks) {
    const loc = s.locationId || 'unknown';
    const entry = groups.get(loc) || { locationName: s.locationName || loc, stacks: [] };
    entry.stacks.push(s);
    groups.set(loc, entry);
  }
  return [...groups.entries()].map(([locationId, g]) => ({
    locationId,
    locationName: g.locationName,
    stacks: g.stacks,
  }));
}

export function stacksAtLocation(stacks: StackNode[], locationId: string): boolean {
  return stacks.some((s) => s.locationId === locationId);
}

export function ownedRegionIds(report: ParsedReport): Set<string> {
  const ids = new Set<string>();
  const { factionId } = report;
  for (const s of flattenStacks(report.stacks)) {
    if (s.faction === factionId && s.locationId) ids.add(s.locationId);
  }
  return ids;
}

export function locationIdsWithPresence(
  report: ParsedReport,
  factionId?: string
): Set<string> {
  const ids = new Set<string>();
  for (const s of flattenStacks(report.stacks)) {
    if (s.locationId && (!factionId || s.faction === factionId)) ids.add(s.locationId);
  }
  return ids;
}

export function systemHasPresence(report: ParsedReport, systemId: string): boolean {
  return flattenStacks(report.stacks).some((s) => s.systemId === systemId);
}

export function getRegionDetail(report: ParsedReport, regionId: string): RegionNode | undefined {
  return regionCatalog(report).get(regionId);
}

const HOME_SYSTEM_IDS = new Set(['SS0001', 'SS0002']);

function parseAldersonLinks(root: Element): AldersonLink[] {
  const galaxy = root.querySelector('galaxy');
  if (!galaxy) return [];

  const gateSystem = new Map<string, string>();
  const gateUnstable = new Map<string, boolean>();
  const reportedGateIds = new Set<string>();
  const pairs: { gateId: string; pairId: string }[] = [];
  galaxy.querySelectorAll(':scope > system').forEach((systemEl) => {
    const systemId = attr(systemEl, 'name', 'id') || '';
    systemEl.querySelectorAll(':scope > alderson').forEach((gateEl) => {
      const gateId = attr(gateEl, 'name', 'id');
      const pairId = gateEl.getAttribute('pair');
      if (!gateId) return;
      gateSystem.set(gateId, systemId);
      gateUnstable.set(gateId, gateEl.getAttribute('stability') === 'unstable');
      reportedGateIds.add(gateId);
      if (pairId) pairs.push({ gateId, pairId });
    });
  });

  const seen = new Set<string>();
  const links: AldersonLink[] = [];
  for (const { gateId, pairId } of pairs) {
    if (!reportedGateIds.has(gateId) || !reportedGateIds.has(pairId)) continue;
    const fromSystemId = gateSystem.get(gateId);
    const toSystemId = gateSystem.get(pairId);
    if (!fromSystemId || !toSystemId || fromSystemId === toSystemId) continue;
    const key = [fromSystemId, toSystemId].sort().join('|');
    if (seen.has(key)) continue;
    seen.add(key);
    const unstable = !!(gateUnstable.get(gateId) || gateUnstable.get(pairId));
    links.push({
      fromSystemId,
      toSystemId,
      stable: !unstable,
      homePair: HOME_SYSTEM_IDS.has(fromSystemId) && HOME_SYSTEM_IDS.has(toSystemId),
    });
  }
  return links;
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

function parseOrderVerbs(el: Element): OrderVerb[] {
  const verbs: OrderVerb[] = [];
  const moveEl = el.querySelector('move');
  if (moveEl) verbs.push({ kind: 'move' });
  const produceEl = el.querySelector('produce');
  if (produceEl) {
    const type = produceEl.getAttribute('produce-type') || produceEl.getAttribute('type') || 'item';
    const qty = produceEl.getAttribute('quantity');
    verbs.push({ kind: 'produce', detail: qty ? `${type} ×${qty}` : type });
  }
  for (const child of el.children) {
    const tag = child.tagName.toLowerCase();
    if (tag === 'move' || tag === 'produce') continue;
    const detail =
      child.getAttribute('type') ||
      child.getAttribute('train-type') ||
      child.getAttribute('research-type') ||
      child.getAttribute('name-en') ||
      undefined;
    verbs.push({ kind: tag, detail });
  }
  return verbs;
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
      verbs: parseOrderVerbs(el),
    });
  });
  return orders;
}

export function parseReportXml(xml: string): ParsedReport {
  const doc = new DOMParser().parseFromString(xml, 'application/xml');
  const root = doc.documentElement;
  const turn = parseInt(root.getAttribute('turn') || '1', 10);
  const factionEl = root.querySelector('faction');
  const factionId = attr(factionEl, 'name', 'id') || '';
  const factionName =
    text(root.querySelector('faction-name')) ||
    attr(factionEl, 'name-en') ||
    text(root.querySelector('faction')) ||
    '';

  const systems = parseSystems(root);
  const aldersonLinks = parseAldersonLinks(root);
  const systemDetails = parseSystemDetails(root);
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
    factionId,
    factionName,
    stacks,
    systems,
    aldersonLinks,
    regions,
    systemDetails,
    orders: parseOrders(root),
    technologies,
    battles,
    faction: { upkeep, research, press, contracts },
    bank,
    diplomacy,
    regionHints: [],
    regionReportLines: {},
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
  return findOrderForTarget(orders, 'modulestack', stackId);
}

export function systemIdForRegion(regions: RegionNode[], regionId: string): string | undefined {
  return regions.find((r) => r.id === regionId)?.systemId;
}

export function resolveMoveRouteSystems(
  stack: StackNode,
  order: ParsedOrderEntry,
  regions: RegionNode[]
): { fromSystemId: string; toSystemId: string } | null {
  const fromSystemId = stack.systemId;
  const lastDest = order.moveDestinations[order.moveDestinations.length - 1];
  if (!fromSystemId || !lastDest) return null;
  const toSystemId =
    systemIdForRegion(regions, lastDest) ||
    (regions.some((r) => r.systemId === lastDest) ? lastDest : undefined);
  if (!toSystemId || fromSystemId === toSystemId) return null;
  return { fromSystemId, toSystemId };
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

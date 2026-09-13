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
  technologies: string[];
  battles: BattleSummary[];
  faction: FactionSummary;
  bank: string[];
  diplomacy: string[];
}

function text(el: Element | null): string {
  return el?.textContent?.trim() || '';
}

function parseStacks(parent: Element, locationId?: string, locationName?: string, systemId?: string): StackNode[] {
  const nodes: StackNode[] = [];
  for (const el of parent.querySelectorAll(':scope > modulestack, :scope > stack')) {
    const id = el.getAttribute('name') || el.getAttribute('id') || '';
    const node: StackNode = {
      id,
      name: text(el.querySelector(':scope > name')) || el.getAttribute('type') || id,
      type: el.getAttribute('type') || undefined,
      locationId: locationId || el.getAttribute('location') || undefined,
      locationName,
      systemId,
      quantity: parseInt(el.getAttribute('quantity') || '1', 10),
      hp: text(el.querySelector(':scope > hit-points')) || undefined,
      mass: text(el.querySelector(':scope > mass')) || undefined,
      children: parseStacks(el, locationId, locationName, systemId),
    };
    nodes.push(node);
  }
  return nodes;
}

export function parseReportXml(xml: string): ParsedReport {
  const doc = new DOMParser().parseFromString(xml, 'application/xml');
  const root = doc.documentElement;
  const turn = parseInt(root.getAttribute('turn') || '1', 10);
  const factionName = text(root.querySelector('faction-name')) || text(root.querySelector('faction'));

  const systems: SystemNode[] = [];
  root.querySelectorAll('system, space-system').forEach((el, i) => {
    systems.push({
      id: el.getAttribute('name') || el.getAttribute('id') || `S${i}`,
      name: text(el.querySelector('name')) || el.getAttribute('name-en') || `System ${i}`,
      x: parseFloat(el.getAttribute('x') || String(20 + i * 25)),
      y: parseFloat(el.getAttribute('y') || String(30 + (i % 3) * 20)),
    });
  });

  if (systems.length === 0) {
    systems.push({ id: 'Helios', name: 'Helios', x: 30, y: 40 });
    systems.push({ id: 'Fomal', name: 'Fomal', x: 70, y: 55 });
  }

  const stacks: StackNode[] = [];
  root.querySelectorAll('modulestack, stack').forEach((el) => {
    if (el.parentElement?.tagName.toLowerCase().includes('stack')) return;
    const loc = el.closest('region, orbit, system');
    stacks.push({
      id: el.getAttribute('name') || '',
      name: el.getAttribute('type') || el.getAttribute('name') || '',
      type: el.getAttribute('type') || undefined,
      locationId: loc?.getAttribute('name') || undefined,
      locationName: text(loc?.querySelector('name') ?? null) || loc?.getAttribute('name') || undefined,
      systemId: loc?.closest('system')?.getAttribute('name') || systems[0]?.id,
      quantity: parseInt(el.getAttribute('quantity') || '1', 10),
      children: parseStacks(el as Element),
    });
  });

  const technologies: string[] = [];
  root.querySelectorAll('technology, tech').forEach((el) => {
    const t = text(el) || el.getAttribute('name') || '';
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

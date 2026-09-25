import {
  flattenStacks,
  regionCatalog,
  type ParsedReport,
  type RegionAnomaly,
  type StackNode,
  type SystemBodyNode,
  type SystemDetail,
} from '../parsers/reportXml';

/** One mark per presence group. Order is the row order on the map. */
export type PresenceKind =
  | 'hq'
  | 'habitat'
  | 'ground'
  | 'sea'
  | 'craft'
  | 'hull'
  | 'station'
  | 'anomaly'
  | 'fauna';

export interface PresenceIcon {
  kind: PresenceKind;
  glyph: string;
  label: string;
}

const ICONS: Record<PresenceKind, PresenceIcon> = {
  hq: { kind: 'hq', glyph: '⌂', label: 'Headquarters or branch' },
  habitat: { kind: 'habitat', glyph: '▣', label: 'Surface habitat' },
  ground: { kind: 'ground', glyph: '⚔', label: 'Ground units' },
  sea: { kind: 'sea', glyph: '≋', label: 'Sea units' },
  craft: { kind: 'craft', glyph: '✧', label: 'Small spacecraft' },
  hull: { kind: 'hull', glyph: '⬢', label: 'Frigate or larger hull' },
  station: { kind: 'station', glyph: '⌖', label: 'Space station' },
  anomaly: { kind: 'anomaly', glyph: '⌘', label: 'Anomaly to investigate' },
  fauna: { kind: 'fauna', glyph: '※', label: 'Hostile fauna' },
};

const ORDER: PresenceKind[] = [
  'hq',
  'habitat',
  'ground',
  'sea',
  'craft',
  'hull',
  'station',
  'anomaly',
  'fauna',
];

const HQ = new Set(['corphq', 'brnofc']);
const HABITAT = new Set(['city', 'town', 'uscty', 'mtrply', 'tunnls', 'senvdm', 'dmdcty', 'clddom', 'hotdom']);
const GROUND = new Set(['trucks', 'tanks', 'inftry']);
const SEA = new Set(['coastr', 'uwtruk', 'gunbot', 'uwtank']);
const CRAFT = new Set(['shuttl', 'alndrn', 'tanker', 'orbtug']);
const HULL = new Set(['sshull', 'alnhul', 'lghul', 'corhul', 'deshul', 'cruhul', 'arkhul']);
const STATION = new Set(['orcmpx']);
const FAUNA_TYPES = new Set([
  'brmstr', 'mulcrw', 'canalp', 'crstlb', 'slgmnt', 'urstlk',
  'ribgrz', 'glacra', 'frostb', 'silskk', 'qtzrol', 'spngrf',
]);
/** Arbor / Anvil / Haven / Graph wild factions. See Game/FaunaRumors.cs. */
const FAUNA_FACTIONS = new Set(['14', '15', '16', '17']);

/** Pixel width of one glyph plus gap, used to decide if a region cell can hold the row. */
export const PRESENCE_ICON_SLOT_PX = 17;

export function presenceIcon(kind: PresenceKind): PresenceIcon {
  return ICONS[kind];
}

export function stackPresenceKind(stack: Pick<StackNode, 'type' | 'faction'>): PresenceKind | null {
  const type = (stack.type || '').toLowerCase();
  if (FAUNA_TYPES.has(type) || (stack.faction != null && FAUNA_FACTIONS.has(stack.faction))) return 'fauna';
  if (HQ.has(type)) return 'hq';
  if (HABITAT.has(type)) return 'habitat';
  if (GROUND.has(type)) return 'ground';
  if (SEA.has(type)) return 'sea';
  if (CRAFT.has(type)) return 'craft';
  if (HULL.has(type)) return 'hull';
  if (STATION.has(type)) return 'station';
  return null;
}

export function anomalyNeedsInvestigation(anomaly: RegionAnomaly | undefined, factionId: string): boolean {
  if (!anomaly?.type) return false;
  if (!factionId) return true;
  return !(anomaly.resolvedFactions || []).includes(factionId);
}

export function iconsFromKinds(kinds: Iterable<PresenceKind>): PresenceIcon[] {
  const present = new Set(kinds);
  return ORDER.filter((kind) => present.has(kind)).map((kind) => ICONS[kind]);
}

const NAME_FONT_PX = 10;
const NAME_CHAR_PX = NAME_FONT_PX * 0.56;
const NAME_LINE_PX = NAME_FONT_PX * 1.15;
const CELL_PAD_PX = 6;

/** Lines the full name needs, or null when a word is wider than the cell. */
function regionNameLines(name: string, usableW: number): number | null {
  const text = name.trim();
  if (!text || usableW < NAME_FONT_PX) return null;
  const charsPerLine = Math.floor(usableW / NAME_CHAR_PX);
  if (charsPerLine < 1) return null;
  const words = text.split(/\s+/);
  let lines = 1;
  let col = 0;
  for (const word of words) {
    if (word.length > charsPerLine) return null;
    if (col === 0) col = word.length;
    else if (col + 1 + word.length <= charsPerLine) col += 1 + word.length;
    else {
      lines += 1;
      col = word.length;
    }
  }
  return lines;
}

export function regionNameFits(name: string, cellPx: number, reservedPx = 0): boolean {
  const usableW = cellPx - CELL_PAD_PX;
  const usableH = cellPx - CELL_PAD_PX - reservedPx;
  const lines = regionNameLines(name, usableW);
  if (lines == null || usableH < NAME_LINE_PX) return false;
  return lines * NAME_LINE_PX <= usableH;
}

function regionNameHeight(name: string, cellPx: number): number {
  const lines = regionNameLines(name, cellPx - CELL_PAD_PX);
  return lines == null ? 0 : lines * NAME_LINE_PX;
}

/** True when the icon row fits beside whatever else is already reserved. */
export function regionCellShowsIcons(cellPx: number, iconCount: number, reservedPx = 0): boolean {
  if (iconCount <= 0) return false;
  return cellPx >= CELL_PAD_PX + reservedPx + iconCount * PRESENCE_ICON_SLOT_PX;
}

/** Show a name only when the whole string fits; icons stay when their row fits. */
export function regionCellMarks(
  name: string,
  cellPx: number,
  iconCount: number
): { showName: boolean; showIcons: boolean } {
  const iconBand = iconCount > 0 ? iconCount * PRESENCE_ICON_SLOT_PX : 0;
  const nameWithIcons = regionNameFits(name, cellPx, iconBand);
  const nameHeight = nameWithIcons ? regionNameHeight(name, cellPx) : 0;
  const iconsWithName = regionCellShowsIcons(cellPx, iconCount, nameHeight);
  if (nameWithIcons && (iconCount === 0 || iconsWithName)) {
    return { showName: true, showIcons: iconCount > 0 };
  }
  if (regionNameFits(name, cellPx, 0)) return { showName: true, showIcons: false };
  if (regionCellShowsIcons(cellPx, iconCount, 0)) return { showName: false, showIcons: true };
  return { showName: false, showIcons: false };
}

export function buildPresenceIndex(report: ParsedReport): Map<string, PresenceIcon[]> {
  const kinds = new Map<string, Set<PresenceKind>>();
  const add = (locationId: string | undefined, kind: PresenceKind | null) => {
    if (!locationId || !kind) return;
    const set = kinds.get(locationId) || new Set<PresenceKind>();
    set.add(kind);
    kinds.set(locationId, set);
  };

  for (const stack of flattenStacks(report.stacks)) {
    add(stack.locationId, stackPresenceKind(stack));
  }
  for (const region of regionCatalog(report).values()) {
    if (anomalyNeedsInvestigation(region.anomaly, report.factionId)) add(region.id, 'anomaly');
  }

  const index = new Map<string, PresenceIcon[]>();
  for (const [locationId, set] of kinds) index.set(locationId, iconsFromKinds(set));
  return index;
}

export function bodyLocationIds(body: SystemBodyNode): string[] {
  return [body.id, ...body.regions.map((region) => region.id), ...body.orbitIds];
}

export function systemLocationIds(detail: SystemDetail | undefined, systemId: string): string[] {
  const ids = new Set<string>([systemId]);
  for (const body of detail?.bodies || []) {
    for (const id of bodyLocationIds(body)) ids.add(id);
  }
  return [...ids];
}

export function presenceIconsForLocations(
  index: Map<string, PresenceIcon[]>,
  locationIds: Iterable<string>
): PresenceIcon[] {
  const kinds: PresenceKind[] = [];
  for (const id of locationIds) {
    for (const icon of index.get(id) || []) kinds.push(icon.kind);
  }
  return iconsFromKinds(kinds);
}

/** Move-order arrows: straight region hops, curved space hops, solid when the weeks are known. */

import {
  flattenStacks,
  type ExitEntry,
  type ParsedReport,
  type StackNode,
  type SystemBodyNode,
} from '../parsers/reportXml';
import { parseOrdersTemplate } from './orderTemplate';
import { durationWeeks, durationWeeksRaw } from './transit';

export type RouteGeometry = 'straight' | 'angled';
export type RouteEstimate = 'exit' | 'implicit' | 'calculated' | 'unknown';

export interface RouteLocation {
  id: string;
  kind: 'region' | 'orbit' | 'body';
  bodyId: string;
  au: number;
  exits: ExitEntry[];
}

export interface Traveler {
  catalogSpeed: number;
  thrust: number;
  mass: number;
}

export interface MoveSegment {
  fromId: string;
  toId: string;
  geometry: RouteGeometry;
  solid: boolean;
  weeks: number | null;
  estimate: RouteEstimate;
}

export interface PlannedArrow {
  index: number;
  fromKey: string;
  toKey: string;
  segment: MoveSegment;
}

export interface Point {
  x: number;
  y: number;
}

export interface Obstacle {
  x: number;
  y: number;
  r: number;
}

export interface LabelRect {
  left: number;
  top: number;
  right: number;
  bottom: number;
}

export interface ArrowGeometry {
  d: string;
  labelAt: Point;
}

const MOVE_LINE = /^(?:\+\+|--|[+-])?\s*move\b\s*(.*)$/i;

export interface OrderPaneFocus {
  kind: 'modulestack' | 'person';
  id: string;
}

/** Unit block that contains the order-pane caret. Null on the faction header or after #end. */
export function orderFocusAtOffset(text: string, offset: number): OrderPaneFocus | null {
  if (!text) return null;
  const clamped = Math.max(0, Math.min(offset, text.length));
  const lineIndex = text.slice(0, clamped).split('\n').length - 1;
  const lines = text.split('\n');
  let current: OrderPaneFocus | null = null;
  for (let i = 0; i <= lineIndex && i < lines.length; i += 1) {
    const trimmed = lines[i].trim();
    if (/^#end\b/i.test(trimmed)) {
      current = null;
      continue;
    }
    const stackMatch = trimmed.match(/^#modulestack\s+(\S+)/i);
    const personMatch = trimmed.match(/^#person\s+(\S+)/i);
    if (stackMatch) current = { kind: 'modulestack', id: stackMatch[1] };
    else if (personMatch) current = { kind: 'person', id: personMatch[1] };
  }
  return current;
}

/** One unit in the pane is that focus. Several units follow the caret. */
export function orderFocusForPane(text: string, offset: number): OrderPaneFocus | null {
  const unitBlocks = parseOrdersTemplate(text).blocks.filter(
    (block) => block.kind === 'modulestack' || block.kind === 'person',
  );
  if (unitBlocks.length === 1 && unitBlocks[0].kind !== 'other') {
    return { kind: unitBlocks[0].kind, id: unitBlocks[0].id };
  }
  return orderFocusAtOffset(text, offset);
}

/** Destinations from the unit's order block. Null when that unit is not in the template. */
export function moveDestinationsForSelection(
  template: string,
  stackId: string | null,
  personId: string | null,
): string[] | null {
  if (!template.trim() || (!stackId && !personId)) return null;
  const { blocks } = parseOrdersTemplate(template);
  const block = personId
    ? blocks.find((entry) => entry.kind === 'person' && entry.id === personId)
    : blocks.find((entry) => entry.kind === 'modulestack' && entry.id === stackId);
  if (!block) return null;
  const destinations: string[] = [];
  for (const line of block.lines) {
    const match = line.trim().split(';')[0].trim().match(MOVE_LINE);
    if (!match) continue;
    for (const token of match[1].split(/\s+/)) {
      if (token) destinations.push(token);
    }
  }
  return destinations;
}

export function travelerFromMass(mass?: string): Traveler | null {
  if (!mass) return null;
  const match = mass.match(/(\d+)\s*\/\s*(\d+)/);
  if (!match) return null;
  return { catalogSpeed: 1, thrust: parseInt(match[1], 10), mass: parseInt(match[2], 10) };
}

export function selectionTravelContext(
  stacks: StackNode[],
  stackId: string | null,
  personId: string | null,
): { locationId?: string; mass?: string } | undefined {
  const flat = flattenStacks(stacks);
  if (stackId) {
    const stack = flat.find((s) => s.id === stackId);
    return stack ? { locationId: stack.locationId, mass: stack.mass } : undefined;
  }
  if (personId) {
    const stack = flat.find((s) => s.persons.some((p) => p.id === personId));
    return stack ? { locationId: stack.locationId, mass: stack.mass } : undefined;
  }
  return undefined;
}

/** Planet / belt / gate AU as stored. Moon AU is parent AU plus the moon offset (SpaceTransit.BodyAu). */
export function indexRouteLocations(report: ParsedReport): Map<string, RouteLocation> {
  const map = new Map<string, RouteLocation>();
  for (const system of report.systemDetails) {
    const byId = new Map(system.bodies.map((body) => [body.id, body]));
    for (const body of system.bodies) {
      const au = holderAu(body, byId);
      map.set(body.id, { id: body.id, kind: 'body', bodyId: body.id, au, exits: [] });
      for (const orbit of body.orbits) {
        map.set(orbit.id, {
          id: orbit.id,
          kind: 'orbit',
          bodyId: body.id,
          au,
          exits: orbit.exits,
        });
      }
      for (const region of body.regions) {
        map.set(region.id, {
          id: region.id,
          kind: 'region',
          bodyId: body.id,
          au,
          exits: region.exits ?? [],
        });
      }
    }
  }
  return map;
}

export function routeAnchorProps(body: SystemBodyNode): {
  'data-route-id': string;
  'data-route-also'?: string;
  'data-route-obstacle'?: string;
} {
  const also = [...body.orbitIds, ...body.regions.map((region) => region.id)].filter(Boolean).join(' ');
  return {
    'data-route-id': body.id,
    ...(also ? { 'data-route-also': also } : {}),
    ...(body.kind === 'moon' || body.kind === 'belt' ? { 'data-route-obstacle': '1' } : {}),
  };
}

export function buildMoveSegments(
  startId: string | undefined,
  destinations: string[],
  locations: Map<string, RouteLocation>,
  traveler: Traveler | null,
): MoveSegment[] {
  const ids: string[] = [];
  if (startId) ids.push(startId);
  for (const dest of destinations) {
    if (ids[ids.length - 1] !== dest) ids.push(dest);
  }
  const segments: MoveSegment[] = [];
  for (let i = 0; i < ids.length - 1; i += 1) {
    const fromId = ids[i];
    const toId = ids[i + 1];
    const hop = describeHop(locations.get(fromId), locations.get(toId), traveler);
    segments.push({ fromId, toId, ...hop });
  }
  return segments;
}

/** Drop hops that are missing on this map, or that collapse onto one icon. */
export function planVisibleArrows(
  segments: MoveSegment[],
  anchorKey: (id: string) => string | null,
): PlannedArrow[] {
  const planned: PlannedArrow[] = [];
  segments.forEach((segment, index) => {
    const fromKey = anchorKey(segment.fromId);
    const toKey = anchorKey(segment.toId);
    if (!fromKey || !toKey || fromKey === toKey) return;
    planned.push({ index, fromKey, toKey, segment });
  });
  return planned;
}

export function segmentTimeLabel(segment: MoveSegment): string {
  if (segment.weeks == null) return 'Time unknown';
  const unit = segment.weeks === 1 ? 'week' : 'weeks';
  if (segment.estimate === 'calculated') return `~${segment.weeks} ${unit}`;
  return `${segment.weeks} ${unit}`;
}

/** Pixels of clear space between an arrow tip and a space-object label. */
const LABEL_GAP = 6;

export function arrowPath(
  from: Point,
  to: Point,
  geometry: RouteGeometry,
  obstacles: Obstacle[],
  labels?: { from: LabelRect; to: LabelRect },
): ArrowGeometry | null {
  const dx = to.x - from.x;
  const dy = to.y - from.y;
  const len = Math.hypot(dx, dy);
  if (len < 1) return null;
  const control = geometry === 'straight' ? mid(from, to) : curveControl(from, to, len, obstacles);
  const trimmed = labels ? trimCurveToLabels(from, control, to, labels.from, labels.to) : null;
  const start = trimmed?.start ?? from;
  const handle = trimmed?.control ?? control;
  const end = trimmed?.end ?? to;
  const labelAt = geometry === 'straight' && !trimmed
    ? mid(from, to)
    : quadPoint(start, handle, end, 0.5);
  if (geometry === 'straight' && !trimmed) {
    return {
      d: `M ${fmt(from.x)} ${fmt(from.y)} L ${fmt(to.x)} ${fmt(to.y)}`,
      labelAt,
    };
  }
  if (geometry === 'straight' && trimmed) {
    return {
      d: `M ${fmt(start.x)} ${fmt(start.y)} L ${fmt(end.x)} ${fmt(end.y)}`,
      labelAt: { x: round1(labelAt.x), y: round1(labelAt.y) },
    };
  }
  return {
    d: `M ${fmt(start.x)} ${fmt(start.y)} Q ${fmt(handle.x)} ${fmt(handle.y)} ${fmt(end.x)} ${fmt(end.y)}`,
    labelAt: { x: round1(labelAt.x), y: round1(labelAt.y) },
  };
}

function curveControl(from: Point, to: Point, len: number, obstacles: Obstacle[]): Point {
  const dx = to.x - from.x;
  const dy = to.y - from.y;
  const nx = -dy / len;
  const ny = dx / len;
  const base = Math.min(90, Math.max(24, len * 0.28));
  const magnitudes = [base, Math.min(120, base * 1.6), Math.min(160, base * 2.3)];
  let bestBulge = base;
  let bestScore = -Infinity;
  for (const mag of magnitudes) {
    for (const sign of [1, -1]) {
      const bulge = mag * sign;
      const score = curveClearance(from, to, nx, ny, bulge, obstacles);
      if (score > bestScore) {
        bestScore = score;
        bestBulge = bulge;
      }
    }
    if (bestScore > 14) break;
  }
  return {
    x: (from.x + to.x) / 2 + nx * bestBulge,
    y: (from.y + to.y) / 2 + ny * bestBulge,
  };
}

function trimCurveToLabels(
  from: Point,
  control: Point,
  to: Point,
  fromLabel: LabelRect,
  toLabel: LabelRect,
): { start: Point; control: Point; end: Point } | null {
  const t0 = boundaryT(from, control, to, fromLabel, true);
  const t1 = boundaryT(from, control, to, toLabel, false);
  if (t1 - t0 < 0.02) return null;
  const [start, handle, end] = subcurve(from, control, to, t0, t1);
  return { start, control: handle, end };
}

function boundaryT(
  from: Point,
  control: Point,
  to: Point,
  label: LabelRect,
  leaveStart: boolean,
): number {
  const at = (t: number) => quadPoint(from, control, to, t);
  if (leaveStart) {
    let lo = 0;
    let hi = 1;
    for (let i = 1; i <= 32; i += 1) {
      const t = i / 32;
      if (!insideLabel(at(t), label)) {
        lo = (i - 1) / 32;
        hi = t;
        break;
      }
    }
    if (insideLabel(at(hi), label)) return 0;
    for (let n = 0; n < 10; n += 1) {
      const midT = (lo + hi) / 2;
      if (insideLabel(at(midT), label)) lo = midT;
      else hi = midT;
    }
    return hi;
  }
  let lo = 0;
  let hi = 1;
  for (let i = 31; i >= 0; i -= 1) {
    const t = i / 32;
    if (!insideLabel(at(t), label)) {
      lo = t;
      hi = (i + 1) / 32;
      break;
    }
  }
  if (insideLabel(at(lo), label)) return 1;
  for (let n = 0; n < 10; n += 1) {
    const midT = (lo + hi) / 2;
    if (insideLabel(at(midT), label)) hi = midT;
    else lo = midT;
  }
  return lo;
}

function insideLabel(point: Point, label: LabelRect): boolean {
  return point.x >= label.left - LABEL_GAP
    && point.x <= label.right + LABEL_GAP
    && point.y >= label.top - LABEL_GAP
    && point.y <= label.bottom + LABEL_GAP;
}

function subcurve(p0: Point, p1: Point, p2: Point, t0: number, t1: number): [Point, Point, Point] {
  const head = cutQuad(p0, p1, p2, t1).left;
  const u = t1 === 0 ? 0 : t0 / t1;
  return cutQuad(head[0], head[1], head[2], u).right;
}

function cutQuad(
  p0: Point,
  p1: Point,
  p2: Point,
  t: number,
): { left: [Point, Point, Point]; right: [Point, Point, Point] } {
  const a = lerp(p0, p1, t);
  const b = lerp(p1, p2, t);
  const c = lerp(a, b, t);
  return { left: [p0, a, c], right: [c, b, p2] };
}

function lerp(a: Point, b: Point, t: number): Point {
  return { x: a.x + (b.x - a.x) * t, y: a.y + (b.y - a.y) * t };
}

function mid(a: Point, b: Point): Point {
  return { x: round1((a.x + b.x) / 2), y: round1((a.y + b.y) / 2) };
}

function round1(value: number): number {
  return Math.round(value * 10) / 10;
}

function fmt(value: number): string {
  return String(round1(value));
}

function holderAu(body: SystemBodyNode, byId: Map<string, SystemBodyNode>): number {
  if (body.kind === 'moon' && body.parentId) {
    const parent = byId.get(body.parentId);
    return (parent ? holderAu(parent, byId) : 0) + body.au;
  }
  return body.au;
}

function describeHop(
  from: RouteLocation | undefined,
  to: RouteLocation | undefined,
  traveler: Traveler | null,
): Pick<MoveSegment, 'geometry' | 'solid' | 'weeks' | 'estimate'> {
  if (!from || !to) {
    return { geometry: 'angled', solid: false, weeks: null, estimate: 'unknown' };
  }
  const geometry: RouteGeometry =
    from.kind === 'region' && to.kind === 'region' && from.bodyId === to.bodyId ? 'straight' : 'angled';

  const sameBody = from.bodyId === to.bodyId;
  const surfaceAndOrbit =
    sameBody &&
    ((from.kind === 'region' && to.kind === 'orbit') || (from.kind === 'orbit' && to.kind === 'region'));
  if (surfaceAndOrbit) {
    return { geometry, solid: true, weeks: 1, estimate: 'implicit' };
  }

  const exitWeeks = exitDuration(from, to.id) ?? exitDuration(to, from.id);
  if (exitWeeks != null) {
    return { geometry, solid: true, weeks: exitWeeks, estimate: 'exit' };
  }

  const spaceHop = from.kind === 'orbit' || to.kind === 'orbit' || from.bodyId !== to.bodyId;
  if (spaceHop && Number.isFinite(from.au) && Number.isFinite(to.au)) {
    return {
      geometry,
      solid: true,
      weeks: spaceWeeks(Math.abs(from.au - to.au), traveler),
      estimate: 'calculated',
    };
  }

  return { geometry, solid: false, weeks: null, estimate: 'unknown' };
}

function exitDuration(location: RouteLocation, targetId: string): number | null {
  const exit = location.exits.find((entry) => entry.targetId === targetId && entry.duration != null);
  if (exit?.duration == null) return null;
  return Math.max(1, Math.ceil(exit.duration));
}

function spaceWeeks(deltaAu: number, traveler: Traveler | null): number {
  if (deltaAu < 0.0001) return 1;
  const speed = traveler && traveler.catalogSpeed > 0 ? traveler.catalogSpeed : 1;
  if (traveler && traveler.thrust > 0) {
    return Math.max(1, durationWeeks(deltaAu, speed, traveler.thrust, traveler.mass));
  }
  return Math.max(1, Math.ceil(durationWeeksRaw(deltaAu) / speed - 1e-9));
}

function curveClearance(
  from: Point,
  to: Point,
  nx: number,
  ny: number,
  bulge: number,
  obstacles: Obstacle[],
): number {
  const control = {
    x: (from.x + to.x) / 2 + nx * bulge,
    y: (from.y + to.y) / 2 + ny * bulge,
  };
  if (obstacles.length === 0) {
    return bulge > 0 ? 1000 - Math.abs(bulge) * 0.01 : -1000;
  }
  let minClear = Infinity;
  for (let step = 1; step <= 7; step += 1) {
    const point = quadPoint(from, control, to, step / 8);
    for (const obstacle of obstacles) {
      minClear = Math.min(minClear, Math.hypot(point.x - obstacle.x, point.y - obstacle.y) - obstacle.r);
    }
  }
  return minClear;
}

function quadPoint(p0: Point, p1: Point, p2: Point, t: number): Point {
  const u = 1 - t;
  return {
    x: u * u * p0.x + 2 * u * t * p1.x + t * t * p2.x,
    y: u * u * p0.y + 2 * u * t * p1.y + t * t * p2.y,
  };
}

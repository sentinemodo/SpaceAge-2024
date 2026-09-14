import type { SystemNode } from '../parsers/reportXml';

const SYSTEM_NODE_RADIUS = 4.2;

/** Shorten a link so endpoints sit on the system circle edge, not the center. */
export function clipLinkEndpoints(
  from: SystemNode,
  to: SystemNode,
  radius = SYSTEM_NODE_RADIUS
): { from: SystemNode; to: SystemNode } {
  const dx = to.x - from.x;
  const dy = to.y - from.y;
  const len = Math.hypot(dx, dy) || 1;
  const ux = dx / len;
  const uy = dy / len;
  return {
    from: { ...from, x: from.x + ux * radius, y: from.y + uy * radius },
    to: { ...to, x: to.x - ux * radius, y: to.y - uy * radius },
  };
}

function pointToSegmentDistance(
  px: number,
  py: number,
  x1: number,
  y1: number,
  x2: number,
  y2: number
): number {
  const dx = x2 - x1;
  const dy = y2 - y1;
  const lenSq = dx * dx + dy * dy;
  if (lenSq === 0) return Math.hypot(px - x1, py - y1);
  let t = ((px - x1) * dx + (py - y1) * dy) / lenSq;
  t = Math.max(0, Math.min(1, t));
  const nx = x1 + t * dx;
  const ny = y1 + t * dy;
  return Math.hypot(px - nx, py - ny);
}

function lineCrossesSystems(
  from: SystemNode,
  to: SystemNode,
  systems: SystemNode[],
  excludeIds: Set<string>,
  threshold = 4
): boolean {
  for (const sys of systems) {
    if (excludeIds.has(sys.id)) continue;
    if (pointToSegmentDistance(sys.x, sys.y, from.x, from.y, to.x, to.y) < threshold) {
      return true;
    }
  }
  return false;
}

function arcPath(from: SystemNode, to: SystemNode, bulge: number): string {
  const mx = (from.x + to.x) / 2;
  const my = (from.y + to.y) / 2;
  const dx = to.x - from.x;
  const dy = to.y - from.y;
  const len = Math.hypot(dx, dy) || 1;
  const nx = -dy / len;
  const ny = dx / len;
  const cx = mx + nx * bulge;
  const cy = my + ny * bulge;
  return `M ${from.x} ${from.y} Q ${cx} ${cy} ${to.x} ${to.y}`;
}

/** SVG path for a system link; arcs when a straight segment would pass over other systems. */
export function buildSystemLinkPath(
  from: SystemNode,
  to: SystemNode,
  systems: SystemNode[],
  clipRadius = SYSTEM_NODE_RADIUS
): string {
  const clipped = clipLinkEndpoints(from, to, clipRadius);
  const exclude = new Set([from.id, to.id]);
  if (!lineCrossesSystems(clipped.from, clipped.to, systems, exclude)) {
    return `M ${clipped.from.x} ${clipped.from.y} L ${clipped.to.x} ${clipped.to.y}`;
  }
  const dist = Math.hypot(clipped.to.x - clipped.from.x, clipped.to.y - clipped.from.y);
  const bulge = Math.max(6, dist * 0.22);
  const positive = arcPath(clipped.from, clipped.to, bulge);
  const negative = arcPath(clipped.from, clipped.to, -bulge);
  const midPos = {
    id: '',
    name: '',
    x: (clipped.from.x + clipped.to.x) / 2 + (-(clipped.to.y - clipped.from.y) / dist) * bulge,
    y: (clipped.from.y + clipped.to.y) / 2 + ((clipped.to.x - clipped.from.x) / dist) * bulge,
  };
  const midNeg = {
    id: '',
    name: '',
    x: (clipped.from.x + clipped.to.x) / 2 - (-(clipped.to.y - clipped.from.y) / dist) * bulge,
    y: (clipped.from.y + clipped.to.y) / 2 - ((clipped.to.x - clipped.from.x) / dist) * bulge,
  };
  const posHits = lineCrossesSystems(clipped.from, midPos, systems, exclude, 3)
    || lineCrossesSystems(midPos, clipped.to, systems, exclude, 3);
  const negHits = lineCrossesSystems(clipped.from, midNeg, systems, exclude, 3)
    || lineCrossesSystems(midNeg, clipped.to, systems, exclude, 3);
  if (posHits && !negHits) return negative;
  if (negHits && !posHits) return positive;
  return positive;
}

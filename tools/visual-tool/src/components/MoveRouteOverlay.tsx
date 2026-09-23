import { useEffect, useId, useLayoutEffect, useState } from 'react';
import {
  arrowPath,
  segmentTimeLabel,
  type LabelRect,
  type MoveSegment,
  type Obstacle,
  type Point,
} from '../lib/moveRoute';

interface DrawnArrow {
  key: string;
  d: string;
  labelAt: Point;
  solid: boolean;
  label: string;
  fromEl: HTMLElement;
  toEl: HTMLElement;
}

export function MoveRouteOverlay({
  segments,
  layoutKey,
  clipToLabels = false,
}: {
  segments: MoveSegment[];
  layoutKey: string;
  /** Helios view: stop the shaft just outside each space-object label. */
  clipToLabels?: boolean;
}) {
  const markerId = `move-arrow-${useId().replace(/:/g, '')}`;
  const [host, setHost] = useState<HTMLDivElement | null>(null);
  const [size, setSize] = useState({ w: 0, h: 0 });
  const [arrows, setArrows] = useState<DrawnArrow[]>([]);
  const [hot, setHot] = useState<number | null>(null);

  useLayoutEffect(() => {
    const root = host?.parentElement;
    if (!host || !root || segments.length === 0) {
      setArrows([]);
      setSize({ w: 0, h: 0 });
      return;
    }

    const measure = () => {
      const content = contentSize(root, host);
      const anchors = indexAnchors(root);
      const obstacles = indexObstacles(root);
      const origin = root.getBoundingClientRect();
      const drawn: DrawnArrow[] = [];
      segments.forEach((segment, index) => {
        const fromEl = anchors.get(segment.fromId);
        const toEl = anchors.get(segment.toId);
        if (!fromEl || !toEl || fromEl === toEl) return;
        const from = centerOf(fromEl, origin, root);
        const to = centerOf(toEl, origin, root);
        const blocked = obstacles.filter((obstacle) => obstacle.el !== fromEl && obstacle.el !== toEl);
        const labels = clipToLabels
          ? { from: labelRect(fromEl, origin, root), to: labelRect(toEl, origin, root) }
          : undefined;
        const path = arrowPath(from, to, segment.geometry, blocked, labels);
        if (!path) return;
        drawn.push({
          key: `${segment.fromId}-${segment.toId}-${index}`,
          d: path.d,
          labelAt: path.labelAt,
          solid: segment.solid,
          label: segmentTimeLabel(segment),
          fromEl,
          toEl,
        });
      });
      setSize((prev) => (prev.w === content.w && prev.h === content.h ? prev : content));
      setArrows((prev) => (sameArrows(prev, drawn) ? prev : drawn));
    };

    measure();
    const observer = new ResizeObserver(measure);
    observer.observe(root);
    root.querySelectorAll<HTMLElement>('[data-route-id]').forEach((el) => observer.observe(el));
    root.addEventListener('scroll', measure, { passive: true, capture: true });
    window.addEventListener('resize', measure);
    return () => {
      observer.disconnect();
      root.removeEventListener('scroll', measure, true);
      window.removeEventListener('resize', measure);
    };
  }, [host, segments, layoutKey, clipToLabels]);

  useEffect(() => {
    if (hot == null) return;
    const arrow = arrows[hot];
    if (!arrow) return;
    arrow.fromEl.classList.add('route-lit');
    arrow.toEl.classList.add('route-lit');
    return () => {
      arrow.fromEl.classList.remove('route-lit');
      arrow.toEl.classList.remove('route-lit');
    };
  }, [hot, arrows]);

  const active = hot != null ? arrows[hot] : undefined;

  return (
    <div ref={setHost} className="move-route-layer" style={{ width: size.w, height: size.h }}>
      {size.w > 0 && size.h > 0 && (
        <svg viewBox={`0 0 ${size.w} ${size.h}`} width={size.w} height={size.h} aria-hidden>
          <defs>
            <marker
              id={markerId}
              viewBox="0 0 10 10"
              markerWidth="9"
              markerHeight="9"
              refX="8"
              refY="5"
              orient="auto"
              markerUnits="userSpaceOnUse"
            >
              <path d="M0,1 L9,5 L0,9 z" fill="var(--accent)" />
            </marker>
          </defs>
          {arrows.map((arrow, index) => (
            <g key={arrow.key}>
              <path
                d={arrow.d}
                className="route-arrow-hit"
                onMouseEnter={() => setHot(index)}
                onMouseLeave={() => setHot((current) => (current === index ? null : current))}
              />
              <path
                d={arrow.d}
                className={`route-arrow ${arrow.solid ? 'solid' : 'dashed'} ${hot === index ? 'hot' : ''}`}
                markerEnd={`url(#${markerId})`}
              />
            </g>
          ))}
        </svg>
      )}
      {active && (
        <div className="route-eta" style={{ left: active.labelAt.x, top: active.labelAt.y }}>
          {active.label}
        </div>
      )}
    </div>
  );
}

function sameArrows(prev: DrawnArrow[], next: DrawnArrow[]): boolean {
  if (prev.length !== next.length) return false;
  return prev.every((arrow, index) => {
    const other = next[index];
    return arrow.key === other.key
      && arrow.d === other.d
      && arrow.label === other.label
      && arrow.solid === other.solid
      && arrow.labelAt.x === other.labelAt.x
      && arrow.labelAt.y === other.labelAt.y;
  });
}

function contentSize(root: HTMLElement, layer: HTMLElement): { w: number; h: number } {
  const previous = layer.style.display;
  layer.style.display = 'none';
  const w = Math.max(root.scrollWidth, root.clientWidth);
  const h = Math.max(root.scrollHeight, root.clientHeight);
  layer.style.display = previous;
  return { w, h };
}

function labelRect(el: HTMLElement, origin: DOMRect, root: HTMLElement): LabelRect {
  const rect = el.getBoundingClientRect();
  const left = rect.left - origin.left + root.scrollLeft;
  const top = rect.top - origin.top + root.scrollTop;
  return { left, top, right: left + rect.width, bottom: top + rect.height };
}

function centerOf(el: HTMLElement, origin: DOMRect, root: HTMLElement): Point {
  const rect = el.getBoundingClientRect();
  return {
    x: rect.left - origin.left + root.scrollLeft + rect.width / 2,
    y: rect.top - origin.top + root.scrollTop + rect.height / 2,
  };
}

function preferSpatial(list: HTMLElement[]): HTMLElement {
  return list.find((el) => !el.closest('.planet-view-switcher')) ?? list[0];
}

function indexAnchors(root: HTMLElement): Map<string, HTMLElement> {
  const exact = new Map<string, HTMLElement[]>();
  const also = new Map<string, HTMLElement[]>();
  root.querySelectorAll<HTMLElement>('[data-route-id]').forEach((el) => {
    const id = el.dataset.routeId;
    if (!id) return;
    const exactList = exact.get(id) ?? [];
    exactList.push(el);
    exact.set(id, exactList);
    for (const extra of (el.dataset.routeAlso ?? '').split(/\s+/)) {
      if (!extra) continue;
      const list = also.get(extra) ?? [];
      list.push(el);
      also.set(extra, list);
    }
  });
  const resolved = new Map<string, HTMLElement>();
  for (const [id, list] of also) resolved.set(id, preferSpatial(list));
  for (const [id, list] of exact) resolved.set(id, preferSpatial(list));
  return resolved;
}

function indexObstacles(root: HTMLElement): (Obstacle & { el: HTMLElement })[] {
  const origin = root.getBoundingClientRect();
  const found: (Obstacle & { el: HTMLElement })[] = [];
  root.querySelectorAll<HTMLElement>('[data-route-obstacle]').forEach((el) => {
    if (el.closest('.planet-view-switcher')) return;
    const rect = el.getBoundingClientRect();
    found.push({
      el,
      x: rect.left - origin.left + root.scrollLeft + rect.width / 2,
      y: rect.top - origin.top + root.scrollTop + rect.height / 2,
      r: Math.max(rect.width, rect.height) * 0.55,
    });
  });
  return found;
}

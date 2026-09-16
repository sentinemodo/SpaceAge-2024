import { useMemo } from 'react';

function hashSeed(id: string): number {
  let h = 2166136261;
  for (let i = 0; i < id.length; i++) {
    h ^= id.charCodeAt(i);
    h = Math.imul(h, 16777619);
  }
  return h >>> 0;
}

function mulberry32(seed: number) {
  return () => {
    seed |= 0;
    seed = (seed + 0x6d2b79f5) | 0;
    let t = Math.imul(seed ^ (seed >>> 15), 1 | seed);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

export function BeltDotField({ seed, count = 52 }: { seed: string; count?: number }) {
  const dots = useMemo(() => {
    const rnd = mulberry32(hashSeed(seed));
    return Array.from({ length: count }, (_, i) => ({
      id: i,
      x: rnd() * 96 + 2,
      y: rnd() * 94 + 3,
      r: 1.2 + rnd() * 3.8,
      shade: 0.22 + rnd() * 0.55,
    }));
  }, [seed, count]);

  return (
    <div className="belt-dot-field" aria-hidden>
      {dots.map((d) => (
        <span
          key={d.id}
          className="belt-dot"
          style={{
            left: `${d.x}%`,
            top: `${d.y}%`,
            width: d.r,
            height: d.r,
            opacity: d.shade,
          }}
        />
      ))}
    </div>
  );
}

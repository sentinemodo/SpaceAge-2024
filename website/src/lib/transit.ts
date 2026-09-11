/** Port of Game/game/SpaceTransit.cs DurationWeeks + mass factor (Phase 4 /eta). */

export function durationWeeksRaw(deltaAu: number): number {
  if (deltaAu < 0.1) {
    return 50 * deltaAu;
  }
  const ln = Math.log;
  return 6 + (33 * ln((1 + deltaAu) / 2.7)) / ln(80 / 2.7);
}

export function massFactor(thrust: number, mass: number): number {
  const load = thrust / Math.max(mass, 1);
  const referenceLoad = 40000 / 4150;
  return Math.min(1.5, Math.max(0.67, load / referenceLoad));
}

export function durationWeeks(deltaAu: number, catalogSpeed: number, thrust: number, mass: number): number {
  const effectiveSpeed = catalogSpeed * massFactor(thrust, mass);
  const raw = durationWeeksRaw(deltaAu);
  return Math.ceil(raw / effectiveSpeed);
}

export function parseMassLine(text: string): { thrust: number; mass: number } | null {
  const m = text.match(/mass:\s*(\d+)\s*\/\s*(\d+)/i);
  if (!m) return null;
  return { thrust: parseInt(m[1], 10), mass: parseInt(m[2], 10) };
}

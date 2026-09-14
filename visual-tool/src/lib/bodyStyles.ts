import type { SystemBodyNode } from '../parsers/reportXml';

const PLANET_TYPE_COLORS: Record<string, string> = {
  ocean: '#3a7bd5',
  grassl: '#4a9e5c',
  desert: '#c4a35a',
  dust: '#a8927a',
  ice: '#b8d4e8',
  lava: '#d45a3a',
  gas: '#c9a0dc',
  gasgnt: '#b87edc',
  gasgnt2: '#a86ed0',
  rock: '#8a8a8a',
  arbor: '#4a9e5c',
};

const MOON_TYPE_COLORS: Record<string, string> = {
  rock: '#9aa0a8',
  ice: '#c8dce8',
  dust: '#b0a090',
};

export function bodyTypeColor(body: SystemBodyNode): string {
  const type = (body.planetType || '').toLowerCase();
  if (body.kind === 'moon') return MOON_TYPE_COLORS[type] || '#9aa0a8';
  if (body.kind === 'planet') return PLANET_TYPE_COLORS[type] || '#5a8fc4';
  if (body.kind === 'belt') return '#8a7a60';
  if (body.kind === 'alderson') return '#9fd4ff';
  return '#8a9bb5';
}

/** Icon scale from AU band (log-ish). Moons smaller than planets. */
export function bodyIconScale(body: SystemBodyNode): number {
  const base = body.kind === 'moon' ? 0.75 : body.kind === 'planet' ? 1 : 0.85;
  const auFactor = body.au > 0 ? Math.min(1.4, 0.85 + Math.log10(body.au + 1) * 0.15) : 1;
  return base * auFactor;
}

export function starAmberGradient(starType?: string): string {
  const t = (starType || '').toLowerCase();
  if (t.includes('m')) return 'linear-gradient(135deg, #ffb347 0%, #e8943a 45%, #c76b28 100%)';
  if (t.includes('k')) return 'linear-gradient(135deg, #ffe08a 0%, #f0c040 50%, #d4a020 100%)';
  if (t.includes('g')) return 'linear-gradient(135deg, #fff4c2 0%, #ffd966 50%, #e8b830 100%)';
  return 'linear-gradient(135deg, #ffc966 0%, #e8a030 50%, #c87820 100%)';
}

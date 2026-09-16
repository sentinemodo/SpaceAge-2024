import type { SystemBodyNode } from '../parsers/reportXml';

const PLANET_TYPE_COLORS: Record<string, string> = {
  ocean: '#3a7bd5',
  grassl: '#4a9e5c',
  desert: '#c4a35a',
  dust: '#a8927a',
  ice: '#b8d4e8',
  lava: '#d45a3a',
  vulcan: '#e06040',
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
  vulcan: '#c87860',
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

/** Star gradient for system map arc and galaxy system nodes. */
export function starAmberGradient(starType?: string): string {
  const t = (starType || '').toLowerCase();
  if (t.includes('m')) return 'linear-gradient(145deg, #ffb070 0%, #e87830 45%, #a84818 100%)';
  if (t.includes('k')) return 'linear-gradient(145deg, #ffe090 0%, #f0b838 50%, #c88820 100%)';
  if (t.includes('g')) return 'linear-gradient(145deg, #fff8d0 0%, #ffd850 50%, #e8a828 100%)';
  if (t.includes('f')) return 'linear-gradient(145deg, #fffef5 0%, #fff0c8 50%, #e8d898 100%)';
  if (t.includes('a')) return 'linear-gradient(145deg, #f0f4ff 0%, #d8e4ff 55%, #a8c0f0 100%)';
  if (t.includes('b')) return 'linear-gradient(145deg, #e8f0ff 0%, #b8d0ff 50%, #6898e8 100%)';
  if (t.includes('o')) return 'linear-gradient(145deg, #e8f4ff 0%, #98c8ff 45%, #4080e0 100%)';
  if (t.includes('d')) return 'linear-gradient(145deg, #f8fcff 0%, #d0e8ff 50%, #88b8f0 100%)';
  return 'linear-gradient(145deg, #ffc966 0%, #e8a030 50%, #c87820 100%)';
}

export function starTypeBorderColor(starType?: string): string {
  const t = (starType || '').toLowerCase();
  if (t.includes('m')) return '#e87830';
  if (t.includes('k')) return '#f0b838';
  if (t.includes('g')) return '#ffd850';
  if (t.includes('f')) return '#e8d898';
  if (t.includes('a')) return '#a8c0f0';
  if (t.includes('b')) return '#6898e8';
  if (t.includes('o')) return '#4080e0';
  return '#e8a030';
}

/** Left-edge body arc fill for regional / surface map view. */
export function bodyArcGradient(body: SystemBodyNode): string {
  const c = bodyTypeColor(body);
  return `linear-gradient(145deg, color-mix(in srgb, ${c} 55%, white) 0%, ${c} 52%, color-mix(in srgb, ${c} 65%, black) 100%)`;
}

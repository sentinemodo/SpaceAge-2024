/** Surface region type → map cell background (from play/campaign/data.xml ground types). */
export const REGION_TERRAIN_COLORS: Record<string, string> = {
  grassl: '#2d5a3a',
  dust: '#6b5a45',
  barren: '#4a4a52',
  mountn: '#5c5348',
  sea: '#1e4a6e',
  ocean: '#153550',
  smmast: '#3d6b8a',
  smcast: '#356078',
  lrmast: '#2a5870',
  lrcast: '#244a62',
  orbit: '#2a3548',
};

export function regionTerrainColor(terrainType?: string): string {
  if (!terrainType) return '#2a3548';
  return REGION_TERRAIN_COLORS[terrainType] ?? '#3a4558';
}

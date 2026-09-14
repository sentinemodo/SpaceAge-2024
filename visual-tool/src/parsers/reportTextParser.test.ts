// @vitest-environment jsdom
import { describe, it, expect } from 'vitest';
import { parseGalaxyText, enrichReportFromGalaxyText } from './reportTextParser';
import { parseReportXml } from './reportXml';
import { buildRegionMapCells, regionCatalog } from './reportXml';

describe('reportTextParser', () => {
  it('parses region coords from galaxy report text', () => {
    const text = `
Galaxy report:
  Sundock Grant [R00022] (3,3), grassland region, settlement capacity 8/0.
  Exits:
    Prairie [R00021] (2,3), grassland region, ground travel duration 3 weeks.
`;
    const index = parseGalaxyText(text);
    expect(index.regions.get('R00021')?.x).toBe(2);
    expect(index.regions.get('R00021')?.y).toBe(3);
  });

  it('parses stack report lines', () => {
    const text = '+ small cargo bay [230003], 2 small cargo bays [cargob], immobile.';
    const index = parseGalaxyText(text);
    expect(index.stackLines.get('230003')).toMatch(/small cargo bay/);
  });

  it('enriches region map with exit neighbours from text', () => {
    const xml = `<?xml version="1.0"?><report turn="1"><faction name="2"/><galaxy>
      <system name="SS0001"><planet name="P00001" name-en="Arbor">
        <region name="R00022" name-en="Sundock Grant" X="3" Y="3" type="grassl">
          <exit region="R00021"><exitmode mode="ground" duration="3"/></exit>
        </region>
      </planet></system>
    </galaxy></report>`;
    const galaxyText = 'Prairie [R00021] (2,3), grassland region, ground travel duration 3 weeks.';
    const report = enrichReportFromGalaxyText(parseReportXml(xml), galaxyText);
    const body = report.systemDetails[0].bodies[0];
    const cells = buildRegionMapCells(body, regionCatalog(report), new Set(['R00022']));
    expect(cells.some((c) => c.id === 'R00021')).toBe(true);
  });
});

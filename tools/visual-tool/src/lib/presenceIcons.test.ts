// @vitest-environment jsdom
import { describe, expect, it } from 'vitest';
import { parseReportXml, type ParsedReport, type StackNode } from '../parsers/reportXml';
import {
  anomalyNeedsInvestigation,
  buildPresenceIndex,
  presenceIconsForLocations,
  regionCellMarks,
  regionCellShowsIcons,
  stackPresenceKind,
} from './presenceIcons';

function stack(partial: Partial<StackNode> & Pick<StackNode, 'id'>): StackNode {
  return {
    name: partial.id,
    children: [],
    upkeep: [],
    moduleCount: 1,
    persons: [],
    ...partial,
  };
}

describe('stackPresenceKind', () => {
  it('maps each presence group once, including NPC cities and wild fauna', () => {
    expect(stackPresenceKind(stack({ id: '1', type: 'corphq' }))?.toString()).toBe('hq');
    expect(stackPresenceKind(stack({ id: '2', type: 'brnofc' }))).toBe('hq');
    expect(stackPresenceKind(stack({ id: '3', type: 'city', faction: '1' }))).toBe('habitat');
    expect(stackPresenceKind(stack({ id: '4', type: 'mtrply', faction: '9' }))).toBe('habitat');
    expect(stackPresenceKind(stack({ id: '5', type: 'tanks' }))).toBe('ground');
    expect(stackPresenceKind(stack({ id: '6', type: 'inftry', faction: '2' }))).toBe('ground');
    expect(stackPresenceKind(stack({ id: '7', type: 'gunbot' }))).toBe('sea');
    expect(stackPresenceKind(stack({ id: '8', type: 'shuttl' }))).toBe('craft');
    expect(stackPresenceKind(stack({ id: '9', type: 'alndrn' }))).toBe('craft');
    expect(stackPresenceKind(stack({ id: '10', type: 'sshull' }))).toBe('hull');
    expect(stackPresenceKind(stack({ id: '11', type: 'arkhul' }))).toBe('hull');
    expect(stackPresenceKind(stack({ id: '12', type: 'orcmpx' }))).toBe('station');
    expect(stackPresenceKind(stack({ id: '13', type: 'brmstr' }))).toBe('fauna');
    expect(stackPresenceKind(stack({ id: '14', type: 'inftry', faction: '14' }))).toBe('fauna');
    expect(stackPresenceKind(stack({ id: '15', type: 'factry' }))).toBeNull();
    expect(stackPresenceKind(stack({ id: '16', type: 'cbridg' }))).toBeNull();
  });
});

describe('regionCellMarks', () => {
  it('hides the icon row until the cell can hold every icon', () => {
    expect(regionCellShowsIcons(54, 2)).toBe(true);
    expect(regionCellShowsIcons(54, 5)).toBe(false);
    expect(regionCellShowsIcons(110, 5)).toBe(true);
    expect(regionCellShowsIcons(27, 1)).toBe(true);
  });

  it('hides a region name that cannot fit the cell', () => {
    expect(regionCellMarks('West', 27, 0)).toEqual({ showName: false, showIcons: false });
    expect(regionCellMarks('Northwind', 27, 1)).toEqual({ showName: false, showIcons: true });
    expect(regionCellMarks('West', 54, 0).showName).toBe(true);
    expect(regionCellMarks('Farm Belt', 54, 0).showName).toBe(true);
    expect(regionCellMarks('Northwind', 72, 0).showName).toBe(true);
  });
});

describe('buildPresenceIndex', () => {
  it('keeps one icon per group at a location and skips a resolved anomaly', () => {
    const xml = `<?xml version="1.0"?><report turn="1"><faction name="2"/>
      <galaxy><system name="SS0001"><planet name="P1">
        <region name="R1" name-en="Vale" X="0" Y="0" type="plain">
          <anomaly type="ruins" points="4"><resolved faction="2"/></anomaly>
          <modulestack name="10" type="city" quantity="10" faction="1">
            <modulestack name="11" type="corphq" quantity="1" faction="2"/>
            <modulestack name="12" type="city" quantity="3" faction="1"/>
          </modulestack>
          <modulestack name="13" type="trucks" quantity="4" faction="2"/>
          <modulestack name="14" type="tanks" quantity="2" faction="2"/>
        </region>
        <region name="R2" name-en="Wild" X="1" Y="0" type="plain">
          <anomaly type="signal" points="2"/>
          <modulestack name="20" type="brmstr" quantity="6" faction="14"/>
        </region>
        <orbit name="O1">
          <modulestack name="30" type="shuttl" quantity="2" faction="2"/>
          <modulestack name="31" type="sshull" quantity="1" faction="2"/>
          <modulestack name="32" type="orcmpx" quantity="1" faction="2"/>
        </orbit>
      </planet></system></galaxy></report>`;
    const report = parseReportXml(xml) as ParsedReport;
    const index = buildPresenceIndex(report);
    expect(index.get('R1')?.map((icon) => icon.kind)).toEqual(['hq', 'habitat', 'ground']);
    expect(index.get('R2')?.map((icon) => icon.kind)).toEqual(['anomaly', 'fauna']);
    expect(index.get('O1')?.map((icon) => icon.kind)).toEqual(['craft', 'hull', 'station']);
    expect(presenceIconsForLocations(index, ['R1', 'R2', 'O1']).map((icon) => icon.kind)).toEqual([
      'hq', 'habitat', 'ground', 'craft', 'hull', 'station', 'anomaly', 'fauna',
    ]);
    expect(anomalyNeedsInvestigation(report.systemDetails[0].bodies[0].regions[0].anomaly, '2')).toBe(false);
  });
});

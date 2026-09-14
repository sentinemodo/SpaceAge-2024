import type { ParsedReport, RegionNode, StackNode, StarNode } from './reportXml';

/** Region line: `Prairie [R00021] (2,3), grassland region` */
const REGION_LINE =
  /^\s*(.+?)\s*\[([A-Za-z0-9]+)\]\s*\((\d+)\s*,\s*(\d+)\)\s*,\s*(.+?)\s+region/i;

/** Exit target line under Exits: (indented) */
const EXIT_REGION_LINE =
  /^\s+(.+?)\s*\[([A-Za-z0-9]+)\]\s*\((\d+)\s*,\s*(\d+)\)\s*,\s*(.+)$/i;

/** Stack line: `+ small cargo bay [230003], 2 small cargo bays [cargob], immobile.` */
const STACK_LINE =
  /^\s*\+?\s*(.+?)\s*\[([A-Za-z0-9]+)\]\s*,\s*(.+?)\s*,\s*(\w+)\.?\s*$/;

/** Star line: `* Sol [S00001] (0, 0, 0), M4 star, unexplored.` */
const STAR_LINE =
  /^\s*\*\s*(.+?)\s*\[([A-Za-z0-9]+)\][^,]*,\s*(.+?)\s+star(?:,\s*(unexplored))?\.?\s*$/i;

export interface GalaxyTextIndex {
  regions: Map<string, Partial<RegionNode>>;
  stackLines: Map<string, string>;
  stars: Map<string, StarNode>;
  /** Full report line keyed by region id (exits and region headers). */
  regionReportLines: Map<string, string>;
}

export function parseGalaxyText(galaxyText: string): GalaxyTextIndex {
  const regions = new Map<string, Partial<RegionNode>>();
  const stackLines = new Map<string, string>();
  const stars = new Map<string, StarNode>();
  const regionReportLines = new Map<string, string>();

  for (const rawLine of galaxyText.split('\n')) {
    const line = rawLine.trimEnd();
    const trimmed = line.trim();

    const regionMatch = line.match(REGION_LINE);
    if (regionMatch) {
      const [, name, id, x, y, terrainText] = regionMatch;
      const terrainType = terrainText.trim().split(/\s+/)[0]?.toLowerCase();
      regions.set(id, {
        id,
        name: name.trim(),
        x: parseInt(x, 10),
        y: parseInt(y, 10),
        terrainType: terrainType || undefined,
        systemId: '',
      });
      regionReportLines.set(id, trimmed.endsWith('.') ? trimmed : `${trimmed}.`);
      continue;
    }

    const exitMatch = line.match(EXIT_REGION_LINE);
    if (exitMatch) {
      const [, , id, x, y] = exitMatch;
      if (!regions.has(id)) {
        regions.set(id, {
          id,
          name: exitMatch[1].trim(),
          x: parseInt(x, 10),
          y: parseInt(y, 10),
          systemId: '',
        });
      }
      regionReportLines.set(id, trimmed.endsWith('.') ? trimmed : `${trimmed}.`);
      continue;
    }

    const stackMatch = line.match(STACK_LINE);
    if (stackMatch?.[2]) {
      const label = line.replace(/^\s*\+?\s*/, '+ ').trim();
      stackLines.set(stackMatch[2], label.endsWith('.') ? label : `${label}.`);
      continue;
    }

    const starMatch = line.match(STAR_LINE);
    if (starMatch) {
      const [, name, id, starType, unexploredFlag] = starMatch;
      stars.set(id, {
        id,
        name: name.trim(),
        starType: starType.trim(),
        unexplored: !!unexploredFlag,
      });
    }
  }

  return { regions, stackLines, stars, regionReportLines };
}

function mergeRegion(r: RegionNode, fromText?: Partial<RegionNode>): RegionNode {
  if (!fromText) return r;
  return {
    ...r,
    name: r.name || fromText.name || r.id,
    x: r.x ?? fromText.x,
    y: r.y ?? fromText.y,
    terrainType: r.terrainType || fromText.terrainType,
  };
}

function attachStackLines(stacks: StackNode[], stackLines: Map<string, string>): StackNode[] {
  return stacks.map((s) => ({
    ...s,
    reportLine: stackLines.get(s.id) || s.reportLine,
    children: attachStackLines(s.children, stackLines),
  }));
}

/** Merge galaxy report text coords and stack labels into a parsed XML report. */
export function enrichReportFromGalaxyText(
  report: ParsedReport,
  galaxyText: string
): ParsedReport {
  if (!galaxyText.trim()) return report;
  const index = parseGalaxyText(galaxyText);

  const regions = report.regions.map((r) => mergeRegion(r, index.regions.get(r.id)));

  const systemDetails = report.systemDetails.map((sys) => {
    const starFromText = sys.star?.id ? index.stars.get(sys.star.id) : undefined;
    const star = sys.star
      ? { ...sys.star, ...starFromText, unexplored: starFromText?.unexplored ?? sys.star.unexplored }
      : starFromText;

    return {
      ...sys,
      star,
      bodies: sys.bodies.map((body) => ({
        ...body,
        regions: body.regions.map((r) => mergeRegion(r, index.regions.get(r.id))),
      })),
    };
  });

  const regionHints: RegionNode[] = [...index.regions.values()]
    .filter((r) => r.id && r.x != null && r.y != null)
    .map((r) => ({
      id: r.id!,
      name: r.name || r.id!,
      systemId: r.systemId || '',
      bodyId: r.bodyId,
      x: r.x,
      y: r.y,
      terrainType: r.terrainType,
    }));

  return {
    ...report,
    regions,
    systemDetails,
    stacks: attachStackLines(report.stacks, index.stackLines),
    regionHints,
    regionReportLines: Object.fromEntries(index.regionReportLines),
  };
}

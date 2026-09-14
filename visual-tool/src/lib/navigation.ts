import type { ParsedReport } from '../parsers/reportXml';
import {
  flattenStacks,
  getBodyDetail,
  getRegionDetail,
  regionCatalog,
} from '../parsers/reportXml';

export interface MapNavigation {
  clearUnitSelection: () => void;
  setPanel: (p: 'map') => void;
  setFilterSystems: (ids: string[]) => void;
  setSystemViewId: (id: string | null) => void;
  setBodyViewId: (id: string | null) => void;
  setFilterBodyId: (id: string | null) => void;
  setFilterRegionId: (id: string | null) => void;
  setFilterOrbitId: (id: string | null) => void;
  setFilterStar: (v: boolean) => void;
  selectStack: (id: string) => void;
  selectPerson: (id: string) => void;
}

function clearLocationFocus(nav: MapNavigation) {
  nav.setFilterRegionId(null);
  nav.setFilterOrbitId(null);
  nav.setFilterStar(false);
}

function findBodyForRegion(
  report: ParsedReport,
  regionId: string
): { systemId: string; bodyId: string } | null {
  for (const sys of report.systemDetails) {
    for (const body of sys.bodies) {
      if (body.regions.some((r) => r.id === regionId)) {
        return { systemId: sys.id, bodyId: body.id };
      }
    }
  }
  const hint = regionCatalog(report).get(regionId);
  if (hint?.bodyId && hint.systemId) {
    return { systemId: hint.systemId, bodyId: hint.bodyId };
  }
  return null;
}

function focusBody(
  report: ParsedReport,
  nav: MapNavigation,
  systemId: string,
  bodyId: string,
  regionId?: string | null
) {
  nav.setFilterSystems([systemId]);
  nav.setSystemViewId(systemId);
  nav.setBodyViewId(bodyId);
  nav.setFilterBodyId(bodyId);
  nav.setFilterStar(false);
  nav.setFilterOrbitId(null);
  nav.setFilterRegionId(regionId ?? null);
}

/** Focus map / tree on an object id from report text (region, body, stack, etc.). */
export function focusReportId(report: ParsedReport, id: string, nav: MapNavigation): boolean {
  nav.setPanel('map');
  nav.clearUnitSelection();

  const stack = flattenStacks(report.stacks).find((s) => s.id === id);
  if (stack) {
    nav.selectStack(id);
    if (stack.systemId) {
      nav.setFilterSystems([stack.systemId]);
      nav.setSystemViewId(stack.systemId);
    }
    if (stack.locationId) {
      const region = getRegionDetail(report, stack.locationId);
      const bodyLoc = region ? findBodyForRegion(report, stack.locationId) : null;
      if (bodyLoc) {
        focusBody(report, nav, bodyLoc.systemId, bodyLoc.bodyId, stack.locationId);
      } else if (stack.locationId.startsWith('O')) {
        for (const sys of report.systemDetails) {
          for (const body of sys.bodies) {
            if (body.orbitIds.includes(stack.locationId)) {
              focusBody(report, nav, sys.id, body.id);
              nav.setFilterOrbitId(stack.locationId);
              return true;
            }
          }
        }
      } else {
        nav.setFilterRegionId(stack.locationId);
      }
    }
    return true;
  }

  for (const sys of report.systemDetails) {
    if (sys.star?.id === id) {
      nav.setFilterSystems([sys.id]);
      nav.setSystemViewId(sys.id);
      nav.setBodyViewId(null);
      nav.setFilterBodyId(null);
      clearLocationFocus(nav);
      nav.setFilterStar(true);
      return true;
    }
  }

  const region = getRegionDetail(report, id);
  if (region) {
    const loc = findBodyForRegion(report, id);
    const systemId = loc?.systemId || region.systemId;
    if (!systemId) return false;
    if (loc?.bodyId) {
      focusBody(report, nav, loc.systemId, loc.bodyId, id);
    } else {
      nav.setFilterSystems([systemId]);
      nav.setSystemViewId(systemId);
      nav.setFilterRegionId(id);
      nav.setFilterOrbitId(null);
      nav.setFilterStar(false);
    }
    return true;
  }

  for (const sys of report.systemDetails) {
    const body = getBodyDetail(report, sys.id, id);
    if (body) {
      focusBody(report, nav, sys.id, id);
      return true;
    }
    if (sys.id === id) {
      nav.setFilterSystems([id]);
      nav.setSystemViewId(id);
      nav.setBodyViewId(null);
      nav.setFilterBodyId(null);
      clearLocationFocus(nav);
      return true;
    }
  }

  for (const sys of report.systemDetails) {
    for (const body of sys.bodies) {
      if (body.orbitIds.includes(id)) {
        focusBody(report, nav, sys.id, body.id);
        nav.setFilterOrbitId(id);
        return true;
      }
    }
  }

  const personStack = flattenStacks(report.stacks).find((s) => s.persons.some((p) => p.id === id));
  if (personStack) {
    nav.selectPerson(id);
    return focusReportId(report, personStack.id, { ...nav, clearUnitSelection: () => {} });
  }

  return false;
}

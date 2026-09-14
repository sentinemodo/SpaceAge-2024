import { useState, type ReactNode } from 'react';

import type {
  ParsedReport,
  PersonNode,
  RegionNode,
  StackNode,
  StarNode,
  SystemBodyNode,
  SystemDetail,
} from '../parsers/reportXml';
import {
  describeExitTarget,
  getBodyDetail,
  getRegionDetail,
  getSystemDetail,
  regionCatalog,
} from '../parsers/reportXml';

import { ClickableInline } from './ClickableReportText';
import { PersonDescription } from './PersonDescription';
import { UnitDescription } from './UnitDescription';

function MetaRow({ label, value }: { label: string; value?: string | null }) {
  if (!value) return null;
  return (
    <div className="obj-meta-row">
      <span className="obj-meta-label">{label}</span>
      <span className="obj-meta-value">{value}</span>
    </div>
  );
}

function UnexploredNote() {
  return <p className="obj-unexplored">Unexplored — limited data in faction report.</p>;
}

function titled(name: string, id?: string) {
  return id ? `${name} [${id}]` : name;
}

function RegionDescription({
  region,
  catalog,
  regionReportLines,
  onFocusId,
}: {
  region: RegionNode;
  catalog: Map<string, RegionNode>;
  regionReportLines?: Record<string, string>;
  onFocusId?: (id: string) => void;
}) {
  const settlement = region.capacities?.find((c) => c.group === 'settlement' && c.quantity > 0);

  return (
    <div className="object-description-body">
      <div className="obj-meta">
        <MetaRow label="Type" value={region.terrainType} />
        {region.x != null && region.y != null && (
          <MetaRow label="Grid" value={`${region.x}, ${region.y}`} />
        )}
        {settlement && <MetaRow label="Settlement" value={String(settlement.quantity)} />}
      </div>
      {region.description && <p className="obj-desc">{region.description}</p>}
      {region.resources && region.resources.length > 0 && (
        <div className="obj-block">
          <strong>Resources</strong>
          <ul>
            {region.resources.map((r) => (
              <li key={r.type}>{r.type}: {r.quantity}</li>
            ))}
          </ul>
        </div>
      )}
      {region.deepResources && region.deepResources.length > 0 && (
        <div className="obj-block">
          <strong>Deep resources</strong>
          <ul>
            {region.deepResources.map((r) => (
              <li key={r.type}>{r.type}: {r.quantity}</li>
            ))}
          </ul>
        </div>
      )}
      {region.anomaly && (
        <div className="obj-block">
          <strong>Anomaly</strong>
          <p className="obj-desc">
            {region.anomaly.type}
            {region.anomaly.points != null ? ` (${region.anomaly.points} pts)` : ''}
            {region.anomaly.description ? ` — ${region.anomaly.description}` : ''}
          </p>
        </div>
      )}
      {region.exits && region.exits.length > 0 && (
        <div className="obj-block">
          <strong>Exits</strong>
          <ul>
            {region.exits.map((ex) => {
              const { label, missingCoords } = describeExitTarget(ex, catalog, regionReportLines);
              return (
                <li key={`${ex.targetKind}-${ex.targetId}`} className={missingCoords ? 'obj-exit-missing' : ''}>
                  <ClickableInline text={label} onFocusId={onFocusId} />
                </li>
              );
            })}
          </ul>
        </div>
      )}
    </div>
  );
}

function StarDescription({ star, detail }: { star: StarNode; detail: SystemDetail }) {
  return (
    <div className="object-description-body">
      <div className="obj-meta">
        <MetaRow label="Kind" value="Star" />
        <MetaRow label="System" value={detail.name} />
        <MetaRow label="Type" value={star.starType} />
      </div>
      {star.unexplored && <UnexploredNote />}
      {detail.surveyFlavor && <p className="obj-desc">{detail.surveyFlavor}</p>}
    </div>
  );
}

function OrbitDescription({
  orbitId,
  body,
}: {
  orbitId: string;
  body: SystemBodyNode;
}) {
  const orbit = body.orbits.find((o) => o.id === orbitId);
  return (
    <div className="object-description-body">
      <div className="obj-meta">
        <MetaRow label="Parent" value={body.name} />
      </div>
      {orbit && orbit.resources.length > 0 && (
        <div className="obj-block">
          <strong>Resources</strong>
          <ul>
            {orbit.resources.map((r) => (
              <li key={r.type}>{r.type}: {r.quantity}</li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}

function BodyDescription({ body }: { body: SystemBodyNode }) {
  return (
    <div className="object-description-body">
      <div className="obj-meta">
        <MetaRow label="Kind" value={body.kind} />
        <MetaRow label="AU" value={String(body.au)} />
        <MetaRow label="Type" value={body.planetType} />
        <MetaRow label="Gravity" value={body.gravity} />
        <MetaRow label="Temperature" value={body.temperature} />
        <MetaRow label="Atmosphere" value={body.atmosphere} />
      </div>
      {body.unexplored && <UnexploredNote />}
      {body.description && <p className="obj-desc">{body.description}</p>}
      {body.orbits.length > 0 && (
        <div className="obj-block">
          <strong>Orbits</strong>
          {body.orbits.map((orbit) => (
            <div key={orbit.id} className="obj-orbit">
              <div>{orbit.id}</div>
              {orbit.resources.length > 0 && (
                <ul>
                  {orbit.resources.map((r) => (
                    <li key={r.type}>{r.type}: {r.quantity}</li>
                  ))}
                </ul>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export function ObjectDescription({
  report,
  selectedStack,
  selectedPerson,
  filterRegionId,
  filterBodyId,
  filterOrbitId,
  filterStar,
  systemViewId,
  onFocusId,
}: {
  report: ParsedReport;
  selectedStack: StackNode | null;
  selectedPerson: PersonNode | null;
  filterRegionId: string | null;
  filterBodyId: string | null;
  filterOrbitId: string | null;
  filterStar: boolean;
  systemViewId: string | null;
  onFocusId?: (id: string) => void;
}) {
  const [collapsed, setCollapsed] = useState(false);
  const catalog = regionCatalog(report);
  const systemDetail = systemViewId ? getSystemDetail(report, systemViewId) : undefined;
  const regionReportLines = report.regionReportLines;

  let title = 'Object info';
  let body: ReactNode = (
    <p className="obj-desc">Select a unit, person, region, or body on the map.</p>
  );

  if (selectedPerson) {
    title = titled(selectedPerson.name || selectedPerson.id, selectedPerson.id);
    body = <PersonDescription person={selectedPerson} />;
  } else if (selectedStack) {
    title = titled(selectedStack.name || selectedStack.type || selectedStack.id, selectedStack.id);
    body = <UnitDescription stack={selectedStack} />;
  } else if (filterOrbitId && filterBodyId && systemDetail) {
    const b = getBodyDetail(report, systemViewId!, filterBodyId);
    if (b) {
      title = titled(`Orbit ${filterOrbitId}`, filterOrbitId);
      body = <OrbitDescription orbitId={filterOrbitId} body={b} />;
    }
  } else if (filterRegionId) {
    const region = getRegionDetail(report, filterRegionId);
    if (region) {
      title = titled(region.name, region.id);
      body = (
        <RegionDescription
          region={region}
          catalog={catalog}
          regionReportLines={regionReportLines}
          onFocusId={onFocusId}
        />
      );
    }
  } else if (filterStar && systemDetail?.star) {
    title = titled(systemDetail.star.name, systemDetail.star.id);
    body = <StarDescription star={systemDetail.star} detail={systemDetail} />;
  } else if (filterBodyId && systemViewId) {
    const b = getBodyDetail(report, systemViewId, filterBodyId);
    if (b) {
      title = titled(b.name, b.id);
      body = <BodyDescription body={b} />;
    }
  }

  return (
    <div className={`object-description ${collapsed ? 'object-description-collapsed' : ''}`}>
      <div className="object-description-header">
        <button
          type="button"
          className="obj-collapse-btn"
          onClick={() => setCollapsed((c) => !c)}
          aria-expanded={!collapsed}
        >
          {collapsed ? '▸' : '▾'} {title}
        </button>
      </div>
      {!collapsed && body}
    </div>
  );
}

import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  login,
  fetchMeta,
  fetchReportXml,
  fetchReportTxt,
  fetchReportSections,
  viewAsFaction,
  parseOrders,
  runBattleSim,
  submitOrders,
  getToken,
  setToken,
  type FactionOption,
  type ReportSection,
  type SessionMeta,
} from './api/client';
import {
  parseReportXml,
  flattenStacks,
  estimateMoveWeeks,
  findOrderForStack,
  findOrderForTarget,
  findPersonInStacks,
  formatOrderEntry,
  resolveMoveRouteSystems,
  getSystemDetail,
  getBodyDetail,
  childBodies,
  groupSystemOrbitBands,
  collectBodyLocationIds,
  bodyKindIcon,
  buildRegionMapCells,
  regionCatalog,
  regionMapGridSize,
  ownedRegionIds,
  systemHasPresence,
  locationIdsWithPresence,
  type ParsedReport,
  type StackNode,
  type SystemBodyNode,
  type SystemDetail,
  type SystemNode,
} from './parsers/reportXml';
import { enrichReportFromGalaxyText } from './parsers/reportTextParser';
import { regionTerrainColor } from './lib/terrainColors';
import { bodyTypeColor, bodyIconScale, starAmberGradient } from './lib/bodyStyles';
import { SidePanel, type SideTab } from './components/SidePanel';
import { ClickableReportText } from './components/ClickableReportText';
import { TechnologyCatalog } from './components/TechnologyCatalog';
import { ResizeHandle } from './components/ResizeHandle';
import { buildSystemLinkPath } from './lib/mapLinks';
import { focusReportId } from './lib/navigation';
import {
  sectionText,
  filterReportLines,
  extractDiplomacyText,
  splitTechnologyReport,
  bankReportText,
} from './lib/reportSections';

const REGION_ZOOM_BASE = 1.5;

type Panel = 'map' | 'tech' | 'diplomacy' | 'contracts' | 'bank' | 'battle' | 'faction';

function LoginScreen({ onLogin }: { onLogin: () => void }) {
  const [factionId, setFactionId] = useState('2');
  const [password, setPassword] = useState('');
  const [gmKey, setGmKey] = useState('');
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setNotice('');
    try {
      const trimmedGm = gmKey.trim();
      const data = await login(parseInt(factionId, 10), password, trimmedGm || undefined);
      if (trimmedGm && data.admin === undefined) {
        setNotice('Game-host is outdated — restart it so admin login works (stop the old node process on port 8787, then npm start).');
      } else if (trimmedGm && !data.admin) {
        setNotice('GM key not accepted. Use dev-gm-key (or your GAME_HOST_GM_KEY env value).');
      } else if (data.admin) {
        setNotice('Admin access enabled — use the faction dropdown in the header to browse reports.');
      }
      onLogin();
    } catch (err) {
      setError(String(err));
    }
  }

  return (
    <form className="login-form" onSubmit={handleSubmit}>
      <h1>SpaceAge Client</h1>
      <p>Closed campaign — invitation only</p>
      <label>
        Faction (2–11)
        <input type="number" min={2} max={11} value={factionId} onChange={(e) => setFactionId(e.target.value)} />
      </label>
      <label>
        Password
        <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
      </label>
      <label>
        GM key (optional — browse all factions)
        <input type="password" value={gmKey} onChange={(e) => setGmKey(e.target.value)} placeholder="dev-gm-key" />
      </label>
      {error && <p className="warnings">{error}</p>}
      {notice && <p className="obj-desc">{notice}</p>}
      <button type="submit">Enter client</button>
    </form>
  );
}

function StarMapLinks({
  links,
  systems,
  route,
}: {
  links: { from: SystemNode; to: SystemNode; stable: boolean; homePair: boolean }[];
  systems: SystemNode[];
  route: { from: SystemNode; to: SystemNode } | null;
}) {
  return (
    <svg className="star-map-links" viewBox="0 0 100 100" preserveAspectRatio="none" aria-hidden>
      {links.map((link) => (
        <path
          key={`${link.from.id}-${link.to.id}`}
          d={buildSystemLinkPath(link.from, link.to, systems)}
          className={`alderson-path ${link.stable ? (link.homePair ? 'home' : 'hub') : 'unstable'}`}
          fill="none"
        />
      ))}
      {route && (
        <path
          d={buildSystemLinkPath(route.from, route.to, systems)}
          className="route-path"
          fill="none"
        />
      )}
    </svg>
  );
}

function RegionMap({
  body,
  report,
  selectedRegionId,
  onSelectRegion,
  zoom,
  large,
}: {
  body: SystemBodyNode;
  report: ParsedReport;
  selectedRegionId: string | null;
  onSelectRegion: (regionId: string) => void;
  zoom: number;
  large?: boolean;
}) {
  const cells = buildRegionMapCells(body, regionCatalog(report), ownedRegionIds(report));
  if (cells.length === 0) return null;
  const { gridW, gridH, minX, minY } = regionMapGridSize(cells);
  const cell = (large ? 36 : 28) * zoom;

  return (
    <div
      className={`region-map ${large ? 'region-map-large' : ''}`}
      style={{
        gridTemplateColumns: `repeat(${gridW}, ${cell}px)`,
        gridTemplateRows: `repeat(${gridH}, ${cell}px)`,
        width: gridW * cell + (large ? 18 : 10),
        height: gridH * cell + (large ? 18 : 10),
      }}
    >
      {cells.map((region) => {
        const rx = (region.x ?? 0) - minX + 1;
        const ry = (region.y ?? 0) - minY + 1;
        return (
          <button
            key={region.id}
            type="button"
            className={`region-cell ${selectedRegionId === region.id ? 'selected' : ''} ${!region.hasPresence ? 'no-presence' : ''} ${region.reachableViaExit ? 'exit-only' : ''}`}
            style={{
              gridColumn: rx,
              gridRow: ry,
              backgroundColor: regionTerrainColor(region.terrainType),
            }}
            title={`${region.name} [${region.id}]${region.terrainType ? ` · ${region.terrainType}` : ''}`}
            onClick={() => onSelectRegion(region.id)}
          >
            <span className="region-cell-label">{region.name.split(' ')[0]}</span>
          </button>
        );
      })}
    </div>
  );
}

function BodyIconButton({
  body,
  selected,
  compact,
  hasPresence,
  onSelect,
  onOpen,
}: {
  body: SystemBodyNode;
  selected: boolean;
  compact?: boolean;
  hasPresence?: boolean;
  onSelect: () => void;
  onOpen: () => void;
}) {
  const scale = bodyIconScale(body);
  const color = bodyTypeColor(body);
  return (
    <button
      type="button"
      className={`body-icon body-icon-${body.kind} ${selected ? 'selected' : ''} ${compact ? 'compact' : ''} ${hasPresence ? 'has-presence' : ''}`}
      title={`${body.name} (${body.au} AU)${body.unexplored ? ' — unexplored' : ''}`}
      onClick={onSelect}
      onDoubleClick={(e) => {
        e.preventDefault();
        onOpen();
      }}
      style={{
        ['--body-color' as string]: color,
        ['--body-scale' as string]: String(scale),
      }}
    >
      <span className="body-icon-glyph" aria-hidden>{bodyKindIcon(body)}</span>
      {!compact && <span className="body-icon-name">{body.name}</span>}
    </button>
  );
}

function BandBodyStack({
  body,
  satellites,
  selectedBodyId,
  presenceIds,
  onSelectBody,
  onOpenBody,
}: {
  body: SystemBodyNode;
  satellites: SystemBodyNode[];
  selectedBodyId: string | null;
  presenceIds: Set<string>;
  onSelectBody: (bodyId: string) => void;
  onOpenBody: (bodyId: string) => void;
}) {
  const bodyPresence = presenceIds.has(body.id)
    || body.regions.some((r) => presenceIds.has(r.id))
    || body.orbitIds.some((o) => presenceIds.has(o));
  return (
    <div className="band-body-stack">
      <BodyIconButton
        body={body}
        hasPresence={bodyPresence}
        selected={selectedBodyId === body.id}
        onSelect={() => onSelectBody(body.id)}
        onOpen={() => onOpenBody(body.id)}
      />
      <span className="band-au">{body.au} AU</span>
      {satellites.length > 0 && (
        <div className="band-children">
          {satellites.map((child) => (
            <BodyIconButton
              key={child.id}
              body={child}
              compact
              hasPresence={presenceIds.has(child.id) || child.regions.some((r) => presenceIds.has(r.id))}
              selected={selectedBodyId === child.id}
              onSelect={() => onSelectBody(child.id)}
              onOpen={() => onOpenBody(child.id)}
            />
          ))}
        </div>
      )}
    </div>
  );
}

function SystemView({
  detail,
  selectedBodyId,
  filterStar,
  presenceIds,
  onSelectStar,
  onSelectBody,
  onOpenBody,
  onBack,
}: {
  detail: SystemDetail;
  selectedBodyId: string | null;
  filterStar: boolean;
  presenceIds: Set<string>;
  onSelectStar: () => void;
  onSelectBody: (bodyId: string) => void;
  onOpenBody: (bodyId: string) => void;
  onBack: () => void;
}) {
  const { bands, gates } = groupSystemOrbitBands(detail);
  const starGradient = starAmberGradient(detail.star?.starType);

  return (
    <div className="system-view">
      <div className="system-view-toolbar">
        <button type="button" className="system-view-back" onClick={onBack}>← Galaxy map</button>
        <strong>{detail.name}</strong>
        {detail.starName && <span className="system-view-star">{detail.starName}</span>}
      </div>
      <div className="system-orbit-map">
        <button
          type="button"
          className={`system-orbit-star-arc ${filterStar ? 'selected' : ''}`}
          style={{ background: starGradient }}
          title={detail.star?.name || detail.starName || 'Star'}
          onClick={onSelectStar}
        />
        {bands.map(({ body, children: satellites }) => (
          <div key={body.id} className="system-orbit-band">
            <BandBodyStack
              body={body}
              satellites={satellites}
              selectedBodyId={selectedBodyId}
              presenceIds={presenceIds}
              onSelectBody={onSelectBody}
              onOpenBody={onOpenBody}
            />
          </div>
        ))}
        {gates.length > 0 && (
          <div className="system-orbit-band system-orbit-band-gates">
            <div className="band-gates-stack">
              {gates.map((gate) => (
                <BodyIconButton
                  key={gate.id}
                  body={gate}
                  hasPresence={presenceIds.has(gate.id)}
                  selected={selectedBodyId === gate.id}
                  onSelect={() => onSelectBody(gate.id)}
                  onOpen={() => onOpenBody(gate.id)}
                />
              ))}
            </div>
            <span className="band-au">{gates[0].au} AU</span>
          </div>
        )}
      </div>
    </div>
  );
}

function BodyView({
  report,
  detail,
  body,
  selectedRegionId,
  filterOrbitId,
  regionZoom,
  presenceIds,
  onRegionZoom,
  onSelectRegion,
  onSelectOrbit,
  onOpenBody,
  onBack,
}: {
  report: ParsedReport;
  detail: SystemDetail;
  body: SystemBodyNode;
  selectedRegionId: string | null;
  filterOrbitId: string | null;
  regionZoom: number;
  presenceIds: Set<string>;
  onRegionZoom: (z: number) => void;
  onSelectRegion: (regionId: string) => void;
  onSelectOrbit: (orbitId: string, bodyId: string) => void;
  onOpenBody: (bodyId: string) => void;
  onBack: () => void;
}) {
  const parent = body.parentId ? detail.bodies.find((b) => b.id === body.parentId) : undefined;
  const siblings = parent ? childBodies(detail, parent.id) : childBodies(detail, body.id);
  const switchTargets = parent
    ? [parent, ...siblings]
    : body.kind === 'planet'
      ? [body, ...siblings]
      : [body];

  return (
    <div className="system-view">
      <div className="system-view-toolbar">
        <button type="button" className="system-view-back" onClick={onBack}>← {detail.name}</button>
        <strong>{body.name}</strong>
        <span className="system-view-star">{body.au} AU · {body.kind}</span>
      </div>
      <div className="region-view">
        {switchTargets.length > 1 && (
          <div className="region-view-switcher">
            {switchTargets.map((target) => (
              <BodyIconButton
                key={target.id}
                body={target}
                compact={target.id !== body.id}
                selected={target.id === body.id}
                onSelect={() => onOpenBody(target.id)}
                onOpen={() => onOpenBody(target.id)}
              />
            ))}
          </div>
        )}
        {body.kind === 'planet' && body.orbitIds.length > 0 && (
          <div className="body-view-orbit">
            <span className="body-view-orbit-label">Orbit</span>
            {body.orbitIds.map((orbitId) => (
              <button
                key={orbitId}
                type="button"
                className={`body-icon body-icon-orbit ${filterOrbitId === orbitId ? 'selected' : ''} ${presenceIds.has(orbitId) ? 'has-presence' : ''}`}
                title={`${body.name} orbit ${orbitId}`}
                onClick={() => onSelectOrbit(orbitId, body.id)}
              >
                <span className="body-icon-glyph" aria-hidden>◯</span>
              </button>
            ))}
          </div>
        )}
        {body.regions.length > 0 && (
          <div className="region-map-zoom-toolbar">
            <button type="button" onClick={() => onRegionZoom(Math.max(REGION_ZOOM_BASE * 0.5, regionZoom - REGION_ZOOM_BASE * 0.25))}>−</button>
            <span>{Math.round((regionZoom / REGION_ZOOM_BASE) * 100)}%</span>
            <button type="button" onClick={() => onRegionZoom(Math.min(REGION_ZOOM_BASE * 2.5, regionZoom + REGION_ZOOM_BASE * 0.25))}>+</button>
          </div>
        )}
        {body.regions.length > 0 ? (
          <div className="region-map-panel">
            <RegionMap
              body={body}
              report={report}
              selectedRegionId={selectedRegionId}
              onSelectRegion={onSelectRegion}
              zoom={regionZoom}
              large
            />
          </div>
        ) : (
          <p className="region-view-empty">
            No surface regions — units in orbit only.
            {body.orbitIds.length > 0 && ` Orbits: ${body.orbitIds.join(', ')}.`}
          </p>
        )}
        {body.regions.length > 0 && (
          <p className="region-view-hint">Click a cell to filter units to that region.</p>
        )}
      </div>
    </div>
  );
}

function ReportSearchText({
  text,
  query,
  onFocusId,
}: {
  text: string;
  query: string;
  onFocusId?: (id: string) => void;
}) {
  return (
    <ClickableReportText
      text={filterReportLines(text, query)}
      onFocusId={onFocusId}
    />
  );
}

export default function App() {
  const [authed, setAuthed] = useState(!!getToken());
  const [meta, setMeta] = useState<SessionMeta>({});
  const [adminHint, setAdminHint] = useState('');
  const [report, setReport] = useState<ParsedReport | null>(null);
  const [panel, setPanel] = useState<Panel>('map');
  const [filterSystems, setFilterSystems] = useState<string[]>([]);
  const [systemViewId, setSystemViewId] = useState<string | null>(null);
  const [bodyViewId, setBodyViewId] = useState<string | null>(null);
  const [filterBodyId, setFilterBodyId] = useState<string | null>(null);
  const [filterRegionId, setFilterRegionId] = useState<string | null>(null);
  const [filterOrbitId, setFilterOrbitId] = useState<string | null>(null);
  const [factionSearch, setFactionSearch] = useState('');
  const [selectedStack, setSelectedStack] = useState<string | null>(null);
  const [selectedPerson, setSelectedPerson] = useState<string | null>(null);
  const [orderText, setOrderText] = useState('');
  const [aiPromptText, setAiPromptText] = useState('');
  const [sidePanelWidth, setSidePanelWidth] = useState(320);
  const [orderPaneHeight, setOrderPaneHeight] = useState(220);
  const [aiPaneWidth, setAiPaneWidth] = useState(280);
  const [regionMapZoom, setRegionMapZoom] = useState(REGION_ZOOM_BASE);
  const [reportFullText, setReportFullText] = useState('');
  const [filterStar, setFilterStar] = useState(false);
  const [warnings, setWarnings] = useState<string[]>([]);
  const [parseErrors, setParseErrors] = useState<string[]>([]);
  const [sections, setSections] = useState<ReportSection[]>([]);
  const [battleSimXml, setBattleSimXml] = useState('');
  const [battleSimOutput, setBattleSimOutput] = useState('');
  const [sideTab, setSideTab] = useState<SideTab>('units');

  const load = useCallback(async () => {
    const m = await fetchMeta();
    setMeta(m);
    if (m.admin) {
      setAdminHint('');
    } else if (m.admin === undefined && 'engineVersion' in m) {
      setAdminHint('Game-host needs restart for admin browse (stop node on port 8787, run npm start in game-host).');
    } else {
      setAdminHint('');
    }
    try {
      const [xml, reportSections, fullText] = await Promise.all([
        fetchReportXml(),
        fetchReportSections(),
        fetchReportTxt().catch(() => ''),
      ]);
      const galaxyText = reportSections.find((s) => s.id === 'galaxy')?.text || '';
      setReport(enrichReportFromGalaxyText(parseReportXml(xml), galaxyText));
      setSections(reportSections);
      setReportFullText(fullText);
    } catch {
      setReport(null);
      setSections([]);
      setReportFullText('');
    }
  }, []);

  useEffect(() => {
    if (authed) load();
  }, [authed, load]);

  const flatStacks = useMemo(() => (report ? flattenStacks(report.stacks) : []), [report]);

  const filteredRoots = useMemo(() => {
    if (!report) return [];
    if (filterOrbitId) {
      return report.stacks.filter((s) => s.locationId === filterOrbitId);
    }
    if (filterRegionId) {
      return report.stacks.filter((s) => s.locationId === filterRegionId);
    }
    if (filterBodyId && systemViewId) {
      const detail = getSystemDetail(report, systemViewId);
      if (detail) {
        const locs = new Set(collectBodyLocationIds(detail, filterBodyId));
        return report.stacks.filter((s) => s.locationId && locs.has(s.locationId));
      }
    }
    if (filterSystems.length) {
      return report.stacks.filter((s) =>
        filterSystems.some((f) => s.systemId === f || s.locationId === f)
      );
    }
    return report.stacks;
  }, [report, filterSystems, filterRegionId, filterOrbitId, filterBodyId, systemViewId]);

  const atLocationLevel = !!(filterRegionId || filterBodyId || filterOrbitId);

  const systemView = report && systemViewId ? getSystemDetail(report, systemViewId) : undefined;
  const bodyView =
    report && systemViewId && bodyViewId
      ? getBodyDetail(report, systemViewId, bodyViewId)
      : undefined;
  const focusedSystemId = systemViewId || filterSystems[0] || null;

  const selected = flatStacks.find((s) => s.id === selectedStack);
  const selectedPersonNode = report && selectedPerson
    ? findPersonInStacks(report.stacks, selectedPerson)
    : undefined;
  const moveWeeks = selected ? estimateMoveWeeks(selected.mass) : null;
  const selectedOrder = report && selected
    ? findOrderForStack(report.orders, selected.id)
    : report && selectedPersonNode
      ? findOrderForTarget(report.orders, 'person', selectedPersonNode.id)
      : undefined;
  const selectedOrderLabel = selected
    ? (selected.name || selected.type)
    : selectedPersonNode
      ? (selectedPersonNode.name || selectedPersonNode.id)
      : null;

  const moveRoute = useMemo(() => {
    if (!report || !selected || !selectedOrder?.moveDestinations.length) return null;
    const endpoints = resolveMoveRouteSystems(selected, selectedOrder, report.regions);
    if (!endpoints) return null;
    const from = report.systems.find((s) => s.id === endpoints.fromSystemId);
    const to = report.systems.find((s) => s.id === endpoints.toSystemId);
    if (!from || !to) return null;
    return { from, to };
  }, [report, selected, selectedOrder]);

  const aldersonLines = useMemo(() => {
    if (!report) return [];
    const byId = new Map(report.systems.map((s) => [s.id, s]));
    return report.aldersonLinks
      .map((link) => {
        const from = byId.get(link.fromSystemId);
        const to = byId.get(link.toSystemId);
        if (!from || !to) return null;
        return { from, to, stable: link.stable, homePair: link.homePair };
      })
      .filter(Boolean) as { from: SystemNode; to: SystemNode; stable: boolean; homePair: boolean }[];
  }, [report]);

  const presenceIds = useMemo(
    () => (report ? locationIdsWithPresence(report, report.factionId) : new Set<string>()),
    [report]
  );

  function clearUnitSelection() {
    setSelectedStack(null);
    setSelectedPerson(null);
  }

  function selectStack(id: string) {
    setSelectedStack(id);
    setSelectedPerson(null);
  }

  function selectPerson(id: string) {
    setSelectedPerson(id);
    setSelectedStack(null);
  }

  function handleSystemClick(id: string, shift: boolean) {
    clearUnitSelection();
    setFilterStar(false);
    setFilterRegionId(null);
    setFilterBodyId(null);
    setFilterOrbitId(null);
    if (shift) {
      setFilterSystems((prev) =>
        prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]
      );
    } else {
      setFilterSystems([id]);
    }
  }

  function openSystemView(systemId: string) {
    clearUnitSelection();
    setFilterSystems([systemId]);
    setFilterRegionId(null);
    setFilterBodyId(null);
    setFilterOrbitId(null);
    setFilterStar(false);
    setBodyViewId(null);
    setSystemViewId(systemId);
  }

  function closeSystemView() {
    setSystemViewId(null);
    setBodyViewId(null);
    setFilterBodyId(null);
    setFilterRegionId(null);
    setFilterOrbitId(null);
    setFilterStar(false);
  }

  function selectStar() {
    clearUnitSelection();
    setFilterStar(true);
    setFilterBodyId(null);
    setFilterRegionId(null);
    setFilterOrbitId(null);
  }

  function selectBody(bodyId: string) {
    clearUnitSelection();
    setFilterBodyId(bodyId);
    setFilterRegionId(null);
    setFilterOrbitId(null);
    setFilterStar(false);
  }

  function selectRegion(regionId: string) {
    clearUnitSelection();
    setFilterRegionId(regionId);
    setFilterBodyId(null);
    setFilterOrbitId(null);
    setFilterStar(false);
  }

  function selectOrbit(orbitId: string, bodyId: string) {
    clearUnitSelection();
    setFilterOrbitId(orbitId);
    setFilterBodyId(bodyId);
    setFilterRegionId(null);
    setFilterStar(false);
  }

  function openBodyView(bodyId: string) {
    clearUnitSelection();
    setFilterBodyId(bodyId);
    setFilterRegionId(null);
    setFilterOrbitId(null);
    setFilterStar(false);
    setBodyViewId(bodyId);
  }

  function closeBodyView() {
    setBodyViewId(null);
  }

  const handleFocusId = useCallback(
    (id: string) => {
      if (!report) return;
      focusReportId(report, id, {
        clearUnitSelection,
        setPanel,
        setFilterSystems,
        setSystemViewId,
        setBodyViewId,
        setFilterBodyId,
        setFilterRegionId,
        setFilterOrbitId,
        setFilterStar,
        selectStack,
        selectPerson,
      });
    },
    [report]
  );

  async function handleViewAs(factionId: number) {
    await viewAsFaction(factionId);
    await load();
  }

  async function handleCheck() {
    const result = await parseOrders(orderText);
    setParseErrors(result.errors);
    setWarnings(result.warnings);
  }

  async function handleSubmit() {
    await handleCheck();
    await submitOrders(orderText);
    alert('Orders submitted to game host');
  }

  async function handleBattleSim() {
    const result = await runBattleSim(battleSimXml);
    setBattleSimOutput(result.output);
  }

  if (!authed) {
    return <LoginScreen onLogin={() => setAuthed(true)} />;
  }

  return (
    <div className="app-shell" style={{ gridTemplateColumns: `56px 1fr ${sidePanelWidth}px` }}>
      <header className="top-bar">
        {meta.admin && meta.factions && meta.factions.length > 0 ? (
          <select
            className="faction-selector"
            value={meta.viewAsFactionId ?? meta.factionId}
            onChange={(e) => handleViewAs(parseInt(e.target.value, 10))}
            title="Admin: view report as faction"
          >
            {(meta.factions as FactionOption[]).map((f) => (
              <option key={f.id} value={f.id}>{f.name}</option>
            ))}
          </select>
        ) : (
          <strong>{meta.name || 'SpaceAge'}</strong>
        )}
        <span>Turn {meta.turn ?? '—'}</span>
        {meta.admin && <span className="admin-badge">Admin</span>}
        {adminHint && <span className="warnings admin-hint">{adminHint}</span>}
        <button type="button" onClick={() => { setToken(null); setAuthed(false); setAdminHint(''); }}>Logout</button>
        <button type="button" onClick={load}>Refresh report</button>
      </header>

      <nav className="icon-rail" aria-label="Panels">
        {(
          [
            ['map', '✦'],
            ['tech', '⚗'],
            ['diplomacy', '🤝'],
            ['contracts', '📜'],
            ['bank', '🏦'],
            ['battle', '⚔'],
            ['faction', '👤'],
          ] as const
        ).map(([id, icon]) => (
          <button
            key={id}
            type="button"
            className={panel === id ? 'active' : ''}
            title={id === 'map' ? 'Star map' : id}
            aria-label={id === 'map' ? 'Star map' : id}
            onClick={() => setPanel(id)}
          >
            {icon}
          </button>
        ))}
      </nav>

      <main className="main-stage">
        {panel === 'map' && report && systemView && bodyView && (
          <BodyView
            report={report}
            detail={systemView}
            body={bodyView}
            selectedRegionId={filterRegionId}
            filterOrbitId={filterOrbitId}
            regionZoom={regionMapZoom}
            presenceIds={presenceIds}
            onRegionZoom={setRegionMapZoom}
            onSelectRegion={selectRegion}
            onSelectOrbit={selectOrbit}
            onOpenBody={openBodyView}
            onBack={closeBodyView}
          />
        )}

        {panel === 'map' && report && systemView && !bodyView && (
          <SystemView
            detail={systemView}
            selectedBodyId={filterBodyId}
            filterStar={filterStar}
            presenceIds={presenceIds}
            onSelectStar={selectStar}
            onSelectBody={selectBody}
            onOpenBody={openBodyView}
            onBack={closeSystemView}
          />
        )}

        {panel === 'map' && report && !systemView && (
          <div className="star-map">
            <StarMapLinks links={aldersonLines} systems={report.systems} route={moveRoute} />
            {report.systems.map((sys) => (
              <button
                key={sys.id}
                type="button"
                className={`system-node ${filterSystems.includes(sys.id) ? 'selected' : ''} ${filterSystems.length && !filterSystems.includes(sys.id) ? 'filtered' : ''} ${systemHasPresence(report, sys.id) ? 'has-presence' : ''}`}
                style={{ left: `${sys.x}%`, top: `${sys.y}%` }}
                onClick={(e) => handleSystemClick(sys.id, e.shiftKey)}
                onDoubleClick={(e) => {
                  e.preventDefault();
                  openSystemView(sys.id);
                }}
              >
                {sys.name}
              </button>
            ))}
            {focusedSystemId && (
              <button
                type="button"
                className="system-view-btn"
                title="System view"
                aria-label="System view"
                onClick={() => openSystemView(focusedSystemId)}
              >
                ⊙
              </button>
            )}
            {selected && moveWeeks != null && (
              <div className="map-hud map-hud-bl">
                MOVE ETA ~{moveWeeks} weeks (estimate)
              </div>
            )}
            {selectedOrder && selectedOrder.moveDestinations.length > 0 && (
              <div className="map-hud map-hud-br">
                MOVE route: {selectedOrder.moveDestinations.join(' → ')}
              </div>
            )}
          </div>
        )}

        {panel === 'tech' && (() => {
          const tech = splitTechnologyReport(sectionText(sections, 'technology', ''));
          return (
            <div className="sub-panel scroll-area">
              <h3>Technologies</h3>
              <h4>Known technologies</h4>
              <TechnologyCatalog />
              <h4>Breakthrough this turn</h4>
              <ClickableReportText text={tech.breakthrough} onFocusId={handleFocusId} />
            </div>
          );
        })()}

        {panel === 'diplomacy' && (
          <div className="sub-panel scroll-area">
            <h3>Diplomacy</h3>
            <ClickableReportText text={extractDiplomacyText(sections, reportFullText)} onFocusId={handleFocusId} />
          </div>
        )}

        {panel === 'contracts' && (
          <div className="sub-panel scroll-area">
            <h3>Contracts</h3>
            <ClickableReportText
              text={sectionText(sections, 'contracts', 'No contracts in report.')}
              onFocusId={handleFocusId}
            />
          </div>
        )}

        {panel === 'bank' && (
          <div className="sub-panel scroll-area">
            <h3>Bank</h3>
            <ClickableReportText
              text={bankReportText(sections, reportFullText)}
              onFocusId={handleFocusId}
            />
          </div>
        )}

        {panel === 'battle' && (
          <div className="sub-panel scroll-area">
            <h3>Battle summaries</h3>
            <ClickableReportText
              text={sectionText(sections, 'battles', 'No battles this quarter.')}
              onFocusId={handleFocusId}
            />
            <h4>Battle simulator</h4>
            <textarea
              value={battleSimXml}
              onChange={(e) => setBattleSimXml(e.target.value)}
              placeholder={'Paste <battle-sim> XML…'}
              style={{ width: '100%', minHeight: 120, fontFamily: 'Consolas, monospace', fontSize: '0.75rem' }}
            />
            <button type="button" onClick={handleBattleSim}>Run simulation</button>
            {battleSimOutput && (
              <pre className="report-section" style={{ marginTop: '0.75rem' }}>{battleSimOutput}</pre>
            )}
          </div>
        )}

        {panel === 'faction' && (
          <div className="sub-panel faction-panel">
            <div className="faction-panel-header">
              <h3>Faction report</h3>
              <input
                type="search"
                className="report-search"
                placeholder="Search faction report…"
                value={factionSearch}
                onChange={(e) => setFactionSearch(e.target.value)}
              />
            </div>
            <div className="faction-panel-body">
              <ReportSearchText
                text={[
                  sectionText(sections, 'events', ''),
                  sectionText(sections, 'survey', ''),
                  sectionText(sections, 'galaxy', 'No galaxy report section.'),
                ].filter(Boolean).join('\n\n')}
                query={factionSearch}
                onFocusId={handleFocusId}
              />
            </div>
          </div>
        )}

        {panel === 'map' && (
          <div className="order-editor" style={{ height: orderPaneHeight }}>
            <ResizeHandle direction="vertical" onDelta={(d) => setOrderPaneHeight((h) => Math.max(120, Math.min(480, h - d)))} className="order-editor-resize" />
            <div className="order-split">
              <div className="order-split-col order-split-orders">
                <h4 className="order-split-heading">Orders</h4>
                {selectedOrderLabel && (
                  <div className="selected-unit-order" key={selectedStack ?? selectedPerson ?? 'none'}>
                    <strong>{selectedOrderLabel}</strong>
                    {selectedOrder ? (
                      <pre>{formatOrderEntry(selectedOrder)}</pre>
                    ) : (
                      <p className="obj-desc">No order this turn.</p>
                    )}
                  </div>
                )}
                <textarea
                  value={orderText}
                  onChange={(e) => setOrderText(e.target.value)}
                  placeholder={'#faction N "password"\nMOVE …'}
                />
                {parseErrors.map((e) => <div key={e} className="warnings">{e}</div>)}
                {warnings.map((w) => <div key={w} className="warnings">{w}</div>)}
                <div className="order-split-actions">
                  <button type="button" onClick={handleCheck}>Parse orders</button>
                  <button type="button" onClick={handleSubmit}>Submit orders</button>
                </div>
              </div>
              <ResizeHandle direction="horizontal" onDelta={(d) => setAiPaneWidth((w) => Math.max(160, w - d))} />
              <div className="order-split-col order-split-ai" style={{ width: aiPaneWidth, flexShrink: 0 }}>
                <h4 className="order-split-heading">AI prompt</h4>
                <textarea
                  className="ai-prompt-textarea"
                  value={aiPromptText}
                  onChange={(e) => setAiPromptText(e.target.value)}
                  placeholder="Describe strategic intent for AI…"
                />
                <div className="order-split-actions">
                  <button type="button" onClick={() => { /* no-op for now */ }}>Submit prompt</button>
                </div>
              </div>
            </div>
          </div>
        )}
      </main>

      {report && (
        <SidePanel
          report={report}
          roots={filteredRoots}
          atLocationLevel={atLocationLevel}
          filterRegionId={filterRegionId}
          filterBodyId={filterBodyId}
          filterOrbitId={filterOrbitId}
          filterStar={filterStar}
          systemViewId={systemViewId}
          selectedStack={selected ?? null}
          selectedPerson={selectedPersonNode ?? null}
          onSelectStack={selectStack}
          onSelectPerson={selectPerson}
          sideTab={sideTab}
          onSideTab={setSideTab}
          eventsText={sectionText(sections, 'events', 'No events this turn.')}
          onFocusId={handleFocusId}
          width={sidePanelWidth}
          onResize={(d) => setSidePanelWidth((w) => Math.max(240, Math.min(640, w + d)))}
        />
      )}
    </div>
  );
}

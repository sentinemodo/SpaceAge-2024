import { useCallback, useEffect, useLayoutEffect, useMemo, useRef, useState } from 'react';
import { RunPodStartModal } from './components/RunPodStartModal';
import {
  login,
  fetchMeta,
  fetchReportXml,
  fetchReportTxt,
  fetchReportSections,
  viewAsFaction,
  setSessionContext,
  parseOrders,
  runBattleSim,
  submitOrders,
  fetchSubmittedOrders,
  fetchRunPodStatus,
  startRunPod,
  stopRunPod,
  fetchPersonas,
  fetchPersona,
  savePersona,
  fetchStory,
  saveStory,
  submitAiQuery,
  regenerateStory,
  getToken,
  setToken,
  type FactionOption,
  type PersonaOption,
  type ReportSection,
  type RunPodStatus,
  type SessionMeta,
} from './api/client';
import {
  parseReportXml,
  flattenStacks,
  estimateMoveWeeks,
  findOrderForStack,
  findOrderForTarget,
  findPersonInStacks,
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
import {
  bodyTypeColor,
  bodyIconScale,
  bodyArcGradient,
  starAmberGradient,
  starTypeBorderColor,
} from './lib/bodyStyles';
import { MovementPanel } from './components/MovementPanel';
import { SidePanel, type SideTab } from './components/SidePanel';
import { AiPane, type AiViewMode } from './components/AiPane';
import { OrbitSelector } from './components/OrbitSelector';
import { PresenceIcons } from './components/PresenceIcons';
import {
  bodyLocationIds,
  buildPresenceIndex,
  presenceIconsForLocations,
  regionCellMarks,
  systemLocationIds,
  type PresenceIcon,
} from './lib/presenceIcons';
import { BeltDotField } from './components/BeltDotField';
import { MoveRouteOverlay } from './components/MoveRouteOverlay';
import {
  focusedStackIds,
  ordersTemplateText,
  ordersSummaryForFocus,
  preloadFactionOrders,
  factionOrdersPane,
} from './lib/orderDisplay';
import { applyFocusedOrderEdits } from './lib/orderTemplate';
import {
  estimateCashUpkeep,
  estimateInterest,
  parseBankSummary,
  parseMarketOffers,
} from './lib/bankMarket';
import { ClickableReportText } from './components/ClickableReportText';
import { TechnologyCatalog } from './components/TechnologyCatalog';
import { catalogAnchor, humanCatalogEntries, resolveCatalogLink } from './lib/techCatalog';
import { ResizeHandle } from './components/ResizeHandle';
import { buildSystemLinkPath } from './lib/mapLinks';
import {
  buildMoveSegments,
  indexRouteLocations,
  moveDestinationsForSelection,
  orderFocusForPane,
  routeAnchorProps,
  selectionTravelContext,
  travelerFromMass,
  type MoveSegment,
} from './lib/moveRoute';
import { defaultLoginFactionId, readUrlFactionId } from './lib/factionLink';
import { focusReportId } from './lib/navigation';
import {
  sectionText,
  filterReportLines,
  extractDiplomacyText,
  splitTechnologyReport,
  bankReportText,
} from './lib/reportSections';

const REGION_ZOOM_BASE = 1.5;

type Panel = 'map' | 'tech' | 'diplomacy' | 'bank' | 'movement' | 'battle' | 'faction' | 'story';
type OrderMode = 'units' | 'faction' | 'parser';
type ParseButtonStatus = 'ok' | 'warnings' | 'errors';
type TechView = 'known' | 'breakthrough';

function parseButtonStatusFromResult(
  errors: string[],
  warnings: string[],
  ok: boolean,
): ParseButtonStatus {
  if (errors.length > 0) return 'errors';
  if (warnings.length > 0) return 'warnings';
  return ok ? 'ok' : 'errors';
}

function isGasGiant(body: SystemBodyNode): boolean {
  return body.kind === 'planet' && (body.planetType || '').toLowerCase().includes('gas');
}

function LoginScreen({ onLogin }: { onLogin: () => void }) {
  const [factionId, setFactionId] = useState(defaultLoginFactionId);
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
        setError('Game-host is outdated — restart it (npm run restart in game-host/).');
        return;
      }
      if (trimmedGm && !data.admin) {
        setError('GM key not accepted. Use the GAME_HOST_GM_KEY value from repo-root .env (not dev-gm-key unless that is what .env sets).');
        setToken(null);
        return;
      }
      if (data.admin) {
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
        <input type="password" value={gmKey} onChange={(e) => setGmKey(e.target.value)} placeholder="from .env GAME_HOST_GM_KEY" />
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
  presenceIndex,
}: {
  body: SystemBodyNode;
  report: ParsedReport;
  selectedRegionId: string | null;
  onSelectRegion: (regionId: string) => void;
  zoom: number;
  large?: boolean;
  presenceIndex: Map<string, PresenceIcon[]>;
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
        const icons = presenceIndex.get(region.id) || [];
        const marks = regionCellMarks(region.name, cell, icons.length);
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
            title={`${region.name} [${region.id}]${region.terrainType ? ` · ${region.terrainType}` : ''}${icons.length ? ` · ${icons.map((icon) => icon.label).join(', ')}` : ''}`}
            data-route-id={region.id}
            onClick={() => onSelectRegion(region.id)}
          >
            {marks.showIcons && <PresenceIcons icons={icons} />}
            {marks.showName && <span className="region-cell-label">{region.name}</span>}
          </button>
        );
      })}
    </div>
  );
}

function BodySurfaceArc({
  body,
  selected,
  onSelect,
}: {
  body: SystemBodyNode;
  selected?: boolean;
  onSelect?: () => void;
}) {
  const color = bodyTypeColor(body);
  return (
    <button
      type="button"
      className={`body-surface-arc body-surface-arc-${body.kind} ${selected ? 'selected' : ''}`}
      style={{
        background: bodyArcGradient(body),
        ['--body-color' as string]: color,
      }}
      title={`${body.name} [${body.id}]${body.planetType ? ` · ${body.planetType}` : ''}`}
      {...routeAnchorProps(body)}
      onClick={onSelect}
    >
      <span className="body-surface-arc-glyph" aria-hidden>{bodyKindIcon(body)}</span>
      <span className="body-surface-arc-name">{body.name}</span>
    </button>
  );
}

function BodyIconButton({
  body,
  selected,
  compact,
  hasPresence,
  presenceIcons,
  onSelect,
  onOpen,
}: {
  body: SystemBodyNode;
  selected: boolean;
  compact?: boolean;
  hasPresence?: boolean;
  presenceIcons?: PresenceIcon[];
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
      {...routeAnchorProps(body)}
      onClick={onSelect}
      onDoubleClick={(e) => {
        e.preventDefault();
        onOpen();
      }}
      style={{
        ['--body-color' as string]: color,
        ['--body-scale' as string]: String(scale),
        borderColor: selected ? 'var(--accent)' : `color-mix(in srgb, ${color} 65%, var(--accent-dim))`,
      }}
    >
      <span className="body-icon-glyph" aria-hidden>{bodyKindIcon(body)}</span>
      {!compact && <span className="body-icon-name">{body.name}</span>}
      {!compact && <PresenceIcons icons={presenceIcons || []} />}
    </button>
  );
}

function BandBodyStack({
  body,
  satellites,
  selectedBodyId,
  filterOrbitId,
  presenceIds,
  presenceIndex,
  onSelectBody,
  onOpenBody,
  onSelectOrbit,
}: {
  body: SystemBodyNode;
  satellites: SystemBodyNode[];
  selectedBodyId: string | null;
  filterOrbitId?: string | null;
  presenceIds: Set<string>;
  presenceIndex: Map<string, PresenceIcon[]>;
  onSelectBody: (bodyId: string) => void;
  onOpenBody: (bodyId: string) => void;
  onSelectOrbit?: (orbitId: string, bodyId: string) => void;
}) {
  const bodyPresence = presenceIds.has(body.id)
    || body.regions.some((r) => presenceIds.has(r.id))
    || body.orbitIds.some((o) => presenceIds.has(o));

  return (
    <div className="band-body-stack">
      <BodyIconButton
        body={body}
        hasPresence={bodyPresence}
        presenceIcons={presenceIconsForLocations(presenceIndex, bodyLocationIds(body))}
        selected={selectedBodyId === body.id}
        onSelect={() => onSelectBody(body.id)}
        onOpen={() => onOpenBody(body.id)}
      />
      <span className="band-au">{body.au} AU</span>
      {body.kind === 'belt' && body.orbitIds.length > 0 && onSelectOrbit && (
        <OrbitSelector
          orbitIds={body.orbitIds}
          filterOrbitId={filterOrbitId ?? null}
          presenceIds={presenceIds}
          presenceIndex={presenceIndex}
          glyph="◎"
          onSelectOrbit={(orbitId) => onSelectOrbit(orbitId, body.id)}
        />
      )}
      {satellites.length > 0 && (
        <div className="band-children">
          {[...satellites]
            .sort((a, b) => a.au - b.au || a.name.localeCompare(b.name))
            .map((child) => (
              <BodyIconButton
                key={child.id}
                body={child}
                hasPresence={presenceIds.has(child.id) || child.regions.some((r) => presenceIds.has(r.id))}
                presenceIcons={presenceIconsForLocations(presenceIndex, bodyLocationIds(child))}
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
  filterOrbitId,
  filterStar,
  presenceIds,
  presenceIndex,
  moveSegments,
  onSelectStar,
  onSelectBody,
  onOpenBody,
  onSelectOrbit,
  onBack,
}: {
  detail: SystemDetail;
  selectedBodyId: string | null;
  filterOrbitId: string | null;
  filterStar: boolean;
  presenceIds: Set<string>;
  presenceIndex: Map<string, PresenceIcon[]>;
  moveSegments: MoveSegment[];
  onSelectStar: () => void;
  onSelectBody: (bodyId: string) => void;
  onOpenBody: (bodyId: string) => void;
  onSelectOrbit: (orbitId: string, bodyId: string) => void;
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
              filterOrbitId={filterOrbitId}
              presenceIds={presenceIds}
              presenceIndex={presenceIndex}
              onSelectBody={onSelectBody}
              onOpenBody={onOpenBody}
              onSelectOrbit={onSelectOrbit}
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
                  presenceIcons={presenceIconsForLocations(presenceIndex, bodyLocationIds(gate))}
                  selected={selectedBodyId === gate.id}
                  onSelect={() => onSelectBody(gate.id)}
                  onOpen={() => onOpenBody(gate.id)}
                />
              ))}
            </div>
            <span className="band-au">{gates[0].au} AU</span>
          </div>
        )}
        {moveSegments.length > 0 && (
          <MoveRouteOverlay segments={moveSegments} layoutKey={detail.id} clipToLabels />
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
  presenceIndex,
  moveSegments,
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
  presenceIndex: Map<string, PresenceIcon[]>;
  moveSegments: MoveSegment[];
  onRegionZoom: (z: number) => void;
  onSelectRegion: (regionId: string) => void;
  onSelectOrbit: (orbitId: string, bodyId: string) => void;
  onOpenBody: (bodyId: string) => void;
  onBack: () => void;
}) {
  const parent = body.parentId ? detail.bodies.find((b) => b.id === body.parentId) : undefined;
  const siblings = parent ? childBodies(detail, parent.id) : childBodies(detail, body.id);
  const switchTargets = parent
    ? [parent, ...siblings.filter((s) => s.id !== parent.id)]
    : body.kind === 'planet'
      ? [body, ...siblings]
      : [body];
  const hasRegions = body.regions.length > 0;
  const gasGiant = isGasGiant(body);
  const hasOrbits = body.orbitIds.length > 0;
  const satellites = childBodies(detail, body.id)
    .filter((c) => c.kind === 'moon' || c.kind === 'belt')
    .sort((a, b) => a.au - b.au || a.name.localeCompare(b.name));

  if (body.kind === 'alderson') {
    return (
      <div className="system-view">
        <div className="system-view-toolbar">
          <button type="button" className="system-view-back" onClick={onBack}>← {detail.name}</button>
          <strong>{body.name}</strong>
          <span className="system-view-star">{body.au} AU · Alderson gate</span>
        </div>
        <div
          className="gate-detail-map"
          style={{ ['--body-color' as string]: bodyTypeColor(body) }}
        >
          {hasOrbits && (
            <div className="gate-orbit-center">
              <div className="gate-orbit-marker">
                <span className="gate-orbit-name">{body.name}</span>
                <OrbitSelector
                  orbitIds={body.orbitIds}
                  filterOrbitId={filterOrbitId}
                  presenceIds={presenceIds}
                  presenceIndex={presenceIndex}
                  glyph="◯"
                  onSelectOrbit={(orbitId) => onSelectOrbit(orbitId, body.id)}
                />
              </div>
            </div>
          )}
          {moveSegments.length > 0 && (
            <MoveRouteOverlay segments={moveSegments} layoutKey={body.id} />
          )}
        </div>
      </div>
    );
  }

  if (body.kind === 'belt') {
    return (
      <div className="system-view">
        <div className="system-view-toolbar">
          <button type="button" className="system-view-back" onClick={onBack}>← {detail.name}</button>
          <strong>{body.name}</strong>
          <span className="system-view-star">{body.au} AU · belt</span>
        </div>
        <div className="belt-detail-map">
          <BeltDotField seed={body.id} />
          {hasOrbits && (
            <div className="belt-orbit-center">
              <OrbitSelector
                orbitIds={body.orbitIds}
                filterOrbitId={filterOrbitId}
                presenceIds={presenceIds}
                presenceIndex={presenceIndex}
                glyph="◎"
                onSelectOrbit={(orbitId) => onSelectOrbit(orbitId, body.id)}
              />
            </div>
          )}
          {moveSegments.length > 0 && (
            <MoveRouteOverlay segments={moveSegments} layoutKey={body.id} />
          )}
        </div>
      </div>
    );
  }

  return (
    <div className="system-view">
      <div className="system-view-toolbar">
        <button type="button" className="system-view-back" onClick={onBack}>← {detail.name}</button>
        <strong>{body.name}</strong>
        <span className="system-view-star">{body.au} AU · {body.planetType || body.kind}</span>
      </div>
      <div
        className={`planet-view-map ${hasRegions ? '' : 'planet-view-map-no-regions'}`}
        style={{ ['--body-color' as string]: bodyTypeColor(body) }}
      >
        <BodySurfaceArc body={body} selected />

        {hasRegions && (
          <div className="planet-view-central-pane">
            {switchTargets.length > 1 && (
              <div className="planet-view-switcher">
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
            <div className="region-map-zoom-toolbar">
              <button type="button" onClick={() => onRegionZoom(Math.max(REGION_ZOOM_BASE * 0.5, regionZoom - REGION_ZOOM_BASE * 0.25))}>−</button>
              <span>{Math.round((regionZoom / REGION_ZOOM_BASE) * 100)}%</span>
              <button type="button" onClick={() => onRegionZoom(Math.min(REGION_ZOOM_BASE * 2.5, regionZoom + REGION_ZOOM_BASE * 0.25))}>+</button>
            </div>
            <div className="region-map-panel">
              <div className="region-map-center">
              <RegionMap
                body={body}
                report={report}
                selectedRegionId={selectedRegionId}
                onSelectRegion={onSelectRegion}
                zoom={regionZoom}
                large
                presenceIndex={presenceIndex}
              />
              </div>
            </div>
            <p className="region-view-hint">Click a cell to filter units to that region.</p>
          </div>
        )}

        {hasOrbits && (
          <div className="planet-view-orbit-column">
            <OrbitSelector
              orbitIds={body.orbitIds}
              filterOrbitId={filterOrbitId}
              presenceIds={presenceIds}
              presenceIndex={presenceIndex}
              vertical
              glyph={gasGiant ? '◎' : '◯'}
              onSelectOrbit={(orbitId) => onSelectOrbit(orbitId, body.id)}
            />
          </div>
        )}

        {satellites.length > 0 && (
          <div className="planet-view-satellite-bands">
            {satellites.map((sat) => (
              <div key={sat.id} className="planet-view-satellite-band">
                <BandBodyStack
                  body={sat}
                  satellites={childBodies(detail, sat.id)}
                  selectedBodyId={null}
                  filterOrbitId={filterOrbitId}
                  presenceIds={presenceIds}
                  presenceIndex={presenceIndex}
                  onSelectBody={onOpenBody}
                  onOpenBody={onOpenBody}
                  onSelectOrbit={onSelectOrbit}
                />
              </div>
            ))}
          </div>
        )}
        {moveSegments.length > 0 && (
          <MoveRouteOverlay segments={moveSegments} layoutKey={`${body.id}:${regionZoom}`} />
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
  const [factionView, setFactionView] = useState<'report' | 'orders'>('report');
  const [factionOrderText, setFactionOrderText] = useState('');
  const [factionParseOutput, setFactionParseOutput] = useState('No orders submitted yet.');
  const [factionOrdersTop, setFactionOrdersTop] = useState(280);
  const factionOrdersDirty = useRef(false);
  const [selectedStack, setSelectedStack] = useState<string | null>(null);
  const [selectedPerson, setSelectedPerson] = useState<string | null>(null);
  const [draftOrders, setDraftOrders] = useState('');
  const [orderCaret, setOrderCaret] = useState(0);
  const [parserOutput, setParserOutput] = useState('No parser output yet.');
  const [orderMode, setOrderMode] = useState<OrderMode>('faction');
  const [techView, setTechView] = useState<TechView>('known');
  const [techFocus, setTechFocus] = useState<{ anchor: string; n: number } | null>(null);
  const [aiPromptText, setAiPromptText] = useState('');
  const [aiOutputText, setAiOutputText] = useState('No AI output yet.');
  const [aiMode, setAiMode] = useState<AiViewMode>('query');
  const [includeStory, setIncludeStory] = useState(false);
  const [storyAiPrompt, setStoryAiPrompt] = useState('');
  const [storyAiOutput, setStoryAiOutput] = useState('No AI output yet.');
  const [storyAiMode, setStoryAiMode] = useState<AiViewMode>('query');
  const [storyIncludeStory, setStoryIncludeStory] = useState(true);
  const [runpodStatus, setRunpodStatus] = useState<RunPodStatus | null>(null);
  const [runpodBusy, setRunpodBusy] = useState(false);
  const [runpodModalOpen, setRunpodModalOpen] = useState(false);
  const [runpodStartResponse, setRunpodStartResponse] = useState<RunPodStatus | null>(null);
  const [aiQueryBusy, setAiQueryBusy] = useState(false);
  const [personas, setPersonas] = useState<PersonaOption[]>([]);
  const [selectedPersonaId, setSelectedPersonaId] = useState('military');
  const [personaText, setPersonaText] = useState('');
  const [storyText, setStoryText] = useState('');
  const [storyBusy, setStoryBusy] = useState(false);
  const [sidePanelWidth, setSidePanelWidth] = useState(320);
  const [orderPaneHeight, setOrderPaneHeight] = useState(220);
  const [aiPaneWidth, setAiPaneWidth] = useState(280);
  const [regionMapZoom, setRegionMapZoom] = useState(REGION_ZOOM_BASE);
  const [adminContextList, setAdminContextList] = useState<'campaigns' | 'turns'>('turns');
  const [reportFullText, setReportFullText] = useState('');
  const [filterStar, setFilterStar] = useState(false);
  const [warnings, setWarnings] = useState<string[]>([]);
  const [parseErrors, setParseErrors] = useState<string[]>([]);
  const [parseButtonStatus, setParseButtonStatus] = useState<ParseButtonStatus | null>(null);
  const ordersDirtyRef = useRef(false);
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
      const parsed = enrichReportFromGalaxyText(parseReportXml(xml), galaxyText);
      setReport(parsed);
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

  useEffect(() => {
    if (!authed || meta.factionId == null) return;
    const requested = readUrlFactionId();
    if (requested == null) return;

    const viewing = meta.viewAsFactionId ?? meta.factionId;
    if (viewing === requested) return;

    if (meta.admin) {
      viewAsFaction(requested).then(() => load()).catch(() => {});
      return;
    }

    setToken(null);
    setAuthed(false);
  }, [authed, load, meta.admin, meta.factionId, meta.viewAsFactionId]);

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

  const fullOrdersTemplate = useMemo(() => ordersTemplateText(sections), [sections]);

  const factionOrdersText = useMemo(() => {
    if (!report || !fullOrdersTemplate) return '';
    return preloadFactionOrders(fullOrdersTemplate, report.stacks, report.factionId);
  }, [report, fullOrdersTemplate]);

  const unitsOrdersText = useMemo(() => {
    if (!report || !fullOrdersTemplate) return '';
    return ordersSummaryForFocus(
      draftOrders || fullOrdersTemplate,
      filteredRoots,
      selectedStack,
      selectedPerson
    );
  }, [report, fullOrdersTemplate, draftOrders, filteredRoots, selectedStack, selectedPerson]);

  useEffect(() => {
    ordersDirtyRef.current = false;
    setParseButtonStatus(null);
  }, [report?.factionId, meta.viewAsFactionId]);

  useEffect(() => {
    if (ordersDirtyRef.current) return;
    setDraftOrders(factionOrdersText);
    setUnitsEditText(null);
  }, [factionOrdersText]);

  const unitsFocus = useMemo(() => {
    if (selectedPerson) {
      return { stackIds: new Set<string>(), personIds: new Set([selectedPerson]) };
    }
    return {
      stackIds: new Set(focusedStackIds(filteredRoots, selectedStack)),
      personIds: new Set<string>(),
    };
  }, [filteredRoots, selectedPerson, selectedStack]);
  const [unitsEditText, setUnitsEditText] = useState<string | null>(null);
  const unitsFocusKey = `${selectedStack ?? ''}|${selectedPerson ?? ''}|${[...unitsFocus.stackIds].join(',')}|${[...unitsFocus.personIds].join(',')}`;
  const unitsFocusKeyRef = useRef(unitsFocusKey);
  if (unitsFocusKeyRef.current !== unitsFocusKey) {
    unitsFocusKeyRef.current = unitsFocusKey;
    if (unitsEditText !== null) setUnitsEditText(null);
  }

  const orderEditorText = orderMode === 'units' ? (unitsEditText ?? unitsOrdersText) : draftOrders;
  const orderEditorReadOnly = false;
  const orderFocus = useMemo(
    () => (orderMode === 'parser' ? null : orderFocusForPane(orderEditorText, orderCaret)),
    [orderMode, orderEditorText, orderCaret],
  );
  const orderFocusKind = orderFocus?.kind ?? null;
  const orderFocusId = orderFocus?.id ?? null;

  const parserPaneText = parserOutput;

  const orderLineCount = useMemo(() => {
    const lines = orderEditorText.split('\n').length;
    return Math.max(lines, 1);
  }, [orderEditorText]);

  const orderLineNumbersRef = useRef<HTMLDivElement>(null);
  const orderTextareaRef = useRef<HTMLTextAreaElement>(null);

  function syncOrderLineNumbers() {
    const textarea = orderTextareaRef.current;
    const gutter = orderLineNumbersRef.current;
    if (textarea && gutter) {
      gutter.scrollTop = textarea.scrollTop;
    }
  }

  function syncOrderCaret(el: HTMLTextAreaElement) {
    const next = el.selectionStart ?? 0;
    setOrderCaret((prev) => (prev === next ? prev : next));
  }

  useLayoutEffect(() => {
    const el = orderTextareaRef.current;
    if (!el || orderMode === 'parser') return;
    const next = el.selectionStart ?? 0;
    setOrderCaret((prev) => (prev === next ? prev : next));
  }, [orderEditorText, orderMode]);

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

  const moveDestinationIds = useMemo(() => {
    if (!orderFocusKind || !orderFocusId) return [];
    return moveDestinationsForSelection(
      orderEditorText,
      orderFocusKind === 'modulestack' ? orderFocusId : null,
      orderFocusKind === 'person' ? orderFocusId : null,
    ) ?? [];
  }, [orderEditorText, orderFocusKind, orderFocusId]);

  const focusedMover = useMemo(() => {
    if (!report || !orderFocusKind || !orderFocusId) return undefined;
    return selectionTravelContext(
      report.stacks,
      orderFocusKind === 'modulestack' ? orderFocusId : null,
      orderFocusKind === 'person' ? orderFocusId : null,
    );
  }, [report, orderFocusKind, orderFocusId]);

  const focusedMoverStack = useMemo(() => {
    if (!orderFocusKind || !orderFocusId) return undefined;
    if (orderFocusKind === 'modulestack') return flatStacks.find((stack) => stack.id === orderFocusId);
    return flatStacks.find((stack) => stack.persons.some((person) => person.id === orderFocusId));
  }, [flatStacks, orderFocusKind, orderFocusId]);

  const moveSegments = useMemo(() => {
    if (!report || moveDestinationIds.length === 0) return [];
    return buildMoveSegments(
      focusedMover?.locationId,
      moveDestinationIds,
      indexRouteLocations(report),
      travelerFromMass(focusedMover?.mass),
    );
  }, [report, moveDestinationIds, focusedMover]);

  const moveRoute = useMemo(() => {
    if (!report || !focusedMoverStack || moveDestinationIds.length === 0) return null;
    const endpoints = resolveMoveRouteSystems(focusedMoverStack, {
      subject: orderFocusKind ?? 'modulestack',
      targetId: orderFocusId ?? focusedMoverStack.id,
      moveDestinations: moveDestinationIds,
      verbs: [],
    }, report.regions);
    if (!endpoints) return null;
    const from = report.systems.find((s) => s.id === endpoints.fromSystemId);
    const to = report.systems.find((s) => s.id === endpoints.toSystemId);
    if (!from || !to) return null;
    return { from, to };
  }, [report, focusedMoverStack, orderFocusKind, orderFocusId, moveDestinationIds]);

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
  const presenceIndex = useMemo(
    () => (report ? buildPresenceIndex(report) : new Map<string, PresenceIcon[]>()),
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

  const openCatalogId = useCallback((id: string, before?: string) => {
    const hit = resolveCatalogLink(before ?? '', id, humanCatalogEntries);
    if (!hit) return false;
    setPanel('tech');
    setTechView('known');
    setTechFocus({ anchor: catalogAnchor(hit.kind, hit.id), n: Date.now() });
    return true;
  }, []);

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

  const focusFromText = useCallback(
    (id: string, before?: string) => {
      if (openCatalogId(id, before)) return;
      handleFocusId(id);
    },
    [openCatalogId, handleFocusId]
  );

  async function handleViewAs(factionId: number) {
    await viewAsFaction(factionId);
    await load();
  }

  async function handleRunChange(runId: string) {
    await setSessionContext({ runId });
    await load();
  }

  async function handleTurnChange(turn: number) {
    await setSessionContext({ turn });
    await load();
  }

  function publishMapOrders(next: string) {
    ordersDirtyRef.current = true;
    factionOrdersDirty.current = true;
    setParseButtonStatus(null);
    setDraftOrders(next);
    setFactionOrderText(next);
  }

  function applyParseResult(result: Awaited<ReturnType<typeof parseOrders>>) {
    setParseErrors(result.errors);
    setWarnings(result.warnings);
    setParserOutput(result.output || `ok: ${result.ok}`);
    setParseButtonStatus(parseButtonStatusFromResult(result.errors, result.warnings, result.ok));
  }

  async function handleCheck() {
    const packageText = draftOrders || factionOrdersText;
    const result = await parseOrders(packageText);
    applyParseResult(result);
  }

  async function handleSubmit() {
    const packageText = draftOrders || factionOrdersText;
    const result = await parseOrders(packageText);
    applyParseResult(result);
    try {
      await submitOrders(packageText);
      alert('Orders submitted to game host');
    } catch (err) {
      alert(String(err));
    }
  }

  async function handleFactionParse() {
    const result = await parseOrders(factionOrderText);
    setFactionParseOutput(result.output || `ok: ${result.ok}`);
  }

  async function handleFactionSubmit() {
    const result = await parseOrders(factionOrderText);
    const blocks = [result.output || `ok: ${result.ok}`];
    try {
      await submitOrders(factionOrderText);
      factionOrdersDirty.current = false;
      blocks.push('', 'Orders submitted.');
    } catch (err) {
      blocks.push('', String(err));
    }
    setFactionParseOutput(blocks.join('\n'));
  }

  useEffect(() => {
    if (!authed) return;
    let cancelled = false;
    fetchSubmittedOrders()
      .then((submitted) => {
        if (cancelled || factionOrdersDirty.current) return;
        const pane = factionOrdersPane(submitted, ordersTemplateText(sections));
        setFactionOrderText(pane.ordersText);
        if (pane.parseOutput) setFactionParseOutput(pane.parseOutput);
      })
      .catch(() => {
        if (cancelled || factionOrdersDirty.current) return;
        const pane = factionOrdersPane(null, ordersTemplateText(sections));
        setFactionOrderText(pane.ordersText);
        if (pane.parseOutput) setFactionParseOutput(pane.parseOutput);
      });
    return () => { cancelled = true; };
  }, [authed, meta.factionId, meta.viewAsFactionId, sections]);

  async function handleBattleSim() {
    const result = await runBattleSim(battleSimXml);
    setBattleSimOutput(result.output);
  }

  const refreshRunPod = useCallback(async () => {
    try {
      const status = await fetchRunPodStatus();
      setRunpodStatus(status);
    } catch {
      setRunpodStatus({ phase: 'stopped', podId: null, ollamaHost: null, model: '', message: 'Status unavailable', ollamaReady: false });
    }
  }, []);

  const loadStoryContext = useCallback(async () => {
    const [personaList, story] = await Promise.all([
      fetchPersonas(),
      fetchStory(),
    ]);
    setPersonas(personaList);
    setStoryText(story);
    if (personaList.length) {
      setSelectedPersonaId((current) => (
        personaList.some((p) => p.id === current) ? current : personaList[0].id
      ));
    }
  }, []);

  useEffect(() => {
    if (!authed || !meta.admin) return;
    loadStoryContext().catch(() => {});
  }, [authed, meta.admin, loadStoryContext]);

  useEffect(() => {
    if (!authed || !meta.admin) return;
    refreshRunPod();
    const timer = window.setInterval(() => { refreshRunPod(); }, 30_000);
    return () => window.clearInterval(timer);
  }, [authed, meta.admin, refreshRunPod]);

  useEffect(() => {
    if (!meta.admin && panel === 'story') setPanel('map');
  }, [meta.admin, panel]);

  useEffect(() => {
    if (!authed || !meta.admin || !selectedPersonaId) return;
    fetchPersona(selectedPersonaId).then(setPersonaText).catch(() => setPersonaText(''));
  }, [authed, meta.admin, selectedPersonaId]);

  async function handleStartRunpod() {
    setRunpodModalOpen(true);
    setRunpodStartResponse(null);
    setRunpodBusy(true);
    try {
      const status = await startRunPod();
      setRunpodStartResponse(status);
      setRunpodStatus(status);
      for (let i = 0; i < 120; i += 1) {
        await new Promise((r) => window.setTimeout(r, 2000));
        const next = await fetchRunPodStatus();
        setRunpodStatus(next);
        if (next.stage === 'ready' || next.phase === 'running') break;
        if (next.stage === 'failed' || (next.phase === 'stopped' && i > 0 && next.stage !== 'idle')) break;
      }
    } catch (err) {
      setRunpodStatus((prev) => ({
        phase: 'stopped',
        stage: 'failed',
        podId: prev?.podId ?? null,
        ollamaHost: prev?.ollamaHost ?? null,
        model: prev?.model ?? '',
        message: String(err),
        ollamaReady: false,
        logs: [
          ...(prev?.logs || []),
          { at: new Date().toISOString(), level: 'error', message: String(err) },
        ],
      }));
      await refreshRunPod();
    } finally {
      setRunpodBusy(false);
    }
  }

  async function handleStopRunpod() {
    setRunpodBusy(true);
    try {
      const status = await stopRunPod();
      setRunpodStatus(status);
    } catch (err) {
      alert(String(err));
    } finally {
      setRunpodBusy(false);
    }
  }

  async function handleAiQuery(useStoryPane: boolean) {
    const prompt = useStoryPane ? storyAiPrompt : aiPromptText;
    const include = useStoryPane ? storyIncludeStory : includeStory;
    setAiQueryBusy(true);
    try {
      const result = await submitAiQuery(prompt, include);
      const output = result.output || result.error || 'No output';
      if (useStoryPane) {
        setStoryAiOutput(output);
        setStoryAiMode('output');
      } else {
        setAiOutputText(output);
        setAiMode('output');
      }
    } catch (err) {
      const msg = String(err);
      if (useStoryPane) setStoryAiOutput(msg);
      else setAiOutputText(msg);
    } finally {
      setAiQueryBusy(false);
    }
  }

  async function handleSaveStoryDraft() {
    setStoryBusy(true);
    try {
      await saveStory(storyText);
    } catch (err) {
      alert(String(err));
    } finally {
      setStoryBusy(false);
    }
  }

  async function handleSavePersonaDraft() {
    if (!meta.admin) return;
    setStoryBusy(true);
    try {
      await savePersona(selectedPersonaId, personaText);
    } catch (err) {
      alert(String(err));
    } finally {
      setStoryBusy(false);
    }
  }

  async function handleRegenerateStory() {
    setStoryBusy(true);
    try {
      const result = await regenerateStory(selectedPersonaId);
      if (result.story) setStoryText(result.story);
      setStoryAiOutput(result.output || 'Story regenerated.');
      setStoryAiMode('output');
    } catch (err) {
      alert(String(err));
    } finally {
      setStoryBusy(false);
    }
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
            <optgroup label="Player factions">
              {(meta.factions as FactionOption[]).filter((f) => !f.npc).map((f) => (
                <option key={f.id} value={f.id}>{f.name}</option>
              ))}
            </optgroup>
            <optgroup label="NPC factions">
              {(meta.factions as FactionOption[]).filter((f) => f.npc).map((f) => (
                <option key={f.id} value={f.id}>{f.name}</option>
              ))}
            </optgroup>
          </select>
        ) : (
          <strong>{meta.name || 'SpaceAge'}</strong>
        )}
        {!meta.admin && meta.runs && meta.runs.length > 0 && (
          <span className="context-label" title="Campaign">{meta.runs[0].label}</span>
        )}
        {meta.admin && (
          <div className="context-switch" role="group" aria-label="Campaign list or turns list">
            <button
              type="button"
              className={adminContextList === 'campaigns' ? 'active' : ''}
              aria-pressed={adminContextList === 'campaigns'}
              title="Campaign list"
              onClick={() => setAdminContextList('campaigns')}
            >
              Campaigns
            </button>
            <button
              type="button"
              className={adminContextList === 'turns' ? 'active' : ''}
              aria-pressed={adminContextList === 'turns'}
              title="Turns list"
              onClick={() => setAdminContextList('turns')}
            >
              Turns
            </button>
          </div>
        )}
        {meta.admin && adminContextList === 'campaigns' && meta.runs && meta.runs.length > 0 && (
          <select
            className="context-selector"
            value={meta.viewRunId ?? meta.runId ?? ''}
            onChange={(e) => handleRunChange(e.target.value)}
            title="x — visible to players"
          >
            {meta.runs.map((r) => (
              <option key={r.id} value={r.id}>{r.playerVisible ? `x  ${r.label}` : r.label}</option>
            ))}
          </select>
        )}
        {(!meta.admin || adminContextList === 'turns') && meta.turns && meta.turns.length > 0 && (
          <select
            className="context-selector"
            value={meta.viewTurn ?? meta.turn ?? ''}
            onChange={(e) => handleTurnChange(parseInt(e.target.value, 10))}
            title="Report turn"
          >
            {meta.turns.map((t) => (
              <option key={t} value={t}>Turn {t}</option>
            ))}
          </select>
        )}
        {(!meta.admin || adminContextList === 'turns') && !meta.turns?.length && (
          <span>Turn {meta.viewTurn ?? meta.turn ?? '—'}</span>
        )}
        {meta.admin && <span className="admin-badge">Admin</span>}
        {adminHint && <span className="warnings admin-hint">{adminHint}</span>}
        <span className="client-title">SpaceAge client ver. 0.8.001</span>
        <button type="button" onClick={() => { setToken(null); setAuthed(false); setAdminHint(''); }}>Logout</button>
        <button type="button" onClick={load}>Refresh report</button>
      </header>

      <nav className="icon-rail" aria-label="Panels">
        {(
          [
            ['map', '✦'],
            ['tech', '⚗'],
            ['diplomacy', '🤝'],
            ['bank', '🏦'],
            ['movement', '⇄'],
            ['battle', '⚔'],
            ['faction', '👤'],
            ['story', '📖'],
          ] as const
        )
          .filter(([id]) => meta.admin || id !== 'story')
          .map(([id, icon]) => (
          <button
            key={id}
            type="button"
            className={panel === id ? 'active' : ''}
            title={id === 'map' ? 'Star map' : id === 'bank' ? 'Banking and market' : id === 'movement' ? 'Movement and transit ETA' : id === 'story' ? 'Persona & story' : id}
            aria-label={id === 'map' ? 'Star map' : id === 'bank' ? 'Banking and market' : id === 'movement' ? 'Movement and transit ETA' : id === 'story' ? 'Persona and story' : id}
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
            presenceIndex={presenceIndex}
            moveSegments={moveSegments}
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
            filterOrbitId={filterOrbitId}
            filterStar={filterStar}
            presenceIds={presenceIds}
            presenceIndex={presenceIndex}
            moveSegments={moveSegments}
            onSelectStar={selectStar}
            onSelectBody={selectBody}
            onOpenBody={openBodyView}
            onSelectOrbit={selectOrbit}
            onBack={closeSystemView}
          />
        )}

        {panel === 'map' && report && !systemView && (
          <div className="star-map">
            <StarMapLinks links={aldersonLines} systems={report.systems} route={moveRoute} />
            {report.systems.map((sys) => {
              const detail = getSystemDetail(report, sys.id);
              const starType = detail?.star?.starType;
              return (
                <button
                  key={sys.id}
                  type="button"
                  className={`system-node system-node-star ${filterSystems.includes(sys.id) ? 'selected' : ''} ${filterSystems.length && !filterSystems.includes(sys.id) ? 'filtered' : ''} ${systemHasPresence(report, sys.id) ? 'has-presence' : ''}`}
                  style={{
                    left: `${sys.x}%`,
                    top: `${sys.y}%`,
                    background: starAmberGradient(starType),
                    borderColor: starTypeBorderColor(starType),
                    boxShadow: filterSystems.includes(sys.id)
                      ? `0 0 14px ${starTypeBorderColor(starType)}`
                      : undefined,
                  }}
                  title={starType ? `${sys.name} · ${starType} star` : sys.name}
                  onClick={(e) => handleSystemClick(sys.id, e.shiftKey)}
                  onDoubleClick={(e) => {
                    e.preventDefault();
                    openSystemView(sys.id);
                  }}
                >
                  {sys.name}
                  <PresenceIcons icons={presenceIconsForLocations(presenceIndex, systemLocationIds(detail, sys.id))} />
                </button>
              );
            })}
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
            {moveDestinationIds.length > 0 && (
              <div className="map-hud map-hud-br">
                MOVE route: {moveDestinationIds.join(' → ')}
              </div>
            )}
          </div>
        )}

        {panel === 'tech' && (() => {
          const tech = splitTechnologyReport(sectionText(sections, 'technology', ''));
          return (
            <div className="sub-panel tech-sub-panel">
              <div className="panel-heading-row">
                <h3>Technologies</h3>
                <div className="panel-mode-tabs">
                  <button type="button" className={techView === 'known' ? 'active' : ''} onClick={() => setTechView('known')}>Known</button>
                  <button type="button" className={techView === 'breakthrough' ? 'active' : ''} onClick={() => setTechView('breakthrough')}>Breakthroughs</button>
                </div>
              </div>
              {techView === 'known' ? (
                <TechnologyCatalog focusAnchor={techFocus?.anchor} focusNonce={techFocus?.n} />
              ) : (
                <div className="tech-breakthrough">
                  <ClickableReportText text={tech.breakthrough} onFocusId={focusFromText} />
                </div>
              )}
            </div>
          );
        })()}

        {panel === 'diplomacy' && (
          <div className="sub-panel scroll-area">
            <h3>Diplomacy</h3>
            <ClickableReportText text={extractDiplomacyText(sections, reportFullText)} onFocusId={focusFromText} />
            <div className="action-placeholders">
              <h4>Actions</h4>
              <p className="obj-desc">Press releases and rumors take effect immediately. Contracts and stance changes are submitted as faction orders.</p>
              <div className="placeholder-grid">
                <label className="placeholder-action">
                  <span>Press release</span>
                  <textarea placeholder="Press release text…" rows={3} disabled />
                  <button type="button" disabled title="Not connected yet">Submit press release</button>
                </label>
                <label className="placeholder-action">
                  <span>Rumor</span>
                  <textarea placeholder="Rumor text…" rows={3} disabled />
                  <button type="button" disabled title="Not connected yet">Submit rumor</button>
                </label>
                <label className="placeholder-action">
                  <span>Contract</span>
                  <textarea placeholder="Contract order text…" rows={3} disabled />
                  <button type="button" disabled title="Not connected yet">Submit contract (faction order)</button>
                </label>
                <label className="placeholder-action">
                  <span>Diplomacy stance</span>
                  <textarea placeholder={'DECLARE FACTION …'} rows={3} disabled />
                  <button type="button" disabled title="Not connected yet">Submit stance change (faction order)</button>
                </label>
              </div>
            </div>
          </div>
        )}

        {panel === 'bank' && report && (() => {
          const bankText = bankReportText(sections, reportFullText);
          const bank = parseBankSummary(bankText);
          const upkeep = estimateCashUpkeep(report.stacks, report.factionId);
          const interest = estimateInterest(bank);
          const galaxyText = sectionText(sections, 'galaxy', '');
          const markets = parseMarketOffers(galaxyText, report);
          return (
            <div className="sub-panel scroll-area">
              <h3>Banking and market</h3>
              <div className="bank-summary-grid">
                <div className="bank-summary-card">
                  <strong>Estimated cash upkeep</strong>
                  <span>{upkeep} cash / turn (owned units)</span>
                </div>
                <div className="bank-summary-card">
                  <strong>Estimated interest</strong>
                  <span>
                    {interest.depositIncome != null && `+${interest.depositIncome} deposit`}
                    {interest.creditCost != null && `−${interest.creditCost} credit`}
                    {interest.depositIncome == null && interest.creditCost == null && '—'}
                  </span>
                </div>
              </div>
              <h4>Bank report</h4>
              <ClickableReportText text={bankText} onFocusId={focusFromText} />
              <h4>Market offers</h4>
              {markets.length === 0 ? (
                <p className="obj-desc">No market offers found in galaxy report.</p>
              ) : (
                markets.map((group) => (
                  <div key={group.locationLabel} className="market-offer-group">
                    <strong>{group.locationLabel}</strong>
                    <ClickableReportText text={group.lines.join('\n')} onFocusId={focusFromText} />
                  </div>
                ))
              )}
            </div>
          );
        })()}

        {panel === 'movement' && report && (
          <MovementPanel report={report} ordersText={draftOrders || factionOrdersText} />
        )}

        {panel === 'movement' && !report && (
          <div className="sub-panel scroll-area">
            <h3>Movement</h3>
            <p className="obj-desc">Load a report to list unit MOVE orders.</p>
          </div>
        )}

        {panel === 'battle' && (
          <div className="sub-panel scroll-area">
            <h3>Battle summaries</h3>
            <ClickableReportText
              text={sectionText(sections, 'battles', 'No battles this quarter.')}
              onFocusId={focusFromText}
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
              <div className="faction-panel-title-row">
                <h3>Faction report</h3>
                <div className="order-mode-tabs" role="group" aria-label="Faction page">
                  <button
                    type="button"
                    className={factionView === 'report' ? 'active' : ''}
                    onClick={() => setFactionView('report')}
                  >
                    Report
                  </button>
                  <button
                    type="button"
                    className={factionView === 'orders' ? 'active' : ''}
                    onClick={() => setFactionView('orders')}
                  >
                    Orders
                  </button>
                </div>
              </div>
              {factionView === 'report' && (
                <input
                  type="search"
                  className="report-search"
                  placeholder="Search faction report…"
                  value={factionSearch}
                  onChange={(e) => setFactionSearch(e.target.value)}
                />
              )}
            </div>
            {factionView === 'report' ? (
              <div className="faction-panel-body">
                <ReportSearchText
                  text={[
                    sectionText(sections, 'events', ''),
                    sectionText(sections, 'survey', ''),
                    sectionText(sections, 'galaxy', 'No galaxy report section.'),
                  ].filter(Boolean).join('\n\n')}
                  query={factionSearch}
                  onFocusId={focusFromText}
                />
              </div>
            ) : (
              <div className="faction-orders">
                <div className="faction-orders-top" style={{ height: factionOrdersTop }}>
                  <textarea
                    className="faction-orders-text"
                    value={factionOrderText}
                    onChange={(e) => {
                      factionOrdersDirty.current = true;
                      setFactionOrderText(e.target.value);
                    }}
                    spellCheck={false}
                  />
                  <div className="order-split-actions">
                    <button type="button" onClick={handleFactionParse}>Parse orders</button>
                    <button type="button" onClick={handleFactionSubmit}>Submit orders</button>
                  </div>
                </div>
                <ResizeHandle
                  direction="vertical"
                  className="faction-orders-resize"
                  onDelta={(d) => setFactionOrdersTop((h) => Math.max(120, Math.min(720, h + d)))}
                />
                <pre className="faction-orders-parse">{factionParseOutput}</pre>
              </div>
            )}
          </div>
        )}

        {panel === 'story' && meta.admin && (
          <div className="story-panel">
            <div className="story-panel-toolbar">
              <h3>Persona &amp; story</h3>
              <span className="system-view-star">Faction {report?.factionId ?? '—'}</span>
            </div>
            <div className={`story-panel-body${meta.admin ? '' : ' story-panel-body-no-ai'}`}>
              <div className="story-editor-col">
                <label className="story-field-label">
                  Persona
                  <select
                    value={selectedPersonaId}
                    onChange={(e) => setSelectedPersonaId(e.target.value)}
                  >
                    {personas.map((p) => (
                      <option key={p.id} value={p.id}>{p.label}</option>
                    ))}
                  </select>
                </label>
                <textarea
                  className={meta.admin ? '' : 'order-text-readonly'}
                  readOnly={!meta.admin}
                  value={personaText}
                  onChange={(e) => setPersonaText(e.target.value)}
                  placeholder="Shared persona description (GM editable)…"
                />
                {meta.admin && (
                  <button type="button" disabled={storyBusy} onClick={handleSavePersonaDraft}>
                    Save persona
                  </button>
                )}
              </div>
              <div className="story-editor-col">
                <label className="story-field-label">Faction story</label>
                <textarea
                  value={storyText}
                  onChange={(e) => setStoryText(e.target.value)}
                  placeholder="Campaign story for this faction…"
                />
                <div className="order-split-actions">
                  <button type="button" disabled={storyBusy} onClick={handleSaveStoryDraft}>Save story</button>
                  <button type="button" disabled={storyBusy} onClick={handleRegenerateStory}>
                    Regenerate story
                  </button>
                </div>
              </div>
              {meta.admin && (
                <AiPane
                  heading="Story AI"
                  aiMode={storyAiMode}
                  onAiModeChange={setStoryAiMode}
                  includeStory={storyIncludeStory}
                  onIncludeStoryChange={setStoryIncludeStory}
                  promptText={storyAiPrompt}
                  onPromptChange={setStoryAiPrompt}
                  outputText={storyAiOutput}
                  runpodStatus={runpodStatus}
                  runpodBusy={runpodBusy}
                  queryBusy={aiQueryBusy}
                  onStartRunpod={handleStartRunpod}
                  onStopRunpod={handleStopRunpod}
                  onSubmit={() => handleAiQuery(true)}
                  submitLabel="Query story AI"
                />
              )}
            </div>
          </div>
        )}

        {panel === 'map' && (
          <div className="order-editor" style={{ height: orderPaneHeight }}>
            <ResizeHandle direction="vertical" onDelta={(d) => setOrderPaneHeight((h) => Math.max(120, Math.min(480, h - d)))} className="order-editor-resize" />
            <div className="order-split">
              <div className="order-split-col order-split-orders">
                <div className="order-split-heading-row">
                  <h4 className="order-split-heading">Orders</h4>
                  <div className="order-mode-tabs">
                    <button
                      type="button"
                      className={orderMode === 'units' ? 'active' : ''}
                      onClick={() => { setUnitsEditText(null); setOrderMode('units'); }}
                    >
                      Units
                    </button>
                    <button
                      type="button"
                      className={orderMode === 'faction' ? 'active' : ''}
                      onClick={() => setOrderMode('faction')}
                    >
                      Full order file
                    </button>
                    <button
                      type="button"
                      className={`${orderMode === 'parser' ? 'active' : ''}${parseButtonStatus ? ` parse-tab-${parseButtonStatus}` : ''}`}
                      onClick={() => setOrderMode('parser')}
                    >
                      Parser output
                    </button>
                  </div>
                </div>
                <div className="order-pane-stack">
                  <div
                    className={`order-editor-with-lines${orderMode === 'parser' ? ' order-pane-hidden' : ''}`}
                  >
                    <div
                      ref={orderLineNumbersRef}
                      className="order-line-numbers"
                      aria-hidden
                    >
                      {Array.from({ length: orderLineCount }, (_, i) => (
                        <div key={i + 1} className="order-line-number">{i + 1}</div>
                      ))}
                    </div>
                    <textarea
                      ref={orderTextareaRef}
                      value={orderEditorText}
                      readOnly={orderEditorReadOnly}
                      onScroll={syncOrderLineNumbers}
                      onSelect={(e) => syncOrderCaret(e.currentTarget)}
                      onKeyUp={(e) => syncOrderCaret(e.currentTarget)}
                      onClick={(e) => syncOrderCaret(e.currentTarget)}
                      onChange={(e) => {
                        syncOrderCaret(e.currentTarget);
                        if (orderMode === 'faction') {
                          publishMapOrders(e.target.value);
                        } else if (orderMode === 'units') {
                          setUnitsEditText(e.target.value);
                          publishMapOrders(applyFocusedOrderEdits(
                            draftOrders || factionOrdersText,
                            e.target.value,
                            unitsFocus,
                          ));
                        }
                      }}
                      placeholder={'#faction N "password"\nMOVE …'}
                      className={orderEditorReadOnly ? 'order-text-readonly' : ''}
                    />
                  </div>
                  <pre
                    className={`order-parser-output${orderMode === 'parser' ? '' : ' order-pane-hidden'}`}
                  >
                    {parserPaneText}
                  </pre>
                </div>
                <div className="order-split-actions">
                  <button type="button" onClick={handleCheck}>Parse orders</button>
                  <button type="button" onClick={handleSubmit}>Submit orders</button>
                </div>
              </div>
              {meta.admin && (
                <>
                  <ResizeHandle direction="horizontal" onDelta={(d) => setAiPaneWidth((w) => Math.max(160, w - d))} />
                  <div style={{ width: aiPaneWidth, flexShrink: 0 }}>
                    <AiPane
                      heading="AI prompt"
                      aiMode={aiMode}
                      onAiModeChange={setAiMode}
                      includeStory={includeStory}
                      onIncludeStoryChange={setIncludeStory}
                      promptText={aiPromptText}
                      onPromptChange={setAiPromptText}
                      outputText={aiOutputText}
                      runpodStatus={runpodStatus}
                      runpodBusy={runpodBusy}
                      queryBusy={aiQueryBusy}
                      onStartRunpod={handleStartRunpod}
                      onStopRunpod={handleStopRunpod}
                      onSubmit={() => handleAiQuery(false)}
                    />
                  </div>
                </>
              )}
            </div>
          </div>
        )}
      </main>

      {meta.admin && (
        <RunPodStartModal
          open={runpodModalOpen}
          status={runpodStatus}
          startResponse={runpodStartResponse}
          busy={runpodBusy}
          onClose={() => setRunpodModalOpen(false)}
        />
      )}

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
          onFocusId={focusFromText}
          width={sidePanelWidth}
          onResize={(d) => setSidePanelWidth((w) => Math.max(240, Math.min(640, w + d)))}
        />
      )}
    </div>
  );
}

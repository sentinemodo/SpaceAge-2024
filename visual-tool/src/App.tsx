import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  login,
  fetchMeta,
  fetchReportXml,
  checkOrders,
  submitOrders,
  getToken,
  setToken,
} from './api/client';
import {
  parseReportXml,
  flattenStacks,
  filterStacksBySystem,
  estimateMoveWeeks,
  type ParsedReport,
  type StackNode,
} from './parsers/reportXml';

type Panel = 'map' | 'tech' | 'diplomacy' | 'contracts' | 'bank' | 'battle' | 'faction';
type OrderMode = 'order' | 'prompt';

function LoginScreen({ onLogin }: { onLogin: () => void }) {
  const [factionId, setFactionId] = useState('2');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    try {
      await login(parseInt(factionId, 10), password);
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
      {error && <p className="warnings">{error}</p>}
      <button type="submit">Enter client</button>
    </form>
  );
}

function UnitTree({
  stacks,
  selectedId,
  onSelect,
}: {
  stacks: StackNode[];
  selectedId: string | null;
  onSelect: (id: string) => void;
}) {
  function render(nodes: StackNode[]) {
    return (
      <ul>
        {nodes.map((n) => (
          <li key={n.id}>
            <div
              className={`unit-row ${selectedId === n.id ? 'selected' : ''}`}
              onClick={() => onSelect(n.id)}
            >
              {n.id} {n.type || n.name} ×{n.quantity ?? 1}
              {n.locationName ? ` @ ${n.locationName}` : ''}
            </div>
            {n.children.length > 0 && render(n.children)}
          </li>
        ))}
      </ul>
    );
  }
  return <div className="unit-tree">{render(stacks)}</div>;
}

export default function App() {
  const [authed, setAuthed] = useState(!!getToken());
  const [meta, setMeta] = useState<{ name?: string; turn?: number }>({});
  const [report, setReport] = useState<ParsedReport | null>(null);
  const [panel, setPanel] = useState<Panel>('map');
  const [filterSystems, setFilterSystems] = useState<string[]>([]);
  const [regionView, setRegionView] = useState<string | null>(null);
  const [selectedStack, setSelectedStack] = useState<string | null>(null);
  const [orderText, setOrderText] = useState('');
  const [orderMode, setOrderMode] = useState<OrderMode>('order');
  const [warnings, setWarnings] = useState<string[]>([]);
  const [sideTab, setSideTab] = useState<'units' | 'orders'>('units');

  const load = useCallback(async () => {
    const m = await fetchMeta();
    setMeta(m);
    try {
      const xml = await fetchReportXml();
      setReport(parseReportXml(xml));
    } catch {
      setReport(null);
    }
  }, []);

  useEffect(() => {
    if (authed) load();
  }, [authed, load]);

  const flatStacks = useMemo(() => (report ? flattenStacks(report.stacks) : []), [report]);

  const filteredStacks = useMemo(() => {
    if (!report) return [];
    let s = report.stacks;
    if (filterSystems.length) {
      s = filterStacksBySystem(flatStacks, filterSystems[0]).filter((x) =>
        filterSystems.every((f) => x.systemId === f || x.locationId === f)
      );
      return s;
    }
    return s;
  }, [report, filterSystems, flatStacks]);

  const selected = flatStacks.find((s) => s.id === selectedStack);
  const moveWeeks = selected ? estimateMoveWeeks(selected.mass) : null;

  function handleSystemClick(id: string, shift: boolean) {
    if (shift) {
      setFilterSystems((prev) =>
        prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]
      );
    } else {
      setFilterSystems([id]);
    }
  }

  async function handleCheck() {
    const w = await checkOrders(orderText);
    setWarnings(w);
  }

  async function handleSubmit() {
    await handleCheck();
    await submitOrders(orderText);
    alert('Orders submitted to game host');
  }

  if (!authed) {
    return <LoginScreen onLogin={() => setAuthed(true)} />;
  }

  return (
    <div className="app-shell">
      <header className="top-bar">
        <strong>{meta.name || 'SpaceAge'}</strong>
        <span>Turn {meta.turn ?? '—'}</span>
        <button type="button" onClick={() => { setToken(null); setAuthed(false); }}>Logout</button>
        <button type="button" onClick={load}>Refresh report</button>
      </header>

      <nav className="icon-rail" aria-label="Panels">
        {(
          [
            ['map', '🗺'],
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
            title={id}
            onClick={() => setPanel(id)}
          >
            {icon}
          </button>
        ))}
      </nav>

      <main className="main-stage">
        {panel === 'map' && report && (
          <div
            className="star-map"
            onDoubleClick={() => setRegionView(filterSystems[0] || null)}
          >
            {report.systems.map((sys) => (
              <button
                key={sys.id}
                type="button"
                className={`system-node ${filterSystems.includes(sys.id) ? 'selected' : ''} ${filterSystems.length && !filterSystems.includes(sys.id) ? 'filtered' : ''}`}
                style={{ left: `${sys.x}%`, top: `${sys.y}%` }}
                onClick={(e) => handleSystemClick(sys.id, e.shiftKey)}
              >
                {sys.name}
              </button>
            ))}
            {selected && moveWeeks != null && (
              <div style={{ position: 'absolute', bottom: 8, left: 8, fontSize: '0.8rem', color: 'var(--accent)' }}>
                MOVE ETA ~{moveWeeks} weeks (estimate)
              </div>
            )}
            {regionView && (
              <div style={{ position: 'absolute', top: 8, right: 8, background: 'var(--bg-card)', padding: '0.5rem', borderRadius: 4 }}>
                Region view: {regionView}
              </div>
            )}
          </div>
        )}

        {panel === 'tech' && (
          <div className="sub-panel scroll-area">
            <h3>Technologies</h3>
            <ul>{report?.technologies.map((t) => <li key={t}>{t}</li>) ?? <li>Load report…</li>}</ul>
          </div>
        )}

        {panel === 'diplomacy' && (
          <div className="sub-panel scroll-area">
            <h3>Diplomacy</h3>
            {report?.diplomacy.length ? report.diplomacy.map((d, i) => <p key={i}>{d}</p>) : <p>No diplomacy entries in report.</p>}
          </div>
        )}

        {panel === 'contracts' && (
          <div className="sub-panel scroll-area">
            <h3>Contracts</h3>
            {report?.faction.contracts.map((c, i) => (
              <p key={i} role="button" onClick={() => setFilterSystems([c.slice(0, 6)])}>{c}</p>
            )) ?? null}
          </div>
        )}

        {panel === 'bank' && (
          <div className="sub-panel scroll-area">
            <h3>Bank</h3>
            {report?.bank.map((b, i) => <p key={i}>{b}</p>)}
          </div>
        )}

        {panel === 'battle' && (
          <div className="sub-panel scroll-area">
            <h3>Battle summaries</h3>
            {report?.battles.length
              ? report.battles.map((b) => <pre key={b.id} style={{ whiteSpace: 'pre-wrap' }}>{b.text}</pre>)
              : <p>No battles this quarter.</p>}
          </div>
        )}

        {panel === 'faction' && (
          <div className="sub-panel scroll-area">
            <h3>Faction summary</h3>
            <p>Upkeep: {report?.faction.upkeep || '—'}</p>
            <p>Research output: {report?.faction.research || '—'}</p>
            <h4>Press</h4>
            {report?.faction.press.map((p, i) => <p key={i}>{p}</p>)}
          </div>
        )}

        {panel === 'map' && (
          <div className="order-editor" style={{ padding: '0.75rem', borderTop: '1px solid var(--border)' }}>
            <div className="panel-tabs">
              <button type="button" className={orderMode === 'order' ? 'active' : ''} onClick={() => setOrderMode('order')}>Orders</button>
              <button type="button" className={orderMode === 'prompt' ? 'active' : ''} onClick={() => setOrderMode('prompt')}>AI prompt</button>
            </div>
            <textarea
              value={orderText}
              onChange={(e) => setOrderText(e.target.value)}
              placeholder={orderMode === 'order' ? '#faction N "password"\nMOVE …' : 'Describe strategic intent for AI…'}
            />
            {warnings.map((w) => <div key={w} className="warnings">{w}</div>)}
            <button type="button" onClick={handleCheck}>Check warnings</button>
            <button type="button" onClick={handleSubmit}>Submit orders</button>
          </div>
        )}
      </main>

      <aside className="side-panel">
        <div className="panel-tabs">
          <button type="button" className={sideTab === 'units' ? 'active' : ''} onClick={() => setSideTab('units')}>Units</button>
          <button type="button" className={sideTab === 'orders' ? 'active' : ''} onClick={() => setSideTab('orders')}>Orders</button>
        </div>
        <div className="scroll-area">
          {sideTab === 'units' ? (
            <UnitTree stacks={filteredStacks} selectedId={selectedStack} onSelect={setSelectedStack} />
          ) : (
            <pre style={{ fontSize: '0.75rem', whiteSpace: 'pre-wrap' }}>{orderText || 'No orders drafted'}</pre>
          )}
        </div>
      </aside>
    </div>
  );
}

import { useCallback, useEffect, useRef, useState } from 'react';
import {
  fetchAdminFactionOrders,
  fetchLobbyStatus,
  parseAdminFactionOrders,
  saveAdminFactionOrders,
  syncLobbyStatus,
  type FactionOption,
  type LobbyStatus,
} from '../api/client';

const STATUS_LABELS: Record<string, string> = {
  'not-started': 'Not started',
  'accepting-orders': 'Accepting orders',
  processing: 'Processing turn',
  'reports-out': 'Reports out',
};

function formatNextTurn(nextTurnAt: string | null | undefined): string {
  if (!nextTurnAt) return 'GM-scheduled';
  return nextTurnAt;
}

export function GameManagementPanel({ factions }: { factions: FactionOption[] }) {
  const playerFactions = factions.filter((f) => !f.npc);
  const [lobbyStatus, setLobbyStatus] = useState<LobbyStatus | null>(null);
  const [statusBusy, setStatusBusy] = useState(false);
  const [syncBusy, setSyncBusy] = useState(false);
  const [statusError, setStatusError] = useState('');

  const [quickFactionId, setQuickFactionId] = useState(playerFactions[0]?.id ?? 2);
  const [orderText, setOrderText] = useState('');
  const [orderTurn, setOrderTurn] = useState<number | null>(null);
  const [orderVersion, setOrderVersion] = useState<number | null>(null);
  const [ordersLoading, setOrdersLoading] = useState(false);
  const [ordersBusy, setOrdersBusy] = useState(false);
  const [parseOutput, setParseOutput] = useState('Parse orders to validate before saving.');
  const ordersDirty = useRef(false);

  const refreshTurnStatus = useCallback(async () => {
    setStatusBusy(true);
    setStatusError('');
    try {
      const data = await fetchLobbyStatus();
      setLobbyStatus(data);
    } catch (err) {
      setStatusError(String(err));
    } finally {
      setStatusBusy(false);
    }
  }, []);

  const publishSubmissionStatus = useCallback(async () => {
    setSyncBusy(true);
    setStatusError('');
    try {
      const data = await syncLobbyStatus();
      setLobbyStatus(data);
    } catch (err) {
      setStatusError(String(err));
    } finally {
      setSyncBusy(false);
    }
  }, []);

  const loadQuickOrders = useCallback(async (factionId: number) => {
    setOrdersLoading(true);
    try {
      const data = await fetchAdminFactionOrders(factionId);
      if (!ordersDirty.current) {
        setOrderText(data.text);
        setOrderTurn(data.turn);
        setOrderVersion(data.version);
        setParseOutput(
          data.submitted
            ? `Loaded turn ${data.turn} orders (version ${data.version ?? '—'}).`
            : `No submitted orders for turn ${data.turn}.`,
        );
      }
    } catch (err) {
      setParseOutput(String(err));
    } finally {
      setOrdersLoading(false);
    }
  }, []);

  useEffect(() => {
    refreshTurnStatus().catch(() => {});
  }, [refreshTurnStatus]);

  useEffect(() => {
    ordersDirty.current = false;
    loadQuickOrders(quickFactionId).catch(() => {});
  }, [quickFactionId, loadQuickOrders]);

  async function handleParse() {
    setOrdersBusy(true);
    try {
      const result = await parseAdminFactionOrders(quickFactionId, orderText);
      const lines = [result.output || `ok: ${result.ok}`];
      if (result.errors.length) lines.push('', ...result.errors.map((e) => `ERROR: ${e}`));
      if (result.warnings.length) lines.push('', ...result.warnings.map((w) => `WARN: ${w}`));
      setParseOutput(lines.join('\n'));
    } catch (err) {
      setParseOutput(String(err));
    } finally {
      setOrdersBusy(false);
    }
  }

  async function handleSave() {
    setOrdersBusy(true);
    try {
      const saved = await saveAdminFactionOrders(quickFactionId, orderText);
      ordersDirty.current = false;
      setOrderTurn(saved.turn);
      setOrderVersion(saved.version);
      setParseOutput(`Saved turn ${saved.turn} as version ${saved.version}.`);
      await refreshTurnStatus();
    } catch (err) {
      setParseOutput(String(err));
    } finally {
      setOrdersBusy(false);
    }
  }

  const submittedById = new Map(
    (lobbyStatus?.factions ?? []).map((f) => [f.id, f.submitted]),
  );

  return (
    <div className="sub-panel game-management-panel scroll-area">
      <div className="game-management-header">
        <h3>Game management</h3>
        <p className="obj-desc">
          Turn submission overview. The public lobby (GitHub Pages) reads live status from game-host after you publish.
          Faction picker here does not change the report view in the header.
        </p>
      </div>

      <div className="game-management-status-card">
        <div className="game-management-metrics">
          <div>
            <span className="game-management-metric-label">Turn status</span>
            <strong>{lobbyStatus ? STATUS_LABELS[lobbyStatus.status] ?? lobbyStatus.status : '—'}</strong>
          </div>
          <div>
            <span className="game-management-metric-label">Turn</span>
            <strong>{lobbyStatus?.turn ?? '—'}</strong>
          </div>
          <div>
            <span className="game-management-metric-label">Next turn</span>
            <strong>{formatNextTurn(lobbyStatus?.nextTurnAt)}</strong>
          </div>
        </div>
        <div className="game-management-actions">
          <button type="button" disabled={statusBusy} onClick={() => { void refreshTurnStatus(); }}>
            {statusBusy ? 'Refreshing…' : 'Refresh turn status'}
          </button>
          <button type="button" disabled={syncBusy} onClick={() => { void publishSubmissionStatus(); }}>
            {syncBusy ? 'Publishing…' : 'Publish lobby status (website + snapshot)'}
          </button>
          <button type="button" disabled title="Turn execution will be enabled in a later release">
            Run turn
          </button>
          <button type="button" disabled title="Turn advance will be enabled in a later release">
            Progress to next turn
          </button>
        </div>
        {statusError && <p className="warnings">{statusError}</p>}
      </div>

      <h4>Order submissions (turn {lobbyStatus?.turn ?? '—'})</h4>
      <div className="game-management-submissions">
        {playerFactions.map((f) => {
          const submitted = submittedById.get(f.id) ?? false;
          return (
            <button
              key={f.id}
              type="button"
              className={`game-management-faction-chip ${submitted ? 'submitted' : 'pending'} ${quickFactionId === f.id ? 'selected' : ''}`}
              onClick={() => setQuickFactionId(f.id)}
              title={`Quick-edit orders for ${f.name}`}
            >
              <span>{f.name}</span>
              <span className="game-management-chip-status">{submitted ? '✓' : '…'}</span>
            </button>
          );
        })}
      </div>

      <div className="game-management-orders">
        <div className="game-management-orders-toolbar">
          <label>
            Quick faction
            <select
              value={quickFactionId}
              onChange={(e) => setQuickFactionId(parseInt(e.target.value, 10))}
            >
              {playerFactions.map((f) => (
                <option key={f.id} value={f.id}>{f.name} ({f.id})</option>
              ))}
            </select>
          </label>
          <span className="obj-desc">
            {ordersLoading
              ? 'Loading orders…'
              : `Turn ${orderTurn ?? '—'}${orderVersion != null ? ` · v${orderVersion}` : ''}`}
          </span>
          <button
            type="button"
            disabled={ordersLoading}
            onClick={() => {
              ordersDirty.current = false;
              void loadQuickOrders(quickFactionId);
            }}
          >
            Reload file
          </button>
        </div>
        <textarea
          className="game-management-orders-text"
          value={orderText}
          spellCheck={false}
          onChange={(e) => {
            ordersDirty.current = true;
            setOrderText(e.target.value);
          }}
          placeholder="#faction N &quot;password&quot;&#10;…"
        />
        <div className="order-split-actions">
          <button type="button" disabled={ordersBusy} onClick={() => { void handleParse(); }}>
            Parse orders
          </button>
          <button type="button" disabled={ordersBusy} onClick={() => { void handleSave(); }}>
            Save orders
          </button>
        </div>
        <pre className="game-management-parse-output">{parseOutput}</pre>
      </div>
    </div>
  );
}

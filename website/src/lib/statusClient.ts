import { formatNextTurn, formatStatusLabel, type StatusData } from './statusSchema';
import { withBase } from './paths';

async function fetchStatus(): Promise<StatusData | null> {
  try {
    const res = await fetch(withBase('status.json'), { cache: 'no-store' });
    if (!res.ok) return null;
    return (await res.json()) as StatusData;
  } catch {
    return null;
  }
}

export function bindStatusDashboard(root: HTMLElement): () => void {
  const statusEl = root.querySelector('#status-value');
  const turnEl = root.querySelector('#turn-value');
  const nextEl = root.querySelector('#next-turn-value');
  const grid = root.querySelector('#submissions-grid');

  async function refresh() {
    const data = await fetchStatus();
    if (!data) return;

    if (statusEl) statusEl.textContent = formatStatusLabel(data.status);
    if (turnEl) turnEl.textContent = String(data.turn);
    if (nextEl) nextEl.textContent = formatNextTurn(data.nextTurnAt);

    if (grid && Array.isArray(data.factions)) {
      grid.innerHTML = data.factions
        .map((f) => {
          const cls = f.submitted ? 'faction-card submitted' : 'faction-card pending';
          const name = f.name ?? `Faction ${f.id}`;
          const status = f.submitted ? '✓ Submitted' : '⏳ Pending';
          return `<div class="${cls}" data-id="${f.id}"><div class="faction-id">${name}</div><div class="faction-status">${status}</div></div>`;
        })
        .join('');
    }
  }

  refresh();
  const timer = window.setInterval(refresh, 30000);
  return () => window.clearInterval(timer);
}

export function bindTurnsPage(root: HTMLElement): () => void {
  const statusEl = root.querySelector('#turns-status-value');
  const turnEl = root.querySelector('#turns-turn-value');
  const ordersEl = root.querySelector('#turns-orders-count');
  const nextEl = root.querySelector('#turns-next-value');
  const table = root.querySelector('#turns-factions-table');

  async function refresh() {
    const data = await fetchStatus();
    if (!data) return;

    const submittedCount = data.factions.filter((f) => f.submitted).length;

    if (statusEl) statusEl.textContent = formatStatusLabel(data.status);
    if (turnEl) turnEl.textContent = String(data.turn);
    if (ordersEl) ordersEl.textContent = `${submittedCount} / 10`;
    if (nextEl) nextEl.textContent = formatNextTurn(data.nextTurnAt);

    if (table && Array.isArray(data.factions)) {
      table.innerHTML = data.factions
        .map((f) => {
          const statusClass = f.submitted ? 'submitted' : 'pending';
          const statusText = f.submitted ? '✓ Submitted' : '⏳ Awaiting Orders';
          const name = f.name ?? `Faction ${f.id}`;
          return `<div class="table-row" data-faction-id="${f.id}"><div class="table-cell faction-col"><strong>${name}</strong></div><div class="table-cell status-col ${statusClass}">${statusText}</div></div>`;
        })
        .join('');
    }
  }

  refresh();
  const timer = window.setInterval(refresh, 30000);
  return () => window.clearInterval(timer);
}

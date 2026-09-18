import { campaignFactionsByPlanet, campaignFactionLabel } from '../data/campaignFactions';
import { clientFactionUrl } from './clientUrls';
import { formatNextTurn, formatStatusLabel, type StatusData } from './statusSchema';
import { withBase } from './paths';

function escapeHtml(text: string): string {
  return text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/"/g, '&quot;');
}

async function fetchStatus(): Promise<StatusData | null> {
  try {
    const res = await fetch(withBase('status.json'), { cache: 'no-store' });
    if (!res.ok) return null;
    return (await res.json()) as StatusData;
  } catch {
    return null;
  }
}

function factionName(f: { id: number; name?: string }): string {
  if (f.name && !/^Faction \d+$/.test(f.name)) return f.name;
  return campaignFactionLabel(f.id);
}

function renderDashboardGrid(factions: StatusData['factions']): string {
  const byId = new Map(factions.map((f) => [f.id, f]));

  return campaignFactionsByPlanet
    .map((group) => {
      const cards = group.factions
        .map((faction) => {
          const live = byId.get(faction.id);
          const submitted = live?.submitted ?? false;
          const cls = submitted ? 'faction-card submitted' : 'faction-card pending';
          const status = submitted ? '✓ Submitted' : '⏳ Pending';
          const name = live ? factionName(live) : `${faction.name} (${faction.id})`;
          const href = clientFactionUrl(faction.id);
          return `<div class="${cls}" data-id="${faction.id}"><div class="faction-id"><a href="${escapeHtml(href)}" class="faction-link">${escapeHtml(name)}</a></div><div class="faction-status">${status}</div></div>`;
        })
        .join('');

      return `<div class="planet-group"><h3 class="planet-heading">${group.planet}</h3><div class="planet-factions">${cards}</div></div>`;
    })
    .join('');
}

function renderTurnsTable(factions: StatusData['factions']): string {
  const byId = new Map(factions.map((f) => [f.id, f]));

  return campaignFactionsByPlanet
    .map((group) => {
      const rows = group.factions
        .map((faction) => {
          const live = byId.get(faction.id);
          const submitted = live?.submitted ?? false;
          const statusClass = submitted ? 'submitted' : 'pending';
          const statusText = submitted ? '✓ Submitted' : '⏳ Awaiting Orders';
          const name = live ? factionName(live) : `${faction.name} (${faction.id})`;
          return `<div class="table-row" data-faction-id="${faction.id}"><div class="table-cell faction-col"><strong>${name}</strong></div><div class="table-cell status-col ${statusClass}">${statusText}</div></div>`;
        })
        .join('');

      return `<div class="planet-group"><h3 class="planet-heading">${group.planet}</h3><div class="factions-table"><div class="table-header"><div class="table-cell faction-col">Faction</div><div class="table-cell status-col">Orders Submitted</div></div>${rows}</div></div>`;
    })
    .join('');
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
      grid.innerHTML = renderDashboardGrid(data.factions);
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
      table.innerHTML = renderTurnsTable(data.factions);
    }
  }

  refresh();
  const timer = window.setInterval(refresh, 30000);
  return () => window.clearInterval(timer);
}

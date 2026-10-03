import { campaignFactionsByPlanet, campaignFactionLabel } from '../data/campaignFactions';
import { clientFactionUrl } from './clientUrls';
import { formatNextTurn, formatStatusLabel, validateStatusJson, type StatusData } from './statusSchema';
import { renderIssuesTableHtml } from './issuesTable';
import { renderTurnsTableHtml } from './turnsTable';
import { withBase } from './paths';
import { lobbyStatusLiveUrl, lobbyStatusLocalDevUrl } from './statusUrls';

function escapeHtml(text: string): string {
  return text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/"/g, '&quot;');
}

function statusFetchUrls(): string[] {
  const urls: string[] = [];
  if (typeof window !== 'undefined') {
    const host = window.location.hostname;
    if (host === 'localhost' || host === '127.0.0.1') {
      urls.push(lobbyStatusLocalDevUrl());
    }
  }
  const live = lobbyStatusLiveUrl();
  if (live) urls.push(live);
  urls.push(withBase('status.json'));
  return [...new Set(urls)];
}

async function fetchStatus(): Promise<StatusData | null> {
  for (const url of statusFetchUrls()) {
    try {
      const res = await fetch(url, { cache: 'no-store' });
      if (!res.ok) continue;
      return validateStatusJson(await res.json());
    } catch {
      // try next source
    }
  }
  return null;
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
  return renderTurnsTableHtml(factions);
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
  const issuesEl = root.querySelector('#turns-issues-table');

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
    if (issuesEl) {
      issuesEl.innerHTML = renderIssuesTableHtml(data.issues);
    }
  }

  refresh();
  const timer = window.setInterval(refresh, 30000);
  return () => window.clearInterval(timer);
}

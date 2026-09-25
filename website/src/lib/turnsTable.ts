import { campaignFactionsByPlanet, campaignFactionLabel } from '../data/campaignFactions';
import type { StatusData } from './statusSchema';

type FactionRow = StatusData['factions'][number];

function escapeHtml(text: string): string {
  return text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/"/g, '&quot;');
}

function displayName(f: FactionRow | undefined, fallbackId: number, fallbackName: string): string {
  if (!f) return `${fallbackName} (${fallbackId})`;
  if (f.name && !/^Faction \d+$/.test(f.name)) return f.name;
  return campaignFactionLabel(f.id);
}

function statusCell(f: FactionRow | undefined): { className: string; text: string } {
  const submitted = f?.submitted ?? false;
  return submitted
    ? { className: 'status-col submitted', text: '✓ Submitted' }
    : { className: 'status-col pending', text: '⏳ Awaiting Orders' };
}

/** HTML for the Players & Turns faction table (Arbor | status | Anvil | status). */
export function renderTurnsTableHtml(factions: StatusData['factions'] | null): string {
  const byId = new Map((factions ?? []).map((f) => [f.id, f]));
  const [arborGroup, anvilGroup] = campaignFactionsByPlanet;

  const rows = arborGroup.factions
    .map((arborMeta, index) => {
      const anvilMeta = anvilGroup.factions[index];
      const arborLive = byId.get(arborMeta.id);
      const anvilLive = byId.get(anvilMeta.id);
      const arborStatus = statusCell(arborLive);
      const anvilStatus = statusCell(anvilLive);
      const arborName = escapeHtml(displayName(arborLive, arborMeta.id, arborMeta.name));
      const anvilName = escapeHtml(displayName(anvilLive, anvilMeta.id, anvilMeta.name));

      return `<div class="table-row">
<div class="table-cell faction-col" data-faction-id="${arborMeta.id}"><strong>${arborName}</strong></div>
<div class="table-cell ${arborStatus.className}">${arborStatus.text}</div>
<div class="table-cell faction-col" data-faction-id="${anvilMeta.id}"><strong>${anvilName}</strong></div>
<div class="table-cell ${anvilStatus.className}">${anvilStatus.text}</div>
</div>`;
    })
    .join('');

  return `<div class="factions-table">
<div class="table-planet-row">
<div class="planet-span">${arborGroup.planet}</div>
<div class="planet-span">${anvilGroup.planet}</div>
</div>
<div class="table-header table-row">
<div class="table-cell faction-col">Faction</div>
<div class="table-cell status-col">Orders Submitted</div>
<div class="table-cell faction-col">Faction</div>
<div class="table-cell status-col">Orders Submitted</div>
</div>
${rows}
</div>`;
}

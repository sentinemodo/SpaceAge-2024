import type { TurnIssue } from './statusSchema';

function escapeHtml(text: string): string {
  return text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/"/g, '&quot;');
}

function kindLabel(kind: TurnIssue['kind']): string {
  return kind === 'rumor' ? 'Rumor' : 'Contract';
}

export function renderIssuesTableHtml(issues: TurnIssue[] | undefined | null): string {
  if (!issues || issues.length === 0) {
    return '<p class="issues-empty">No between-turn rumors or contracts published yet.</p>';
  }

  const sorted = [...issues].sort((a, b) => {
    if (a.turn !== b.turn) return a.turn - b.turn;
    return a.no - b.no;
  });

  const rows = sorted
    .map((issue) => {
      const headline = issue.title ?? issue.contractId ?? 'Untitled';
      const meta = `Turn ${issue.turn}, issue ${issue.no} · ${kindLabel(issue.kind)}${issue.planetId ? ` · ${escapeHtml(issue.planetId)}` : ''}${issue.contractId ? ` · ${escapeHtml(issue.contractId)}` : ''}`;
      const flavour = issue.flavour
        ? `<p class="issue-flavour">${escapeHtml(issue.flavour)}</p>`
        : '';
      return `<article class="issue-row"><h3 class="issue-title">${escapeHtml(headline)}</h3><p class="issue-meta">${meta}</p>${flavour}</article>`;
    })
    .join('');

  return `<div class="issues-list">${rows}</div>`;
}

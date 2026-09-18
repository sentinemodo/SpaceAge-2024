/** Faction id from ?faction= on the client URL (lobby links from the website). */

export function readUrlFactionId(): number | null {
  const raw = new URLSearchParams(window.location.search).get('faction');
  if (!raw) return null;
  const id = parseInt(raw, 10);
  if (Number.isNaN(id) || id < 2 || id > 11) return null;
  return id;
}

export function defaultLoginFactionId(): string {
  return String(readUrlFactionId() ?? 2);
}

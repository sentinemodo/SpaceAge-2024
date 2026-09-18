/** Hosted visual client base URL (game-host /client/). */
export function clientBaseUrl(): string {
  return import.meta.env.PUBLIC_CLIENT_URL || 'http://localhost:8787/client/';
}

/** Open client with faction pre-selected (?faction=). Login prompts if needed. */
export function clientFactionUrl(factionId: number): string {
  const url = new URL(clientBaseUrl());
  url.searchParams.set('faction', String(factionId));
  return url.toString();
}

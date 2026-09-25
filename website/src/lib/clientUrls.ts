/** Production client URL (Caddy → game-host). Override at build via PUBLIC_CLIENT_URL. */
export const HOSTED_CLIENT_URL = 'https://spaceage-pbem.duckdns.org/client/';

/** Hosted visual client base URL (game-host /client/). */
export function clientBaseUrl(): string {
  return import.meta.env.PUBLIC_CLIENT_URL || HOSTED_CLIENT_URL;
}

/** Open client with faction pre-selected (?faction=). Login prompts if needed. */
export function clientFactionUrl(factionId: number): string {
  const url = new URL(clientBaseUrl());
  url.searchParams.set('faction', String(factionId));
  return url.toString();
}

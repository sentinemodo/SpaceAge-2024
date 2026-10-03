import { clientBaseUrl, HOSTED_CLIENT_URL } from './clientUrls';

/** Live lobby status from game-host (same schema as public/status.json). */
export function lobbyStatusLiveUrl(): string | null {
  const explicit = import.meta.env.PUBLIC_STATUS_URL;
  if (typeof explicit === 'string' && explicit.trim()) {
    return explicit.trim();
  }
  const client = import.meta.env.PUBLIC_CLIENT_URL || HOSTED_CLIENT_URL;
  try {
    const u = new URL(client);
    u.pathname = '/api/public/lobby-status';
    u.search = '';
    return u.href;
  } catch {
    return null;
  }
}

/** Local game-host during Astro dev (localhost lobby). */
export function lobbyStatusLocalDevUrl(): string {
  return 'http://127.0.0.1:8787/api/public/lobby-status';
}

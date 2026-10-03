import { describe, it, expect } from 'vitest';
import { lobbyStatusLiveUrl } from '../src/lib/statusUrls';

describe('lobbyStatusLiveUrl', () => {
  it('derives live status URL from default hosted client', () => {
    expect(lobbyStatusLiveUrl()).toBe(
      'https://spaceage-pbem.duckdns.org/api/public/lobby-status',
    );
  });
});

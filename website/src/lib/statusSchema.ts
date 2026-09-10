export type StatusEnum =
  | 'not-started'
  | 'accepting-orders'
  | 'processing'
  | 'reports-out';

export interface FactionStatus {
  id: number;
  submitted: boolean;
  name?: string;
}

export interface StatusData {
  status: StatusEnum;
  turn: number;
  nextTurnAt?: string | null;
  factions: FactionStatus[];
}

const VALID_STATUSES: StatusEnum[] = [
  'not-started',
  'accepting-orders',
  'processing',
  'reports-out',
];

const FORBIDDEN_KEYS = ['password', 'email', 'path', 'report', 'orders'] as const;
const PLAYER_FACTION_IDS = [2, 3, 4, 5, 6, 7, 8, 9, 10, 11] as const;

export function formatStatusLabel(status: string): string {
  const labels: Record<string, string> = {
    'not-started': 'Not Started',
    'accepting-orders': 'Accepting Orders',
    processing: 'Processing',
    'reports-out': 'Reports Out',
  };
  return labels[status] ?? status;
}

export function formatNextTurn(nextTurnAt: string | null | undefined): string {
  if (!nextTurnAt) return 'GM-scheduled';
  return nextTurnAt;
}

export function validateStatusJson(data: unknown): StatusData {
  if (!data || typeof data !== 'object') {
    throw new Error('status.json must be an object');
  }

  const record = data as Record<string, unknown>;

  for (const key of Object.keys(record)) {
    if ((FORBIDDEN_KEYS as readonly string[]).includes(key)) {
      throw new Error(`Forbidden top-level key: ${key}`);
    }
  }

  if (!VALID_STATUSES.includes(record.status as StatusEnum)) {
    throw new Error(`Invalid status: ${String(record.status)}`);
  }

  if (typeof record.turn !== 'number' || record.turn < 0) {
    throw new Error('turn must be a number >= 0');
  }

  if (record.nextTurnAt !== undefined && record.nextTurnAt !== null && typeof record.nextTurnAt !== 'string') {
    throw new Error('nextTurnAt must be a string or null');
  }

  if (!Array.isArray(record.factions)) {
    throw new Error('factions must be an array');
  }

  if (record.factions.length !== 10) {
    throw new Error('factions must contain exactly 10 entries');
  }

  const ids = record.factions.map((faction) => {
    if (!faction || typeof faction !== 'object') {
      throw new Error('Each faction must be an object');
    }

    const entry = faction as Record<string, unknown>;
    for (const key of Object.keys(entry)) {
      if ((FORBIDDEN_KEYS as readonly string[]).includes(key)) {
        throw new Error(`Forbidden faction key: ${key}`);
      }
    }

    if (typeof entry.id !== 'number') {
      throw new Error('faction id must be a number');
    }
    if (typeof entry.submitted !== 'boolean') {
      throw new Error('faction submitted must be a boolean');
    }

    return entry.id;
  });

  if (ids.some((id) => id === 1 || id === 12 || id === 13)) {
    throw new Error('NPC factions 1, 12, 13 must not appear in status.json');
  }

  if (JSON.stringify(ids) !== JSON.stringify([...PLAYER_FACTION_IDS])) {
    throw new Error('factions must be ids 2 through 11');
  }

  return {
    status: record.status as StatusEnum,
    turn: record.turn as number,
    nextTurnAt: record.nextTurnAt as string | null | undefined,
    factions: record.factions as FactionStatus[],
  };
}

export function assertNoSecretLeaks(text: string): void {
  const forbidden = [/password/i, /gamein/i, /order\./i, /report\./i];
  for (const pattern of forbidden) {
    if (pattern.test(text)) {
      throw new Error(`Forbidden leak pattern matched: ${pattern}`);
    }
  }
}

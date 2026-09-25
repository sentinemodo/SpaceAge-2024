import fs from 'node:fs';
import crypto from 'node:crypto';
import { gameinPath } from './paths.mjs';

const sessions = new Map();

export const PLAYER_FACTION_MIN = 2;
export const PLAYER_FACTION_MAX = 11;

function isPlayerFaction(id) {
  return id >= PLAYER_FACTION_MIN && id <= PLAYER_FACTION_MAX;
}

/** Every faction row from gamein (players, UN, fauna, etc.). */
export function loadAllFactions() {
  const xml = fs.readFileSync(gameinPath(), 'utf8');
  const factions = new Map();
  const re = /<faction\s+name="(\d+)"[^>]*name-en="([^"]*)"[^>]*password="([^"]*)"/g;
  let m;
  while ((m = re.exec(xml)) !== null) {
    const id = parseInt(m[1], 10);
    factions.set(id, {
      id,
      name: m[2],
      password: m[3],
      npc: !isPlayerFaction(id),
    });
  }
  return factions;
}

/** Player factions only — used for password login and order validation. */
export function loadFactionCredentials() {
  const creds = new Map();
  for (const row of loadAllFactions().values()) {
    if (isPlayerFaction(row.id)) creds.set(row.id, row);
  }
  return creds;
}

export function getFaction(factionId) {
  return loadAllFactions().get(factionId);
}

export function login(factionId, password, gmKey) {
  const row = getFaction(factionId);
  const expectedGm = process.env.GAME_HOST_GM_KEY || 'dev-gm-key';
  const admin = typeof gmKey === 'string' && gmKey.trim() === expectedGm;
  if (!row || (!admin && (!isPlayerFaction(factionId) || row.password !== password))) {
    return null;
  }
  const token = crypto.randomBytes(24).toString('hex');
  sessions.set(token, {
    factionId,
    name: row.name,
    admin,
    viewAsFactionId: factionId,
    viewRunId: null,
    viewTurn: null,
    created: Date.now(),
  });
  return token;
}

/** Admin dropdown: all factions with npc flag for grouping in the client. */
export function listFactions() {
  return [...loadAllFactions().values()]
    .sort((a, b) => a.id - b.id)
    .map((f) => ({
      id: f.id,
      name: f.name,
      npc: f.npc,
    }));
}

export function effectiveFactionId(session) {
  return session.viewAsFactionId ?? session.factionId;
}

export function factionDisplayName(session) {
  const id = effectiveFactionId(session);
  const row = getFaction(id);
  return row?.name || session.name;
}

export function logout(token) {
  sessions.delete(token);
}

export function sessionTokenFromRequest(req) {
  const auth = req.headers.authorization || '';
  if (auth.startsWith('Bearer ')) {
    return auth.slice(7);
  }
  const cookie = (req.headers.cookie || '').split(';').map((s) => s.trim());
  for (const part of cookie) {
    if (part.startsWith('sa_session=')) {
      return part.slice('sa_session='.length);
    }
  }
  return null;
}

export function sessionFromRequest(req) {
  const token = sessionTokenFromRequest(req);
  return token ? sessions.get(token) || null : null;
}

export function requireSession(req, res) {
  const session = sessionFromRequest(req);
  if (!session) {
    res.writeHead(401, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ error: 'unauthorized' }));
    return null;
  }
  return session;
}

export function requireGm(req, res) {
  const key = process.env.GAME_HOST_GM_KEY || 'dev-gm-key';
  const provided = req.headers['x-gm-key'];
  if (provided !== key) {
    res.writeHead(403, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ error: 'forbidden' }));
    return null;
  }
  return true;
}

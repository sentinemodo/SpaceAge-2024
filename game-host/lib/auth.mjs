import fs from 'node:fs';
import crypto from 'node:crypto';
import { gameinPath } from './paths.mjs';

const sessions = new Map();

export function loadFactionCredentials() {
  const xml = fs.readFileSync(gameinPath(), 'utf8');
  const creds = new Map();
  const re = /<faction\s+name="(\d+)"[^>]*name-en="([^"]*)"[^>]*password="([^"]*)"/g;
  let m;
  while ((m = re.exec(xml)) !== null) {
    const id = parseInt(m[1], 10);
    if (id >= 2 && id <= 11) {
      creds.set(id, { id, name: m[2], password: m[3] });
    }
  }
  return creds;
}

export function login(factionId, password) {
  const creds = loadFactionCredentials();
  const row = creds.get(factionId);
  if (!row || row.password !== password) {
    return null;
  }
  const token = crypto.randomBytes(24).toString('hex');
  sessions.set(token, { factionId, name: row.name, created: Date.now() });
  return token;
}

export function logout(token) {
  sessions.delete(token);
}

export function sessionFromRequest(req) {
  const auth = req.headers.authorization || '';
  if (auth.startsWith('Bearer ')) {
    return sessions.get(auth.slice(7)) || null;
  }
  const cookie = (req.headers.cookie || '').split(';').map((s) => s.trim());
  for (const part of cookie) {
    if (part.startsWith('sa_session=')) {
      return sessions.get(part.slice('sa_session='.length)) || null;
    }
  }
  return null;
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
    return false;
  }
  return true;
}

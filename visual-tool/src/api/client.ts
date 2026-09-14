const API_BASE = import.meta.env.VITE_API_BASE || '';

let token: string | null = localStorage.getItem('sa_token');

export function setToken(t: string | null) {
  token = t;
  if (t) localStorage.setItem('sa_token', t);
  else localStorage.removeItem('sa_token');
}

export function getToken() {
  return token;
}

async function api(path: string, init: RequestInit = {}) {
  const headers: Record<string, string> = {
    ...(init.headers as Record<string, string>),
  };
  if (token) headers.Authorization = `Bearer ${token}`;
  const res = await fetch(`${API_BASE}${path}`, { ...init, headers });
  if (!res.ok) {
    const err = await res.json().catch(() => ({ error: res.statusText }));
    throw new Error(err.error || res.statusText);
  }
  const ct = res.headers.get('content-type') || '';
  if (ct.includes('json')) return res.json();
  return res.text();
}

export interface FactionOption {
  id: number;
  name: string;
}

export interface SessionMeta {
  factionId?: number;
  viewAsFactionId?: number;
  name?: string;
  turn?: number;
  admin?: boolean;
  factions?: FactionOption[];
}

export interface LoginResult {
  token: string;
  factionId: number;
  viewAsFactionId?: number;
  name?: string;
  admin?: boolean;
}

export async function login(factionId: number, password: string, gmKey?: string): Promise<LoginResult> {
  setToken(null);
  const headers: Record<string, string> = { 'Content-Type': 'application/json' };
  const trimmedGm = gmKey?.trim();
  if (trimmedGm) headers['X-GM-Key'] = trimmedGm;
  const data = await api('/api/auth/login', {
    method: 'POST',
    headers,
    body: JSON.stringify({ factionId, password, gmKey: trimmedGm }),
  }) as LoginResult;
  setToken(data.token);
  return data;
}

export async function fetchMeta(): Promise<SessionMeta> {
  return api('/api/session/meta');
}

export async function viewAsFaction(factionId: number) {
  return api('/api/session/view-as', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ factionId }),
  });
}

export async function fetchReportXml(): Promise<string> {
  return api('/api/session/report.xml');
}

export async function fetchReportTxt(): Promise<string> {
  return api('/api/session/report.txt');
}

export interface ReportSection {
  id: string;
  title: string;
  text: string;
}

export async function fetchReportSections(): Promise<ReportSection[]> {
  const data = await api('/api/session/report-sections');
  return data.sections || [];
}

export interface ParseOrdersResult {
  ok: boolean;
  errors: string[];
  warnings: string[];
  engineUnavailable?: boolean;
}

export async function parseOrders(text: string): Promise<ParseOrdersResult> {
  return api('/api/session/parse-orders', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ text }),
  });
}

export async function submitOrders(text: string) {
  return api('/api/session/orders', {
    method: 'PUT',
    headers: { 'Content-Type': 'text/plain; charset=utf-8' },
    body: text,
  });
}

export async function runBattleSim(xml: string, seed?: number) {
  return api('/api/session/battle-sim', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ xml, seed }),
  });
}

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

export async function login(factionId: number, password: string) {
  const data = await api('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ factionId, password }),
  });
  setToken(data.token);
  return data;
}

export async function fetchMeta() {
  return api('/api/session/meta');
}

export async function fetchReportXml(): Promise<string> {
  return api('/api/session/report.xml');
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

/** @deprecated prefer parseOrders for engine-backed validation */
export async function checkOrders(text: string): Promise<string[]> {
  const data = await api('/api/session/check-orders', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ text }),
  });
  return data.warnings || [];
}

export interface BattleSimResult {
  output: string;
  result: string | null;
}

export async function runBattleSim(xml: string, seed?: number): Promise<BattleSimResult> {
  return api('/api/session/battle-sim', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ xml, seed }),
  });
}

export async function submitOrders(text: string) {
  return api('/api/session/orders', {
    method: 'PUT',
    headers: { 'Content-Type': 'text/plain; charset=utf-8' },
    body: text,
  });
}

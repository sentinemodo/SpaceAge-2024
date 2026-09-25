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
  npc?: boolean;
}

export interface RunOption {
  id: string;
  label: string;
  /** True for the single campaign whose data.xml is newest. Marked "x" in the admin list. */
  playerVisible?: boolean;
}

export interface SessionMeta {
  factionId?: number;
  viewAsFactionId?: number;
  name?: string;
  turn?: number;
  viewTurn?: number;
  runId?: string;
  viewRunId?: string;
  runs?: RunOption[];
  turns?: number[];
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

export async function logout(): Promise<void> {
  try {
    if (token) {
      await api('/api/auth/logout', { method: 'POST' });
    }
  } catch {
    // Drop local credentials even if the host is unreachable.
  }
  setToken(null);
}

export async function login(factionId: number, password: string, gmKey?: string): Promise<LoginResult> {
  await logout();
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

export async function setSessionContext(ctx: { runId?: string; turn?: number }) {
  return api('/api/session/context', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(ctx),
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
  output?: string;
  engineUnavailable?: boolean;
}

export async function parseOrders(text: string): Promise<ParseOrdersResult> {
  return api('/api/session/parse-orders', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ text }),
  });
}

export async function fetchSubmittedOrders(): Promise<string | null> {
  const data = await api('/api/session/orders');
  if (!data.submitted || !String(data.text || '').trim()) return null;
  return data.text as string;
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

export type RunPodStage = 'idle' | 'starting' | 'attached' | 'warming' | 'ready' | 'failed';

export interface RunPodLogEntry {
  at: string;
  level: 'info' | 'warn' | 'error' | 'success';
  message: string;
  detail?: unknown;
}

export interface RunPodStatus {
  phase: 'stopped' | 'preparing' | 'running';
  stage?: RunPodStage;
  podId: string | null;
  ollamaHost: string | null;
  model: string;
  message: string;
  ollamaReady: boolean;
  gpuAttempt?: string | null;
  podStatus?: string | null;
  logs?: RunPodLogEntry[];
}

export async function fetchRunPodStatus(): Promise<RunPodStatus> {
  return api('/api/session/runpod/status');
}

export async function startRunPod(): Promise<RunPodStatus> {
  return api('/api/session/runpod/start', { method: 'POST' });
}

export async function stopRunPod(): Promise<RunPodStatus> {
  return api('/api/session/runpod/stop', { method: 'POST' });
}

export interface PersonaOption {
  id: string;
  label: string;
}

export async function fetchPersonas(): Promise<PersonaOption[]> {
  const data = await api('/api/session/personas');
  return data.personas || [];
}

export async function fetchPersona(personaId: string): Promise<string> {
  const data = await api(`/api/session/persona?id=${encodeURIComponent(personaId)}`);
  return data.text || '';
}

export async function savePersona(personaId: string, text: string) {
  return api('/api/session/persona', {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ personaId, text }),
  });
}

export async function fetchStory(): Promise<string> {
  const data = await api('/api/session/story');
  return data.text || '';
}

export async function saveStory(text: string) {
  return api('/api/session/story', {
    method: 'PUT',
    headers: { 'Content-Type': 'text/plain; charset=utf-8' },
    body: text,
  });
}

export interface AiQueryResult {
  ok: boolean;
  query: string;
  output: string;
  hits?: number;
  error?: string;
}

export async function submitAiQuery(prompt: string, includeStory: boolean): Promise<AiQueryResult> {
  return api('/api/session/ai/query', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ prompt, includeStory }),
  });
}

export async function regenerateStory(personaId: string): Promise<{ ok: boolean; story: string; output?: string }> {
  return api('/api/session/ai/draft-story', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ personaId }),
  });
}

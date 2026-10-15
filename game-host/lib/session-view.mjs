import {
  listRuns,
  reportPaths,
  runId,
} from './paths.mjs';
import { effectiveFactionId, isPlayerFaction } from './auth.mjs';

export function playerVisibleRunId() {
  const marked = listRuns().find((run) => run.playerVisible);
  return marked?.id || runId();
}

export function sessionRunId(session) {
  if (!session?.admin) return playerVisibleRunId();
  return session.viewRunId || runId();
}

export function runsForSession(session) {
  const runs = listRuns();
  if (session.admin) return runs;
  const visible = runs.filter((run) => run.playerVisible);
  if (visible.length) return visible;
  const fallback = runs.find((run) => run.id === runId());
  return fallback ? [fallback] : [];
}

export function sessionTurn(session) {
  return session.viewTurn ?? null;
}

export function reportPathsForSession(session, factionId) {
  return reportPaths(factionId, sessionRunId(session), sessionTurn(session));
}

export function orderContextForSession(session) {
  const run = sessionRunId(session);
  const factionId = effectiveFactionId(session);
  const { turn } = reportPaths(factionId, run, sessionTurn(session));
  return { run, factionId, turn };
}

export function orderContextForFactionId(session, factionId) {
  const run = sessionRunId(session);
  const { turn } = reportPaths(factionId, run, sessionTurn(session));
  return { run, factionId, turn };
}

export function parseFactionIdParam(url) {
  const raw = url.searchParams.get('factionId');
  if (!raw) return null;
  const factionId = parseInt(raw, 10);
  if (!Number.isFinite(factionId) || !isPlayerFaction(factionId)) return null;
  return factionId;
}

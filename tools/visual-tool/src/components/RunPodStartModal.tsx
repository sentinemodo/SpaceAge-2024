import { useEffect, useRef } from 'react';
import type { RunPodLogEntry, RunPodStage, RunPodStatus } from '../api/client';

const STAGES: { id: RunPodStage; label: string }[] = [
  { id: 'starting', label: 'Starting up' },
  { id: 'warming', label: 'Warming up' },
  { id: 'ready', label: 'Ready' },
];

function stageIndex(stage: RunPodStage | undefined): number {
  if (stage === 'ready') return 2;
  if (stage === 'warming') return 1;
  if (stage === 'starting') return 0;
  return -1;
}

function formatDetail(detail: unknown): string {
  if (detail == null) return '';
  if (typeof detail === 'string') return detail;
  try {
    return JSON.stringify(detail, null, 2);
  } catch {
    return String(detail);
  }
}

function LogLine({ entry }: { entry: RunPodLogEntry }) {
  const detail = formatDetail(entry.detail);
  return (
    <div className={`runpod-log-line runpod-log-${entry.level}`}>
      <span className="runpod-log-time">{entry.at.slice(11, 19)}</span>
      <span className="runpod-log-level">{entry.level}</span>
      <span className="runpod-log-message">{entry.message}</span>
      {detail ? <pre className="runpod-log-detail">{detail}</pre> : null}
    </div>
  );
}

export function RunPodStartModal({
  open,
  status,
  startResponse,
  busy,
  onClose,
}: {
  open: boolean;
  status: RunPodStatus | null;
  startResponse: RunPodStatus | null;
  busy: boolean;
  onClose: () => void;
}) {
  const logRef = useRef<HTMLDivElement>(null);
  const stage = status?.stage || startResponse?.stage || (busy ? 'starting' : 'idle');
  const activeIdx = stageIndex(stage);
  const failed = stage === 'failed';
  const ready = stage === 'ready' || status?.phase === 'running';
  const logs = status?.logs?.length ? status.logs : startResponse?.logs || [];

  useEffect(() => {
    const el = logRef.current;
    if (el) el.scrollTop = el.scrollHeight;
  }, [logs.length, status?.message]);

  if (!open) return null;

  return (
    <div className="runpod-modal-backdrop" role="presentation" onClick={onClose}>
      <div
        className="runpod-modal"
        role="dialog"
        aria-labelledby="runpod-modal-title"
        aria-modal="true"
        onClick={(e) => e.stopPropagation()}
      >
        <header className="runpod-modal-header">
          <h2 id="runpod-modal-title">RunPod start</h2>
          <button type="button" className="runpod-modal-close" onClick={onClose} aria-label="Close">
            ×
          </button>
        </header>

        <ol className="runpod-stage-steps" aria-label="Startup progress">
          {STAGES.map((step, idx) => {
            const done = activeIdx > idx || (ready && step.id === 'ready');
            const active = activeIdx === idx && !failed && !ready;
            return (
              <li
                key={step.id}
                className={[
                  'runpod-stage-step',
                  done ? 'runpod-stage-done' : '',
                  active ? 'runpod-stage-active' : '',
                ].filter(Boolean).join(' ')}
              >
                <span className="runpod-stage-dot" aria-hidden />
                <span>{step.label}</span>
              </li>
            );
          })}
        </ol>

        <p className={`runpod-modal-status ${failed ? 'runpod-modal-status-error' : ''}`}>
          {failed
            ? (status?.message || 'Start failed')
            : ready
              ? 'RunPod is ready for AI queries.'
              : (status?.message || startResponse?.message || (busy ? 'Contacting RunPod API…' : 'Waiting…'))}
        </p>

        {status?.gpuAttempt ? (
          <p className="runpod-modal-meta">GPU: {status.gpuAttempt}{status.podStatus ? ` · pod ${status.podStatus}` : ''}</p>
        ) : null}

        <div className="runpod-log-panel" ref={logRef}>
          {startResponse && (
            <div className="runpod-log-line runpod-log-info">
              <span className="runpod-log-time">API</span>
              <span className="runpod-log-level">start</span>
              <span className="runpod-log-message">{startResponse.message || '202 Accepted'}</span>
            </div>
          )}
          {logs.length === 0 && busy ? (
            <div className="runpod-log-line runpod-log-info">
              <span className="runpod-log-message">Waiting for API responses…</span>
            </div>
          ) : null}
          {logs.map((entry, i) => (
            <LogLine key={`${entry.at}-${i}`} entry={entry} />
          ))}
        </div>

        <footer className="runpod-modal-footer">
          {ready ? (
            <button type="button" onClick={onClose}>Close</button>
          ) : failed ? (
            <button type="button" onClick={onClose}>Dismiss</button>
          ) : (
            <button type="button" onClick={onClose} disabled={busy && !failed}>
              {busy ? 'Run in background' : 'Close'}
            </button>
          )}
        </footer>
      </div>
    </div>
  );
}

import type { RunPodStatus } from '../api/client';

const PHASE_CLASS: Record<string, string> = {
  stopped: 'runpod-dot-stopped',
  preparing: 'runpod-dot-preparing',
  running: 'runpod-dot-running',
};

export function RunPodControls({
  status,
  busy,
  onStart,
  onStop,
}: {
  status: RunPodStatus | null;
  busy: boolean;
  onStart: () => void;
  onStop: () => void;
}) {
  const phase = status?.phase || 'stopped';
  const ready = phase === 'running' && !!status?.ollamaReady;
  const attached = !!status?.podId;
  const preparing = phase === 'preparing' || (attached && !ready);
  const dotClass = ready ? PHASE_CLASS.running : (preparing ? PHASE_CLASS.preparing : PHASE_CLASS.stopped);
  const stageLabel = status?.stage === 'warming'
    ? 'Warming up'
    : status?.stage === 'starting'
      ? 'Starting up'
      : status?.stage === 'attached'
        ? 'Pod found'
      : ready
        ? 'Running'
        : attached
          ? 'Pod found'
          : 'Not running';

  return (
    <div className="runpod-controls">
      <span className={`runpod-dot ${dotClass}`} title={status?.message || phase} aria-hidden />
      <span className="runpod-status-text">
        {stageLabel}
        {status?.podId ? ` · ${status.podId}` : ''}
        {status?.model ? ` · ${status.model}` : ''}
      </span>
      <div className="runpod-actions">
        <button type="button" disabled={busy || ready} onClick={onStart}>
          {attached && !ready ? 'Connect RunPod' : 'Start RunPod'}
        </button>
        <button type="button" disabled={busy || !attached} onClick={onStop}>
          Stop RunPod
        </button>
      </div>
    </div>
  );
}

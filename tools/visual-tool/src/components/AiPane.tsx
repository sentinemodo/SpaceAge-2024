import type { RunPodStatus } from '../api/client';
import { RunPodControls } from './RunPodControls';

export type AiViewMode = 'query' | 'output';

export function AiPane({
  heading,
  aiMode,
  onAiModeChange,
  includeStory,
  onIncludeStoryChange,
  promptText,
  onPromptChange,
  outputText,
  runpodStatus,
  runpodBusy,
  queryBusy,
  onStartRunpod,
  onStopRunpod,
  onSubmit,
  submitLabel = 'Submit prompt',
}: {
  heading?: string;
  aiMode: AiViewMode;
  onAiModeChange: (mode: AiViewMode) => void;
  includeStory: boolean;
  onIncludeStoryChange: (v: boolean) => void;
  promptText: string;
  onPromptChange: (v: string) => void;
  outputText: string;
  runpodStatus: RunPodStatus | null;
  runpodBusy: boolean;
  queryBusy: boolean;
  onStartRunpod: () => void;
  onStopRunpod: () => void;
  onSubmit: () => void;
  submitLabel?: string;
}) {
  const readOnly = aiMode === 'output';
  const displayText = aiMode === 'output' ? outputText : promptText;

  return (
    <div className="order-split-col order-split-ai ai-pane">
      <div className="order-split-heading-row">
        <h4 className="order-split-heading">{heading || 'AI'}</h4>
        <div className="order-mode-tabs">
          <button
            type="button"
            className={aiMode === 'query' ? 'active' : ''}
            onClick={() => onAiModeChange('query')}
          >
            Query
          </button>
          <button
            type="button"
            className={aiMode === 'output' ? 'active' : ''}
            onClick={() => onAiModeChange('output')}
          >
            Output
          </button>
        </div>
      </div>
      <label className="ai-include-story">
        <input
          type="checkbox"
          checked={includeStory}
          onChange={(e) => onIncludeStoryChange(e.target.checked)}
        />
        Include story
      </label>
      <textarea
        className={`ai-prompt-textarea ${readOnly ? 'order-text-readonly' : ''}`}
        value={displayText}
        readOnly={readOnly}
        onChange={(e) => {
          if (!readOnly) onPromptChange(e.target.value);
        }}
        placeholder="Ask the campaign advisor…"
      />
      <RunPodControls
        status={runpodStatus}
        busy={runpodBusy}
        onStart={onStartRunpod}
        onStop={onStopRunpod}
      />
      <div className="order-split-actions">
        <button type="button" disabled={queryBusy || !promptText.trim()} onClick={onSubmit}>
          {queryBusy ? 'Querying…' : submitLabel}
        </button>
      </div>
    </div>
  );
}

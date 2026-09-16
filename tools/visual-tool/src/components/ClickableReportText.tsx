import type { ReactNode } from 'react';

const ID_IN_BRACKETS = /\[([A-Za-z0-9]+)\]/g;

export function ClickableReportText({
  text,
  onFocusId,
  className = 'report-section',
}: {
  text: string;
  onFocusId?: (id: string) => void;
  className?: string;
}) {
  if (!onFocusId) {
    return <pre className={className}>{text}</pre>;
  }

  const parts: ReactNode[] = [];
  let last = 0;
  let m: RegExpExecArray | null;
  const re = new RegExp(ID_IN_BRACKETS.source, 'g');
  while ((m = re.exec(text)) !== null) {
    if (m.index > last) parts.push(text.slice(last, m.index));
    const id = m[1];
    parts.push(
      <button
        key={`${m.index}-${id}`}
        type="button"
        className="clickable-id"
        onClick={() => onFocusId(id)}
      >
        [{id}]
      </button>
    );
    last = m.index + m[0].length;
  }
  if (last < text.length) parts.push(text.slice(last));

  return <pre className={className}>{parts}</pre>;
}

/** Inline clickable ids within a string (for list items). */
export function ClickableInline({
  text,
  onFocusId,
}: {
  text: string;
  onFocusId?: (id: string) => void;
}) {
  if (!onFocusId) return <>{text}</>;
  const parts: ReactNode[] = [];
  let last = 0;
  let m: RegExpExecArray | null;
  const re = new RegExp(ID_IN_BRACKETS.source, 'g');
  while ((m = re.exec(text)) !== null) {
    if (m.index > last) parts.push(text.slice(last, m.index));
    const id = m[1];
    parts.push(
      <button key={`${m.index}-${id}`} type="button" className="clickable-id" onClick={() => onFocusId(id)}>
        [{id}]
      </button>
    );
    last = m.index + m[0].length;
  }
  if (last < text.length) parts.push(text.slice(last));
  return <>{parts}</>;
}

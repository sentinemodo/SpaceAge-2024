import { useCallback, useEffect, useRef } from 'react';

export function ResizeHandle({
  direction,
  onDelta,
  className = '',
}: {
  direction: 'horizontal' | 'vertical';
  onDelta: (delta: number) => void;
  className?: string;
}) {
  const dragging = useRef(false);
  const last = useRef(0);

  const onMouseDown = useCallback((e: React.MouseEvent) => {
    e.preventDefault();
    dragging.current = true;
    last.current = direction === 'horizontal' ? e.clientX : e.clientY;
  }, [direction]);

  useEffect(() => {
    function onMove(e: MouseEvent) {
      if (!dragging.current) return;
      const pos = direction === 'horizontal' ? e.clientX : e.clientY;
      const delta = pos - last.current;
      last.current = pos;
      onDelta(delta);
    }
    function onUp() {
      dragging.current = false;
    }
    window.addEventListener('mousemove', onMove);
    window.addEventListener('mouseup', onUp);
    return () => {
      window.removeEventListener('mousemove', onMove);
      window.removeEventListener('mouseup', onUp);
    };
  }, [direction, onDelta]);

  return (
    <div
      className={`resize-handle resize-handle-${direction} ${className}`}
      onMouseDown={onMouseDown}
      role="separator"
      aria-orientation={direction === 'horizontal' ? 'vertical' : 'horizontal'}
    />
  );
}

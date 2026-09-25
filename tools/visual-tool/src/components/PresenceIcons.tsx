import type { PresenceIcon } from '../lib/presenceIcons';

export function PresenceIcons({ icons }: { icons: PresenceIcon[] }) {
  if (icons.length === 0) return null;
  return (
    <span className="presence-icons">
      {icons.map((icon) => (
        <span key={icon.kind} className="presence-icon" title={icon.label}>
          {icon.glyph}
        </span>
      ))}
    </span>
  );
}

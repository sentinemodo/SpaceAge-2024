import { PresenceIcons } from './PresenceIcons';
import type { PresenceIcon } from '../lib/presenceIcons';

export function OrbitSelector({
  orbitIds,
  filterOrbitId,
  presenceIds,
  presenceIndex,
  onSelectOrbit,
  vertical,
  label = 'Orbit',
  glyph = '◯',
  labelBelow = true,
}: {
  orbitIds: string[];
  filterOrbitId: string | null;
  presenceIds: Set<string>;
  presenceIndex?: Map<string, PresenceIcon[]>;
  onSelectOrbit: (orbitId: string) => void;
  vertical?: boolean;
  label?: string;
  glyph?: string;
  labelBelow?: boolean;
}) {
  if (orbitIds.length === 0) return null;

  return (
    <div
      className={`body-view-orbit ${vertical ? 'body-view-orbit-vertical' : ''} ${labelBelow ? 'body-view-orbit-label-below' : ''}`}
    >
      {!labelBelow && <span className="body-view-orbit-label">{label}</span>}
      <div className="body-view-orbit-icons">
        {orbitIds.map((orbitId) => (
          <button
            key={orbitId}
            type="button"
            className={`body-icon body-icon-orbit ${filterOrbitId === orbitId ? 'selected' : ''} ${presenceIds.has(orbitId) ? 'has-presence' : ''}`}
            title={`Orbit ${orbitId}`}
            data-route-id={orbitId}
            onClick={() => onSelectOrbit(orbitId)}
          >
            <span className="body-icon-glyph" aria-hidden>{glyph}</span>
            <PresenceIcons icons={presenceIndex?.get(orbitId) || []} />
          </button>
        ))}
      </div>
      {labelBelow && <span className="band-au">{label}</span>}
    </div>
  );
}

import { formatUpkeep, type PersonNode } from '../parsers/reportXml';

export function PersonDescription({ person }: { person: PersonNode }) {
  return (
    <div className="object-description-body">
      <div className="obj-meta">
        <div className="obj-meta-row">
          <span className="obj-meta-label">Kind</span>
          <span className="obj-meta-value">Person</span>
        </div>
        {person.race && (
          <div className="obj-meta-row">
            <span className="obj-meta-label">Race</span>
            <span className="obj-meta-value">{person.race}</span>
          </div>
        )}
        <div className="obj-meta-row">
          <span className="obj-meta-label">Faction</span>
          <span className="obj-meta-value">{person.faction || '—'}</span>
        </div>
        <div className="obj-meta-row">
          <span className="obj-meta-label">Upkeep</span>
          <span className="obj-meta-value">{formatUpkeep(person.upkeep)}</span>
        </div>
      </div>
    </div>
  );
}

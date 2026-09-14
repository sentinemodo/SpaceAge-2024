import { formatUpkeep, type StackNode } from '../parsers/reportXml';

export function UnitDescription({ stack }: { stack: StackNode }) {
  return (
    <div className="object-description-body">
      <div className="obj-meta">
        <div className="obj-meta-row">
          <span className="obj-meta-label">Type</span>
          <span className="obj-meta-value">{stack.type || '—'}</span>
        </div>
        <div className="obj-meta-row">
          <span className="obj-meta-label">Modules</span>
          <span className="obj-meta-value">{stack.moduleCount}</span>
        </div>
        <div className="obj-meta-row">
          <span className="obj-meta-label">Quantity</span>
          <span className="obj-meta-value">{stack.quantity ?? 1}</span>
        </div>
        <div className="obj-meta-row">
          <span className="obj-meta-label">Faction</span>
          <span className="obj-meta-value">{stack.faction || '—'}</span>
        </div>
        {stack.mass && (
          <div className="obj-meta-row">
            <span className="obj-meta-label">Mass</span>
            <span className="obj-meta-value">{stack.mass}</span>
          </div>
        )}
        {stack.hp && (
          <div className="obj-meta-row">
            <span className="obj-meta-label">Hit points</span>
            <span className="obj-meta-value">{stack.hp}</span>
          </div>
        )}
        <div className="obj-meta-row">
          <span className="obj-meta-label">Upkeep</span>
          <span className="obj-meta-value">{formatUpkeep(stack.upkeep)}</span>
        </div>
        {stack.producesEnergy && (
          <div className="obj-meta-row">
            <span className="obj-meta-label">Energy</span>
            <span className="obj-meta-value">produces</span>
          </div>
        )}
        {stack.locationName && (
          <div className="obj-meta-row">
            <span className="obj-meta-label">Location</span>
            <span className="obj-meta-value">{stack.locationName}</span>
          </div>
        )}
      </div>
    </div>
  );
}

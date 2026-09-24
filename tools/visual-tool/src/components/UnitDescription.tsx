import { catalogEntry, humanCatalogEntries } from '../lib/techCatalog';
import { formatUpkeep, type StackNode } from '../parsers/reportXml';
import { ClickableReportText } from './ClickableReportText';

export function UnitDescription({
  stack,
  onFocusId,
}: {
  stack: StackNode;
  onFocusId?: (id: string, before?: string) => void;
}) {
  const moduleEntry = stack.type
    ? catalogEntry(humanCatalogEntries, stack.type, 'module')
    : null;

  return (
    <div className="object-description-body">
      <div className="obj-meta">
        <div className="obj-meta-row">
          <span className="obj-meta-label">Type</span>
          <span className="obj-meta-value">
            {moduleEntry && onFocusId ? (
              <button type="button" className="clickable-id" onClick={() => onFocusId(moduleEntry.id, moduleEntry.name)}>
                {stack.type}
              </button>
            ) : (
              stack.type || '—'
            )}
          </span>
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
      {stack.reportDetail && (
        <div className="obj-block unit-report-detail">
          <ClickableReportText
            text={stack.reportDetail}
            onFocusId={onFocusId}
            className="unit-report-detail-text"
          />
        </div>
      )}
    </div>
  );
}

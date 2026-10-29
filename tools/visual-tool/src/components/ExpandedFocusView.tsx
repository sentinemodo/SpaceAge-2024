import { useMemo, useState } from 'react';
import type { ParsedReport, PersonNode, StackNode } from '../parsers/reportXml';
import { formatUpkeep } from '../parsers/reportXml';
import { aggregateInventory, type InventoryScope } from '../lib/inventory';
import { ClickableReportText } from './ClickableReportText';
import { ObjectDescription } from './ObjectDescription';

type SortKey = 'type' | 'quantity' | 'size' | 'mass';

function fmt(value: number | null | undefined): string {
  if (value == null || Number.isNaN(value)) return '—';
  return Number.isInteger(value) ? value.toLocaleString() : value.toLocaleString(undefined, { maximumFractionDigits: 1 });
}

function unitValue(total: number | null, quantity: number): number | null {
  if (total == null || quantity <= 0) return null;
  return total / quantity;
}

function Meta({ label, value }: { label: string; value?: string | number | null }) {
  if (value == null || value === '') return null;
  return (
    <div className="obj-meta-row">
      <span className="obj-meta-label">{label}</span>
      <span className="obj-meta-value">{value}</span>
    </div>
  );
}

function InventoryTable({ stack }: { stack: StackNode }) {
  const [scope, setScope] = useState<InventoryScope>('subtree');
  const [sortKey, setSortKey] = useState<SortKey>('quantity');
  const [sortAsc, setSortAsc] = useState(false);

  const inventory = useMemo(() => aggregateInventory(stack, scope), [stack, scope]);
  const rows = useMemo(() => {
    const sorted = [...inventory.rows].sort((a, b) => {
      const av = sortKey === 'type' ? a.label.toLowerCase() : (a[sortKey] ?? -1);
      const bv = sortKey === 'type' ? b.label.toLowerCase() : (b[sortKey] ?? -1);
      if (av < bv) return -1;
      if (av > bv) return 1;
      return a.type.localeCompare(b.type);
    });
    return sortAsc ? sorted : sorted.reverse();
  }, [inventory.rows, sortKey, sortAsc]);

  const shareBase = inventory.totals.size > 0 ? inventory.totals.size : inventory.totals.quantity;

  function toggleSort(key: SortKey) {
    if (key === sortKey) setSortAsc((v) => !v);
    else {
      setSortKey(key);
      setSortAsc(key === 'type');
    }
  }

  const header = (key: SortKey, label: string, numeric = true) => (
    <th key={key} className={numeric ? 'num' : undefined} aria-sort={sortKey === key ? (sortAsc ? 'ascending' : 'descending') : undefined}>
      <button type="button" className="inventory-sort" onClick={() => toggleSort(key)}>
        {label}{sortKey === key ? (sortAsc ? ' ▲' : ' ▼') : ''}
      </button>
    </th>
  );

  return (
    <section className="inventory-section" aria-label="Inventory">
      <div className="inventory-heading-row">
        <h4>Inventory</h4>
        <div className="panel-mode-tabs" role="group" aria-label="Inventory scope">
          <button type="button" className={scope === 'direct' ? 'active' : ''} onClick={() => setScope('direct')}>
            Direct only
          </button>
          <button type="button" className={scope === 'subtree' ? 'active' : ''} onClick={() => setScope('subtree')}>
            Including sub-units
          </button>
        </div>
      </div>

      {rows.length === 0 ? (
        <p className="obj-desc">No items held{scope === 'direct' ? ' directly by this unit' : ' in this unit or its sub-units'}.</p>
      ) : (
        <div className="inventory-table-wrap">
          <table className="inventory-table">
            <thead>
              <tr>
                {header('type', 'Item', false)}
                {header('quantity', 'Qty')}
                <th className="num">Unit size</th>
                <th className="num">Unit mass</th>
                {header('size', 'Total size')}
                {header('mass', 'Total mass')}
                <th className="num">Share</th>
                <th className="num">Holders</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((row) => {
                const share = shareBase > 0
                  ? ((inventory.totals.size > 0 ? (row.size ?? 0) : row.quantity) / shareBase) * 100
                  : 0;
                return (
                  <tr key={row.type}>
                    <td>
                      {row.label} <span className="inventory-type-id">[{row.type}]</span>
                    </td>
                    <td className="num">{fmt(row.quantity)}</td>
                    <td className="num">{fmt(unitValue(row.size, row.quantity))}</td>
                    <td className="num">{fmt(unitValue(row.mass, row.quantity))}</td>
                    <td className="num">{fmt(row.size)}</td>
                    <td className="num">{fmt(row.mass)}</td>
                    <td className="num">
                      <span className="inventory-share">
                        <span className="inventory-share-bar" style={{ width: `${(Math.min(100, share) / 100) * 3}rem` }} />
                        <span>{share.toFixed(share >= 10 ? 0 : 1)}%</span>
                      </span>
                    </td>
                    <td className="num">{scope === 'subtree' ? row.holderCount : '—'}</td>
                  </tr>
                );
              })}
            </tbody>
            <tfoot>
              <tr>
                <td>
                  Total — {inventory.totals.types} item type{inventory.totals.types === 1 ? '' : 's'}
                  {scope === 'subtree' && inventory.totals.holders > 0
                    ? ` in ${inventory.totals.holders} holder${inventory.totals.holders === 1 ? '' : 's'}`
                    : ''}
                </td>
                <td className="num">{fmt(inventory.totals.quantity)}</td>
                <td />
                <td />
                <td className="num">{fmt(inventory.totals.size)}</td>
                <td className="num">{fmt(inventory.totals.mass)}</td>
                <td className="num">100%</td>
                <td />
              </tr>
            </tfoot>
          </table>
        </div>
      )}

      {inventory.capacity && inventory.capacity.total > 0 && (
        <div className="inventory-capacity">
          <div className="inventory-capacity-label">
            <span>Capacity (this unit)</span>
            <span>{fmt(inventory.capacity.used)} / {fmt(inventory.capacity.total)} ({Math.round((inventory.capacity.used / inventory.capacity.total) * 100)}%)</span>
          </div>
          <div className="inventory-capacity-track">
            <div
              className="inventory-capacity-fill"
              style={{ width: `${Math.min(100, (inventory.capacity.used / inventory.capacity.total) * 100)}%` }}
            />
          </div>
        </div>
      )}
    </section>
  );
}

export function ExpandedFocusView({
  report,
  selectedStack,
  selectedPerson,
  filterRegionId,
  filterBodyId,
  filterOrbitId,
  filterStar,
  systemViewId,
  onFocusId,
  onRestoreMap,
  onSelectStack,
  onSelectPerson,
}: {
  report: ParsedReport;
  selectedStack: StackNode | null;
  selectedPerson: PersonNode | null;
  filterRegionId: string | null;
  filterBodyId: string | null;
  filterOrbitId: string | null;
  filterStar: boolean;
  systemViewId: string | null;
  onFocusId?: (id: string, before?: string) => void;
  onRestoreMap: () => void;
  onSelectStack: (id: string) => void;
  onSelectPerson: (id: string) => void;
}) {
  const otherDetails = useMemo(() => {
    if (!selectedStack?.reportDetail) return '';
    return selectedStack.reportDetail
      .split('\n')
      .filter((line) => !/^\s*items:/i.test(line))
      .join('\n')
      .trim();
  }, [selectedStack]);

  return (
    <div className="expanded-focus">
      <div className="expanded-focus-toolbar">
        <button type="button" className="system-view-back" onClick={onRestoreMap}>◧ Galaxy map</button>
        <strong>{selectedStack ? 'Unit focus' : selectedPerson ? 'Person focus' : 'Object focus'}</strong>
        <span className="system-view-star">Map minimized</span>
      </div>

      {!selectedStack && (
        <ObjectDescription
          report={report}
          selectedStack={selectedStack}
          selectedPerson={selectedPerson}
          filterRegionId={filterRegionId}
          filterBodyId={filterBodyId}
          filterOrbitId={filterOrbitId}
          filterStar={filterStar}
          systemViewId={systemViewId}
          onFocusId={onFocusId}
        />
      )}

      {selectedStack && (
        <div className="expanded-focus-body">
          <h3 className="expanded-focus-title">
            {selectedStack.name || selectedStack.type || selectedStack.id} <span className="inventory-type-id">[{selectedStack.id}]</span>
          </h3>
          <div className="obj-meta expanded-focus-meta">
            <Meta label="Type" value={selectedStack.type} />
            <Meta label="Modules" value={selectedStack.moduleCount} />
            <Meta label="Quantity" value={selectedStack.quantity ?? 1} />
            <Meta label="Faction" value={selectedStack.faction} />
            <Meta label="Mass" value={selectedStack.mass} />
            <Meta label="Hit points" value={selectedStack.hp} />
            <Meta label="Upkeep" value={formatUpkeep(selectedStack.upkeep)} />
            <Meta label="Location" value={selectedStack.locationName} />
          </div>

          {/* Key by stack so scope/sort reset per unit. */}
          <InventoryTable key={selectedStack.id} stack={selectedStack} />

          {selectedStack.children.length > 0 && (
            <section className="expanded-focus-section">
              <h4>Sub-units</h4>
              <ul className="expanded-focus-list">
                {selectedStack.children.map((child) => (
                  <li key={child.id}>
                    <button type="button" className="clickable-id" onClick={() => onSelectStack(child.id)}>
                      {child.name || child.type || child.id} [{child.id}]
                    </button>
                    {child.type ? ` · ${child.type}` : ''}{(child.quantity ?? 1) > 1 ? ` ×${child.quantity}` : ''}
                  </li>
                ))}
              </ul>
            </section>
          )}

          {selectedStack.persons.length > 0 && (
            <section className="expanded-focus-section">
              <h4>Persons</h4>
              <ul className="expanded-focus-list">
                {selectedStack.persons.map((person) => (
                  <li key={person.id}>
                    <button type="button" className="clickable-id" onClick={() => onSelectPerson(person.id)}>
                      {person.name || person.id} [{person.id}]
                    </button>
                  </li>
                ))}
              </ul>
            </section>
          )}

          {otherDetails && (
            <section className="expanded-focus-section">
              <h4>Details</h4>
              <ClickableReportText text={otherDetails} onFocusId={onFocusId} className="unit-report-detail-text" />
            </section>
          )}
        </div>
      )}
    </div>
  );
}

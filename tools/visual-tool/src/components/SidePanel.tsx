import type { ParsedReport, PersonNode, StackNode } from '../parsers/reportXml';
import { ClickableReportText } from './ClickableReportText';
import { ObjectDescription } from './ObjectDescription';
import { ResizeHandle } from './ResizeHandle';
import { UnitTreeView } from './UnitTreeView';

export type SideTab = 'units' | 'events';

export function SidePanel({
  report,
  roots,
  atLocationLevel,
  filterRegionId,
  filterBodyId,
  filterOrbitId,
  filterStar,
  systemViewId,
  selectedStack,
  selectedPerson,
  width,
  onResize,
  onSelectStack,
  onSelectPerson,
  sideTab,
  onSideTab,
  eventsText,
  onFocusId,
}: {
  report: ParsedReport;
  roots: StackNode[];
  atLocationLevel: boolean;
  filterRegionId: string | null;
  filterBodyId: string | null;
  filterOrbitId: string | null;
  filterStar: boolean;
  systemViewId: string | null;
  width: number;
  onResize: (delta: number) => void;
  selectedStack: StackNode | null;
  selectedPerson: PersonNode | null;
  onSelectStack: (id: string) => void;
  onSelectPerson: (id: string) => void;
  sideTab: SideTab;
  onSideTab: (tab: SideTab) => void;
  eventsText: string;
  onFocusId?: (id: string) => void;
}) {
  return (
    <aside className="side-panel" style={{ width }}>
      <ResizeHandle direction="horizontal" onDelta={(d) => onResize(-d)} className="side-panel-resize" />
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
      <div className="panel-tabs">
        <button
          type="button"
          className={sideTab === 'units' ? 'active' : ''}
          onClick={() => onSideTab('units')}
        >
          Units
        </button>
        <button
          type="button"
          className={sideTab === 'events' ? 'active' : ''}
          onClick={() => onSideTab('events')}
        >
          Events
        </button>
      </div>
      <div className="scroll-area side-panel-body">
        {sideTab === 'units' && (
          <UnitTreeView
            roots={roots}
            factionId={report.factionId}
            atLocationLevel={atLocationLevel}
            selectedStackId={selectedStack?.id ?? null}
            selectedPersonId={selectedPerson?.id ?? null}
            onSelectStack={onSelectStack}
            onSelectPerson={onSelectPerson}
          />
        )}
        {sideTab === 'events' && (
          <ClickableReportText
            text={eventsText || 'No events this turn.'}
            onFocusId={onFocusId}
          />
        )}
      </div>
    </aside>
  );
}

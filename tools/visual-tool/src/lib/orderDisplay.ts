import type { StackNode } from '../parsers/reportXml';
import { flattenStacks } from '../parsers/reportXml';
import type { ReportSection } from '../api/client';
import { sectionText } from './reportSections';
import {
  filterOrdersTemplate,
  focusOrdersTemplate,
  ownedFactionOrdersTemplate,
} from './orderTemplate';

export function ordersTemplateText(sections: ReportSection[]): string {
  const fromSection = sectionText(sections, 'orders', '');
  if (fromSection.trim()) return fromSection.trim();
  return '';
}

function withoutOrdersTemplateLabel(text: string): string {
  return text.replace(/^Orders Template:\s*/i, '');
}

/** Latest submitted orders, or the report template when nothing has been submitted. */
export function factionOrdersPane(
  submittedText: string | null | undefined,
  reportOrders: string,
): { ordersText: string; parseOutput: string | null } {
  const report = withoutOrdersTemplateLabel(reportOrders);
  if (submittedText && submittedText.trim()) {
    return { ordersText: withoutOrdersTemplateLabel(submittedText), parseOutput: null };
  }
  return { ordersText: report, parseOutput: 'No orders submitted yet.' };
}

export function focusedStackIds(
  filteredRoots: StackNode[],
  selectedStackId: string | null
): string[] {
  if (selectedStackId) return [selectedStackId];
  return flattenStacks(filteredRoots).map((s) => s.id);
}

export function ordersSummaryForFocus(
  template: string,
  filteredRoots: StackNode[],
  selectedStackId: string | null,
  selectedPersonId: string | null
): string {
  if (selectedPersonId) {
    const personBlock = filterOrdersTemplate(template, {
      personIds: new Set([selectedPersonId]),
      includeHeader: false,
    });
    return personBlock || `#person ${selectedPersonId}\n; (no order in report)`;
  }
  return focusOrdersTemplate(template, focusedStackIds(filteredRoots, selectedStackId));
}

export function preloadFactionOrders(
  template: string,
  stacks: StackNode[],
  factionId: string
): string {
  return ownedFactionOrdersTemplate(template, stacks, factionId);
}


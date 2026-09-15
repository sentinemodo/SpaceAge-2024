import type { StackNode } from '../parsers/reportXml';
import { flattenStacks } from '../parsers/reportXml';
import type { ReportSection } from '../api/client';
import { sectionText } from './reportSections';
import {
  focusOrdersTemplate,
  ownedFactionOrdersTemplate,
} from './orderTemplate';

export function ordersTemplateText(sections: ReportSection[]): string {
  const fromSection = sectionText(sections, 'orders', '');
  if (fromSection.trim()) return fromSection.trim();
  return '';
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
  selectedStackId: string | null
): string {
  return focusOrdersTemplate(template, focusedStackIds(filteredRoots, selectedStackId));
}

export function preloadFactionOrders(
  template: string,
  stacks: StackNode[],
  factionId: string
): string {
  return ownedFactionOrdersTemplate(template, stacks, factionId);
}

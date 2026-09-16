import type { ParsedReport, StackNode } from '../parsers/reportXml';
import { flattenStacks } from '../parsers/reportXml';

export interface BankSummary {
  balance: number | null;
  creditMax: number | null;
  creditRate: number | null;
  depositRate: number | null;
}

export interface MarketOfferGroup {
  locationLabel: string;
  systemId?: string;
  bodyId?: string;
  regionId?: string;
  lines: string[];
}

export function parseBankSummary(text: string): BankSummary {
  const balance = text.match(/Bank account balance:\s*(-?\d+)/i);
  const creditMax = text.match(/Credit line maximum:\s*(-?\d+)/i);
  const creditRate = text.match(/Credit rate:\s*(-?\d+(?:\.\d+)?)\s*%/i);
  const depositRate = text.match(/Deposit rate:\s*(-?\d+(?:\.\d+)?)\s*%/i);
  return {
    balance: balance ? parseInt(balance[1], 10) : null,
    creditMax: creditMax ? parseInt(creditMax[1], 10) : null,
    creditRate: creditRate ? parseFloat(creditRate[1]) : null,
    depositRate: depositRate ? parseFloat(depositRate[1]) : null,
  };
}

export function estimateCashUpkeep(stacks: StackNode[], factionId: string): number {
  let total = 0;
  for (const stack of flattenStacks(stacks)) {
    if (stack.faction !== factionId) continue;
    for (const u of stack.upkeep) {
      if (/cash/i.test(u.type)) total += u.quantity;
    }
  }
  return total;
}

export function estimateInterest(summary: BankSummary): {
  depositIncome: number | null;
  creditCost: number | null;
  net: number | null;
} {
  if (summary.balance == null) return { depositIncome: null, creditCost: null, net: null };
  const depositIncome =
    summary.balance > 0 && summary.depositRate != null
      ? Math.round(summary.balance * summary.depositRate / 100)
      : 0;
  const creditCost =
    summary.balance < 0 && summary.creditRate != null
      ? Math.round(Math.abs(summary.balance) * summary.creditRate / 100)
      : 0;
  return {
    depositIncome: summary.balance > 0 ? depositIncome : null,
    creditCost: summary.balance < 0 ? creditCost : null,
    net: depositIncome - creditCost,
  };
}

/** Parse market offer blocks from galaxy report text grouped by location context. */
export function parseMarketOffers(galaxyText: string, report: ParsedReport): MarketOfferGroup[] {
  if (!galaxyText.trim()) return [];
  const groups: MarketOfferGroup[] = [];
  let currentSystem = '';
  let currentBody = '';
  let currentRegion = '';
  let inMarket = false;
  let marketLines: string[] = [];

  const flush = () => {
    if (!inMarket || marketLines.length === 0) return;
    const locationLabel = [currentSystem, currentBody, currentRegion].filter(Boolean).join(' · ');
    groups.push({
      locationLabel: locationLabel || 'Unknown location',
      systemId: report.systems.find((s) => s.name === currentSystem)?.id,
      bodyId: undefined,
      regionId: undefined,
      lines: [...marketLines],
    });
    marketLines = [];
    inMarket = false;
  };

  for (const rawLine of galaxyText.split('\n')) {
    const line = rawLine.trimEnd();
    const trimmed = line.trim();
    if (!trimmed) continue;

    const systemMatch = trimmed.match(/^\*\s+system\s+.+\[([A-Za-z0-9]+)\]/i);
    if (systemMatch) {
      flush();
      currentSystem = trimmed.replace(/^\*\s+system\s+/i, '').replace(/\s*\[[^\]]+\].*$/, '').trim();
      currentBody = '';
      currentRegion = '';
      continue;
    }
    const bodyMatch = trimmed.match(/^\*\s+.+\[(P\d+)\]/i);
    if (bodyMatch) {
      flush();
      currentBody = trimmed.replace(/^\*\s+/, '').replace(/\s*\[P[^\]]+\].*$/, '').trim();
      currentRegion = '';
      continue;
    }
    const regionMatch = trimmed.match(/^(.+)\[(R\d+)\]\s*\(\d+,\d+\)/);
    if (regionMatch && !trimmed.startsWith('Exits:')) {
      flush();
      currentRegion = regionMatch[1].trim();
      continue;
    }
    if (/^Market report:/i.test(trimmed)) {
      flush();
      inMarket = true;
      marketLines = [trimmed];
      continue;
    }
    if (inMarket) {
      if (/^[+\-]\s/.test(trimmed) || /^\*\s/.test(trimmed) || /^-{3,}/.test(trimmed)) {
        flush();
        continue;
      }
      marketLines.push(trimmed);
    }
  }
  flush();
  return groups;
}

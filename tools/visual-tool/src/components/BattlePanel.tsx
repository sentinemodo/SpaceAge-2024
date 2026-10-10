import { useEffect, useMemo, useState } from 'react';
import type { ReportSection } from '../api/client';
import { ClickableReportText } from './ClickableReportText';
import type { ParsedReport } from '../parsers/reportXml';
import {
  battleSelectLabel,
  battleSummaryText,
  filterBattles,
  hitRatioPercent,
  parseBattlesReport,
  uniqueBattleLocations,
  type ParsedBattle,
} from '../lib/battleReport';
import { extractSectionFromFullText, sectionText } from '../lib/reportSections';

export function BattlePanel({
  sections,
  reportFullText,
  report,
  onFocusId,
  battleSimXml,
  onBattleSimXmlChange,
  battleSimOutput,
  onRunBattleSim,
}: {
  sections: ReportSection[];
  reportFullText: string;
  report: ParsedReport | null;
  onFocusId?: (id: string) => void;
  battleSimXml: string;
  onBattleSimXmlChange: (value: string) => void;
  battleSimOutput: string;
  onRunBattleSim: () => void;
}) {
  const [weekFilter, setWeekFilter] = useState<number | 'all'>('all');
  const [locationFilter, setLocationFilter] = useState<string>('all');
  const [selectedBattleId, setSelectedBattleId] = useState<string>('');
  const [summariesOnly, setSummariesOnly] = useState(true);

  const battlesText = useMemo(() => {
    const fromSection = sectionText(sections, 'battles', '');
    if (fromSection && !fromSection.startsWith('No ')) return fromSection;
    const extracted = extractSectionFromFullText(reportFullText, 'Battles report:');
    return extracted || '';
  }, [sections, reportFullText]);

  const allBattles = useMemo(() => parseBattlesReport(battlesText), [battlesText]);

  const locations = useMemo(() => uniqueBattleLocations(allBattles), [allBattles]);

  const weeks = useMemo(() => {
    const set = new Set(allBattles.map((b) => b.week));
    return [...set].sort((a, b) => a - b);
  }, [allBattles]);

  const filteredBattles = useMemo(
    () =>
      filterBattles(allBattles, {
        week: weekFilter,
        locationKey: locationFilter,
      }),
    [allBattles, weekFilter, locationFilter],
  );

  useEffect(() => {
    if (!filteredBattles.length) {
      setSelectedBattleId('');
      return;
    }
    if (!filteredBattles.some((b) => b.id === selectedBattleId)) {
      setSelectedBattleId(filteredBattles[0].id);
    }
  }, [filteredBattles, selectedBattleId]);

  const selectedBattle: ParsedBattle | undefined = filteredBattles.find((b) => b.id === selectedBattleId);

  const factionStackIds = useMemo(() => {
    if (!report) return new Set<string>();
    return new Set(report.stacks.filter((s) => s.faction === report.factionId).map((s) => s.id));
  }, [report]);

  const stats = selectedBattle?.stats;
  const showDamageFootnote =
    stats != null && stats.totalHits > 0 && stats.combatants.every((c) => c.damageDealt === 0);

  return (
    <div className="sub-panel scroll-area battle-panel">
      <h3>Battles</h3>
      {!allBattles.length ? (
        <p className="obj-desc">No battles this quarter.</p>
      ) : (
        <>
          <div className="battle-filters">
            <label className="battle-filter">
              Location
              <select
                value={locationFilter}
                onChange={(e) => setLocationFilter(e.target.value)}
              >
                <option value="all">All locations</option>
                {locations.map((loc) => (
                  <option key={loc.key} value={loc.key}>
                    {loc.label}
                    {loc.regionId ? ` [${loc.regionId}]` : ''}
                  </option>
                ))}
              </select>
            </label>
            <label className="battle-filter">
              Week
              <select
                value={weekFilter === 'all' ? 'all' : String(weekFilter)}
                onChange={(e) => {
                  const v = e.target.value;
                  setWeekFilter(v === 'all' ? 'all' : parseInt(v, 10));
                }}
              >
                <option value="all">All weeks</option>
                {weeks.map((w) => (
                  <option key={w} value={w}>
                    Week {w}
                  </option>
                ))}
              </select>
            </label>
            <label className="battle-filter battle-filter-grow">
              Battle
              <select
                value={selectedBattleId}
                onChange={(e) => setSelectedBattleId(e.target.value)}
                disabled={!filteredBattles.length}
              >
                {filteredBattles.length === 0 ? (
                  <option value="">No battles match filters</option>
                ) : (
                  filteredBattles.map((b) => (
                    <option key={b.id} value={b.id}>
                      {battleSelectLabel(b)}
                    </option>
                  ))
                )}
              </select>
            </label>
            <label className="battle-filter battle-filter-check">
              <input
                type="checkbox"
                checked={summariesOnly}
                onChange={(e) => setSummariesOnly(e.target.checked)}
              />
              Summaries only
            </label>
          </div>

          {report && selectedBattle && (
            <p className="battle-meta">
              Turn {report.turn} — {report.factionName} [{report.factionId}]
              {' · '}
              {filteredBattles.length} battle(s) shown
            </p>
          )}

          {selectedBattle && stats && (
            <section className="battle-section">
              <h4>Statistics</h4>
              <p className="obj-desc battle-stats-note">
                Loot lists only items explicitly reported (e.g. fauna wreckage). Scavenge cargo may be omitted from
                reports.
              </p>
              <div className="battle-summary-cards">
                <div className="bank-summary-card">
                  <strong>Hit rate</strong>
                  <span>
                    {stats.totalHits} hits / {stats.totalMisses} misses (
                    {hitRatioPercent(stats.totalHits, stats.totalMisses)})
                  </span>
                </div>
                <div className="bank-summary-card">
                  <strong>Captures</strong>
                  <span>{stats.captures.length}</span>
                </div>
                <div className="bank-summary-card">
                  <strong>Destroyed / wrecked</strong>
                  <span>
                    {stats.destroyed.length} / {stats.wrecked.length}
                  </span>
                </div>
                <div className="bank-summary-card">
                  <strong>Reported loot</strong>
                  <span>{stats.loot.length}</span>
                </div>
              </div>

              {showDamageFootnote && (
                <p className="obj-desc battle-damage-footnote">
                  Hits recorded but HP damage totals are missing (legacy report lines without &quot;doing N
                  damage&quot;).
                </p>
              )}

              <h5 className="battle-subheading">Damage by combatant</h5>
              <div className="movement-table-wrap">
                <table className="movement-table battle-stats-table">
                  <thead>
                    <tr>
                      <th>Unit</th>
                      <th>Dealt</th>
                      <th>Received</th>
                      <th>Hits</th>
                      <th>Misses</th>
                      <th>Hit %</th>
                    </tr>
                  </thead>
                  <tbody>
                    {stats.combatants.length === 0 ? (
                      <tr>
                        <td colSpan={6} className="movement-empty">
                          No shot lines parsed.
                        </td>
                      </tr>
                    ) : (
                      stats.combatants.map((row) => {
                        const own = factionStackIds.has(row.id);
                        return (
                          <tr key={row.id} className={own ? 'battle-own-faction' : undefined}>
                            <td>
                              <strong>{row.label}</strong>
                              <br />
                              <span className="movement-muted">[{row.id}]</span>
                            </td>
                            <td>{row.damageDealt}</td>
                            <td>{row.damageReceived}</td>
                            <td>{row.hits}</td>
                            <td>{row.misses}</td>
                            <td>{hitRatioPercent(row.hits, row.misses)}</td>
                          </tr>
                        );
                      })
                    )}
                  </tbody>
                </table>
              </div>

              {stats.captures.length > 0 && (
                <>
                  <h5 className="battle-subheading">Captures</h5>
                  <ul className="battle-list">
                    {stats.captures.map((c, i) => (
                      <li key={`${c.subject}-${i}`}>
                        {c.subject} → {c.captor}
                      </li>
                    ))}
                  </ul>
                </>
              )}

              {stats.loot.length > 0 && (
                <>
                  <h5 className="battle-subheading">Loot</h5>
                  <ul className="battle-list">
                    {stats.loot.map((line, i) => (
                      <li key={i}>{line}</li>
                    ))}
                  </ul>
                </>
              )}

              {(stats.destroyed.length > 0 || stats.wrecked.length > 0) && (
                <>
                  <h5 className="battle-subheading">Losses</h5>
                  <ul className="battle-list">
                    {stats.destroyed.map((d, i) => (
                      <li key={`d-${i}`}>{d} — destroyed</li>
                    ))}
                    {stats.wrecked.map((w, i) => (
                      <li key={`w-${i}`}>{w} — wrecked</li>
                    ))}
                  </ul>
                </>
              )}
            </section>
          )}

          {selectedBattle && (
            <section className="battle-section">
              <h4>{summariesOnly ? 'Summary' : 'Battle log'}</h4>
              <ClickableReportText
                text={
                  summariesOnly
                    ? battleSummaryText(selectedBattle, selectedBattle.stats)
                    : selectedBattle.bodyText
                }
                onFocusId={onFocusId}
              />
            </section>
          )}
        </>
      )}

      <section className="battle-section battle-simulator">
        <h4>Battle simulator</h4>
        <textarea
          value={battleSimXml}
          onChange={(e) => onBattleSimXmlChange(e.target.value)}
          placeholder={'Paste <battle-sim> XML…'}
          className="battle-sim-input"
        />
        <button type="button" onClick={onRunBattleSim}>
          Run simulation
        </button>
        {battleSimOutput && <pre className="report-section battle-sim-output">{battleSimOutput}</pre>}
      </section>
    </div>
  );
}

import { useMemo, useState } from 'react';
import type { ParsedReport } from '../parsers/reportXml';
import { movementRowsFromOrders } from '../lib/movementReport';
import { durationWeeks, massFactor, parseMassLine } from '../lib/transit';

export function MovementPanel({ report, ordersText }: { report: ParsedReport; ordersText: string }) {
  const rows = useMemo(() => movementRowsFromOrders(report, ordersText), [report, ordersText]);

  const [paste, setPaste] = useState('');
  const [thrust, setThrust] = useState('40000');
  const [mass, setMass] = useState('4150');
  const [au1, setAu1] = useState('1');
  const [au2, setAu2] = useState('80');
  const [speed, setSpeed] = useState('1');
  const [etaResult, setEtaResult] = useState('');

  function handlePasteInput(text: string) {
    setPaste(text);
    const parsed = parseMassLine(text);
    if (parsed) {
      setThrust(String(parsed.thrust));
      setMass(String(parsed.mass));
    }
  }

  function handleCalcEta() {
    const delta = Math.abs(parseFloat(au2) - parseFloat(au1));
    const t = parseInt(thrust, 10);
    const m = parseInt(mass, 10);
    const sp = parseFloat(speed);
    const weeks = durationWeeks(delta, sp, t, m);
    const mf = massFactor(t, m);
    setEtaResult(
      `${weeks} weeks (ΔAU=${delta.toFixed(2)}, mass factor=${mf.toFixed(2)}, effective speed=${(sp * mf).toFixed(2)})`,
    );
  }

  return (
    <div className="sub-panel scroll-area movement-panel">
      <h3>Movement</h3>
      <p className="obj-desc">
        Module stacks that have a <code>MOVE</code> order in the current orders, including orders entered on the star
        map. Planning aid only — the next processed turn is authoritative.
      </p>

      <section className="movement-section">
        <h4>Unit movement</h4>
        <p className="movement-meta">
          Turn {report.turn} — {report.factionName} [{report.factionId}]
          {' · '}
          {rows.length} module stack(s) with MOVE orders
        </p>
        <div className="movement-table-wrap">
          <table className="movement-table">
            <thead>
              <tr>
                <th>Unit</th>
                <th>Location</th>
                <th>Mass</th>
                <th>MOVE route</th>
                <th>Repeat</th>
              </tr>
            </thead>
            <tbody>
              {rows.length === 0 ? (
                <tr>
                  <td colSpan={5} className="movement-empty">
                    No module stacks with a MOVE order.
                  </td>
                </tr>
              ) : (
                rows.map((row) => (
                  <tr key={row.id} className="has-move">
                    <td>
                      <strong>{row.name}</strong>
                      <br />
                      <span className="movement-muted">{row.id}</span>
                    </td>
                    <td>{row.location || '—'}</td>
                    <td>{row.mass || '—'}</td>
                    <td>{row.route}</td>
                    <td>{row.repeat || '—'}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </section>

      <section className="movement-section movement-eta">
        <h4>Transit ETA simulator</h4>
        <p className="obj-desc">Paste a ship excerpt or enter thrust/mass and two AU-from-star radii.</p>
        <label className="movement-field">
          Paste ship report excerpt (optional)
          <textarea rows={3} value={paste} onChange={(e) => handlePasteInput(e.target.value)} />
        </label>
        <div className="movement-eta-fields">
          <label className="movement-field">
            Thrust
            <input type="number" value={thrust} onChange={(e) => setThrust(e.target.value)} />
          </label>
          <label className="movement-field">
            Mass
            <input type="number" value={mass} onChange={(e) => setMass(e.target.value)} />
          </label>
          <label className="movement-field">
            AU from star (origin)
            <input type="number" value={au1} onChange={(e) => setAu1(e.target.value)} step="0.01" />
          </label>
          <label className="movement-field">
            AU from star (destination)
            <input type="number" value={au2} onChange={(e) => setAu2(e.target.value)} step="0.01" />
          </label>
          <label className="movement-field">
            Drive speed
            <input type="number" value={speed} onChange={(e) => setSpeed(e.target.value)} step="0.1" />
          </label>
          <label className="movement-field">
            ETA
            <input className="movement-eta-result" type="text" readOnly value={etaResult} />
          </label>
        </div>
        <button type="button" onClick={handleCalcEta}>
          Calculate ETA
        </button>
      </section>
    </div>
  );
}

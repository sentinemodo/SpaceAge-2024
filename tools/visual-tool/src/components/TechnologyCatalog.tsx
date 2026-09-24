import { useEffect, useMemo, useState, type ReactNode } from 'react';

import {
  catalogAnchor,
  humanCatalogBlocks,
  humanCatalogEntries,
  resolveCatalogLink,
  visibleCatalogBlocks,
  type CatalogDocumentBlock,
  type CatalogEntry,
} from '../lib/techCatalog';

function renderInline(text: string, entries: CatalogEntry[]): ReactNode[] {
  const parts: ReactNode[] = [];
  const re = /\[([a-z0-9]+)\]|\*\*(.+?)\*\*/gi;
  let last = 0;
  let m: RegExpExecArray | null;
  while ((m = re.exec(text)) !== null) {
    if (m.index > last) parts.push(text.slice(last, m.index));
    if (m[2] != null) {
      parts.push(<strong key={`b-${m.index}`}>{m[2]}</strong>);
    } else {
      const id = m[1].toLowerCase();
      const hit = resolveCatalogLink(text.slice(0, m.index), id, entries);
      if (hit) {
        const anchor = catalogAnchor(hit.kind, hit.id);
        parts.push(
          <a
            key={`${m.index}-${anchor}`}
            className="tech-crosslink"
            href={`#${anchor}`}
            onClick={(event) => {
              event.preventDefault();
              document.getElementById(anchor)?.scrollIntoView({ block: 'start' });
            }}
          >
            [{m[1]}]
          </a>
        );
      } else {
        parts.push(`[${m[1]}]`);
      }
    }
    last = m.index + m[0].length;
  }
  if (last < text.length) parts.push(text.slice(last));
  return parts;
}

function renderTable(rows: string[][], key: string, entries: CatalogEntry[]): ReactNode {
  const [head, ...body] = rows;
  return (
    <div key={key} className="tech-table-wrap">
      <table className="tech-table">
        <thead>
          <tr>
            {head.map((cell, index) => (
              <th key={index}>{renderInline(cell, entries)}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {body.map((row, rowIndex) => (
            <tr key={rowIndex}>
              {row.map((cell, index) => (
                <td key={index}>{renderInline(cell, entries)}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function renderBlock(block: CatalogDocumentBlock, index: number, entries: CatalogEntry[]): ReactNode {
  if (block.type === 'title') {
    return (
      <h1 key={`title-${index}`} className="tech-title">
        {block.lines[0]}
      </h1>
    );
  }
  if (block.type === 'section') {
    const slug = block.lines[0].toLowerCase().replace(/[^a-z0-9]+/g, '-');
    return (
      <h2 key={`section-${index}`} id={`tech-${slug}`} className="tech-section">
        {block.lines[0]}
      </h2>
    );
  }
  if (block.type === 'subsection') {
    return (
      <h3 key={`sub-${index}`} className="tech-subsection">
        {block.lines[0]}
      </h3>
    );
  }
  if (block.type === 'table' && block.table) {
    return renderTable(block.table, `table-${index}`, entries);
  }
  if (block.type === 'entry' && block.kind && block.id) {
    return (
      <article key={`${block.kind}-${block.id}`} className="tech-entry">
        <h3 id={catalogAnchor(block.kind, block.id)} className="tech-entry-title">
          {block.name} <span className="tech-id">[{block.id}]</span>
        </h3>
        {block.lines.map((line, lineIndex) => (
          <p key={lineIndex} className="tech-paragraph">
            {renderInline(line, entries)}
          </p>
        ))}
      </article>
    );
  }
  return (
    <p key={`prose-${index}`} className="tech-paragraph">
      {renderInline(block.lines[0] ?? '', entries)}
    </p>
  );
}

function UseCalculator() {
  const technologies = humanCatalogEntries.filter((entry) => entry.kind === 'tech');
  const modules = humanCatalogEntries.filter((entry) => entry.kind === 'module');
  return (
    <form className="use-calculator" onSubmit={(event) => event.preventDefault()}>
      <h4>Use calculator</h4>
      <div className="use-calculator-inputs">
        <label>
          Technology
          <select name="technology" defaultValue="">
            <option value="">Choose a technology</option>
            {technologies.map((entry) => (
              <option key={entry.id} value={entry.id}>
                {entry.name} [{entry.id}]
              </option>
            ))}
          </select>
        </label>
        <label>
          Producing module
          <select name="module" defaultValue="">
            <option value="">Choose a module type</option>
            {modules.map((entry) => (
              <option key={entry.id} value={entry.id}>
                {entry.name} [{entry.id}]
              </option>
            ))}
          </select>
        </label>
        <label>
          Modules
          <input name="count" type="number" min={1} defaultValue={1} />
        </label>
      </div>
      <dl className="use-calculator-results">
        <div>
          <dt>Estimated consumption</dt>
          <dd>—</dd>
        </div>
        <div>
          <dt>Estimated output</dt>
          <dd>—</dd>
        </div>
        <div>
          <dt>Estimated use time</dt>
          <dd>—</dd>
        </div>
      </dl>
    </form>
  );
}

export function TechnologyCatalog({
  focusAnchor,
  focusNonce,
}: {
  focusAnchor?: string | null;
  focusNonce?: number;
}) {
  const [query, setQuery] = useState('');
  const [level, setLevel] = useState('');
  const [tag, setTag] = useState('');

  const levels = useMemo(() => {
    const found = new Set<number>();
    for (const block of humanCatalogBlocks) {
      if (block.type === 'entry' && block.level != null) found.add(block.level);
    }
    return [...found].sort((a, b) => a - b);
  }, []);

  const tags = useMemo(() => {
    const found = new Set<string>();
    for (const block of humanCatalogBlocks) {
      if (block.type === 'entry' && block.kind === 'tech') {
        for (const item of block.tags) found.add(item);
      }
    }
    return [...found].sort();
  }, []);

  const visible = useMemo(
    () =>
      visibleCatalogBlocks(humanCatalogBlocks, {
        query,
        level: level === '' ? null : Number(level),
        tag: tag === '' ? null : tag,
      }),
    [query, level, tag]
  );

  const hasEntry = visible.some((block) => block.type === 'entry');

  useEffect(() => {
    if (!focusAnchor) return;
    document.getElementById(focusAnchor)?.scrollIntoView({ block: 'start' });
  }, [focusAnchor, focusNonce]);

  return (
    <div className="tech-workspace">
      <div className="tech-filters">
        <label>
          Search
          <input
            type="search"
            value={query}
            placeholder="Description text"
            onChange={(event) => setQuery(event.target.value)}
          />
        </label>
        <label>
          Level
          <select value={level} onChange={(event) => setLevel(event.target.value)}>
            <option value="">All levels</option>
            {levels.map((item) => (
              <option key={item} value={item}>
                Level {item}
              </option>
            ))}
          </select>
        </label>
        <label>
          Tag
          <select value={tag} onChange={(event) => setTag(event.target.value)}>
            <option value="">All tags</option>
            {tags.map((item) => (
              <option key={item} value={item}>
                {item}
              </option>
            ))}
          </select>
        </label>
      </div>
      <div className="technology-catalog">
        {visible.map((block, index) => renderBlock(block, index, humanCatalogEntries))}
        {!hasEntry && <p className="tech-empty">No technologies or items match.</p>}
      </div>
      <UseCalculator />
    </div>
  );
}

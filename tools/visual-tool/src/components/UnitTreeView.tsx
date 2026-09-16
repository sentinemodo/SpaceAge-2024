import { useMemo, useState } from 'react';
import {
  formatStackLabel,
  formatPersonLabel,
  partitionOwnedStacks,
  filterStacksByQuery,
  sortRootStacks,
  type PersonNode,
  type StackNode,
  type UnitSortKey,
} from '../parsers/reportXml';

function TreeBranch({
  node,
  depth,
  selectedStackId,
  selectedPersonId,
  factionId,
  collapsed,
  onToggle,
  onSelectStack,
  onSelectPerson,
}: {
  node: StackNode;
  depth: number;
  selectedStackId: string | null;
  selectedPersonId: string | null;
  factionId: string;
  collapsed: Set<string>;
  onToggle: (id: string) => void;
  onSelectStack: (id: string) => void;
  onSelectPerson: (id: string) => void;
}) {
  const hasChildren = node.children.length > 0;
  const isCollapsed = collapsed.has(node.id);
  const isOwned = node.faction === factionId;

  return (
    <li className={`tree-branch depth-${depth}`}>
      <div className={`tree-row ${selectedStackId === node.id ? 'selected' : ''} ${isOwned ? 'owned' : 'other-faction'}`}>
        {hasChildren ? (
          <button
            type="button"
            className="tree-toggle"
            aria-label={isCollapsed ? 'Expand' : 'Collapse'}
            onClick={(e) => {
              e.stopPropagation();
              onToggle(node.id);
            }}
          >
            {isCollapsed ? '▸' : '▾'}
          </button>
        ) : (
          <span className="tree-toggle spacer" />
        )}
        <button type="button" className="tree-label" onClick={() => onSelectStack(node.id)}>
          {formatStackLabel(node)}
        </button>
      </div>
      {!isCollapsed && node.persons.length > 0 && (
        <ul className="tree-children tree-persons">
          {node.persons.map((person) => (
            <PersonBranch
              key={person.id}
              person={person}
              depth={depth + 1}
              factionId={factionId}
              selectedPersonId={selectedPersonId}
              onSelectPerson={onSelectPerson}
            />
          ))}
        </ul>
      )}
      {hasChildren && !isCollapsed && (
        <ul className="tree-children">
          {node.children.map((child) => (
            <TreeBranch
              key={child.id}
              node={child}
              depth={depth + 1}
              selectedStackId={selectedStackId}
              selectedPersonId={selectedPersonId}
              factionId={factionId}
              collapsed={collapsed}
              onToggle={onToggle}
              onSelectStack={onSelectStack}
              onSelectPerson={onSelectPerson}
            />
          ))}
        </ul>
      )}
    </li>
  );
}

function PersonBranch({
  person,
  depth,
  factionId,
  selectedPersonId,
  onSelectPerson,
}: {
  person: PersonNode;
  depth: number;
  factionId: string;
  selectedPersonId: string | null;
  onSelectPerson: (id: string) => void;
}) {
  const isOwned = person.faction === factionId;
  return (
    <li className={`tree-branch tree-person depth-${depth}`}>
      <div className={`tree-row ${selectedPersonId === person.id ? 'selected' : ''} ${isOwned ? 'owned' : 'other-faction'}`}>
        <span className="tree-toggle spacer" />
        <button type="button" className="tree-label tree-label-person" onClick={() => onSelectPerson(person.id)}>
          {formatPersonLabel(person)}
        </button>
      </div>
    </li>
  );
}

function LocationGroup({
  locationName,
  stacks,
  factionId,
  selectedStackId,
  selectedPersonId,
  collapsedLocations,
  collapsedStacks,
  onToggleLocation,
  onToggleStack,
  onSelectStack,
  onSelectPerson,
}: {
  locationName: string;
  stacks: StackNode[];
  factionId: string;
  selectedStackId: string | null;
  selectedPersonId: string | null;
  collapsedLocations: Set<string>;
  collapsedStacks: Set<string>;
  onToggleLocation: (id: string) => void;
  onToggleStack: (id: string) => void;
  onSelectStack: (id: string) => void;
  onSelectPerson: (id: string) => void;
}) {
  const locKey = `loc:${locationName}`;
  const locCollapsed = collapsedLocations.has(locKey);

  return (
    <li className="tree-location-group">
      <div className="tree-location-header">
        <button type="button" className="tree-toggle" onClick={() => onToggleLocation(locKey)}>
          {locCollapsed ? '▸' : '▾'}
        </button>
        <span className="tree-location-name">{locationName}</span>
      </div>
      {!locCollapsed && (
        <ul className="tree-root">
          {stacks.map((s) => (
            <TreeBranch
              key={s.id}
              node={s}
              depth={0}
              selectedStackId={selectedStackId}
              selectedPersonId={selectedPersonId}
              factionId={factionId}
              collapsed={collapsedStacks}
              onToggle={onToggleStack}
              onSelectStack={onSelectStack}
              onSelectPerson={onSelectPerson}
            />
          ))}
        </ul>
      )}
    </li>
  );
}

export function UnitTreeView({
  roots,
  factionId,
  atLocationLevel,
  selectedStackId,
  selectedPersonId,
  onSelectStack,
  onSelectPerson,
}: {
  roots: StackNode[];
  factionId: string;
  atLocationLevel: boolean;
  selectedStackId: string | null;
  selectedPersonId: string | null;
  onSelectStack: (id: string) => void;
  onSelectPerson: (id: string) => void;
}) {
  const [query, setQuery] = useState('');
  const [sortKey, setSortKey] = useState<UnitSortKey>('id');
  const [collapsedLocations, setCollapsedLocations] = useState<Set<string>>(() => new Set());
  const [collapsedStacks, setCollapsedStacks] = useState<Set<string>>(() => new Set());

  const displayRoots = useMemo(() => {
    let list = filterStacksByQuery(roots, query);
    list = sortRootStacks(list, sortKey);
    const { owned, other } = partitionOwnedStacks(list, factionId);
    return [...sortRootStacks(owned, sortKey), ...sortRootStacks(other, sortKey)];
  }, [roots, query, sortKey, factionId]);

  const groups = useMemo(() => {
    if (atLocationLevel) return null;
    const byLoc = new Map<string, { name: string; stacks: StackNode[] }>();
    for (const s of displayRoots) {
      const id = s.locationId || 'unknown';
      const g = byLoc.get(id) || { name: s.locationName || id, stacks: [] };
      g.stacks.push(s);
      byLoc.set(id, g);
    }
    return [...byLoc.entries()].map(([locationId, g]) => ({
      locationId,
      locationName: g.name,
      stacks: g.stacks,
    }));
  }, [displayRoots, atLocationLevel]);

  function toggleLocation(id: string) {
    setCollapsedLocations((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  function toggleStack(id: string) {
    setCollapsedStacks((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  return (
    <div className="unit-tree-panel">
      <div className="unit-tree-toolbar">
        <input
          type="search"
          className="unit-tree-search"
          placeholder="Search units…"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
        />
        <select
          className="unit-tree-sort"
          value={sortKey}
          onChange={(e) => setSortKey(e.target.value as UnitSortKey)}
          aria-label="Sort units"
        >
          <option value="id">Sort: ID</option>
          <option value="type">Sort: type</option>
          <option value="name">Sort: name</option>
        </select>
      </div>
      <ul className="tree-view">
        {atLocationLevel ? (
          displayRoots.map((s) => (
            <TreeBranch
              key={s.id}
              node={s}
              depth={0}
              selectedStackId={selectedStackId}
              selectedPersonId={selectedPersonId}
              factionId={factionId}
              collapsed={collapsedStacks}
              onToggle={toggleStack}
              onSelectStack={onSelectStack}
              onSelectPerson={onSelectPerson}
            />
          ))
        ) : (
          groups?.map((g) => (
            <LocationGroup
              key={g.locationId}
              locationName={g.locationName}
              stacks={g.stacks}
              factionId={factionId}
              selectedStackId={selectedStackId}
              selectedPersonId={selectedPersonId}
              collapsedLocations={collapsedLocations}
              collapsedStacks={collapsedStacks}
              onToggleLocation={toggleLocation}
              onToggleStack={toggleStack}
              onSelectStack={onSelectStack}
              onSelectPerson={onSelectPerson}
            />
          ))
        )}
        {displayRoots.length === 0 && <li className="tree-empty">No units match filter.</li>}
      </ul>
    </div>
  );
}

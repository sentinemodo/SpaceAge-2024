import { describe, it, expect } from 'vitest';
import { filterStacksBySystem, estimateMoveWeeks } from './reportXml';

describe('estimateMoveWeeks', () => {
  it('returns weeks from mass line', () => {
    expect(estimateMoveWeeks('40000/4150')).toBeTypeOf('number');
  });
});

describe('filterStacksBySystem', () => {
  it('filters by system id', () => {
    const stacks = [
      { id: '1', name: 'a', systemId: 'Helios', children: [] },
      { id: '2', name: 'b', systemId: 'Fomal', children: [] },
    ];
    expect(filterStacksBySystem(stacks, 'Helios')).toHaveLength(1);
  });
});

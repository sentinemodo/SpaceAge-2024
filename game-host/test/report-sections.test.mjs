import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { splitReportSections } from '../lib/report-sections.mjs';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const sampleReport = fs.readFileSync(
  path.join(__dirname, '../../Tests/SampleGame/testreport.1.1.txt'),
  'utf8'
);

test('splitReportSections preserves engine text without reformatting', () => {
  const sections = splitReportSections(sampleReport);
  assert.ok(sections.some((s) => s.id === 'galaxy'));
  assert.ok(sections.some((s) => s.id === 'events'));
  assert.ok(sections.some((s) => s.id === 'bank'));
  const galaxy = sections.find((s) => s.id === 'galaxy');
  assert.match(galaxy.text, /Galaxy report:/);
  assert.match(galaxy.text, /system Sol \[SS0001\]/);
});

import { describe, expect, it } from 'vitest';

import { loadFaqMarkdown, renderFaqHtml } from '../src/lib/faq';

describe('faq', () => {
  it('loads markdown with expected sections', () => {
    const md = loadFaqMarkdown();
    expect(md).toMatch(/^# Frequently asked questions/m);
    expect(md).toMatch(/## Scout — what to build for scouting initially/);
    expect(md).toMatch(/## How do I build whatever\? \(Iron, trucks, factory chain\)/);
    expect(md).toMatch(/## Anomaly investigation/);
  });

  it('renders HTML with question headings', () => {
    const html = renderFaqHtml();
    expect(html).toContain('<h2');
    expect(html).toContain('grndtr');
    expect(html).toContain('warehouse');
  });
});

import { test, expect } from '@playwright/test';

const FACTION_ID = process.env.SA_TEST_FACTION || '2';
const FACTION_PASSWORD = process.env.SA_TEST_PASSWORD || 'northwnd';

async function login(page: import('@playwright/test').Page) {
  await page.goto('/client/');
  await page.getByRole('spinbutton', { name: 'Faction (2–11)' }).fill(FACTION_ID);
  await page.getByLabel('Password').fill(FACTION_PASSWORD);
  await page.getByRole('button', { name: /Enter client/i }).click();
  await expect(page.getByRole('button', { name: /Refresh report/i })).toBeVisible({ timeout: 15000 });
}

/** UT-001 — login and browse report shell (map / units) */
test('UT-001 login page loads', async ({ page }) => {
  await page.goto('/client/');
  await expect(page.getByRole('heading', { name: /SpaceAge Client/i })).toBeVisible();
});

test('UT-001 browse report after login', async ({ page }) => {
  await login(page);
  await page.getByRole('button', { name: /Refresh report/i }).click();
  await expect(page.locator('.star-map .system-node').first()).toBeVisible({ timeout: 15000 });
  await expect(page.getByRole('complementary').getByRole('button', { name: 'Units' })).toBeVisible();
  await expect(page.getByRole('navigation', { name: 'Panels' }).getByRole('button', { name: 'Star map' })).toHaveText('✦');
});

/** UT-002 — engine-backed order parse surfaces errors */
test('UT-002 parse orders shows password error', async ({ page }) => {
  await login(page);
  const editor = page.getByPlaceholder(/#faction N/i);
  await expect(editor).toContainText('northwnd', { timeout: 15000 });
  await editor.click();
  await page.keyboard.press('Control+A');
  await editor.fill('#faction 2 "wrongpass"\n#modulestack 200001\nMOVE R99999\n#end\n');
  await expect(editor).toContainText('wrongpass');
  await page.getByRole('button', { name: /Parse orders/i }).click();
  await page.getByRole('button', { name: /Parser output/i }).click();
  await expect(page.locator('.order-parser-output').filter({ hasText: /password|wrongpass|bad command/i }).first()).toBeVisible({
    timeout: 15000,
  });
});

/** UT-003 — star map filter + MOVE route from report XML when present */
test('UT-003 star map filters unit tree', async ({ page }) => {
  await login(page);
  await page.getByRole('button', { name: /Refresh report/i }).click();
  const firstSystem = page.locator('.star-map .system-node').first();
  await expect(firstSystem).toBeVisible({ timeout: 15000 });
  await firstSystem.click();
  await expect(firstSystem).toHaveClass(/selected/);
  await expect(page.locator('.unit-tree-panel .tree-view button').first()).toBeVisible();
});

test('UT-003 system view opens on double-click', async ({ page }) => {
  await login(page);
  await page.getByRole('button', { name: /Refresh report/i }).click();
  const firstSystem = page.locator('.star-map .system-node').first();
  await expect(firstSystem).toBeVisible({ timeout: 15000 });
  await firstSystem.dblclick();
  await expect(page.getByRole('button', { name: /Galaxy map/i })).toBeVisible();
  await expect(page.locator('.system-orbit-map')).toBeVisible();
});

test('UT-003 MOVE route preview when unit has XML move order', async ({ page }) => {
  await login(page);
  await page.getByRole('button', { name: /Refresh report/i }).click();
  const stackButtons = page.locator('.unit-tree-panel .tree-view button:not(.tree-toggle)');
  await expect(stackButtons.first()).toBeVisible({ timeout: 15000 });
  const hq = stackButtons.filter({ hasText: /Headquarters|\[200001\]/i });
  if (await hq.count()) {
    await hq.first().click();
  } else {
    await stackButtons.first().click();
  }
  const route = page.getByText(/^MOVE route:/);
  if (await route.isVisible().catch(() => false)) {
    await expect(route).toContainText('→');
    return;
  }
  test.info().annotations.push({
    type: 'note',
    description: 'No XML MOVE orders in current report; unit selection verified',
  });
});

/** UT-004 — mobile-friendly chrome */
test('UT-004 mobile layout shows side panel', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await login(page);
  await expect(page.locator('.side-panel')).toBeVisible();
  await expect(page.locator('.icon-rail')).toBeVisible();
});

/** UT-005 — static bundle leak bar */
test('UT-005 static bundle has no gamein paths', async ({ page }) => {
  await page.goto('/client/');
  const html = await page.content();
  expect(html).not.toMatch(/gamein\.xml/);
  expect(html).not.toMatch(/order\.\d+\.txt/);
});

/** UT-006 — regional map shows adjacent cells from exit target stubs */
test('UT-006 regional map renders exit-only neighbour cells', async ({ page }) => {
  await login(page);
  await page.getByRole('button', { name: /Refresh report/i }).click();
  const presenceSystem = page.locator('.star-map .system-node.has-presence').first();
  await expect(presenceSystem).toBeVisible({ timeout: 15000 });
  await presenceSystem.dblclick();
  await expect(page.getByRole('button', { name: /Galaxy map/i })).toBeVisible();

  const bodyWithPresence = page.locator('.body-icon.has-presence').first();
  await expect(bodyWithPresence).toBeVisible({ timeout: 15000 });
  await bodyWithPresence.dblclick();

  const regionCells = page.locator('.region-cell');
  await expect(regionCells.first()).toBeVisible({ timeout: 15000 });
  const count = await regionCells.count();
  expect(count).toBeGreaterThanOrEqual(2);

  const exitOnly = page.locator('.region-cell.exit-only');
  if ((await exitOnly.count()) > 0) {
    await expect(exitOnly.first()).toBeVisible();
  } else {
    test.info().annotations.push({
      type: 'note',
      description: 'All visible regions have presence in this report; exit-only cells require adjacent unvisited neighbours',
    });
  }
});

import { test, expect } from '@playwright/test';

const FORBIDDEN_PATTERNS = [/password/i, /gamein/i, /order\./i, /report\./i];
const ROUTES = ['/', '/client', '/turns', '/rules'] as const;

function assertNoLeaks(body: string): void {
  for (const pattern of FORBIDDEN_PATTERNS) {
    expect(body).not.toMatch(pattern);
  }
}

test.describe('Phase 1 lobby acceptance', () => {
  test('WS-001: Home flavour and required credits', async ({ page }) => {
    await page.goto('/');

    const attribution = page.locator('#attribution');
    await expect(attribution).toBeVisible();
    await expect(attribution.getByText(/Atlantis/i)).toBeVisible();
    await expect(attribution.getByText(/Rise of Heroes/i)).toBeVisible();
    await expect(attribution.getByText(/Vincent Archer/i)).toBeVisible();
    await expect(page.getByRole('link', { name: /Overlord PBEM engine/i })).toHaveAttribute(
      'href',
      'https://overlord.sourceforge.net/',
    );

    await expect(page.getByText(/750 light-years/i)).toBeVisible();
    await expect(page.getByText(/weak points/i)).toBeVisible();
    await expect(page.getByText(/Alderson Drive/i)).toBeVisible();
    await expect(page.getByText(/shut down/i)).toBeVisible();
    await expect(page.getByText(/Two Points that led to Earth stayed dark/i)).toBeVisible();
  });

  test('WS-002: Closed lobby (no join / signup)', async ({ page }) => {
    await page.goto('/');

    await expect(page.getByText(/invitation-only/i)).toBeVisible();
    await expect(page.getByRole('link', { name: /Open the Client/i })).toBeVisible();
    await expect(page.getByText(/Join Game Now/i)).toHaveCount(0);
    await expect(page.locator('input[type="password"]')).toHaveCount(0);
    await expect(page.locator('form')).toHaveCount(0);
  });

  test('WS-003: Status dashboard from /status.json', async ({ page, request }) => {
    const jsonResponse = await request.get('/status.json');
    expect(jsonResponse.status()).toBe(200);
    const json = await jsonResponse.json();
    assertNoLeaks(JSON.stringify(json));

    await page.goto('/');
    await expect(page.locator('#status-value')).toHaveText(/Not Started/i);
    await expect(page.locator('#turn-value')).toHaveText(String(json.turn));
    await expect(page.locator('#next-turn-value')).toHaveText(/GM-scheduled/i);
    await expect(page.locator('#submissions-grid .faction-card')).toHaveCount(10);

    await page.goto('/turns');
    await expect(page.locator('#turns-status-value')).toHaveText(/Not Started/i);
    await expect(page.locator('#turns-turn-value')).toHaveText(String(json.turn));
    await expect(page.locator('#turns-next-value')).toHaveText(/GM-scheduled/i);
  });

  test('WS-004: Players & Turns — ten seats, factions 2–11 only', async ({ page }) => {
    await page.goto('/turns');

    const rows = page.locator('#turns-factions-table .table-row');
    await expect(rows).toHaveCount(10);

    for (let id = 2; id <= 11; id += 1) {
      await expect(page.locator(`[data-faction-id="${id}"]`)).toBeVisible();
    }

    await expect(page.getByText(/Faction 1\b/)).toHaveCount(0);
    await expect(page.getByText(/Faction 12\b/)).toHaveCount(0);
    await expect(page.getByText(/Faction 13\b/)).toHaveCount(0);
  });

  test('WS-005: Game Client placeholder', async ({ page }) => {
    await page.goto('/client');
    const main = page.locator('main');
    await expect(main.getByRole('heading', { name: /Game Client/i })).toBeVisible();
    await expect(main.getByRole('heading', { name: 'Coming Soon' })).toBeVisible();
    await expect(main.getByText(/Visual Tool Launch: Phase 3/i)).toBeVisible();
  });

  test('WS-006: Rules — short principles, not the rulebook', async ({ page }) => {
    await page.goto('/rules');
    const main = page.locator('main');

    await expect(main.getByText(/open PBEM/i)).toBeVisible();
    await expect(main.getByRole('heading', { name: /Interests & Factions/i })).toBeVisible();
    await expect(main.getByText(/factions 2/i)).toBeVisible();
    await expect(main.getByText(/13 weeks/i).first()).toBeVisible();

    const bodyText = await page.locator('main').innerText();
    expect(bodyText.length).toBeLessThan(8000);
  });

  test('WS-007: Mobile nav and cards', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto('/');

    const toggle = page.locator('#nav-toggle');
    await expect(toggle).toBeVisible();

    const menu = page.locator('#nav-menu');
    await expect(menu).not.toHaveClass(/is-open/);

    await toggle.click();
    await expect(menu).toHaveClass(/is-open/);
    await expect(menu.getByRole('link', { name: 'Rules', exact: true })).toBeVisible();

    for (const route of ROUTES) {
      const response = await page.goto(route);
      expect(response?.status()).toBe(200);
    }

    await page.goto('/');
    const dashboardHeader = page.locator('.status-header');
    const box = await dashboardHeader.boundingBox();
    expect(box?.width ?? 0).toBeLessThanOrEqual(390);
  });

  test('WS-008: No secrets on public pages or status JSON', async ({ page, request }) => {
    for (const route of [...ROUTES, '/status.json']) {
      const response = await request.get(route);
      expect(response.status()).toBe(200);
      assertNoLeaks(await response.text());
    }

    await page.goto('/');
    assertNoLeaks(await page.content());
  });

  test('WS-009: Four public routes and shared chrome', async ({ page }) => {
    await page.goto('/');

    const nav = page.locator('nav');
    await nav.getByRole('link', { name: 'Home', exact: true }).click();
    await expect(page).toHaveURL('/');

    await nav.getByRole('link', { name: 'Turns', exact: true }).click();
    await expect(page).toHaveURL('/turns');

    await nav.getByRole('link', { name: 'Rules', exact: true }).click();
    await expect(page).toHaveURL('/rules');

    await nav.getByRole('link', { name: 'Client', exact: true }).click();
    await expect(page).toHaveURL('/client');

    await page.goto('/');
    await expect(page.locator('footer')).toContainText(/0\.1\./);
    await expect(page.locator('footer')).toContainText(/Quarterly schedule/i);
  });
});

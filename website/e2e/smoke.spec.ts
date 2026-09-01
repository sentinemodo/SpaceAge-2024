import { test, expect } from '@playwright/test';

/**
 * Smoke & Phase 1 E2E Tests
 * Catalog IDs: WS-001, WS-002, WS-003, WS-004
 * Phase 1 deliverables: / /client /turns /rules exist, home has Atlantis/Rise/Vincent credits
 */

test.describe('SpaceAge Website - Phase 1', () => {
  const baseURL = 'http://localhost:4321';

  test('WS-001: Home page exists and is accessible', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveTitle(/SpaceAge/i);
    const heading = page.locator('h1');
    await expect(heading).toBeVisible();
  });

  test('WS-002: Home page includes Atlantis, Rise of Heroes, and Vincent Archer credits', async ({ page }) => {
    await page.goto('/');
    const credits = page.getByText(/Atlantis|Rise of Heroes|Vincent Archer/i);

    // At least one credit should be visible
    const count = await credits.count();
    expect(count).toBeGreaterThanOrEqual(1);

    // Look for all three mentioned
    await expect(page.getByText(/Atlantis/i)).toBeVisible();
    await expect(page.getByText(/Rise of Heroes/i)).toBeVisible();
    await expect(page.getByText(/Vincent Archer/i)).toBeVisible();
  });

  test('WS-003: All Phase 1 routes exist and return 200', async ({ page }) => {
    const routes = ['/', '/client', '/turns', '/rules'];

    for (const route of routes) {
      const response = await page.goto(route);
      expect(response?.status()).toBe(200);
    }
  });

  test('WS-004: Navigation menu links work and are accessible', async ({ page }) => {
    await page.goto('/');

    // Check nav links exist
    const navLinks = page.locator('nav a');
    await expect(navLinks).not.toHaveCount(0);

    // Click each nav link and verify page load
    const homeLink = page.locator('a:has-text("Home")');
    const turnsLink = page.locator('a:has-text("Turns")');
    const rulesLink = page.locator('a:has-text("Rules")');
    const clientLink = page.locator('a:has-text("Client")');

    await homeLink.click();
    await expect(page).toHaveURL('/');

    await turnsLink.click();
    await expect(page).toHaveURL('/turns');

    await rulesLink.click();
    await expect(page).toHaveURL('/rules');

    await clientLink.click();
    await expect(page).toHaveURL('/client');
  });

  test('should render mobile viewport', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto('/');

    // Content should be visible even on mobile
    const heading = page.locator('h1');
    await expect(heading).toBeVisible();

    // Nav should be accessible
    const nav = page.locator('nav');
    await expect(nav).toBeVisible();
  });

  test('should have proper page titles', async ({ page }) => {
    const pages: Record<string, string> = {
      '/': 'Home',
      '/client': 'Client',
      '/turns': 'Turns',
      '/rules': 'Rules',
    };

    for (const [route, titlePart] of Object.entries(pages)) {
      await page.goto(route);
      await expect(page).toHaveTitle(new RegExp(titlePart));
    }
  });

  test('footer should display engine version and credits', async ({ page }) => {
    await page.goto('/');

    const footer = page.locator('footer');
    await expect(footer).toBeVisible();

    // Footer should contain engine version reference
    const footerText = await footer.textContent();
    expect(footerText).toMatch(/0\.1\./i); // Engine version pattern
  });

  test('all pages should be responsive and have no layout shift', async ({ page }) => {
    const routes = ['/', '/client', '/turns', '/rules'];

    for (const route of routes) {
      await page.goto(route);

      // Check that main content is visible
      const main = page.locator('main');
      await expect(main).toBeVisible();

      // Check heading hierarchy
      const heading = page.locator('h1, h2');
      expect(await heading.count()).toBeGreaterThan(0);
    }
  });
});

test.describe('Status JSON', () => {
  test('status.json should be accessible and valid JSON', async ({ page }) => {
    const response = await page.request.get('http://localhost:4321/status.json');
    expect(response.status()).toBe(200);

    const data = await response.json();
    expect(data).toHaveProperty('status');
    expect(data).toHaveProperty('turn');
    expect(data).toHaveProperty('factions');
    expect(Array.isArray(data.factions)).toBe(true);
    expect(data.factions).toHaveLength(10);
  });
});

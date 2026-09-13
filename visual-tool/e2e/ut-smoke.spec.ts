import { test, expect } from '@playwright/test';

/** UT-001 / UT-005 smoke — login UI without secrets in static bundle */
test('UT-001 login page loads', async ({ page }) => {
  await page.goto('/client/');
  await expect(page.getByRole('heading', { name: /SpaceAge Client/i })).toBeVisible();
});

test('UT-005 static bundle has no gamein paths', async ({ page }) => {
  await page.goto('/client/');
  const html = await page.content();
  expect(html).not.toMatch(/gamein\.xml/);
  expect(html).not.toMatch(/order\.\d+\.txt/);
});

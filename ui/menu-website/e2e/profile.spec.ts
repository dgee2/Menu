// This spec relies on the setup project's authenticated storageState; do not run it without that dependency.
import { expect, test } from '@playwright/test';

test('renders the authenticated user profile', async ({ page }) => {
  const expectedIdentity = process.env.E2E_AUTH0_USERNAME;

  if (!expectedIdentity) {
    throw new Error('E2E_AUTH0_USERNAME is required to identify the authenticated profile.');
  }

  await page.goto('/#/profile');

  await expect(page).toHaveURL(/\/#\/profile$/);
  await expect(page.getByText(expectedIdentity, { exact: false }).first()).toBeVisible();
});

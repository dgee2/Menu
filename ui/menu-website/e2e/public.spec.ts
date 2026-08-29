import { expect, test } from '@playwright/test';

test('shows the public home page', async ({ page }) => {
  await page.goto('/');

  await expect(page).toHaveTitle('Menu');
  await expect(page.getByText('Menu', { exact: true })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Login' })).toBeVisible();
});

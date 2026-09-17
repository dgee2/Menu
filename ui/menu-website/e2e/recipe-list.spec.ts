import { expect, test } from '@playwright/test';

const apiBaseUrl = 'http://localhost:65273/api/recipe';
const errorMessage = 'Something went wrong loading recipes.';

// Regression guard for #1243: a consent failure must not silently skip the API request.
test('loads owned and shared recipe lists with authenticated requests', async ({ page }) => {
  const mineResponsePromise = page.waitForResponse(
    (response) => response.url() === `${apiBaseUrl}?scope=mine`,
  );

  await page.goto('/#/recipes');

  const mineResponse = await mineResponsePromise;
  expect(mineResponse.status()).toBe(200);
  expect(mineResponse.request().headers().authorization).toMatch(/^Bearer\s+\S+$/i);

  await expect(page.getByText('Loading recipes...', { exact: true })).toBeHidden();
  await expect(page.getByText(errorMessage, { exact: true })).toBeHidden();
  await expect(
    page.locator('.q-list').or(page.getByText('You have not created any recipes yet.', { exact: true })),
  ).toBeVisible();

  const sharedResponsePromise = page.waitForResponse(
    (response) => response.url() === `${apiBaseUrl}?scope=authenticated`,
  );

  await page.getByRole('button', { name: 'Shared with everyone' }).click();

  const sharedResponse = await sharedResponsePromise;
  expect(sharedResponse.status()).toBe(200);
  expect(sharedResponse.request().headers().authorization).toMatch(/^Bearer\s+\S+$/i);

  await expect(page.getByText('Loading recipes...', { exact: true })).toBeHidden();
  await expect(page.getByText(errorMessage, { exact: true })).toBeHidden();
  await expect(
    page.locator('.q-list').or(page.getByText('Nobody has shared a recipe yet.', { exact: true })),
  ).toBeVisible();
});

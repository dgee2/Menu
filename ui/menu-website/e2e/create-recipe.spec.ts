import { expect, test } from '@playwright/test';

const apiBaseUrl = 'http://localhost:65273/api/recipe';

// Regression guard for recipe creation: this proves the authenticated form writes a complete
// recipe, shows the saved detail, and invalidates the owned-recipe list.
test('creates a recipe through the authenticated form', async ({ page, request }, testInfo) => {
  const uniqueTitle = `E2E create ${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}`;
  const ingredientText = 'coriander';
  const measureText = '1 bunch';
  const instructionText = 'Chop the coriander and serve.';
  let recipeId: string | undefined;
  let authorization: string | undefined;

  try {
    const initialListResponsePromise = page.waitForResponse(
      (response) => response.url() === `${apiBaseUrl}?scope=mine` && response.status() === 200,
    );
    await page.goto('/#/recipes');
    const initialListResponse = await initialListResponsePromise;
    authorization = initialListResponse.request().headers().authorization;
    expect(authorization).toMatch(/^Bearer\s+\S+$/i);

    await page.evaluate(() => {
      window.location.hash = '#/new-recipe';
    });
    await expect(page).toHaveURL(/\/#\/new-recipe$/);

    await page.getByLabel('Name').fill(uniqueTitle);
    await page.getByLabel('Summary').fill('A recipe created by the authenticated E2E smoke test.');
    await page.getByLabel('Measure').first().fill(measureText);
    await page.getByLabel('Ingredient').first().fill(ingredientText);
    await page.getByLabel('Instructions').first().fill(instructionText);

    const createResponsePromise = page.waitForResponse(
      (response) => response.url() === apiBaseUrl && response.request().method() === 'POST',
    );
    await page.getByRole('button', { name: 'Save recipe' }).click();

    const createResponse = await createResponsePromise;
    expect(createResponse.status()).toBe(200);
    authorization = createResponse.request().headers().authorization;
    expect(authorization).toMatch(/^Bearer\s+\S+$/i);

    const createdRecipe = (await createResponse.json()) as { id?: number | string };
    expect(createdRecipe.id).toBeDefined();
    recipeId = String(createdRecipe.id);

    await page.waitForURL((url) => url.hash === `#/recipe/${recipeId}`);
    await expect(page.getByRole('heading', { name: uniqueTitle, exact: true })).toBeVisible();
    await expect(page.getByText(`${measureText} ${ingredientText}`, { exact: true })).toBeVisible();
    await expect(page.getByText(instructionText, { exact: true })).toBeVisible();

    const listResponsePromise = page.waitForResponse(
      (response) =>
        response.url() === `${apiBaseUrl}?scope=mine` && response.request().method() === 'GET',
    );
    await page.goto('/#/recipes');

    const listResponse = await listResponsePromise;
    expect(listResponse.status()).toBe(200);
    expect(listResponse.request().headers().authorization).toMatch(/^Bearer\s+\S+$/i);
    await expect(page.getByRole('link').filter({ hasText: uniqueTitle })).toBeVisible();
  } finally {
    if (authorization !== undefined) {
      if (recipeId === undefined) {
        const lookupResponse = await request.get(`${apiBaseUrl}?scope=mine`, {
          headers: { authorization },
        });
        expect(lookupResponse.status()).toBe(200);
        const recipes = (await lookupResponse.json()) as Array<{
          id: number | string;
          title: string;
        }>;
        recipeId = String(recipes.find((recipe) => recipe.title === uniqueTitle)?.id ?? '');
      }
    }
    if (recipeId) {
      const cleanupAuthorization = authorization;
      if (cleanupAuthorization !== undefined) {
        const cleanupResponse = await request.delete(`${apiBaseUrl}/${recipeId}`, {
          headers: { authorization: cleanupAuthorization },
        });
        expect([204, 404]).toContain(cleanupResponse.status());
      }
    }
  }
});

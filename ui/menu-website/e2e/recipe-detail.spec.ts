import { randomUUID } from 'node:crypto';
import { expect, test } from '@playwright/test';

const apiBaseUrl = 'http://localhost:65273/api/recipe';

type SeededRecipe = {
  id: string;
  title: string;
};

// Protection for #1250: this spec seeds and removes its own row so parallel workers and shards
// cannot depend on another test's data or mutate a shared recipe.
test('opens an owned recipe detail from the recipe list', async ({ page }, testInfo) => {
  const title = `E2E detail ${testInfo.workerIndex}-${Date.now()}-${randomUUID().slice(0, 8)}`;
  const ingredient = 'detail smoke ingredient';
  const measure = '1 test unit';
  const instruction = 'Complete the detail smoke step';
  let seededRecipe: SeededRecipe | undefined;
  let authorization = '';

  try {
    const mineResponsePromise = page.waitForResponse(
      (response) => response.url() === `${apiBaseUrl}?scope=mine` && response.status() === 200,
    );
    await page.goto('/#/recipes');
    const mineResponse = await mineResponsePromise;
    authorization = mineResponse.request().headers().authorization ?? '';
    expect(authorization).toMatch(/^Bearer\s+\S+$/i);

    const createResponse = await page.request.post(apiBaseUrl, {
      headers: { authorization },
      data: {
        title,
        accessScope: 'Private',
        ingredients: [
          {
            ingredientText: ingredient,
            measureText: measure,
            sortOrder: 0,
          },
        ],
        steps: [{ instructionText: instruction, sortOrder: 0 }],
      },
    });
    expect(createResponse.status()).toBe(200);
    const created = (await createResponse.json()) as { id: string | number };
    const recipeId = String(created.id);
    seededRecipe = { id: recipeId, title };

    const refreshedMineResponsePromise = page.waitForResponse(
      (response) => response.url() === `${apiBaseUrl}?scope=mine` && response.status() === 200,
    );
    await page.goto('/#/recipes');
    await refreshedMineResponsePromise;

    const recipeLink = page.getByRole('link').filter({ hasText: title });
    await expect(recipeLink).toBeVisible();

    const detailResponsePromise = page.waitForResponse(
      (response) => response.url() === `${apiBaseUrl}/${recipeId}`,
    );
    await recipeLink.click();

    const detailResponse = await detailResponsePromise;
    expect(detailResponse.status()).toBe(200);
    await expect(page).toHaveURL(new RegExp(`/\\#/recipe/${recipeId}$`));
    await expect(page.locator('h1')).toHaveText(title);
    await expect(page.getByText(`${measure} ${ingredient}`, { exact: true })).toBeVisible();
    await expect(page.getByText(instruction, { exact: true })).toBeVisible();
  } finally {
    if (seededRecipe) {
      if (authorization) {
        const deleteResponse = await page.request.delete(`${apiBaseUrl}/${seededRecipe.id}`, {
          headers: { authorization },
        });
        expect([204, 404]).toContain(deleteResponse.status());
      }
    }
  }
});

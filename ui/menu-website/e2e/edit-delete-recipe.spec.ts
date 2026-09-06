import { randomUUID } from 'node:crypto';
import { expect, test, type APIRequestContext } from '@playwright/test';

const apiBaseUrl = 'http://localhost:65273/api/recipe';

type CreatedRecipe = {
  id: string;
};

const bestEffortDelete = async (
  request: APIRequestContext,
  recipeId: string | undefined,
  authorization: string | undefined,
) => {
  if (recipeId === undefined || authorization === undefined) return;

  let response: Awaited<ReturnType<APIRequestContext['delete']>>;
  try {
    response = await request.delete(`${apiBaseUrl}/${recipeId}`, {
      headers: { authorization },
    });
  } catch {
    // Preserve the original test failure when cleanup cannot reach the API.
    return;
  }
  if (![204, 404].includes(response.status())) {
    throw new Error(`Cleanup delete returned unexpected status ${response.status()}.`);
  }
};

// Protection for #1251: this spec owns its fixture and cleans it up so parallel workers and
// shards cannot depend on, or mutate, the recipe created by another smoke test.
test('edits and deletes an owned recipe', async ({ page, request }, testInfo) => {
  const uniqueTitle = `E2E edit-delete ${testInfo.workerIndex}-${Date.now()}-${randomUUID().slice(0, 8)}`;
  const updatedTitle = `${uniqueTitle} updated`;
  const ingredient = 'edit-delete smoke ingredient';
  const updatedIngredient = `${ingredient} updated`;
  const measure = '1 test unit';
  const instruction = 'Complete the edit-delete smoke step';
  let recipeId: string | undefined;
  let authorization: string | undefined;

  try {
    const mineResponsePromise = page.waitForResponse(
      (response) => response.url() === `${apiBaseUrl}?scope=mine` && response.status() === 200,
    );
    await page.goto('/#/recipes');
    const mineResponse = await mineResponsePromise;
    authorization = mineResponse.request().headers().authorization;
    expect(authorization).toMatch(/^Bearer\s+\S+$/i);

    const createResponse = await request.post(apiBaseUrl, {
      headers: { authorization },
      data: {
        title: uniqueTitle,
        accessScope: 'Private',
        ingredients: [{ ingredientText: ingredient, measureText: measure, sortOrder: 0 }],
        steps: [{ instructionText: instruction, sortOrder: 0 }],
      },
    });
    expect(createResponse.status()).toBe(200);
    const created = (await createResponse.json()) as CreatedRecipe;
    recipeId = String(created.id);

    const detailResponsePromise = page.waitForResponse(
      (response) => response.url() === `${apiBaseUrl}/${recipeId}` && response.status() === 200,
    );
    await page.goto(`/#/recipe/${recipeId}`);
    const detailResponse = await detailResponsePromise;
    expect(detailResponse.request().headers().authorization).toMatch(/^Bearer\s+\S+$/i);
    await expect(page).toHaveURL(new RegExp(`/\\#/recipe/${recipeId}$`));
    await expect(page.getByRole('heading', { name: uniqueTitle, exact: true })).toBeVisible();
    await expect(page.getByText(`${measure} ${ingredient}`, { exact: true })).toBeVisible();
    await expect(page.getByText(instruction, { exact: true })).toBeVisible();

    await page.getByRole('link', { name: 'Edit', exact: true }).click();
    await expect(page).toHaveURL(new RegExp(`/\\#/recipe/${recipeId}/edit$`));
    await expect(page.getByRole('heading', { name: 'Edit recipe', exact: true })).toBeVisible();
    await page.getByLabel('Name').fill(updatedTitle);
    await page.getByLabel('Ingredient').first().fill(updatedIngredient);

    const updateResponsePromise = page.waitForResponse(
      (response) =>
        response.url() === `${apiBaseUrl}/${recipeId}` && response.request().method() === 'PUT',
    );
    await page.getByRole('button', { name: 'Save changes', exact: true }).click();
    const updateResponse = await updateResponsePromise;
    expect(updateResponse.status()).toBe(200);
    expect(updateResponse.request().headers().authorization).toMatch(/^Bearer\s+\S+$/i);

    await page.waitForURL((url) => url.hash === `#/recipe/${recipeId}`);
    await expect(page.getByRole('heading', { name: updatedTitle, exact: true })).toBeVisible();
    await expect(page.getByText(`${measure} ${updatedIngredient}`, { exact: true })).toBeVisible();
    await expect(page.getByText(instruction, { exact: true })).toBeVisible();

    await page.getByRole('button', { name: 'Delete', exact: true }).click();
    const dialog = page.getByRole('dialog');
    await expect(dialog).toBeVisible();
    await expect(dialog.getByText('Delete this recipe?', { exact: true })).toBeVisible();

    const deleteResponsePromise = page.waitForResponse(
      (response) =>
        response.url() === `${apiBaseUrl}/${recipeId}` && response.request().method() === 'DELETE',
    );
    const listResponsePromise = page.waitForResponse(
      (response) => response.url() === `${apiBaseUrl}?scope=mine` && response.status() === 200,
    );
    await dialog.getByRole('button', { name: 'Delete', exact: true }).click();
    const deleteResponse = await deleteResponsePromise;
    expect(deleteResponse.status()).toBe(204);
    expect(deleteResponse.request().headers().authorization).toMatch(/^Bearer\s+\S+$/i);

    await page.waitForURL((url) => url.hash === '#/recipes');
    await listResponsePromise;
    await expect(page.getByRole('link').filter({ hasText: updatedTitle })).toBeHidden();
  } finally {
    await bestEffortDelete(request, recipeId, authorization);
  }
});

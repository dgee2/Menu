import type { APIRequestContext } from '@playwright/test';

export const apiBaseUrl = 'http://localhost:65273/api/recipe';

export const hardDeleteRecipe = async (
  request: APIRequestContext,
  recipeId: string | undefined,
  authorization: string | undefined,
) => {
  if (recipeId === undefined || authorization === undefined) return;

  const response = await request.delete(`http://localhost:65273/api/e2e-test/recipe/${recipeId}`, {
    headers: { authorization },
  });

  if (response.status() !== 204) {
    throw new Error(`E2E cleanup delete returned unexpected status ${response.status()}.`);
  }
};

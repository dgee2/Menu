import type { APIRequestContext } from '@playwright/test';

export const apiBaseUrl = 'http://localhost:65273/api/recipe';
const e2eCleanupBaseUrl = 'http://localhost:65273/api/e2e-test/recipe';

export const assertE2eCleanupAvailable = async (
  request: APIRequestContext,
  authorization: string,
) => {
  const response = await request.get(`${e2eCleanupBaseUrl}/status`, {
    headers: { authorization },
  });

  if (response.status() !== 204) {
    throw new Error(
      `E2E cleanup endpoint is unavailable (HTTP ${response.status()}). Restart the Aspire stack so it starts with the current E2E configuration.`,
    );
  }
};

export const hardDeleteRecipe = async (
  request: APIRequestContext,
  recipeId: string | undefined,
  authorization: string | undefined,
) => {
  if (recipeId === undefined || authorization === undefined) return;

  const response = await request.delete(`${e2eCleanupBaseUrl}/${recipeId}`, {
    headers: { authorization },
  });

  if (response.status() !== 204) {
    throw new Error(`E2E cleanup delete returned unexpected status ${response.status()}.`);
  }
};

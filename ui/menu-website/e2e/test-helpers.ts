import type { APIRequestContext } from '@playwright/test';

const apiHost = 'http://localhost:65273';
export const apiBaseUrl = `${apiHost}/api/recipe`;
const e2eCleanupBaseUrl = `${apiHost}/api/e2e-test/recipe`;

export const requireAuthorizationHeader = (authorization: string | undefined): string => {
  if (authorization === undefined) {
    throw new Error('Authenticated recipe list response did not include an authorization header.');
  }

  return authorization;
};

export const assertE2eCleanupAvailable = async (
  request: APIRequestContext,
  authorization: string,
) => {
  const response = await request.get(`${e2eCleanupBaseUrl}/status`, {
    headers: { authorization },
  });

  if (response.status() !== 204) {
    if (response.status() === 401) {
      throw new Error(
        'E2E cleanup endpoint returned HTTP 401. Ensure the authenticated E2E caller is configured for cleanup access.',
      );
    }

    if (response.status() >= 500) {
      throw new Error(
        `E2E cleanup endpoint returned HTTP ${response.status()}. The API is unhealthy; inspect the AppHost/API logs before retrying.`,
      );
    }

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

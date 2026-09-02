import { mkdir } from 'node:fs/promises';
import path from 'node:path';
import { expect, test as setup } from '@playwright/test';

const authFile = path.join('.playwright', '.auth', 'user.json');
const apiUrl = 'http://localhost:65273/api/recipe?scope=mine';

setup('authenticate with Auth0', async ({ page }) => {
  const username = process.env.E2E_AUTH0_USERNAME;
  const password = process.env.E2E_AUTH0_PASSWORD;

  if (!username || !password) {
    throw new Error(
      'E2E_AUTH0_USERNAME and E2E_AUTH0_PASSWORD are required in .env.e2e.local to run authenticated e2e tests.',
    );
  }

  const authenticatedResponse = page.waitForResponse((response) => {
    if (response.url() !== apiUrl || response.status() !== 200) {
      return false;
    }

    const authorization = response.request().headers().authorization;
    return /^Bearer\s+\S+$/i.test(authorization ?? '');
  });

  await page.goto('/#/recipes');

  if (new URL(page.url()).hostname.endsWith('auth0.com')) {
    const usernameField = page.locator('input[name="username"], input[type="email"]').first();
    const passwordField = page.locator('input[type="password"]').first();
    const submit = page.getByRole('button', { name: /^(continue|log in|login)$/i }).first();

    if (await usernameField.isVisible()) {
      await usernameField.fill(username);
      await submit.click();
    }

    await expect(passwordField).toBeVisible();
    await passwordField.fill(password);
    await submit.click();

    const consent = page.getByRole('button', { name: /^(accept|allow|authorize|continue)$/i }).first();
    await consent.waitFor({ state: 'visible', timeout: 5000 }).catch(() => undefined);
    if (new URL(page.url()).hostname.endsWith('auth0.com') && (await consent.isVisible().catch(() => false))) {
      await consent.click();
    }

    await page.waitForURL((url) => !url.hostname.endsWith('auth0.com'));
  }

  const response = await authenticatedResponse;
  expect(response.status()).toBe(200);
  expect(response.request().headers().authorization).toMatch(/^Bearer\s+\S+$/i);

  await mkdir(path.dirname(authFile), { recursive: true });
  await page.context().storageState({ path: authFile });
});

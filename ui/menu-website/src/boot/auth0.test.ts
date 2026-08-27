import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

const auth0 = vi.hoisted(() => vi.fn((options: unknown) => options));

vi.mock('@auth0/auth0-vue', () => ({
  createAuth0: auth0,
}));

describe('createAuth0', () => {
  beforeEach(() => {
    auth0.mockClear();
  });

  afterEach(() => {
    vi.unstubAllEnvs();
  });

  it('configures the API audience and persistent refresh-token cache', async () => {
    vi.stubEnv('VITE_MENU_API_URL', 'http://localhost:65273');
    const { createAuth0 } = await import('./auth0');

    createAuth0();

    expect(auth0).toHaveBeenCalledWith({
      domain: 'dev-oz81ytsjd1h4r1lz.uk.auth0.com',
      clientId: 'XkygJHG4uxZ2g4vF0LMYmRWsWxeIXqQa',
      authorizationParams: {
        redirect_uri: window.location.origin,
        audience: 'http://localhost:65273',
      },
      useRefreshTokens: true,
      cacheLocation: 'localstorage',
    });
  });
});

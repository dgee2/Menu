import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ref } from 'vue';
import { useAuth } from './auth';

const { loginWithRedirect, getAccessTokenSilently } = vi.hoisted(() => ({
  loginWithRedirect: vi.fn(),
  getAccessTokenSilently: vi.fn(),
}));

const isAuthenticated = ref(true);
const user = ref<{ sub?: string; name?: string } | undefined>(undefined);

vi.mock('@auth0/auth0-vue', () => ({
  useAuth0: () => ({
    loginWithRedirect,
    getAccessTokenSilently,
    logout: vi.fn(),
    isAuthenticated,
    user,
  }),
}));

describe('useAuth audience', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    isAuthenticated.value = true;
    user.value = undefined;
    vi.stubEnv('VITE_MENU_API_URL', 'http://localhost:5002');
    vi.stubEnv('VITE_AUTH0_AUDIENCE', 'http://localhost:65273');
  });

  afterEach(() => {
    vi.unstubAllEnvs();
  });

  it('clears the user when authentication ends even if the profile remains available', () => {
    user.value = { sub: 'user-1', name: 'Test user' };
    const auth = useAuth();

    expect(auth.user.value?.subject).toBe('user-1');
    isAuthenticated.value = false;
    expect(auth.user.value).toBeUndefined();
  });

  it('requests the registered audience during login when the API is hosted at another URL', () => {
    void useAuth().login();

    expect(loginWithRedirect).toHaveBeenCalledWith({
      authorizationParams: { audience: 'http://localhost:65273' },
    });
  });

  it('requests tokens for the registered audience when the API is hosted at another URL', async () => {
    getAccessTokenSilently.mockResolvedValue('test-access-token');

    await expect(useAuth().getAccessToken()).resolves.toBe('test-access-token');
    expect(getAccessTokenSilently).toHaveBeenCalledWith({
      authorizationParams: { audience: 'http://localhost:65273' },
    });
  });
});

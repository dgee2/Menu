import { ref } from 'vue';

type MockUser = {
  sub?: string;
  name?: string;
  nickname?: string;
  picture?: string;
  email?: string;
  email_verified?: boolean;
};

export type { MockUser };

const isAuthenticated = ref(false);
const user = ref<MockUser | undefined>(undefined);

export const setMockAuthState = (state: { isAuthenticated?: boolean; user?: MockUser }) => {
  if (state.isAuthenticated !== undefined) {
    isAuthenticated.value = state.isAuthenticated;
  }

  if ('user' in state) {
    user.value = state.user;
  }
};

export const resetMockAuthState = () => {
  isAuthenticated.value = false;
  user.value = undefined;
};

export const useAuth0 = () => ({
  loginWithRedirect: () => Promise.resolve(undefined),
  logout: () => Promise.resolve(undefined),
  isAuthenticated,
  user,
  getAccessTokenSilently: () => Promise.resolve('storybook-token'),
});

export const createAuth0 = () => ({
  install: () => undefined,
});

export const authGuard = () => true;

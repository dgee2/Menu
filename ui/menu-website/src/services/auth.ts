import { useAuth0 } from '@auth0/auth0-vue';
import { computed } from 'vue';

type User = {
  nickname: string;
  name: string;
  picture: URL | undefined;
  subject: string;
};

export const useAuth = () => {
  // eslint-disable-next-line @typescript-eslint/unbound-method
  const { loginWithRedirect, logout, isAuthenticated, user } = useAuth0();

  const auth0Login = () => loginWithRedirect({});
  const auth0Logout = async () => {
    await logout({ logoutParams: { returnTo: window.location.origin } });
  };
  const auth0User = computed(() => {
    if (!isAuthenticated || user.value === undefined || user.value.sub === undefined)
      return undefined;

    return {
      name: user.value.name ?? '',
      nickname: user.value.nickname ?? '',
      picture:
        user.value.picture && URL.canParse(user.value.picture)
          ? new URL(user.value.picture)
          : undefined,
      subject: user.value.sub,
    } satisfies User;
  });

  return {
    login: auth0Login,
    logout: auth0Logout,
    isAuthenticated: isAuthenticated && user.value,
    user: auth0User,
  };
};

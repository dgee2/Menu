import { useAuth0 } from '@auth0/auth0-vue';
import { computed } from 'vue';

type User = {
  nickname: string;
  name: string;
  picture: URL | undefined;
  subject: string;
  email: string | undefined;
  emailVerified: boolean | undefined;
};

export const useAuth = () => {
  // eslint-disable-next-line @typescript-eslint/unbound-method
  const { loginWithRedirect, logout, isAuthenticated, user } = useAuth0();

  const auth0Login = () => loginWithRedirect({});
  const auth0Logout = async () => {
    await logout({ logoutParams: { returnTo: window.location.origin } });
  };
  const auth0User = computed(() => {
    const userValue = user.value;
    if (!isAuthenticated || userValue?.sub === undefined) return undefined;

    return {
      name: userValue.name ?? '',
      nickname: userValue.nickname ?? '',
      picture:
        userValue.picture && URL.canParse(userValue.picture)
          ? new URL(userValue.picture)
          : undefined,
      subject: userValue.sub,
      email: userValue.email,
      emailVerified: userValue.email_verified,
    } satisfies User;
  });

  return {
    login: auth0Login,
    logout: auth0Logout,
    isAuthenticated: isAuthenticated && user.value,
    user: auth0User,
  };
};

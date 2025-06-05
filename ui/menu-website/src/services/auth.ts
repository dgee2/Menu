import { useAuth0 } from '@auth0/auth0-vue';
import { computed } from 'vue';

type User = {
  nickname: string;
  name: string;
  picture: string | undefined;
  subject: string;
  email: string | undefined;
  emailVerified: boolean | undefined;
};

export const useAuth = () => {
  // eslint-disable-next-line @typescript-eslint/unbound-method
  const { loginWithRedirect, logout, isAuthenticated, user, getAccessTokenSilently } = useAuth0();

  const auth0IsAuthenticated = computed(() => isAuthenticated.value && user.value !== undefined);
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
      picture: userValue.picture && URL.canParse(userValue.picture) ? userValue.picture : undefined,
      subject: userValue.sub,
      email: userValue.email,
      emailVerified: userValue.email_verified,
    } satisfies User;
  });

  const getAccessToken = async () => {
    return await getAccessTokenSilently();
  };

  return {
    login: auth0Login,
    logout: auth0Logout,
    isAuthenticated: auth0IsAuthenticated,
    user: auth0User,
    getAccessToken: getAccessToken,
  };
};

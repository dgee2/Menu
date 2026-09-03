import { QLayout, QPageContainer, Quasar } from 'quasar';
import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { defineComponent, ref } from 'vue';
import UserInfo from './UserInfo.vue';

const authMocks = vi.hoisted(() => ({
  useAuth: vi.fn(),
}));

vi.mock('@/services/auth', () => ({
  useAuth: authMocks.useAuth,
}));

const mountPage = () => {
  const LayoutHost = defineComponent({
    components: { QLayout, QPageContainer, UserInfo },
    template: '<q-layout><q-page-container><user-info /></q-page-container></q-layout>',
  });

  return mount(LayoutHost, { global: { plugins: [Quasar] } });
};

describe('UserInfo', () => {
  beforeEach(() => {
    authMocks.useAuth.mockReset();
  });

  it('renders the authenticated identity', () => {
    authMocks.useAuth.mockReturnValue({
      isAuthenticated: ref(true),
      user: ref({
        name: 'Jane Doe',
        nickname: 'Jane',
        picture: 'https://example.com/avatar.png',
        subject: 'auth0|user-42',
        email: 'jane@example.com',
        emailVerified: true,
      }),
    });

    const wrapper = mountPage();

    expect(wrapper.get('h1').text()).toBe('Profile');
    expect(wrapper.get('[data-testid="profile-identity"]').text()).toBe('jane@example.com');
    expect(wrapper.get('img').attributes('alt')).toBe('Jane Doe profile picture');
  });

  it('renders an unavailable message for anonymous users', () => {
    authMocks.useAuth.mockReturnValue({
      isAuthenticated: ref(false),
      user: ref(undefined),
    });

    const wrapper = mountPage();

    expect(wrapper.text()).toContain('Profile unavailable.');
  });
});

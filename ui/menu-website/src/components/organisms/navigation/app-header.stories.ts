import preview from '@storybook-config/preview';
import { QLayout, QPageContainer } from 'quasar';
import { expect, within } from 'storybook/test';
import AppHeader from './app-header.vue';
import { resetMockAuthState, setMockAuthState } from '@storybook-config/mocks/auth0-vue';

const meta = preview.meta({
  title: 'Organisms/Navigation/AppHeader',
  component: AppHeader,
  tags: ['autodocs'],
  render: () => ({
    components: { AppHeader, QLayout, QPageContainer },
    template:
      '<q-layout view="lHh lpr lFf"><app-header /><q-page-container><div /></q-page-container></q-layout>',
  }),
});

export const Anonymous = meta.story({
  beforeEach: () => {
    resetMockAuthState();
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByRole('button', { name: 'Login' })).toBeInTheDocument();
  },
});

export const Authenticated = meta.story({
  beforeEach: () => {
    resetMockAuthState();
    setMockAuthState({
      isAuthenticated: true,
      user: {
        sub: 'user-12',
        nickname: 'Jane Doe',
      },
    });
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByRole('button', { name: 'Logout' })).toBeInTheDocument();
  },
});

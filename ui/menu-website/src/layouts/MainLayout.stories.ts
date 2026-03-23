import preview from '@storybook-config/preview';
import { expect, within } from 'storybook/test';
import MainLayout from './MainLayout.vue';
import { resetMockAuthState, setMockAuthState } from '@storybook-config/mocks/auth0-vue';

const meta = preview.meta({
  title: 'Layouts/MainLayout',
  component: MainLayout,
  tags: ['autodocs'],
});

export const Anonymous = meta.story({
  render: () => {
    resetMockAuthState();
    return {
      components: { MainLayout },
      template: '<main-layout />',
    };
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByRole('button', { name: 'Login' })).toBeInTheDocument();
  },
});

export const Authenticated = meta.story({
  render: () => {
    resetMockAuthState();
    setMockAuthState({
      isAuthenticated: true,
      user: {
        sub: 'user-12',
        nickname: 'Jane Doe',
      },
    });

    return {
      components: { MainLayout },
      template: '<main-layout />',
    };
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByRole('button', { name: 'Logout' })).toBeInTheDocument();
  },
});

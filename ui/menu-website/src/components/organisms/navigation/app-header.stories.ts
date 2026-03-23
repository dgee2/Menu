import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { QLayout, QPageContainer } from 'quasar';
import { expect, within } from 'storybook/test';
import AppHeader from './app-header.vue';
import { resetMockAuthState, setMockAuthState } from '../../../../.storybook/mocks/auth0-vue';

const meta = {
  title: 'Organisms/Navigation/AppHeader',
  component: AppHeader,
  tags: ['autodocs'],
  render: () => ({
    components: { AppHeader, QLayout, QPageContainer },
    template:
      '<q-layout view="lHh lpr lFf"><app-header /><q-page-container><div /></q-page-container></q-layout>',
  }),
} satisfies Meta<typeof AppHeader>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Anonymous: Story = {
  beforeEach: () => {
    resetMockAuthState();
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByRole('button', { name: 'Login' })).toBeInTheDocument();
  },
};

export const Authenticated: Story = {
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
};

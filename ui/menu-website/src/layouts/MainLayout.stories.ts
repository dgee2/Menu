import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { expect, within } from 'storybook/test';
import MainLayout from './MainLayout.vue';
import { resetMockAuthState, setMockAuthState } from '../../.storybook/mocks/auth0-vue';

const meta = {
  title: 'Layouts/MainLayout',
  component: MainLayout,
  tags: ['autodocs'],
} satisfies Meta<typeof MainLayout>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Anonymous: Story = {
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
};

export const Authenticated: Story = {
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
};



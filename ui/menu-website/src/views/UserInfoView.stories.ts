import UserInfoView from './UserInfoView.vue';
import type { Meta, StoryObj } from '@storybook/vue3-vite';

const meta: Meta<typeof UserInfoView> = {
  title: 'Views/UserInfoView',
  component: UserInfoView,
};

export default meta;

type Story = StoryObj<typeof UserInfoView>;

export const Authenticated: Story = {
  args: {
    isAuthenticated: true,
    user: { picture: 'https://example.com/avatar.png', name: 'Test User' },
    auth0: 'auth0-client',
  },
};

export const NotAuthenticated: Story = {
  args: {
    isAuthenticated: false,
    auth0: 'auth0-client',
  },
};

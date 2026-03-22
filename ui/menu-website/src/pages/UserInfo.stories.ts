import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { expect, within } from 'storybook/test';
import { withPageLayout } from '../../.storybook/preview';
import UserInfo from './UserInfo.vue';
import { resetMockAuthState, setMockAuthState } from '../../.storybook/mocks/auth0-vue';

const meta = {
  title: 'Pages/UserInfo',
  component: UserInfo,
  tags: ['autodocs'],
  decorators: [withPageLayout],
} satisfies Meta<typeof UserInfo>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Authenticated: Story = {
  render: () => {
    resetMockAuthState();
    setMockAuthState({
      isAuthenticated: true,
      user: {
        sub: 'user-42',
        nickname: 'Jane Doe',
        picture: 'https://example.com/avatar.png',
        email: 'jane@example.com',
      },
    });

    return {
      components: { UserInfo },
      template: '<user-info />',
    };
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getAllByText(/jane@example.com/i)).toHaveLength(2);
    await expect(canvas.getByRole('img')).toBeInTheDocument();
  },
};

export const Anonymous: Story = {
  render: () => {
    resetMockAuthState();
    return {
      components: { UserInfo },
      template: '<user-info />',
    };
  },
};



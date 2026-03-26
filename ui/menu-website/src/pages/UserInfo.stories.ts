import { expect, within } from 'storybook/test';
import preview, { withPageLayout } from '@storybook-config/preview';
import UserInfo from './UserInfo.vue';
import { resetMockAuthState, setMockAuthState } from '@storybook-config/mocks/auth0-vue';

const meta = preview.meta({
  title: 'Pages/UserInfo',
  component: UserInfo,
  tags: ['autodocs'],
  decorators: [withPageLayout],
});

export const Authenticated = meta.story({
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
});

export const Anonymous = meta.story({
  render: () => {
    resetMockAuthState();
    return {
      components: { UserInfo },
      template: '<user-info />',
    };
  },
});

import preview from '@storybook-config/preview';
import { expect, userEvent, within } from 'storybook/test';
import ProfileButton from './ProfileButton.vue';
import { resetMockAuthState, setMockAuthState } from '@storybook-config/mocks/auth0-vue';

const meta = preview.meta({
  title: 'Molecules/Navigation/ProfileButton',
  component: ProfileButton,
  tags: ['autodocs'],
});

export const WithAvatar = meta.story({
  render: () => {
    resetMockAuthState();
    setMockAuthState({
      isAuthenticated: true,
      user: {
        sub: 'user-1',
        nickname: 'Daniel Gee',
        picture: 'https://example.com/avatar.png',
      },
    });

    return {
      components: { ProfileButton },
      template: '<profile-button />',
    };
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const button = canvas.getByRole('button');
    await userEvent.click(button);
    await expect(button).toBeInTheDocument();
  },
});

export const InitialsFallback = meta.story({
  render: () => {
    resetMockAuthState();
    setMockAuthState({
      isAuthenticated: true,
      user: {
        sub: 'user-2',
        nickname: 'Jane Doe',
      },
    });

    return {
      components: { ProfileButton },
      template: '<profile-button />',
    };
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByText('JD')).toBeInTheDocument();
  },
});

import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { expect, userEvent, within } from 'storybook/test';
import ProfileButton from './ProfileButton.vue';
import { resetMockAuthState, setMockAuthState } from '../../../../.storybook/mocks/auth0-vue';

const meta = {
  title: 'Molecules/Navigation/ProfileButton',
  component: ProfileButton,
  tags: ['autodocs'],
} satisfies Meta<typeof ProfileButton>;

export default meta;
type Story = StoryObj<typeof meta>;

export const WithAvatar: Story = {
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
};

export const InitialsFallback: Story = {
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
};


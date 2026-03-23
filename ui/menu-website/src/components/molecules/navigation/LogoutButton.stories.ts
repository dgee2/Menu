import preview from '@storybook-config/preview';
import { expect, userEvent, within } from 'storybook/test';
import LogoutButton from './LogoutButton.vue';
import { resetMockAuthState } from '@storybook-config/mocks/auth0-vue';

const meta = preview.meta({
  title: 'Molecules/Navigation/LogoutButton',
  component: LogoutButton,
  tags: ['autodocs'],
  render: () => {
    resetMockAuthState();
    return {
      components: { LogoutButton },
      template: '<logout-button />',
    };
  },
});

export const Default = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const button = canvas.getByRole('button', { name: 'Logout' });

    await userEvent.click(button);
    await expect(button).toBeInTheDocument();
  },
});

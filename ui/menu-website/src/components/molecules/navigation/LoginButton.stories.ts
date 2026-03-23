import preview from '../../../../.storybook/preview';
import { expect, userEvent, within } from 'storybook/test';
import LoginButton from './LoginButton.vue';
import { resetMockAuthState } from '../../../../.storybook/mocks/auth0-vue';

const meta = preview.meta({
  title: 'Molecules/Navigation/LoginButton',
  component: LoginButton,
  tags: ['autodocs'],
  render: () => {
    resetMockAuthState();
    return {
      components: { LoginButton },
      template: '<login-button />',
    };
  },
});

export const Default = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const button = canvas.getByRole('button', { name: 'Login' });

    await userEvent.click(button);
    await expect(button).toBeInTheDocument();
  },
});

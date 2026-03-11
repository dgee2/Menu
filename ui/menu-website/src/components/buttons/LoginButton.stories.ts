import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { expect, userEvent, within } from 'storybook/test';
import LoginButton from './LoginButton.vue';
import { resetMockAuthState } from '../../../.storybook/mocks/auth0-vue';

const meta = {
  title: 'Buttons/LoginButton',
  component: LoginButton,
  tags: ['autodocs'],
  render: () => {
    resetMockAuthState();
    return {
      components: { LoginButton },
      template: '<login-button />',
    };
  },
} satisfies Meta<typeof LoginButton>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const button = canvas.getByRole('button', { name: 'Login' });

    await userEvent.click(button);
    await expect(button).toBeInTheDocument();
  },
};



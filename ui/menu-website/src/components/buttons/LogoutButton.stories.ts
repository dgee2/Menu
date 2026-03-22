import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { expect, userEvent, within } from 'storybook/test';
import LogoutButton from './LogoutButton.vue';
import { resetMockAuthState } from '../../../.storybook/mocks/auth0-vue';

const meta = {
  title: 'Buttons/LogoutButton',
  component: LogoutButton,
  tags: ['autodocs'],
  render: () => {
    resetMockAuthState();
    return {
      components: { LogoutButton },
      template: '<logout-button />',
    };
  },
} satisfies Meta<typeof LogoutButton>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const button = canvas.getByRole('button', { name: 'Logout' });

    await userEvent.click(button);
    await expect(button).toBeInTheDocument();
  },
};



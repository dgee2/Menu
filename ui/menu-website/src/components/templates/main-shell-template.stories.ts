import type { Meta, StoryObj } from '@storybook/vue3-vite';
import { expect, within } from 'storybook/test';
import MainShellTemplate from './main-shell-template.vue';
import { resetMockAuthState } from '../../../.storybook/mocks/auth0-vue';

const meta = {
  title: 'Templates/MainShellTemplate',
  component: MainShellTemplate,
  tags: ['autodocs'],
} satisfies Meta<typeof MainShellTemplate>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  render: () => {
    resetMockAuthState();
    return {
      components: { MainShellTemplate },
      template: '<main-shell-template><main>Page Content</main></main-shell-template>',
    };
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByText('Page Content')).toBeInTheDocument();
  },
};

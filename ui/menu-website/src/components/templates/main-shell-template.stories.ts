import preview from '../../../.storybook/preview';
import { expect, within } from 'storybook/test';
import MainShellTemplate from './main-shell-template.vue';
import { resetMockAuthState } from '../../../.storybook/mocks/auth0-vue';

const meta = preview.meta({
  title: 'Templates/MainShellTemplate',
  component: MainShellTemplate,
  tags: ['autodocs'],
});

export const Default = meta.story({
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
});

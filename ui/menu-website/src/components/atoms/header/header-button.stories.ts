import preview from '../../../../.storybook/preview';
import HeaderButton from '@/components/atoms/header/header-button.vue';
import { expect, fn, userEvent, within } from 'storybook/test';

const meta = preview.meta({
  title: 'Atoms/Header/HeaderButton',
  component: HeaderButton,
  tags: ['autodocs'],
  args: {
    label: 'Button',
    onClick: fn(),
  },
});

export const Default = meta.story({
  play: async ({ canvasElement, args }) => {
    const canvas = within(canvasElement);
    const button = canvas.getByRole('button', { name: 'Button' });

    await userEvent.click(button);
    await expect(args.onClick).toHaveBeenCalledTimes(1);
  },
});

export const ChangeLabel = meta.story({
  args: {
    label: 'Click Me',
  },
  play: async ({ canvasElement, args }) => {
    const canvas = within(canvasElement);
    const button = canvas.getByRole('button', { name: 'Click Me' });

    await userEvent.click(button);
    await expect(args.onClick).toHaveBeenCalledTimes(1);
  },
});

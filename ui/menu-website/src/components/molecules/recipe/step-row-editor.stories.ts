import preview from '@storybook-config/preview';
import StepRowEditor from './step-row-editor.vue';
import { expect, fn, userEvent, within } from 'storybook/test';

const meta = preview.meta({
  title: 'Molecules/Recipe/StepRowEditor',
  component: StepRowEditor,
  tags: ['autodocs'],
  args: {
    canMoveUp: true,
    canMoveDown: true,
    'onUpdate:instructionText': fn(),
    'onUpdate:title': fn(),
    'onUpdate:durationMinutes': fn(),
    onRemove: fn(),
    onMoveUp: fn(),
    onMoveDown: fn(),
  },
});

export const Default = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByLabelText('Title')).toBeInTheDocument();
    await expect(canvas.getByLabelText('Instructions')).toBeInTheDocument();
    await expect(canvas.getByLabelText('Duration (minutes)')).toBeInTheDocument();
  },
});

export const EditingFieldsUpdatesModel = meta.story({
  play: async ({ canvasElement, args }) => {
    const canvas = within(canvasElement);

    await userEvent.type(canvas.getByLabelText('Instructions'), 'Preheat the oven to 180C');
    await expect(args['onUpdate:instructionText']).toHaveBeenCalled();

    await userEvent.type(canvas.getByLabelText('Title'), 'Preheat');
    await expect(args['onUpdate:title']).toHaveBeenCalled();

    await userEvent.type(canvas.getByLabelText('Duration (minutes)'), '10');
    await expect(args['onUpdate:durationMinutes']).toHaveBeenCalledWith(10);
  },
});

export const RemoveAndReorderEmitEvents = meta.story({
  play: async ({ canvasElement, args }) => {
    const canvas = within(canvasElement);

    await userEvent.click(canvas.getByRole('button', { name: 'Move step up' }));
    await expect(args.onMoveUp).toHaveBeenCalled();

    await userEvent.click(canvas.getByRole('button', { name: 'Move step down' }));
    await expect(args.onMoveDown).toHaveBeenCalled();

    await userEvent.click(canvas.getByRole('button', { name: 'Remove step' }));
    await expect(args.onRemove).toHaveBeenCalled();
  },
});

export const FirstRowCannotMoveUp = meta.story({
  args: {
    canMoveUp: false,
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByRole('button', { name: 'Move step up' })).toBeDisabled();
    await expect(canvas.getByRole('button', { name: 'Move step down' })).toBeEnabled();
  },
});

export const LastRowCannotMoveDown = meta.story({
  args: {
    canMoveDown: false,
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await expect(canvas.getByRole('button', { name: 'Move step down' })).toBeDisabled();
    await expect(canvas.getByRole('button', { name: 'Move step up' })).toBeEnabled();
  },
});

import { expect, userEvent, within } from 'storybook/test';
import preview from '@storybook-config/preview';
import ComboboxField from './combobox-field.vue';

const meta = preview.meta({
  title: 'Atoms/Form/ComboboxField',
  component: ComboboxField,
  tags: ['autodocs'],
  args: {
    label: 'Section',
    hint: 'e.g. For the sauce',
    suggestions: ['For the sauce', 'For the topping'],
  },
});

export const Default = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    await expect(canvas.getByLabelText('Section')).toBeInTheDocument();
    await expect(canvas.getByText('e.g. For the sauce')).toBeInTheDocument();
  },
});

export const AcceptsAValueNotInTheSuggestions = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    const input = canvas.getByLabelText('Section');
    await userEvent.type(input, 'For the glaze');
    await userEvent.keyboard('{Enter}');

    await expect(input).toHaveValue('For the glaze');
  },
});

export const ShowsTheCurrentValue = meta.story({
  args: { modelValue: 'For the topping' },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);

    await expect(canvas.getByLabelText('Section')).toHaveValue('For the topping');
  },
});

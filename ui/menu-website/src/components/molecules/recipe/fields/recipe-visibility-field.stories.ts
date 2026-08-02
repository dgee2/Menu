import preview from '@storybook-config/preview';
import RecipeVisibilityField from './recipe-visibility-field.vue';
import { expect, fn, userEvent, within } from 'storybook/test';

const meta = preview.meta({
  title: 'Molecules/Recipe/Fields/RecipeVisibilityField',
  component: RecipeVisibilityField,
  tags: ['autodocs'],
  args: {
    'onUpdate:modelValue': fn(),
  },
});

export const Default = meta.story({
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const combobox = canvas.getByRole('combobox', { name: 'Visibility' });
    await expect(combobox).toHaveValue('Private');
  },
});

export const SelectingAuthenticatedUsersUpdatesModel = meta.story({
  play: async ({ canvasElement, args }) => {
    const canvas = within(canvasElement);
    const combobox = canvas.getByRole('combobox', { name: 'Visibility' });

    await userEvent.click(combobox);

    const body = within(document.body);
    await userEvent.click(await body.findByRole('option', { name: 'Visible to all Menu users' }));

    await expect(args['onUpdate:modelValue']).toHaveBeenCalledWith('AuthenticatedUsers');
  },
});

export const PreFilledValue = meta.story({
  args: {
    modelValue: 'AuthenticatedUsers',
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    const combobox = canvas.getByRole('combobox', { name: 'Visibility' });
    await expect(combobox).toHaveValue('Visible to all Menu users');
  },
});

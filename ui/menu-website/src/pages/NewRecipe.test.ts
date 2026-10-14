import { mount } from '@vue/test-utils';
import { Quasar } from 'quasar';
import { describe, expect, it } from 'vitest';
import NewRecipe from './NewRecipe.vue';

describe('NewRecipe', () => {
  it('renders the recipe form inside the page', () => {
    const wrapper = mount(NewRecipe, {
      global: {
        plugins: [Quasar],
        stubs: {
          'q-page': { template: '<div><slot /></div>' },
          RecipeForm: true,
        },
      },
    });

    expect(wrapper.findComponent({ name: 'RecipeForm' }).exists()).toBe(true);
  });
});

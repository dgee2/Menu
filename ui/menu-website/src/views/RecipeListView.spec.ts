import { mount } from '@vue/test-utils';
import { describe, it, expect } from 'vitest';
import RecipeListView from './RecipeListView.vue';

describe('RecipeListView', () => {
  it('renders list of recipes', () => {
    const recipes = [
      { id: 1, name: 'Pancakes', description: 'Fluffy' },
      { id: 2, name: 'Salad', description: 'Fresh' },
    ];

    const wrapper = mount(RecipeListView, {
      props: { recipes, isLoading: false },
      global: {
        stubs: {
          'q-list': { template: '<div><slot/></div>' },
          'q-item': { template: '<div><slot/></div>' },
          'q-item-section': { template: '<div><slot/></div>' },
        },
      },
    });

    expect(wrapper.text()).toContain('Pancakes');
    expect(wrapper.text()).toContain('Salad');
    expect(wrapper.text()).not.toContain('Loading recipes');
    expect(wrapper.text()).not.toContain('No recipes found.');
  });

  it('shows loading state when loading and no recipes', () => {
    const wrapper = mount(RecipeListView, {
      props: { recipes: [], isLoading: true },
      global: {
        stubs: {
          'q-list': { template: '<div><slot/></div>' },
          'q-item': { template: '<div><slot/></div>' },
          'q-item-section': { template: '<div><slot/></div>' },
        },
      },
    });

    expect(wrapper.text()).toContain('Loading recipes...');
  });

  it('shows empty state when no recipes and not loading', () => {
    const wrapper = mount(RecipeListView, {
      props: { recipes: [], isLoading: false },
      global: {
        stubs: {
          'q-list': { template: '<div><slot/></div>' },
          'q-item': { template: '<div><slot/></div>' },
          'q-item-section': { template: '<div><slot/></div>' },
        },
      },
    });

    expect(wrapper.text()).toContain('No recipes found.');
  });
});

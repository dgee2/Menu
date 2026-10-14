import { flushPromises, mount } from '@vue/test-utils';
import { Quasar } from 'quasar';
import { describe, expect, it } from 'vitest';
import { createMemoryHistory, createRouter } from 'vue-router';
import RecipeListButton from './RecipeListButton.vue';

describe('RecipeListButton', () => {
  it('navigates to the recipe list when clicked', async () => {
    const router = createRouter({
      history: createMemoryHistory(),
      routes: [
        { path: '/', component: { template: '<div />' } },
        { path: '/recipes', component: { template: '<div />' } },
      ],
    });
    await router.push('/');
    await router.isReady();

    const wrapper = mount(RecipeListButton, {
      global: { plugins: [Quasar, router] },
    });

    await wrapper.get('button').trigger('click');
    await flushPromises();

    expect(router.currentRoute.value.path).toBe('/recipes');
  });
});

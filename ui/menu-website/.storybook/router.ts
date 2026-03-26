import { createMemoryHistory, createRouter } from 'vue-router';
import { defineComponent } from 'vue';

const StoryRouteView = defineComponent({
  template: '<div />',
});

export const router = createRouter({
  history: createMemoryHistory(),
  routes: [
    { path: '/', component: StoryRouteView },
    { path: '/recipes', component: StoryRouteView },
    { path: '/new-recipe', component: StoryRouteView },
    { path: '/profile', component: StoryRouteView },
  ],
});


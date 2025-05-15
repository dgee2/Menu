import type { RouteRecordRaw } from 'vue-router';

export const authenticatedRoutes: RouteRecordRaw[] = [
  {
    path: 'profile',
    component: () => import('pages/UserInfo.vue'),
  },
  {
    path: 'new-recipe',
    component: () => import('pages/NewRecipe.vue'),
  },
];

import type { RouteRecordRaw } from 'vue-router';

export const authenticatedRoutes: RouteRecordRaw[] = [
  {
    path: 'profile',
    component: () => import('@/pages/UserInfoPage.vue'),
  },
  {
    path: 'new-recipe',
    component: () => import('@/pages/NewRecipePage.vue'),
  },
  {
    path: 'recipes',
    component: () => import('@/pages/RecipeListPage.vue'),
  },
];

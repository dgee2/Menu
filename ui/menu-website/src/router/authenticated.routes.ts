import type { RouteRecordRaw } from 'vue-router';

export const authenticatedRoutes: RouteRecordRaw[] = [
  {
    path: 'profile',
    component: () => import('@/pages/UserInfo.vue'),
  },
  {
    path: 'new-recipe',
    component: () => import('@/pages/NewRecipe.vue'),
  },
  {
    path: 'recipes',
    component: () => import('@/pages/RecipeList.vue'),
  },
  {
    path: 'recipe/:recipeId',
    component: () => import('@/pages/RecipeDetail.vue'),
    props: true,
  },
  {
    path: 'recipe/:recipeId/edit',
    component: () => import('@/pages/EditRecipe.vue'),
    props: true,
  },
];

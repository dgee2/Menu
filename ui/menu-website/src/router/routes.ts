import type { RouteRecordRaw } from 'vue-router';
import { authGuard } from '@auth0/auth0-vue';
import { authenticatedRoutes } from 'src/router/authenticated.routes';
import { publicRoutes } from 'src/router/public.routes';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    children: publicRoutes,
  },
  {
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    children: authenticatedRoutes,
    beforeEnter: authGuard,
  },

  // Always leave this as last one,
  // but you can also remove it
  {
    path: '/:catchAll(.*)*',
    component: () => import('pages/ErrorNotFound.vue'),
  },
];

export default routes;

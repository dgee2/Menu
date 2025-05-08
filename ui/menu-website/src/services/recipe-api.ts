import type { Middleware } from 'openapi-fetch';
import createClient from 'openapi-fetch';
import type { paths, components } from 'src/generated/open-api/menu-api';
import { useAuth } from 'src/services/auth';

const authMiddleware: Middleware = {
  async onRequest({ request }) {
    const auth = useAuth();
    const accessToken = await auth.getAccessToken();
    request.headers.set('Authorization', `Bearer ${accessToken}`);
    return request;
  },
};

const client = createClient<paths>({
  baseUrl: process.env.VITE_MENU_API_URL,
});

client.use(authMiddleware);

export const createRecipe = async (recipe: components['schemas']['NewRecipe']) => {
  const result = await client.POST('/api/recipe', {
    body: recipe,
  });

  return result.data;
};

export const getRecipes = async (): Promise<components['schemas']['Recipe'][] | undefined> => {
  const result = await client.GET('/api/recipe');
  return result.data;
};

export const getRecipe = async (
  recipeId: string,
): Promise<components['schemas']['FullRecipe'] | undefined> => {
  const result = await client.GET('/api/recipe/{recipeId}', {
    params: {
      path: {
        recipeId,
      },
    },
  });
  return result.data;
};

import type { Middleware } from 'openapi-fetch';
import createClient from 'openapi-fetch';
import type { paths, components } from '@/generated/open-api/menu-api';
import { useAuth } from '@/services/auth';

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

export type NewRecipe = components['schemas']['NewRecipe'];
export type Recipe = components['schemas']['Recipe'];
export type FullRecipe = components['schemas']['FullRecipe'];
export type IngredientUnit = components['schemas']['IngredientUnit'];

export const postRecipe = async (recipe: NewRecipe) => {
  const { data, error } = await client.POST('/api/recipe', {
    body: recipe,
  });

  if (error) {
    console.error('Failed to post recipe:', error);
    throw new Error('Failed to post recipe');
  }

  return data;
};

export const putRecipe = async (recipeId: string, recipe: NewRecipe) => {
  const { data, error } = await client.PUT('/api/recipe/{recipeId}', {
    params: {
      path: {
        recipeId: recipeId,
      },
    },
    body: recipe,
  });

  if (error) {
    console.error('Failed to put recipe:', error);
    throw new Error('Failed to put recipe');
  }

  return data;
};

export const getRecipes = async (): Promise<Recipe[]> => {
  const { data, error } = await client.GET('/api/recipe');

  if (error) {
    console.error('Failed to get recipes:', error);
    throw new Error('Failed to get recipes');
  }

  return data;
};

export const getRecipe = async (recipeId: string): Promise<FullRecipe> => {
  const { data, error } = await client.GET('/api/recipe/{recipeId}', {
    params: {
      path: {
        recipeId,
      },
    },
  });

  if (error) {
    console.error('Failed to get recipe:', error);
    throw new Error('Failed to get recipe');
  }

  return data;
};

export const getIngredientUnits = async (): Promise<IngredientUnit[]> => {
  const { data, error } = await client.GET('/api/ingredient/unit', {});

  if (error) {
    console.error('Failed to get ingredient units:', error);
    throw new Error('Failed to get ingredient units');
  }

  return data;
};

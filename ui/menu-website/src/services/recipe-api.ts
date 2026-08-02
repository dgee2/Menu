import type { Middleware } from 'openapi-fetch';
import createClient from 'openapi-fetch';
import type { paths, components } from '@/generated/open-api/menu-api';
import { useAuth } from '@/services/auth';

export type UpsertRecipe = components['schemas']['UpsertRecipe'];
export type RecipeListItem = components['schemas']['RecipeListItem'];
export type RecipeDetail = components['schemas']['RecipeDetail'];
export type IngredientUnit = components['schemas']['IngredientUnit'];

export const useRecipeApi = () => {
  const auth = useAuth();

  const authMiddleware: Middleware = {
    onRequest: async ({ request }) => {
      const accessToken = await auth.getAccessToken();
      request.headers.set('Authorization', `Bearer ${accessToken}`);
      return request;
    },
  };

  const client = createClient<paths>({
    baseUrl: import.meta.env.VITE_MENU_API_URL,
  });

  client.use(authMiddleware);

  const postRecipe = async (recipe: UpsertRecipe) => {
    const { data, error } = await client.POST('/api/recipe', {
      body: recipe,
    });

    if (error) {
      console.error('Failed to post recipe:', error);
      throw new Error('Failed to post recipe');
    }

    return data;
  };

  const putRecipe = async (recipeId: string, recipe: UpsertRecipe) => {
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

  const getRecipes = async (): Promise<RecipeListItem[]> => {
    const { data, error } = await client.GET('/api/recipe');
    if (error) {
      console.error('Failed to get recipes:', error);
      throw new Error('Failed to get recipes');
    }

    return data;
  };

  const getRecipe = async (recipeId: string): Promise<RecipeDetail> => {
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

  const getIngredientUnits = async (): Promise<IngredientUnit[]> => {
    const { data, error } = await client.GET('/api/ingredient/unit', {});

    if (error) {
      console.error('Failed to get ingredient units:', error);
      throw new Error('Failed to get ingredient units');
    }

    return data;
  };
  return {
    postRecipe,
    putRecipe,
    getRecipes,
    getRecipe,
    getIngredientUnits,
  };
};

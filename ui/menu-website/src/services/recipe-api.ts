import type { Middleware } from 'openapi-fetch';
import createClient from 'openapi-fetch';
import type { paths, components } from '@/generated/open-api/menu-api';
import { useAuth } from '@/services/auth';
import { ApiError } from '@/services/api-error';

export type UpsertRecipe = components['schemas']['UpsertRecipe'];
export type RecipeDetail = components['schemas']['RecipeDetail'];
export type RecipeListItem = components['schemas']['RecipeListItem'];
export type RecipeIngredientItem = components['schemas']['RecipeIngredientItem'];
export type RecipeStepItem = components['schemas']['RecipeStepItem'];
export type IngredientUnit = components['schemas']['IngredientUnit'];
export type RecipeListScope = 'mine' | 'authenticated';

/**
 * The values the API accepts for `accessScope`. Generated from the backend's `RecipeAccessScope`
 * enum, so adding a scope on the server is a compile error here rather than a silent mismatch.
 */
export type RecipeAccessScope = components['schemas']['RecipeAccessScope'];

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

  const postRecipe = async (recipe: UpsertRecipe): Promise<RecipeDetail> => {
    const { data, error, response } = await client.POST('/api/recipe', {
      body: recipe,
    });

    if (error) {
      throw ApiError.from('Create recipe', error, response);
    }

    return data;
  };

  const putRecipe = async (recipeId: string, recipe: UpsertRecipe): Promise<RecipeDetail> => {
    const { data, error, response } = await client.PUT('/api/recipe/{recipeId}', {
      params: {
        path: {
          recipeId: recipeId,
        },
      },
      body: recipe,
    });

    if (error) {
      throw ApiError.from('Update recipe', error, response);
    }

    return data;
  };

  const deleteRecipe = async (recipeId: string): Promise<void> => {
    const { error, response } = await client.DELETE('/api/recipe/{recipeId}', {
      params: {
        path: {
          recipeId,
        },
      },
    });

    if (error) {
      throw ApiError.from('Delete recipe', error, response);
    }
  };

  const getRecipes = async (scope: RecipeListScope = 'mine'): Promise<RecipeListItem[]> => {
    const { data, error, response } = await client.GET('/api/recipe', {
      params: {
        query: { scope },
      },
    });

    if (error) {
      throw ApiError.from('List recipes', error, response);
    }

    return data as RecipeListItem[];
  };

  const getRecipe = async (recipeId: string): Promise<RecipeDetail> => {
    const { data, error, response } = await client.GET('/api/recipe/{recipeId}', {
      params: {
        path: {
          recipeId,
        },
      },
    });

    if (error) {
      throw ApiError.from('Get recipe', error, response);
    }

    // The endpoint documents a 401 with no body, so the generated union admits `undefined` on the
    // success branch; reaching here means the request succeeded and the body is there.
    return data as RecipeDetail;
  };

  const getIngredientUnits = async (): Promise<IngredientUnit[]> => {
    const { data, error, response } = await client.GET('/api/ingredient/unit', {});

    if (error) {
      throw ApiError.from('List ingredient units', error, response);
    }

    return data;
  };

  return {
    postRecipe,
    putRecipe,
    deleteRecipe,
    getRecipes,
    getRecipe,
    getIngredientUnits,
  };
};

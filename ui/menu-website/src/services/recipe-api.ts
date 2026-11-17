import {
  createRecipe,
  deleteRecipe,
  getIngredientUnits,
  getRecipe,
  getRecipes,
  restoreRecipe,
  updateRecipe,
} from '@/generated/open-api/menu-api';
import type { UpsertRecipe } from '@/generated/open-api/model';
import { contractRequest } from '@/services/api-fetch';
import { useAuth } from '@/services/auth';

export type {
  IngredientUnit,
  RecipeAccessScope,
  RecipeDetail,
  RecipeIngredientItem,
  RecipeListItem,
  RecipeStepItem,
  UpsertRecipe,
} from '@/generated/open-api/model';

export type RecipeListScope = 'mine' | 'authenticated';

export const useRecipeApi = () => {
  const { getAccessToken } = useAuth();

  return {
    postRecipe: async (recipe: UpsertRecipe) =>
      createRecipe(recipe, contractRequest('createRecipe', getAccessToken, { body: recipe })),

    putRecipe: async (recipeId: string, recipe: UpsertRecipe) =>
      updateRecipe(
        recipeId,
        recipe,
        contractRequest('updateRecipe', getAccessToken, {
          body: recipe,
          path: { recipeId },
        }),
      ),

    deleteRecipe: async (recipeId: string): Promise<void> => {
      await deleteRecipe(
        recipeId,
        contractRequest('deleteRecipe', getAccessToken, { path: { recipeId } }),
      );
    },

    restoreRecipe: async (recipeId: string) =>
      restoreRecipe(
        recipeId,
        contractRequest('restoreRecipe', getAccessToken, { path: { recipeId } }),
      ),

    getRecipes: async (scope: RecipeListScope = 'mine', query?: string) => {
      const params = { scope, ...(query === undefined ? {} : { query }) };
      const options = contractRequest('getRecipes', getAccessToken, { query: params });
      return getRecipes(params, options);
    },

    getRecipe: async (recipeId: string) =>
      getRecipe(recipeId, contractRequest('getRecipe', getAccessToken, { path: { recipeId } })),

    getIngredientUnits: async () =>
      getIngredientUnits(contractRequest('getIngredientUnits', getAccessToken)),
  };
};

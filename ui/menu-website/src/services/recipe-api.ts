import { createMenuApi } from '@/generated/open-api/menu-api';
import { useAuth } from '@/services/auth';
import type { UpsertRecipe } from '@/generated/open-api/model';

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
  const api = createMenuApi(useAuth().getAccessToken);
  return {
    postRecipe: (recipe: UpsertRecipe) => api.createRecipe(recipe),
    putRecipe: api.updateRecipe,
    deleteRecipe: api.deleteRecipe,
    restoreRecipe: api.restoreRecipe,
    getRecipe: api.getRecipe,
    getIngredientUnits: api.getIngredientUnits,
    getRecipes: (scope: RecipeListScope = 'mine', query?: string) =>
      api.getRecipes({ scope, ...(query === undefined ? {} : { query }) }),
  };
};

import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query';
import { type UpsertRecipe, useRecipeApi } from '@/services/recipe-api';
import { toValue, type MaybeRef } from 'vue';

const RECIPE_QUERY_KEY = 'recipe' as const;
const RECIPE_LIST_QUERY_KEY = 'recipe-list' as const;
const INGREDIENT_UNIT_QUERY_KEY = 'ingredient-unit-list' as const;

export const useRecipeService = () => {
  const { getRecipes, getRecipe, getIngredientUnits, postRecipe, putRecipe } = useRecipeApi();
  const queryClient = useQueryClient();
  const recipeListQueryKey = [RECIPE_LIST_QUERY_KEY] as const;
  const recipeDetailQueryKey = (recipeId: string) => [RECIPE_QUERY_KEY, String(recipeId)] as const;

  const invalidateRecipeQueries = async (recipeId: string) => {
    await Promise.allSettled([
      queryClient.invalidateQueries({ queryKey: recipeListQueryKey }),
      queryClient.invalidateQueries({ queryKey: recipeDetailQueryKey(recipeId) }),
    ]);
  };

  // getRecipes/getRecipe are stable for the lifetime of this useRecipeService() call, not
  // reactive query input — including them in the key (a fresh function reference every call)
  // breaks query caching, so the exhaustive-deps rule is intentionally suppressed below.
  const useRecipes = () =>
    // eslint-disable-next-line @tanstack/query/exhaustive-deps
    useQuery({ queryKey: recipeListQueryKey, queryFn: () => getRecipes('mine') });

  const useRecipe = (recipeId: MaybeRef<string>) =>
    // eslint-disable-next-line @tanstack/query/exhaustive-deps
    useQuery({
      queryKey: [RECIPE_QUERY_KEY, recipeId] as const,
      queryFn: () => getRecipe(toValue(recipeId)),
      enabled: () => !!toValue(recipeId),
    });

  const useCreateRecipe = () => {
    return useMutation({
      mutationFn: postRecipe,
      onSuccess: async (data) => invalidateRecipeQueries(data.id.toString()),
    });
  };

  const useUpdateRecipe = () => {
    return useMutation({
      mutationFn: ({ recipeId, recipe }: { recipeId: string; recipe: UpsertRecipe }) =>
        putRecipe(recipeId, recipe),
      onSuccess: async (data) => invalidateRecipeQueries(data.id.toString()),
    });
  };

  const useIngredientUnits = () =>
    useQuery({ queryKey: [INGREDIENT_UNIT_QUERY_KEY], queryFn: getIngredientUnits });

  return {
    useRecipes,
    useRecipe,
    useCreateRecipe,
    useUpdateRecipe,
    useIngredientUnits,
  };
};

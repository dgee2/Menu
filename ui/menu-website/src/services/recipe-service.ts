import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query';
import { type NewRecipe, useRecipeApi } from '@/services/recipe-api';
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

  const useRecipes = () => useQuery({ queryKey: recipeListQueryKey, queryFn: getRecipes });

  const useRecipe = (recipeId: MaybeRef<string>) =>
    useQuery({
      queryKey: [RECIPE_QUERY_KEY, recipeId, getRecipe] as const,
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
      mutationFn: ({ recipeId, recipe }: { recipeId: string; recipe: NewRecipe }) =>
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

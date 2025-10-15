import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query';
import { type NewRecipe, useRecipeApi } from '@/services/recipe-api';
import { toValue, type MaybeRef } from 'vue';

const RECIPE_QUERY_KEY = 'recipe';
const RECIPE_LIST_QUERY_KEY = 'recipe-list';
const INGREDIENT_UNIT_QUERY_KEY = 'ingredient-unit-list';

export const useRecipeService = () => {
  const api = useRecipeApi();
  const queryClient = useQueryClient();

  const useRecipes = () => useQuery({ queryKey: [RECIPE_LIST_QUERY_KEY], queryFn: api.getRecipes });

  const useRecipe = (recipeId: MaybeRef<string>) =>
    useQuery({
      queryKey: [RECIPE_QUERY_KEY, recipeId],
      queryFn: () => api.getRecipe(toValue(recipeId)),
    });

  const useCreateRecipe = () => {
    return useMutation({
      mutationFn: api.postRecipe,
      onSuccess: async (data) => {
        // Invalidate and refetch
        await Promise.allSettled([
          queryClient.invalidateQueries({ queryKey: [RECIPE_LIST_QUERY_KEY] }),
          queryClient.invalidateQueries({ queryKey: [RECIPE_QUERY_KEY, data.id] }),
        ]);
      },
    });
  };

  const useUpdateRecipe = () => {
    return useMutation({
      mutationFn: ({ recipeId, recipe }: { recipeId: string; recipe: NewRecipe }) =>
        api.putRecipe(recipeId, recipe),
      onSuccess: async (data) => {
        // Invalidate and refetch
        await Promise.allSettled([
          queryClient.invalidateQueries({ queryKey: [RECIPE_LIST_QUERY_KEY] }),
          queryClient.invalidateQueries({ queryKey: [RECIPE_QUERY_KEY, data.id] }),
        ]);
      },
    });
  };

  const useIngredientUnits = () =>
    useQuery({ queryKey: [INGREDIENT_UNIT_QUERY_KEY], queryFn: api.getIngredientUnits });

  return {
    useRecipes,
    useRecipe,
    useCreateRecipe,
    useUpdateRecipe,
    useIngredientUnits,
  };
};

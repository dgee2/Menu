import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query';
import type { NewRecipe } from 'src/services/recipe-api';
import { getRecipes, getRecipe, postRecipe, putRecipe } from 'src/services/recipe-api';
import { toValue, type MaybeRef } from 'vue';

const RECIPE_QUERY_KEY = 'recipe';
const RECIPE_LIST_QUERY_KEY = 'recipe';

export const useRecipes = () =>
  useQuery({ queryKey: [RECIPE_LIST_QUERY_KEY], queryFn: getRecipes });

export const useRecipe = (recipeId: MaybeRef<string>) =>
  useQuery({
    queryKey: [RECIPE_QUERY_KEY, recipeId],
    queryFn: () => getRecipe(toValue(recipeId)),
  });

export const useCreateRecipe = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: postRecipe,
    onSuccess: async (data) => {
      // Invalidate and refetch
      await Promise.allSettled([
        queryClient.invalidateQueries({ queryKey: [RECIPE_LIST_QUERY_KEY] }),
        queryClient.invalidateQueries({ queryKey: [RECIPE_QUERY_KEY, data.id] }),
      ]);
    },
  });
};

export const useUpdateRecipe = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ recipeId, recipe }: { recipeId: string; recipe: NewRecipe }) =>
      putRecipe(recipeId, recipe),
    onSuccess: async (data) => {
      // Invalidate and refetch
      await Promise.allSettled([
        queryClient.invalidateQueries({ queryKey: [RECIPE_LIST_QUERY_KEY] }),
        queryClient.invalidateQueries({ queryKey: [RECIPE_QUERY_KEY, data.id] }),
      ]);
    },
  });
};

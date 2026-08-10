import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query';
import {
  type RecipeListScope,
  type UpsertRecipe,
  useRecipeApi,
} from '@/services/recipe-api';
import { toValue, type MaybeRef } from 'vue';

const RECIPE_QUERY_KEY = 'recipe' as const;
const RECIPE_LIST_QUERY_KEY = 'recipe-list' as const;
const INGREDIENT_UNIT_QUERY_KEY = 'ingredient-unit-list' as const;

export const useRecipeService = () => {
  const { getRecipes, getRecipe, getIngredientUnits, postRecipe, putRecipe, deleteRecipe } =
    useRecipeApi();
  const queryClient = useQueryClient();
  const recipeListQueryKey = [RECIPE_LIST_QUERY_KEY] as const;
  const recipeDetailQueryKey = (recipeId: string) => [RECIPE_QUERY_KEY, String(recipeId)] as const;

  const invalidateRecipeQueries = async (recipeId: string) => {
    await Promise.allSettled([
      queryClient.invalidateQueries({ queryKey: recipeListQueryKey }),
      queryClient.invalidateQueries({ queryKey: recipeDetailQueryKey(recipeId) }),
    ]);
  };

  // Scope is part of the key so the two scopes cache separately; the list key stays a prefix of
  // it, which keeps invalidateQueries({ queryKey: recipeListQueryKey }) covering both.
  const useRecipes = (scope: MaybeRef<RecipeListScope> = 'mine') =>
    useQuery({
      queryKey: [...recipeListQueryKey, scope] as const,
      queryFn: () => getRecipes(toValue(scope)),
    });

  const useRecipe = (recipeId: MaybeRef<string>) =>
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

  const useDeleteRecipe = () => {
    return useMutation({
      mutationFn: (recipeId: string) => deleteRecipe(recipeId),
      onSuccess: async (_data, recipeId) => {
        // The detail query is removed rather than invalidated: refetching a deleted recipe would
        // only produce the 404 that the caller is already navigating away from.
        queryClient.removeQueries({ queryKey: recipeDetailQueryKey(recipeId) });
        await queryClient.invalidateQueries({ queryKey: recipeListQueryKey });
      },
    });
  };

  const useIngredientUnits = () =>
    useQuery({ queryKey: [INGREDIENT_UNIT_QUERY_KEY], queryFn: getIngredientUnits });

  return {
    useRecipes,
    useRecipe,
    useCreateRecipe,
    useUpdateRecipe,
    useDeleteRecipe,
    useIngredientUnits,
  };
};

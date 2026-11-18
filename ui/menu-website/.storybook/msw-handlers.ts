import { http, HttpResponse } from 'msw/http';
import { delay } from 'msw/utils/delay';
import {
  getCreateRecipeMockHandler,
  getDeleteRecipeMockHandler,
  getGetIngredientUnitsMockHandler,
  getGetRecipeMockHandler,
  getGetRecipesMockHandler,
  getRestoreRecipeMockHandler,
  getUpdateRecipeMockHandler,
} from '../src/generated/open-api/menu-api.msw';
import type { RecipeDetail, RecipeListItem } from '../src/generated/open-api/model';

const recipePath = '*/api/recipe';
const recipeDetailPath = '*/api/recipe/:recipeId';
const problemHeaders = { 'Content-Type': 'application/problem+json' };

export const sampleRecipeId = '00000000-0000-4000-8000-000000000001';
export const editableRecipeId = '00000000-0000-4000-8000-000000000007';

const sampleRecipeDetail: RecipeDetail = {
  id: sampleRecipeId,
  title: 'Chocolate Cake',
  accessScope: 'Private',
  summary: 'A rich, moist chocolate cake.',
  servings: 8,
  yieldText: 'One 9-inch cake',
  prepTimeMinutes: 20,
  cookTimeMinutes: 35,
  totalTimeMinutes: 55,
  createdAtUtc: '2025-01-01T12:00:00Z',
  updatedAtUtc: '2025-01-01T12:00:00Z',
  canEdit: false,
  canDelete: false,
  ingredients: [
    {
      sortOrder: 0,
      ingredientText: 'Flour',
      measureText: '2 cups',
      sectionTitle: null,
      preparationText: 'sifted',
      isOptional: true,
    },
    {
      sortOrder: 1,
      ingredientText: 'Cocoa Powder',
      measureText: '3/4 cup',
      sectionTitle: null,
      preparationText: null,
      isOptional: true,
    },
    {
      sortOrder: 2,
      ingredientText: 'Salt',
      measureText: '1 pinch',
      sectionTitle: null,
      preparationText: '   ',
      isOptional: false,
    },
  ],
  steps: [
    {
      sortOrder: 0,
      instructionText: 'Preheat the oven to 180C.',
      title: null,
      durationMinutes: null,
    },
    { sortOrder: 1, instructionText: 'Mix dry ingredients.', title: null, durationMinutes: 5 },
  ],
};

export const recipeDetailSuccessHandler = getGetRecipeMockHandler(async () => {
  await delay(150);
  return sampleRecipeDetail;
});

export const recipeDetailNotFoundHandler = http.get(recipeDetailPath, async () => {
  await delay(150);
  return HttpResponse.json(
    { title: 'Not Found', status: 404 },
    { status: 404, headers: problemHeaders },
  );
});

/** The same recipe, with the access flags the API returns to its owner. */
export const recipeDetailEditableHandler = getGetRecipeMockHandler(async () => {
  await delay(150);
  return { ...sampleRecipeDetail, canEdit: true, canDelete: true };
});

export const recipeDetailErrorHandler = http.get(recipeDetailPath, async () => {
  await delay(150);
  return serverError();
});

export const recipeDetailLoadingHandler = getGetRecipeMockHandler(async () => {
  await delay(3000);
  return sampleRecipeDetail;
});

export const recipeDeleteSuccessHandler = getDeleteRecipeMockHandler(async () => {
  await delay(150);
});

export const recipeRestoreSuccessHandler = getRestoreRecipeMockHandler(async () => {
  await delay(150);
  return { ...sampleRecipeDetail, canEdit: true, canDelete: true };
});

export const recipeDeleteErrorHandler = http.delete(recipeDetailPath, async () => {
  await delay(150);
  return serverError();
});

const sampleRecipes: RecipeListItem[] = [
  {
    id: sampleRecipeId,
    title: 'Chocolate Cake',
    summary: 'A rich chocolate cake for celebrations.',
    accessScope: 'AuthenticatedUsers',
    servings: 8,
    effectiveTotalTimeMinutes: 55,
  },
  {
    id: '00000000-0000-4000-8000-000000000002',
    title: 'Tomato Soup',
    summary: 'A smooth tomato soup.',
    accessScope: 'Private',
    servings: 4,
    effectiveTotalTimeMinutes: 35,
  },
];

export const recipesSuccessHandler = getGetRecipesMockHandler(async () => {
  await delay(150);
  return sampleRecipes;
});

export const recipesSearchHandler = getGetRecipesMockHandler(({ request }) => {
  const query = new URL(request.url).searchParams.get('query');
  return query ? sampleRecipes.slice(0, 1) : sampleRecipes;
});

export const recipesEmptyHandler = getGetRecipesMockHandler(async () => {
  await delay(150);
  return [];
});

export const recipesErrorHandler = http.get(recipePath, async () => {
  await delay(150);
  return serverError();
});

export const recipesLoadingHandler = getGetRecipesMockHandler(async () => {
  await delay(3000);
  return sampleRecipes;
});

export const recipeCreateSuccessHandler = getCreateRecipeMockHandler(async () => {
  await delay(150);
  return sampleRecipeDetail;
});

export const recipeCreateErrorHandler = http.post(recipePath, async () => {
  await delay(150);
  return serverError();
});

export const recipeCreateConflictHandler = http.post(recipePath, async () => {
  await delay(150);
  return HttpResponse.json(
    { title: 'Conflict', detail: 'A recipe with this name already exists.', status: 409 },
    { status: 409 },
  );
});

export const recipeUpdateSuccessHandler = getUpdateRecipeMockHandler(async () => {
  await delay(150);
  return { ...sampleRecipeDetail, id: editableRecipeId };
});

export const ingredientUnitsHandler = getGetIngredientUnitsMockHandler([
  { name: 'Gram', abbreviation: 'g', type: 'Mass' },
  { name: 'Millilitre', abbreviation: 'ml', type: 'Volume' },
]);

function serverError() {
  return HttpResponse.json(
    { title: 'Internal Server Error', status: 500 },
    { status: 500, headers: problemHeaders },
  );
}

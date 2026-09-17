import { http, HttpResponse, delay } from 'msw';

const recipePath = '*/api/recipe';
const recipeDetailPath = '*/api/recipe/:recipeId';
const ingredientUnitsPath = '*/api/ingredient/unit';

const sampleRecipeDetail = {
  id: 1,
  title: 'Chocolate Cake',
  accessScope: 'Private',
  summary: 'A rich, moist chocolate cake.',
  servings: 8,
  yieldText: 'One 9-inch cake',
  prepTimeMinutes: 20,
  cookTimeMinutes: 35,
  totalTimeMinutes: 55,
  ingredients: [
    {
      sortOrder: 0,
      ingredientText: 'Flour',
      measureText: '2 cups',
      sectionTitle: null,
      isOptional: false,
    },
    {
      sortOrder: 1,
      ingredientText: 'Cocoa Powder',
      measureText: '3/4 cup',
      sectionTitle: null,
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

export const recipeDetailSuccessHandler = http.get(recipeDetailPath, async () => {
  await delay(150);
  return HttpResponse.json(sampleRecipeDetail);
});

export const recipeDetailNotFoundHandler = http.get(recipeDetailPath, async () => {
  await delay(150);
  return HttpResponse.json({ title: 'Not Found', status: 404 }, { status: 404 });
});

/** The same recipe, with the access flags the API returns to its owner. */
export const recipeDetailEditableHandler = http.get(recipeDetailPath, async () => {
  await delay(150);
  return HttpResponse.json({ ...sampleRecipeDetail, canEdit: true, canDelete: true });
});

export const recipeDetailErrorHandler = http.get(recipeDetailPath, async () => {
  await delay(150);
  return HttpResponse.json({ title: 'Internal Server Error', status: 500 }, { status: 500 });
});

export const recipeDetailLoadingHandler = http.get(recipeDetailPath, async () => {
  await delay(3000);
  return HttpResponse.json(sampleRecipeDetail);
});

export const recipeDeleteSuccessHandler = http.delete(recipeDetailPath, async () => {
  await delay(150);
  return new HttpResponse(null, { status: 204 });
});

export const recipeRestoreSuccessHandler = http.post('*/api/recipe/:recipeId/restore', async () => {
  await delay(150);
  return HttpResponse.json(
    { ...sampleRecipeDetail, canEdit: true, canDelete: true },
    { status: 200 },
  );
});

export const recipeDeleteErrorHandler = http.delete(recipeDetailPath, async () => {
  await delay(150);
  return HttpResponse.json({ title: 'Internal Server Error', status: 500 }, { status: 500 });
});

export const recipesSuccessHandler = http.get(recipePath, async () => {
  await delay(150);
  return HttpResponse.json([
    { id: 1, title: 'Chocolate Cake' },
    { id: 2, title: 'Tomato Soup' },
  ]);
});

export const recipesEmptyHandler = http.get(recipePath, async () => {
  await delay(150);
  return HttpResponse.json([]);
});

export const recipesErrorHandler = http.get(recipePath, async () => {
  await delay(150);
  return HttpResponse.json({ message: 'Server error' }, { status: 500 });
});

export const recipesLoadingHandler = http.get(recipePath, async () => {
  await delay(3000);
  return HttpResponse.json([
    { id: 1, title: 'Chocolate Cake' },
    { id: 2, title: 'Tomato Soup' },
  ]);
});

export const recipeCreateSuccessHandler = http.post(recipePath, async () => {
  await delay(150);
  return HttpResponse.json(sampleRecipeDetail, { status: 200 });
});

export const recipeCreateErrorHandler = http.post(recipePath, async () => {
  await delay(150);
  return HttpResponse.json({ title: 'Internal Server Error', status: 500 }, { status: 500 });
});

export const recipeCreateConflictHandler = http.post(recipePath, async () => {
  await delay(150);
  return HttpResponse.json(
    { title: 'Conflict', detail: 'A recipe with this name already exists.', status: 409 },
    { status: 409 },
  );
});

export const recipeUpdateSuccessHandler = http.put(recipeDetailPath, async () => {
  await delay(150);
  return HttpResponse.json({ ...sampleRecipeDetail, id: 7 }, { status: 200 });
});

export const ingredientUnitsHandler = http.get(ingredientUnitsPath, () => {
  return HttpResponse.json([
    { id: 1, name: 'g' },
    { id: 2, name: 'ml' },
  ]);
});

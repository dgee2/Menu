import { http, HttpResponse, delay } from 'msw';

const recipePath = '*/api/recipe';
const ingredientUnitsPath = '*/api/ingredient/unit';

export const recipesSuccessHandler = http.get(recipePath, async () => {
  await delay(150);
  return HttpResponse.json([
    { id: '1', name: 'Chocolate Cake' },
    { id: '2', name: 'Tomato Soup' },
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
    { id: '1', name: 'Chocolate Cake' },
    { id: '2', name: 'Tomato Soup' },
  ]);
});

export const ingredientUnitsHandler = http.get(ingredientUnitsPath, () => {
  return HttpResponse.json([
    { id: 1, name: 'g' },
    { id: 2, name: 'ml' },
  ]);
});


import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query';
import { enableAutoUnmount, mount } from '@vue/test-utils';
import { HttpResponse, http } from 'msw';
import { setupServer } from 'msw/node';
import { afterAll, afterEach, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest';
import { ref } from 'vue';
import { createMenuApi } from '@/generated/open-api/menu-api';
import { getGetIngredientUnitsMockHandler } from '@/generated/open-api/menu-api.msw';
import {
  ApiContractError,
  ApiError,
  shouldRetryQuery,
  userFacingMessage,
} from '@/services/api-error';

import { useRecipeApi, type RecipeListScope, type UpsertRecipe } from '@/services/recipe-api';
import { useRecipeService } from '@/services/recipe-service';

const auth = vi.hoisted(() => ({ getAccessToken: vi.fn<() => Promise<string>>() }));
vi.mock('@/services/auth', () => ({ useAuth: () => auth }));

const baseUrl = 'https://menu.test';
const recipeUrl = `${baseUrl}/api/recipe`;
const recipeId = '00000000-0000-4000-8000-000000000009';
const recipe = {
  title: '  Leek and potato soup  ',
  accessScope: 'Private',
  ingredients: [{ ingredientText: 'Leek', measureText: '2', sortOrder: 0, isOptional: false }],
  steps: [{ instructionText: 'Cook gently', sortOrder: 0 }],
  summary: null,
  servings: 0,
} satisfies UpsertRecipe;
const detail = {
  ...recipe,
  id: recipeId,
  canEdit: true,
  canDelete: true,
  createdAtUtc: '2025-01-01T12:00:00Z',
  updatedAtUtc: '2025-01-01T12:00:00Z',
};
const problemJson = (body: unknown, status: number, mediaType = 'application/problem+json') =>
  HttpResponse.text(JSON.stringify(body), { status, headers: { 'Content-Type': mediaType } });
const server = setupServer();

enableAutoUnmount(afterEach);
beforeAll(() => server.listen({ onUnhandledFrame: 'error' }));
afterAll(() => server.close());

beforeEach(() => {
  vi.stubEnv('VITE_MENU_API_URL', `${baseUrl}/`);
  auth.getAccessToken.mockReset().mockResolvedValue('test-token');
});

const queryClient = new QueryClient({
  defaultOptions: { queries: { retry: shouldRetryQuery, retryDelay: 0, staleTime: Infinity } },
});

afterEach(() => {
  server.resetHandlers();
  queryClient.clear();
  vi.unstubAllEnvs();
});

const mountService = <T>(setup: (service: ReturnType<typeof useRecipeService>) => T): T => {
  let result!: T;
  mount(
    {
      setup() {
        result = setup(useRecipeService());
        return () => null;
      },
    },
    { global: { plugins: [[VueQueryPlugin, { queryClient }]] } },
  );
  return result;
};

describe('recipe API contract', () => {
  it('acquires a bearer token for each concurrent request and retains original body values', async () => {
    const observed = new Map<string, string | null>();
    auth.getAccessToken.mockResolvedValueOnce('create-token').mockResolvedValueOnce('detail-token');
    const received = vi.fn<(body: unknown) => void>();
    server.use(
      http.post(recipeUrl, async ({ request }) => {
        observed.set('create', request.headers.get('authorization'));
        received(await request.json());
        return HttpResponse.json({ ...detail, extra: 'discard' });
      }),
      http.get(`${recipeUrl}/${recipeId}`, ({ request }) => {
        observed.set('detail', request.headers.get('authorization'));
        return HttpResponse.json(detail);
      }),
    );

    const api = useRecipeApi();
    const [created, fetched] = await Promise.all([api.postRecipe(recipe), api.getRecipe(recipeId)]);

    expect(observed).toEqual(
      new Map([
        ['create', 'Bearer create-token'],
        ['detail', 'Bearer detail-token'],
      ]),
    );
    expect(received).toHaveBeenCalledExactlyOnceWith(recipe);
    expect(created).toEqual(detail);
    expect(fetched).toEqual(detail);
  });

  it('rejects invalid nested bodies, path IDs and query inputs before token acquisition', async () => {
    const api = useRecipeApi();
    const invalidRows = recipe.ingredients.map((row) => ({ ...row, ingredientText: null }));
    await expect(
      api.postRecipe({ ...recipe, ingredients: invalidRows } as unknown as UpsertRecipe),
    ).rejects.toMatchObject({ name: 'ApiContractError', reason: 'request body' });
    await expect(api.getRecipe('not-a-recipe-id')).rejects.toMatchObject({
      name: 'ApiContractError',
      reason: 'request path',
    });
    await expect(api.getRecipes('wrong' as RecipeListScope)).rejects.toMatchObject({
      name: 'ApiContractError',
      reason: 'request query',
    });
    expect(auth.getAccessToken).not.toHaveBeenCalled();
  });

  it('does not dispatch when token acquisition fails', async () => {
    const network = vi.spyOn(globalThis, 'fetch');
    try {
      const error = new Error('login_required');
      auth.getAccessToken.mockRejectedValueOnce(error);
      await expect(useRecipeApi().getRecipe(recipeId)).rejects.toBe(error);
      expect(network).not.toHaveBeenCalled();
    } finally {
      network.mockRestore();
    }
  });

  it('validates actual arguments for every generated request before authentication or dispatch', async () => {
    const api = createMenuApi(auth.getAccessToken);
    const network = vi.spyOn(globalThis, 'fetch');
    try {
      await expect(api.getRecipe('not-a-recipe-id')).rejects.toBeInstanceOf(ApiContractError);
      await expect(api.getRecipes({ scope: 'wrong' })).rejects.toBeInstanceOf(ApiContractError);
      await expect(
        api.createRecipe({ ...recipe, title: null } as unknown as UpsertRecipe),
      ).rejects.toBeInstanceOf(ApiContractError);
      expect(auth.getAccessToken).not.toHaveBeenCalled();
      expect(network).not.toHaveBeenCalled();
    } finally {
      network.mockRestore();
    }
  });
  it('sends search text as a query value and loads units with the generated MSW handler', async () => {
    const requested = vi.fn<(params: URLSearchParams) => void>();
    const units = [{ name: 'gram', abbreviation: 'g', type: 'Mass' }];
    server.use(
      http.get(recipeUrl, ({ request }) => {
        requested(new URL(request.url).searchParams);
        return HttpResponse.json([]);
      }),
      getGetIngredientUnitsMockHandler(units),
    );
    const service = mountService((api) => ({
      search: api.useRecipeSearch('authenticated', 'thyme & garlic'),
      units: api.useIngredientUnits(),
    }));

    await vi.waitFor(() => expect(service.search.data.value).toEqual([]));
    await vi.waitFor(() => expect(service.units.data.value).toEqual(units));
    expect(requested).toHaveBeenCalledExactlyOnceWith(
      new URLSearchParams({ scope: 'authenticated', query: 'thyme & garlic' }),
    );
  });

  it('preserves validation messages, bodyless HTTP errors and valid conflicts', async () => {
    const api = useRecipeApi();
    server.use(
      http.get(`${recipeUrl}/${recipeId}`, () => new HttpResponse(null, { status: 401 })),
      http.post(recipeUrl, () =>
        problemJson({ errors: { Title: ['Choose a title.'] }, status: 400 }, 400),
      ),
    );
    await expect(api.getRecipe(recipeId)).rejects.toMatchObject({ name: 'ApiError', status: 401 });
    await expect(api.postRecipe(recipe)).rejects.toMatchObject({
      status: 400,
      detail: 'Choose a title.',
      validationErrors: { Title: ['Choose a title.'] },
    });

    server.use(
      http.post(recipeUrl, () =>
        problemJson({ status: 409, detail: 'Already exists.' }, 409, 'application/json'),
      ),
    );
    await expect(api.postRecipe(recipe)).rejects.toMatchObject({
      status: 409,
      isConflict: true,
      detail: 'Already exists.',
    });
  });

  it('keeps malformed conflict status without retaining or displaying untrusted details', async () => {
    const untrusted = 'untrusted-response-content';
    server.use(
      http.post(recipeUrl, () => problemJson({ detail: [untrusted] }, 409, 'application/json')),
    );
    const logError = vi.spyOn(console, 'error');
    const logWarning = vi.spyOn(console, 'warn');
    try {
      const error: unknown = await useRecipeApi()
        .postRecipe(recipe)
        .catch((error: unknown) => error);
      expect(error).toBeInstanceOf(ApiContractError);
      expect(error).toMatchObject({ status: 409, reason: 'response body' });
      expect(error).not.toHaveProperty('problem');
      expect(JSON.stringify(error)).not.toContain(untrusted);
      expect(userFacingMessage(error, 'Failed to save recipe.')).toBe('Failed to save recipe.');
      expect(logError).not.toHaveBeenCalled();
      expect(logWarning).not.toHaveBeenCalled();
    } finally {
      logError.mockRestore();
      logWarning.mockRestore();
    }
  });

  it.each([
    ['status', HttpResponse.json(detail, { status: 202 }), 202],
    ['media type', HttpResponse.text(JSON.stringify(detail)), 200],
    ['media type', problemJson(detail, 200, 'constructor'), 200],
    [
      'response body',
      HttpResponse.text('{', { headers: { 'Content-Type': 'application/json' } }),
      200,
    ],
    ['response body', HttpResponse.json({ ...detail, accessScope: 'Unknown' }), 200],
  ])('rejects an invalid response %s', async (reason, response, status) => {
    server.use(http.get(`${recipeUrl}/${recipeId}`, () => response));
    await expect(useRecipeApi().getRecipe(recipeId)).rejects.toMatchObject({
      name: 'ApiContractError',
      reason,
      status,
    });
  });
});

describe('recipe service through Vue Query', () => {
  it('loads migrated recipes with empty ingredient and instruction text', async () => {
    const legacyDetail = {
      ...detail,
      ingredients: [{ ...detail.ingredients[0], ingredientText: '', measureText: '' }],
      steps: [{ ...detail.steps[0], instructionText: '' }],
    };
    server.use(http.get(`${recipeUrl}/${recipeId}`, () => HttpResponse.json(legacyDetail)));

    const details = mountService((api) => api.useRecipe(recipeId));

    await vi.waitFor(() => expect(details.data.value).toEqual(legacyDetail));
  });

  it('caches reactive scopes separately and refreshes mutations without retaining deleted details', async () => {
    const scopes: string[] = [];
    const listItem = { id: recipeId, title: detail.title, accessScope: detail.accessScope };
    server.use(
      http.get(recipeUrl, ({ request }) => {
        const scope = new URL(request.url).searchParams.get('scope') ?? '';
        scopes.push(scope);
        return HttpResponse.json(scope === 'mine' ? [listItem] : []);
      }),
      http.get(`${recipeUrl}/${recipeId}`, () => HttpResponse.json(detail)),
      http.post(recipeUrl, () => HttpResponse.json(detail)),
      http.put(`${recipeUrl}/${recipeId}`, () => HttpResponse.json(detail)),
      http.delete(`${recipeUrl}/${recipeId}`, () => new HttpResponse(null, { status: 204 })),
      http.post(`${recipeUrl}/${recipeId}/restore`, () => HttpResponse.json(detail)),
    );
    const scope = ref<RecipeListScope>('mine');
    const service = mountService((api) => ({
      list: api.useRecipes(scope),
      details: api.useRecipe(recipeId),
      create: api.useCreateRecipe(),
      update: api.useUpdateRecipe(),
      remove: api.useDeleteRecipe(),
      restore: api.useRestoreRecipe(),
    }));

    await vi.waitFor(() => expect(service.list.data.value).toEqual([listItem]));
    await vi.waitFor(() => expect(service.details.data.value).toEqual(detail));
    scope.value = 'authenticated';
    await vi.waitFor(() => expect(service.list.data.value).toEqual([]));
    expect(queryClient.getQueryData(['recipe-list', 'mine', undefined])).toEqual([listItem]);
    expect(queryClient.getQueryData(['recipe-list', 'authenticated', undefined])).toEqual([]);

    await expect(service.create.mutateAsync(recipe)).resolves.toEqual(detail);
    await expect(service.update.mutateAsync({ recipeId, recipe })).resolves.toEqual(detail);
    await expect(service.remove.mutateAsync(recipeId)).resolves.toBeUndefined();
    expect(queryClient.getQueryData(['recipe', recipeId])).toBeUndefined();
    await expect(service.restore.mutateAsync(recipeId)).resolves.toEqual(detail);
    expect(scopes).toEqual(['mine', ...Array<string>(5).fill('authenticated')]);
  });

  it('does not cache or retry invalid responses and still retries a documented server failure', async () => {
    const invalid = vi.fn(() => HttpResponse.json([{ id: recipeId }]));
    const unavailable = vi.fn(() => problemJson({ status: 500 }, 500));
    server.use(http.get(recipeUrl, invalid), http.get(`${recipeUrl}/${recipeId}`, unavailable));
    const { list, details } = mountService((api) => ({
      list: api.useRecipes(),
      details: api.useRecipe(recipeId),
    }));

    await vi.waitFor(() => expect(list.error.value).toBeInstanceOf(ApiContractError));
    expect(invalid).toHaveBeenCalledTimes(1);
    expect(queryClient.getQueryData(['recipe-list', 'mine', undefined])).toBeUndefined();
    await vi.waitFor(() => expect(details.error.value).toBeInstanceOf(ApiError));
    expect(unavailable).toHaveBeenCalledTimes(4);
  });
});

import type { RecipeAccessScope } from '@/services/recipe-api';

/**
 * Display labels for recipe visibility, shared by the editor's visibility field and the detail
 * page's badge.
 *
 * Deliberately client-side. A `DisplayName` column on the lookup table would be one language only,
 * would need an endpoint serving a two-item list, and would give a natural-looking reason to expose
 * the lookup row — and with it its id — over the API.
 */
export const recipeAccessScopeLabels: Record<RecipeAccessScope, string> = {
  Private: 'Private',
  AuthenticatedUsers: 'Visible to all Menu users',
};

/** The short form used where space is tight, such as the detail page badge. */
export const recipeAccessScopeBadgeLabels: Record<RecipeAccessScope, string> = {
  Private: 'Private',
  AuthenticatedUsers: 'Shared',
};

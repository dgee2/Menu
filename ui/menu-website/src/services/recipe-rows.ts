import type { RecipeIngredientItem, RecipeStepItem } from '@/services/recipe-api';

/**
 * A row the user has not touched. Create mode seeds one blank ingredient and one blank step so the
 * editor opens with somewhere to type; those rows must neither block validation nor be saved.
 *
 * The same predicate does both jobs — the row editors relax their required-field rules while it
 * holds, and the payload builder drops the row entirely — so a row can never be simultaneously
 * "valid because blank" and "submitted anyway".
 */

const isEmptyText = (value: string | null | undefined) => !value?.trim();

export const isBlankIngredientRow = (row: Partial<RecipeIngredientItem>): boolean =>
  isEmptyText(row.ingredientText) &&
  isEmptyText(row.measureText) &&
  isEmptyText(row.preparationText) &&
  isEmptyText(row.sectionTitle) &&
  !row.isOptional;

export const isBlankStepRow = (row: Partial<RecipeStepItem>): boolean =>
  isEmptyText(row.instructionText) && isEmptyText(row.title) && row.durationMinutes == null;

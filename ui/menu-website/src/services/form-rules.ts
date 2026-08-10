import type { ValidationRule } from 'quasar';

/**
 * The validation rules shared across the recipe form and its row editors. Kept in one place
 * because the same four "X is required" and whole-number/range pairs were previously written out
 * per field, and a rule that exists in four copies drifts in three of them.
 */

/** Requires non-whitespace text. Takes the whole message, since not every field reads "X is". */
export const requiredText = (message: string): ValidationRule<string | null> => (val) =>
  !!val?.trim() || message;

/**
 * Requires non-whitespace text *unless* the row is blank.
 *
 * A form that seeds an empty row must not immediately fail validation on it, and pruning blank rows
 * at submit time does not work: Quasar's `@submit` only fires once validation has passed, so the
 * blank row has already failed by then. Making the rule itself conditional is what preserves
 * "a recipe with zero ingredients and zero steps is valid".
 */
export const requiredTextUnlessRowIsBlank =
  (message: string, rowIsBlank: () => boolean): ValidationRule<string | null> =>
  (val) =>
    rowIsBlank() || !!val?.trim() || message;

/** Rejects a non-integer. Applied only when a value is present; empty means "not given". */
export const wholeNumber: ValidationRule<number | null> = (val) =>
  val == null || Number.isInteger(val) || 'Must be a whole number';

/** Rejects a value below `min`. */
export const atLeast = (min: number): ValidationRule<number | null> => (val) =>
  val == null || val >= min || `Must be ${min} or greater`;

/** Rejects a value at or below `exclusiveMin`. */
export const greaterThan =
  (exclusiveMin: number): ValidationRule<number | null> =>
  (val) =>
    val == null || val > exclusiveMin || `Must be greater than ${exclusiveMin}`;

/** A whole number of zero or more — servings and the recipe-level time fields. */
export const nonNegativeIntegerRules: ValidationRule<number | null>[] = [wholeNumber, atLeast(0)];

/** A whole number greater than zero — a step that takes no time is a step with no duration. */
export const positiveIntegerRules: ValidationRule<number | null>[] = [wholeNumber, greaterThan(0)];

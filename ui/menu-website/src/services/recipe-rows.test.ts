import { describe, expect, it } from 'vitest';
import { isBlankIngredientRow, isBlankStepRow } from './recipe-rows';

describe('isBlankIngredientRow', () => {
  it('treats a seeded empty row as blank', () => {
    expect(
      isBlankIngredientRow({
        ingredientText: '',
        measureText: '',
        sectionTitle: null,
        preparationText: null,
        isOptional: false,
      }),
    ).toBe(true);
  });

  it('treats whitespace as empty', () => {
    expect(isBlankIngredientRow({ ingredientText: '   ', measureText: '\t' })).toBe(true);
  });

  it.each([
    ['ingredientText', { ingredientText: 'Flour' }],
    ['measureText', { measureText: '200g' }],
    ['preparationText', { preparationText: 'sifted' }],
    ['sectionTitle', { sectionTitle: 'For the sauce' }],
    ['isOptional', { isOptional: true }],
  ])('treats a row with %s as touched', (_field, row) => {
    expect(isBlankIngredientRow(row)).toBe(false);
  });

  it.each([
    ['amount', { amount: 2 }],
    ['unitText', { unitText: 'cup' }],
    ['canonicalIngredientId', { canonicalIngredientId: 17 }],
    ['canonicalUnitId', { canonicalUnitId: 4 }],
  ])('does not call a row carrying %s blank', (_field, row) => {
    // The form does not expose these, but a row loaded for editing can carry them. Calling such a
    // row blank would drop it from the payload, and the repository replaces the whole collection -
    // so the ingredient would be deleted by an edit that never touched it.
    expect(isBlankIngredientRow(row)).toBe(false);
  });
});

describe('isBlankStepRow', () => {
  it('treats a seeded empty row as blank', () => {
    expect(isBlankStepRow({ instructionText: '', title: null, durationMinutes: null })).toBe(true);
  });

  it.each([
    ['instructionText', { instructionText: 'Mix' }],
    ['title', { title: 'Preheat' }],
    ['durationMinutes', { durationMinutes: 10 }],
  ])('treats a row with %s as touched', (_field, row) => {
    expect(isBlankStepRow(row)).toBe(false);
  });
});

---
name: "canonical-reference-row-reuse"
description: "Reuse existing canonical ingredient rows by normalized definition before inserting new reference data"
domain: "backend"
confidence: "high"
source: "earned"
---

## Context
Use this pattern when a Menu backend write creates reference data that should have one canonical stored definition, and future writes resolve that data by a business key rather than by surrogate ID.

## Patterns
- Normalize request collections before comparison so equivalent payloads match regardless of ordering or repeated IDs.
- Query by the canonical business key (`Ingredient.Name`) before insert and compare the effective definition (`UnitIds` set) against existing rows.
- Reuse the existing row when the normalized definitions match; return the existing API shape so the behavior stays client-transparent for equivalent requests.
- Reject mismatched redefinitions before insert so later schema constraints can rely on a single persisted definition per canonical key.

## Examples
```csharp
var normalizedUnitIds = newIngredient.UnitIds.Distinct().ToList();
var existingEquivalentIngredient = existingIngredients
    .FirstOrDefault(i => i.IngredientUnits.Select(iu => iu.UnitId).ToHashSet().SetEquals(normalizedUnitIds));

if (existingEquivalentIngredient is not null)
{
    return MapIngredient(existingEquivalentIngredient);
}
```

## Anti-Patterns
- Comparing raw request lists without deduplicating or ignoring order first.
- Inserting a second reference row with the same canonical name and hoping later code will "pick the right one".
- Reusing an existing canonical row when the incoming definition differs in a business-significant way.

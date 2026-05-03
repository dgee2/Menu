---
name: "set-like-write-dedupe"
description: "Normalize exact duplicates for set-like backend writes before child or junction table persistence"
domain: "backend"
confidence: "high"
source: "earned"
---

## Context
Use this pattern when a Menu backend request carries a set-like collection whose duplicates add no business meaning, but the persistence layer writes child rows or junction-table links that would otherwise attempt duplicate inserts.

## Patterns
- Normalize duplicate-equivalent request items in the service layer before calling repositories so the request becomes idempotent without changing the API contract.
- Repeat the dedupe defensively in repository write paths before creating child/link entities; repository methods should stay safe even if a future caller skips the service.
- For `RecipeIngredient` inputs, only collapse **exact** duplicates (`IngredientName`, `UnitName`, and `Amount` all match). Leave conflicting duplicates to explicit validation behavior.
- For junction-table ID collections like `UnitIds`, deduplicate by the identifier value before materializing `IngredientUnitEntity` rows.

## Examples
```csharp
var normalizedIngredient = new NewIngredient
{
    Name = newIngredient.Name,
    UnitIds = [.. newIngredient.UnitIds.Distinct()],
};

var ingredients = [.. ViewModelMapper.Map(newRecipe.Ingredients).Distinct()];
```

## Anti-Patterns
- Deduplicating only at the database boundary and leaving service calls free to send unstable duplicate payloads downstream.
- Collapsing conflicting duplicates that should stay available for a later validation or conflict response.
- Assuming composite primary keys are enough; they block persisted duplicates but do not stop duplicate insert attempts from request payloads.

CREATE PROCEDURE [dbo].[UpsertRecipeIngredients]
	@recipeId int,
	@tvpIngredients dbo.RecipeIngredientType READONLY
AS
	MERGE RecipeIngredient AS TARGET
	USING @tvpIngredients AS SOURCE
	ON (TARGET.RecipeId = @recipeId AND TARGET.IngredientId = SOURCE.IngredientId AND TARGET.UnitId = SOURCE.UnitId)

	WHEN MATCHED AND TARGET.Amount <> SOURCE.Amount
	THEN UPDATE SET TARGET.Amount = SOURCE.Amount

	WHEN NOT MATCHED BY TARGET
	THEN INSERT (RecipeId, IngredientId, UnitId, Amount) VALUES (@recipeId, SOURCE.IngredientId, SOURCE.UnitId, SOURCE.Amount)

	WHEN NOT MATCHED BY SOURCE AND TARGET.RecipeId = @recipeId
	THEN DELETE ;
RETURN 0

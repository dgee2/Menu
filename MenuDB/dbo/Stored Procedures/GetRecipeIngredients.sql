CREATE PROCEDURE [dbo].[GetRecipeIngredients]
	@recipeId int
AS
	SELECT I.Name AS IngredientName, RI.Amount, U.Name AS UnitName, U.Abbreviation AS UnitAbbreviation
	FROM RecipeIngredient RI
		INNER JOIN Ingredient I ON RI.IngredientId = I.Id
		INNER JOIN Unit U ON RI.UnitId = U.Id
	WHERE RI.RecipeId = @recipeId

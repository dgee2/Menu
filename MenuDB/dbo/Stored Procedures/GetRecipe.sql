CREATE PROCEDURE [dbo].[GetRecipe](@recipeId int)
AS
	SELECT TOP 1 R.Id, R.Name
	FROM Recipe R
	WHERE R.Id = @recipeId

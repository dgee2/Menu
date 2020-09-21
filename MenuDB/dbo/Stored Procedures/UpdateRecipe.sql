CREATE PROCEDURE [dbo].[UpdateRecipe]
	@recipeId INT,
	@name varchar(500)
AS
BEGIN
	UPDATE Recipe
	SET Name = @name
	WHERE Id = @recipeId
END

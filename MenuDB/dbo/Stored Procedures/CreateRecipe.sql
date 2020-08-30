CREATE PROCEDURE [dbo].[CreateRecipe]
	@name varchar(500)
AS
BEGIN
	INSERT INTO Recipe(Name)
	VALUES(@name)

	SELECT SCOPE_IDENTITY() AS id
END
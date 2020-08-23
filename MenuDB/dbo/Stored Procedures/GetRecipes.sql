CREATE PROCEDURE [dbo].[GetRecipes]
AS
	SELECT R.Id, R.Name
	FROM Recipe R

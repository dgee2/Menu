CREATE PROCEDURE GetIngredientUnits
AS
SELECT
	U.Name,
	U.Abbreviation
FROM Units
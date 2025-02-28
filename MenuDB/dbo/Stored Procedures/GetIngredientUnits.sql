CREATE PROCEDURE GetIngredientUnits
AS
SELECT
	U.Name,
	U.Abbreviation,
	UT.Name AS UnitType
FROM Unit U INNER JOIN UnitType UT ON U.UnitTypeId = UT.Id
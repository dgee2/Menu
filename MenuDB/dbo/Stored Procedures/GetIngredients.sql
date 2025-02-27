CREATE PROCEDURE [dbo].[GetIngredients]
AS
SELECT I.Id, I.Name, U.Name AS Unit, U.Abbreviation AS UnitAbbreviation, UT.Name AS UnitType
FROM dbo.Ingredient I
    INNER JOIN IngredientUnit IU ON I.Id = IU.IngredientId
    INNER JOIN Unit U ON U.Id = IU.UnitId
    INNER JOIN UnitType UT ON U.UnitTypeId = UT.Id

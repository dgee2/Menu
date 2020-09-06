CREATE TYPE [dbo].[RecipeIngredientType] AS TABLE
(
	[IngredientId] INT NOT NULL,
	[UnitId] INT NOT NULL,
	[Amount] DECIMAL(10, 4) NOT NULL
)

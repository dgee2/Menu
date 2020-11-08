CREATE TYPE [dbo].[RecipeIngredientType] AS TABLE
(
	[Name] VARCHAR(50) NOT NULL,
	[Unit] VARCHAR(50) NOT NULL,
	[Amount] DECIMAL(10, 4) NOT NULL
)

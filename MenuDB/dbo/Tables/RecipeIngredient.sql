CREATE TABLE [dbo].[RecipeIngredient]
(
	[RecipeId] INT NOT NULL,
	[IngredientId] INT NOT NULL,
	[UnitId] INT NOT NULL,
	[Amount] DECIMAL(10, 4) NOT NULL,
	PRIMARY KEY([RecipeId], [IngredientId], [UnitId]),
	CONSTRAINT [FK_RecipeIngredient_ToRecipe] FOREIGN KEY ([RecipeId]) REFERENCES [Recipe]([Id]),
	CONSTRAINT [FK_RecipeIngredient_ToIngredientUnit] FOREIGN KEY ([IngredientId], [UnitId]) REFERENCES [IngredientUnit]([IngredientId],[UnitId])
)

CREATE TABLE [dbo].[IngredientUnit]
(
	[IngredientId] INT NOT NULL,
	[UnitId] INT NOT NULL,
	PRIMARY KEY ([IngredientId], [UnitId]),
	CONSTRAINT [FK_IngredientUnit_ToIngredient] FOREIGN KEY ([IngredientId]) REFERENCES [Ingredient]([Id]),
	CONSTRAINT [FK_IngredientUnit_ToUnit] FOREIGN KEY ([UnitId]) REFERENCES [Unit]([Id])
)

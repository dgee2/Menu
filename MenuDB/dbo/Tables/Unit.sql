CREATE TABLE [dbo].[Unit]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[Name] VARCHAR(50) NOT NULL,
	[Abbreviation] VARCHAR(5) NULL,
	[UnitTypeId] INT NOT NULL,
	CONSTRAINT [FK_Unit_ToUnitType] FOREIGN KEY ([UnitTypeId]) REFERENCES [UnitType]([Id])
)

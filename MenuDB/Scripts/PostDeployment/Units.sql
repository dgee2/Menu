﻿MERGE dbo.UnitType AS target
USING (VALUES
    (1, 'Volume')
    ,(2, 'Quantity')
    ,(3, 'Weight'))
       AS Source (Id, Name)
ON Target.Id = Source.Id
WHEN MATCHED THEN
UPDATE SET Name = Source.Name
WHEN NOT MATCHED BY TARGET THEN
INSERT (Id, Name) VALUES (Id, Name);


MERGE dbo.Unit AS target
USING (VALUES
    (1, 'Millilitres','ml',1)
    ,(2, 'Litres','l',1)
    ,(3, 'Quantity',NULL,2)
    ,(4, 'Grams', 'g', 3)
    ,(5, 'Kilograms', 'kg', 3)
) AS Source (Id, Name, Abbreviation, UnitTypeId)
ON Target.Id = Source.Id
WHEN MATCHED THEN
UPDATE SET Name = Source.Name,
    Abbreviation = Source.Abbreviation,
    UnitTypeId = Source.UnitTypeId
WHEN NOT MATCHED BY TARGET THEN
INSERT (Id, Name, Abbreviation, UnitTypeId) VALUES (Id, Name, Abbreviation, UnitTypeId);
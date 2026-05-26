USE SberbankDB;
GO

UPDATE BankProducts
SET Rate = CASE Name
        WHEN N'Дебетовая карта СберКарта' THEN 3.00
        WHEN N'Кредитная карта 120 дней' THEN 29.90
        WHEN N'Потребительский кредит' THEN 18.90
        WHEN N'Вклад СберВклад' THEN 14.50
        WHEN N'Расчетный счет для бизнеса' THEN 1.50
        ELSE Rate
    END,
    ServiceCost = CASE Name
        WHEN N'Дебетовая карта СберКарта' THEN 150.00
        WHEN N'Кредитная карта 120 дней' THEN 150000.00
        WHEN N'Потребительский кредит' THEN 300000.00
        WHEN N'Вклад СберВклад' THEN 10000.00
        WHEN N'Расчетный счет для бизнеса' THEN 990.00
        ELSE ServiceCost
    END
WHERE Name IN
(
    N'Дебетовая карта СберКарта',
    N'Кредитная карта 120 дней',
    N'Потребительский кредит',
    N'Вклад СберВклад',
    N'Расчетный счет для бизнеса'
);
GO

-- Fallback for databases created from SberbankDB_SchemaAndData.sql, where
-- ProductId values are assigned by the seed INSERT order.
UPDATE BankProducts
SET Rate = CASE ProductId
        WHEN 1 THEN 3.00
        WHEN 2 THEN 29.90
        WHEN 3 THEN 18.90
        WHEN 4 THEN 14.50
        WHEN 6 THEN 1.50
        ELSE Rate
    END,
    ServiceCost = CASE ProductId
        WHEN 1 THEN 150.00
        WHEN 2 THEN 150000.00
        WHEN 3 THEN 300000.00
        WHEN 4 THEN 10000.00
        WHEN 6 THEN 990.00
        ELSE ServiceCost
    END
WHERE ProductId IN (1, 2, 3, 4, 6);
GO

-- Создаем временную таблицу для хранения соответствий старых и новых ID
CREATE TABLE #OrderIdMapping (
    OldId UNIQUEIDENTIFIER,
    NewId NVARCHAR(10)
);

-- Генерируем новые ID для существующих заказов
DECLARE @counter INT = 1;
DECLARE @oldId UNIQUEIDENTIFIER;
DECLARE @newId NVARCHAR(10);

DECLARE order_cursor CURSOR FOR 
SELECT Id FROM Orders;

OPEN order_cursor;
FETCH NEXT FROM order_cursor INTO @oldId;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Генерируем новый ID в формате AB1234
    SET @newId = CHAR(65 + (@counter % 26)) + CHAR(65 + ((@counter / 26) % 26)) + RIGHT('0000' + CAST(1000 + (@counter % 9000) AS VARCHAR), 4);
    
    INSERT INTO #OrderIdMapping (OldId, NewId) VALUES (@oldId, @newId);
    
    SET @counter = @counter + 1;
    FETCH NEXT FROM order_cursor INTO @oldId;
END;

CLOSE order_cursor;
DEALLOCATE order_cursor;

-- Отключаем проверку внешних ключей
ALTER TABLE OrderDetails NOCHECK CONSTRAINT ALL;

-- Добавляем новый столбец для строкового ID
ALTER TABLE Orders ADD NewId NVARCHAR(10);

-- Обновляем новый столбец значениями из маппинга
UPDATE o 
SET NewId = m.NewId
FROM Orders o
INNER JOIN #OrderIdMapping m ON o.Id = m.OldId;

-- Добавляем новый столбец в OrderDetails
ALTER TABLE OrderDetails ADD NewOrderId NVARCHAR(10);

-- Обновляем OrderDetails новыми ID
UPDATE od
SET NewOrderId = m.NewId
FROM OrderDetails od
INNER JOIN #OrderIdMapping m ON od.OrderId = m.OldId;

-- Удаляем старые столбцы
ALTER TABLE OrderDetails DROP CONSTRAINT FK_OrderDetails_Orders_OrderId;
ALTER TABLE OrderDetails DROP COLUMN OrderId;

-- Переименовываем новые столбцы
EXEC sp_rename 'Orders.NewId', 'Id', 'COLUMN';
EXEC sp_rename 'OrderDetails.NewOrderId', 'OrderId', 'COLUMN';

-- Делаем новые столбцы обязательными
ALTER TABLE Orders ALTER COLUMN Id NVARCHAR(10) NOT NULL;
ALTER TABLE OrderDetails ALTER COLUMN OrderId NVARCHAR(10) NOT NULL;

-- Добавляем первичный ключ обратно
ALTER TABLE Orders ADD CONSTRAINT PK_Orders PRIMARY KEY (Id);

-- Добавляем внешний ключ обратно
ALTER TABLE OrderDetails ADD CONSTRAINT FK_OrderDetails_Orders_OrderId 
FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE;

-- Включаем проверку внешних ключей обратно
ALTER TABLE OrderDetails CHECK CONSTRAINT ALL;

-- Удаляем временную таблицу
DROP TABLE #OrderIdMapping;
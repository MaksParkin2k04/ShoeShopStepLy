-- Удаляем все данные
DELETE FROM OrderDetails;
DELETE FROM Orders;

-- Удаляем индекс если существует
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OrderDetails_OrderId')
    DROP INDEX IX_OrderDetails_OrderId ON OrderDetails;

-- Изменяем тип столбца OrderId в OrderDetails
ALTER TABLE OrderDetails ALTER COLUMN OrderId NVARCHAR(10) NOT NULL;

-- Изменяем тип столбца Id в Orders
ALTER TABLE Orders ALTER COLUMN Id NVARCHAR(10) NOT NULL;

-- Добавляем первичный ключ обратно
ALTER TABLE Orders ADD CONSTRAINT PK_Orders PRIMARY KEY (Id);

-- Создаем индекс обратно
CREATE INDEX IX_OrderDetails_OrderId ON OrderDetails (OrderId);

-- Добавляем внешний ключ обратно
ALTER TABLE OrderDetails ADD CONSTRAINT FK_OrderDetails_Orders_OrderId 
FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE;
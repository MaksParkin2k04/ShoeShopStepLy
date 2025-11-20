-- Удаляем все существующие заказы для упрощения
DELETE FROM OrderDetails;
DELETE FROM Orders;

-- Удаляем индекс
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OrderDetails_OrderId')
    DROP INDEX IX_OrderDetails_OrderId ON OrderDetails;

-- Удаляем внешний ключ
ALTER TABLE OrderDetails DROP CONSTRAINT FK_OrderDetails_Orders_OrderId;

-- Удаляем первичный ключ
ALTER TABLE Orders DROP CONSTRAINT PK_Orders;

-- Изменяем тип столбца Id в Orders
ALTER TABLE Orders ALTER COLUMN Id NVARCHAR(10) NOT NULL;

-- Изменяем тип столбца OrderId в OrderDetails
ALTER TABLE OrderDetails ALTER COLUMN OrderId NVARCHAR(10) NOT NULL;

-- Добавляем первичный ключ обратно
ALTER TABLE Orders ADD CONSTRAINT PK_Orders PRIMARY KEY (Id);

-- Добавляем внешний ключ обратно
ALTER TABLE OrderDetails ADD CONSTRAINT FK_OrderDetails_Orders_OrderId 
FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE;

-- Создаем индекс обратно
CREATE INDEX IX_OrderDetails_OrderId ON OrderDetails (OrderId);
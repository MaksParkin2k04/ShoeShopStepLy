-- Добавляем колонку DeliveryType в таблицу Orders
ALTER TABLE Orders 
ADD DeliveryType int NOT NULL DEFAULT 0;

-- Обновляем существующие записи (0 = Courier по умолчанию)
UPDATE Orders 
SET DeliveryType = 0 
WHERE DeliveryType IS NULL;
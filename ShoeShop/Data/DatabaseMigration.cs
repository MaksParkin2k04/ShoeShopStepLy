using Microsoft.EntityFrameworkCore;

namespace ShoeShop.Data
{
    public static class DatabaseMigration
    {
        public static async Task EnsureDeliveryTypeColumnExists(ApplicationContext context)
        {
            try
            {
                // Проверяем, существует ли колонка DeliveryType
                var sql = @"
                    IF NOT EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'DeliveryType'
                    )
                    BEGIN
                        ALTER TABLE Orders ADD DeliveryType int NOT NULL DEFAULT 0;
                        UPDATE Orders SET DeliveryType = 0 WHERE DeliveryType IS NULL;
                    END";
                
                await context.Database.ExecuteSqlRawAsync(sql);
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не прерываем работу приложения
                Console.WriteLine($"Migration error: {ex.Message}");
            }
        }
    }
}
namespace ShoeShop.MultiTenantAdmin.Models {
    /// <summary>
    /// Статистика продаж
    /// </summary>
    public class SalesStatistics {
        /// <summary>
        /// Общее количество проданных пар
        /// </summary>
        public int TotalQuantitySold { get; set; }

        /// <summary>
        /// Общая выручка (грязные деньги)
        /// </summary>
        public double TotalRevenue { get; set; }

        /// <summary>
        /// Общие затраты на закупку
        /// </summary>
        public double TotalCosts { get; set; }

        /// <summary>
        /// Чистая прибыль
        /// </summary>
        public double NetProfit => TotalRevenue - TotalCosts;

        /// <summary>
        /// Рентабельность в процентах
        /// </summary>
        public double ProfitMargin => TotalCosts > 0 ? (NetProfit / TotalCosts) * 100 : 0;
    }

    /// <summary>
    /// Статистика по товару
    /// </summary>
    public class ProductSalesStatistics {
        /// <summary>
        /// ID товара
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Название товара
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Количество проданных пар
        /// </summary>
        public int QuantitySold { get; set; }

        /// <summary>
        /// Выручка с товара
        /// </summary>
        public double Revenue { get; set; }

        /// <summary>
        /// Затраты на закупку
        /// </summary>
        public double Costs { get; set; }

        /// <summary>
        /// Прибыль с товара
        /// </summary>
        public double Profit => Revenue - Costs;
    }
}

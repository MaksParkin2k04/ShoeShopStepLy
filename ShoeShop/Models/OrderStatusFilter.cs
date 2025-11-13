namespace ShoeShop.Models {
    public enum OrderStatusFilter {
        /// <summary>
        /// Все
        /// </summary>
        All,
        /// <summary>
        /// Cозданные
        /// </summary>
        Created,
        /// <summary>
        /// Находяшиеся в обработке
        /// </summary>
        Processing,
        /// <summary>
        /// Выполненные
        /// </summary>
        Completed,
        /// <summary>
        /// Отмененные
        /// </summary>
        Canceled
    }
}

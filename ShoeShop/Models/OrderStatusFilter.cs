namespace ShoeShop.Models {
    public enum OrderStatusFilter {
        /// <summary>
        /// Все
        /// </summary>
        All,
        /// <summary>
        /// Активные (все кроме выполненных)
        /// </summary>
        Active,
        /// <summary>
        /// Cозданные
        /// </summary>
        Created,
        /// <summary>
        /// Оплаченные
        /// </summary>
        Paid,
        /// <summary>
        /// Находяшиеся в обработке
        /// </summary>
        Processing,
        /// <summary>
        /// Ожидает отправления
        /// </summary>
        AwaitingShipment,
        /// <summary>
        /// Отправлен
        /// </summary>
        Shipped,
        /// <summary>
        /// В пути
        /// </summary>
        InTransit,
        /// <summary>
        /// Прибыл
        /// </summary>
        Arrived,
        /// <summary>
        /// Готов к выдаче
        /// </summary>
        ReadyForPickup,
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

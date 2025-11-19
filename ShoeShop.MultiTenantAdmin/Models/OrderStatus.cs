namespace ShoeShop.MultiTenantAdmin.Models {
    public enum OrderStatus {
        /// <summary>
        /// Создан
        /// </summary>
        Created,
        /// <summary>
        /// Оплачен
        /// </summary>
        Paid,
        /// <summary>
        /// Обрабатывается
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
        /// Прибыл в пункт выдачи
        /// </summary>
        Arrived,
        /// <summary>
        /// Готов к выдаче
        /// </summary>
        ReadyForPickup,
        /// <summary>
        /// Выполнен (доставлен)
        /// </summary>
        Completed,
        /// <summary>
        /// Возвращен
        /// </summary>
        Returned,
        /// <summary>
        /// Отменен
        /// </summary>
        Canceled
    }
}

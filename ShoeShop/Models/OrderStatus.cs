namespace ShoeShop.Models {
    public enum OrderStatus {
        /// <summary>
        /// Создан
        /// </summary>
        Created,
        /// <summary>
        /// Обрабатывается
        /// </summary>
        Processing,
        /// <summary>
        /// Выполнен
        /// </summary>
        Completed,
        /// <summary>
        /// Отменен
        /// </summary>
        Canceled
    }
}

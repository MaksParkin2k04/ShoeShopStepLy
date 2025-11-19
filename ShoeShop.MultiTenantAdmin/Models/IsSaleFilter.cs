namespace ShoeShop.MultiTenantAdmin.Models {
    public enum IsSaleFilter {
        /// <summary>
        /// Все товары
        /// </summary>
        All,
        /// <summary>
        /// Товары в продаже
        /// </summary>
        IsSale,
        /// <summary>
        /// Товары снятые с продажи
        /// </summary>
        IsNotSale
    }
}

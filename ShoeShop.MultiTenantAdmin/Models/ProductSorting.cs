namespace ShoeShop.MultiTenantAdmin.Models {
    public enum ProductSorting {
        /// <summary>
        /// По умолчанию
        /// </summary>
        Default = 0,
        /// <summary>
        /// Дешевле
        /// </summary>
        Cheaper = 1,
        /// <summary>
        /// Дороже
        /// </summary>
        Expensive = 2,
        /// <summary>
        /// По дате
        /// </summary>
        ByDate = 3
    }
}

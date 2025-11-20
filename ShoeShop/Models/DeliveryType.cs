namespace ShoeShop.Models
{
    public enum DeliveryType
    {
        /// <summary>
        /// Курьерская доставка
        /// </summary>
        Courier = 0,
        
        /// <summary>
        /// Самовывоз из магазина
        /// </summary>
        Pickup = 1,
        
        /// <summary>
        /// Доставка Почтой России
        /// </summary>
        RussianPost = 2,
        
        /// <summary>
        /// Доставка СДЭК
        /// </summary>
        CDEK = 3,
        
        /// <summary>
        /// Доставка Boxberry
        /// </summary>
        Boxberry = 4
    }
}
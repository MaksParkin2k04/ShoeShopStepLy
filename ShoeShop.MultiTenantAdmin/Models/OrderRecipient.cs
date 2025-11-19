namespace ShoeShop.MultiTenantAdmin.Models {
    /// <summary>
    /// Информация о получателе заказа
    /// </summary>
    public class OrderRecipient {

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="name">Имя получателя заказа</param>
        /// <param name="city">Город доставки заказа</param>
        /// <param name="street">Улица доставки заказа</param>
        /// <param name="house">Дом доставки заказа</param>
        /// <param name="apartment">Квартира доставки заказа</param>
        /// <param name="phone">Телефон получателя заказа</param>
        private OrderRecipient(string name, string city, string street, string house, string apartment, string phone) {
            Name = name;
            City = city;
            Street = street;
            House = house;
            Apartment = apartment;
            Phone = phone;
        }

        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Город
        /// </summary>
        public string City { get; private set; }
        /// <summary>
        /// Улица
        /// </summary>
        public string Street { get; private set; }
        /// <summary>
        /// Дом
        /// </summary>
        public string House { get; private set; }
        /// <summary>
        /// Квартира
        /// </summary>
        public string Apartment { get; private set; }
        /// <summary>
        /// Телефон
        /// </summary>
        public string Phone { get; private set; }

        /// <summary>
        /// Создание объекта содержащего информацию о получателе заказа
        /// </summary>
        /// <param name="name">Имя получателя заказа</param>
        /// <param name="city">Город доставки заказа</param>
        /// <param name="street">Улица доставки заказа</param>
        /// <param name="house">Дом доставки заказа</param>
        /// <param name="apartment">Квартира доставки заказа</param>
        /// <param name="phone">Телефон получателя заказа</param>
        /// <returns>Объект содержащий информацию о получателе заказа</returns>
        public static OrderRecipient Create(string name, string city, string street, string house, string apartment, string phone) {
            return new OrderRecipient(name, city, street, house, apartment, phone);
        }
    }
}

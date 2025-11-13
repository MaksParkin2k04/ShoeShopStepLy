namespace ShoeShop.Models {
    /// <summary>
    /// Информация о заказе
    /// </summary>
    public class Order {

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="id">Идентификатор заказа</param>
        /// <param name="customerId">Идентификатор клиента</param>
        /// <param name="createdDate">Дата создания заказа</param>
        /// <param name="coment">Коментарий к заказу</param>
        /// <param name="status">Статус заказа</param>
        private Order(Guid id, Guid customerId, DateTime createdDate, string? coment, OrderStatus status) {
            Id = id;
            CustomerId = customerId;
            CreatedDate = createdDate;
            Coment = coment;
            Status = status;
        }

        //private OrderRecipient? recipient;
        private List<OrderDetail>? orderDetails;

        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public Guid CustomerId { get; private set; }
        /// <summary>
        /// Дата создания заказа
        /// </summary>
        public DateTime CreatedDate { get; private set; }
        /// <summary>
        /// Получатель заказа
        /// </summary>
        public OrderRecipient? Recipient { get; private set; }
        /// <summary>
        /// Коментарий к заказу
        /// </summary>
        public string? Coment { get; private set; }
        /// <summary>
        /// Состав заказа
        /// </summary>
        public List<OrderDetail>? OrderDetails { get { return orderDetails; } }
        /// <summary>
        /// Статус заказа
        /// </summary>
        public OrderStatus Status { get; private set; }

        public void SetStatus(OrderStatus status) {
            if (Status == OrderStatus.Completed) { throw new ArgumentOutOfRangeException("status", "Нельзя изменить статус выполненого заказа"); }
            Status = status;
        }

        /// <summary>
        /// Создает объект содержащий информацию о заказе
        /// </summary>
        /// <param name="customerId">Идентификатор клиента</param>
        /// <param name="createdDate">Дата создания заказа</param>
        /// <param name="coment">Коментарий к заказу</param>
        /// <param name="recipient">Получатель заказа</param>
        /// <param name="orderDetails">Состав заказа</param>
        /// <returns>Объект содержащий информацию о заказе</returns>
        public static Order Create(Guid customerId, DateTime createdDate, string? coment, OrderRecipient recipient, IEnumerable<OrderDetail> orderDetails) {
            Order order = new Order(Guid.Empty, customerId, createdDate, coment, OrderStatus.Created);
            order.Recipient = recipient;
            order.orderDetails = orderDetails != null ? new List<OrderDetail>(orderDetails) : new List<OrderDetail>();
            return order;
        }
    }
}

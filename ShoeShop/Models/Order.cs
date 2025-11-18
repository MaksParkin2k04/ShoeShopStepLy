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
        /// <param name="paymentType">Тип оплаты</param>
        private Order(Guid id, Guid customerId, DateTime createdDate, string? coment, OrderStatus status, PaymentType paymentType) {
            Id = id;
            CustomerId = customerId;
            CreatedDate = createdDate;
            Coment = coment;
            Status = status;
            PaymentType = paymentType;
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
        /// <summary>
        /// Тип оплаты
        /// </summary>
        public PaymentType PaymentType { get; private set; }
        /// <summary>
        /// Дата оплаты
        /// </summary>
        public DateTime? PaymentDate { get; private set; }
        /// <summary>
        /// Комментарии администратора
        /// </summary>
        public List<string>? AdminComments { get; private set; }
        
        /// <summary>
        /// Источник заказа (Сайт, Telegram, VK)
        /// </summary>
        public string? Source { get; private set; }
        
        /// <summary>
        /// Telegram ID пользователя (если заказ из Telegram)
        /// </summary>
        public long? TelegramUserId { get; private set; }
        
        /// <summary>
        /// ID пользователя сайта (если заказ связан с аккаунтом)
        /// </summary>
        public Guid? WebUserId { get; private set; }
        
        /// <summary>
        /// Номер заказа для отслеживания
        /// </summary>
        public string OrderNumber { get; private set; } = string.Empty;

        public void SetStatus(OrderStatus status) {
            Status = status;
            if (status == OrderStatus.Paid) {
                PaymentDate = DateTime.Now;
            }
        }
        
        public void AddAdminComment(string comment) {
            AdminComments = AdminComments ?? new List<string>();
            AdminComments.Add($"[{DateTime.Now:dd.MM.yyyy HH:mm}] Администратор: {comment}");
        }
        
        public void AddCustomerComment(string comment) {
            AdminComments = AdminComments ?? new List<string>();
            AdminComments.Add($"[{DateTime.Now:dd.MM.yyyy HH:mm}] Клиент: {comment}");
        }
        
        public void SetSource(string source) {
            Source = source;
        }
        
        public void SetTelegramUser(long telegramUserId) {
            TelegramUserId = telegramUserId;
        }
        
        public void SetOrderNumber(string orderNumber) {
            OrderNumber = orderNumber;
        }
        
        public void SetWebUser(Guid webUserId) {
            WebUserId = webUserId;
        }

        /// <summary>
        /// Создает объект содержащий информацию о заказе
        /// </summary>
        /// <param name="customerId">Идентификатор клиента</param>
        /// <param name="createdDate">Дата создания заказа</param>
        /// <param name="coment">Коментарий к заказу</param>
        /// <param name="recipient">Получатель заказа</param>
        /// <param name="orderDetails">Состав заказа</param>
        /// <param name="paymentType">Тип оплаты</param>
        /// <returns>Объект содержащий информацию о заказе</returns>
        public static Order Create(Guid customerId, DateTime createdDate, string? coment, OrderRecipient recipient, IEnumerable<OrderDetail> orderDetails, PaymentType paymentType = PaymentType.Cash) {
            Order order = new Order(Guid.Empty, customerId, createdDate, coment, OrderStatus.Created, paymentType);
            order.Recipient = recipient;
            order.orderDetails = orderDetails != null ? new List<OrderDetail>(orderDetails) : new List<OrderDetail>();
            return order;
        }
    }
}

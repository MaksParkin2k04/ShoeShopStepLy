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
        private Order(string id, Guid customerId, DateTime createdDate, string? coment, OrderStatus status, PaymentType paymentType) {
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
        public string Id { get; private set; }
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
        
        /// <summary>
        /// Тип доставки
        /// </summary>
        public DeliveryType DeliveryType { get; private set; } = DeliveryType.Courier;

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
        
        public void GenerateOrderNumber() {
            var random = new Random();
            var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var letter1 = letters[random.Next(letters.Length)];
            var letter2 = letters[random.Next(letters.Length)];
            var numbers = random.Next(1000, 9999);
            OrderNumber = $"{letter1}{letter2}{numbers}";
        }
        
        public void SetWebUser(Guid webUserId) {
            WebUserId = webUserId;
        }
        
        public void SetDeliveryType(DeliveryType deliveryType) {
            DeliveryType = deliveryType;
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
        /// <param name="deliveryType">Тип доставки</param>
        /// <returns>Объект содержащий информацию о заказе</returns>
        public static Order Create(Guid customerId, DateTime createdDate, string? coment, OrderRecipient recipient, IEnumerable<OrderDetail> orderDetails, PaymentType paymentType = PaymentType.Cash, DeliveryType deliveryType = DeliveryType.Courier) {
            var random = new Random();
            var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var letter1 = letters[random.Next(letters.Length)];
            var letter2 = letters[random.Next(letters.Length)];
            var numbers = random.Next(1000, 9999);
            var orderId = $"{letter1}{letter2}{numbers}";
            
            Order order = new Order(orderId, customerId, createdDate, coment, OrderStatus.Created, paymentType);
            order.Recipient = recipient;
            order.orderDetails = orderDetails != null ? new List<OrderDetail>(orderDetails) : new List<OrderDetail>();
            order.OrderNumber = orderId;
            order.DeliveryType = deliveryType;
            return order;
        }
    }
}

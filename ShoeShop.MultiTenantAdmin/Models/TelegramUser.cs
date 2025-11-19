namespace ShoeShop.MultiTenantAdmin.Models {
    public class TelegramUser {
        public long TelegramId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastActivity { get; set; }
        
        public List<Order> Orders { get; set; } = new List<Order>();
        
        /// <summary>
        /// Связь с пользователем сайта
        /// </summary>
        public Guid? WebUserId { get; set; }
        
        /// <summary>
        /// Email для связи с сайтом
        /// </summary>
        public string? Email { get; set; }
        
        /// <summary>
        /// Подтверждение связи с сайтом
        /// </summary>
        public bool IsLinkedToWebsite { get; set; }
        
        public string GetFullName() => $"{FirstName} {LastName}".Trim();
    }
}

namespace ShoeShop.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Response { get; set; }
        public string? RespondedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public bool IsAnswered { get; set; }
        public bool IsAutoResponse { get; set; }
        public bool IsClosed { get; set; }
    }
}
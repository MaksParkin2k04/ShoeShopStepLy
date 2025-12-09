namespace ShoeShop.Models;

public class EmailCampaign
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public int RecipientCount { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Sent
}

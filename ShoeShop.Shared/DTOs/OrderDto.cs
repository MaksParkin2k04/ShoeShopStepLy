namespace ShoeShop.Shared.DTOs;

public class OrderDto {
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public string Status { get; set; } = "";
    public double Total { get; set; }
    public string Source { get; set; } = "";
    public long? TelegramUserId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public CustomerDto Customer { get; set; } = new();
}

public class OrderItemDto {
    public Guid ProductId { get; set; }
    public string Name { get; set; } = "";
    public double Price { get; set; }
    public int Size { get; set; }
    public string Image { get; set; } = "";
}

public class OrderCreateDto {
    public List<OrderItemDto> Items { get; set; } = new();
    public CustomerDto Customer { get; set; } = new();
    public string Source { get; set; } = "";
    public long? TelegramUserId { get; set; }
}

public class CustomerDto {
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public string Email { get; set; } = "";
}
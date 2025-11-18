namespace ShoeShop.TelegramBot.Models;

public class UserSession {
    public List<CartItem> Cart { get; set; } = new();
    public UserState State { get; set; } = UserState.None;
    public OrderData OrderData { get; set; } = new();
}

public class CartItem {
    public Guid ProductId { get; set; }
    public string Name { get; set; } = "";
    public double Price { get; set; }
    public int Size { get; set; }
    public int Quantity { get; set; }
}

public class OrderData {
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
}

public enum UserState {
    None,
    WaitingName,
    WaitingPhone,
    WaitingAddress
}
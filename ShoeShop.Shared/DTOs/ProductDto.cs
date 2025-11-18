namespace ShoeShop.Shared.DTOs;

public class ProductDto {
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Content { get; set; } = "";
    public double Price { get; set; }
    public double? SalePrice { get; set; }
    public double FinalPrice => SalePrice ?? Price;
    public bool IsSale { get; set; }
    public DateTime DateAdded { get; set; }
    public string Category { get; set; } = "";
    public List<string> Images { get; set; } = new();
    public List<int> Sizes { get; set; } = new();
}

public class ProductCreateDto {
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Content { get; set; } = "";
    public double Price { get; set; }
    public double? SalePrice { get; set; }
    public bool IsSale { get; set; }
    public Guid CategoryId { get; set; }
    public List<int> Sizes { get; set; } = new();
}
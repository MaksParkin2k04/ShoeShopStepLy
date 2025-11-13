using Microsoft.AspNetCore.Mvc;
namespace ShoeShop.Infrastructure {
    public class EditImage {
        public Guid? Id { get; set; }
        public IFormFile? Image { get; set; }
        public string? Alt { get; set; }
        public EditImageMode? Mode { get; set; }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;

namespace ShoeShop.Pages.Admin {

    [Authorize(Roles = "Admin")]
    public class CatalogModel : PageModel {
        /// <summary>
        /// Максимальное количество элементов на стронице
        /// </summary>
        private const int MAX_COUNT_ITEMS_ON_PAGE = 20;

        public CatalogModel(IAdminRepository repository) {
            this.repository = repository;
        }

        private readonly IAdminRepository repository;

        public int CurrentPage { get; private set; }
        public int ElementsPerPage { get; private set; }
        public int TotalElementsCount { get; private set; }
        public ProductSorting Sorting { get; private set; }
        public IsSaleFilter IsSaleFilter { get; private set; }
        public string PartProductName { get; private set; }
        public IReadOnlyList<Product>? Products { get; private set; }

        public async Task OnGet(ProductSorting sorting, IsSaleFilter saleFilter, string partProductName = "", int pageIndex = 1) {
            Sorting = sorting;
            IsSaleFilter = saleFilter;
            PartProductName = partProductName;
            CurrentPage = pageIndex;
            ElementsPerPage = MAX_COUNT_ITEMS_ON_PAGE;
            TotalElementsCount = await repository.ProductCount(saleFilter, partProductName);
            Products = await repository.GetProducts(sorting, saleFilter, partProductName, pageIndex - 1, MAX_COUNT_ITEMS_ON_PAGE);
        }
    }
}

using ShoeShop.Models;

namespace ShoeShop.Data {
    public static class ProductQueryable {
        public static IQueryable<Product> OrderProductsBy(this IQueryable<Product> products, ProductSorting sorting) {
            switch (sorting) {
                case ProductSorting.Default:
                    return products.OrderBy(p => p.Id);
                case ProductSorting.Cheaper:
                    return products.OrderBy(p => p.Price);
                case ProductSorting.Expensive:
                    return products.OrderByDescending(p => p.Price);
                case ProductSorting.ByDate:
                    return products.OrderBy(p => p.DateAdded);
                default:
                    throw new ArgumentOutOfRangeException(nameof(sorting));
            }
        }

        public static IQueryable<Product> SearchByName(this IQueryable<Product> products, string partProductName) {

            if (string.IsNullOrWhiteSpace(partProductName)) {
                return products;
            }

            return products.Where(p => p.Name.Contains(partProductName));
        }

        public static IQueryable<Product> IsSaleFilters(this IQueryable<Product> products, IsSaleFilter filter) {
            switch (filter) {
                case IsSaleFilter.All:
                    return products;
                case IsSaleFilter.IsSale:
                    return products.Where(p => p.IsSale);
                case IsSaleFilter.IsNotSale:
                    return products.Where(p => p.IsSale == false);
                default:
                    throw new ArgumentOutOfRangeException(nameof(filter));
            }
        }

        public static IQueryable<Product> Page(this IQueryable<Product> products, int start, int take) {
            if (take == 0) { throw new ArgumentOutOfRangeException(nameof(take)); }

            if (start != 0) {
                products = products.Skip(start * take);
            }

            return products.Take(take);
        }
    }
}

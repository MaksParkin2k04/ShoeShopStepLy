using Microsoft.EntityFrameworkCore;
using ShoeShop.Models;

namespace ShoeShop.Data {
    /// <summary>
    /// Репозиторий для работы с остатками товаров
    /// </summary>
    public class ProductStockRepository : IProductStockRepository {
        private readonly ApplicationContext _context;

        public ProductStockRepository(ApplicationContext context) {
            _context = context;
        }

        public async Task<IEnumerable<ProductStock>> GetByProductIdAsync(Guid productId) {
            return await _context.ProductStocks
                .Where(s => s.ProductId == productId)
                .ToListAsync();
        }

        public async Task<ProductStock?> GetByProductAndSizeAsync(Guid productId, int size) {
            return await _context.ProductStocks
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.Size == size);
        }

        public async Task SaveAsync(ProductStock productStock) {
            var existing = await _context.ProductStocks
                .FirstOrDefaultAsync(s => s.ProductId == productStock.ProductId && s.Size == productStock.Size);
            
            if (existing == null) {
                productStock.Id = Guid.NewGuid();
                _context.ProductStocks.Add(productStock);
            } else {
                existing.SetQuantity(productStock.Quantity);
                existing.SetPurchasePrice(productStock.PurchasePrice);
            }
            
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalQuantityAsync(Guid productId) {
            return await _context.ProductStocks
                .Where(s => s.ProductId == productId)
                .SumAsync(s => s.Quantity);
        }

        public async Task<int> GetQuantityAsync(Guid productId, int size) {
            var stock = await GetByProductAndSizeAsync(productId, size);
            return stock?.Quantity ?? 0;
        }
        
        public async Task<List<ProductStock>> GetByProductIdsBatchAsync(List<Guid> productIds) {
            return await _context.ProductStocks
                .Where(s => productIds.Contains(s.ProductId))
                .ToListAsync();
        }
        
        public async Task<IEnumerable<ProductStock>> GetSimpleStocksAsync(int skip, int take) {
            return await _context.ProductStocks
                .OrderBy(s => s.ProductId)
                .ThenBy(s => s.Size)
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();
        }
        
        public async Task<int> GetTotalStocksCountAsync() {
            return await _context.ProductStocks
                .AsNoTracking()
                .CountAsync();
        }
        
        public async Task<IEnumerable<ProductStock>> GetStocksWithSearchAsync(string? search, string? status, int skip, int take) {
            var query = _context.ProductStocks
                .Include(s => s.Product)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(search)) {
                query = query.Where(s => s.Product != null && s.Product.Name.Contains(search));
            }
            
            if (!string.IsNullOrEmpty(status)) {
                switch (status) {
                    case "instock":
                        query = query.Where(s => s.Quantity >= 5);
                        break;
                    case "lowstock":
                        query = query.Where(s => s.Quantity > 0 && s.Quantity < 5);
                        break;
                    case "outofstock":
                        query = query.Where(s => s.Quantity == 0);
                        break;
                }
            }
            
            return await query
                .OrderBy(s => s.ProductId)
                .ThenBy(s => s.Size)
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();
        }
        
        public async Task<int> GetStocksCountWithSearchAsync(string? search, string? status) {
            var query = _context.ProductStocks
                .Include(s => s.Product)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(search)) {
                query = query.Where(s => s.Product != null && s.Product.Name.Contains(search));
            }
            
            if (!string.IsNullOrEmpty(status)) {
                switch (status) {
                    case "instock":
                        query = query.Where(s => s.Quantity >= 5);
                        break;
                    case "lowstock":
                        query = query.Where(s => s.Quantity > 0 && s.Quantity < 5);
                        break;
                    case "outofstock":
                        query = query.Where(s => s.Quantity == 0);
                        break;
                }
            }
            
            return await query.AsNoTracking().CountAsync();
        }
    }
}
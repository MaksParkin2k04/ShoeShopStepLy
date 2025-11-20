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
                _context.ProductStocks.Add(productStock);
            } else {
                _context.Entry(existing).CurrentValues.SetValues(productStock);
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
    }
}
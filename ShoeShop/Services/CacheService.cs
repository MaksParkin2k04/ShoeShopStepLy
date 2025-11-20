using Microsoft.Extensions.Caching.Memory;
using ShoeShop.Models;

namespace ShoeShop.Services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
        void Remove(string key);
        void RemoveByPattern(string pattern);
    }

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly HashSet<string> _cacheKeys = new();

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            _cache.TryGetValue(key, out T? value);
            return Task.FromResult(value);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(key, value, options);
            _cacheKeys.Add(key);
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
            _cacheKeys.Remove(key);
        }

        public void RemoveByPattern(string pattern)
        {
            var keysToRemove = _cacheKeys.Where(k => k.Contains(pattern)).ToList();
            foreach (var key in keysToRemove)
            {
                Remove(key);
            }
        }
    }
}
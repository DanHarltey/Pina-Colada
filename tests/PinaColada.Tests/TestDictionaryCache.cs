using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PinaColada.Tests
{
    internal class TestDictionaryCache : ICache
    {
        private readonly Dictionary<string, object> keyValue = new Dictionary<string, object>();

        public Task<(bool, T)> Get<T>(string cacheKey)
        {
            var found = keyValue.TryGetValue(cacheKey, out var obj);

            return Task.FromResult( (found, (T)obj));
        }

        public Task Set<T>(string cacheKey, T obj, TimeSpan? ttl)
        {
            keyValue.Add(cacheKey, obj);
            return Task.CompletedTask;
        }
    }
}

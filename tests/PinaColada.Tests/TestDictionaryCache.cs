using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PinaColada.Tests
{
    internal class TestDictionaryCache : ICache
    {
        private readonly Dictionary<string, object> keyValue = new Dictionary<string, object>();
        private readonly int _delayTime;
        private readonly bool _throwOnGet;
        private readonly bool _throwOnSet;

        public TestDictionaryCache(int delayTime = 0, bool throwOnGet = false, bool throwOnSet = false)
        {
            _delayTime = delayTime;
            _throwOnGet = throwOnGet;
            _throwOnSet = throwOnSet;
        }

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

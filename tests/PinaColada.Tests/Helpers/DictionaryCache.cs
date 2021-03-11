using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PinaColada.Tests.Helpers
{
    internal class DictionaryCache : ICache
    {
        private readonly Dictionary<string, object> keyValue = new Dictionary<string, object>();
        private readonly int _getDelay;
        private readonly int _setDelay;
        private readonly bool _throwOnGet;
        private readonly bool _throwOnSet;

        public DictionaryCache(int getDelay = 0, int setDelay = 0, bool throwOnGet = false, bool throwOnSet = false)
        {
            _getDelay = getDelay;
            _setDelay = setDelay;
            _throwOnGet = throwOnGet;
            _throwOnSet = throwOnSet;
        }

        public async Task<Result<T>> TryGet<T>(string cacheKey)
        {
            if (_getDelay != 0)
            {
                await Task.Delay(_getDelay);
            }

            if (_throwOnGet)
            {
                throw new InvalidOperationException();
            }

            var found = keyValue.TryGetValue(cacheKey, out var obj);

            if(found)
            {
                return Result.CacheHit((T)obj);
            }
            else
            {
                return Result<T>.CacheMiss;
            }
        }

        public async Task Set<T>(string cacheKey, T obj, TimeSpan? ttl)
        {
            if (_setDelay != 0)
            {
                await Task.Delay(_setDelay);
            }

            if (_throwOnSet)
            {
                throw new InvalidOperationException();
            }

            keyValue.Add(cacheKey, obj);
        }
    }
}

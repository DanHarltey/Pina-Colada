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

        public Task<Result<T>> TryGet<T>(string cacheKey)
        {
            if(_throwOnGet)
            {
                throw new Exception();
            }

            var found = keyValue.TryGetValue(cacheKey, out var obj);

            if(found)
            {
                return Result<T>.CaheHitTask((T)obj);
            }
            else
            {
                return Result<T>.CaheMissTask;
            }
        }

        public Task Set<T>(string cacheKey, T obj, TimeSpan? ttl)
        {
            if (_throwOnSet)
            {
                throw new Exception();
            }

            keyValue.Add(cacheKey, obj);
            return Task.CompletedTask;
        }
    }
}

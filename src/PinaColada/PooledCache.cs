using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PinaColada
{
    public class PooledCache : IPooledCache
    {
        private readonly ConcurrentDictionary<string, Task> _requestPool = new ConcurrentDictionary<string, Task>();
        private readonly ICache _cache;

        public PooledCache(ICache cache)
        {
            _cache = cache;
        }

        public async Task<Result<T>> Fetch<T>(string cacheKey, Func<Task<T>> createAction, TimeSpan? ttl)
        {
            var createdTask = new Task<Task<Result<T>>>(GetOrSet<T>, new CacheRequest<T>(cacheKey, createAction, ttl));
            var createdTask2 = createdTask.Unwrap();

            var pooledTask = _requestPool.GetOrAdd(cacheKey, createdTask2);

            if (createdTask2 == pooledTask)
            {
                // we added the task
                try
                {
                    createdTask.Start();
                    return await createdTask2;
                }
                finally
                {
                    // we most remove it
                    _requestPool.TryRemove(cacheKey, out _);
                }
            }
            else
            {
                return await (Task<Result<T>>)pooledTask;
            }
        }

        private async Task<Result<T>> GetOrSet<T>(object obj)
        {
            var cacheRequest = (CacheRequest<T>)obj;

            var getResult = await _cache.TryGet<T>(cacheRequest.CacheKey);

            if (getResult.CacheHit)
            {
                return getResult;
            }

            var createdObj = await cacheRequest.CreateAction();

            //TODO use a different thread
            var setTask = _cache.Set(cacheRequest.CacheKey, createdObj, cacheRequest.TTL);
            await setTask;

            return Result.CacheHit(createdObj, setTask);
        }

        private class CacheRequest<T>
        {
            public CacheRequest(string cacheKey, Func<Task<T>> createAction, TimeSpan? ttl)
            {
                CacheKey = cacheKey;
                CreateAction = createAction;
                TTL = ttl;
            }

            public string CacheKey { get; }
            public Func<Task<T>> CreateAction { get; }
            public TimeSpan? TTL { get; }
        }
    }
}

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
        private readonly ICache _cache = (ICache)new object();

        public async Task<T> Fetch<T>(string cacheKey, Func<Task<T>> createAction, TimeSpan? ttl)
        {
            var completionSource = new TaskCompletionSource<T>();
            var createdTask = completionSource.Task;// (this.GetOrSet, new CacheRequest(cacheKey, createAction, ttl));

            var pooledTask = _requestPool.GetOrAdd(cacheKey, (Task)null);

            if (createdTask == pooledTask)
            {
                // we added the task
                try
                {
                    var complete = await GetOrSet<T>(new CacheRequest<T>(cacheKey, createAction, ttl));
                    completionSource.SetResult(complete);
                }
                finally
                {
                    // we most remove it
                    _requestPool.TryRemove(cacheKey, out _);
                }
                return null;
            }
            else
            {
                return await (Task<T>)pooledTask;
            }
        }

        private Task<T> GetOrSet2<T>(object obj)
        {
            var cacheRequest = (CacheRequest<T>)obj;

            var cacheTask = _cache.Get<T>(cacheRequest.CacheKey);

            T CacheMiss(Task<(bool, T)> input)
            {
                if (input.Result.Item1)
                {
                    return input.Result.Item2;
                }
                else
                {
                    return cacheRequest.CreateAction();
                }
            };

            var returnThisOne = cacheTask.ContinueWith(CacheMiss);


            //var createdObj = await cacheRequest.CreateAction();

            //// use a different thread
            //await _cache.Set(createdObj, cacheRequest.TTL);

            //return createdObj;
            return returnThisOne;
        }

        private async Task<T> GetOrSet<T>(object obj)
        {
            var cacheRequest = (CacheRequest<T>)obj;

            var cache = await _cache.Get<T>(cacheRequest.CacheKey);

            if (cache.Item1)
            {
                return cache.Item2;
            }

            var createdObj = await cacheRequest.CreateAction();

            // use a different thread
            await _cache.Set(createdObj, cacheRequest.TTL);

            return createdObj;
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

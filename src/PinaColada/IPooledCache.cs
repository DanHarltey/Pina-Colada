using System;
using System.Threading.Tasks;

namespace PinaColada
{
    public interface IPooledCache
    {
        Task<Result<T>> Fetch<T>(string cacheKey, Func<Task<T>> createAction, TimeSpan? ttl);
    }
}

using System;
using System.Threading.Tasks;

namespace PinaColada
{
    public interface IPooledCache
    {
        Task<T> Fetch<T>(string cacheKey, Func<Task<T>> createAction, TimeSpan? ttl);
    }
}

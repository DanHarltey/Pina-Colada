using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PinaColada
{
    public interface ICache
    {
        Task<Result<T>> TryGet<T>(string cacheKey);
        Task Set<T>(string cacheKey, T obj, TimeSpan? ttl);
    }
}

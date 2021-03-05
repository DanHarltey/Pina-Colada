using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PinaColada
{
    public interface ICache
    {
        // this needs to be try get or something
        Task<(bool, T)> Get<T>(string cacheKey);
        Task Set<T>(string cacheKey, T obj, TimeSpan? ttl);
    }
}

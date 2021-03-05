using System;
using System.Threading.Tasks;
using Xunit;

namespace PinaColada.Tests
{
    public class PooledCacheTests
    {
        [Fact]
        public async Task CacheMissCallsFectch()
        {
            PooledCache cache = new PooledCache(new TestDictionaryCache());
            var testFetch = new TestFecth();

            var actual = await cache.Fetch("test1", testFetch.Fetch, null);

            Assert.Equal(0, actual);
            Assert.Equal(1, testFetch.Counter);
        }

        private class TestFecth
        {
            private int _counter;

            public int Counter => _counter;

            public Task<int> Fetch() => Task.FromResult( _counter++);
        }
    }
}

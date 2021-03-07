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

        [Fact]
        public async Task CacheHitDoesNotCallsFectch()
        {
            PooledCache cache = new PooledCache(new TestDictionaryCache());
            var testFetch = new TestFecth();

            var actual = await cache.Fetch("test1", testFetch.Fetch, null);
            var actual2 = await cache.Fetch("test1", testFetch.Fetch, null);

            Assert.Equal(0, actual);
            Assert.Equal(0, actual2);
            Assert.Same(actual, actual2);
            Assert.Equal(1, testFetch.Counter);
        }


        [Fact]
        public async Task CachePoolsRequests()
        {
            PooledCache cache = new PooledCache(new TestDictionaryCache());
            var testFetch = new TestFecth(500);

            var actualTask = cache.Fetch("test1", testFetch.Fetch, null);
            var actual2Task = cache.Fetch("test1", testFetch.Fetch, null);

            var actual = await actualTask;
            var actual2 = await actual2Task;

            Assert.Equal(0, actual);
            Assert.Equal(0, actual2);
            Assert.Same(actual, actual2);
            Assert.Equal(1, testFetch.Counter);
        }

        private class TestFecth
        {
            private readonly int _delayTime;
            private int _counter;

            public TestFecth(int sleepTime = 0)
            {
                _delayTime = sleepTime;
            }

            public int Counter => _counter;

            public async Task<object> Fetch()
            {
                if(_delayTime !=0)
                {
                    await Task.Delay(_delayTime);
                }
                return _counter++;
            }
        }
    }
}

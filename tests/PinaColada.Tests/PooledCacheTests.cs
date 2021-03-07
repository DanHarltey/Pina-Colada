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

        [Fact]
        public async Task KeyRemovedFromPoolOnFetchException()
        {
            PooledCache cache = new PooledCache(new TestDictionaryCache());
            var testFetch = new TestFecth(throwException: true);

            await Assert.ThrowsAsync<Exception>(() => cache.Fetch("test1", testFetch.Fetch, null));
            await Assert.ThrowsAsync<Exception>(() => cache.Fetch("test1", testFetch.Fetch, null));

            Assert.Equal(2, testFetch.Counter);
        }

        [Fact]
        public async Task KeyRemovedFromPoolOnGetException()
        {
            PooledCache cache = new PooledCache(new TestDictionaryCache(throwOnGet: true));
            var testFetch = new TestFecth(throwException: true);

            await Assert.ThrowsAsync<Exception>(() => cache.Fetch("test1", testFetch.Fetch, null));
            await Assert.ThrowsAsync<Exception>(() => cache.Fetch("test1", testFetch.Fetch, null));

            Assert.Equal(2, testFetch.Counter);
        }

        [Fact]
        public async Task KeyRemovedFromPoolOnSetException()
        {
            PooledCache cache = new PooledCache(new TestDictionaryCache(throwOnSet: true));
            var testFetch = new TestFecth(throwException: true);

            await Assert.ThrowsAsync<Exception>(() => cache.Fetch("test1", testFetch.Fetch, null));
            await Assert.ThrowsAsync<Exception>(() => cache.Fetch("test1", testFetch.Fetch, null));

            Assert.Equal(2, testFetch.Counter);
        }

        [Fact]
        public async Task PoolReturnsFetchException()
        {
            PooledCache cache = new PooledCache(new TestDictionaryCache());
            var testFetch = new TestFecth(delayTime: 500, throwException: true);

            var actual1Task = Assert.ThrowsAsync<Exception>(() => cache.Fetch("test1", testFetch.Fetch, null));
            var actual2Task = Assert.ThrowsAsync<Exception>(() => cache.Fetch("test1", testFetch.Fetch, null));

            var actual1 = await actual1Task;
            var actual2 = await actual2Task;

            Assert.Equal(1, testFetch.Counter);
            Assert.Same(actual1, actual2);
        }

        [Fact]
        public async Task PoolReturnsGetException()
        {
            PooledCache cache = new PooledCache(new TestDictionaryCache(delayTime: 500, throwOnSet: true));
            var testFetch = new TestFecth();

            var actual1Task = Assert.ThrowsAsync<Exception>(() => cache.Fetch("test1", testFetch.Fetch, null));
            var actual2Task = Assert.ThrowsAsync<Exception>(() => cache.Fetch("test1", testFetch.Fetch, null));

            var actual1 = await actual1Task;
            var actual2 = await actual2Task;

            Assert.Equal(1, testFetch.Counter);
            Assert.Same(actual1, actual2);
        }

        private class TestFecth
        {
            private readonly int _delayTime;
            private readonly bool _throwException;
            private int _counter;

            public TestFecth(int delayTime = 0, bool throwException = false)
            {
                _delayTime = delayTime;
                _throwException = throwException;
            }

            public int Counter => _counter;

            public async Task<object> Fetch()
            {
                if(_delayTime !=0)
                {
                    await Task.Delay(_delayTime);
                }

                var tmp = _counter++;

                if (_throwException)
                {
                    throw new Exception();
                }

                return tmp;
            }
        }
    }
}

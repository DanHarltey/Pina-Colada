using PinaColada.Tests.Helpers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PinaColada.Tests.PooledCacheFeatures
{
    public class PooledCacheTests
    {
        [Fact]
        public async Task CacheMissCallsFectch()
        {
            var cacheToTest = new PooledCache(new DictionaryCache());
            var counter = new Counter();

            var actual = await cacheToTest.Fetch("test1", counter.Increment, null);

            Assert.Equal(0, actual.Value.Count);
            Assert.Equal(1, counter.Count);
        }

        [Fact]
        public async Task CacheHitDoesNotCallsFectch()
        {
            var cacheToTest = new PooledCache(new DictionaryCache());
            var counter = new Counter();

            var actual = await cacheToTest.Fetch("test1", counter.Increment, null);
            var actual2 = await cacheToTest.Fetch("test1", counter.Increment, null);

            Assert.Equal(0, actual.Value.Count);
            Assert.Equal(0, actual2.Value.Count);
            Assert.NotSame(actual, actual2);
            Assert.Same(actual.Value, actual2.Value);
            Assert.Equal(1, counter.Count);
        }

        [Fact]
        public async Task CachePoolsRequests()
        {
            var cacheToTest = new PooledCache(new DictionaryCache());
            var counter = new Counter(500);

            var actualTask = cacheToTest.Fetch("test1", counter.Increment, null);
            var actual2Task = cacheToTest.Fetch("test1", counter.Increment, null);

            var actual = await actualTask;
            var actual2 = await actual2Task;

            Assert.Equal(0, actual.Value.Count);
            Assert.Equal(0, actual2.Value.Count);
            Assert.Same(actual, actual2);
            Assert.Equal(1, counter.Count);
        }

        [Fact]
        public async Task KeyRemovedFromPoolOnFetchException()
        {
            var cacheToTest = new PooledCache(new DictionaryCache());
            var counter = new Counter(throwException: true);

            await Assert.ThrowsAsync<Exception>(() => cacheToTest.Fetch("test1", counter.Increment, null));
            await Assert.ThrowsAsync<Exception>(() => cacheToTest.Fetch("test1", counter.Increment, null));

            Assert.Equal(2, counter.Count);
        }

        [Fact]
        public async Task KeyRemovedFromPoolOnGetException()
        {
            var cacheToTest = new PooledCache(new DictionaryCache(throwOnGet: true));
            var counter = new Counter();

            var actual1 = await Assert.ThrowsAsync<Exception>(() => cacheToTest.Fetch("test1", counter.Increment, null));
            var actual2 = await Assert.ThrowsAsync<Exception>(() => cacheToTest.Fetch("test1", counter.Increment, null));

            Assert.Equal(0, counter.Count);
            Assert.NotSame(actual1, actual2);
        }

        [Fact]
        public async Task KeyRemovedFromPoolOnSetException()
        {
            var cacheToTest = new PooledCache(new DictionaryCache(throwOnSet: true));
            var counter = new Counter();

            await Assert.ThrowsAsync<Exception>(() => cacheToTest.Fetch("test1", counter.Increment, null));
            await Assert.ThrowsAsync<Exception>(() => cacheToTest.Fetch("test1", counter.Increment, null));

            Assert.Equal(2, counter.Count);
        }

        [Fact]
        public async Task PoolReturnsFetchException()
        {
            var cacheToTest = new PooledCache(new DictionaryCache());
            var counter = new Counter(delayTime: 500, throwException: true);

            var task1 = cacheToTest.Fetch("test1", counter.Increment, null);
            var task2 = cacheToTest.Fetch("test1", counter.Increment, null);

            var actual1 = await Assert.ThrowsAsync<Exception>(() => task1);
            var actual2 = await Assert.ThrowsAsync<Exception>(() => task2);

            Assert.Equal(1, counter.Count);
            Assert.Same(actual1, actual2);
        }

        [Fact]
        public async Task PoolReturnsGetException()
        {
            var cache = new PooledCache(new DictionaryCache(getDelay: 500, throwOnGet: true));
            var counter = new Counter();

            var task1 = cache.Fetch("test1", counter.Increment, null);
            var task2 = cache.Fetch("test1", counter.Increment, null);

            var actual1 = await Assert.ThrowsAsync<Exception>(() => task1);
            var actual2 = await Assert.ThrowsAsync<Exception>(() => task2);

            Assert.Equal(0, counter.Count);
            Assert.Same(actual1, actual2);
        }
    }
}

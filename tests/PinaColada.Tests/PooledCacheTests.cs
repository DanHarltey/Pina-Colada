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
            var cacheToTest = new PooledCache(new TestDictionaryCache());
            var testFetch = new TestFetcher();

            var actual = await cacheToTest.Fetch("test1", testFetch.Fetch, null);

            Assert.Equal(0, actual.Value);
            Assert.Equal(1, testFetch.Counter);
        }

        [Fact]
        public async Task CacheHitDoesNotCallsFectch()
        {
            var cacheToTest = new PooledCache(new TestDictionaryCache());
            var testFetch = new TestFetcher();

            var actual = await cacheToTest.Fetch("test1", testFetch.Fetch, null);
            var actual2 = await cacheToTest.Fetch("test1", testFetch.Fetch, null);

            Assert.Equal(0, actual.Value);
            Assert.Equal(0, actual2.Value);
            Assert.NotSame(actual, actual2);
            Assert.Same(actual.Value, actual2.Value);
            Assert.Equal(1, testFetch.Counter);
        }

        [Fact]
        public async Task CachePoolsRequests()
        {
            var cacheToTest = new PooledCache(new TestDictionaryCache());
            var testFetch = new TestFetcher(500);

            var actualTask = cacheToTest.Fetch("test1", testFetch.Fetch, null);
            var actual2Task = cacheToTest.Fetch("test1", testFetch.Fetch, null);

            var actual = await actualTask;
            var actual2 = await actual2Task;

            Assert.Equal(0, actual.Value);
            Assert.Equal(0, actual2.Value);
            Assert.Same(actual, actual2);
            Assert.Equal(1, testFetch.Counter);
        }

        [Fact]
        public async Task KeyRemovedFromPoolOnFetchException()
        {
            var cacheToTest = new PooledCache(new TestDictionaryCache());
            var testFetch = new TestFetcher(throwException: true);

            await Assert.ThrowsAsync<Exception>(() => cacheToTest.Fetch("test1", testFetch.Fetch, null));
            await Assert.ThrowsAsync<Exception>(() => cacheToTest.Fetch("test1", testFetch.Fetch, null));

            Assert.Equal(2, testFetch.Counter);
        }

        [Fact]
        public async Task KeyRemovedFromPoolOnGetException()
        {
            var cacheToTest = new PooledCache(new TestDictionaryCache(throwOnGet: true));
            var testFetch = new TestFetcher();

            var actual1 = await Assert.ThrowsAsync<Exception>(() => cacheToTest.Fetch("test1", testFetch.Fetch, null));
            var actual2 = await Assert.ThrowsAsync<Exception>(() => cacheToTest.Fetch("test1", testFetch.Fetch, null));

            Assert.Equal(0, testFetch.Counter);
            Assert.NotSame(actual1, actual2);
        }

        [Fact]
        public async Task KeyRemovedFromPoolOnSetException()
        {
            var cacheToTest = new PooledCache(new TestDictionaryCache(throwOnSet: true));
            var testFetch = new TestFetcher();

            await Assert.ThrowsAsync<Exception>(() => cacheToTest.Fetch("test1", testFetch.Fetch, null));
            await Assert.ThrowsAsync<Exception>(() => cacheToTest.Fetch("test1", testFetch.Fetch, null));

            Assert.Equal(2, testFetch.Counter);
        }

        [Fact]
        public async Task PoolReturnsFetchException()
        {
            var cacheToTest = new PooledCache(new TestDictionaryCache());
            var testFetch = new TestFetcher(delayTime: 500, throwException: true);

            var task1 = cacheToTest.Fetch("test1", testFetch.Fetch, null);
            var task2 = cacheToTest.Fetch("test1", testFetch.Fetch, null);

            var actual1 = await Assert.ThrowsAsync<Exception>(() => task1);
            var actual2 = await Assert.ThrowsAsync<Exception>(() => task2);

            Assert.Equal(1, testFetch.Counter);
            Assert.Same(actual1, actual2);
        }

        [Fact]
        public async Task PoolReturnsGetException()
        {
            var cache = new PooledCache(new TestDictionaryCache(delayTime: 10_000, throwOnSet: true));
            var testFetch = new TestFetcher();

            var task1 = cache.Fetch("test1", testFetch.Fetch, null);
            var task2 = cache.Fetch("test1", testFetch.Fetch, null);

            var actual1 = await Assert.ThrowsAsync<Exception>(() => task1);
            var actual2 = await Assert.ThrowsAsync<Exception>(() => task2);

            Assert.Equal(1, testFetch.Counter);
            Assert.Same(actual1, actual2);
        }
    }
}

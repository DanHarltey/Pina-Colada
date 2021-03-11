using PinaColada.Tests.Helpers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PinaColada.Tests.PooledCacheFeatures
{
    public class PoolsRequestsTests
    {
        [Fact]
        public async Task ConcurrentRequestsShareResult()
        {
            var cacheToTest = new PooledCache(new DictionaryCache());
            var counter = new Counter(delayTime: 500);

            var actualTask = cacheToTest.Fetch("test1", counter.Increment, null);
            var actual2Task = cacheToTest.Fetch("test1", counter.Increment, null);

            var actual = await actualTask;
            var actual2 = await actual2Task;

            Assert.Same(actual, actual2);
            Assert.Equal(1, counter.Count);
            Assert.Equal(0, actual.Value.Count);
        }

        [Fact]
        public async Task ConcurrentRequestsShareActionException()
        {
            var cacheToTest = new PooledCache(new DictionaryCache());
            var counter = new Counter(delayTime: 500, throwException: true);

            var task1 = cacheToTest.Fetch("test1", counter.Increment, null);
            var task2 = cacheToTest.Fetch("test1", counter.Increment, null);

            var actual1 = await Assert.ThrowsAsync<InvalidOperationException>(() => task1);
            var actual2 = await Assert.ThrowsAsync<InvalidOperationException>(() => task2);

            Assert.Equal(1, counter.Count);
            Assert.Same(actual1, actual2);
        }

        [Fact]
        public async Task ConcurrentRequestsShareGetException()
        {
            var cache = new PooledCache(new DictionaryCache(getDelay: 500, throwOnGet: true));
            var counter = new Counter();

            var task1 = cache.Fetch("test1", counter.Increment, null);
            var task2 = cache.Fetch("test1", counter.Increment, null);

            var actual1 = await Assert.ThrowsAsync<InvalidOperationException>(() => task1);
            var actual2 = await Assert.ThrowsAsync<InvalidOperationException>(() => task2);

            Assert.Equal(0, counter.Count);
            Assert.Same(actual1, actual2);
        }

        [Fact]
        public async Task CacheKeyIsRemovedFromPoolOnActionException()
        {
            var cacheToTest = new PooledCache(new DictionaryCache());
            var counter = new Counter(throwException: true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => cacheToTest.Fetch("test1", counter.Increment, null));
            await Assert.ThrowsAsync<InvalidOperationException>(() => cacheToTest.Fetch("test1", counter.Increment, null));

            Assert.Equal(2, counter.Count);
        }

        [Fact]
        public async Task CacheKeyIsRemovedFromPoolOnGetException()
        {
            var backingCache = new DictionaryCache(throwOnGet: true);
            var cacheToTest = new PooledCache(backingCache);
            var counter = new Counter();

            var actual1 = await Assert.ThrowsAsync<InvalidOperationException>(() => cacheToTest.Fetch("test1", counter.Increment, null));
            var actual2 = await Assert.ThrowsAsync<InvalidOperationException>(() => cacheToTest.Fetch("test1", counter.Increment, null));

            Assert.NotSame(actual1, actual2);
        }

        [Fact]
        public async Task CacheKeyIsRemovedFromPoolOnSetException()
        {
            var backingCache = new DictionaryCache(throwOnSet: true);
            var cacheToTest = new PooledCache(backingCache);
            var counter = new Counter();

            var result1 = await cacheToTest.Fetch("test1", counter.Increment, null);
            var exception1 = await Assert.ThrowsAsync<InvalidOperationException>(() => result1.SetTask);

            var result2 = await cacheToTest.Fetch("test1", counter.Increment, null);
            var exception2 = await Assert.ThrowsAsync<InvalidOperationException>(() => result2.SetTask);

            // action called twice as it was never set in cache
            Assert.Equal(2, counter.Count);

            // value was never set in cache
            Assert.NotSame(result1.Value, result2.Value);

            // Set task shouldn't be share
            Assert.NotSame(result1.SetTask, result2.SetTask);

            // exception shouldn't get cached
            Assert.NotSame(exception1, exception2);
        }

        [Fact]
        public async Task CacheKeyIsRemovedOnlyAfterSetComplete()
        {
            var backingCache = new DictionaryCache(setDelay: 500);
            var cacheToTest = new PooledCache(backingCache);
            var counter = new Counter();


            var result1 = await cacheToTest.Fetch("test1", counter.Increment, null);
            
            // the cache is still setting as it has a 500ms delay, this request should get pooled
            var result2 = await cacheToTest.Fetch("test1", counter.Increment, null);

            Assert.Same(result1.Value, result2.Value);
            Assert.Same(result1.SetTask, result2.SetTask);

            // action called once as it was pooled
            Assert.Equal(1, counter.Count);
        }
    }
}

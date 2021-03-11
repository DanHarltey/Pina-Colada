using PinaColada.Tests.Helpers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PinaColada.Tests.PooledCacheFeatures
{
    public class RetrievesAndSetsValuesTests
    {
        [Fact]
        public async Task RetrievesAndSetsValueWhenNotInCache()
        {
            var cacheToTest = new PooledCache(new DictionaryCache());
            var counter = new Counter();

            var actual = await cacheToTest.Fetch("test1", counter.Increment, null);

            Assert.Equal(0, actual.Value.Count);
            Assert.Equal(1, counter.Count);
        }

        [Fact]
        public async Task RetrievesValueFromCache()
        {
            var cacheToTest = new PooledCache(new DictionaryCache());
            var counter = new Counter();

            // places the value in the cache
            var actual = await cacheToTest.Fetch("test1", counter.Increment, null);
            
            // gets the value from the cache
            var actual2 = await cacheToTest.Fetch("test1", counter.Increment, null);

            Assert.Same(actual.Value, actual2.Value);

            // only called once
            Assert.Equal(1, counter.Count);

            // correct value
            Assert.Equal(0, actual.Value.Count);
        }

    }
}

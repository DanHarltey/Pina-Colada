using PinaColada.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PinaColada.Tests.PooledCacheFeatures
{
    public class ReturnResultBeforeSetTests
    {
        [Fact]
        public async Task ReturnResultBeforeSet()
        {
            TimeSpan setDelay = TimeSpan.FromMilliseconds(500);
            var timer = Stopwatch.StartNew();
            var backingCache = new DictionaryCache(setDelay: (int)setDelay.TotalMilliseconds);

            var cache = new PooledCache(backingCache);
            var counter = new Counter();

            var actual = await cache.Fetch("test1", counter.Increment, null);

            Assert.Equal(1, counter.Count);
            Assert.True(timer.ElapsedMilliseconds <= setDelay.TotalMilliseconds, "Waited for set task when shouldn't");
        }

        [Fact]
        public async Task CanAwaitSetTask()
        {
            var counter = new Counter();

            var timer = Stopwatch.StartNew();

            TimeSpan setDelay = TimeSpan.FromMilliseconds(500);
            var backingCache = new DictionaryCache(setDelay: (int)setDelay.TotalMilliseconds);
            var cache = new PooledCache(backingCache);

            var actual = await cache.Fetch("test1", counter.Increment, null);
            await actual.SetTask;

            Assert.Equal(1, counter.Count);
            Assert.True(timer.ElapsedMilliseconds >= setDelay.TotalMilliseconds, "Did not wait for set task");
        }
    }
}

using System;
using System.Threading.Tasks;

namespace PinaColada.Tests
{
    internal class TestFetcher
    {
        private readonly int _delayTime;
        private readonly bool _throwException;
        private int _counter;

        public TestFetcher(int delayTime = 0, bool throwException = false)
        {
            _delayTime = delayTime;
            _throwException = throwException;
        }

        public int Counter => _counter;

        public async Task<object> Fetch()
        {
            if (_delayTime != 0)
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

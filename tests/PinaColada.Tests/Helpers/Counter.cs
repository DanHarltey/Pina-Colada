using System;
using System.Threading.Tasks;

namespace PinaColada.Tests.Helpers
{
    internal class Counter : IEquatable<int>
    {
        private readonly int _delayTime;
        private readonly bool _throwException;
        private int _counter;

        public Counter(int delayTime = 0, bool throwException = false)
        {
            _delayTime = delayTime;
            _throwException = throwException;
        }

        private Counter(int value) => _counter = value;

        public int Count => _counter;

        public async Task<Counter> Increment()
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

            return new Counter(tmp);
        }


        bool IEquatable<int>.Equals(int other)
        {
            throw new NotImplementedException();
        }
    }
}

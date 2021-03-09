using System.Threading.Tasks;

namespace PinaColada
{
    public sealed class Result<T>
    {
        private static readonly Result<T> _cacheMiss = new Result<T>(false, default, Task.CompletedTask);
        private static readonly Task<Result<T>> _cacheMissTask = Task.FromResult(_cacheMiss);

        public bool CacheHit { get; }
        public T Value { get; }
        public Task SetTask { get; }

        public static Result<T> CacheMiss => _cacheMiss;

        public static Task<Result<T>> CaheMissTask => _cacheMissTask;

        public static Task<Result<T>> CaheHitTask(T value) => Task.FromResult(CaheHit(value));

        public static Result<T> CaheHit(T value) => CaheHit( value, Task.CompletedTask);

        public static Result<T> CaheHit(T value, Task setTask) => new Result<T>(true, value, setTask);

        internal Result(bool cacheHit, T value, Task setTask)
        {
            CacheHit = cacheHit;
            Value = value;
            SetTask = setTask;
        }
    }
}

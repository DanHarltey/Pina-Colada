using System.Threading.Tasks;

namespace PinaColada
{
    public static class Result
    {
        public static Task<Result<T>> CacheHitTask<T>(T value) => Task.FromResult(CacheHit(value));

        public static Result<T> CacheHit<T>(T value) => CacheHit(value, Task.CompletedTask);

        public static Result<T> CacheHit<T>(T value, Task setTask) => new Result<T>(true, value, setTask);
    }

    public sealed class Result<T>
    {
        private static readonly Result<T> _cacheMiss = new Result<T>(false, default, Task.CompletedTask);
        private static readonly Task<Result<T>> _cacheMissTask = Task.FromResult(_cacheMiss);

        public static Result<T> CacheMiss => _cacheMiss;

        public static Task<Result<T>> CacheMissTask => _cacheMissTask;

        public bool CacheHit { get; }
        public T Value { get; }
        public Task SetTask { get; }

        internal Result(bool cacheHit, T value, Task setTask)
        {
            CacheHit = cacheHit;
            Value = value;
            SetTask = setTask;
        }
    }
}

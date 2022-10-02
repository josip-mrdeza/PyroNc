using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Pyro.Nc.Simulation
{
    public static class Sim3DStats
    {
        private static Stopwatch _stopwatch;
        /// <summary>
        /// Measures the time taken by executing an async function.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static async Task<TimeSpan> MeasureAsyncTime(this Task task)
        {
            _stopwatch = Stopwatch.StartNew();
            await task;
            _stopwatch.Stop();
            return _stopwatch.Elapsed;
        }
        
        /// <summary>
        /// Measures the time taken by executing an async function.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static async Task<MeasureResult<T>> MeasureAsyncTime<T>(this Task<T> task)
        {
            _stopwatch = Stopwatch.StartNew();
            var result = await task;
            _stopwatch.Stop();
            return new MeasureResult<T>(_stopwatch.Elapsed, result);
        }

        public struct MeasureResult<T>
        {
            public TimeSpan Time;
            public T Result;

            public MeasureResult(TimeSpan time, T result)
            {
                Time = time;
                Result = result;
            }
        }
    }
}
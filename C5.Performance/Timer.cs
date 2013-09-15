// Peter Sestoft
namespace C5.Performance {
    public class Timer {
        private readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        public Timer() { Play(); }
        /// <summary>
        /// Return the time passed in seconds.
        /// </summary>
        /// <returns></returns>
        public double Check() { return stopwatch.ElapsedMilliseconds / 1000.0; }
        public void Pause() { stopwatch.Stop(); }
        public void Play() { stopwatch.Start(); }
    }
}
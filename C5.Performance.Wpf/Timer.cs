namespace C5.Performance.Wpf
{
    public class Timer
    {
        private readonly System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();

        public Timer(bool startTimer = false)
        {
            if (startTimer)
                Play();
        }

        /// <summary>
        /// Return the time passed in seconds.
        /// </summary>
        /// <returns></returns>
        public double Check()
        {
            return _stopwatch.ElapsedMilliseconds / 1000.0;
        }

        public void Pause()
        {
            _stopwatch.Stop();
        }

        public void Play()
        {
            _stopwatch.Start();
        }
    }
}
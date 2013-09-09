namespace C5.Performance
{
    internal abstract class ProfilingBase
    {
        /// <summary>
        /// Value that decides how many times <see cref="MultipleProfilingRuns"/> runs <see cref="SingleProfilingRun"/>.
        /// </summary>
        public int NumberOfRuns = 1000;

        /// <summary>
        /// Runs the <see cref="SingleProfilingRun"/> method for <see cref="NumberOfRuns"/> times. The standard value for <see cref="NumberOfRuns"/> is 1000.
        /// </summary>
        public void MultipleProfilingRuns()
        {
            for (var i = 0; i < this.NumberOfRuns; i++)
            {
                this.SingleProfilingRun();
            }
        }

        /// <summary>
        /// Insert the code you would like to be profiled for a single run here.
        /// </summary>
        public abstract void SingleProfilingRun();
    }
}
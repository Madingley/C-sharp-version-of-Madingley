using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Timing
{
    /// <summary>
    /// Timer to track time elapsed
    /// </summary>
    public class StopWatch
    {
        /// <summary>
        /// The time accumulated by a given stopwatch instance
        /// </summary>
        private double _AccumulatedTime;
        /// <summary>
        /// Get or set the time accumulated by a given stopwatch instance
        /// </summary>
        public double AccumulatedTime
        {
            get { return _AccumulatedTime; }
            set { _AccumulatedTime = value; }
        }
        
        /// <summary>
        /// The start time of a given stopwatch run
        /// </summary>
        private DateTime startTime;
        /// <summary>
        /// The stop time of a given stopwatch run
        /// </summary>
        private DateTime stopTime;
        /// <summary>
        /// Whether the stopwatch is running
        /// </summary>
        private bool running = false;

        /// <summary>
        /// Start the stopwatch
        /// </summary>
        public void Start()
        {
            // Set the start time for the stopwatch run
            this.startTime = DateTime.Now;
            // Set the stopwatch as being running
            this.running = true;
        }

        /// <summary>
        /// Stop the stopwatch
        /// </summary>
        public void Stop()
        {
            // Set the stop time for the stopwatch run
            this.stopTime = DateTime.Now;
            // Set the stopwatch as being not running
            this.running = false;
            // Calculate the time elapsed during this stopwatch run
            TimeSpan interval = this.stopTime - this.startTime;
            // Update the time accumulated by this stopwatch instance
            this._AccumulatedTime += interval.TotalSeconds;

        } 




        /// <summary>
        /// Get the non-cumulative elapsed time of a stopwatch run in milliseconds
        /// </summary>
        /// <returns>Elapsed time since stopwatch started in milliseconds</returns>
        public double GetElapsedTime()
        {
            // Holds the time elapsed
            TimeSpan interval;

            // If the stopwatch is running, then calculate time since the stopwatch started, otherwise calculate the time elapsed during the last stopwatch run
            if (running)
                interval = DateTime.Now - startTime;
            else
                interval = stopTime - startTime;

            // Return the elapsed time in milliseconds
            return interval.TotalMilliseconds;
        }

        /// <summary>
        /// Get the non-cumulative elapsed time of a stopwatch run in seconds
        /// </summary>
        /// <returns>Elapsed time since stopwatch started in seconds</returns>
        public double GetElapsedTimeSecs()
        {
            // Holds the time elapsed
            TimeSpan interval;

            // If the stopwatch is running, then calculate time since the stopwatch started, otherwise calculate the time elapsed during the last stopwatch run
            if (running)
                interval = DateTime.Now - startTime;
            else
                interval = stopTime - startTime;

            // Return the elapsed time in seconds
            return interval.TotalSeconds;
        }

    }
}

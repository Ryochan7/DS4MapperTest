using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.Common
{
    public class GyroAverageWindow
    {
        public int x;
        public int y;
        public int z;
        public double accelMagnitude;
        public int numSamples;
        public DateTime start;
        public DateTime stop;

        public int DurationMs   // property
        {
            get
            {
                TimeSpan timeDiff = stop - start;
                return Convert.ToInt32(timeDiff.TotalMilliseconds);
            }
        }

        public GyroAverageWindow()
        {
            Reset();
        }

        public void Reset()
        {
            x = y = z = numSamples = 0;
            accelMagnitude = 0.0;
            start = stop = DateTime.UtcNow;
        }

        public bool StopIfElapsed(int ms)
        {
            DateTime end = DateTime.UtcNow;
            TimeSpan timeDiff = end - start;
            bool shouldStop = Convert.ToInt32(timeDiff.TotalMilliseconds) >= ms;
            if (!shouldStop) stop = end;
            return shouldStop;
        }
        public double GetWeight(int expectedMs)
        {
            if (expectedMs == 0) return 0;
            return Math.Min(1.0, DurationMs / expectedMs);
        }
    }
}

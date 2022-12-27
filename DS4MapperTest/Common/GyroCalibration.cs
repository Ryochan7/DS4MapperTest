using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.Common
{
    public class GyroCalibration
    {
        // for continuous calibration (JoyShockLibrary)
        const int num_gyro_average_windows = 3;
        private int gyro_average_window_front_index = 0;
        const int gyro_average_window_ms = 5000;
        private GyroAverageWindow[] gyro_average_window = new GyroAverageWindow[num_gyro_average_windows];
        public int gyro_offset_x = 0;
        public int gyro_offset_y = 0;
        public int gyro_offset_z = 0;
        public double gyro_accel_magnitude = 1.0f;
        public Stopwatch gyroAverageTimer = new Stopwatch();

        public long CntCalibrating
        {
            get
            {
                return gyroAverageTimer.IsRunning ? gyroAverageTimer.ElapsedMilliseconds : 0;
            }
        }

        public GyroCalibration()
        {
            StartContinuousCalibration();
        }

        public unsafe void CalcSensorCamples(ref int currentYaw, ref int currentPitch, ref int currentRoll, ref int AccelX, ref int AccelY, ref int AccelZ)
        {
            unchecked
            {
                double accelMag = Math.Sqrt(AccelX * AccelX + AccelY * AccelY + AccelZ * AccelZ);
                PushSensorSamples(currentYaw, currentPitch, currentRoll, (float)accelMag);
                if (gyroAverageTimer.ElapsedMilliseconds > 5000L)
                {
                    gyroAverageTimer.Stop();
                    AverageGyro(ref gyro_offset_x, ref gyro_offset_y, ref gyro_offset_z, ref gyro_accel_magnitude);
#if TRACE
                    Trace.WriteLine(string.Format("AverageGyro {0} {1} {2} {3}", gyro_offset_x, gyro_offset_y, gyro_offset_z, gyro_accel_magnitude));
#endif
                }
            }
        }

        public void StartContinuousCalibration()
        {
            for (int i = 0; i < gyro_average_window.Length; i++) gyro_average_window[i] = new GyroAverageWindow();
            gyroAverageTimer.Start();
        }

        public void StopContinuousCalibration()
        {
            gyroAverageTimer.Stop();
            gyroAverageTimer.Reset();
            for (int i = 0; i < gyro_average_window.Length; i++) gyro_average_window[i].Reset();
        }

        public void ResetContinuousCalibration()
        {
            // Potential race condition with CalcSensorCamples() since this method is called after checking gyroAverageTimer.IsRunning == true
            StopContinuousCalibration();
            StartContinuousCalibration();
        }

        public unsafe void PushSensorSamples(int x, int y, int z, double accelMagnitude)
        {
            // push samples
            GyroAverageWindow windowPointer = gyro_average_window[gyro_average_window_front_index];

            if (windowPointer.StopIfElapsed(gyro_average_window_ms))
            {
#if TRACE
                Trace.WriteLine(string.Format("GyroAvg[{0}], numSamples: {1}", gyro_average_window_front_index,
                    windowPointer.numSamples));
#endif

                // next
                gyro_average_window_front_index = (gyro_average_window_front_index + num_gyro_average_windows - 1) % num_gyro_average_windows;
                windowPointer = gyro_average_window[gyro_average_window_front_index];
                windowPointer.Reset();
            }
            // accumulate
            windowPointer.numSamples++;
            windowPointer.x += x;
            windowPointer.y += y;
            windowPointer.z += z;
            windowPointer.accelMagnitude += accelMagnitude;
        }

        public void AverageGyro(ref int x, ref int y, ref int z, ref double accelMagnitude)
        {
            double weight = 0.0;
            double totalX = 0.0;
            double totalY = 0.0;
            double totalZ = 0.0;
            double totalAccelMagnitude = 0.0;

            int wantedMs = 5000;
            for (int i = 0; i < num_gyro_average_windows && wantedMs > 0; i++)
            {
                int cycledIndex = (i + gyro_average_window_front_index) % num_gyro_average_windows;
                GyroAverageWindow windowPointer = gyro_average_window[cycledIndex];
                if (windowPointer.numSamples == 0 || windowPointer.DurationMs == 0) continue;

                double thisWeight;
                double fNumSamples = windowPointer.numSamples;
                if (wantedMs < windowPointer.DurationMs)
                {
                    thisWeight = (float)wantedMs / windowPointer.DurationMs;
                    wantedMs = 0;
                }
                else
                {
                    thisWeight = windowPointer.GetWeight(gyro_average_window_ms);
                    wantedMs -= windowPointer.DurationMs;
                }

                totalX += (windowPointer.x / fNumSamples) * thisWeight;
                totalY += (windowPointer.y / fNumSamples) * thisWeight;
                totalZ += (windowPointer.z / fNumSamples) * thisWeight;
                totalAccelMagnitude += (windowPointer.accelMagnitude / fNumSamples) * thisWeight;
                weight += thisWeight;
            }

            if (weight > 0.0)
            {
                x = (int)(totalX / weight);
                y = (int)(totalY / weight);
                z = (int)(totalZ / weight);
                accelMagnitude = totalAccelMagnitude / weight;
            }
        }
    }
}

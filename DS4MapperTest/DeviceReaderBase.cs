using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS4MapperTest
{
    public abstract class DeviceReaderBase
    {
        protected ManualResetEventSlim readWaitEv = new ManualResetEventSlim();
        public ManualResetEventSlim ReadWaitEv { get => readWaitEv; }

        protected bool fireReport = true;

        public abstract void StartUpdate();
        public abstract void StopUpdate();
        public abstract void WriteRumbleReport();

        /// <summary>
        /// Must not be run from input thread. Waits for input thread to be in a wait state
        /// and then tell thread to no longer invoke the Report event. Input thread will then
        /// resume followed by invoking the action passed. Flag will be set to have
        /// Report event to resume being invoked after
        /// </summary>
        /// <param name="act">Action to execute in current thread</param>
        public void HaltReportingRunAction(Action act)
        {
            // Wait for controller to be in a wait period
            bool result = readWaitEv.Wait(millisecondsTimeout: 500);
            if (result)
            {
                readWaitEv.Reset();

                // Tell device to no longer fire reports
                fireReport = false;

                // Flag is set. Allow input thread to resume
                readWaitEv.Set();

                // Invoke main desired action
                act?.Invoke();

                // Start firing reports again
                fireReport = true;
            }
        }
    }
}

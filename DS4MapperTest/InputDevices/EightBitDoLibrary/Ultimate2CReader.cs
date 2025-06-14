using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;

namespace DS4MapperTest.InputDevices.EightBitDoLibrary
{
    internal class Ultimate2CReader : DeviceReaderBase
    {
        private Ultimate2CDevice device;
        private Thread inputThread;
        private bool activeInputLoop = false;
        private byte[] inputReportBuffer;
        private byte[] outputReportBuffer;
        private bool started;

        public delegate void Ultimate2CDeviceReportDelegate(Ultimate2CReader sender,
            Ultimate2CDevice device);
        public event Ultimate2CDeviceReportDelegate Report;

        public Ultimate2CReader(Ultimate2CDevice device)
        {
            this.device = device;
            inputReportBuffer = new byte[device.InputReportLen];
            outputReportBuffer = new byte[device.OutputReportLen];
        }

        public override void StartUpdate()
        {
            if (started)
            {
                return;
            }

            device.SetOperational();
            //device.PrepareOutputReport(outputReportBuffer);
            //device.WriteReport(outputReportBuffer);

            inputThread = new Thread(ReadInput);
            inputThread.IsBackground = true;
            inputThread.Priority = ThreadPriority.AboveNormal;
            inputThread.Name = "8BitDo Ultimate 2C Reader Thread";
            inputThread.Start();

            started = true;
        }

        private void ReadInput(object obj)
        {
            activeInputLoop = true;

            byte tempByte;

            long currentTime = Stopwatch.GetTimestamp();
            long previousTime = Stopwatch.GetTimestamp();
            long deltaElapsed = 0;
            double lastTimeElapsedDouble = 0.0;
            double lastElapsed;
            double tempTimeElapsed;
            DateTime utcNow = DateTime.UtcNow;
            bool firstReport = true;

            unchecked
            {
                while(activeInputLoop)
                {
                    HidDevice.ReadStatus res = device.HidDevice.ReadWithFileStream(inputReportBuffer, 100);
                    if (res != HidDevice.ReadStatus.Success && res != HidDevice.ReadStatus.WaitTimedOut)
                    {
                        activeInputLoop = false;
                        device.RaiseRemoval();
                        continue;
                    }

                    ref Ultimate2CState current = ref device.CurrentStateRef;
                    ref Ultimate2CState previous = ref device.PreviousStateRef;

                    if (res == HidDevice.ReadStatus.WaitTimedOut)
                    {
                        current.Battery = (byte)100;
                        if (current.Battery != previous.Battery)
                        {
                            // Send the BatteryChanged event
                            device.Battery = current.Battery;
                        }
                    }

                    currentTime = Stopwatch.GetTimestamp();
                    deltaElapsed = currentTime - previousTime;
                    lastElapsed = deltaElapsed * (1.0 / Stopwatch.Frequency) * 1000.0;
                    tempTimeElapsed = lastElapsed * .001;

                    current.timeElapsed = tempTimeElapsed;
                    previousTime = currentTime;

                    utcNow = DateTime.UtcNow; // timestamp with UTC in case system time zone changes
                    current.PacketCounter = previous.PacketCounter + 1;
                    current.ReportTimeStamp = utcNow;

                    //Trace.WriteLine($"TIME {current.timeElapsed}");

                    tempByte = inputReportBuffer[1];
                    current.A = (tempByte & (1 << 0)) != 0;
                    current.B = (tempByte & (1 << 1)) != 0;
                    current.L4 = (tempByte & (1 << 2)) != 0;
                    current.X = (tempByte & (1 << 3)) != 0;
                    current.Y = (tempByte & (1 << 4)) != 0;
                    current.R4 = (tempByte & (1 << 5)) != 0;
                    current.L1 = (tempByte & (1 << 6)) != 0;
                    current.R1 = (tempByte & (1 << 7)) != 0;

                    tempByte = inputReportBuffer[2];
                    current.LTBtn = (tempByte & (1 << 0)) != 0;
                    current.RTBtn = (tempByte & (1 << 1)) != 0;
                    current.Minus = (tempByte & (1 << 2)) != 0;
                    current.Plus = (tempByte & (1 << 3)) != 0;
                    current.Guide = (tempByte & (1 << 4)) != 0;
                    current.L3 = (tempByte & (1 << 5)) != 0;
                    current.R3 = (tempByte & (1 << 6)) != 0;

                    tempByte = inputReportBuffer[3];
                    // First 4 bits denote dpad state. Clock representation
                    // with 15 meaning centered and 0 meaning DpadUp.
                    byte dpad_state = (byte)(tempByte & 0x0F);

                    switch (dpad_state)
                    {
                        case 0: current.DpadUp = true; current.DpadDown = false; current.DpadLeft = false; current.DpadRight = false; break;
                        case 1: current.DpadUp = true; current.DpadDown = false; current.DpadLeft = false; current.DpadRight = true; break;
                        case 2: current.DpadUp = false; current.DpadDown = false; current.DpadLeft = false; current.DpadRight = true; break;
                        case 3: current.DpadUp = false; current.DpadDown = true; current.DpadLeft = false; current.DpadRight = true; break;
                        case 4: current.DpadUp = false; current.DpadDown = true; current.DpadLeft = false; current.DpadRight = false; break;
                        case 5: current.DpadUp = false; current.DpadDown = true; current.DpadLeft = true; current.DpadRight = false; break;
                        case 6: current.DpadUp = false; current.DpadDown = false; current.DpadLeft = true; current.DpadRight = false; break;
                        case 7: current.DpadUp = true; current.DpadDown = false; current.DpadLeft = true; current.DpadRight = false; break;
                        case 8:
                        default: current.DpadUp = false; current.DpadDown = false; current.DpadLeft = false; current.DpadRight = false; break;
                    }

                    current.LX = inputReportBuffer[4];
                    current.LY = inputReportBuffer[5];
                    current.RX = inputReportBuffer[6];
                    current.RY = inputReportBuffer[7];
                    current.RT = inputReportBuffer[8];
                    current.LT = inputReportBuffer[9];

                    current.Battery = (byte)100;
                    if (current.Battery != previous.Battery)
                    {
                        // Send the BatteryChanged event
                        device.Battery = current.Battery;
                    }

                    Report?.Invoke(this, device);

                    if (device.RumbleDirty)
                    {
                        WriteRumbleReport();
                        device.RumbleDirty = false;
                    }

                    device.SyncStates();

                    firstReport = false;
                }
            }

            activeInputLoop = false;
        }

        public override void StopUpdate()
        {
            if (!started)
            {
                return;
            }

            activeInputLoop = false;

            device.PurgeRemoval();
            device.HidDevice.CancelIO();
            if (inputThread != null && inputThread.IsAlive &&
                Thread.CurrentThread != inputThread)
            {
                inputThread.Join();
            }

            started = false;
        }

        public override void WriteRumbleReport()
        {
            device.PrepareOutputReport(outputReportBuffer);
            device.WriteReport(outputReportBuffer);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4MapperTest.Common;
using HidLibrary;

namespace DS4MapperTest.InputDevices.EightBitDoLibrary
{
    internal class Ultimate2WirelessReader : DeviceReaderBase
    {
        private Ultimate2WirelessDevice device;
        private Thread inputThread;
        private bool activeInputLoop = false;
        private byte[] inputReportBuffer;
        private byte[] outputReportBuffer;
        private GyroCalibration gyroCalibrationUtil = new GyroCalibration();
        private bool started;

        public delegate void Ultimate2WirelessDeviceReportDelegate(Ultimate2WirelessReader sender,
            Ultimate2WirelessDevice device);
        public event Ultimate2WirelessDeviceReportDelegate Report;

        public Ultimate2WirelessReader(Ultimate2WirelessDevice device)
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
            inputThread.Name = "8BitDo Ultimate 2 Wireless Reader Thread";
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

            // Run continuous calibration on Gyro when starting input loop
            gyroCalibrationUtil.ResetContinuousCalibration();

            unchecked
            {
                while(activeInputLoop)
                {
                    HidDevice.ReadStatus res = device.HidDevice.ReadWithFileStream(inputReportBuffer);
                    if (res != HidDevice.ReadStatus.Success)
                    {
                        activeInputLoop = false;
                        device.RaiseRemoval();
                        continue;
                    }

                    ref Ultimate2WirelessState current = ref device.CurrentStateRef;
                    ref Ultimate2WirelessState previous = ref device.PreviousStateRef;

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

                    current.LX = inputReportBuffer[2];
                    current.LY = inputReportBuffer[3];
                    current.RX = inputReportBuffer[4];
                    current.RY = inputReportBuffer[5];
                    current.RT = inputReportBuffer[6];
                    current.LT = inputReportBuffer[7];

                    tempByte = inputReportBuffer[8];
                    current.A = (tempByte & (1 << 0)) != 0;
                    current.B = (tempByte & (1 << 1)) != 0;
                    current.PR = (tempByte & (1 << 2)) != 0;
                    current.X = (tempByte & (1 << 3)) != 0;
                    current.Y = (tempByte & (1 << 4)) != 0;
                    current.PL = (tempByte & (1 << 5)) != 0;
                    current.L1 = (tempByte & (1 << 6)) != 0;
                    current.R1 = (tempByte & (1 << 7)) != 0;

                    tempByte = inputReportBuffer[9];
                    current.LTBtn = (tempByte & (1 << 0)) != 0;
                    current.RTBtn = (tempByte & (1 << 1)) != 0;
                    current.Minus = (tempByte & (1 << 2)) != 0;
                    current.Plus = (tempByte & (1 << 3)) != 0;
                    current.Guide = (tempByte & (1 << 4)) != 0;
                    current.L3 = (tempByte & (1 << 5)) != 0;
                    current.R3 = (tempByte & (1 << 6)) != 0;

                    tempByte = inputReportBuffer[10];
                    current.L4 = (tempByte & (1 << 0)) != 0;
                    current.R4 = (tempByte & (1 << 1)) != 0;

                    int AccelY = (short)((ushort)(inputReportBuffer[16] << 8) | inputReportBuffer[15]); // Pitch
                    int AccelX = (short)((ushort)(inputReportBuffer[18] << 8) | inputReportBuffer[17]); // Yaw
                    int AccelZ = (short)((ushort)(inputReportBuffer[20] << 8) | inputReportBuffer[19]); // Roll

                    int currentRoll = (short)((ushort)(inputReportBuffer[22] << 8) | inputReportBuffer[21]); // Roll
                    int currentPitch = (short)((ushort)(inputReportBuffer[24] << 8) | inputReportBuffer[23]); // Pitch
                    int currentYaw = (short)((ushort)(inputReportBuffer[26] << 8) | inputReportBuffer[25]); // Yaw

                    //Trace.WriteLine($"X: {AccelX} | Y: {AccelY} | Z: {AccelZ}");
                    //Trace.WriteLine($"P: {currentPitch} | Y: {currentYaw} | R: {currentRoll}");
                    //Trace.WriteLine("");

                    if (gyroCalibrationUtil.gyroAverageTimer.IsRunning)
                    {
                        gyroCalibrationUtil.CalcSensorCamples(ref currentYaw, ref currentPitch, ref currentRoll,
                            ref AccelX, ref AccelY, ref AccelZ);
                    }

                    currentYaw -= gyroCalibrationUtil.gyro_offset_x;
                    currentPitch -= gyroCalibrationUtil.gyro_offset_y;
                    currentRoll -= gyroCalibrationUtil.gyro_offset_z;

                    current.Motion.GyroYaw = (short)-currentYaw;
                    current.Motion.GyroPitch = (short)-currentPitch;
                    current.Motion.GyroRoll = (short)currentRoll;
                    //current.Motion.AngGyroYaw = current.Motion.GyroYaw / Ultimate2WirelessState.Ult2Motion.F_GYRO_RES_IN_DEG_SEC;
                    //current.Motion.AngGyroPitch = current.Motion.GyroPitch / Ultimate2WirelessState.Ult2Motion.F_GYRO_RES_IN_DEG_SEC;
                    //current.Motion.AngGyroRoll = current.Motion.GyroRoll / Ultimate2WirelessState.Ult2Motion.F_GYRO_RES_IN_DEG_SEC;

                    current.Motion.AccelX = (short)AccelX;
                    current.Motion.AccelY = (short)AccelY;
                    current.Motion.AccelZ = (short)AccelZ;
                    current.Motion.AccelXG = current.Motion.AccelX / Ultimate2WirelessState.Ult2Motion.F_ACC_RES_PER_G;
                    current.Motion.AccelYG = current.Motion.AccelY / Ultimate2WirelessState.Ult2Motion.F_ACC_RES_PER_G;
                    current.Motion.AccelZG = current.Motion.AccelZ / Ultimate2WirelessState.Ult2Motion.F_ACC_RES_PER_G;

                    Report?.Invoke(this, device);

                    if (device.RumbleDirty)
                    {
                        WriteRumbleReport();
                        device.RumbleDirty = false;
                    }

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

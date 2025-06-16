using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;
using DS4MapperTest.DS4Library;
using DS4MapperTest.Common;

namespace DS4MapperTest.DualSense
{
    public class DualSenseReader : DeviceReaderBase
    {
        private DualSenseDevice device;
        public DualSenseDevice Device => device;

        private Thread inputThread;
        private bool activeInputLoop = false;
        private byte[] inputReportBuffer;
        private byte[] outputReportBuffer;
        //private byte[] rumbleReportBuffer;
        private GyroCalibration gyroCalibrationUtil = new GyroCalibration();
        private bool started;

        public delegate void DualSenseDeviceReportDelegate(DualSenseReader sender,
            DualSenseDevice device);
        public event DualSenseDeviceReportDelegate Report;

        public DualSenseReader(DualSenseDevice device)
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
            device.PrepareOutputReport(outputReportBuffer);
            device.WriteReport(outputReportBuffer);

            inputThread = new Thread(ReadInput);
            inputThread.IsBackground = true;
            inputThread.Priority = ThreadPriority.AboveNormal;
            inputThread.Name = "DualSense Reader Thread";
            inputThread.Start();

            started = true;
        }

        private void ReadInput()
        {
            activeInputLoop = true;

            byte tempByte;
            int tempBattery = 0;
            bool tempCharging = false, charging = false;
            short battery;
            int maxBatteryValue = 0;
            bool tempFull;

            uint tempStamp = 0;
            bool timeStampInit = false;
            uint timeStampPrevious = 0;
            uint deltaTimeCurrent = 0;
            uint tempDelta = 0;
            double elapsedDeltaTime = 0.0;

            long currentTime = 0;
            long previousTime = 0;
            long deltaElapsed = 0;
            double lastTimeElapsedDouble = 0.0;
            double lastElapsed;
            double tempTimeElapsed;
            DateTime utcNow = DateTime.UtcNow;
            bool firstReport = true;

            int reportOffset =
                device.DevConnectionType == DualSenseDevice.ConnectionType.Bluetooth ? 1 : 0;

            // Run continuous calibration on Gyro when starting input loop
            gyroCalibrationUtil.ResetContinuousCalibration();

            unchecked
            {
                while (activeInputLoop)
                {
                    readWaitEv.Set();

                    if (device.DevConnectionType == DualSenseDevice.ConnectionType.Bluetooth)
                    {
                        HidDevice.ReadStatus res = device.HidDevice.ReadWithFileStream(inputReportBuffer);
                        if (res != HidDevice.ReadStatus.Success)
                        {
                            activeInputLoop = false;
                            readWaitEv.Reset();
                            device.RaiseRemoval();
                            continue;
                        }
                    }
                    else
                    {
                        HidDevice.ReadStatus res = device.HidDevice.ReadWithFileStream(inputReportBuffer);
                        if (res != HidDevice.ReadStatus.Success)
                        {
                            activeInputLoop = false;
                            readWaitEv.Reset();
                            device.RaiseRemoval();
                            continue;
                        }
                    }

                    readWaitEv.Wait();
                    readWaitEv.Reset();

                    ref DualSenseState current = ref device.CurrentStateRef;
                    ref DualSenseState previous = ref device.PreviousStateRef;

                    currentTime = Stopwatch.GetTimestamp();
                    deltaElapsed = currentTime - previousTime;
                    lastElapsed = deltaElapsed * (1.0 / Stopwatch.Frequency) * 1000.0;
                    tempTimeElapsed = lastElapsed * .001;

                    utcNow = DateTime.UtcNow; // timestamp with UTC in case system time zone changes
                    current.PacketCounter = previous.PacketCounter + 1;
                    current.ReportTimeStamp = utcNow;

                    current.LX = inputReportBuffer[1 + reportOffset];
                    current.LY = inputReportBuffer[2 + reportOffset];
                    current.RX = inputReportBuffer[3 + reportOffset];
                    current.RY = inputReportBuffer[4 + reportOffset];
                    current.L2 = inputReportBuffer[5 + reportOffset];
                    current.R2 = inputReportBuffer[6 + reportOffset];

                    // DS4 Frame Counter range is [0-127]. DS version range is [0-255]. Convert
                    //current.FrameCounter = (byte)(inputReport[7 + reportOffset] % 128);
                    tempByte = inputReportBuffer[8 + reportOffset];
                    current.Triangle = (tempByte & (1 << 7)) != 0;
                    current.Circle = (tempByte & (1 << 6)) != 0;
                    current.Cross = (tempByte & (1 << 5)) != 0;
                    current.Square = (tempByte & (1 << 4)) != 0;

                    // First 4 bits denote dpad state. Clock representation
                    // with 8 meaning centered and 0 meaning DpadUp.
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

                    tempByte = inputReportBuffer[9 + reportOffset];
                    current.R3 = (tempByte & (1 << 7)) != 0;
                    current.L3 = (tempByte & (1 << 6)) != 0;
                    current.Options = (tempByte & (1 << 5)) != 0;
                    current.Create = (tempByte & (1 << 4)) != 0;
                    current.R2Btn = (tempByte & (1 << 3)) != 0;
                    current.L2Btn = (tempByte & (1 << 2)) != 0;
                    current.R1 = (tempByte & (1 << 1)) != 0;
                    current.L1 = (tempByte & (1 << 0)) != 0;

                    tempByte = inputReportBuffer[10 + reportOffset];
                    current.PS = (tempByte & (1 << 0)) != 0;
                    current.TouchClickButton = (tempByte & 0x02) != 0;
                    current.Mute = (tempByte & (1 << 2)) != 0;

                    // Extra DualSense Edge buttons
                    current.FnL = (tempByte & (1 << 4)) != 0;
                    current.FnR = (tempByte & (1 << 5)) != 0;
                    current.BLP = (tempByte & (1 << 6)) != 0;
                    current.BRP = (tempByte & (1 << 7)) != 0;

                    //tempStamp = (uint)((ushort)(inputReportBuffer[11 + reportOffset] << 8) | inputReportBuffer[10 + reportOffset]);
                    tempStamp = inputReportBuffer[28 + reportOffset] |
                                (uint)(inputReportBuffer[29 + reportOffset] << 8) |
                                (uint)(inputReportBuffer[30 + reportOffset] << 16) |
                                (uint)(inputReportBuffer[31 + reportOffset] << 24);
                    if (timeStampInit == false)
                    {
                        timeStampInit = true;
                        deltaTimeCurrent = tempStamp * 1u / 3u;
                    }
                    else if (timeStampPrevious > tempStamp)
                    {
                        tempDelta = uint.MaxValue - timeStampPrevious + tempStamp + 1u;
                        deltaTimeCurrent = tempDelta * 1u / 3u;
                    }
                    else
                    {
                        tempDelta = tempStamp - timeStampPrevious;
                        deltaTimeCurrent = tempDelta * 1u / 3u;
                    }

                    // Make sure timestamps don't match
                    if (deltaTimeCurrent != 0)
                    {
                        elapsedDeltaTime = 0.000001 * deltaTimeCurrent; // Convert from microseconds to seconds
                        current.TotalMicroSec = previous.TotalMicroSec + deltaTimeCurrent;
                    }
                    else
                    {
                        // Duplicate timestamp. Use system clock for elapsed time instead
                        elapsedDeltaTime = lastTimeElapsedDouble * .001;
                        current.TotalMicroSec = previous.TotalMicroSec + (uint)(elapsedDeltaTime * 1000000);
                    }

                    current.timeElapsed = elapsedDeltaTime;
                    current.DS4Timestamp = (ushort)((tempStamp / 16) % ushort.MaxValue);
                    timeStampPrevious = tempStamp;

                    //Trace.WriteLine($"TIME {current.timeElapsed}");

                    int currentPitch = (short)((ushort)(inputReportBuffer[17 + reportOffset] << 8) | inputReportBuffer[16 + reportOffset]);
                    int currentYaw = (short)((ushort)(inputReportBuffer[19 + reportOffset] << 8) | inputReportBuffer[18 + reportOffset]);
                    int currentRoll = (short)((ushort)(inputReportBuffer[21 + reportOffset] << 8) | inputReportBuffer[20 + reportOffset]);
                    int AccelX = (short)((ushort)(inputReportBuffer[23 + reportOffset] << 8) | inputReportBuffer[22 + reportOffset]);
                    int AccelY = (short)((ushort)(inputReportBuffer[25 + reportOffset] << 8) | inputReportBuffer[24 + reportOffset]);
                    int AccelZ = (short)((ushort)(inputReportBuffer[27 + reportOffset] << 8) | inputReportBuffer[26 + reportOffset]);

                    if (device.CalibrationDone)
                    {
                        device.ApplyCalibs(ref currentYaw, ref currentPitch, ref currentRoll,
                            ref AccelX, ref AccelY, ref AccelZ);
                    }

                    if (gyroCalibrationUtil.gyroAverageTimer.IsRunning)
                    {
                        gyroCalibrationUtil.CalcSensorCamples(ref currentYaw, ref currentPitch, ref currentRoll,
                            ref AccelX, ref AccelY, ref AccelZ);
                    }

                    currentYaw -= gyroCalibrationUtil.gyro_offset_x;
                    currentPitch -= gyroCalibrationUtil.gyro_offset_y;
                    currentRoll -= gyroCalibrationUtil.gyro_offset_z;

                    current.Motion.GyroYaw = (short)-currentYaw;
                    current.Motion.GyroPitch = (short)currentPitch;
                    current.Motion.GyroRoll = (short)-currentRoll;
                    current.Motion.AngGyroYaw = current.Motion.GyroYaw / DS4State.DS4Motion.F_GYRO_RES_IN_DEG_SEC;
                    current.Motion.AngGyroPitch = current.Motion.GyroPitch / DS4State.DS4Motion.F_GYRO_RES_IN_DEG_SEC;
                    current.Motion.AngGyroRoll = current.Motion.GyroRoll / DS4State.DS4Motion.F_GYRO_RES_IN_DEG_SEC;

                    current.Motion.AccelX = (short)-AccelX;
                    current.Motion.AccelY = (short)-AccelY;
                    current.Motion.AccelZ = (short)AccelZ;
                    current.Motion.AccelXG = current.Motion.AccelX / DS4State.DS4Motion.F_ACC_RES_PER_G;
                    current.Motion.AccelYG = current.Motion.AccelY / DS4State.DS4Motion.F_ACC_RES_PER_G;
                    current.Motion.AccelZG = current.Motion.AccelZ / DS4State.DS4Motion.F_ACC_RES_PER_G;

                    current.Touch1.RawTrackingNum = (byte)(inputReportBuffer[33 + reportOffset]);
                    current.Touch1.Id = (byte)(inputReportBuffer[33 + reportOffset] & 0x7f);
                    current.Touch1.IsActive = (inputReportBuffer[33 + reportOffset] & 0x80) == 0;
                    current.Touch1.Touch = current.Touch1.IsActive;
                    current.Touch1.X = (short)(((ushort)(inputReportBuffer[35 + reportOffset] & 0x0f) << 8) | (ushort)(inputReportBuffer[34 + reportOffset]));
                    current.Touch1.Y = (short)(((ushort)(inputReportBuffer[36 + reportOffset]) << 4) | ((ushort)(inputReportBuffer[35 + reportOffset] & 0xf0) >> 4));

                    current.Touch2.RawTrackingNum = (byte)(inputReportBuffer[37 + reportOffset]);
                    current.Touch2.Id = (byte)(inputReportBuffer[37 + reportOffset] & 0x7f);
                    current.Touch2.IsActive = (inputReportBuffer[37 + reportOffset] & 0x80) == 0;
                    current.Touch2.Touch = current.Touch2.IsActive;
                    current.Touch2.X = (short)(((ushort)(inputReportBuffer[39 + reportOffset] & 0x0f) << 8) | (ushort)(inputReportBuffer[38 + reportOffset]));
                    current.Touch2.Y = (short)(((ushort)(inputReportBuffer[40 + reportOffset]) << 4) | ((ushort)(inputReportBuffer[39 + reportOffset] & 0xf0) >> 4));
                    current.TouchPacketNum = (byte)(inputReportBuffer[41 + reportOffset]);
                    //Trace.WriteLine($"{current.TouchPacketNum}");

                    uint numTouches = 0;
                    if (current.Touch1.IsActive)
                    {
                        numTouches++;
                    }

                    if (current.Touch2.IsActive)
                    {
                        numTouches++;
                    }

                    current.NumTouches = numTouches;

                    tempByte = inputReportBuffer[54 + reportOffset];
                    tempCharging = (tempByte & 0x08) != 0;
                    if (tempCharging != charging)
                    {
                        charging = tempCharging;
                    }

                    tempByte = inputReportBuffer[53 + reportOffset];
                    tempFull = (tempByte & 0x20) != 0; // Check for Full status
                    maxBatteryValue = DualSenseDevice.BATTERY_MAX;
                    if (tempFull)
                    {
                        // Full Charge flag found
                        tempBattery = 100;
                    }
                    else
                    {
                        // Partial charge
                        tempBattery = (tempByte & 0x0F) * 100 / maxBatteryValue;
                        tempBattery = Math.Min(tempBattery, 100);
                    }

                    current.Battery = (byte)tempBattery;
                    if (previous.Battery != tempBattery)
                    {
                        device.Battery = (uint)tempBattery;
                    }

                    if (fireReport)
                    {
                        Report?.Invoke(this, device);
                    }

                    if (device.HapticsDirty)
                    {
                        WriteRumbleReport();
                        device.HapticsDirty = false;
                    }

                    device.SyncStates();

                    firstReport = false;
                }
            }

            activeInputLoop = false;
            readWaitEv.Reset();
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

        public void WriteHapticsReport()
        {
            device.PrepareOutputReport(outputReportBuffer, rumble: false);
            device.WriteReport(outputReportBuffer);
        }
    }
}

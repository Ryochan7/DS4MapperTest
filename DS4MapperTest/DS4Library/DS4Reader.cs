using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;

namespace DS4MapperTest.DS4Library
{
    public class DS4Reader : DeviceReaderBase
    {
        private DS4Device device;
        public DS4Device Device => device;

        private Thread inputThread;
        private bool activeInputLoop = false;
        private byte[] inputReportBuffer;
        private byte[] outputReportBuffer;
        //private byte[] rumbleReportBuffer;
        private bool started;

        public delegate void DS4DeviceReportDelegate(DS4Reader sender,
            DS4Device device);
        public event DS4DeviceReportDelegate Report;

        public DS4Reader(DS4Device device)
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
            inputThread.Name = "DS4Device Reader Thread";
            inputThread.Start();

            started = true;
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
            if (inputThread != null && inputThread.IsAlive)
            {
                inputThread.Join();
            }

            started = false;
        }

        private void ReadInput()
        {
            activeInputLoop = true;

            //short tempAxisX;
            //short tempAxisY;
            byte tempByte;
            int tempBattery = 0;
            bool tempCharging = false, charging = false;
            short battery;
            int maxBatteryValue = 0;

            uint tempStamp = 0;
            double elapsedDeltaTime = 0.0;
            uint tempDelta = 0;
            int CRC32_POS_1 = DS4Device.BT_INPUT_REPORT_CRC32_POS + 1,
                CRC32_POS_2 = DS4Device.BT_INPUT_REPORT_CRC32_POS + 2,
                CRC32_POS_3 = DS4Device.BT_INPUT_REPORT_CRC32_POS + 3;
            int crcpos = DS4Device.BT_INPUT_REPORT_CRC32_POS;
            int crcoffset = 0;
            long latencySum = 0;
            bool timeStampInit = false;
            uint timeStampPrevious = 0;
            uint deltaTimeCurrent = 0;

            long currentTime = 0;
            long previousTime = 0;
            long deltaElapsed = 0;
            double lastTimeElapsedDouble = 0.0;
            double lastElapsed;
            double tempTimeElapsed;
            DateTime utcNow = DateTime.UtcNow;
            bool firstReport = true;

            int reportOffset =
                device.DevConnectionType == DS4Device.ConnectionType.Bluetooth ? 2 : 0;

            unchecked
            {
                while(activeInputLoop)
                {
                    if (device.DevConnectionType == DS4Device.ConnectionType.Bluetooth)
                    {
                        HidDevice.ReadStatus res = device.HidDevice.ReadWithFileStream(inputReportBuffer);
                        if (res != HidDevice.ReadStatus.Success)
                        {
                            activeInputLoop = false;
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
                            device.RaiseRemoval();
                            continue;
                        }
                    }

                    //Trace.WriteLine("BRING BACK THE PLAGUE");

                    ref DS4State current = ref device.CurrentStateRef;
                    ref DS4State previous = ref device.PreviousStateRef;

                    currentTime = Stopwatch.GetTimestamp();
                    deltaElapsed = currentTime - previousTime;
                    lastElapsed = deltaElapsed * (1.0 / Stopwatch.Frequency) * 1000.0;
                    tempTimeElapsed = lastElapsed * .001;

                    current.timeElapsed = tempTimeElapsed;
                    previousTime = currentTime;

                    utcNow = DateTime.UtcNow; // timestamp with UTC in case system time zone changes
                    current.PacketCounter = previous.PacketCounter + 1;
                    current.ReportTimeStamp = utcNow;

                    current.LX = inputReportBuffer[1+reportOffset];
                    current.LY = inputReportBuffer[2+reportOffset];
                    current.RX = inputReportBuffer[3+reportOffset];
                    current.RY = inputReportBuffer[4+reportOffset];
                    current.L2 = inputReportBuffer[8+reportOffset];
                    current.R2 = inputReportBuffer[9+reportOffset];

                    tempByte = inputReportBuffer[5+reportOffset];
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

                    tempByte = inputReportBuffer[6+reportOffset];
                    current.R3 = (tempByte & (1 << 7)) != 0;
                    current.L3 = (tempByte & (1 << 6)) != 0;
                    current.Options = (tempByte & (1 << 5)) != 0;
                    current.Share = (tempByte & (1 << 4)) != 0;
                    current.R2Btn = (tempByte & (1 << 3)) != 0;
                    current.L2Btn = (tempByte & (1 << 2)) != 0;
                    current.R1 = (tempByte & (1 << 1)) != 0;
                    current.L1 = (tempByte & (1 << 0)) != 0;

                    tempByte = inputReportBuffer[7 + reportOffset];
                    current.PS = (tempByte & (1 << 0)) != 0;
                    current.TouchClickButton = (tempByte & 0x02) != 0;

                    tempStamp = (uint)((ushort)(inputReportBuffer[11 + reportOffset] << 8) | inputReportBuffer[10 + reportOffset]);
                    if (timeStampInit == false)
                    {
                        timeStampInit = true;
                        deltaTimeCurrent = tempStamp * 16u / 3u;
                    }
                    else if (timeStampPrevious > tempStamp)
                    {
                        tempDelta = ushort.MaxValue - timeStampPrevious + tempStamp + 1u;
                        deltaTimeCurrent = tempDelta * 16u / 3u;
                    }
                    else
                    {
                        tempDelta = tempStamp - timeStampPrevious;
                        deltaTimeCurrent = tempDelta * 16u / 3u;
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
                    current.DS4Timestamp = (ushort)tempStamp;
                    timeStampPrevious = tempStamp;

                    //Trace.WriteLine($"TIME {current.timeElapsed}");

                    int currentPitch = (short)((ushort)(inputReportBuffer[14 + reportOffset] << 8) | inputReportBuffer[13 + reportOffset]);
                    int currentYaw = (short)((ushort)(inputReportBuffer[16 + reportOffset] << 8) | inputReportBuffer[15 + reportOffset]);
                    int currentRoll = (short)((ushort)(inputReportBuffer[18 + reportOffset] << 8) | inputReportBuffer[17 + reportOffset]);
                    int AccelX = (short)((ushort)(inputReportBuffer[20 + reportOffset] << 8) | inputReportBuffer[19 + reportOffset]);
                    int AccelY = (short)((ushort)(inputReportBuffer[22 + reportOffset] << 8) | inputReportBuffer[21 + reportOffset]);
                    int AccelZ = (short)((ushort)(inputReportBuffer[24 + reportOffset] << 8) | inputReportBuffer[23 + reportOffset]);

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

                    current.TouchPacketNum = (byte)(inputReportBuffer[34 + reportOffset]);
                    //Trace.WriteLine($"{current.TouchPacketNum}");
                    current.Touch1.RawTrackingNum = (byte)(inputReportBuffer[35 + reportOffset]);
                    current.Touch1.Id = (byte)(inputReportBuffer[35 + reportOffset] & 0x7f);
                    current.Touch1.IsActive = (inputReportBuffer[35 + reportOffset] & 0x80) == 0;
                    current.Touch1.Touch = current.Touch1.IsActive;
                    current.Touch1.X = (short)(((ushort)(inputReportBuffer[37 + reportOffset] & 0x0f) << 8) | (ushort)(inputReportBuffer[36 + reportOffset]));
                    current.Touch1.Y = (short)(((ushort)(inputReportBuffer[38 + reportOffset]) << 4) | ((ushort)(inputReportBuffer[37 + reportOffset] & 0xf0) >> 4));

                    current.Touch2.RawTrackingNum = (byte)(inputReportBuffer[39 + reportOffset]);
                    current.Touch2.Id = (byte)(inputReportBuffer[39 + reportOffset] & 0x7f);
                    current.Touch2.IsActive = (inputReportBuffer[39 + reportOffset] & 0x80) == 0;
                    current.Touch2.Touch = current.Touch2.IsActive;
                    current.Touch2.X = (short)(((ushort)(inputReportBuffer[41 + reportOffset] & 0x0f) << 8) | (ushort)(inputReportBuffer[40 + reportOffset]));
                    current.Touch2.Y = (short)(((ushort)(inputReportBuffer[42 + reportOffset]) << 4) | ((ushort)(inputReportBuffer[41 + reportOffset] & 0xf0) >> 4));

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

                    tempByte = inputReportBuffer[30 + reportOffset];
                    tempCharging = (tempByte & 0x10) != 0;

                    maxBatteryValue = charging ? DS4Device.BATTERY_MAX_USB : DS4Device.BATTERY_MAX;
                    tempBattery = (tempByte & 0x0f) * 100 / maxBatteryValue;
                    tempBattery = Math.Clamp(tempBattery, 0, 100);

                    current.Battery = (byte)tempBattery;
                    if (previous.Battery != tempBattery)
                    {
                        device.Battery = (uint)tempBattery;
                    }

                    Report?.Invoke(this, device);
                    device.SyncStates();

                    firstReport = false;
                }
            }
        }

        public override void WriteRumbleReport()
        {
            device.PrepareOutputReport(outputReportBuffer);
            device.WriteReport(outputReportBuffer);
        }
    }
}

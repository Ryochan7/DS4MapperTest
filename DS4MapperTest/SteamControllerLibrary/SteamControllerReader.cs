using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;
using DS4MapperTest.Common;

namespace DS4MapperTest.SteamControllerLibrary
{
    public class SteamControllerReader : DeviceReaderBase
    {
        protected SteamControllerDevice device;
        public SteamControllerDevice Device { get => device; }
        protected Thread inputThread;
        protected bool activeInputLoop = false;
        protected byte[] inputReportBuffer;
        protected byte[] outputReportBuffer;
        protected byte[] rumbleReportBuffer;
        protected byte[] hapticsReportBuffer;
        protected GyroCalibration gyroCalibrationUtil = new GyroCalibration();
        private bool started;

        public delegate void SteamControllerReportDelegate(SteamControllerReader sender,
            SteamControllerDevice device);
        public virtual event SteamControllerReportDelegate Report;

        public SteamControllerReader(SteamControllerDevice inputDevice)
        {
            this.device = inputDevice;

            inputReportBuffer = new byte[device.InputReportLen];
            outputReportBuffer = new byte[device.OutputReportLen];
            rumbleReportBuffer = new byte[SteamControllerDevice.FEATURE_REPORT_LEN];
            hapticsReportBuffer = new byte[SteamControllerDevice.FEATURE_REPORT_LEN];
        }

        public virtual void PrepareDevice()
        {
            NativeMethods.HidD_SetNumInputBuffers(device.HidDevice.safeReadHandle.DangerousGetHandle(),
                3);

            if (device.Synced)
            {
                device.SetOperational();
            }
        }

        private void PrepareSyncedDevice()
        {
            //NativeMethods.HidD_SetNumInputBuffers(device.HidDevice.safeReadHandle.DangerousGetHandle(),
            //    2);

            device.SetOperational();
        }

        public override void StartUpdate()
        {
            if (started)
            {
                return;
            }

            PrepareDevice();

            inputThread = new Thread(ReadInput);
            inputThread.IsBackground = true;
            inputThread.Priority = ThreadPriority.AboveNormal;
            inputThread.Name = "Steam Controller Reader Thread";
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
            Report = null;
            device.PurgeRemoval();
            device.HidDevice.CancelIO();
            //inputThread.Interrupt();
            if (inputThread != null && inputThread.IsAlive &&
                Thread.CurrentThread != inputThread)
            {
                inputThread.Join();
            }

            started = false;
        }

        protected virtual void ReadInput()
        {
            activeInputLoop = true;

            bool lsCoorConflict = false;
            short tempAxisX;
            short tempAxisY;
            byte tempByte;

            long currentTime = 0;
            long previousTime = 0;
            long deltaElapsed = 0;
            double lastElapsed;
            double tempTimeElapsed;
            bool firstReport = true;

            // Run continuous calibration on Gyro when starting input loop
            gyroCalibrationUtil.ResetContinuousCalibration();

            unchecked
            {
                while (activeInputLoop)
                {
                    HidDevice.ReadStatus res = device.HidDevice.ReadWithFileStream(inputReportBuffer);
                    if (res == HidDevice.ReadStatus.Success)
                    {
                        //Trace.WriteLine(string.Format("{0}", string.Join(" ", inputReportBuffer)));
                        tempByte = inputReportBuffer[3];
                        //Trace.WriteLine($"{inputReportBuffer[0]} {inputReportBuffer[2]} {inputReportBuffer[3]} {inputReportBuffer[4]}");

                        ref SteamControllerState current = ref device.CurrentStateRef;
                        ref SteamControllerState previous = ref device.PreviousStateRef;

                        if (tempByte == SteamControllerDevice.SCPacketType.PT_HOTPLUG)
                        {
                            byte statusByte = inputReportBuffer[5];
                            // 2 means a new device was connected. Looks like
                            // 1 means a device was disconnected
                            bool hasConnected = statusByte == 2;
                            if (!device.Synced && hasConnected)
                            {
                                // Disable lizard mode and activate components of newly
                                // connected Steam Controller
                                PrepareSyncedDevice();
                                device.Synced = true;
                            }
                            else if (device.Synced && !hasConnected)
                            {
                                device.Synced = false;
                            }

                            continue;
                        }
                        else if (tempByte != SteamControllerDevice.SCPacketType.PT_INPUT &&
                            tempByte != SteamControllerDevice.SCPacketType.PT_IDLE)
                        {
                            Trace.WriteLine(String.Format("Got unexpected input report id 0x{0:X2}. Try again",
                                inputReportBuffer[3]));

                            continue;
                        }
                        else if (tempByte == SteamControllerDevice.SCPacketType.PT_BATTERY)
                        {
                            //Trace.WriteLine($"IDLE {inputReportBuffer[4]}");
                            byte batt = inputReportBuffer[15];
                            device.Battery = batt;
                            continue;
                        }
                        /*
                        else if (tempByte == SteamControllerDevice.SCPacketType.PT_IDLE && !firstReport)
                        {
                            tempByte = 0;

                            // Repeat previously grabbed state with updated timestamp
                            currentTime = Stopwatch.GetTimestamp();
                            deltaElapsed = currentTime - previousTime;
                            lastElapsed = deltaElapsed * (1.0 / Stopwatch.Frequency) * 1000.0;
                            tempTimeElapsed = lastElapsed * .001;

                            current.timeElapsed = tempTimeElapsed;
                            previousTime = currentTime;

                            Report?.Invoke(this, device);
                            device.SyncStates();

                            continue;
                        }
                        */
                        else if (firstReport && tempByte == SteamControllerDevice.SCPacketType.PT_INPUT)
                        {
                            Console.WriteLine("CAN READ REPORTS. NICE");

                            // Run continuous calibration on Gyro when starting input loop
                            gyroCalibrationUtil.ResetContinuousCalibration();
                        }

                        //Console.WriteLine("Got unexpected input report id 0x{0:X2}. Try again",
                        //        inputReportBuffer[3]);

                        //ref SteamControllerState current = ref device.CurrentStateRef;
                        //ref SteamControllerState previous = ref device.PreviousStateRef;
                        tempByte = 0;

                        currentTime = Stopwatch.GetTimestamp();
                        deltaElapsed = currentTime - previousTime;
                        lastElapsed = deltaElapsed * (1.0 / Stopwatch.Frequency) * 1000.0;
                        tempTimeElapsed = lastElapsed * .001;

                        current.timeElapsed = tempTimeElapsed;
                        previousTime = currentTime;

                        /*if (inputReportBuffer[3] != SteamControllerDevice.SCPacketType.PT_INPUT)
                        {
                            Console.WriteLine("GOT INPUT REPORT {0} 0x{1:X2}", res, inputReportBuffer[3]);
                        }
                        */

                        /*
                        if (!firstReport)
                        {
                            Console.WriteLine("Poll Time: {0}", tempTimeElapsed);
                        }
                        //*/

                        //Console.WriteLine("BUTTONS?: {0} {1} {2} {3}", inputReportBuffer[8], inputReportBuffer[9], inputReportBuffer[10], inputReportBuffer[11]);

                        // ???
                        //tempByte = inputReportBuffer[8];

                        // Buttons
                        tempByte = inputReportBuffer[9];
                        current.RTClick = (tempByte & 0x01) != 0;
                        current.LTClick = (tempByte & 0x02) != 0;
                        current.RB = (tempByte & 0x04) != 0;
                        current.LB = (tempByte & 0x08) != 0;
                        current.Y = (tempByte & 0x10) != 0;
                        current.B = (tempByte & 0x20) != 0;
                        current.X = (tempByte & 0x40) != 0;
                        current.A = (tempByte & 0x80) != 0;

                        /*if (inputReportBuffer[3] != SteamControllerDevice.SCPacketType.PT_IDLE)
                        {
                            Console.WriteLine("LKJDLKDLK: {0}", current.A);
                        }
                        */

                        // Buttons
                        tempByte = inputReportBuffer[10];
                        current.DPadUp = (tempByte & 0x01) != 0;
                        current.DPadRight = (tempByte & 0x02) != 0;
                        current.DPadLeft = (tempByte & 0x04) != 0;
                        current.DPadDown = (tempByte & 0x08) != 0;
                        current.Back = (tempByte & 0x10) != 0;
                        current.Guide = (tempByte & 0x20) != 0;
                        current.Start = (tempByte & 0x40) != 0;
                        current.LGrip = (tempByte & 0x80) != 0;

                        // Buttons
                        tempByte = inputReportBuffer[11];
                        current.RGrip = (tempByte & 0x01) != 0;
                        current.LeftPad.Click = (tempByte & 0x02) != 0;
                        current.RightPad.Click = (tempByte & 0x04) != 0;
                        current.LeftPad.Touch = (tempByte & 0x08) != 0;
                        current.RightPad.Touch = (tempByte & 0x10) != 0;
                        current.LSClick = (tempByte & 0x40) != 0;
                        lsCoorConflict = (tempByte & 0x80) != 0;

                        current.LT = inputReportBuffer[12];
                        current.RT = inputReportBuffer[13];

                        //Console.WriteLine(current.LT);
                        //Console.WriteLine(current.RT);

                        tempAxisX = (short)((inputReportBuffer[18] << 8) | inputReportBuffer[17]);
                        tempAxisY = (short)((inputReportBuffer[20] << 8) | inputReportBuffer[19]);

                        if (!lsCoorConflict)
                        {
                            if (current.LeftPad.Touch)
                            {
                                // Only Touchpad is active
                                current.LeftPad.X = tempAxisX;
                                current.LeftPad.Y = tempAxisY;
                                current.LX = 0;
                                current.LY = 0;
                            }
                            else
                            {
                                // Only LS is active
                                current.LX = tempAxisX;
                                current.LY = tempAxisY;
                                current.LeftPad.X = 0;
                                current.LeftPad.Y = 0;
                            }
                        }
                        else
                        {
                            // Preserve previous coordinate of unused input
                            if (current.LeftPad.Touch)
                            {
                                // Preserve previous LS state
                                current.LeftPad.X = tempAxisX;
                                current.LeftPad.Y = tempAxisY;
                                current.LX = previous.LX;
                                current.LY = previous.LY;
                            }
                            else
                            {
                                // Preserve previous Left Touchpad state
                                current.LX = tempAxisX;
                                current.LY = tempAxisY;
                                current.LeftPad.X = previous.LeftPad.X;
                                current.LeftPad.Y = previous.LeftPad.Y;
                            }
                        }

                        current.RightPad.X = (short)((inputReportBuffer[22] << 8) | inputReportBuffer[21]);
                        current.RightPad.Y = (short)((inputReportBuffer[24] << 8) | inputReportBuffer[23]);

                        //Trace.WriteLine(string.Format("X: {0} Y: {1} {2}", current.RightPad.X, current.RightPad.Y, current.RightPad.Touch));

                        current.Motion.AccelX = (short)(-1 * ((inputReportBuffer[30] << 8) | inputReportBuffer[29]));
                        current.Motion.AccelY = (short)((inputReportBuffer[32] << 8) | inputReportBuffer[31]);
                        current.Motion.AccelZ = (short)((inputReportBuffer[34] << 8) | inputReportBuffer[33]);

                        current.Motion.GyroPitch = (short)(-1 * ((inputReportBuffer[36] << 8) | inputReportBuffer[35]));
                        current.Motion.GyroPitch = (short)(current.Motion.GyroPitch - device.gyroCalibOffsets[SteamControllerDevice.IMU_PITCH_IDX]);

                        current.Motion.GyroRoll = (short)(-1 * ((inputReportBuffer[38] << 8) | inputReportBuffer[37]));
                        current.Motion.GyroRoll = (short)(current.Motion.GyroRoll - device.gyroCalibOffsets[SteamControllerDevice.IMU_ROLL_IDX]);

                        current.Motion.GyroYaw = (short)(-1 * ((inputReportBuffer[40] << 8) | inputReportBuffer[39]));
                        current.Motion.GyroYaw = (short)(current.Motion.GyroYaw - device.gyroCalibOffsets[SteamControllerDevice.IMU_YAW_IDX]);

                        if (gyroCalibrationUtil.gyroAverageTimer.IsRunning)
                        {
                            int currentYaw = current.Motion.GyroPitch, currentPitch = current.Motion.GyroRoll, currentRoll = current.Motion.GyroYaw;
                            int AccelX = current.Motion.AccelX, AccelY = current.Motion.AccelY, AccelZ = current.Motion.AccelZ;
                            gyroCalibrationUtil.CalcSensorCamples(ref currentYaw, ref currentPitch, ref currentRoll,
                                ref AccelX, ref AccelY, ref AccelZ);
                        }

                        current.Motion.GyroYaw -= (short)gyroCalibrationUtil.gyro_offset_x;
                        current.Motion.GyroPitch -= (short)gyroCalibrationUtil.gyro_offset_y;
                        current.Motion.GyroRoll -= (short)gyroCalibrationUtil.gyro_offset_z;

                        current.Motion.AccelXG = current.Motion.AccelX / SteamControllerState.SteamControllerMotion.D_ACC_RES_PER_G;
                        current.Motion.AccelYG = current.Motion.AccelY / SteamControllerState.SteamControllerMotion.D_ACC_RES_PER_G;
                        current.Motion.AccelZG = current.Motion.AccelZ / SteamControllerState.SteamControllerMotion.D_ACC_RES_PER_G;

                        current.Motion.AngGyroPitch = -1 * current.Motion.GyroPitch * SteamControllerState.SteamControllerMotion.GYRO_RES_IN_DEG_SEC_RATIO;
                        current.Motion.AngGyroRoll = current.Motion.GyroRoll * SteamControllerState.SteamControllerMotion.GYRO_RES_IN_DEG_SEC_RATIO;
                        current.Motion.AngGyroYaw = current.Motion.GyroYaw * SteamControllerState.SteamControllerMotion.GYRO_RES_IN_DEG_SEC_RATIO;

                        //Trace.WriteLine($"TEST: {inputReportBuffer[41]}");
                        //Trace.WriteLine(string.Format("Yaw: {0}", current.Motion.GyroYaw));

                        current.Motion.QuaternionX = (short)(-1 * ((inputReportBuffer[42] << 8) | inputReportBuffer[41]));
                        current.Motion.QuaternionY = (short)(-1 * ((inputReportBuffer[44] << 8) | inputReportBuffer[43]));
                        current.Motion.QuaternionZ = (short)(-1 * ((inputReportBuffer[46] << 8) | inputReportBuffer[45]));
                        current.Motion.QuaternionW = (short)(-1 * ((inputReportBuffer[48] << 8) | inputReportBuffer[47]));

                        Report?.Invoke(this, device);
                        device.SyncStates();

                        firstReport = false;
                    }
                    else
                    {
                        activeInputLoop = false;
                        device.RaiseRemoval();
                    }
                }
            }
        }

        public override void WriteRumbleReport()
        {
            if (device.hapticInfo.dirty)
            {
                return;
            }

            // Send Left Haptic rumble
            device.PrepareRumbleData(rumbleReportBuffer, SteamControllerDevice.HAPTIC_POS_LEFT);
            device.SendRumbleReport(rumbleReportBuffer);

            // Send Right Haptic rumble
            device.PrepareRumbleData(rumbleReportBuffer, SteamControllerDevice.HAPTIC_POS_RIGHT);
            device.SendRumbleReport(rumbleReportBuffer);
        }

        public void WriteHapticsReport()
        {
            //if (device.hapticInfo != device.previousHapticInfo)
            //{

            //}

            // Send Left Haptic rumble
            device.PrepareHapticsData(hapticsReportBuffer, SteamControllerDevice.HAPTIC_POS_LEFT);
            device.SendRumbleReport(hapticsReportBuffer);

            // Send Right Haptic rumble
            device.PrepareHapticsData(hapticsReportBuffer, SteamControllerDevice.HAPTIC_POS_RIGHT);
            device.SendRumbleReport(hapticsReportBuffer);

            device.hapticInfo.dirty = false;
            device.previousHapticInfo = device.hapticInfo;

            //device.hapticInfo = new SteamControllerDevice.HapticFeedbackInfo();
        }
    }
}

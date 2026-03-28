using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;
using DS4MapperTest.Common;

namespace DS4MapperTest.InputDevices.SteamControllerTritonLibrary
{
    public class SteamControllerTritonReader : DeviceReaderBase
    {
        protected SteamControllerTritonDevice device;
        public SteamControllerTritonDevice Device { get => device; }
        protected Thread inputThread;
        protected bool activeInputLoop = false;
        protected byte[] inputReportBuffer;
        protected byte[] outputReportBuffer;
        protected byte[] rumbleReportBuffer;
        protected byte[] hapticsReportBuffer;
        protected GyroCalibration gyroCalibrationUtil = new GyroCalibration();
        private bool started;

        public delegate void SteamControllerReportDelegate(SteamControllerTritonReader sender,
            SteamControllerTritonDevice device);
        public virtual event SteamControllerReportDelegate Report;

        public SteamControllerTritonReader(SteamControllerTritonDevice inputDevice)
        {
            this.device = inputDevice;

            inputReportBuffer = new byte[device.InputReportLen];
            outputReportBuffer = new byte[device.OutputReportLen];
            rumbleReportBuffer = new byte[SteamControllerTritonDevice.FEATURE_REPORT_LEN];
            hapticsReportBuffer = new byte[SteamControllerTritonDevice.FEATURE_REPORT_LEN];
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
            inputThread.Name = "Steam Controller Triton Reader Thread";
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
                    readWaitEv.Set();

                    HidDevice.ReadStatus res = device.HidDevice.ReadWithFileStream(inputReportBuffer);
                    if (res == HidDevice.ReadStatus.Success)
                    {
                        //Trace.WriteLine(string.Format("{0}", string.Join(" ", inputReportBuffer)));
                        tempByte = inputReportBuffer[1];
                        //Trace.WriteLine($"{inputReportBuffer[0]} {inputReportBuffer[1]} {inputReportBuffer[2]} {inputReportBuffer[3]} {inputReportBuffer[4]}");

                        readWaitEv.Wait();
                        readWaitEv.Reset();

                        ref SteamControllerState current = ref device.CurrentStateRef;
                        ref SteamControllerState previous = ref device.PreviousStateRef;

                        if (tempByte == SteamControllerTritonDevice.SCPacketType.PT_HOTPLUG)
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
                        else if (tempByte != SteamControllerTritonDevice.ID_TRITON_CONTROLLER_STATE)
                        {
                            Trace.WriteLine(String.Format("Got unexpected input report id 0x{0:X2}. Try again",
                                inputReportBuffer[1]));

                            continue;
                        }
                        else if (tempByte == SteamControllerTritonDevice.ID_TRITON_BATTERY_STATUS)
                        {
                            //Trace.WriteLine($"IDLE {inputReportBuffer[3]}");
                            byte batt = inputReportBuffer[3];
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
                        else if (firstReport && tempByte == SteamControllerTritonDevice.ID_TRITON_CONTROLLER_STATE)
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
                        //tempByte = inputReportBuffer[3];
                        uint buttons = inputReportBuffer[3] |
                                (uint)(inputReportBuffer[3 + 1] << 8) |
                                (uint)(inputReportBuffer[3 + 2] << 16) |
                                (uint)(inputReportBuffer[3 + 3] << 24);

                        current.R1 = (buttons & 0x200) != 0;
                        current.L1 = (buttons & 0x80000) != 0;
                        current.Y = (buttons & 0x08) != 0;
                        current.B = (buttons & 0x02) != 0;
                        current.X = (buttons & 0x04) != 0;
                        current.A = (buttons & 0x01) != 0;

                        /*if (inputReportBuffer[3] != SteamControllerDevice.SCPacketType.PT_IDLE)
                        {
                            Console.WriteLine("LKJDLKDLK: {0}", current.A);
                        }
                        */

                        // Buttons
                        current.DPadUp = (buttons & 0x2000) != 0;
                        current.DPadDown = (buttons & 0x400) != 0;
                        current.DPadLeft = (buttons & 0x1000) != 0;
                        current.DPadRight = (buttons & 0x800) != 0;
                        current.Select = (buttons & 0x4000) != 0;
                        current.Start = (buttons & 0x40) != 0;
                        current.Steam = (buttons & 0x10000) != 0;
                        current.QAM = (buttons & 0x10) != 0;
                        current.L3 = (buttons & 0x8000) != 0;
                        current.R3 = (buttons & 0x20) != 0;
                        current.L4 = (buttons & 0x20000) != 0;
                        current.R4 = (buttons & 0x80) != 0;
                        current.L5 = (buttons & 0x40000) != 0;
                        current.R5 = (buttons & 0x100) != 0;

                        current.LeftPad.Click = (buttons & 0x04000000) != 0;
                        current.RightPad.Click = (buttons & 0x00400000) != 0;
                        current.LeftPad.Touch = (buttons & 0x02000000) != 0;
                        current.RightPad.Touch = (buttons & 0x00200000) != 0;

                        current.LeftGripSenseTouch = (buttons & 0x20000000) != 0;
                        current.RightGripSenseTouch = (buttons & 0x20000000) != 0;
                        current.LSTouch = (buttons & 0x01000000) != 0;
                        current.RSTouch = (buttons & 0x00100000) != 0;

                        current.L2 = (short)((inputReportBuffer[8] << 8) | inputReportBuffer[7]);
                        current.R2 = (short)((inputReportBuffer[10] << 8) | inputReportBuffer[9]);

                        //Console.WriteLine(current.L2);
                        //Console.WriteLine(current.R2);

                        tempAxisX = (short)((inputReportBuffer[12] << 8) | inputReportBuffer[11]);
                        tempAxisY = (short)((inputReportBuffer[14] << 8) | inputReportBuffer[13]);

                        current.LX = tempAxisX;
                        current.LY = tempAxisY;

                        tempAxisX = (short)((inputReportBuffer[16] << 8) | inputReportBuffer[15]);
                        tempAxisY = (short)((inputReportBuffer[18] << 8) | inputReportBuffer[17]);

                        current.RX = tempAxisX;
                        current.RY = tempAxisY;

                        current.LeftPad.X = (short)((inputReportBuffer[20] << 8) | inputReportBuffer[19]);
                        current.LeftPad.Y = (short)((inputReportBuffer[22] << 8) | inputReportBuffer[21]);

                        // TODO: Use this somewhere.
                        short tempTouchpadPressure = (short)((inputReportBuffer[24] << 8) | inputReportBuffer[23]);

                        current.RightPad.X = (short)((inputReportBuffer[26] << 8) | inputReportBuffer[25]);
                        current.RightPad.Y = (short)((inputReportBuffer[28] << 8) | inputReportBuffer[27]);

                        // TODO: Use this somewhere.
                        tempTouchpadPressure = (short)((inputReportBuffer[30] << 8) | inputReportBuffer[29]);

                        //Trace.WriteLine(string.Format("X: {0} Y: {1} {2}", current.RightPad.X, current.RightPad.Y, current.RightPad.Touch));

                        // TODO: Use sometime
                        uint devTimestamp = inputReportBuffer[31] |
                                (uint)(inputReportBuffer[31 + 1] << 8) |
                                (uint)(inputReportBuffer[31 + 2] << 16) |
                                (uint)(inputReportBuffer[31 + 3] << 24);

                        current.Motion.AccelX = (short)(-1 * ((inputReportBuffer[36] << 8) | inputReportBuffer[35]));
                        current.Motion.AccelY = (short)((inputReportBuffer[38] << 8) | inputReportBuffer[37]);
                        current.Motion.AccelZ = (short)((inputReportBuffer[40] << 8) | inputReportBuffer[39]);

                        current.Motion.GyroPitch = (short)(-1 * ((inputReportBuffer[42] << 8) | inputReportBuffer[41]));
                        current.Motion.GyroPitch = (short)(current.Motion.GyroPitch - device.gyroCalibOffsets[SteamControllerTritonDevice.IMU_PITCH_IDX]);

                        current.Motion.GyroRoll = (short)(-1 * ((inputReportBuffer[44] << 8) | inputReportBuffer[41]));
                        current.Motion.GyroRoll = (short)(current.Motion.GyroRoll - device.gyroCalibOffsets[SteamControllerTritonDevice.IMU_ROLL_IDX]);

                        current.Motion.GyroYaw = (short)(-1 * ((inputReportBuffer[46] << 8) | inputReportBuffer[45]));
                        current.Motion.GyroYaw = (short)(current.Motion.GyroYaw - device.gyroCalibOffsets[SteamControllerTritonDevice.IMU_YAW_IDX]);

                        if (gyroCalibrationUtil.gyroAverageTimer.IsRunning)
                        {
                            int currentYaw = current.Motion.GyroYaw, currentPitch = current.Motion.GyroPitch, currentRoll = current.Motion.GyroRoll;
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

                        current.Motion.AngGyroPitch = current.Motion.GyroPitch * SteamControllerState.SteamControllerMotion.GYRO_RES_IN_DEG_SEC_RATIO;
                        current.Motion.AngGyroRoll = current.Motion.GyroRoll * SteamControllerState.SteamControllerMotion.GYRO_RES_IN_DEG_SEC_RATIO;
                        current.Motion.AngGyroYaw = current.Motion.GyroYaw * SteamControllerState.SteamControllerMotion.GYRO_RES_IN_DEG_SEC_RATIO;

                        //Trace.WriteLine($"TEST: {inputReportBuffer[41]}");
                        //Trace.WriteLine(string.Format("Yaw: {0}", current.Motion.GyroYaw));

                        // TODO: Looks like current test SDL code does not use Quaternion data
                        //current.Motion.QuaternionW = (short)(-1 * ((inputReportBuffer[48] << 8) | inputReportBuffer[47]));
                        //current.Motion.QuaternionX = (short)(-1 * ((inputReportBuffer[50] << 8) | inputReportBuffer[49]));
                        //current.Motion.QuaternionY = (short)(-1 * ((inputReportBuffer[52] << 8) | inputReportBuffer[51]));
                        //current.Motion.QuaternionZ = (short)(-1 * ((inputReportBuffer[54] << 8) | inputReportBuffer[53]));
                        

                        if (fireReport)
                        {
                            Report?.Invoke(this, device);
                        }

                        device.SyncStates();

                        firstReport = false;
                    }
                    else
                    {
                        activeInputLoop = false;
                        readWaitEv.Reset();
                        device.RaiseRemoval();
                    }
                }
            }
        }

        public override void WriteRumbleReport()
        {
            // Ignore rumble request if haptics are active
            if (device.hapticInfo.dirty)
            {
                return;
            }

            // Send Left Haptic rumble
            device.PrepareRumbleData(rumbleReportBuffer, SteamControllerTritonDevice.HAPTIC_POS_LEFT);
            device.SendRumbleReport(rumbleReportBuffer);

            // Send Right Haptic rumble
            device.PrepareRumbleData(rumbleReportBuffer, SteamControllerTritonDevice.HAPTIC_POS_RIGHT);
            device.SendRumbleReport(rumbleReportBuffer);

            device.activeLeftAmpRatio = device.currentLeftAmpRatio;
            device.activeRightAmpRatio = device.currentRightAmpRatio;
        }

        public void WriteHapticsReport()
        {
            //if (device.hapticInfo != device.previousHapticInfo)
            //{

            //}

            // Send Left Haptic rumble
            device.PrepareHapticsData(hapticsReportBuffer, SteamControllerTritonDevice.HAPTIC_POS_LEFT);
            device.SendRumbleReport(hapticsReportBuffer);

            // Send Right Haptic rumble
            device.PrepareHapticsData(hapticsReportBuffer, SteamControllerTritonDevice.HAPTIC_POS_RIGHT);
            device.SendRumbleReport(hapticsReportBuffer);

            device.hapticInfo.dirty = false;
            device.previousHapticInfo = device.hapticInfo;

            //device.hapticInfo = new SteamControllerDevice.HapticFeedbackInfo();
        }
    }
}

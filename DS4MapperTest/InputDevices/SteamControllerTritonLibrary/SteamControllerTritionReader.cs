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
        private const int WIRELESS_STATE_DISCONNECT = 1;
        private const int WIRELESS_STATE_CONNECT = 2;

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
        private Stopwatch lizardCheckSW = new Stopwatch();

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

            lizardCheckSW.Start();

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
                        tempByte = inputReportBuffer[0];
                        //Trace.WriteLine($"{inputReportBuffer[0]} {inputReportBuffer[1]} {inputReportBuffer[2]} {inputReportBuffer[3]} {inputReportBuffer[4]}");

                        readWaitEv.Wait();
                        readWaitEv.Reset();

                        ref SteamControllerState current = ref device.CurrentStateRef;
                        ref SteamControllerState previous = ref device.PreviousStateRef;

                        if (tempByte == SteamControllerTritonDevice.ID_TRITON_WIRELESS_STATUS ||
                            tempByte == SteamControllerTritonDevice.ID_TRITON_WIRELESS_STATUS_X)
                        {
                            byte statusByte = inputReportBuffer[1];
                            // 2 means a new device was connected. Looks like
                            // 1 means a device was disconnected
                            bool hasConnected = statusByte == WIRELESS_STATE_CONNECT;
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
                        else if (tempByte == SteamControllerTritonDevice.ID_TRITON_BATTERY_STATUS)
                        {
                            //Trace.WriteLine($"IDLE {inputReportBuffer[1]}");
                            byte chargeState = inputReportBuffer[1];
                            byte batt = inputReportBuffer[2];
                            device.Battery = Math.Clamp(batt, 0u, 100u);
                            continue;
                        }
                        else if (tempByte != SteamControllerTritonDevice.ID_TRITON_CONTROLLER_STATE &&
                            tempByte != SteamControllerTritonDevice.ID_TRITON_CONTROLLER_STATE_BLE)
                        {
                            Trace.WriteLine(String.Format("Got unexpected input report id 0x{0:X2}. Try again",
                                inputReportBuffer[0]));

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
                        else if (firstReport && (tempByte == SteamControllerTritonDevice.ID_TRITON_CONTROLLER_STATE ||
                            tempByte != SteamControllerTritonDevice.ID_TRITON_CONTROLLER_STATE_BLE))
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

                        current.PacketCounter = inputReportBuffer[1];
                        if (current.PacketCounter == previous.PacketCounter)
                        {
                            // According to docs, it looks like there is a small
                            // possibility that the controller can send duplicate input
                            // packets. Ignore current packet if counter is the same
                            // as the last processed input
                            continue;
                        }

                        // Keep controller from switching back to lizard mode.
                        // Hardware watchdog reverts back after 10 seconds
                        if (lizardCheckSW.IsRunning && lizardCheckSW.ElapsedMilliseconds >= 3000)
                        {
                            device.DisableLizardMode();
                            lizardCheckSW.Restart();
                        }

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
                        //tempByte = inputReportBuffer[2];
                        uint buttons = inputReportBuffer[2] |
                                (uint)(inputReportBuffer[2 + 1] << 8) |
                                (uint)(inputReportBuffer[2 + 2] << 16) |
                                (uint)(inputReportBuffer[2 + 3] << 24);

                        current.R1 = (buttons & 0x200) != 0;
                        current.L1 = (buttons & 0x80000) != 0;
                        current.Y = (buttons & 0x08) != 0;
                        current.B = (buttons & 0x02) != 0;
                        current.X = (buttons & 0x04) != 0;
                        current.A = (buttons & 0x01) != 0;

                        //Trace.WriteLine($"Buttons {buttons}");

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
                        current.RightGripSenseTouch = (buttons & 0x10000000) != 0;
                        current.LSTouch = (buttons & 0x01000000) != 0;
                        current.RSTouch = (buttons & 0x00100000) != 0;

                        current.L2 = (short)((inputReportBuffer[7] << 8) | inputReportBuffer[6]);
                        current.R2 = (short)((inputReportBuffer[9] << 8) | inputReportBuffer[8]);

                        //Console.WriteLine(current.L2);
                        //Console.WriteLine(current.R2);

                        tempAxisX = (short)((inputReportBuffer[11] << 8) | inputReportBuffer[10]);
                        tempAxisY = (short)((inputReportBuffer[13] << 8) | inputReportBuffer[12]);

                        current.LX = tempAxisX;
                        current.LY = tempAxisY;

                        tempAxisX = (short)((inputReportBuffer[15] << 8) | inputReportBuffer[14]);
                        tempAxisY = (short)((inputReportBuffer[17] << 8) | inputReportBuffer[16]);

                        current.RX = tempAxisX;
                        current.RY = tempAxisY;

                        current.LeftPad.X = (short)((inputReportBuffer[19] << 8) | inputReportBuffer[18]);
                        current.LeftPad.Y = (short)((inputReportBuffer[21] << 8) | inputReportBuffer[20]);

                        // TODO: Use this somewhere.
                        short tempTouchpadPressure = (short)((inputReportBuffer[23] << 8) | inputReportBuffer[22]);

                        current.RightPad.X = (short)((inputReportBuffer[25] << 8) | inputReportBuffer[24]);
                        current.RightPad.Y = (short)((inputReportBuffer[27] << 8) | inputReportBuffer[26]);

                        // TODO: Use this somewhere.
                        tempTouchpadPressure = (short)((inputReportBuffer[29] << 8) | inputReportBuffer[28]);

                        //Trace.WriteLine(string.Format("X: {0} Y: {1} {2}", current.RightPad.X, current.RightPad.Y, current.RightPad.Touch));

                        // TODO: Use sometime
                        uint devTimestamp = inputReportBuffer[30] |
                                (uint)(inputReportBuffer[30 + 1] << 8) |
                                (uint)(inputReportBuffer[30 + 2] << 16) |
                                (uint)(inputReportBuffer[30 + 3] << 24);

                        current.Motion.AccelX = (short)(-1 * ((inputReportBuffer[35] << 8) | inputReportBuffer[34]));
                        current.Motion.AccelY = (short)((inputReportBuffer[37] << 8) | inputReportBuffer[36]);
                        current.Motion.AccelZ = (short)((inputReportBuffer[39] << 8) | inputReportBuffer[38]);

                        current.Motion.GyroPitch = (short)(-1 * ((inputReportBuffer[41] << 8) | inputReportBuffer[40]));
                        current.Motion.GyroPitch = (short)(current.Motion.GyroPitch - device.gyroCalibOffsets[SteamControllerTritonDevice.IMU_PITCH_IDX]);

                        current.Motion.GyroRoll = (short)(-1 * ((inputReportBuffer[43] << 8) | inputReportBuffer[42]));
                        current.Motion.GyroRoll = (short)(current.Motion.GyroRoll - device.gyroCalibOffsets[SteamControllerTritonDevice.IMU_ROLL_IDX]);

                        current.Motion.GyroYaw = (short)(-1 * ((inputReportBuffer[45] << 8) | inputReportBuffer[44]));
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
                        //current.Motion.QuaternionW = (short)(-1 * ((inputReportBuffer[47] << 8) | inputReportBuffer[46]));
                        //current.Motion.QuaternionX = (short)(-1 * ((inputReportBuffer[49] << 8) | inputReportBuffer[48]));
                        //current.Motion.QuaternionY = (short)(-1 * ((inputReportBuffer[51] << 8) | inputReportBuffer[50]));
                        //current.Motion.QuaternionZ = (short)(-1 * ((inputReportBuffer[53] << 8) | inputReportBuffer[52]));
                        

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

            lizardCheckSW.Stop();
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

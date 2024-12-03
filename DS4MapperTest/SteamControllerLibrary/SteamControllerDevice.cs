using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS4MapperTest.SteamControllerLibrary
{
    public class SteamControllerDevice : InputDeviceBase
    {
        public static class SCPacketType
        {
            public const byte PT_INPUT = 0x01;
            public const byte PT_HOTPLUG = 0x03;
            public const byte PT_IDLE = 0x04;
            public const byte PT_BATTERY = 0x04;
            public const byte PT_OFF = 0x9F;
            public const byte PT_AUDIO = 0xB6;
            public const byte PT_CLEAR_MAPPINGS = 0x81;
            public const byte PT_CONFIGURE = 0x87;
            public const byte PT_LED = 0x87;
            public const byte PT_CALIBRATE_JOYSTICK = 0xBF;
            public const byte PT_CALIBRATE_TRACKPAD = 0xA7;
            public const byte PT_SET_AUDIO_INDICES = 0xC1;
            public const byte PT_LIZARD_BUTTONS = 0x85;
            public const byte PT_LIZARD_MOUSE = 0x8E;
            public const byte PT_FEEDBACK = 0x8F;
            public const byte PT_RESET = 0x95;
            public const byte PT_GET_SERIAL = 0xAE;
        }

        public static class SCPacketLength
        {
            public const byte PL_LED = 0x03;
            public const byte PL_OFF = 0x04;
            public const byte PL_FEEDBACK = 0x07;
            public const byte PL_CONFIGURE = 0x15;
            public const byte PL_CONFIGURE_BT = 0x0F;
            public const byte PL_GET_SERIAL = 0x15;
        }

        public static class SCConfigType
        {
            public const byte CT_LED = 0x2D;
            public const byte CT_CONFIGURE = 0x32;
            public const byte CT_CONFIGURE_BT = 0x18;
        }

        public enum SCControllerState : uint
        {
            SS_NOT_CONFIGURED,
            SS_READY,
            SS_FAILED,
        }

        public struct StickAxisData
        {
            public short max;
            public short mid;
            public short min;
        };

        public struct RumbleInfo
        {
            public double leftRumbleRatio;
            public double rightRumbleRatio;
        }

        public struct HapticFeedbackInfo
        {
            public double leftActuatorAmpRatio;
            public double rightActuatorAmpRatio;
            public double durationLeft;
            public double durationRight;
            public uint countLeft;
            public uint countRight;
            public double leftPeriodRatio;
            public double rightPeriodRatio;
            public bool dirty;
        };

        public struct LightLEDInfo
        {
            public ushort brightness;
        }

        public HapticFeedbackInfo hapticInfo = new HapticFeedbackInfo();
        public HapticFeedbackInfo previousHapticInfo = new HapticFeedbackInfo();
        public bool rumbleDirty;


        public enum ConnectionType : uint
        {
            USB,
            SCDongle,
            Bluetooth,
        }

        public const byte HAPTIC_POS_LEFT = 0x01;
        public const byte HAPTIC_POS_RIGHT = 0x00;

        public const int IMU_XAXIS_IDX = 0, IMU_YAW_IDX = 0;
        public const int IMU_YAXIS_IDX = 1, IMU_PITCH_IDX = 1;
        public const int IMU_ZAXIS_IDX = 2, IMU_ROLL_IDX = 2;

        protected ConnectionType conType;
        public ConnectionType ConType => conType;

        protected HidDevice hidDevice;
        public HidDevice HidDevice { get => hidDevice; }

        private bool modeChangeDone;
        public bool ModeChangeDone { get => modeChangeDone; }

        protected SteamControllerState currentState;
        public SteamControllerState CurrentState { get => currentState; }
        public ref SteamControllerState CurrentStateRef { get => ref currentState; }

        protected SteamControllerState previousState;
        public SteamControllerState PreviousState { get => previousState; }
        public ref SteamControllerState PreviousStateRef { get => ref previousState; }

        public const int INPUT_REPORT_LEN = 65;
        public const int OUTPUT_REPORT_LEN = 65;
        public const int RUMBLE_REPORT_LEN = 65;
        public const int FEATURE_REPORT_LEN = 65;

        public virtual int InputReportLen { get => INPUT_REPORT_LEN; }
        public virtual int OutputReportLen { get => OUTPUT_REPORT_LEN; }
        public virtual int RumbleReportLen { get => RUMBLE_REPORT_LEN; }

        protected SCControllerState controllerModeState;
        public SCControllerState ControllerModeState { get => controllerModeState; }

        public short[] gyroCalibOffsets = new short[3];

        public double currentLeftAmpRatio = 0.0;
        public double currentRightAmpRatio = 0.0;

        //private SteamControllerControllerOptions deviceOptions;
        //public SteamControllerControllerOptions DeviceOptions
        //{
        //    get => deviceOptions;
        //}

        private bool checkForSyncChange;
        public bool CheckForSyncChange
        {
            get => checkForSyncChange;
        }

        private SteamControllerControllerOptions nativeDeviceOptions;
        public SteamControllerControllerOptions NativeDeviceOptions
        {
            get => nativeDeviceOptions;
        }

        public override event EventHandler Removal;

        public SteamControllerDevice(HidDevice device, string displayName)
        {
            hidDevice = device;
            conType = DetermineConnectionType(hidDevice);
            baseElapsedReference = 125.0;
            deviceType = InputDeviceType.SteamController;
            devTypeStr = displayName;

            deviceOptions = nativeDeviceOptions =
                new SteamControllerControllerOptions(InputDeviceType.SteamController);
            if (conType != ConnectionType.SCDongle)
            {
                synced = true;
            }
            else
            {
                // Temporary check to attempt to see if controller is
                // connected to Dongle. Might have to rely on polls in the
                // reader class instead
                synced = TestDongleSCConnected(hidDevice);
                checkForSyncChange = true;
            }

            if (!hidDevice.IsFileStreamOpen())
            {
                hidDevice.OpenFileStream(OutputReportLen);
            }

            if (synced)
            {
                ReadSerial();
            }

            SyncedChanged += SteamControllerDevice_SyncedChanged;
        }

        public static ConnectionType DetermineConnectionType(HidDevice device)
        {
            // Initially assume a USB connection
            ConnectionType result = ConnectionType.USB;
            if (device.Attributes.ProductId ==
                SteamControllerEnumerator.STEAM_DONGLE_CONTROLLER_PRODUCT_ID)
            //if (device.Attributes.ProductId == 0x1142)
            {
                result = ConnectionType.SCDongle;
            }
            else if (device.Attributes.ProductId ==
                SteamControllerEnumerator.STEAM_BT_CONTROLLER_PRODUCT_ID)
            {
                result = ConnectionType.Bluetooth;
            }

            return result;
        }

        public override void SetOperational()
        {
            if (modeChangeDone)
            {
                return;
            }

            ClearMappings();
            if (string.IsNullOrEmpty(serial))
            {
                ReadSerial();
            }

            Configure();

            Thread.Sleep(100);

            controllerModeState = SCControllerState.SS_READY;
            modeChangeDone = true;
        }

        private void SteamControllerDevice_SyncedChanged(object sender, EventArgs e)
        {
            if (!synced)
            {
                modeChangeDone = false;
                controllerModeState = SCControllerState.SS_NOT_CONFIGURED;
            }
        }

        public override void Detach()
        {
            if (controllerModeState == SCControllerState.SS_READY)
            {
                ChangeToLizardMode();
            }

            controllerModeState = SCControllerState.SS_NOT_CONFIGURED;
        }

        protected virtual void ReadSerial()
        {
            byte[] featureData = new byte[FEATURE_REPORT_LEN];
            featureData[1] = SCPacketType.PT_GET_SERIAL;
            featureData[2] = 0x15;
            featureData[3] = 0x01;
            hidDevice.WriteFeatureReport(featureData);
            hidDevice.fileStream.Flush();

            // Sleep seems to be needed when probing from a Dongle connection
            if (conType == ConnectionType.SCDongle)
            {
                Thread.Sleep(100);
            }

            byte[] retReportData = new byte[FEATURE_REPORT_LEN];
            hidDevice.readFeatureData(retReportData);
            //Console.WriteLine("LKJDKJLLD: {0}", retReportData[1]);

            string baseS = System.Text.Encoding.Default.GetString(retReportData, 4, 12);
            string MACAddr = baseS.Replace("\0", string.Empty).ToUpper();
            //List<string> tempStrList = new List<string>();
            //for (int i=0; i < MACAddr.Length; i += 2)
            //{
            //    tempStrList.Add($"{MACAddr[i]}{MACAddr[i + 1]}");
            //}

            //serial = string.Join(":", tempStrList);
            serial = MACAddr;
        }

        public static bool TestDongleSCConnected(HidDevice device)
        {
            bool result = false;

            byte[] featureData = new byte[FEATURE_REPORT_LEN];
            featureData[1] = SCPacketType.PT_GET_SERIAL;
            featureData[2] = 0x15;
            featureData[3] = 0x01;
            device.WriteFeatureReport(featureData);
            // Sleep seems to be needed when probing from a Dongle connection
            Thread.Sleep(100);

            byte[] retReportData = new byte[FEATURE_REPORT_LEN];
            device.readFeatureData(retReportData);

            if (retReportData[4] != 0)
            {
                result = true;
            }

            return result;
        }

        protected virtual void ClearMappings()
        {
            byte[] featureData = new byte[FEATURE_REPORT_LEN];
            featureData[1] = SCPacketType.PT_CLEAR_MAPPINGS;
            featureData[2] = 0x01;
            hidDevice.WriteFeatureReport(featureData);
        }

        protected virtual void Configure()
        {
            int timeout = 600;
            int ledLevel = 30;
            byte[] gyroAndTimeoutFeatureData = new byte[FEATURE_REPORT_LEN];
            gyroAndTimeoutFeatureData[1] = SCPacketType.PT_CONFIGURE;
            gyroAndTimeoutFeatureData[2] = SCPacketLength.PL_CONFIGURE;
            gyroAndTimeoutFeatureData[3] = SCConfigType.CT_CONFIGURE;
            gyroAndTimeoutFeatureData[4] = 0; // Idle Timeout
            gyroAndTimeoutFeatureData[5] = 0; // Idle Timeout
            // Unknown Header
            Array.Copy(new byte[] { 0x18, 0x00, 0x00, 0x31, 0x02, 0x00, 0x08, 0x07, 0x00, 0x07, 0x07, 0x00, 0x30 }, 0, gyroAndTimeoutFeatureData, 6, 13);
            gyroAndTimeoutFeatureData[19] = true ? 0x1C : 0x00; // Gyro Enable (0x1C = Enable. 0x00 = Disable)
            gyroAndTimeoutFeatureData[20] = 0x00; // Unknown
            gyroAndTimeoutFeatureData[21] = 0x2E; // Unknown
            hidDevice.WriteFeatureReport(gyroAndTimeoutFeatureData);

            byte[] ledsFeatureData = new byte[FEATURE_REPORT_LEN];
            ledsFeatureData[1] = SCPacketType.PT_CONFIGURE;
            ledsFeatureData[2] = SCPacketLength.PL_LED;
            ledsFeatureData[3] = SCConfigType.CT_LED;
            ledsFeatureData[4] = (byte)(Math.Min(Math.Max(ledLevel, 0), 100)); // LED Level (0-100?)
            hidDevice.WriteFeatureReport(ledsFeatureData);
        }

        protected virtual void ChangeToLizardMode()
        {
            byte[] featureData = new byte[FEATURE_REPORT_LEN];
            featureData[1] = SCPacketType.PT_LIZARD_BUTTONS;
            featureData[2] = 0x01;
            hidDevice.WriteFeatureReport(featureData);

            featureData[1] = SCPacketType.PT_LIZARD_MOUSE;
            hidDevice.WriteFeatureReport(featureData);
        }
        public void SyncStates()
        {
            previousState = currentState;
        }

        public virtual void PrepareRumbleData(byte[] buffer, byte position)
        {
            // Taken from SteamControllerSinger app
            // https://gitlab.com/Pilatomic/SteamControllerSinger
            const double STEAM_CONTROLLER_MAGIC_PERIOD_RATIO = 495483.0;

            double tempRatio = (position == HAPTIC_POS_RIGHT) ?
                currentRightAmpRatio : currentLeftAmpRatio;

            ushort amplitude = 0;
            if (tempRatio != 0.0)
            {
                amplitude = (ushort)((900 - 600) * tempRatio + 600);
                //amplitude = (ushort)((1400 - 1000) * tempRatio + 1000);
                //amplitude = 1000;
                //amplitude = (ushort)((1200 - 100) * tempRatio + 100);
                //amplitude = (ushort)((2000 - 1400) * tempRatio + 1400);
                //amplitude = (ushort)((1400 - 2800) * tempRatio + 2800);
            }

            /*if (tempRatio != 0.0)
            {
                amplitude = 500;
            }
            */

            ushort tmp_period_command = 15000;
            //ushort period_command = 15000;
            ushort period_command = 0;
            if (tempRatio != 0.0)
            {
                period_command = (ushort)((6000 - 25000) * tempRatio + 25000);
            }

            double raw_period = period_command / STEAM_CONTROLLER_MAGIC_PERIOD_RATIO;
            int duration_num_seconds = 5;
            ushort count = 0;
            if (raw_period != 0)
            {
                count = (ushort)(Math.Min((int)(duration_num_seconds * 1.5 / raw_period),
                    0x7FFF));
            }

            buffer[1] = SCPacketType.PT_FEEDBACK;
            buffer[2] = SCPacketLength.PL_FEEDBACK;
            buffer[3] = position; // Left or Right Haptic actuator

            // Amplitude
            buffer[4] = (byte)amplitude;
            buffer[5] = (byte)(amplitude >> 8);

            // Period
            buffer[6] = (byte)period_command;
            buffer[7] = (byte)(period_command >> 8);

            // Repeat count
            buffer[8] = (byte)count;
            buffer[9] = (byte)(count >> 8);
        }

        public virtual void PrepareHapticsData(byte[] buffer, byte position)
        {
            // Taken from SteamControllerSinger app
            // https://gitlab.com/Pilatomic/SteamControllerSinger
            const double STEAM_CONTROLLER_MAGIC_PERIOD_RATIO = 495483.0;

            double tempRatio = (position == HAPTIC_POS_RIGHT) ?
                hapticInfo.rightActuatorAmpRatio : hapticInfo.leftActuatorAmpRatio;

            ushort amplitude = 0;
            if (tempRatio != 0.0)
            {
                amplitude = (ushort)((1200 - 200) * tempRatio + 200);
                //amplitude = 800;
                //amplitude = (ushort)((1400 - 1000) * tempRatio + 1000);
                //amplitude = 1000;
                //amplitude = (ushort)((1200 - 100) * tempRatio + 100);
                //amplitude = (ushort)((2000 - 1400) * tempRatio + 1400);
                //amplitude = (ushort)((1400 - 2800) * tempRatio + 2800);
            }

            /*if (tempRatio != 0.0)
            {
                amplitude = 500;
            }
            */

            ushort tmp_period_command = 15000;
            //ushort period_command = 15000;
            ushort period_command = 0;
            if (tempRatio != 0.0)
            {
                //period_command = (ushort)((6000 - 25000) * tempRatio + 25000);
                //period_command = 600;
                period_command = 600;
            }

            ushort count = (position == HAPTIC_POS_RIGHT) ? (ushort)hapticInfo.countRight : (ushort)hapticInfo.countLeft;
            if (count == 0)
            {
                double raw_period = period_command / STEAM_CONTROLLER_MAGIC_PERIOD_RATIO;
                //raw_period = period_command;
                double duration = (position == HAPTIC_POS_RIGHT) ? hapticInfo.durationRight : hapticInfo.durationLeft;

                if (raw_period != 0)
                {
                    count = (ushort)(Math.Min((int)(duration * 1.5 / raw_period),
                        0x7FFF));
                }
            }

            //count = 1;

            buffer[1] = SCPacketType.PT_FEEDBACK;
            buffer[2] = SCPacketLength.PL_FEEDBACK;
            buffer[3] = position; // Left or Right Haptic actuator

            // Amplitude
            buffer[4] = (byte)amplitude;
            buffer[5] = (byte)(amplitude >> 8);

            // Period
            buffer[6] = (byte)period_command;
            buffer[7] = (byte)(period_command >> 8);

            // Repeat count
            buffer[8] = (byte)count;
            buffer[9] = (byte)(count >> 8);
        }

        public virtual void SendRumbleReport(byte[] buffer)
        {
            hidDevice.WriteFeatureReport(buffer);
        }

        public void RaiseRemoval()
        {
            Removal?.Invoke(this, EventArgs.Empty);
        }

        public void PurgeRemoval()
        {
            Removal = null;
        }
    }
}

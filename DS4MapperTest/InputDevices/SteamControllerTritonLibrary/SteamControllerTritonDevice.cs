using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS4MapperTest.InputDevices.SteamControllerTritonLibrary
{
    public class SteamControllerTritonDevice : InputDeviceBase
    {
        public const byte ID_TRITON_CONTROLLER_STATE = 0x42;
        public const byte ID_TRITON_BATTERY_STATUS = 0x43;
        public const byte ID_TRITON_CONTROLLER_STATE_BLE = 0x45;
        public const byte ID_TRITON_WIRELESS_STATUS_X = 0x46;
        public const byte ID_TRITON_WIRELESS_STATUS = 0x79;

        // TODO: Old from 2015 SC code. Remove in future
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

        // TODO: Old from 2015 SC code. Remove in future
        public static class SCPacketLength
        {
            public const byte PL_LED = 0x03;
            public const byte PL_OFF = 0x04;
            public const byte PL_FEEDBACK = 0x07;
            public const byte PL_CONFIGURE = 0x15;
            public const byte PL_CONFIGURE_BT = 0x0F;
            public const byte PL_GET_SERIAL = 0x15;
        }

        // TODO: Old from 2015 SC code. Remove in future
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

        public enum EChargeState : ushort
        {
            k_EChargeStateReset,
            k_EChargeStateDischarging,
            k_EChargeStateCharging,
            k_EChargeStateSrcValidate,
            k_EChargeStateChargingDone,
        };

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

        public const int INPUT_REPORT_LEN = 54;
        public const int OUTPUT_REPORT_LEN = 64;
        public const int RUMBLE_REPORT_LEN = 64;
        public const int FEATURE_REPORT_LEN = 64;

        public const int TRITON_WIRED_PID = 0x1302;
        public const int PROTEUS_DONGLE_PID = 0x1304;
        public const int NEREID_DONGLE_PID = 0x1305;
        public const int BLE_PID = 0x1303;

        public virtual int InputReportLen { get => INPUT_REPORT_LEN; }
        public virtual int OutputReportLen { get => OUTPUT_REPORT_LEN; }
        public virtual int RumbleReportLen { get => RUMBLE_REPORT_LEN; }

        protected SCControllerState controllerModeState;
        public SCControllerState ControllerModeState { get => controllerModeState; }

        public short[] gyroCalibOffsets = new short[3];

        public double currentLeftAmpRatio = 0.0;
        public double currentRightAmpRatio = 0.0;
        public double activeLeftAmpRatio = 0.0;
        public double activeRightAmpRatio = 0.0;

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

        private SteamControllerTritionControllerOptions nativeDeviceOptions;
        public SteamControllerTritionControllerOptions NativeDeviceOptions
        {
            get => nativeDeviceOptions;
        }

        public override event EventHandler Removal;

        public SteamControllerTritonDevice(HidDevice device, string displayName)
        {
            hidDevice = device;
            conType = DetermineConnectionType(hidDevice);
            baseElapsedReference = 250.0;
            deviceType = InputDeviceType.SteamControllerTriton;
            devTypeStr = displayName;

            deviceOptions = nativeDeviceOptions =
                new SteamControllerTritionControllerOptions(InputDeviceType.SteamControllerTriton);
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
            // TODO: Change Product IDs later

            // Initially assume a USB connection
            ConnectionType result = ConnectionType.USB;
            if (device.Attributes.ProductId == PROTEUS_DONGLE_PID ||
                device.Attributes.ProductId == NEREID_DONGLE_PID)
            {
                result = ConnectionType.SCDongle;
            }
            else if (device.Attributes.ProductId == BLE_PID)
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
            string test_serial = hidDevice.ReadSerial();
            serial = test_serial;
        }

        public static bool TestDongleSCConnected(HidDevice device)
        {
            bool result = false;

            // Test writing a feature report to exposed device.
            // Result will be false for any non-synced device slot
            byte[] featureData = new byte[FEATURE_REPORT_LEN];
            featureData[0] = 0x01;
            featureData[1] = 0xAE; // ID_GET_STRING_ATTRIBUTE
            featureData[2] = 0x15; // length 21 bytes
            featureData[3] = 0x01; // unknown
            result = device.WriteFeatureReport(featureData);

            /*if (result)
            {
                // Sleep seems to be needed when probing from a Dongle connection
                Thread.Sleep(100);

                byte[] retReportData = new byte[FEATURE_REPORT_LEN];
                retReportData[0] = 0x01;
                device.readFeatureData(retReportData);
                //Trace.WriteLine($"BYTE {retReportData[1]}");
            }
            */

            return result;
        }

        protected virtual void ClearMappings()
        {
            byte SETTING_LIZARD_OFF = 0x00;

            byte[] featureData = new byte[FEATURE_REPORT_LEN];
            featureData[0] = 0x01;
            featureData[1] = 0x87; // ID_SET_SETTINGS_VALUES
            featureData[2] = 0x03;
            featureData[3] = 0x09; // SETTING_LIZARD_MODE
            featureData[4] = SETTING_LIZARD_OFF; // SETTING_LIZARD_OFF (2 bytes)
            featureData[5] = (byte)(SETTING_LIZARD_OFF >> 8);
            hidDevice.WriteFeatureReport(featureData);
        }

        public void DisableLizardMode()
        {
            byte SETTING_LIZARD_OFF = 0x00;

            byte[] featureData = new byte[FEATURE_REPORT_LEN];
            featureData[0] = 0x01;
            featureData[1] = 0x87; // ID_SET_SETTINGS_VALUES
            featureData[2] = 0x03;
            featureData[3] = 0x09; // SETTING_LIZARD_MODE
            featureData[4] = SETTING_LIZARD_OFF; // SETTING_LIZARD_OFF (2 bytes)
            featureData[5] = (byte)(SETTING_LIZARD_OFF >> 8);
            hidDevice.WriteFeatureReport(featureData);
        }

        protected virtual void Configure()
        {
            byte gyroMode = 0x08 | 0x10; // SETTING_GYRO_MODE_SEND_RAW_ACCEL | SETTING_GYRO_MODE_SEND_RAW_GYRO
            byte hapticType = 0x01; // HAPTIC_TYPE_TICK
            byte hapticIntensity = 0x02; // HAPTIC_INTENSITY_MEDIUM
            short masterDbGain = 0x02; // 2 dB

            byte[] featureData = new byte[FEATURE_REPORT_LEN];
            featureData[0] = 0x01;
            featureData[1] = 0x87; // ID_SET_SETTINGS_VALUES
            featureData[2] = 0x0C; // Payload length

            featureData[3] = 0x30; // SETTING_IMU_MODE
            featureData[4] = gyroMode; // (setting value is 2 bytes)
            featureData[5] = (byte)(gyroMode >> 8);

            featureData[6] = 0x46; // SETTING_HAPTICS_ENABLED
            featureData[7] = hapticType; // (setting value is 2 bytes)
            featureData[8] = (byte)(hapticType >> 8);

            featureData[9] = 0x4F; // SETTING_HAPTIC_INTENSITY
            featureData[10] = hapticIntensity; // (setting value is 2 bytes)
            featureData[11] = (byte)(hapticIntensity >> 8);

            featureData[12] = 0x4C; // SETTING_HAPTIC_MASTER_GAIN_DB
            featureData[13] = (byte)(masterDbGain); // (setting value is 2 bytes)
            featureData[14] = (byte)(masterDbGain >> 8);

            hidDevice.WriteFeatureReport(featureData);
        }

        protected virtual void ChangeToLizardMode()
        {
            byte LIZARD_MODE_ON = 0x01;

            byte[] featureData = new byte[FEATURE_REPORT_LEN];
            featureData[0] = 0x01;
            featureData[1] = 0x87; // ID_SET_SETTINGS_VALUES
            featureData[2] = 0x03; // Length is 3 bytes
            featureData[3] = 0x09; // SETTING_LIZARD_MODE
            featureData[4] = LIZARD_MODE_ON; // LIZARD_MODE_ON (2 bytes)
            featureData[5] = (byte)(LIZARD_MODE_ON >> 8);
            hidDevice.WriteFeatureReport(featureData);
        }
        public void SyncStates()
        {
            previousState = currentState;
        }

        public void SendRumbleReportTest(byte[] buffer)
        {
            ushort rumbleSpeedLeft = (ushort)(currentLeftAmpRatio * 255.0);
            ushort rumbleSpeedRight = (ushort)(currentRightAmpRatio * 255.0);

            buffer[0] = 0x80; // ID_OUT_REPORT_HAPTIC_RUMBLE
            buffer[1] = 0x00; // type
            buffer[2] = 0x00; // intensity (byte 1)
            buffer[3] = 0x00; // intensity (byte 2)
            buffer[4] = (byte)(rumbleSpeedLeft); // left speed (byte 1)
            buffer[5] = (byte)(rumbleSpeedLeft >> 8); // left speed (byte 2)
            buffer[6] = 0x00; // left gain
            buffer[7] = (byte)(rumbleSpeedRight); // right speed (byte 1)
            buffer[8] = (byte)(rumbleSpeedRight >> 8); // right speed (byte 2)
            buffer[9] = 0x00; // right gain

            hidDevice.WriteOutputReportViaInterrupt(buffer, 100);
        }

        public void SendHapticsReportTest(byte[] buffer)
        {
            byte side = HAPTIC_POS_LEFT;
            ushort periodOnUs = (ushort)(0x0FFF * hapticInfo.leftActuatorAmpRatio);
            ushort periodOffUs = 0xFF;
            ushort count = (ushort)(hapticInfo.leftActuatorAmpRatio > 0.0 ? 0x01 : 0x00);

            buffer[0] = 0x81; // ID_OUT_REPORT_HAPTIC_PULSE
            buffer[1] = side;
            buffer[2] = (byte)(periodOnUs);
            buffer[3] = (byte)(periodOnUs >> 8);
            buffer[4] = (byte)(periodOffUs);
            buffer[5] = (byte)(periodOffUs >> 8);
            buffer[6] = (byte)(count);
            buffer[7] = (byte)(count >> 8);

            hidDevice.WriteOutputReportViaInterrupt(buffer, 100);

            side = HAPTIC_POS_RIGHT;
            periodOnUs = (ushort)(0x0FFF * hapticInfo.rightActuatorAmpRatio);
            periodOffUs = 0xFF;
            count = (ushort)(hapticInfo.rightActuatorAmpRatio > 0.0 ? 0x01 : 0x00);

            buffer[0] = 0x81; // ID_OUT_REPORT_HAPTIC_PULSE
            buffer[1] = side;
            buffer[2] = (byte)(periodOnUs);
            buffer[3] = (byte)(periodOnUs >> 8);
            buffer[4] = (byte)(periodOffUs);
            buffer[5] = (byte)(periodOffUs >> 8);
            buffer[6] = (byte)(count);
            buffer[7] = (byte)(count >> 8);

            hidDevice.WriteOutputReportViaInterrupt(buffer, 100);
        }

        public void ResetRumbleData()
        {
            currentLeftAmpRatio = currentRightAmpRatio = 0.0;
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

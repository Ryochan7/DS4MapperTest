using System;
using System.Collections.Generic;
using System.Text;

namespace DS4MapperTest.InputDevices.SteamControllerTritonLibrary
{
    public partial class SteamControllerTritonDevice
    {
        public const byte ID_TRITON_CONTROLLER_STATE = 0x42;
        public const byte ID_TRITON_BATTERY_STATUS = 0x43;
        public const byte ID_TRITON_CONTROLLER_STATE_BLE = 0x45;
        public const byte ID_TRITON_WIRELESS_STATUS_X = 0x46;
        public const byte ID_TRITON_WIRELESS_STATUS = 0x79;

        public enum ETritonReportIDTypes : byte
        {
            ID_TRITON_CONTROLLER_STATE = 0x42,
            ID_TRITON_BATTERY_STATUS = 0x43,
            ID_TRITON_CONTROLLER_STATE_BLE = 0x45,
            ID_TRITON_WIRELESS_STATUS_X = 0x46,
            ID_TRITON_CONTROLLER_STATE_TIMESTAMP = 0x47,

            ID_TRITON_WIRELESS_STATUS = 0x79,
        };

        public enum ETritonWirelessState : byte
        {
            k_ETritonWirelessStateDisconnect = 1,
            k_ETritonWirelessStateConnect = 2,
        };

        public enum HapticType : byte
        {
            HAPTIC_TYPE_OFF,
            HAPTIC_TYPE_TICK,
            HAPTIC_TYPE_CLICK,
            HAPTIC_TYPE_TONE,
            HAPTIC_TYPE_RUMBLE,
            HAPTIC_TYPE_NOISE,
            HAPTIC_TYPE_SCRIPT,
            HAPTIC_TYPE_LOG_SWEEP,
        }

        public enum HapticIntensity : byte
        {
            HAPTIC_INTENSITY_SYSTEM,
            HAPTIC_INTENSITY_SHORT,
            HAPTIC_INTENSITY_MEDIUM,
            HAPTIC_INTENSITY_LONG,
            HAPTIC_INTENSITY_INSANE,
        }

        public enum ValveTritonOutReportMessageIDs
        {
            ID_OUT_REPORT_HAPTIC_RUMBLE = 0x80,
            ID_OUT_REPORT_HAPTIC_PULSE = 0x81,
            ID_OUT_REPORT_HAPTIC_COMMAND = 0x82,
            ID_OUT_REPORT_HAPTIC_LFO_TONE = 0x83,
            ID_OUT_REPORT_HAPTIC_LOG_SWEEP = 0x84,
            ID_OUT_REPORT_HAPTIC_SCRIPT = 0x85,
        }

        public enum ValveInReportMessageIDs
        {
            ID_CONTROLLER_STATE = 1,
            ID_CONTROLLER_DEBUG = 2,
            ID_CONTROLLER_WIRELESS = 3,
            ID_CONTROLLER_STATUS = 4,
            ID_CONTROLLER_DEBUG2 = 5,
            ID_CONTROLLER_SECONDARY_STATE = 6,
            ID_CONTROLLER_BLE_STATE = 7,
            ID_CONTROLLER_DECK_STATE = 9,
            ID_CONTROLLER_MSG_COUNT
        }

        [Flags]
        public enum SettingGyroMode
        {
            SETTING_GYRO_MODE_OFF = 0x0000,
            SETTING_GYRO_MODE_STEERING = 0x0001,
            SETTING_GYRO_MODE_TILT = 0x0002,
            SETTING_GYRO_MODE_SEND_ORIENTATION = 0x0004,
            SETTING_GYRO_MODE_SEND_RAW_ACCEL = 0x0008,
            SETTING_GYRO_MODE_SEND_RAW_GYRO = 0x0010,
        }

        [Flags]
        public enum SettingHapticPulseFlags
        {
            HAPTIC_PULSE_NORMAL = 0x0000,
            HAPTIC_PULSE_HIGH_PRIORITY = 0x0001,
            HAPTIC_PULSE_VERY_HIGH_PRIORITY = 0x0002,
            HAPTIC_PULSE_IGNORE_USER_PREFS = 0x0003,
        }

        public enum ControllerSettings
        {
            SETTING_MOUSE_SENSITIVITY,
            SETTING_MOUSE_ACCELERATION,
            SETTING_TRACKBALL_ROTATION_ANGLE,
            SETTING_HAPTIC_INTENSITY_UNUSED,
            SETTING_LEFT_GAMEPAD_STICK_ENABLED,
            SETTING_RIGHT_GAMEPAD_STICK_ENABLED,
            SETTING_USB_DEBUG_MODE,
            SETTING_LEFT_TRACKPAD_MODE,
            SETTING_RIGHT_TRACKPAD_MODE,
            SETTING_LIZARD_MODE,

            // 10
            SETTING_DPAD_DEADZONE,
            SETTING_MINIMUM_MOMENTUM_VEL,
            SETTING_MOMENTUM_DECAY_AMOUNT,
            SETTING_TRACKPAD_RELATIVE_MODE_TICKS_PER_PIXEL,
            SETTING_HAPTIC_INCREMENT,
            SETTING_DPAD_ANGLE_SIN,
            SETTING_DPAD_ANGLE_COS,
            SETTING_MOMENTUM_VERTICAL_DIVISOR,
            SETTING_MOMENTUM_MAXIMUM_VELOCITY,
            SETTING_TRACKPAD_Z_ON,

            // 20
            SETTING_TRACKPAD_Z_OFF,
            SETTING_SENSITIVITY_SCALE_AMOUNT,
            SETTING_LEFT_TRACKPAD_SECONDARY_MODE,
            SETTING_RIGHT_TRACKPAD_SECONDARY_MODE,
            SETTING_SMOOTH_ABSOLUTE_MOUSE,
            SETTING_STEAMBUTTON_POWEROFF_TIME,
            SETTING_UNUSED_1,
            SETTING_TRACKPAD_OUTER_RADIUS,
            SETTING_TRACKPAD_Z_ON_LEFT,
            SETTING_TRACKPAD_Z_OFF_LEFT,

            // 30
            SETTING_TRACKPAD_OUTER_SPIN_VEL,
            SETTING_TRACKPAD_OUTER_SPIN_RADIUS,
            SETTING_TRACKPAD_OUTER_SPIN_HORIZONTAL_ONLY,
            SETTING_TRACKPAD_RELATIVE_MODE_DEADZONE,
            SETTING_TRACKPAD_RELATIVE_MODE_MAX_VEL,
            SETTING_TRACKPAD_RELATIVE_MODE_INVERT_Y,
            SETTING_TRACKPAD_DOUBLE_TAP_BEEP_ENABLED,
            SETTING_TRACKPAD_DOUBLE_TAP_BEEP_PERIOD,
            SETTING_TRACKPAD_DOUBLE_TAP_BEEP_COUNT,
            SETTING_TRACKPAD_OUTER_RADIUS_RELEASE_ON_TRANSITION,

            // 40
            SETTING_RADIAL_MODE_ANGLE,
            SETTING_HAPTIC_INTENSITY_MOUSE_MODE,
            SETTING_LEFT_DPAD_REQUIRES_CLICK,
            SETTING_RIGHT_DPAD_REQUIRES_CLICK,
            SETTING_LED_BASELINE_BRIGHTNESS,
            SETTING_LED_USER_BRIGHTNESS,
            SETTING_ENABLE_RAW_JOYSTICK,
            SETTING_ENABLE_FAST_SCAN,
            SETTING_IMU_MODE,
            SETTING_WIRELESS_PACKET_VERSION,

            // 50
            SETTING_SLEEP_INACTIVITY_TIMEOUT,
            SETTING_TRACKPAD_NOISE_THRESHOLD,
            SETTING_LEFT_TRACKPAD_CLICK_PRESSURE,
            SETTING_RIGHT_TRACKPAD_CLICK_PRESSURE,
            SETTING_LEFT_BUMPER_CLICK_PRESSURE,
            SETTING_RIGHT_BUMPER_CLICK_PRESSURE,
            SETTING_LEFT_GRIP_CLICK_PRESSURE,
            SETTING_RIGHT_GRIP_CLICK_PRESSURE,
            SETTING_LEFT_GRIP2_CLICK_PRESSURE,
            SETTING_RIGHT_GRIP2_CLICK_PRESSURE,

            // 60
            SETTING_PRESSURE_MODE,
            SETTING_CONTROLLER_TEST_MODE,
            SETTING_TRIGGER_MODE,
            SETTING_TRACKPAD_Z_THRESHOLD,
            SETTING_FRAME_RATE,
            SETTING_TRACKPAD_FILT_CTRL,
            SETTING_TRACKPAD_CLIP,
            SETTING_DEBUG_OUTPUT_SELECT,
            SETTING_TRIGGER_THRESHOLD_PERCENT,
            SETTING_TRACKPAD_FREQUENCY_HOPPING,

            // 70
            SETTING_HAPTICS_ENABLED,
            SETTING_STEAM_WATCHDOG_ENABLE,
            SETTING_TIMP_TOUCH_THRESHOLD_ON,
            SETTING_TIMP_TOUCH_THRESHOLD_OFF,
            SETTING_FREQ_HOPPING,
            SETTING_TEST_CONTROL,
            SETTING_HAPTIC_MASTER_GAIN_DB,
            SETTING_THUMB_TOUCH_THRESH,
            SETTING_DEVICE_POWER_STATUS,
            SETTING_HAPTIC_INTENSITY,

            // 80
            SETTING_STABILIZER_ENABLED,
            SETTING_TIMP_MODE_MTE,
            SETTING_COUNT,

            // This is a special setting value use for callbacks and should not be set/get explicitly.
            SETTING_ALL = 0xFF
        }

        public enum FeatureReportMessageIDs
        {
            ID_SET_DIGITAL_MAPPINGS = 0x80,
            ID_CLEAR_DIGITAL_MAPPINGS = 0x81,
            ID_GET_DIGITAL_MAPPINGS = 0x82,
            ID_GET_ATTRIBUTES_VALUES = 0x83,
            ID_GET_ATTRIBUTE_LABEL = 0x84,
            ID_SET_DEFAULT_DIGITAL_MAPPINGS = 0x85,
            ID_FACTORY_RESET = 0x86,
            ID_SET_SETTINGS_VALUES = 0x87,
            ID_CLEAR_SETTINGS_VALUES = 0x88,
            ID_GET_SETTINGS_VALUES = 0x89,
            ID_GET_SETTING_LABEL = 0x8A,
            ID_GET_SETTINGS_MAXS = 0x8B,
            ID_GET_SETTINGS_DEFAULTS = 0x8C,
            ID_SET_CONTROLLER_MODE = 0x8D,
            ID_LOAD_DEFAULT_SETTINGS = 0x8E,
            ID_TRIGGER_HAPTIC_PULSE = 0x8F,

            ID_TURN_OFF_CONTROLLER = 0x9F,

            ID_GET_DEVICE_INFO = 0xA1,

            ID_CALIBRATE_TRACKPADS = 0xA7,
            ID_RESERVED_0 = 0xA8,
            ID_SET_SERIAL_NUMBER = 0xA9,
            ID_GET_TRACKPAD_CALIBRATION = 0xAA,
            ID_GET_TRACKPAD_FACTORY_CALIBRATION = 0xAB,
            ID_GET_TRACKPAD_RAW_DATA = 0xAC,
            ID_ENABLE_PAIRING = 0xAD,
            ID_GET_STRING_ATTRIBUTE = 0xAE,
            ID_RADIO_ERASE_RECORDS = 0xAF,
            ID_RADIO_WRITE_RECORD = 0xB0,
            ID_SET_DONGLE_SETTING = 0xB1,
            ID_DONGLE_DISCONNECT_DEVICE = 0xB2,
            ID_DONGLE_COMMIT_DEVICE = 0xB3,
            ID_DONGLE_GET_WIRELESS_STATE = 0xB4,
            ID_CALIBRATE_GYRO = 0xB5,
            ID_PLAY_AUDIO = 0xB6,
            ID_AUDIO_UPDATE_START = 0xB7,
            ID_AUDIO_UPDATE_DATA = 0xB8,
            ID_AUDIO_UPDATE_COMPLETE = 0xB9,
            ID_GET_CHIPID = 0xBA,

            ID_CALIBRATE_JOYSTICK = 0xBF,
            ID_CALIBRATE_ANALOG_TRIGGERS = 0xC0,
            ID_SET_AUDIO_MAPPING = 0xC1,
            ID_CHECK_GYRO_FW_LOAD = 0xC2,
            ID_CALIBRATE_ANALOG = 0xC3,
            ID_DONGLE_GET_CONNECTED_SLOTS = 0xC4,

            ID_RESET_IMU = 0xCE,

            // Deck only
            ID_TRIGGER_HAPTIC_CMD = 0xEA,
            ID_TRIGGER_RUMBLE_CMD = 0xEB,
        }

        public enum WirelessEventTypes
        {
            WIRELESS_EVENT_DISCONNECT = 1,
            WIRELESS_EVENT_CONNECT = 2,
            WIRELESS_EVENT_PAIR = 3,
        }
    }
}

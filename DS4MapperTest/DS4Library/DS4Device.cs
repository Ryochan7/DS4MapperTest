using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;

namespace DS4MapperTest.DS4Library
{
    public struct DS4ForceFeedbackState
    {
        public byte LeftHeavy;
        public byte RightLight;
    }

    public class DS4Device : InputDeviceBase
    {
        public enum ConnectionType : ushort
        {
            Bluetooth,
            SonyWA,
            USB,
        }

        private const int BT_OUTPUT_REPORT_LENGTH = 334;
        private const int BT_OUTPUT_REPORT_0x15_LENGTH = BT_OUTPUT_REPORT_LENGTH;
        private const int BT_OUTPUT_REPORT_0x11_LENGTH = 78;
        private const int BT_INPUT_REPORT_LENGTH = 547;
        private const int USB_INPUT_REPORT_LENGTH = 64;
        public const string BLANK_SERIAL = "00:00:00:00:00:00";
        private const byte SERIAL_FEATURE_ID = 18;
        protected const int DS4_FEATURE_REPORT_5_LEN = 41;
        protected const int DS4_FEATURE_REPORT_5_CRC32_POS = DS4_FEATURE_REPORT_5_LEN - 4;
        public const int BT_INPUT_REPORT_CRC32_POS = 74; // last 4 bytes of the 78-sized input report are crc32
        private const int READ_STREAM_TIMEOUT = 100;
        // Maximum values for battery level when no USB cable is connected
        // and when a USB cable is connected
        public const int BATTERY_MAX = 8;
        public const int BATTERY_MAX_USB = 11;
        private const string DEVICE_TYPE_STRING = "DualShock4";
        private byte[] outputBTCrc32Head = new byte[] { 0xA2 };

        private HidDevice hidDevice;
        public HidDevice HidDevice => hidDevice;

        private ConnectionType conType;
        public ConnectionType DevConnectionType => conType;

        protected DS4State currentState;
        public DS4State CurrentState { get => currentState; }
        public ref DS4State CurrentStateRef { get => ref currentState; }

        protected DS4State previousState;
        public DS4State PreviousState { get => previousState; }
        public ref DS4State PreviousStateRef { get => ref previousState; }

        private DS4ForceFeedbackState feedbackState = new DS4ForceFeedbackState();
        public DS4ForceFeedbackState FeedbackState { get => feedbackState; }
        public ref DS4ForceFeedbackState FeedbackStateRef { get => ref feedbackState; }

        private int inputReportLen;
        private int outputReportLen;
        private int btOutputPayloadLen;
        public int InputReportLen { get => inputReportLen; }
        public int OutputReportLen { get => outputReportLen; }
        public int BtOutputPayloadLen { get => btOutputPayloadLen; }

        public override event EventHandler Removal;

        public DS4Device(HidDevice device, string displayName)
        {
            this.hidDevice = device;
            conType = DetermineConnectionType(device);
            deviceType = InputDeviceType.DS4;
            //devTypeStr = DEVICE_TYPE_STRING;
            devTypeStr = displayName;
            PostInit();
        }

        public void PostInit()
        {
            if (conType == ConnectionType.Bluetooth)
            {
                inputReportLen = BT_INPUT_REPORT_LENGTH;
                outputReportLen = BT_OUTPUT_REPORT_LENGTH;
                // Buffer len and output report payload len will differ
                btOutputPayloadLen = BT_OUTPUT_REPORT_0x11_LENGTH;
            }
            else if (conType == ConnectionType.USB)
            {
                inputReportLen = USB_INPUT_REPORT_LENGTH;
                outputReportLen = hidDevice.Capabilities.OutputReportByteLength;
            }

            // Read device serial number. Also sets input mode to DS4 mode
            serial = hidDevice.ReadSerial(SERIAL_FEATURE_ID);

            // Grab calibration data from IMU
            RefreshCalibration();

            baseElapsedReference = 250.0;
            deviceOptions = new DS4ControllerOptions(deviceType);
            synced = true;
        }

        public static ConnectionType DetermineConnectionType(HidDevice device)
        {
            ConnectionType result = ConnectionType.USB;
            if (device.Capabilities.InputReportByteLength == 64)
            {
                if (device.Capabilities.NumberFeatureDataIndices == 22)
                {
                    result = ConnectionType.SonyWA;
                }
            }
            else
            {
                result = ConnectionType.Bluetooth;
            }

            return result;
        }

        public override void SetOperational()
        {
            if (!hidDevice.IsFileStreamOpen())
            {
                hidDevice.OpenFileStream(outputReportLen);
            }

            NativeMethods.HidD_SetNumInputBuffers(hidDevice.safeReadHandle.DangerousGetHandle(), 3);
        }

        private void RefreshCalibration()
        {
            byte[] calibration = new byte[41];
            calibration[0] = conType == ConnectionType.Bluetooth ? (byte)0x05 : (byte)0x02;

            if (conType == ConnectionType.Bluetooth)
            {
                bool found = false;
                for (int tries = 0; !found && tries < 5; tries++)
                {
                    hidDevice.readFeatureData(calibration);
                    uint recvCrc32 = calibration[DS4_FEATURE_REPORT_5_CRC32_POS] |
                                (uint)(calibration[DS4_FEATURE_REPORT_5_CRC32_POS + 1] << 8) |
                                (uint)(calibration[DS4_FEATURE_REPORT_5_CRC32_POS + 2] << 16) |
                                (uint)(calibration[DS4_FEATURE_REPORT_5_CRC32_POS + 3] << 24);

                    uint calcCrc32 = ~Crc32Algorithm.Compute(new byte[] { 0xA3 });
                    calcCrc32 = ~Crc32Algorithm.CalculateBasicHash(ref calcCrc32, ref calibration, 0, DS4_FEATURE_REPORT_5_LEN - 4);
                    bool validCrc = recvCrc32 == calcCrc32;
                    if (!validCrc && tries >= 5)
                    {
                        continue;
                    }
                    else if (validCrc)
                    {
                        found = true;
                    }
                }
            }
            else
            {
                hidDevice.readFeatureData(calibration);
            }
        }

        public void SyncStates()
        {
            previousState = currentState;
        }

        public override void Detach()
        {
            byte[] outputReportBuffer = new byte[outputReportLen];
            PrepareOutputReport(outputReportBuffer);
            if (conType == ConnectionType.USB)
            {
                outputReportBuffer[6] = outputReportBuffer[7] = outputReportBuffer[8] = 0;
            }
            else if (conType == ConnectionType.Bluetooth)
            {
                outputReportBuffer[8] = outputReportBuffer[9] = outputReportBuffer[10] = 0;

                // Need to calculate and populate CRC-32 data so controller will accept the report
                //int len = outputReport.Length;
                int len = btOutputPayloadLen;
                uint calcCrc32 = ~Crc32Algorithm.Compute(outputBTCrc32Head);
                calcCrc32 = ~Crc32Algorithm.CalculateBasicHash(ref calcCrc32, ref outputReportBuffer, 0, len - 4);
                outputReportBuffer[len - 4] = (byte)calcCrc32;
                outputReportBuffer[len - 3] = (byte)(calcCrc32 >> 8);
                outputReportBuffer[len - 2] = (byte)(calcCrc32 >> 16);
                outputReportBuffer[len - 1] = (byte)(calcCrc32 >> 24);
            }

            WriteReport(outputReportBuffer);
            hidDevice.CloseDevice();
        }

        public void PurgeRemoval()
        {
            Removal = null;
        }

        public void RaiseRemoval()
        {
            Removal?.Invoke(this, EventArgs.Empty);
        }

        public void PrepareOutputReport(byte[] outReportBuffer)
        {
            if (conType == ConnectionType.Bluetooth)
            {
                outReportBuffer[0] = 0x11;
                //outReportBuffer[0] = 0x15;
                //outReportBuffer[1] = (byte)(0x80 | btPollRate); // input report rate
                outReportBuffer[1] = (byte)(0xC0 | 0x04); // input report rate
                //outReportBuffer[2] = 0xA0;

                // Headphone volume L (0x10), Headphone volume R (0x20), Mic volume (0x40), Speaker volume (0x80)
                // enable rumble (0x01), lightbar (0x02), flash (0x04). Default: 0x07
                outReportBuffer[3] = 0x07;
                outReportBuffer[4] = 0x04;

                outReportBuffer[6] = feedbackState.RightLight; // fast motor
                outReportBuffer[7] = feedbackState.LeftHeavy; // slow motor
                outReportBuffer[8] = 0; // red
                outReportBuffer[9] = 0; // green
                outReportBuffer[10] = 255; // blue
                outReportBuffer[11] = 0; // flash on duration
                outReportBuffer[12] = 0; // flash off duration

                // Need to calculate and populate CRC-32 data so controller will accept the report
                //int len = outputReport.Length;
                int len = btOutputPayloadLen;
                uint calcCrc32 = ~Crc32Algorithm.Compute(outputBTCrc32Head);
                calcCrc32 = ~Crc32Algorithm.CalculateBasicHash(ref calcCrc32, ref outReportBuffer, 0, len - 4);
                outReportBuffer[len - 4] = (byte)calcCrc32;
                outReportBuffer[len - 3] = (byte)(calcCrc32 >> 8);
                outReportBuffer[len - 2] = (byte)(calcCrc32 >> 16);
                outReportBuffer[len - 1] = (byte)(calcCrc32 >> 24);

            }
            else
            {
                outReportBuffer[0] = 0x05;
                // Headphone volume L (0x10), Headphone volume R (0x20), Mic volume (0x40), Speaker volume (0x80)
                // enable rumble (0x01), lightbar (0x02), flash (0x04). Default: 0x07
                outReportBuffer[1] = 0x07;
                outReportBuffer[2] = 0x04;
                outReportBuffer[4] = feedbackState.RightLight; // fast motor
                outReportBuffer[5] = feedbackState.LeftHeavy; // slow  motor
                outReportBuffer[6] = 0; // red
                outReportBuffer[7] = 0; // green
                outReportBuffer[8] = 255; // blue
                //outReportBuffer[6] = currentHap.lightbarState.LightBarColor.red; // red
                //outReportBuffer[7] = currentHap.lightbarState.LightBarColor.green; // green
                //outReportBuffer[8] = currentHap.lightbarState.LightBarColor.blue; // blue
                //outReportBuffer[9] = currentHap.lightbarState.LightBarFlashDurationOn; // flash on duration
                //outReportBuffer[10] = currentHap.lightbarState.LightBarFlashDurationOff; // flash off duration
            }
        }

        public void WriteReport(byte[] outReportBuffer)
        {
            hidDevice.WriteOutputReportViaInterrupt(outReportBuffer, READ_STREAM_TIMEOUT);
        }
    }
}

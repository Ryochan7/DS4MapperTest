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

    public struct DS4Color
    {
        public byte red;
        public byte green;
        public byte blue;

        public DS4Color()
        {
            red = 0;
            green = 0;
            blue = 255;
        }
    }

    internal class CalibData
    {
        public int bias;
        public int sensNumer;
        public int sensDenom;
        public const int GyroPitchIdx = 0, GyroYawIdx = 1, GyroRollIdx = 2,
            AccelXIdx = 3, AccelYIdx = 4, AccelZIdx = 5;
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
        private const int DS4_FEATURE_REPORT_5_LEN = 41;
        private const int DS4_FEATURE_REPORT_5_CRC32_POS = DS4_FEATURE_REPORT_5_LEN - 4;
        public const int BT_INPUT_REPORT_CRC32_POS = 74; // last 4 bytes of the 78-sized input report are crc32
        private const int READ_STREAM_TIMEOUT = 100;
        // Maximum values for battery level when no USB cable is connected
        // and when a USB cable is connected
        public const int BATTERY_MAX = 8;
        public const int BATTERY_MAX_USB = 11;
        private const string DEVICE_TYPE_STRING = "DualShock4";

        public const int ACC_RES_PER_G = 8192;
        public const float F_ACC_RES_PER_G = ACC_RES_PER_G;
        public const int GYRO_RES_IN_DEG_SEC = 16;
        public const float F_GYRO_RES_IN_DEG_SEC = GYRO_RES_IN_DEG_SEC;

        private byte[] outputBTCrc32Head = new byte[] { 0xA2 };

        private CalibData[] calibrationData = new CalibData[6]
        {
            // Pitch, Yaw, Roll
            new CalibData(), new CalibData(), new CalibData(),
            // AccelX, AccelY, AccelZ
            new CalibData(), new CalibData(), new CalibData()
        };
        private bool calibrationDone = false;
        public bool CalibrationDone => calibrationDone;
        int tempInt = 0;

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

        private DS4Color lightbarColor = new DS4Color();
        public DS4Color LightbarColor { get => lightbarColor; }
        public ref DS4Color LightbarColorRef { get => ref lightbarColor; }

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

                if (found)
                {
                    SetCalibrationData(ref calibration, conType == ConnectionType.USB);
                }
            }
            else
            {
                hidDevice.readFeatureData(calibration);
                SetCalibrationData(ref calibration, conType == ConnectionType.USB);
            }
        }

        private void SetCalibrationData(ref byte[] calibData, bool useAltGyroCalib)
        {
            int pitchPlus, pitchMinus, yawPlus, yawMinus, rollPlus, rollMinus,
                accelXPlus, accelXMinus, accelYPlus, accelYMinus, accelZPlus, accelZMinus,
                gyroSpeedPlus, gyroSpeedMinus;

            calibrationData[0].bias = (short)((ushort)(calibData[2] << 8) | calibData[1]);
            calibrationData[1].bias = (short)((ushort)(calibData[4] << 8) | calibData[3]);
            calibrationData[2].bias = (short)((ushort)(calibData[6] << 8) | calibData[5]);

            if (!useAltGyroCalib)
            {
                pitchPlus = (short)((ushort)(calibData[8] << 8) | calibData[7]);
                yawPlus = (short)((ushort)(calibData[10] << 8) | calibData[9]);
                rollPlus = (short)((ushort)(calibData[12] << 8) | calibData[11]);
                pitchMinus = (short)((ushort)(calibData[14] << 8) | calibData[13]);
                yawMinus = (short)((ushort)(calibData[16] << 8) | calibData[15]);
                rollMinus = (short)((ushort)(calibData[18] << 8) | calibData[17]);
            }
            else
            {
                pitchPlus = (short)((ushort)(calibData[8] << 8) | calibData[7]);
                pitchMinus = (short)((ushort)(calibData[10] << 8) | calibData[9]);
                yawPlus = (short)((ushort)(calibData[12] << 8) | calibData[11]);
                yawMinus = (short)((ushort)(calibData[14] << 8) | calibData[13]);
                rollPlus = (short)((ushort)(calibData[16] << 8) | calibData[15]);
                rollMinus = (short)((ushort)(calibData[18] << 8) | calibData[17]);
            }

            gyroSpeedPlus = (short)((ushort)(calibData[20] << 8) | calibData[19]);
            gyroSpeedMinus = (short)((ushort)(calibData[22] << 8) | calibData[21]);
            accelXPlus = (short)((ushort)(calibData[24] << 8) | calibData[23]);
            accelXMinus = (short)((ushort)(calibData[26] << 8) | calibData[25]);

            accelYPlus = (short)((ushort)(calibData[28] << 8) | calibData[27]);
            accelYMinus = (short)((ushort)(calibData[30] << 8) | calibData[29]);

            accelZPlus = (short)((ushort)(calibData[32] << 8) | calibData[31]);
            accelZMinus = (short)((ushort)(calibData[34] << 8) | calibData[33]);

            int gyroSpeed2x = (gyroSpeedPlus + gyroSpeedMinus);
            calibrationData[0].sensNumer = gyroSpeed2x * GYRO_RES_IN_DEG_SEC;
            calibrationData[0].sensDenom = pitchPlus - pitchMinus;

            calibrationData[1].sensNumer = gyroSpeed2x * GYRO_RES_IN_DEG_SEC;
            calibrationData[1].sensDenom = yawPlus - yawMinus;

            calibrationData[2].sensNumer = gyroSpeed2x * GYRO_RES_IN_DEG_SEC;
            calibrationData[2].sensDenom = rollPlus - rollMinus;

            int accelRange = tempInt = accelXPlus - accelXMinus;
            calibrationData[3].bias = accelXPlus - accelRange / 2;
            calibrationData[3].sensNumer = 2 * ACC_RES_PER_G;
            calibrationData[3].sensDenom = accelRange;

            accelRange = tempInt = accelYPlus - accelYMinus;
            calibrationData[4].bias = accelYPlus - accelRange / 2;
            calibrationData[4].sensNumer = 2 * ACC_RES_PER_G;
            calibrationData[4].sensDenom = accelRange;

            accelRange = tempInt = accelZPlus - accelZMinus;
            calibrationData[5].bias = accelZPlus - accelRange / 2;
            calibrationData[5].sensNumer = 2 * ACC_RES_PER_G;
            calibrationData[5].sensDenom = accelRange;

            // Check that denom will not be zero.
            calibrationDone = calibrationData[0].sensDenom != 0 &&
                calibrationData[1].sensDenom != 0 &&
                calibrationData[2].sensDenom != 0 &&
                accelRange != 0;
        }

        public void ApplyCalibs(ref int yaw, ref int pitch, ref int roll,
            ref int accelX, ref int accelY, ref int accelZ)
        {
            CalibData current = calibrationData[0];
            tempInt = pitch - current.bias;
            pitch = (int)(tempInt * (current.sensNumer / (float)current.sensDenom));

            current = calibrationData[1];
            tempInt = yaw - current.bias;
            yaw = (int)(tempInt * (current.sensNumer / (float)current.sensDenom));

            current = calibrationData[2];
            tempInt = roll - current.bias;
            roll = (int)(tempInt * (current.sensNumer / (float)current.sensDenom));

            current = calibrationData[3];
            tempInt = accelX - current.bias;
            accelX = (int)(tempInt * (current.sensNumer / (float)current.sensDenom));

            current = calibrationData[4];
            tempInt = accelY - current.bias;
            accelY = (int)(tempInt * (current.sensNumer / (float)current.sensDenom));

            current = calibrationData[5];
            tempInt = accelZ - current.bias;
            accelZ = (int)(tempInt * (current.sensNumer / (float)current.sensDenom));
        }

        public void SyncStates()
        {
            previousState = currentState;
        }

        public override void Detach()
        {
            byte[] outputReportBuffer = new byte[outputReportLen];
            lightbarColor.red = lightbarColor.green = lightbarColor.blue = 0;

            PrepareOutputReport(outputReportBuffer);
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
                outReportBuffer[8] = lightbarColor.red; // red
                outReportBuffer[9] = lightbarColor.green; // green
                outReportBuffer[10] = lightbarColor.blue; // blue
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
                outReportBuffer[6] = lightbarColor.red; // red
                outReportBuffer[7] = lightbarColor.green; // green
                outReportBuffer[8] = lightbarColor.blue; // blue
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

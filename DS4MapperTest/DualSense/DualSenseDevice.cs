﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;
using DS4MapperTest.DS4Library;

namespace DS4MapperTest.DualSense
{
    internal class CalibData
    {
        public int bias;
        public int sensNumer;
        public int sensDenom;
        public const int GyroPitchIdx = 0, GyroYawIdx = 1, GyroRollIdx = 2,
            AccelXIdx = 3, AccelYIdx = 4, AccelZIdx = 5;
    }

    public class DualSenseDevice : InputDeviceBase
    {
        public enum ConnectionType : ushort
        {
            Bluetooth,
            USB,
        }

        public enum DeviceSubType : ushort
        {
            DualSense,
            DSEdge,
        }

        public const int SONY_VID = 0x054C;
        public const int SONY_DUALSENSE_PID = 0x0CE6;
        public const int SONY_DUALSENSE_EDGE_PID = 0x0DF2;

        private const int BT_OUTPUT_REPORT_LENGTH = 78;
        private const int BT_INPUT_REPORT_LENGTH = 78;
        private const int READ_STREAM_TIMEOUT = 100;
        public const int BATTERY_MAX = 8;
        private const byte SERIAL_FEATURE_ID = 9;
        private const byte OUTPUT_REPORT_ID_USB = 0x02;
        private const byte OUTPUT_REPORT_ID_BT = 0x31;
        private const byte OUTPUT_REPORT_ID_DATA = 0x02;
        private const int DS4_FEATURE_REPORT_5_LEN = 41;
        private const int DS4_FEATURE_REPORT_5_CRC32_POS = DS4_FEATURE_REPORT_5_LEN - 4;

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

        protected DualSenseState currentState;
        public DualSenseState CurrentState { get => currentState; }
        public ref DualSenseState CurrentStateRef { get => ref currentState; }

        protected DualSenseState previousState;
        public DualSenseState PreviousState
        {
            get => previousState;
            set => previousState = value;
        }
        public ref DualSenseState PreviousStateRef { get => ref previousState; }

        private DS4ForceFeedbackState feedbackState = new DS4ForceFeedbackState();
        public DS4ForceFeedbackState FeedbackState
        {
            get => feedbackState;
            set => feedbackState = value;
        }
        public ref DS4ForceFeedbackState FeedbackStateRef { get => ref feedbackState; }

        private DS4ForceFeedbackState hapticsState = new DS4ForceFeedbackState();
        public DS4ForceFeedbackState HapticsState
        {
            get => hapticsState;
            set => hapticsState = value;
        }
        public ref DS4ForceFeedbackState HapticsStateRef { get => ref hapticsState; }

        private DS4Color lightbarColor = new DS4Color();
        public DS4Color LightbarColor { get => lightbarColor; }
        public ref DS4Color LightbarColorRef { get => ref lightbarColor; }

        private bool hapticsDirty = false;
        public bool HapticsDirty
        {
            get => hapticsDirty;
            set => hapticsDirty = value;
        }

        private bool rumbleDirty = false;
        public bool RumbleDirty
        {
            get => rumbleDirty;
            set => rumbleDirty = value;
        }

        private int inputReportLen;
        private int outputReportLen;
        public int InputReportLen { get => inputReportLen; }
        public int OutputReportLen { get => outputReportLen; }

        private DeviceSubType subType = DeviceSubType.DualSense;
        public DeviceSubType SubType => subType;

        public override event EventHandler Removal;

        public DualSenseDevice(HidDevice device, string displayName)
        {
            this.hidDevice = device;
            conType = DetermineConnectionType(device);
            deviceType = InputDeviceType.DualSense;
            devTypeStr = displayName;
            PostInit();
        }

        public void PostInit()
        {
            if (conType == ConnectionType.USB)
            {
                inputReportLen = 64;
                outputReportLen = hidDevice.Capabilities.OutputReportByteLength;
            }
            else
            {
                inputReportLen = BT_INPUT_REPORT_LENGTH;
                outputReportLen = BT_OUTPUT_REPORT_LENGTH;
            }

            // Read device serial number. Also sets input mode to DS4 mode
            serial = hidDevice.ReadSerial(SERIAL_FEATURE_ID);

            // Grab calibration data from IMU
            RefreshCalibration();

            // Check if using a normal DualSense or a DualSense Edge
            DetermineSubType(hidDevice);

            baseElapsedReference = 250.0;
            deviceOptions = new DualSenseControllerOptions(deviceType);
            synced = true;
        }

        private void DetermineSubType(HidDevice device)
        {
            subType = DeviceSubType.DualSense;

            if (device.Attributes.VendorId == SONY_VID &&
                device.Attributes.ProductId == SONY_DUALSENSE_EDGE_PID)
            {
                subType = DeviceSubType.DSEdge;
            }
        }

        public static ConnectionType DetermineConnectionType(HidDevice device)
        {
            ConnectionType result = ConnectionType.USB;
            if (device.Capabilities.InputReportByteLength == 64)
            {
                result = ConnectionType.USB;
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
            calibration[0] = conType == ConnectionType.Bluetooth ? (byte)0x05 : (byte)0x05;

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
                    SetCalibrationData(ref calibration, true);
                }
            }
            else
            {
                hidDevice.readFeatureData(calibration);
                SetCalibrationData(ref calibration, true);
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


        public override void Detach()
        {
            byte[] outputReportBuffer = new byte[outputReportLen];
            lightbarColor.red = lightbarColor.green = lightbarColor.blue = 0xFF;

            PrepareOutputReport(outputReportBuffer);
            WriteReport(outputReportBuffer);
            hidDevice.CloseDevice();
        }

        public void PrepareOutputReport(byte[] outReportBuffer, bool rumble = true)
        {
            if (conType == ConnectionType.Bluetooth)
            {
                outReportBuffer[0] = OUTPUT_REPORT_ID_BT; // Report ID
                outReportBuffer[1] = OUTPUT_REPORT_ID_DATA;

                // 0x01 Set the main motors (also requires flag 0x02)
                // 0x02 Set the main motors (also requires flag 0x01)
                // 0x04 Set the right trigger motor
                // 0x08 Set the left trigger motor
                // 0x10 Enable modification of audio volume
                // 0x20 Enable internal speaker (even while headset is connected)
                // 0x40 Enable modification of microphone volume
                // 0x80 Enable internal mic (even while headset is connected)
                outReportBuffer[2] = 0x0F;

                // 0x01 Toggling microphone LED, 0x02 Toggling Audio/Mic Mute
                // 0x04 Toggling LED strips on the sides of the Touchpad, 0x08 Turn off all LED lights
                // 0x10 Toggle player LED lights below Touchpad, 0x20 ???
                // 0x40 Adjust overall motor/effect power, 0x80 ???
                outReportBuffer[3] = 0x55;

                // Right? High Freq Motor
                outReportBuffer[4] = rumble ? feedbackState.RightLight : hapticsState.RightLight;
                // Left? Low Freq Motor
                outReportBuffer[5] = rumble ? feedbackState.LeftHeavy : hapticsState.LeftHeavy;

                // Mute button LED. 0x01 = Solid. 0x02 = Pulsating
                outReportBuffer[10] = 0x01;

                /* Lightbar colors */
                outReportBuffer[46] = lightbarColor.red;
                outReportBuffer[47] = lightbarColor.green;
                outReportBuffer[48] = lightbarColor.blue;

                // Need to calculate and populate CRC-32 data so controller will accept the report
                //int len = outputReport.Length;
                int len = BT_OUTPUT_REPORT_LENGTH;
                uint calcCrc32 = ~Crc32Algorithm.Compute(outputBTCrc32Head);
                calcCrc32 = ~Crc32Algorithm.CalculateBasicHash(ref calcCrc32, ref outReportBuffer, 0, len - 4);
                outReportBuffer[len - 4] = (byte)calcCrc32;
                outReportBuffer[len - 3] = (byte)(calcCrc32 >> 8);
                outReportBuffer[len - 2] = (byte)(calcCrc32 >> 16);
                outReportBuffer[len - 1] = (byte)(calcCrc32 >> 24);

            }
            else
            {
                outReportBuffer[0] = OUTPUT_REPORT_ID_USB; // Report ID
                // 0x01 Set the main motors (also requires flag 0x02)
                // 0x02 Set the main motors (also requires flag 0x01)
                // 0x04 Set the right trigger motor
                // 0x08 Set the left trigger motor
                // 0x10 Enable modification of audio volume
                // 0x20 Enable internal speaker (even while headset is connected)
                // 0x40 Enable modification of microphone volume
                // 0x80 Enable internal mic (even while headset is connected)
                outReportBuffer[1] = 0x0F;
                // 0x01 Toggling microphone LED, 0x02 Toggling Audio/Mic Mute
                // 0x04 Toggling LED strips on the sides of the Touchpad, 0x08 Turn off all LED lights
                // 0x10 Toggle player LED lights below Touchpad, 0x20 ???
                // 0x40 Adjust overall motor/effect power, 0x80 ???
                outReportBuffer[2] = 0x55;
                // Right? High Freq Motor
                outReportBuffer[3] = rumble ? feedbackState.RightLight : hapticsState.RightLight;
                // Left? Low Freq Motor
                outReportBuffer[4] = rumble ? feedbackState.LeftHeavy : hapticsState.LeftHeavy;

                // Mute button LED. 0x01 = Solid. 0x02 = Pulsating
                outReportBuffer[9] = 0x01;
                // audio settings requiring mute toggling flags
                outReportBuffer[10] = 0x00; // 0x10 microphone mute, 0x40 audio mute

                /* Lightbar colors */
                outReportBuffer[45] = lightbarColor.red;
                outReportBuffer[46] = lightbarColor.green;
                outReportBuffer[47] = lightbarColor.blue;
            }
        }

        public bool WriteReport(byte[] outReportBuffer)
        {
            bool result;
            if (conType == ConnectionType.Bluetooth)
            {
                // DualSense seems to only accept output data via the Interrupt endpoint
                result = hidDevice.WriteOutputReportViaInterrupt(outReportBuffer, READ_STREAM_TIMEOUT);
                //result = hDevice.WriteOutputReportViaControl(outputReport);
            }
            else
            {
                result = hidDevice.WriteOutputReportViaInterrupt(outReportBuffer, READ_STREAM_TIMEOUT);
            }

            //Console.WriteLine("STAUTS: {0}", result);
            return result;
        }

        public void SetLightbarColor(ref DS4Color color)
        {
            if (!lightbarColor.Equals(color))
            {
                hapticsDirty = true;
            }

            lightbarColor = color;
        }

        public void SetForceFeedbackState(ref DS4ForceFeedbackState state)
        {
            feedbackState = state;
            hapticsDirty = true;
        }

        public void SyncStates()
        {
            previousState = currentState;
        }

        public void PurgeRemoval()
        {
            Removal = null;
        }

        public void RaiseRemoval()
        {
            Removal?.Invoke(this, EventArgs.Empty);
        }

        public void ResetRumbleData()
        {
            feedbackState.LeftHeavy = feedbackState.RightLight = 0;
        }
    }
}

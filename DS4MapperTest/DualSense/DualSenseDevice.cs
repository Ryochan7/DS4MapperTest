using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;
using DS4MapperTest.DS4Library;

namespace DS4MapperTest.DualSense
{
    public class DualSenseDevice : InputDeviceBase
    {
        public enum ConnectionType : ushort
        {
            Bluetooth,
            USB,
        }

        private const int BT_OUTPUT_REPORT_LENGTH = 78;
        private const int BT_INPUT_REPORT_LENGTH = 78;
        private const int READ_STREAM_TIMEOUT = 100;
        public const int BATTERY_MAX = 8;
        private const byte SERIAL_FEATURE_ID = 9;
        private const byte OUTPUT_REPORT_ID_USB = 0x02;
        private const byte OUTPUT_REPORT_ID_BT = 0x31;
        private const byte OUTPUT_REPORT_ID_DATA = 0x02;
        private byte[] outputBTCrc32Head = new byte[] { 0xA2 };

        private HidDevice hidDevice;
        public HidDevice HidDevice => hidDevice;

        private ConnectionType conType;
        public ConnectionType DevConnectionType => conType;

        protected DualSenseState currentState;
        public DualSenseState CurrentState { get => currentState; }
        public ref DualSenseState CurrentStateRef { get => ref currentState; }

        protected DualSenseState previousState;
        public DualSenseState PreviousState { get => previousState; }
        public ref DualSenseState PreviousStateRef { get => ref previousState; }

        private DS4ForceFeedbackState feedbackState = new DS4ForceFeedbackState();
        public DS4ForceFeedbackState FeedbackState { get => feedbackState; }
        public ref DS4ForceFeedbackState FeedbackStateRef { get => ref feedbackState; }

        private DS4Color lightbarColor = new DS4Color();
        public DS4Color LightbarColor { get => lightbarColor; }
        public ref DS4Color LightbarColorRef { get => ref lightbarColor; }

        private int inputReportLen;
        private int outputReportLen;
        public int InputReportLen { get => inputReportLen; }
        public int OutputReportLen { get => outputReportLen; }

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

            baseElapsedReference = 250.0;
            deviceOptions = new DualSenseControllerOptions(deviceType);
            synced = true;
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

        public override void Detach()
        {
            hidDevice.CloseDevice();
        }

        public void PrepareOutputReport(byte[] outReportBuffer)
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
                outReportBuffer[4] = feedbackState.RightLight;
                // Left? Low Freq Motor
                outReportBuffer[5] = feedbackState.LeftHeavy;

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
                outReportBuffer[3] = feedbackState.RightLight;
                // Left? Low Freq Motor
                outReportBuffer[4] = feedbackState.LeftHeavy;

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
    }
}

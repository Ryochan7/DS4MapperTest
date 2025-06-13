using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;

namespace DS4MapperTest.InputDevices.EightBitDoLibrary
{
    public class Ultimate2WirelessDevice : InputDeviceBase
    {
        private const int BT_INPUT_REPORT_LEN = 34;
        private const int BT_OUTPUT_REPORT_LEN = 5;

        private const int READ_STREAM_TIMEOUT = 100;

        public struct Ult2ForceFeedbackState : IEquatable<Ult2ForceFeedbackState>
        {
            public byte LeftHeavy;
            public byte RightLight;

            public void Reset()
            {
                LeftHeavy = RightLight = 0;
            }

            public bool Equals(Ult2ForceFeedbackState other)
            {
                return LeftHeavy == other.LeftHeavy && RightLight == other.RightLight;
            }

            public bool IsFeedbackActive()
            {
                return LeftHeavy != 0 || RightLight != 0;
            }
        }

        private HidDevice hidDevice;
        public HidDevice HidDevice => hidDevice;

        protected Ultimate2WirelessState currentState;
        public Ultimate2WirelessState CurrentState { get => currentState; }
        public ref Ultimate2WirelessState CurrentStateRef { get => ref currentState; }

        protected Ultimate2WirelessState previousState;
        public Ultimate2WirelessState PreviousState { get => previousState; }
        public ref Ultimate2WirelessState PreviousStateRef { get => ref previousState; }

        private Ult2ForceFeedbackState feedbackState = new Ult2ForceFeedbackState();
        public Ult2ForceFeedbackState FeedbackState
        {
            get => feedbackState;
            set => feedbackState = value;
        }
        public ref Ult2ForceFeedbackState FeedbackStateRef { get => ref feedbackState; }

        private bool rumbleDirty;
        public bool RumbleDirty
        {
            get => rumbleDirty;
            set => rumbleDirty = value;
        }

        private int inputReportLen;
        private int outputReportLen;

        public Ultimate2WirelessDevice(HidDevice device, string displayName)
        {
            this.hidDevice = device;
            this.devTypeStr = displayName;
            deviceType = InputDeviceType.EightBitDoUltimate2Wireless;
            baseElapsedReference = 125.0; // 125 Hz. 8 ms

            inputReportLen = BT_INPUT_REPORT_LEN;
            outputReportLen = BT_OUTPUT_REPORT_LEN;
            synced = true;
        }

        public int InputReportLen { get => inputReportLen; }
        public int OutputReportLen { get => outputReportLen; }

        public override void Detach()
        {
            hidDevice.CloseDevice();
        }

        public override void SetOperational()
        {
            if (!hidDevice.IsFileStreamOpen())
            {
                hidDevice.OpenFileStream(outputReportLen);
            }

            NativeMethods.HidD_SetNumInputBuffers(hidDevice.safeReadHandle.DangerousGetHandle(), 3);
        }

        public void PrepareOutputReport(byte[] outReportBuffer, bool rumble = true)
        {
            outReportBuffer[0] = 0x05;
            outReportBuffer[1] = feedbackState.LeftHeavy;
            outReportBuffer[2] = feedbackState.RightLight;
        }

        public void WriteReport(byte[] outReportBuffer)
        {
            hidDevice.WriteOutputReportViaInterrupt(outReportBuffer, READ_STREAM_TIMEOUT);
        }
    }
}

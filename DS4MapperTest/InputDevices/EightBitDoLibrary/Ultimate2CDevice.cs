using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;

namespace DS4MapperTest.InputDevices.EightBitDoLibrary
{
    public class Ultimate2CDevice : InputDeviceBase
    {
        private const int USB_INPUT_REPORT_LEN = 64;
        private const int USB_OUTPUT_REPORT_LEN = 64;

        private const int READ_STREAM_TIMEOUT = 100;
        private const byte OUTPUT_REPORT_ID = 0x01;

        public struct Ult2CForceFeedbackState : IEquatable<Ult2CForceFeedbackState>
        {
            public byte LeftHeavy;
            public byte RightLight;

            public void Reset()
            {
                LeftHeavy = RightLight = 0;
            }

            public bool Equals(Ult2CForceFeedbackState other)
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

        protected Ultimate2CState currentState;
        public Ultimate2CState CurrentState { get => currentState; }
        public ref Ultimate2CState CurrentStateRef { get => ref currentState; }

        protected Ultimate2CState previousState;
        public Ultimate2CState PreviousState { get => previousState; }
        public ref Ultimate2CState PreviousStateRef { get => ref previousState; }

        private Ult2CForceFeedbackState feedbackState = new Ult2CForceFeedbackState();
        public Ult2CForceFeedbackState FeedbackState
        {
            get => feedbackState;
            set => feedbackState = value;
        }
        public ref Ult2CForceFeedbackState FeedbackStateRef { get => ref feedbackState; }

        private bool rumbleDirty;
        public bool RumbleDirty
        {
            get => rumbleDirty;
            set => rumbleDirty = value;
        }

        private int inputReportLen;
        private int outputReportLen;

        public override event EventHandler Removal;

        public Ultimate2CDevice(HidDevice device, string displayName)
        {
            this.hidDevice = device;
            this.devTypeStr = displayName;
            deviceType = InputDeviceType.EightBitDoUltimate2C;
            baseElapsedReference = 250.0; // 250 Hz. 4 ms

            inputReportLen = USB_INPUT_REPORT_LEN;
            outputReportLen = USB_OUTPUT_REPORT_LEN;

            serial = hidDevice.GenerateFakeHwSerial();
            deviceOptions = new Ultimate2CControllerOptions(deviceType);
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

        public void PurgeRemoval()
        {
            Removal = null;
        }

        public void RaiseRemoval()
        {
            Removal?.Invoke(this, EventArgs.Empty);
        }

        public void SyncStates()
        {
            previousState = currentState;
        }

        public void PrepareOutputReport(byte[] outReportBuffer, bool rumble = true)
        {
            outReportBuffer[0] = OUTPUT_REPORT_ID;
            outReportBuffer[1] = feedbackState.LeftHeavy;
            outReportBuffer[2] = feedbackState.RightLight;
        }

        public void WriteReport(byte[] outReportBuffer)
        {
            hidDevice.WriteOutputReportViaInterrupt(outReportBuffer, READ_STREAM_TIMEOUT);
        }
    }
}

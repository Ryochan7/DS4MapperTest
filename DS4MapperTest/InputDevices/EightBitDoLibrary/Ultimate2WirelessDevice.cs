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

        private HidDevice hidDevice;
        public HidDevice HidDevice => hidDevice;

        protected Ultimate2WirelessState currentState;
        public Ultimate2WirelessState CurrentState { get => currentState; }
        public ref Ultimate2WirelessState CurrentStateRef { get => ref currentState; }

        protected Ultimate2WirelessState previousState;
        public Ultimate2WirelessState PreviousState { get => previousState; }
        public ref Ultimate2WirelessState PreviousStateRef { get => ref previousState; }

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
    }
}

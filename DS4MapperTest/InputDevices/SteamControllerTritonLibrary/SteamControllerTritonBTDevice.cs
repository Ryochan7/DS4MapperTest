using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;

namespace DS4MapperTest.InputDevices.SteamControllerTritonLibrary
{
    public class SteamControllerTritonBTDevice : SteamControllerTritonDevice
    {
        public new const int INPUT_REPORT_LEN = 20;
        public new const int OUTPUT_REPORT_LEN = 20;
        public new const int RUMBLE_REPORT_LEN = 20;
        public new const int FEATURE_REPORT_LEN = 20;

        private const byte FEATURE_REPORT_ID = 0x03;
        private const int FEATURE_REPORT_BT_PREFIX = 0xC0;
        // Windows seems to need array len to be 65 when reading feature reports.
        // Thanks to VSCView for pointing this out
        private const int READ_FEATURE_REPORT_LEN = 65;

        public override int InputReportLen { get => INPUT_REPORT_LEN; }
        public override int OutputReportLen { get => OUTPUT_REPORT_LEN; }
        public override int RumbleReportLen { get => RUMBLE_REPORT_LEN; }

        public SteamControllerTritonBTDevice(HidDevice device, string displayName) : base(device, displayName)
        {
            baseElapsedReference = 125.0;
        }

        protected override void ReadSerial()
        {
            byte[] featureData = new byte[FEATURE_REPORT_LEN];
            featureData[0] = FEATURE_REPORT_ID;
            featureData[1] = FEATURE_REPORT_BT_PREFIX;
            featureData[2] = SCPacketType.PT_GET_SERIAL;
            featureData[3] = 0x15;
            featureData[4] = 0x01;
            hidDevice.WriteFeatureReport(featureData);
            int error = Marshal.GetLastWin32Error();
            hidDevice.fileStream.Flush();

            // Sleep seems to be needed before probing for feature report data with serial.
            Thread.Sleep(200);

            byte[] retReportData = new byte[READ_FEATURE_REPORT_LEN];
            retReportData[0] = FEATURE_REPORT_ID;
            retReportData[1] = FEATURE_REPORT_BT_PREFIX;
            hidDevice.readFeatureData(retReportData);

            // Might need to try to obtain serial multiple times
            const int MAX_SERIAL_TRIES = 3;
            for (int tries = 0; tries < MAX_SERIAL_TRIES; tries++)
            {
                // Check if first byte of serial is not empty
                if (retReportData[6] != '\0')
                {
                    break;
                }
                else
                {
                    // Write to device again
                    hidDevice.WriteFeatureReport(featureData);
                    hidDevice.fileStream.Flush();

                    // Attempt to read again
                    //Thread.Sleep(200);
                    hidDevice.readFeatureData(retReportData);
                }
            }
            //Console.WriteLine("LKJDKJLLD: {0}", retReportData[1]);

            string baseS = System.Text.Encoding.Default.GetString(retReportData, 6, 12);
            string MACAddr = baseS.Replace("\0", string.Empty).ToUpper();

            if (!string.IsNullOrEmpty(MACAddr))
            {
                serial = MACAddr;
            }
        }

        protected override void ClearMappings()
        {
            byte[] featureData = new byte[FEATURE_REPORT_LEN];
            featureData[0] = FEATURE_REPORT_ID;
            featureData[1] = FEATURE_REPORT_BT_PREFIX;
            featureData[2] = SCPacketType.PT_CLEAR_MAPPINGS;
            featureData[3] = 0x01;
            hidDevice.WriteFeatureReport(featureData);
            int error = Marshal.GetLastWin32Error();
            Console.WriteLine(error);
        }

        protected override void Configure()
        {
            int timeout = 600;
            int ledLevel = 50;
            bool result = false;
            //for (int i = 20; i < 300; i++)
            {
                byte[] gyroAndTimeoutFeatureData = new byte[FEATURE_REPORT_LEN];
                gyroAndTimeoutFeatureData[0] = FEATURE_REPORT_ID;
                gyroAndTimeoutFeatureData[1] = FEATURE_REPORT_BT_PREFIX;
                gyroAndTimeoutFeatureData[2] = SCPacketType.PT_CONFIGURE;
                gyroAndTimeoutFeatureData[3] = SCPacketLength.PL_CONFIGURE_BT;
                gyroAndTimeoutFeatureData[4] = SCConfigType.CT_CONFIGURE_BT;
                //gyroAndTimeoutFeatureData[5] = 0x07;
                //gyroAndTimeoutFeatureData[6] = 0x00;
                //gyroAndTimeoutFeatureData[5] = 0; // Idle Timeout
                //gyroAndTimeoutFeatureData[6] = 0; // Idle Timeout
                //Unknown Header
                Array.Copy(new byte[] { 0x00, 0x00, 0x31, 0x02, 0x00, 0x08, 0x07, 0x00, 0x07, 0x07, 0x00, 0x30 },
                    0, gyroAndTimeoutFeatureData, 5, 12);
                gyroAndTimeoutFeatureData[17] = true ? 0x1C : 0x00; // Gyro Enable (0x1C = Enable. 0x00 = Disable)
                gyroAndTimeoutFeatureData[18] = 0x00; // Unknown
                gyroAndTimeoutFeatureData[19] = 0x2E; // Unknown
                result = hidDevice.WriteFeatureReport(gyroAndTimeoutFeatureData);
                //hidDevice.fileStream.Flush();
                //Trace.WriteLine($"{result} {21}");
                //if (result)
                //{
                //    break;
                //}
            }

            result = false;
            byte[] ledsFeatureData = new byte[FEATURE_REPORT_LEN];
            ledsFeatureData[0] = FEATURE_REPORT_ID;
            ledsFeatureData[1] = FEATURE_REPORT_BT_PREFIX;
            ledsFeatureData[2] = SCPacketType.PT_CONFIGURE;
            ledsFeatureData[3] = SCPacketLength.PL_LED;
            ledsFeatureData[4] = SCConfigType.CT_LED;
            ledsFeatureData[5] = (byte)(Math.Min(Math.Max(ledLevel, 0), 100)); // LED Level (0-100?)
            result = hidDevice.WriteFeatureReport(ledsFeatureData);
            //hidDevice.fileStream.Flush();

            //byte[] buffer = new byte[64];
            //bool result = NativeMethods.HidD_GetSerialNumberString(hidDevice.safeReadHandle.DangerousGetHandle(), buffer, 64);
            //Trace.WriteLine($"{result}");
        }

        protected override void ChangeToLizardMode()
        {
            byte[] featureData = new byte[FEATURE_REPORT_LEN];
            featureData[0] = FEATURE_REPORT_ID;
            featureData[1] = FEATURE_REPORT_BT_PREFIX;
            featureData[2] = SCPacketType.PT_LIZARD_BUTTONS;
            featureData[3] = 0x01;
            hidDevice.WriteFeatureReport(featureData);

            featureData[0] = FEATURE_REPORT_ID;
            featureData[1] = FEATURE_REPORT_BT_PREFIX;
            featureData[2] = SCPacketType.PT_LIZARD_MOUSE;
            featureData[3] = 0x00;
            hidDevice.WriteFeatureReport(featureData);
        }
    }
}

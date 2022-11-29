using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS4MapperTest.SwitchProLibrary
{
    public class RumbleTableData
    {
        public byte high;
        public ushort low;
        public int amp;

        public RumbleTableData(byte high, ushort low, int amp)
        {
            this.high = high;
            this.low = low;
            this.amp = amp;
        }
    }

    public static class SwitchProSubCmd
    {
        public const byte SET_INPUT_MODE = 0x03;
        public const byte SET_LOW_POWER_STATE = 0x08;
        public const byte SPI_FLASH_READ = 0x10;
        public const byte SET_LIGHTS = 0x30; // LEDs on controller
        public const byte SET_HOME_LIGHT = 0x38;
        public const byte ENABLE_IMU = 0x40;
        public const byte SET_IMU_SENS = 0x41;
        public const byte ENABLE_VIBRATION = 0x48;
    }

    public class SwitchProDevice : InputDeviceBase
    {
        private const int AMP_REAL_MIN = 0;
        //private const int AMP_REAL_MAX = 1003;
        private const int AMP_LIMIT_MAX = 800;

        private static RumbleTableData[] fixedRumbleTable = new RumbleTableData[]
        {
            new RumbleTableData(high: 0x00, low: 0x0040, amp: 0),
            new RumbleTableData(high: 0x02, low: 0x8040, amp: 10), new RumbleTableData(high: 0x04, low: 0x0041, amp: 12), new RumbleTableData(high: 0x06, low: 0x8041, amp: 14),
            new RumbleTableData(high: 0x08, low: 0x0042, amp: 17), new RumbleTableData(high: 0x0A, low: 0x8042, amp: 20), new RumbleTableData(high: 0x0C, low: 0x0043, amp: 24),
            new RumbleTableData(high: 0x0E, low: 0x8043, amp: 28), new RumbleTableData(high: 0x10, low: 0x0044, amp: 33), new RumbleTableData(high: 0x12, low: 0x8044, amp: 40),
            new RumbleTableData(high: 0x14, low: 0x0045, amp: 47), new RumbleTableData(high: 0x16, low: 0x8045, amp: 56), new RumbleTableData(high: 0x18, low: 0x0046, amp: 67),
            new RumbleTableData(high: 0x1A, low: 0x8046, amp: 80), new RumbleTableData(high: 0x1C, low: 0x0047, amp: 95), new RumbleTableData(high: 0x1E, low: 0x8047, amp: 112),
            new RumbleTableData(high: 0x20, low: 0x0048, amp: 117), new RumbleTableData(high: 0x22, low: 0x8048, amp: 123), new RumbleTableData(high: 0x24, low: 0x0049, amp: 128),
            new RumbleTableData(high: 0x26, low: 0x8049, amp: 134), new RumbleTableData(high: 0x28, low: 0x004A, amp: 140), new RumbleTableData(high: 0x2A, low: 0x804A, amp: 146),
            new RumbleTableData(high: 0x2C, low: 0x004B, amp: 152), new RumbleTableData(high: 0x2E, low: 0x804B, amp: 159), new RumbleTableData(high: 0x30, low: 0x004C, amp: 166),
            new RumbleTableData(high: 0x32, low: 0x804C, amp: 173), new RumbleTableData(high: 0x34, low: 0x004D, amp: 181), new RumbleTableData(high: 0x36, low: 0x804D, amp: 189),
            new RumbleTableData(high: 0x38, low: 0x004E, amp: 198), new RumbleTableData(high: 0x3A, low: 0x804E, amp: 206), new RumbleTableData(high: 0x3C, low: 0x004F, amp: 215),
            new RumbleTableData(high: 0x3E, low: 0x804F, amp: 225), new RumbleTableData(high: 0x40, low: 0x0050, amp: 230), new RumbleTableData(high: 0x42, low: 0x8050, amp: 235),
            new RumbleTableData(high: 0x44, low: 0x0051, amp: 240), new RumbleTableData(high: 0x46, low: 0x8051, amp: 245), new RumbleTableData(high: 0x48, low: 0x0052, amp: 251),
            new RumbleTableData(high: 0x4A, low: 0x8052, amp: 256), new RumbleTableData(high: 0x4C, low: 0x0053, amp: 262), new RumbleTableData(high: 0x4E, low: 0x8053, amp: 268),
            new RumbleTableData(high: 0x50, low: 0x0054, amp: 273), new RumbleTableData(high: 0x52, low: 0x8054, amp: 279), new RumbleTableData(high: 0x54, low: 0x0055, amp: 286),
            new RumbleTableData(high: 0x56, low: 0x8055, amp: 292), new RumbleTableData(high: 0x58, low: 0x0056, amp: 298), new RumbleTableData(high: 0x5A, low: 0x8056, amp: 305),
            new RumbleTableData(high: 0x5C, low: 0x0057, amp: 311), new RumbleTableData(high: 0x5E, low: 0x8057, amp: 318), new RumbleTableData(high: 0x60, low: 0x0058, amp: 325),
            new RumbleTableData(high: 0x62, low: 0x8058, amp: 332), new RumbleTableData(high: 0x64, low: 0x0059, amp: 340), new RumbleTableData(high: 0x66, low: 0x8059, amp: 347),
            new RumbleTableData(high: 0x68, low: 0x005A, amp: 355), new RumbleTableData(high: 0x6A, low: 0x805A, amp: 362), new RumbleTableData(high: 0x6C, low: 0x005B, amp: 370),
            new RumbleTableData(high: 0x6E, low: 0x805B, amp: 378), new RumbleTableData(high: 0x70, low: 0x005C, amp: 387), new RumbleTableData(high: 0x72, low: 0x805C, amp: 395),
            new RumbleTableData(high: 0x74, low: 0x005D, amp: 404), new RumbleTableData(high: 0x76, low: 0x805D, amp: 413), new RumbleTableData(high: 0x78, low: 0x005E, amp: 422),
            new RumbleTableData(high: 0x7A, low: 0x805E, amp: 431), new RumbleTableData(high: 0x7C, low: 0x005F, amp: 440), new RumbleTableData(high: 0x7E, low: 0x805F, amp: 450),
            new RumbleTableData(high: 0x80, low: 0x0060, amp: 460), new RumbleTableData(high: 0x82, low: 0x8060, amp: 470), new RumbleTableData(high: 0x84, low: 0x0061, amp: 480),
            new RumbleTableData(high: 0x86, low: 0x8061, amp: 491), new RumbleTableData(high: 0x88, low: 0x0062, amp: 501), new RumbleTableData(high: 0x8A, low: 0x8062, amp: 512),
            new RumbleTableData(high: 0x8C, low: 0x0063, amp: 524), new RumbleTableData(high: 0x8E, low: 0x8063, amp: 535), new RumbleTableData(high: 0x90, low: 0x0064, amp: 547),
            new RumbleTableData(high: 0x92, low: 0x8064, amp: 559), new RumbleTableData(high: 0x94, low: 0x0065, amp: 571), new RumbleTableData(high: 0x96, low: 0x8065, amp: 584),
            new RumbleTableData(high: 0x98, low: 0x0066, amp: 596), new RumbleTableData(high: 0x9A, low: 0x8066, amp: 609), new RumbleTableData(high: 0x9C, low: 0x0067, amp: 623),
            new RumbleTableData(high: 0x9E, low: 0x8067, amp: 636), new RumbleTableData(high: 0xA0, low: 0x0068, amp: 650), new RumbleTableData(high: 0xA2, low: 0x8068, amp: 665),
            new RumbleTableData(high: 0xA4, low: 0x0069, amp: 679), new RumbleTableData(high: 0xA6, low: 0x8069, amp: 694), new RumbleTableData(high: 0xA8, low: 0x006A, amp: 709),
            new RumbleTableData(high: 0xAA, low: 0x806A, amp: 725), new RumbleTableData(high: 0xAC, low: 0x006B, amp: 741), new RumbleTableData(high: 0xAE, low: 0x806B, amp: 757),
            new RumbleTableData(high: 0xB0, low: 0x006C, amp: 773), new RumbleTableData(high: 0xB2, low: 0x806C, amp: 790), new RumbleTableData(high: 0xB4, low: 0x006D, amp: 808),
            new RumbleTableData(high: 0xB6, low: 0x806D, amp: 825), new RumbleTableData(high: 0xB8, low: 0x006E, amp: 843), new RumbleTableData(high: 0xBA, low: 0x806E, amp: 862),
            new RumbleTableData(high: 0xBC, low: 0x006F, amp: 881), new RumbleTableData(high: 0xBE, low: 0x806F, amp: 900), new RumbleTableData(high: 0xC0, low: 0x0070, amp: 920),
            new RumbleTableData(high: 0xC2, low: 0x8070, amp: 940), new RumbleTableData(high: 0xC4, low: 0x0071, amp: 960), new RumbleTableData(high: 0xC6, low: 0x8071, amp: 981),
            new RumbleTableData(high: 0xC8, low: 0x0072, amp: 1003),
        };

        private static RumbleTableData[] compiledRumbleTable = new Func<RumbleTableData[]>(() =>
        {
            RumbleTableData[] tmpBuffer = new RumbleTableData[fixedRumbleTable.Last().amp + 1];
            int currentOffset = 0;
            RumbleTableData previousEntry = fixedRumbleTable[0];
            tmpBuffer[currentOffset] = previousEntry;
            int currentAmp = previousEntry.amp + 1;
            currentOffset++;

            for (int i = 1; i < fixedRumbleTable.Length; i++)
            //foreach(RumbleTableData entry in fixedRumbleTable)
            {
                RumbleTableData entry = fixedRumbleTable[i];
                if (currentAmp < entry.amp)
                {
                    while (currentAmp < entry.amp)
                    {
                        tmpBuffer[currentOffset] = previousEntry;
                        currentOffset++;
                        currentAmp++;
                    }
                }

                tmpBuffer[currentOffset] = entry;
                currentAmp = entry.amp + 1;
                currentOffset++;
                previousEntry = entry;
            }

            //fixedRumbleTable = null;
            return tmpBuffer;
        })();

        public struct StickAxisData
        {
            public ushort max;
            public ushort mid;
            public ushort min;
        };

        private static byte[] commandBuffHeader =
            { 0x0, 0x1, 0x40, 0x40, 0x0, 0x1, 0x40, 0x40 };

        private const int SUBCOMMAND_HEADER_LEN = 8;
        private const int SUBCOMMAND_BUFFER_LEN = 64;
        private const int SUBCOMMAND_RESPONSE_TIMEOUT = 500;
        public const int IMU_XAXIS_IDX = 0, IMU_YAW_IDX = 0;
        public const int IMU_YAXIS_IDX = 1, IMU_PITCH_IDX = 1;
        public const int IMU_ZAXIS_IDX = 2, IMU_ROLL_IDX = 2;

        public const short ACCEL_ORIG_HOR_OFFSET_X = -688;
        public const short ACCEL_ORIG_HOR_OFFSET_Y = 0;
        public const short ACCEL_ORIG_HOR_OFFSET_Z = 4038;

        //private const ushort SAMPLE_STICK_MAX = 3200;
        private const ushort SAMPLE_STICK_MAX = 3300;
        private const ushort SAMPLE_STICK_MIN = 500;
        private const ushort SAMPLE_STICK_MID = SAMPLE_STICK_MAX - SAMPLE_STICK_MIN;
        private const double STICK_AXIS_MAX_CUTOFF = 0.96;
        private const double STICK_AXIS_MIN_CUTOFF = 1.04;

        public StickAxisData leftStickXData;
        public StickAxisData leftStickYData;
        public StickAxisData rightStickXData;
        public StickAxisData rightStickYData;

        //private static Guid BLUETOOTH_HID_GUID = new Guid("{00001124-0000-1000-8000-00805F9B34FB}");
        private const string BLUETOOTH_HID_GUID = "{00001124-0000-1000-8000-00805F9B34FB}";

        public enum ConnectionType : uint
        {
            USB,
            Bluetooth,
        }

        private HidDevice hidDevice;
        public HidDevice HidDevice => hidDevice;
        private string macAddress;
        public string MacAddress => macAddress;

        private ConnectionType connectionType;
        public ConnectionType DevConnectionType => connectionType;

        private SwitchProState currentState = new SwitchProState();
        private SwitchProState previousState = new SwitchProState();
        public SwitchProState CurrentState
        {
            get => currentState; set => currentState = value;
        }

        public ref SwitchProState ClothOff
        {
            get => ref currentState;
        }

        public SwitchProState PreviousState
        {
            get => previousState; set => previousState = value;
        }

        public ref SwitchProState ClothOff2
        {
            get => ref previousState;
        }

        private bool modeChangeDone;
        public bool ModeChangeDone { get => modeChangeDone; }


        private byte frameCount = 0;
        public byte FrameCount { get => frameCount; set => frameCount = value; }

        public const int INPUT_REPORT_LEN = 362;
        public const int OUTPUT_REPORT_LEN = 49;
        public const int RUMBLE_REPORT_LEN = 64;

        // Converts raw gyro input value to dps. Equal to (4588/65535)
        private const float GYRO_IN_DEG_SEC_FACTOR = 0.070f;
        private const double USB_ELAPSED_REFERENCE = 133.6;
        private const double BT_ELAPSED_REFERENCE = 66.7;

        public int InputReportLen { get => INPUT_REPORT_LEN; }
        public int OutputReportLen { get => OUTPUT_REPORT_LEN; }
        public int RumbleReportLen { get => RUMBLE_REPORT_LEN; }

        private ushort[] leftStickCalib = new ushort[6];
        private ushort[] rightStickCalib = new ushort[6];

        public ushort[] LeftStickCalib { get => leftStickCalib; }
        public ushort[] RightStickCalib { get => rightStickCalib; }

        public ushort deadzoneLS;
        public ushort deadzoneRS;

        public short[] accelNeutral = new short[3];
        public short[] accelSens = new short[3];
        public double[] accelSensMulti = new double[3];
        public double[] accelCoeff = new double[3];

        public short[] gyroBias = new short[3];
        public short[] gyroSens = new short[3];
        public short[] gyroCalibOffsets = new short[3];
        public double[] gyroSensMulti = new double[3];
        public double[] gyroCoeff = new double[3];

        public SwitchProDevice(HidDevice hidDevice, string displayName)
        {
            this.hidDevice = hidDevice;
            this.devTypeStr = displayName;
            deviceType = InputDeviceType.SwitchPro;
            macAddress = hidDevice.ReadSerial();
            serial = macAddress;
            deviceOptions = new SwitchProControllerOptions(InputDeviceType.SwitchPro);

            DetermineConnectionType();

            leftStickXData.max = SAMPLE_STICK_MAX; leftStickXData.min = SAMPLE_STICK_MIN;
            leftStickXData.mid = SAMPLE_STICK_MID;

            leftStickYData.max = SAMPLE_STICK_MAX; leftStickYData.min = SAMPLE_STICK_MIN;
            leftStickYData.mid = SAMPLE_STICK_MID;

            rightStickXData.max = SAMPLE_STICK_MAX; rightStickXData.min = SAMPLE_STICK_MIN;
            rightStickXData.mid = SAMPLE_STICK_MID;

            rightStickYData.max = SAMPLE_STICK_MAX; rightStickYData.min = SAMPLE_STICK_MIN;
            rightStickYData.mid = SAMPLE_STICK_MID;
            //Console.WriteLine(compiledRumbleTabe[0]);
            //CalibrationData();

            if (connectionType == ConnectionType.USB)
            {
                baseElapsedReference = BT_ELAPSED_REFERENCE;
            }
            else
            {
                baseElapsedReference = BT_ELAPSED_REFERENCE;
            }
        }

        private void DetermineConnectionType()
        {
            if (hidDevice.DevicePath.ToUpper().Contains(BLUETOOTH_HID_GUID))
            {
                connectionType = ConnectionType.Bluetooth;
            }
            else
            {
                connectionType = ConnectionType.USB;
            }
        }

        public override void SetOperational()
        {
            if (modeChangeDone)
            {
                return;
            }

            if (!hidDevice.IsFileStreamOpen())
            {
                hidDevice.OpenFileStream(InputReportLen);
            }

            if (connectionType == ConnectionType.USB)
            {
                RunUSBSetup();
                Thread.Sleep(500);
            }

            //Thread.Sleep(1000);

            //EnableFastPollRate();

            //Thread.Sleep(5000);
            //byte[] tmpReport = new byte[INPUT_REPORT_LEN];
            //byte[] command2 = new byte[SUBCOMMAND_BUFFER_LEN];
            //bool result;
            //HidDevice.ReadStatus res;

            // Set device to normal power state
            byte[] powerChoiceArray = new byte[] { 0x00 };
            Subcommand(SwitchProSubCmd.SET_LOW_POWER_STATE, powerChoiceArray, 1, checkResponse: true);

            // Turn on Home light (Solid)
            byte[] light = Enumerable.Repeat((byte)0xFF, 25).ToArray();
            light[0] = 0x1F; light[1] = 0xF0;
            //Thread.Sleep(1000);
            Subcommand(0x38, light, 25, checkResponse: true);

            // Turn on bottom LEDs
            byte[] leds = new byte[] { 0x01 };
            //Thread.Sleep(1000);
            Subcommand(0x30, leds, 1, checkResponse: true);

            // Enable Gyro
            byte[] imuEnable = new byte[] { 0x01 };
            //Thread.Sleep(1000);
            Subcommand(0x40, imuEnable, 1, checkResponse: true);

            // Enable High Performance Gyro mode
            //byte[] gyroModeBuffer = new byte[] { 0x03, 0x00, 0x00, 0x01 };
            byte[] gyroModeBuffer = new byte[] { 0x03, 0x00, 0x00, 0x00 };
            //Thread.Sleep(1000);
            Subcommand(0x41, gyroModeBuffer, 4, checkResponse: true);

            // Enable Rumble
            byte[] rumbleEnable = new byte[] { 0x01 };
            //Thread.Sleep(1000);
            Subcommand(0x48, rumbleEnable, 1, checkResponse: true);

            //Thread.Sleep(1000);
            EnableFastPollRate();

            // USB Connections seem to need a delay after switching input modes
            if (connectionType == ConnectionType.USB)
            {
                Thread.Sleep(1000);
            }

            SetInitRumble();
            CalibrationData();

            modeChangeDone = true;

            Console.WriteLine("FINISHED");

            //if (connectionType == ConnectionType.USB)
            //{
            //    Thread.Sleep(300);
            //    //SetInitRumble();
            //}
        }

        private void RunUSBSetup()
        {
            bool result;
            //byte[] tmpReport = new byte[INPUT_REPORT_LEN];

            byte[] modeSwitchCommand = new byte[] { 0x3F };
            Subcommand(0x03, modeSwitchCommand, 1, checkResponse: true);

            byte[] data = new byte[64];
            data[0] = 0x80; data[1] = 0x01;
            //result = hidDevice.WriteAsyncOutputReportViaInterrupt(data);
            result = hidDevice.WriteOutputReportViaInterrupt(data, 0);
            //Array.Clear(tmpReport, 0 , 64);
            //res = hidDevice.ReadWithFileStream(tmpReport);
            //Console.WriteLine("TEST BYTE: {0}", tmpReport[2]);

            data[0] = 0x80; data[1] = 0x02; // USB Pairing
            //result = hidDevice.WriteOutputReportViaControl(data);
            //Thread.Sleep(2000);
            //Thread.Sleep(1000);
            result = hidDevice.WriteOutputReportViaControl(data);

            data[0] = 0x80; data[1] = 0x03; // 3Mbit baud rate
            //result = hidDevice.WriteAsyncOutputReportViaInterrupt(data);
            result = hidDevice.WriteOutputReportViaControl(data);
            //Thread.Sleep(2000);

            data[0] = 0x80; data[1] = 0x02; // Handshake at new baud rate
            result = hidDevice.WriteOutputReportViaControl(data);
            //Thread.Sleep(1000);
            //result = hidDevice.WriteOutputReportViaInterrupt(command, 500);
            //Thread.Sleep(2000);

            data[0] = 0x80; data[1] = 0x4; // Prevent HID timeout
            result = hidDevice.WriteOutputReportViaControl(data);
            hidDevice.fileStream.Flush();
            //result = hidDevice.WriteOutputReportViaInterrupt(command, 500);
        }

        // Deprecated method. Leave a stub for now
        private void RunBluetoothSetup()
        {
        }

        private void EnableFastPollRate()
        {
            // Enable fatest poll rate
            byte[] tempArray = new byte[] { 0x30 };
            Subcommand(0x03, tempArray, 1, checkResponse: true);
            //Thread.Sleep(1000);
        }

        public void SetInitRumble()
        {
            bool result;
            HidDevice.ReadStatus res;
            //byte[] tmpReport = new byte[64];
            byte[] rumble_data = new byte[8];
            rumble_data[0] = 0x0;
            rumble_data[1] = 0x1;
            rumble_data[2] = 0x40;
            rumble_data[3] = 0x40;

            for (int i = 0; i < 4; i++)
            {
                rumble_data[4 + i] = rumble_data[i];
            }

            byte[] tmpRumble = new byte[RUMBLE_REPORT_LEN];
            Array.Copy(rumble_data, 0, tmpRumble, 2, rumble_data.Length);
            tmpRumble[0] = 0x10;
            tmpRumble[1] = frameCount;
            frameCount = (byte)(++frameCount & 0x0F);

            result = hidDevice.WriteOutputReportViaInterrupt(tmpRumble, 0);
            hidDevice.fileStream.Flush();
            //res = hidDevice.ReadWithFileStream(tmpReport, 500);
            //res = hidDevice.ReadFile(tmpReport);
        }

        public byte[] Subcommand(byte subcommand, byte[] tmpBuffer, uint bufLen,
            bool checkResponse = false)
        {
            bool result;
            byte[] commandBuffer = new byte[SUBCOMMAND_BUFFER_LEN];
            Array.Copy(commandBuffHeader, 0, commandBuffer, 2, SUBCOMMAND_HEADER_LEN);
            Array.Copy(tmpBuffer, 0, commandBuffer, 11, bufLen);

            commandBuffer[0] = 0x01;
            commandBuffer[1] = frameCount;
            frameCount = (byte)(++frameCount & 0x0F);
            commandBuffer[10] = subcommand;

            result = hidDevice.WriteOutputReportViaInterrupt(commandBuffer, 0);
            hidDevice.fileStream.Flush();

            byte[] tmpReport = null;
            if (checkResponse)
            {
                tmpReport = new byte[INPUT_REPORT_LEN];
                HidDevice.ReadStatus res;
                res = hidDevice.ReadWithFileStream(tmpReport, SUBCOMMAND_RESPONSE_TIMEOUT);
                int tries = 1;
                while (res == HidDevice.ReadStatus.Success &&
                    tmpReport[0] != 0x21 && tmpReport[14] != subcommand && tries < 100)
                {
                    //Console.WriteLine("TRY AGAIN: {0}", tmpReport[0]);
                    res = hidDevice.ReadWithFileStream(tmpReport, SUBCOMMAND_RESPONSE_TIMEOUT);
                    tries++;
                }

                //Console.WriteLine("END GAME: {0} {1}", tmpReport[0], tries);
            }

            return tmpReport;
        }

        public double currentLeftAmpRatio;
        public double currentRightAmpRatio;

        public void PrepareRumbleData(byte[] buffer)
        {
            //Array.Copy(commandBuffHeader, 0, buffer, 2, SUBCOMMAND_HEADER_LEN);
            buffer[0] = 0x10;
            buffer[1] = frameCount;
            frameCount = (byte)(++frameCount & 0x0F);

            ushort freq_data_high = 0x0001; // 320
            byte freq_data_low = 0x40; // 160
            int idx = (int)(currentLeftAmpRatio * AMP_LIMIT_MAX);
            RumbleTableData entry = compiledRumbleTable[idx];
            byte amp_high = entry.high;
            ushort amp_low = entry.low;
            //byte amp_high = 0x9a; // 609
            //ushort amp_low = 0x8066; // 609
            //buffer[2] = 0x28; // 0
            //buffer[3] = 0xc8; // 1
            //buffer[4] = 0x81; // 2
            //buffer[5] = 0x71; // 3

            //buffer[6] = 0x28; // 4
            //buffer[7] = 0xc8; // 5
            //buffer[8] = 0x81; // 6
            //buffer[9] = 0x71; // 7

            buffer[2] = (byte)((freq_data_high >> 8) & 0xFF); // 0
            buffer[3] = (byte)((freq_data_high & 0xFF) + amp_high); // 1
            buffer[4] = (byte)(freq_data_low + (amp_low >> 8) & 0xFF); // 2
            buffer[5] = (byte)(amp_low & 0xFF); // 3

            idx = (int)(currentRightAmpRatio * AMP_LIMIT_MAX);
            entry = compiledRumbleTable[idx];
            amp_high = entry.high;
            amp_low = entry.low;
            buffer[6] = (byte)((freq_data_high >> 8) & 0xFF); // 4
            buffer[7] = (byte)((freq_data_high & 0xFF) + amp_high); // 5
            buffer[8] = (byte)(freq_data_low + (amp_low >> 8) & 0xFF); // 6
            buffer[9] = (byte)(amp_low & 0xFF); // 7
            //Console.WriteLine("RUMBLE BUFF: {0}", string.Join(", ", buffer));
            //Console.WriteLine("RUMBLE BUFF: {0}",
            //    string.Concat(buffer.Select(i => string.Format("{0:x2} ", i))));
        }

        public void CalibrationData()
        {
            const int SPI_RESP_OFFSET = 20;
            byte[] command;
            byte[] tmpBuffer;

            //command = new byte[] { 0x00, 0x50, 0x00, 0x00, 0x01 };
            //tmpBuffer = Subcommand(SwitchProSubCmd.SPI_FLASH_READ, command, 5, checkResponse: true);
            //Console.WriteLine("THE POWER");
            //Console.WriteLine(string.Join(",", tmpBuffer));
            //Console.WriteLine(tmpBuffer[SPI_RESP_OFFSET]);
            //Console.WriteLine();

            bool foundUserCalib = false;
            command = new byte[] { 0x10, 0x80, 0x00, 0x00, 0x02 };
            tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
            if (tmpBuffer[SPI_RESP_OFFSET] == 0xB2 && tmpBuffer[SPI_RESP_OFFSET + 1] == 0xA1)
            {
                foundUserCalib = true;
            }

            if (foundUserCalib)
            {
                command = new byte[] { 0x12, 0x80, 0x00, 0x00, 0x09 };
                tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
                //Console.WriteLine("FOUND USER CALIB");
            }
            else
            {
                command = new byte[] { 0x3D, 0x60, 0x00, 0x00, 0x09 };
                tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
                //Console.WriteLine("CHECK FACTORY CALIB");
            }

            leftStickCalib[0] = (ushort)(((tmpBuffer[1 + SPI_RESP_OFFSET] << 8) & 0xF00) | tmpBuffer[0 + SPI_RESP_OFFSET]); // X Axis Max above center
            leftStickCalib[1] = (ushort)((tmpBuffer[2 + SPI_RESP_OFFSET] << 4) | (tmpBuffer[1 + SPI_RESP_OFFSET] >> 4)); // Y Axis Max above center
            leftStickCalib[2] = (ushort)(((tmpBuffer[4 + SPI_RESP_OFFSET] << 8) & 0xF00) | tmpBuffer[3 + SPI_RESP_OFFSET]); // X Axis Center
            leftStickCalib[3] = (ushort)((tmpBuffer[5 + SPI_RESP_OFFSET] << 4) | (tmpBuffer[4 + SPI_RESP_OFFSET] >> 4)); // Y Axis Center
            leftStickCalib[4] = (ushort)(((tmpBuffer[7 + SPI_RESP_OFFSET] << 8) & 0xF00) | tmpBuffer[6 + SPI_RESP_OFFSET]); // X Axis Min below center
            leftStickCalib[5] = (ushort)((tmpBuffer[8 + SPI_RESP_OFFSET] << 4) | (tmpBuffer[7 + SPI_RESP_OFFSET] >> 4)); // Y Axis Min below center

            if (foundUserCalib)
            {
                leftStickXData.max = (ushort)(leftStickCalib[0] + leftStickCalib[2]);
                leftStickXData.mid = leftStickCalib[2];
                leftStickXData.min = (ushort)(leftStickCalib[2] - leftStickCalib[4]);

                leftStickYData.max = (ushort)(leftStickCalib[1] + leftStickCalib[3]);
                leftStickYData.mid = leftStickCalib[3];
                leftStickYData.min = (ushort)(leftStickCalib[3] - leftStickCalib[5]);
            }
            else
            {
                leftStickXData.max = (ushort)((leftStickCalib[0] + leftStickCalib[2]) * STICK_AXIS_MAX_CUTOFF);
                leftStickXData.min = (ushort)((leftStickCalib[2] - leftStickCalib[4]) * STICK_AXIS_MIN_CUTOFF);
                //leftStickXData.mid = leftStickCalib[2];
                leftStickXData.mid = (ushort)((leftStickXData.max - leftStickXData.min) / 2.0 + leftStickXData.min);

                leftStickYData.max = (ushort)((leftStickCalib[1] + leftStickCalib[3]) * STICK_AXIS_MAX_CUTOFF);
                leftStickYData.min = (ushort)((leftStickCalib[3] - leftStickCalib[5]) * STICK_AXIS_MIN_CUTOFF);
                //leftStickYData.mid = leftStickCalib[3];
                leftStickYData.mid = (ushort)((leftStickYData.max - leftStickYData.min) / 2.0 + leftStickYData.min);
                //leftStickOffsetX = leftStickOffsetY = 140;
            }

            //Console.WriteLine(string.Join(",", tmpBuffer));
            //Console.WriteLine();
            //Console.WriteLine(string.Join(",", leftStickCalib));

            foundUserCalib = false;
            command = new byte[] { 0x1B, 0x80, 0x00, 0x00, 0x02 };
            tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
            if (tmpBuffer[SPI_RESP_OFFSET] == 0xB2 && tmpBuffer[SPI_RESP_OFFSET + 1] == 0xA1)
            {
                foundUserCalib = true;
            }

            if (foundUserCalib)
            {
                command = new byte[] { 0x1D, 0x80, 0x00, 0x00, 0x09 };
                tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
                //Console.WriteLine("FOUND RIGHT USER CALIB");
            }
            else
            {
                command = new byte[] { 0x46, 0x60, 0x00, 0x00, 0x09 };
                tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
                //Console.WriteLine("CHECK RIGHT FACTORY CALIB");
            }

            rightStickCalib[2] = (ushort)(((tmpBuffer[1 + SPI_RESP_OFFSET] << 8) & 0xF00) | tmpBuffer[0 + SPI_RESP_OFFSET]); // X Axis Center
            rightStickCalib[3] = (ushort)((tmpBuffer[2 + SPI_RESP_OFFSET] << 4) | (tmpBuffer[1 + SPI_RESP_OFFSET] >> 4)); // Y Axis Center
            rightStickCalib[4] = (ushort)(((tmpBuffer[4 + SPI_RESP_OFFSET] << 8) & 0xF00) | tmpBuffer[3 + SPI_RESP_OFFSET]); // X Axis Min below center
            rightStickCalib[5] = (ushort)((tmpBuffer[5 + SPI_RESP_OFFSET] << 4) | (tmpBuffer[4 + SPI_RESP_OFFSET] >> 4)); // Y Axis Min below center
            rightStickCalib[0] = (ushort)(((tmpBuffer[7 + SPI_RESP_OFFSET] << 8) & 0xF00) | tmpBuffer[6 + SPI_RESP_OFFSET]); // X Axis Max above center
            rightStickCalib[1] = (ushort)((tmpBuffer[8 + SPI_RESP_OFFSET] << 4) | (tmpBuffer[7 + SPI_RESP_OFFSET] >> 4)); // Y Axis Max above center

            if (foundUserCalib)
            {
                rightStickXData.max = (ushort)(rightStickCalib[2] + rightStickCalib[4]);
                rightStickXData.mid = rightStickCalib[2];
                rightStickXData.min = (ushort)(rightStickCalib[2] - rightStickCalib[0]);

                rightStickYData.max = (ushort)(rightStickCalib[3] + rightStickCalib[5]);
                rightStickYData.mid = rightStickCalib[3];
                rightStickYData.min = (ushort)(rightStickCalib[3] - rightStickCalib[1]);
            }
            else
            {
                rightStickXData.max = (ushort)((rightStickCalib[2] + rightStickCalib[0]) * STICK_AXIS_MAX_CUTOFF);
                rightStickXData.min = (ushort)((rightStickCalib[2] - rightStickCalib[4]) * STICK_AXIS_MIN_CUTOFF);
                //rightStickXData.mid = rightStickCalib[2];
                rightStickXData.mid = (ushort)((rightStickXData.max - rightStickXData.min) / 2.0 + rightStickXData.min);

                rightStickYData.max = (ushort)((rightStickCalib[3] + rightStickCalib[1]) * STICK_AXIS_MAX_CUTOFF);
                rightStickYData.min = (ushort)((rightStickCalib[3] - rightStickCalib[5]) * STICK_AXIS_MIN_CUTOFF);
                //rightStickYData.mid = rightStickCalib[3];
                rightStickYData.mid = (ushort)((rightStickYData.max - rightStickYData.min) / 2.0 + rightStickYData.min);
                //rightStickOffsetX = rightStickOffsetY = 140;
            }

            //Console.WriteLine(string.Join(",", tmpBuffer));
            //Console.WriteLine();
            //Console.WriteLine(string.Join(",", rightStickCalib));

            /*
            // Grab Factory LS Dead Zone
            command = new byte[] { 0x86, 0x60, 0x00, 0x00, 0x10 };
            byte[] deadZoneBuffer = Subcommand(0x10, command, 5, checkResponse: true);
            deadzoneLS = (ushort)((deadZoneBuffer[4 + SPI_RESP_OFFSET] << 8) & 0xF00 | deadZoneBuffer[3 + SPI_RESP_OFFSET]);
            //Console.WriteLine("DZ Left: {0}", deadzoneLS);
            //Console.WriteLine(string.Join(",", deadZoneBuffer));

            // Grab Factory RS Dead Zone
            command = new byte[] { 0x98, 0x60, 0x00, 0x00, 0x10 };
            deadZoneBuffer = Subcommand(0x10, command, 5, checkResponse: true);
            deadzoneRS = (ushort)((deadZoneBuffer[4 + SPI_RESP_OFFSET] << 8) & 0xF00 | deadZoneBuffer[3 + SPI_RESP_OFFSET]);
            //Console.WriteLine("DZ Right: {0}", deadzoneRS);
            //Console.WriteLine(string.Join(",", deadZoneBuffer));*/

            foundUserCalib = false;
            command = new byte[] { 0x26, 0x80, 0x00, 0x00, 0x02 };
            tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
            if (tmpBuffer[SPI_RESP_OFFSET] == 0xB2 && tmpBuffer[SPI_RESP_OFFSET + 1] == 0xA1)
            {
                foundUserCalib = true;
            }

            //Console.WriteLine("{0}", string.Join(",", tmpBuffer.Skip(offset).ToArray()));
            if (foundUserCalib)
            {
                command = new byte[] { 0x28, 0x80, 0x00, 0x00, 0x18 };
                tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
                //Console.WriteLine("FOUND USER CALIB");
            }
            else
            {
                command = new byte[] { 0x20, 0x60, 0x00, 0x00, 0x18 };
                tmpBuffer = Subcommand(0x10, command, 5, checkResponse: true);
                //Console.WriteLine("CHECK FACTORY CALIB");
            }

            accelNeutral[IMU_XAXIS_IDX] = (short)((tmpBuffer[3 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[2 + SPI_RESP_OFFSET]); // Accel X Offset
            accelNeutral[IMU_YAXIS_IDX] = (short)((tmpBuffer[1 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[0 + SPI_RESP_OFFSET]); // Accel Y Offset
            accelNeutral[IMU_ZAXIS_IDX] = (short)((tmpBuffer[5 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[4 + SPI_RESP_OFFSET]); // Accel Z Offset
            //Console.WriteLine("ACCEL NEUTRAL: {0}", string.Join(",", accelNeutral));
            //Console.WriteLine("{0}", string.Join(",", tmpBuffer.Skip(offset).ToArray()));

            accelSens[IMU_XAXIS_IDX] = (short)((tmpBuffer[9 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[8 + SPI_RESP_OFFSET]); // Accel X Sens
            accelSens[IMU_YAXIS_IDX] = (short)((tmpBuffer[7 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[6 + SPI_RESP_OFFSET]); // Accel Y Sens
            accelSens[IMU_ZAXIS_IDX] = (short)((tmpBuffer[11 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[10 + SPI_RESP_OFFSET]); // Accel Z Sens
            //Console.WriteLine("ACCEL SENS: {0}", string.Join(",", accelSens));
            //Console.WriteLine("{0}", string.Join(",", tmpBuffer.Skip(SPI_RESP_OFFSET).ToArray()));

            accelCoeff[IMU_XAXIS_IDX] = 1.0 / (accelSens[IMU_XAXIS_IDX] - accelNeutral[IMU_XAXIS_IDX]) * 4.0;
            accelCoeff[IMU_YAXIS_IDX] = 1.0 / (accelSens[IMU_YAXIS_IDX] - accelNeutral[IMU_YAXIS_IDX]) * 4.0;
            accelCoeff[IMU_ZAXIS_IDX] = 1.0 / (accelSens[IMU_ZAXIS_IDX] - accelNeutral[IMU_ZAXIS_IDX]) * 4.0;
            //accelCoeff[IMU_XAXIS_IDX] = (accelSens[IMU_XAXIS_IDX] - accelNeutral[IMU_XAXIS_IDX]) / 65535.0 / 1000.0;
            //accelCoeff[IMU_YAXIS_IDX] = (accelSens[IMU_YAXIS_IDX] - accelNeutral[IMU_YAXIS_IDX]) / 65535.0 / 1000.0;
            //accelCoeff[IMU_ZAXIS_IDX] = (accelSens[IMU_ZAXIS_IDX] - accelNeutral[IMU_ZAXIS_IDX]) / 65535.0 / 1000.0;
            //Console.WriteLine("ACCEL COEFF: {0}", string.Join(",", accelCoeff));

            //accelSensMulti[IMU_XAXIS_IDX] = accelSens[IMU_XAXIS_IDX] / (2 * 8192.0);
            //accelSensMulti[IMU_YAXIS_IDX] = accelSens[IMU_YAXIS_IDX] / (2 * 8192.0);
            //accelSensMulti[IMU_ZAXIS_IDX] = accelSens[IMU_ZAXIS_IDX] / (2 * 8192.0);

            gyroBias[0] = (short)((tmpBuffer[17 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[16 + SPI_RESP_OFFSET]); // Gyro Yaw Offset
            gyroBias[1] = (short)((tmpBuffer[15 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[14 + SPI_RESP_OFFSET]); // Gyro Pitch Offset
            gyroBias[2] = (short)((tmpBuffer[13 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[12 + SPI_RESP_OFFSET]); // Gyro Roll Offset

            gyroSens[IMU_YAW_IDX] = (short)((tmpBuffer[23 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[22 + SPI_RESP_OFFSET]); // Gyro Yaw Sens
            gyroSens[IMU_PITCH_IDX] = (short)((tmpBuffer[21 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[20 + SPI_RESP_OFFSET]); // Gyro Pitch Sens
            gyroSens[IMU_ROLL_IDX] = (short)((tmpBuffer[19 + SPI_RESP_OFFSET] << 8) & 0xFF00 | tmpBuffer[18 + SPI_RESP_OFFSET]); // Gyro Roll Sens

            //Console.WriteLine("GYRO BIAS: {0}", string.Join(",", gyroBias));
            //Console.WriteLine("GYRO SENS: {0}", string.Join(",", gyroSens));
            //Console.WriteLine("{0}", string.Join(",", tmpBuffer.Skip(SPI_RESP_OFFSET).ToArray()));

            gyroCoeff[IMU_YAW_IDX] = 936.0 / (gyroSens[IMU_YAW_IDX] - gyroBias[IMU_YAW_IDX]);
            gyroCoeff[IMU_PITCH_IDX] = 936.0 / (gyroSens[IMU_PITCH_IDX] - gyroBias[IMU_PITCH_IDX]);
            gyroCoeff[IMU_ROLL_IDX] = 936.0 / (gyroSens[IMU_ROLL_IDX] - gyroBias[IMU_ROLL_IDX]);
            //gyroCoeff[IMU_YAW_IDX] = (gyroSens[IMU_YAW_IDX] - gyroBias[IMU_YAW_IDX]) / 65535.0;
            //gyroCoeff[IMU_PITCH_IDX] = (gyroSens[IMU_PITCH_IDX] - gyroBias[IMU_PITCH_IDX]) / 65535.0;
            //gyroCoeff[IMU_ROLL_IDX] = (gyroSens[IMU_ROLL_IDX] - gyroBias[IMU_ROLL_IDX]) / 65535.0;
            //Console.WriteLine("GYRO COEFF: {0}", string.Join(",", gyroCoeff));
        }

        public override void Detach()
        {
            bool result;
            if (!hidDevice.IsFileStreamOpen())
            {
                return;
            }

            // Disable Gyro
            byte[] tmpOffBuffer = new byte[] { 0x0 };
            Subcommand(0x40, tmpOffBuffer, 1, checkResponse: true);

            // Possibly disable rumble? Leave commented
            tmpOffBuffer = new byte[] { 0x0 };
            Subcommand(0x48, tmpOffBuffer, 1, checkResponse: true);

            // Revert back to low power state
            byte[] powerChoiceArray = new byte[] { 0x01 };
            Subcommand(SwitchProSubCmd.SET_LOW_POWER_STATE, powerChoiceArray, 1, checkResponse: true);

            if (connectionType == ConnectionType.USB)
            {
                byte[] data = new byte[64];
                data[0] = 0x80; data[1] = 0x05;
                result = hidDevice.WriteOutputReportViaControl(data);

                data[0] = 0x80; data[1] = 0x06;
                result = hidDevice.WriteOutputReportViaControl(data);
            }

            hidDevice.fileStream.Flush();
            hidDevice.CloseDevice();
        }

        public void SyncStates()
        {
            previousState = currentState;
        }
    }
}

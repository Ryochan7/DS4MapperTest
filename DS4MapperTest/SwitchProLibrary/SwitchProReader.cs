using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4MapperTest.Common;
using HidLibrary;

namespace DS4MapperTest.SwitchProLibrary
{
    public class SwitchProReader : DeviceReaderBase
    {
        private const ushort STICK_MAX = 3200;
        private const ushort STICK_MIN = 500;
        private const int IMU_XAXIS_IDX = 0, IMU_YAW_IDX = 0;
        private const int IMU_YAXIS_IDX = 1, IMU_PITCH_IDX = 1;
        private const int IMU_ZAXIS_IDX = 2, IMU_ROLL_IDX = 2;

        private SwitchProDevice device;
        public SwitchProDevice Device { get => device; }
        private Thread inputThread;
        private bool activeInputLoop = false;
        private byte[] inputReportBuffer;
        private byte[] outputReportBuffer;
        private byte[] rumbleReportBuffer;
        //private byte frameCount = 0x00;
        private GyroCalibration gyroCalibrationUtil = new GyroCalibration();

        private double combLatency;
        public double CombLatency { get => combLatency; set => combLatency = value; }

        public delegate void SwitchProReportDelegate(SwitchProReader sender,
            SwitchProDevice device);
        public event SwitchProReportDelegate Report;
        public event EventHandler<SwitchProDevice> LeftStickCalibUpdated;
        public event EventHandler<SwitchProDevice> RightStickCalibUpdated;

        public SwitchProReader(SwitchProDevice device)
        {
            this.device = device;

            inputReportBuffer = new byte[device.InputReportLen];
            outputReportBuffer = new byte[device.OutputReportLen];
            rumbleReportBuffer = new byte[SwitchProDevice.RUMBLE_REPORT_LEN];
        }

        public void PrepareDevice()
        {
            NativeMethods.HidD_SetNumInputBuffers(device.HidDevice.safeReadHandle.DangerousGetHandle(),
                3);

            device.SetOperational();
        }

        public override void StartUpdate()
        {
            PrepareDevice();

            inputThread = new Thread(ReadInput);
            inputThread.IsBackground = true;
            inputThread.Priority = ThreadPriority.AboveNormal;
            inputThread.Name = "Switch Pro Reader Thread";
            inputThread.Start();
        }

        public override void StopUpdate()
        {
            activeInputLoop = false;
            Report = null;
            device.PurgeRemoval();
            device.HidDevice.CancelIO();
            //inputThread.Interrupt();
            if (inputThread != null && inputThread.IsAlive &&
                Thread.CurrentThread != inputThread)
            {
                inputThread.Join();
            }
        }

        private void ReadInput()
        {
            activeInputLoop = true;
            byte[] stick_raw = { 0, 0, 0 };
            byte[] stick_raw2 = { 0, 0, 0 };
            short[] accel_raw = { 0, 0, 0 };
            short[] gyro_raw = new short[9];
            short[] gyro_out = new short[9];
            //short gyroYaw = 0, gyroYaw2 = 0, gyroYaw3 = 0;
            //short gyroPitch = 0, gyroPitch2 = 0, gyroPitch3 = 0;
            //short gyroRoll = 0, gyroRoll2 = 0, gyroRoll3 = 0;
            short tempShort = 0;
            int tempAxis = 0;
            int tempAxisX = 0;
            int tempAxisY = 0;

            long currentTime = 0;
            long previousTime = 0;
            long previousCheckTime = 0;
            long deltaElapsed = 0;
            long deltaCheckElapsed;
            double lastElapsed;
            double lastCheckElapsed;
            double tempTimeElapsed;
            double lastCheckTimeElapsed;
            bool firstReport = true;

            // Run continuous calibration on Gyro when starting input loop
            gyroCalibrationUtil.ResetContinuousCalibration();

            unchecked
            {
                while (activeInputLoop)
                {
                    readWaitEv.Set();

                    HidDevice.ReadStatus res = device.HidDevice.ReadWithFileStream(inputReportBuffer);
                    if (res == HidDevice.ReadStatus.Success)
                    {
                        if (inputReportBuffer[0] != 0x30)
                        {
                            Console.WriteLine("Got unexpected input report id 0x{0:X2}. Try again",
                                inputReportBuffer[0]);

                            continue;
                        }
                        else if (firstReport)
                        {
                            Console.WriteLine("CAN READ REPORTS. NICE");
                        }

                        readWaitEv.Wait();
                        readWaitEv.Reset();

                        //Console.WriteLine("GOT INPUT REPORT {0} 0x{1:X2}", res, inputReportBuffer[0]);
                        ref SwitchProState current = ref device.ClothOff;
                        ref SwitchProState previous = ref device.ClothOff2;
                        byte tmpByte;

                        // Obtain stats for last accepted poll time
                        currentTime = Stopwatch.GetTimestamp();
                        deltaElapsed = currentTime - previousTime;
                        lastElapsed = deltaElapsed * (1.0 / Stopwatch.Frequency) * 1000.0;
                        tempTimeElapsed = lastElapsed * .001;
                        //combLatency += tempTimeElapsed;
                        combLatency = tempTimeElapsed;

                        // Obtain stats for current poll time
                        deltaCheckElapsed = currentTime - previousCheckTime;
                        lastCheckElapsed = deltaCheckElapsed * (1.0 / Stopwatch.Frequency) * 1000.0;
                        lastCheckTimeElapsed = lastCheckElapsed * 0.001;
                        previousCheckTime = currentTime;

                        //Trace.WriteLine("Poll Time: {0}", tempTimeElapsed);
                        //Trace.WriteLine("Last Poll Time: {0}", lastCheckTimeElapsed);
                        // Check if most recent poll exceeded a certain duration. Avoids false poll state?
                        if (lastCheckTimeElapsed <= 0.005)
                        {
                            continue;
                        }

                        //current.timeElapsed = tempTimeElapsed;
                        current.timeElapsed = combLatency;

                        ////Console.WriteLine("Poll Time: {0}", lastElapsed);
                        /*if (!firstReport && lastElapsed >= 30.0)
                        {
                            Console.WriteLine("High Latency: {0} {1}", lastElapsed, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"));
                        }

                        if (!firstReport && lastElapsed <= 5.0)
                        {
                            Console.WriteLine("Low Latency: {0} {1}", lastElapsed, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"));
                        }
                        */
                        previousTime = currentTime;
                        combLatency = 0.0;
                        //firstReport = false;
                        //continue;

                        current.FrameTimer = inputReportBuffer[1];

                        tmpByte = inputReportBuffer[2];
                        current.Battery = ((tmpByte & 0xE0) >> 4) * 100 / 8;
                        current.ConnInfo = (byte)(tmpByte & 0x0F);
                        current.Charging = (tmpByte & 0x10) != 0;
                        //Console.WriteLine("BATTERY: {0}", current.Battery);
                        //Console.WriteLine("Frame Time: {0}", current.FrameTimer);
                        //Console.WriteLine("CONN INFO: {0}", current.ConnInfo);
                        //Console.WriteLine("Charging: {0}", current.Charging);

                        tmpByte = inputReportBuffer[3];
                        current.A = (tmpByte & 0x08) != 0;
                        current.B = (tmpByte & 0x04) != 0;
                        current.X = (tmpByte & 0x02) != 0;
                        current.Y = (tmpByte & 0x01) != 0;
                        current.RShoulder = (tmpByte & 0x40) != 0;
                        current.ZR = (tmpByte & 0x80) != 0;

                        tmpByte = inputReportBuffer[4];
                        current.Minus = (tmpByte & 0x01) != 0;
                        current.Plus = (tmpByte & 0x02) != 0;
                        current.Home = (tmpByte & 0x10) != 0;
                        current.Capture = (tmpByte & 0x20) != 0;
                        current.LSClick = (tmpByte & 0x08) != 0;
                        current.RSClick = (tmpByte & 0x04) != 0;

                        tmpByte = inputReportBuffer[5];
                        current.DpadUp = (tmpByte & 0x02) != 0;
                        current.DpadDown = (tmpByte & 0x01) != 0;
                        current.DpadLeft = (tmpByte & 0x08) != 0;
                        current.DpadRight = (tmpByte & 0x04) != 0;
                        current.LShoulder = (tmpByte & 0x40) != 0;
                        current.ZL = (tmpByte & 0x80) != 0;

                        stick_raw[0] = inputReportBuffer[6];
                        stick_raw[1] = inputReportBuffer[7];
                        stick_raw[2] = inputReportBuffer[8];

                        tempAxisX = (stick_raw[0] | ((stick_raw[1] & 0x0F) << 8));
                        tempAxisY = ((stick_raw[1] >> 4) | (stick_raw[2] << 4));
                        if (firstReport && !device.foundLeftStickCalib)
                        {
                            bool calibUpdated = false;
                            if (tempAxisX > device.leftStickXData.mid)
                            {
                                uint diff = (uint)(tempAxisX - device.leftStickXData.mid);
                                device.leftStickXData.min = (ushort)(device.leftStickXData.min + diff);
                                calibUpdated = true;
                            }
                            else if (tempAxisX < device.leftStickXData.mid)
                            {
                                uint diff = (uint)(device.leftStickXData.mid - tempAxisX);
                                device.leftStickXData.max = (ushort)(device.leftStickXData.max - diff);
                                calibUpdated = true;
                            }

                            if (tempAxisY > device.leftStickYData.mid)
                            {
                                uint diff = (uint)(tempAxisY - device.leftStickYData.mid);
                                device.leftStickYData.min = (ushort)(device.leftStickYData.min + diff);
                                calibUpdated = true;
                            }
                            else if (tempAxisY < device.leftStickYData.mid)
                            {
                                uint diff = (uint)(device.leftStickYData.mid - tempAxisY);
                                device.leftStickYData.max = (ushort)(device.leftStickYData.max - diff);
                                calibUpdated = true;
                            }

                            if (calibUpdated)
                            {
                                LeftStickCalibUpdated?.Invoke(this, device);
                            }
                        }

                        //current.LX = tempAxisX > STICK_MAX ? (ushort)STICK_MAX : (tempAxisX < STICK_MIN ? (ushort)STICK_MIN : (ushort)tempAxisX);
                        tempAxisX = tempAxisX > device.leftStickXData.max ? device.leftStickXData.max : (tempAxisX < device.leftStickXData.min ? device.leftStickXData.min : tempAxisX);
                        //current.LX = (byte)((tempAxisX - device.leftStickXData.min) / (double)(device.leftStickXData.max - device.leftStickXData.min) * 255);
                        current.LX = (ushort)tempAxisX;

                        //current.LY = tempAxisY > STICK_MAX ? (ushort)STICK_MAX : (tempAxisY < STICK_MIN ? (ushort)STICK_MIN : (ushort)tempAxisY);
                        tempAxisY = tempAxisY > device.leftStickYData.max ? device.leftStickYData.max : (tempAxisY < device.leftStickYData.min ? device.leftStickYData.min : tempAxisY);
                        current.LY = (ushort)tempAxisY;
                        //current.LY = (byte)((((tempAxisY - device.leftStickYData.min) / (double)(device.leftStickYData.max - device.leftStickYData.min) - 0.5) * -1.0 + 0.5) * 255);

                        //Console.WriteLine("TEST VALUES: {0} {1}", current.LX, current.LY);

                        /*current.LX = (ushort)(stick_raw[0] | ((stick_raw[1] & 0x0F) << 8));
                        current.LX = current.LX > STICK_MAX ? (ushort)STICK_MAX : (current.LX < STICK_MIN ? (ushort)STICK_MIN : current.LX);
                        current.LY = (ushort)((stick_raw[1] >> 4) | (stick_raw[2] << 4));
                        current.LY = current.LY > STICK_MAX ? (ushort)STICK_MAX : (current.LY < STICK_MIN ? (ushort)STICK_MIN : current.LY);
                        */

                        //Console.WriteLine("LX {0}", current.LX);
                        //Console.WriteLine("LY {0}", current.LY);

                        stick_raw2[0] = inputReportBuffer[9];
                        stick_raw2[1] = inputReportBuffer[10];
                        stick_raw2[2] = inputReportBuffer[11];

                        tempAxisX = (stick_raw2[0] | ((stick_raw2[1] & 0x0F) << 8));
                        tempAxisY = ((stick_raw2[1] >> 4) | (stick_raw2[2] << 4));
                        if (firstReport && !device.foundRightStickCalib)
                        {
                            bool calibUpdated = false;
                            if (tempAxisX > device.rightStickXData.mid)
                            {
                                uint diff = (uint)(tempAxisX - device.rightStickXData.mid);
                                device.rightStickXData.min = (ushort)(device.rightStickXData.min + diff);
                                calibUpdated = true;
                            }
                            else if (tempAxisX < device.rightStickXData.mid)
                            {
                                uint diff = (uint)(device.rightStickXData.mid - tempAxisX);
                                device.rightStickXData.max = (ushort)(device.rightStickXData.max - diff);
                                calibUpdated = true;
                            }

                            if (tempAxisY > device.rightStickYData.mid)
                            {
                                uint diff = (uint)(tempAxisY - device.rightStickYData.mid);
                                device.rightStickYData.min = (ushort)(device.rightStickYData.min + diff);
                                calibUpdated = true;
                            }
                            else if (tempAxisY < device.rightStickYData.mid)
                            {
                                uint diff = (uint)(device.rightStickYData.mid - tempAxisY);
                                device.rightStickYData.max = (ushort)(device.rightStickYData.max - diff);
                                calibUpdated = true;
                            }

                            if (calibUpdated)
                            {
                                RightStickCalibUpdated?.Invoke(this, device);
                            }
                        }

                        //current.RX = tempAxisX > STICK_MAX ? (ushort)STICK_MAX : (tempAxisX < STICK_MIN ? (ushort)STICK_MIN : (ushort)tempAxisX);
                        tempAxisX = tempAxisX > device.rightStickXData.max ? device.rightStickXData.max : (tempAxisX < device.rightStickXData.min ? device.rightStickXData.min : tempAxisX);
                        //current.RX = (byte)((tempAxisX - device.rightStickXData.min) / (double)(device.rightStickXData.max - device.rightStickXData.min) * 255);
                        current.RX = (ushort)tempAxisX;

                        //current.RY = tempAxisY > STICK_MAX ? (ushort)STICK_MAX : (tempAxisY < STICK_MIN ? (ushort)STICK_MIN : (ushort)tempAxisY);
                        tempAxisY = tempAxisY > device.rightStickYData.max ? device.rightStickYData.max : (tempAxisY < device.rightStickYData.min ? device.rightStickYData.min : tempAxisY);
                        //current.RY = (byte)((((tempAxisY - device.rightStickYData.min) / (double)(device.rightStickYData.max - device.rightStickYData.min) - 0.5) * -1.0 + 0.5) * 255);
                        current.RY = (ushort)tempAxisY;

                        /*current.RX = (ushort)(stick_raw2[0] | ((stick_raw2[1] & 0x0F) << 8));
                        current.RX = current.RX > STICK_MAX ? (ushort)STICK_MAX : (current.RX < STICK_MIN ? (ushort)STICK_MIN : current.RX);
                        current.RY = (ushort)((stick_raw2[1] >> 4) | (stick_raw2[2] << 4));
                        current.RY = current.RY > STICK_MAX ? (ushort)STICK_MAX : (current.RY < STICK_MIN ? (ushort)STICK_MIN : current.RY);
                        */

                        //Console.WriteLine("TEST VALUES: {0} {1}", current.RX, current.RY);

                        for (int i = 0; i < 3; i++)
                        {
                            int data_offset = i * 12;
                            int gyro_offset = i * 3;
                            accel_raw[IMU_XAXIS_IDX] = (short)((ushort)(inputReportBuffer[16 + data_offset] << 8) | inputReportBuffer[15 + data_offset]);
                            accel_raw[IMU_YAXIS_IDX] = (short)((ushort)(inputReportBuffer[14 + data_offset] << 8) | inputReportBuffer[13 + data_offset]);
                            accel_raw[IMU_ZAXIS_IDX] = (short)((ushort)(inputReportBuffer[18 + data_offset] << 8) | inputReportBuffer[17 + data_offset]);

                            tempShort = gyro_raw[IMU_YAW_IDX + gyro_offset] = (short)((ushort)(inputReportBuffer[24 + data_offset] << 8) | inputReportBuffer[23 + data_offset]);
                            //gyro_out[IMU_YAW_IDX + gyro_offset] = (short)(tempShort - device.gyroBias[IMU_YAW_IDX]);
                            gyro_out[IMU_YAW_IDX + gyro_offset] = (short)(tempShort);

                            tempShort = gyro_raw[IMU_PITCH_IDX + gyro_offset] = (short)((ushort)(inputReportBuffer[22 + data_offset] << 8) | inputReportBuffer[21 + data_offset]);
                            //gyro_out[IMU_PITCH_IDX + gyro_offset] = (short)(tempShort - device.gyroBias[IMU_PITCH_IDX]);
                            gyro_out[IMU_PITCH_IDX + gyro_offset] = (short)(tempShort);

                            tempShort = gyro_raw[IMU_ROLL_IDX + gyro_offset] = (short)((ushort)(inputReportBuffer[20 + data_offset] << 8) | inputReportBuffer[19 + data_offset]);
                            //gyro_out[IMU_ROLL_IDX + gyro_offset] = (short)(tempShort - device.gyroBias[IMU_ROLL_IDX]);
                            gyro_out[IMU_ROLL_IDX + gyro_offset] = (short)(tempShort);

                            //Console.WriteLine($"IDX: ({i}) Accel: X({accel_raw[IMU_XAXIS_IDX]}) Y({accel_raw[IMU_YAXIS_IDX]}) Z({accel_raw[IMU_ZAXIS_IDX]})");
                            //Console.WriteLine($"IDX: ({i}) Gyro: Yaw({gyro_raw[IMU_YAW_IDX + gyro_offset]}) Pitch({gyro_raw[IMU_PITCH_IDX + gyro_offset]}) Roll({gyro_raw[IMU_ROLL_IDX + gyro_offset]})");
                            //Console.WriteLine($"IDX: ({i}) Gyro OUT: Yaw({gyro_out[IMU_YAW_IDX + gyro_offset]}) Pitch({gyro_out[IMU_PITCH_IDX + gyro_offset]}) Roll({gyro_out[IMU_ROLL_IDX + gyro_offset]})");
                            //Console.WriteLine();
                        }

                        //Console.WriteLine();

                        // For Accel, just use most recent sampled values
                        short accelX = accel_raw[IMU_XAXIS_IDX];
                        short accelY = accel_raw[IMU_YAXIS_IDX];
                        short accelZ = accel_raw[IMU_ZAXIS_IDX];

                        // Just use most recent sample for now
                        //short gyroYaw = (short)(-1 * (gyro_out[6 + IMU_YAW_IDX] - device.gyroBias[IMU_YAW_IDX]));
                        //short gyroPitch = (short)(gyro_out[6 + IMU_PITCH_IDX] - device.gyroBias[IMU_PITCH_IDX]);
                        //short gyroRoll = (short)(gyro_out[6 + IMU_ROLL_IDX] - device.gyroBias[IMU_ROLL_IDX]);
                        short gyroYaw = (short)(-1 * (gyro_out[6 + IMU_YAW_IDX] - device.gyroBias[IMU_YAW_IDX] + device.gyroCalibOffsets[IMU_YAW_IDX]));
                        short gyroPitch = (short)(gyro_out[6 + IMU_PITCH_IDX] - device.gyroBias[IMU_PITCH_IDX] - device.gyroCalibOffsets[IMU_PITCH_IDX]);
                        short gyroRoll = (short)(gyro_out[6 + IMU_ROLL_IDX] - device.gyroBias[IMU_ROLL_IDX] - device.gyroCalibOffsets[IMU_ROLL_IDX]);


                        if (gyroCalibrationUtil.gyroAverageTimer.IsRunning)
                        {
                            int currentYaw = gyroYaw, currentPitch = gyroPitch, currentRoll = gyroRoll;
                            int AccelX = accelX, AccelY = accelY, AccelZ = accelZ;
                            gyroCalibrationUtil.CalcSensorCamples(ref currentYaw, ref currentPitch, ref currentRoll,
                                ref AccelX, ref AccelY, ref AccelZ);
                        }

                        gyroYaw -= (short)gyroCalibrationUtil.gyro_offset_x;
                        gyroPitch -= (short)gyroCalibrationUtil.gyro_offset_y;
                        gyroRoll -= (short)gyroCalibrationUtil.gyro_offset_z;

                        current.Motion.Populate(accelX, accelY, accelZ,
                            gyroYaw, gyroPitch, gyroRoll, device.accelCoeff, device.gyroCoeff);
                        //current.Motion.GyroYaw = gyro_out[IMU_YAW_IDX] + gyro_out[3 + IMU_YAW_IDX] + gyro_out[6 + IMU_YAW_IDX];
                        //current.Motion.GyroPitch = gyro_out[IMU_PITCH_IDX] + gyro_out[3 + IMU_PITCH_IDX] + gyro_out[6 + IMU_PITCH_IDX];
                        //current.Motion.GyroRoll = gyro_out[IMU_ROLL_IDX] + gyro_out[3 + IMU_ROLL_IDX] + gyro_out[6 + IMU_ROLL_IDX];

                        //Console.WriteLine("Final Accel: X({0}), Y({1}), Z({2})",
                        //    current.Motion.AccelX, current.Motion.AccelY, current.Motion.AccelZ);
                        //Console.WriteLine("Final Gyro: Yaw({0}), Pitch({1}), Roll({2})",
                        //    current.Motion.GyroYaw, current.Motion.GyroPitch, current.Motion.GyroRoll);

                        if (previous.Battery != current.Battery)
                        {
                            device.Battery = (uint)current.Battery;
                        }

                        if (fireReport)
                        {
                            Report?.Invoke(this, device);
                        }
                        //WriteReport();

                        device.SyncStates();
                        firstReport = false;
                    }
                    else
                    {
                        activeInputLoop = false;
                        readWaitEv.Reset();
                        device.RaiseRemoval();
                    }

                    //Thread.Sleep(16);
                }
            }

            activeInputLoop = false;
            readWaitEv.Reset();
        }

        // Attempt to inline method
        private void PrepareReport()
        {

        }

        public override void WriteRumbleReport()
        {
            if (activeInputLoop)
            {
                //byte[] tmpbuff = new byte[SwitchProDevice.RUMBLE_REPORT_LEN];
                device.PrepareRumbleData(rumbleReportBuffer);
                //Console.WriteLine("RUMBLE BUFF: {0}",
                //    string.Concat(rumbleReportBuffer.Select(i => string.Format("{0:x2} ", i))));
                //bool result = device.HidDevice.WriteOutputReportViaControl(rumbleReportBuffer);
                bool result = device.HidDevice.WriteOutputReportViaInterrupt(rumbleReportBuffer, 100);
                //device.HidDevice.fileStream.Flush();
                //Console.WriteLine("RUMBLE FINISH");
            }
        }
    }
}

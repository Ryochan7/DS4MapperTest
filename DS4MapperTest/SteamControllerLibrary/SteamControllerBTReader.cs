using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;

namespace DS4MapperTest.SteamControllerLibrary
{
    public class SteamControllerBTReader : SteamControllerReader
    {
        private const byte LONG_PACKET = 0x80;
        private const byte SINGLE_PAYLOAD_PREFIX = 0x40;
        private const byte START_PAYLOAD_BYTE = 0xC0;
        private const byte SECOND_PAYLOAD_BYTE = 0xC1;
        private const byte THIRD_PAYLOAD_BYTE = 0xC2;

        private const int BUTTONS_PAYLOAD_LEN = 3;
        private const int TRIGGERS_PAYLOAD_LEN = 2;
        private const int STICK_PAYLOAD_LEN = 4;
        private const int LPAD_PAYLOAD_LEN = 4;
        private const int RPAD_PAYLOAD_LEN = 4;
        private const int GYRO_PAYLOAD_LEN = 20;

        private int currentPacketNum;

        private class BtInputPakcetType
        {
            public const ushort BUTTONS = 0x10;
            public const ushort TRIGGERS = 0x20;
            public const ushort STICK = 0x80;
            public const ushort LPAD = 0x100;
            public const ushort RPAD = 0x200;
            public const ushort GYRO = 0x1800;
            public const ushort PING = 0x5000;
        }

        private byte[] tempInputReportBuffer;
        public override event SteamControllerReportDelegate Report = null;

        public SteamControllerBTReader(SteamControllerBTDevice inputDevice) :
            base(inputDevice)
        {
            inputReportBuffer = new byte[device.InputReportLen * 3];
            tempInputReportBuffer = new byte[device.InputReportLen];
        }

        public override void PrepareDevice()
        {
            NativeMethods.HidD_SetNumInputBuffers(device.HidDevice.safeReadHandle.DangerousGetHandle(),
                20);

            device.SetOperational();
        }

        //public override void StartUpdate()
        //{
        //    PrepareDevice();

        //    inputThread = new Thread(ReadInput);
        //    inputThread.IsBackground = true;
        //    inputThread.Priority = ThreadPriority.AboveNormal;
        //    inputThread.Name = "Steam Controller Reader Thread";
        //    inputThread.Start();
        //}

        protected override void ReadInput()
        {
            activeInputLoop = true;

            bool longPacket = false;
            bool processPayload = false;
            bool inLongPacket = false;
            int payloadLength = 0;
            byte[] usedBuffer = null;

            bool lsCoorConflict = false;
            short tempAxisX;
            short tempAxisY;
            byte tempByte;

            long currentTime = 0;
            long previousTime = 0;
            long deltaElapsed = 0;
            double lastElapsed;
            double tempTimeElapsed;
            bool firstReport = true;

            unchecked
            {
                while (activeInputLoop)
                {
                    HidDevice.ReadStatus res = HidDevice.ReadStatus.NoDataRead;
                    {
                        //res = device.HidDevice.ReadWithFileStream(inputReportBuffer, out bytesRead, 0, 20);
                        res = device.HidDevice.ReadWithFileStream(tempInputReportBuffer);
                    }

                    if (res == HidDevice.ReadStatus.Success)
                    {
                        if (firstReport && tempInputReportBuffer[0] != 0x03)
                        {
                            continue;
                        }
                        else if (tempInputReportBuffer[0] != 0x03)
                        {
                            Trace.WriteLine("IN HERE");
                            continue;
                        }

                        processPayload = false;
                        bool endPayload = false;
                        bool resetDataBuffer = false;
                        bool oldInLongPacket = inLongPacket;

                        byte checkPayloadByte = tempInputReportBuffer[1];
                        currentPacketNum = checkPayloadByte & 0x0F;

                        //string tempOutPartStr = BitConverter.ToString(tempInputReportBuffer);
                        ////Trace.WriteLine(string.Join(", ", inputReportBuffer));
                        //Trace.WriteLine(tempOutPartStr.Replace("-", ", "));

                        if (inLongPacket)
                        {
                            // Copy data from second portion of packet to full input report data buffer.
                            // Skip first two bytes from temp buffer (report ID and payload byte)
                            int offset = (18 * currentPacketNum) + 2;
                            Array.Copy(tempInputReportBuffer, 2, inputReportBuffer, offset, 18);
                        }
                        else
                        {
                            Array.Copy(tempInputReportBuffer, 0, inputReportBuffer, 0, 20);
                        }

                        endPayload = (checkPayloadByte & SINGLE_PAYLOAD_PREFIX) == SINGLE_PAYLOAD_PREFIX;
                        inLongPacket = !endPayload;
                        processPayload = endPayload;

                        //if (oldInLongPacket && endPayload)
                        if (endPayload)
                        {
                            resetDataBuffer = true;
                        }

                        tempByte = inputReportBuffer[3];
                        byte longPacketByte = inputReportBuffer[1];
                        uint packetTypeByte = (uint)((inputReportBuffer[3] << 8) | inputReportBuffer[2]);

                        //if (longPacketByte == 0xC1 || longPacketByte == 0xC2)
                        if (longPacketByte != 0x80 && longPacketByte != 0xC0)
                        {
                            Trace.WriteLine($"SECOND PART IN ERROR. LONG: {longPacket} {longPacketByte}");
                            string tempOutStr3 = BitConverter.ToString(inputReportBuffer);
                            ////Trace.WriteLine(string.Join(", ", inputReportBuffer));
                            Trace.WriteLine(tempOutStr3.Replace("-", ", "));
                        }

                        //Trace.WriteLine(packetTypeByte);
                        if (!processPayload)
                        {
                            //Trace.WriteLine(string.Join(", ", inputReportBuffer));
                            //longPacket = true;
                            //Trace.WriteLine("FOUND LONG");
                            continue;
                        }
                        else
                        {
                            //Trace.WriteLine("LONG END");
                            longPacket = false;
                        }

                        ref SteamControllerState current = ref device.CurrentStateRef;
                        ref SteamControllerState previous = ref device.PreviousStateRef;

                        currentTime = Stopwatch.GetTimestamp();
                        deltaElapsed = currentTime - previousTime;
                        lastElapsed = deltaElapsed * (1.0 / Stopwatch.Frequency) * 1000.0;
                        tempTimeElapsed = lastElapsed * .001;

                        /*if ((packetTypeByte & 0x05) == 0x05)
                        {
                            Trace.WriteLine($"PING {packetTypeByte}");
                            continue;
                        }
                        */

                        if ((packetTypeByte & BtInputPakcetType.PING) == BtInputPakcetType.PING)
                        {
                            //Trace.WriteLine($"PING {packetTypeByte}");

                            /*string tempOutStr3 = BitConverter.ToString(inputReportBuffer);
                            ////Trace.WriteLine(string.Join(", ", inputReportBuffer));
                            Trace.WriteLine(tempOutStr3.Replace("-", ", "));
                            */

                            //if (resetDataBuffer)
                            //{
                            //    Array.Clear(inputReportBuffer, 0, inputReportBuffer.Length);
                            //}

                            uint batt = (uint)((inputReportBuffer[14] << 8) | inputReportBuffer[13]);
                            //Trace.WriteLine($"{batt} {inputReportBuffer[13]:X2} {inputReportBuffer[14]:X2}");
                            // Percentage / Voltage (mV)
                            const uint BATSLOPE = (100 - 0) / (3500 - 2000);
                            //uint tempBat = Math.Clamp(batt / 35, 0, 100);
                            uint tempBat = Math.Clamp(batt * BATSLOPE, 0, 100);
                            device.Battery = tempBat;
                            continue;
                        }

                        current.timeElapsed = tempTimeElapsed;
                        previousTime = currentTime;

                        //Trace.WriteLine(tempTimeElapsed);

                        int dataIdx = 4;
                        if ((packetTypeByte & BtInputPakcetType.BUTTONS) == BtInputPakcetType.BUTTONS)
                        {
                            //Trace.Write("IN BUTTONS ");
                            // Buttons
                            tempByte = inputReportBuffer[dataIdx];
                            current.RTClick = (tempByte & 0x01) != 0;
                            current.LTClick = (tempByte & 0x02) != 0;
                            current.RB = (tempByte & 0x04) != 0;
                            current.LB = (tempByte & 0x08) != 0;
                            current.Y = (tempByte & 0x10) != 0;
                            current.B = (tempByte & 0x20) != 0;
                            current.X = (tempByte & 0x40) != 0;
                            current.A = (tempByte & 0x80) != 0;

                            // Buttons
                            tempByte = inputReportBuffer[dataIdx + 1];
                            current.DPadUp = (tempByte & 0x01) != 0;
                            current.DPadRight = (tempByte & 0x02) != 0;
                            current.DPadLeft = (tempByte & 0x04) != 0;
                            current.DPadDown = (tempByte & 0x08) != 0;
                            current.Back = (tempByte & 0x10) != 0;
                            current.Guide = (tempByte & 0x20) != 0;
                            current.Start = (tempByte & 0x40) != 0;
                            current.LGrip = (tempByte & 0x80) != 0;

                            // Buttons
                            tempByte = inputReportBuffer[dataIdx + 2];
                            current.RGrip = (tempByte & 0x01) != 0;
                            current.LeftPad.Click = (tempByte & 0x02) != 0;
                            current.RightPad.Click = (tempByte & 0x04) != 0;
                            current.LeftPad.Touch = (tempByte & 0x08) != 0;
                            current.RightPad.Touch = (tempByte & 0x10) != 0;
                            current.LSClick = (tempByte & 0x40) != 0;
                            //lsCoorConflict = (tempByte & 0x80) != 0;

                            dataIdx += 3;

                            /*Trace.WriteLine($"BUTTONS {inputReportBuffer[4]} {current.A} {packetTypeByte}");

                            string tempOutStr4 = BitConverter.ToString(inputReportBuffer);
                            //Trace.WriteLine(string.Join(", ", inputReportBuffer));
                            Trace.WriteLine($"DSP {tempOutStr4.Replace("-", ", ")}");
                            */

                            /*if (current.LGrip)
                            {
                                //Trace.WriteLine($"LGRIP {inputReportBuffer[5]}");
                                //string tempOutStr4 = BitConverter.ToString(inputReportBuffer);
                                ////Trace.WriteLine(string.Join(", ", inputReportBuffer));
                                //Trace.WriteLine(tempOutStr4.Replace("-", ", "));
                            }
                            */
                        }

                        if ((packetTypeByte & BtInputPakcetType.TRIGGERS) == BtInputPakcetType.TRIGGERS)
                        {
                            //Trace.Write("IN TRIGGERS ");
                            current.LT = inputReportBuffer[dataIdx];
                            current.RT = inputReportBuffer[dataIdx + 1];

                            //Trace.WriteLine($"TRIGGERS: {inputReportBuffer[dataIdx]}");
                            dataIdx += 2;
                        }

                        if ((packetTypeByte & BtInputPakcetType.STICK) == BtInputPakcetType.STICK)
                        {
                            //Trace.Write("IN STICK ");
                            current.LX = (short)((inputReportBuffer[dataIdx + 1] << 8) | inputReportBuffer[dataIdx]);
                            current.LY = (short)((inputReportBuffer[dataIdx + 3] << 8) | inputReportBuffer[dataIdx + 2]);

                            //Trace.WriteLine($"STICK {inputReportBuffer[dataIdx]} {current.LY}");
                            dataIdx += 4;
                        }

                        if ((packetTypeByte & BtInputPakcetType.LPAD) == BtInputPakcetType.LPAD)
                        {
                            //Trace.Write("IN LPAD ");
                            current.LeftPad.X = (short)((inputReportBuffer[dataIdx + 1] << 8) | inputReportBuffer[dataIdx]);
                            current.LeftPad.Y = (short)((inputReportBuffer[dataIdx + 3] << 8) | inputReportBuffer[dataIdx + 2]);

                            //Trace.WriteLine($"LPAD {inputReportBuffer[dataIdx]} {current.LeftPad.X}");
                            dataIdx += 4;
                        }
                        //else if (!current.LeftPad.Touch)
                        //{
                        //    current.LeftPad.X = 0;
                        //    current.LeftPad.Y = 0;
                        //}

                        if ((packetTypeByte & BtInputPakcetType.RPAD) == BtInputPakcetType.RPAD)
                        {
                            //Trace.Write("IN RPAD ");
                            current.RightPad.X = (short)((inputReportBuffer[dataIdx + 1] << 8) | inputReportBuffer[dataIdx]);
                            current.RightPad.Y = (short)((inputReportBuffer[dataIdx + 3] << 8) | inputReportBuffer[dataIdx + 2]);

                            //Trace.WriteLine($"RPAD {inputReportBuffer[dataIdx]} {current.RightPad.X} {dataIdx}");
                            dataIdx += 4;
                        }
                        //else if (!current.RightPad.Touch)
                        //{
                        //    current.RightPad.X = 0;
                        //    current.RightPad.Y = 0;
                        //}

                        if ((packetTypeByte & BtInputPakcetType.GYRO) == BtInputPakcetType.GYRO)
                        {
                            //Trace.Write("IN GYRO ");
                            current.Motion.AccelX = (short)(-1 * ((inputReportBuffer[dataIdx + 1] << 8) | inputReportBuffer[dataIdx]));
                            current.Motion.AccelY = (short)((inputReportBuffer[dataIdx + 3] << 8) | inputReportBuffer[dataIdx + 2]);
                            current.Motion.AccelZ = (short)((inputReportBuffer[dataIdx + 5] << 8) | inputReportBuffer[dataIdx + 4]);

                            current.Motion.GyroPitch = (short)(-1 * ((inputReportBuffer[dataIdx + 7] << 8) | inputReportBuffer[dataIdx + 6]));
                            current.Motion.GyroPitch = (short)(current.Motion.GyroPitch - device.gyroCalibOffsets[SteamControllerDevice.IMU_PITCH_IDX]);

                            current.Motion.GyroRoll = (short)(-1 * ((inputReportBuffer[dataIdx + 9] << 8) | inputReportBuffer[dataIdx + 8]));
                            current.Motion.GyroRoll = (short)(current.Motion.GyroRoll - device.gyroCalibOffsets[SteamControllerDevice.IMU_ROLL_IDX]);

                            current.Motion.GyroYaw = (short)(-1 * ((inputReportBuffer[dataIdx + 11] << 8) | inputReportBuffer[dataIdx + 10]));
                            current.Motion.GyroYaw = (short)(current.Motion.GyroYaw - device.gyroCalibOffsets[SteamControllerDevice.IMU_YAW_IDX]);

                            if (gyroCalibrationUtil.gyroAverageTimer.IsRunning)
                            {
                                int currentYaw = current.Motion.GyroPitch, currentPitch = current.Motion.GyroRoll, currentRoll = current.Motion.GyroYaw;
                                int AccelX = current.Motion.AccelX, AccelY = current.Motion.AccelY, AccelZ = current.Motion.AccelZ;
                                gyroCalibrationUtil.CalcSensorCamples(ref currentYaw, ref currentPitch, ref currentRoll,
                                    ref AccelX, ref AccelY, ref AccelZ);
                            }

                            current.Motion.GyroYaw -= (short)gyroCalibrationUtil.gyro_offset_x;
                            current.Motion.GyroPitch -= (short)gyroCalibrationUtil.gyro_offset_y;
                            current.Motion.GyroRoll -= (short)gyroCalibrationUtil.gyro_offset_z;

                            current.Motion.AccelXG = current.Motion.AccelX / SteamControllerState.SteamControllerMotion.D_ACC_RES_PER_G;
                            current.Motion.AccelYG = current.Motion.AccelY / SteamControllerState.SteamControllerMotion.D_ACC_RES_PER_G;
                            current.Motion.AccelZG = current.Motion.AccelZ / SteamControllerState.SteamControllerMotion.D_ACC_RES_PER_G;

                            current.Motion.AngGyroPitch = -1 * current.Motion.GyroPitch * SteamControllerState.SteamControllerMotion.GYRO_RES_IN_DEG_SEC_RATIO;
                            current.Motion.AngGyroRoll = current.Motion.GyroRoll * SteamControllerState.SteamControllerMotion.GYRO_RES_IN_DEG_SEC_RATIO;
                            current.Motion.AngGyroYaw = current.Motion.GyroYaw * SteamControllerState.SteamControllerMotion.GYRO_RES_IN_DEG_SEC_RATIO;

                            current.Motion.QuaternionX = (short)(-1 * ((inputReportBuffer[dataIdx + 13] << 8) | inputReportBuffer[dataIdx + 12]));
                            current.Motion.QuaternionY = (short)(-1 * ((inputReportBuffer[dataIdx + 15] << 8) | inputReportBuffer[dataIdx + 14]));
                            current.Motion.QuaternionZ = (short)(-1 * ((inputReportBuffer[dataIdx + 17] << 8) | inputReportBuffer[dataIdx + 16]));
                            current.Motion.QuaternionW = (short)(-1 * ((inputReportBuffer[dataIdx + 19] << 8) | inputReportBuffer[dataIdx + 18]));

                            //Trace.WriteLine($"GYRO {inputReportBuffer[dataIdx]} {current.Motion.AccelX}");
                            dataIdx += 20;
                        }

                        //Trace.WriteLine("");
                        //Trace.WriteLine(string.Join(", ", inputReportBuffer));
                        /*string tempOutStrEnd = BitConverter.ToString(inputReportBuffer);
                        ////Trace.WriteLine(string.Join(", ", inputReportBuffer));
                        Trace.WriteLine(tempOutStrEnd.Replace("-", ", "));
                        */

                        Report?.Invoke(this, device);

                        device.SyncStates();

                        if (resetDataBuffer)
                        {
                            Array.Clear(inputReportBuffer, 0, inputReportBuffer.Length);
                        }

                        firstReport = false;
                    }
                    else
                    {
                        activeInputLoop = false;
                        device.RaiseRemoval();
                    }
                }
            }
        }
    }
}

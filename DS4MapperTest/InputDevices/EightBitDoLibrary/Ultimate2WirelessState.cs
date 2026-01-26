using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.InputDevices.EightBitDoLibrary
{
    public struct Ultimate2WirelessState
    {
        public struct Ult2Motion
        {
            public const int ACC_RES_PER_G = 4096;
            public const float F_ACC_RES_PER_G = ACC_RES_PER_G;
            // Value found in SDL source code for device reader
            public const float F_GYRO_SCALE = 14.2824f;
            
            public short AccelX;
            public short AccelY;
            public short AccelZ;
            public double AccelXG, AccelYG, AccelZG;

            public short GyroYaw;
            public short GyroPitch;
            public short GyroRoll;
            public double AngGyroYaw, AngGyroPitch, AngGyroRoll;
        }

        public double timeElapsed;
        public uint PacketCounter;
        public DateTime ReportTimeStamp;
        public byte Battery;

        public byte LX;
        public byte LY;
        public byte RX;
        public byte RY;
        public bool LB;
        public byte LT;
        public bool LTBtn;
        public bool LSClick;
        public bool L4;
        public bool PL;
        public bool RB;
        public byte RT;
        public bool RTBtn;
        public bool RSClick;
        public bool R4;
        public bool PR;
        public bool A;
        public bool B;
        public bool X;
        public bool Y;
        public bool Minus;
        public bool Plus;
        public bool Guide;
        public bool DpadUp;
        public bool DpadDown;
        public bool DpadLeft;
        public bool DpadRight;
        public Ult2Motion Motion;
    }
}

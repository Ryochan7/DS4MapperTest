using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.DualSense
{
    public struct DualSenseState
    {
        public struct TouchInfo
        {
            public const int TOUCHPAD_MAX_X = 1920;
            public const int TOUCHPAD_MAX_Y = 942;

            public short X;
            public short Y;
            public bool Touch;
            public bool IsActive;
            public byte Id;
            public byte RawTrackingNum;
        }

        public struct DS4Motion
        {
            public const int ACC_RES_PER_G = 8192;
            public const float F_ACC_RES_PER_G = ACC_RES_PER_G;
            public const int GYRO_RES_IN_DEG_SEC = 16;
            public const float F_GYRO_RES_IN_DEG_SEC = GYRO_RES_IN_DEG_SEC;

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
        public ulong TotalMicroSec;
        public ushort DS4Timestamp;

        public byte LX;
        public byte LY;
        public byte RX;
        public byte RY;
        public bool L1;
        public byte L2;
        public bool L2Btn;
        public bool L3;
        public bool R1;
        public byte R2;
        public bool R2Btn;
        public bool R3;
        public bool Circle;
        public bool Cross;
        public bool Square;
        public bool Triangle;
        public bool Create;
        public bool Options;
        public bool PS;
        public bool Mute;
        public bool DpadUp;
        public bool DpadDown;
        public bool DpadLeft;
        public bool DpadRight;
        public bool TouchClickButton;
        public byte TouchPacketNum;
        public TouchInfo Touch1;
        public TouchInfo Touch2;
        public uint NumTouches;
        public DS4Motion Motion;
    }
}

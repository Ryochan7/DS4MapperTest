using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.InputDevices.EightBitDoLibrary
{
    public struct Ultimate2CState
    {
        public double timeElapsed;
        public uint PacketCounter;
        public DateTime ReportTimeStamp;
        public byte Battery;

        public byte LX;
        public byte LY;
        public byte RX;
        public byte RY;
        public bool L1;
        public byte LT;
        public bool LTBtn;
        public bool L3;
        public bool L4;
        public bool R1;
        public byte RT;
        public bool RTBtn;
        public bool R3;
        public bool R4;
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
    }
}

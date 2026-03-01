using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.MapperUtil
{
    public struct IntermediateState
    {
        public uint PacketCounter;
        public double LX;
        public double LY;
        public bool LSDirty;
        public double RX;
        public double RY;
        public bool RSDirty;
        public double LTrigger;
        public double RTrigger;

        public bool BtnNorth;
        public bool BtnWest;
        public bool BtnSouth;
        public bool BtnEast;
        public bool BtnLShoulder;
        public bool BtnRShoulder;
        public bool BtnMode;
        public bool BtnStart;
        public bool BtnSelect;
        public bool BtnHome;
        public bool BtnExtra;
        public bool BtnThumbL;
        public bool BtnThumbR;
        public bool BtnTouchClick;

        public bool DpadUp;
        public bool DpadLeft;
        public bool DpadDown;
        public bool DpadRight;

        public bool Dirty;
    }
}

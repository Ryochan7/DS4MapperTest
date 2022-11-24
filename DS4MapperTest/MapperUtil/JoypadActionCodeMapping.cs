using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.MapperUtil
{
    public class JoypadActionCodeMapping
    {
        public enum ControlType : uint
        {
            Unknown,
            Button,
            Axis,
            Trigger,
            Stick,
            DPad,
        }

        public ControlType type;

        [StructLayout(LayoutKind.Explicit)]
        public struct OutputValueUnion
        {
            [FieldOffset(0)]
            public JoypadActionCodes btnCode;
            [FieldOffset(0)]
            public JoypadAxesCodes axisCode;
            [FieldOffset(0)]
            public StickActionCodes stickCode;
            [FieldOffset(0)]
            public DPadActionCodes dpadCode;
        }

        public OutputValueUnion outputValue;
        //public JoypadActionCodes code;

        public JoypadActionCodeMapping()
        {
        }

        public JoypadActionCodeMapping(JoypadActionCodeMapping other)
        {
            other.type = type;
            other.outputValue = outputValue;
        }
    }
}

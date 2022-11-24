using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.TouchpadActions
{
    public class TouchpadDefinition
    {
        public struct TouchAxisData
        {
            // Used limits by mapper. Might differ from real limits of hardware
            public short max;
            public short mid;
            public short min;

            // Real hardware limits
            public int hard_max;
            public int hard_min;
            public bool invert;

            public double reciprocalInputResolution;
            public short outputResolution;

            public void PostInit()
            {
                reciprocalInputResolution = 1 / (double)(max - min);
                outputResolution = (short)(max - min);
            }
        }

        public TouchAxisData xAxis;
        public TouchAxisData yAxis;
        public TouchpadActionCodes touchCode;
        public double elapsedReference;

        public double mouseScale;
        public double mouseOffset;
        public double trackballScale;

        public TouchpadDefinition(TouchAxisData xAxis, TouchAxisData yAxis, TouchpadActionCodes touchCode)
        {
            this.xAxis = xAxis;
            this.yAxis = yAxis;
            this.touchCode = touchCode;
        }

        public TouchpadDefinition(TouchAxisData xAxis, TouchAxisData yAxis, TouchpadActionCodes touchCode,
            double elapsedReference, double mouseScale, double mouseOffset, double trackballScale)
        {
            this.xAxis = xAxis;
            this.yAxis = yAxis;
            this.touchCode = touchCode;

            this.elapsedReference = elapsedReference;
            this.mouseScale = mouseScale;
            this.mouseOffset = mouseOffset;
            this.trackballScale = trackballScale;
        }

        public TouchpadDefinition(TouchpadDefinition other)
        {
            this.xAxis = other.xAxis;
            this.yAxis = other.yAxis;
            this.touchCode = other.touchCode;

            this.elapsedReference = other.elapsedReference;
            this.mouseScale = other.mouseScale;
            this.mouseOffset = other.mouseOffset;
            this.trackballScale = other.trackballScale;
        }
    }
}

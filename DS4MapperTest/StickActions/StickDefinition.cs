using DS4MapperTest.MapperUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.StickActions
{
    public class StickDefinition
    {
        public struct StickAxisData
        {
            // Used limits by mapper. Might differ from real limits of hardware
            public short max;
            public short mid;
            public short min;
            public bool invert;

            // Real hardware limits
            public int hard_max;
            public int hard_min;

            public double reciprocalInputResolution;
            public short outputResolution;

            public void PostInit()
            {
                reciprocalInputResolution = 1 / (double)(max - min);
                outputResolution = (short)(max - min);
            }
        };

        /*public int axisMin;
        public int axisMax;
        public int axisMid;
        */
        public StickAxisData xAxis;
        public StickAxisData yAxis;
        public StickActionCodes stickCode;

        /*public StickDefinition(int axisMin, int axisMax, int axisMid,
            StickActionCodes stickCode)
        {
            this.axisMin = axisMin;
            this.axisMax = axisMax;
            this.axisMid = axisMid;
            this.stickCode = stickCode;
        }
        */
        public StickDefinition(StickAxisData xAxis, StickAxisData yAxis,
            StickActionCodes stickCode)
        {
            this.xAxis = xAxis;
            this.yAxis = yAxis;
            this.stickCode = stickCode;
        }

        public StickDefinition(StickDefinition other)
        {
            this.xAxis = other.xAxis;
            this.yAxis = other.yAxis;
            this.stickCode = other.stickCode;
        }

        public static short AxisScale(int value, bool flip, StickDefinition.StickAxisData axisData)
        {
            unchecked
            {
                double temp = (value - axisData.min) * axisData.reciprocalInputResolution;
                if (flip) temp = (temp - 0.5f) * -1.0f + 0.5f;
                return (short)((axisData.max - axisData.min) * temp + axisData.min);
            }
        }
    }
}

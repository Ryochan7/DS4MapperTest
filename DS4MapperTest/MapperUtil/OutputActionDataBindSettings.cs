using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.MapperUtil
{
    public class OutputActionDataBindSettings
    {
        public const int MOUSE_X_SPEED = 50;
        public const int MOUSE_Y_SPEED = 50;

        // min full pps speed per unit
        public const int SPEED_UNIT_REFERENCE = 20;

        public int wheelXTicks = 0;
        public int wheelYTicks = 0;

        public int mouseXSpeed = MOUSE_X_SPEED;
        public int mouseYSpeed = MOUSE_Y_SPEED;
    }
}

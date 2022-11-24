using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.ActionUtil
{
    public class ActionFuncStateData
    {
        public Stopwatch elapsed = new Stopwatch();
        public bool state;
        public double axisNormValue = 0.0;
        public bool wasActive;

        public void ResetProps(bool full=false)
        {
            state = false;
            axisNormValue = 0.0;
            if (full)
            {
                wasActive = false;
            }
        }

        public void Reset(bool full=false)
        {
            ResetProps(full);

            if (elapsed.IsRunning)
            {
                elapsed.Reset();
            }
        }
    }
}

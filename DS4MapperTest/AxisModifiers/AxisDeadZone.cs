using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.AxisModifiers
{
    public class AxisDeadZone
    {
        private double deadZone;
        private double maxZone = 1.0;
        private double antiDeadZone;

        public double DeadZone
        {
            get => deadZone; set => deadZone = value;
        }

        public double MaxZone
        {
            get => maxZone; set => maxZone = value;
        }

        public double AntiDeadZone
        {
            get => antiDeadZone; set => antiDeadZone = value;
        }

        public AxisDeadZone()
        {
        }

        public AxisDeadZone(double deadZone, double maxZone, double antiDeadZone)
        {
            this.deadZone = deadZone;
            this.maxZone = maxZone;
            this.antiDeadZone = antiDeadZone;
        }

        public AxisDeadZone(AxisDeadZone other)
        {
            deadZone = other.deadZone;
            maxZone = other.maxZone;
            antiDeadZone = other.antiDeadZone;
        }

        public void CalcOutValues(int axisDir, int maxDir,
            out double axisNorm)
        {
            axisNorm = 0.0;

            bool negative = axisDir < 0;
            int currentDead = (int)(deadZone * maxDir);
            bool tempSafeZone;
            tempSafeZone = axisDir >= 0 ? (axisDir > currentDead) :
                (axisDir < currentDead);

            if (tempSafeZone)
            {
                int maxZoneDir = (int)(maxZone * maxDir);
                int valueTemp = (axisDir < 0 && axisDir < maxZoneDir) ? maxZoneDir : (axisDir > 0 && axisDir > maxZoneDir) ? maxZoneDir : axisDir;
                axisNorm = (1.0 - antiDeadZone) * ((valueTemp - currentDead) / (double)(maxZoneDir - currentDead)) + antiDeadZone;
                if (negative) axisNorm *= -1.0;
            }
        }
    }
}

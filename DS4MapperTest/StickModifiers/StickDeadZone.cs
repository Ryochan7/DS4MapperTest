using System;
//using System.Diagnostics;

namespace DS4MapperTest.StickModifiers
{
    public class StickDeadZone
    {
        public enum DeadZoneTypes : ushort
        {
            Radial,
            Bowtie,
            Axial,
        }

        public class AxialDeadSettings
        {
            public double deadZone;
            public double maxZone = 1.0;
            public double antiDeadZone;

            public void CopyTo(AxialDeadSettings other)
            {
                other.deadZone = deadZone;
                other.maxZone = maxZone;
                other.antiDeadZone = antiDeadZone;
            }
        }

        private double deadZone;
        private double maxZone = 1.0;
        private double antiDeadZone;
        private bool circleDead = true;
        private DeadZoneTypes deadZoneType;

        private AxialDeadSettings deadSettingsXAxis = new AxialDeadSettings();
        private AxialDeadSettings deadSettingsYAxis = new AxialDeadSettings();

        //private double deadZoneX;
        //private double deadZoneY;

        //private double maxZoneX = 1.0;
        //private double maxZoneY = 1.0;

        //private double antiDeadZoneX;
        //private double antiDeadZoneY;

        public bool inSafeZone;

        public bool CircleDead
        {
            get => circleDead;
            set
            {
                circleDead = value;
                if (circleDead)
                {
                    deadZoneType = DeadZoneTypes.Radial;
                }
                else
                {
                    deadZoneType = DeadZoneTypes.Axial;
                }
            }
        }

        public double DeadZone { get => deadZone; set => deadZone = value; }
        public double MaxZone { get => maxZone; set => maxZone = value; }
        public double AntiDeadZone { get => antiDeadZone; set => antiDeadZone = value; }
        public DeadZoneTypes DeadZoneType
        {
            get => deadZoneType; set => deadZoneType = value;
        }
        public AxialDeadSettings DeadSettingsXAxis
        {
            get => deadSettingsXAxis;
        }

        public AxialDeadSettings DeadSettingsYAxis
        {
            get => deadSettingsYAxis;
        }

        public StickDeadZone(double deadZone, double maxZone, double antiDeadZone)
        {
            this.deadZone = deadZone;
            this.maxZone = maxZone;
            this.antiDeadZone = antiDeadZone;
            circleDead = true;
            deadZoneType = DeadZoneTypes.Radial;
        }

        public StickDeadZone(double deadZoneX, double deadZoneY,
            double maxZoneX, double maxZoneY,
            double antiDeadZoneX, double antiDeadZoneY)
        {
            /*this.deadZoneX = deadZoneX;
            this.deadZoneY = deadZoneY;
            this.maxZoneX = maxZoneX;
            this.maxZoneY = maxZoneY;
            this.antiDeadZoneX = antiDeadZoneX;
            this.antiDeadZoneY = antiDeadZoneY;
            */
            deadSettingsXAxis.deadZone = deadZoneX;
            deadSettingsXAxis.maxZone = maxZoneX;
            deadSettingsXAxis.antiDeadZone = antiDeadZoneX;

            deadSettingsYAxis.maxZone = maxZoneY;
            deadSettingsYAxis.deadZone = deadZoneY;
            deadSettingsYAxis.antiDeadZone = antiDeadZoneY;
            circleDead = false;
            deadZoneType = DeadZoneTypes.Axial;
        }

        public StickDeadZone(StickDeadZone other)
        {
            deadZone = other.deadZone;
            //deadZoneX = other.deadZoneX;
            //deadZoneY = other.deadZoneY;
            maxZone = other.maxZone;
            //maxZoneX = other.maxZoneX;
            //maxZoneY = other.maxZoneY;
            antiDeadZone = other.antiDeadZone;
            //antiDeadZoneX = other.antiDeadZoneX;
            //antiDeadZoneY = other.antiDeadZoneY;
            other.deadSettingsXAxis.CopyTo(this.deadSettingsXAxis);
            other.deadSettingsYAxis.CopyTo(this.deadSettingsYAxis);
            circleDead = other.circleDead;
            deadZoneType = other.deadZoneType;
        }

        public void CalcOutValues(int axisXDir, int axisYDir,
            int maxDirX, int maxDirY,
            out double xNorm, out double yNorm)
        {
            bool xNegative = axisXDir < 0;
            bool yNegative = axisYDir < 0;

            if (deadZoneType == DeadZoneTypes.Radial)
            {
                double angle = Math.Atan2(-(axisYDir), axisXDir);
                double angCos = Math.Abs(Math.Cos(angle)),
                    angSin = Math.Abs(Math.Sin(angle));

                int currentDeadX = (int)(deadZone * maxDirX * angCos);
                int currentDeadY = (int)(deadZone * maxDirY * angSin);

                double stickDeadzoneSquared = (currentDeadX * currentDeadX) + (currentDeadY * currentDeadY);
                double stickSquared = Math.Pow(axisXDir, 2) + Math.Pow(axisYDir, 2);
                inSafeZone = stickSquared > stickDeadzoneSquared;

                if (inSafeZone)
                {
                    double antiDeadX = antiDeadZone * angCos;
                    double antiDeadY = antiDeadZone * angSin;

                    //int maxZoneDirX = !xNegative ? _maxZoneP : _maxZoneM;
                    //int maxZoneDirY = !yNegative ? _maxZoneP : _maxZoneM;
                    int maxZoneDirX = (int)(maxZone * maxDirX);
                    int maxZoneDirY = (int)(maxZone * maxDirY);

                    int valueX = (axisXDir < 0 && axisXDir < maxZoneDirX) ? maxZoneDirX : (axisXDir > 0 && axisXDir > maxZoneDirX) ? maxZoneDirX : axisXDir;
                    xNorm = (1.0 - antiDeadX) * ((valueX - currentDeadX) / (double)(maxZoneDirX - currentDeadX)) + antiDeadX;
                    if (xNegative) xNorm *= -1.0;
                    //Console.WriteLine("Val ({0}) Anti ({1}) Norm ({2})", valueX, antiDeadX, xNorm);

                    int valueY = (axisYDir < 0 && axisYDir < maxZoneDirY) ? maxZoneDirY : (axisYDir > 0 && axisYDir > maxZoneDirY) ? maxZoneDirY : axisYDir;
                    yNorm = (1.0 - antiDeadY) * ((valueY - currentDeadY) / (double)(maxZoneDirY - currentDeadY)) + antiDeadY;
                    if (yNegative) yNorm *= -1.0;
                }
                else
                {
                    xNorm = 0.0;
                    yNorm = 0.0;
                }
            }
            else if (deadZoneType == DeadZoneTypes.Axial)
            {
                int currentDeadX = (int)(deadSettingsXAxis.deadZone * maxDirX);
                int currentDeadY = (int)(deadSettingsXAxis.deadZone * maxDirY);

                bool tempSafeZone;
                tempSafeZone = axisXDir >= 0 ? (axisXDir > currentDeadX) :
                    (axisXDir < currentDeadX);
                if (tempSafeZone)
                {
                    inSafeZone = true;
                    double antiDeadX = deadSettingsXAxis.antiDeadZone;
                    int maxZoneDirX = (int)(deadSettingsXAxis.maxZone * maxDirX);
                    int valueX = (axisXDir < 0 && axisXDir < maxZoneDirX) ? maxZoneDirX : (axisXDir > 0 && axisXDir > maxZoneDirX) ? maxZoneDirX : axisXDir;
                    xNorm = (1.0 - antiDeadX) * ((valueX - currentDeadX) / (double)(maxZoneDirX - currentDeadX)) + antiDeadX;
                    if (xNegative) xNorm *= -1.0;
                }
                else
                {
                    xNorm = 0.0;
                }

                tempSafeZone = axisYDir >= 0 ? (axisYDir > currentDeadY) : (axisYDir < currentDeadY);
                if (tempSafeZone)
                {
                    inSafeZone = true;
                    double antiDeadY = deadSettingsYAxis.antiDeadZone;
                    int maxZoneDirY = (int)(deadSettingsYAxis.maxZone * maxDirY);
                    int valueY = (axisYDir < 0 && axisYDir < maxZoneDirY) ? maxZoneDirY : (axisYDir > 0 && axisYDir > maxZoneDirY) ? maxZoneDirY : axisYDir;
                    yNorm = (1.0 - antiDeadY) * ((valueY - currentDeadY) / (double)(maxZoneDirY - currentDeadY)) + antiDeadY;
                    if (yNegative) yNorm *= -1.0;
                }
                else
                {
                    yNorm = 0.0;
                }
            }
            else if (deadZoneType == DeadZoneTypes.Bowtie)
            {
                //Trace.WriteLine("IN HERE");

                double angle = Math.Atan2(-(axisYDir), axisXDir);
                double angCos = Math.Abs(Math.Cos(angle)),
                    angSin = Math.Abs(Math.Sin(angle));

                int currentDeadX = (int)(deadZone * maxDirX * angCos);
                int currentDeadY = (int)(deadZone * maxDirY * angSin);

                double stickDeadzoneSquared = (currentDeadX * currentDeadX) + (currentDeadY * currentDeadY);
                double stickSquared = Math.Pow(axisXDir, 2) + Math.Pow(axisYDir, 2);
                inSafeZone = stickSquared > stickDeadzoneSquared;
                //Trace.WriteLine($"{deadZone} {stickDeadzoneSquared} {Math.Sqrt(stickDeadzoneSquared)} {axisXDir} {axisYDir} {maxDirX} {maxDirY}");

                if (inSafeZone)
                {
                    //Trace.WriteLine("IN SAFE ZONE");
                    double antiDeadX = antiDeadZone * angCos;
                    double antiDeadY = antiDeadZone * angSin;

                    int maxDeadZoneAxialX = (int)(maxDirX * 0.15);
                    int minDeadZoneAxialX = (int)(maxDirX * 0.08);

                    int maxDeadZoneAxialY = (int)(maxDirY * 0.15);
                    int minDeadZoneAxialY = (int)(maxDirY * 0.08);

                    int absDX = Math.Abs(axisXDir);
                    int absDY = Math.Abs(axisYDir);

                    int signX = Math.Sign(axisXDir);
                    int signY = Math.Sign(axisYDir);

                    int maxZoneDirX = (int)(maxZone * maxDirX);
                    int maxZoneDirY = (int)(maxZone * maxDirY);

                    double tempRangeRatioX = absDX / Math.Abs((double)maxZoneDirX);
                    double tempRangeRatioY = absDY / Math.Abs((double)maxZoneDirY);

                    int axialDeadX = (int)((maxDeadZoneAxialX - minDeadZoneAxialX) *
                        Math.Min(1.0, tempRangeRatioY) + minDeadZoneAxialX);
                    int axialDeadY = (int)((maxDeadZoneAxialY - minDeadZoneAxialY) *
                        Math.Min(1.0, tempRangeRatioX) + minDeadZoneAxialY);

                    if (absDX > axialDeadX)
                    {
                        int absRadialDeadX = Math.Abs(currentDeadX);
                        int tempUseDeadX = (absRadialDeadX > axialDeadX ? absRadialDeadX : axialDeadX) * signX;
                        int valueX = (axisXDir < 0 && axisXDir < maxZoneDirX) ? maxZoneDirX : (axisXDir > 0 && axisXDir > maxZoneDirX) ? maxZoneDirX : axisXDir;
                        xNorm = (1.0 - antiDeadX) * (valueX - tempUseDeadX) / (double)(maxZoneDirX - tempUseDeadX) + antiDeadX;
                        //xNorm *= signX;
                        if (xNegative) xNorm *= -1.0;
                    }
                    else
                    {
                        xNorm = 0.0;
                    }

                    if (absDY > axialDeadY)
                    {
                        int absRadialDeadY = Math.Abs(currentDeadY);
                        int tempUseDeadY = (absRadialDeadY > axialDeadY ? absRadialDeadY : axialDeadY) * signY;
                        int valueY = (axisYDir < 0 && axisYDir < maxZoneDirY) ? maxZoneDirY : (axisYDir > 0 && axisYDir > maxZoneDirY) ? maxZoneDirY : axisYDir;
                        //double yratio = ((valueY - tempUseDeadY) / (double)(maxZoneDirY - tempUseDeadY));
                        //Trace.WriteLine($"{valueY} {tempUseDeadY} {maxZoneDirY} {yratio}");
                        yNorm = (1.0 - antiDeadY) * ((valueY - tempUseDeadY) / (double)(maxZoneDirY - tempUseDeadY)) + antiDeadY;
                        if (yNegative) yNorm *= -1.0;
                    }
                    else
                    {
                        yNorm = 0.0;
                    }

                    //Trace.WriteLine($"{xNorm} {yNorm} {antiDeadY}");
                }
                else
                {
                    xNorm = yNorm = 0.0;
                }
            }
            else
            {
                xNorm = yNorm = 0.0;
            }
        }

        public void SetAxialDeadZone(double deadzoneX, double deadzoneY)
        {
            //deadZoneX = deadzoneX;
            //deadZoneY = deadzoneY;
            deadSettingsXAxis.deadZone = deadzoneX;
            deadSettingsYAxis.deadZone = deadzoneY;
            deadZoneType = DeadZoneTypes.Axial;
        }

        public bool ShouldInterpolate()
        {
            bool result;
            if (deadZoneType == DeadZoneTypes.Radial)
            {
                result = deadZone != 0.0 || maxZone != 1.0 || antiDeadZone != 0.0;
            }
            else if (deadZoneType == DeadZoneTypes.Axial)
            {
                result = deadSettingsXAxis.deadZone != 0.0 || deadSettingsYAxis.deadZone != 0.0 ||
                    deadSettingsXAxis.maxZone != 1.0 || deadSettingsYAxis.maxZone != 1.0 ||
                    deadSettingsXAxis.antiDeadZone != 0.0 || deadSettingsYAxis.antiDeadZone != 0.0;
            }
            else
            {
                result = false;
            }

            return result;
        }

        public void Release()
        {
            inSafeZone = false;
        }
    }
}

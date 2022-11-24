using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.AxisModifiers
{
    public static class AxisOutCurve
    {
        public enum Curve : uint
        {
            Linear,
            EnhancedPrecision,
            Quadratic,
            Cubic,
            EaseoutQuad,
            EaseoutCubic,
            Bezier,
        }

        public static double CalcOutValue(Curve type, double axisNormValue)
        {
            double outputValue = 0.0;
            switch (type)
            {
                case Curve.Linear:
                    outputValue = axisNormValue;
                    break;

                case Curve.EnhancedPrecision:
                    {
                        double absVal = Math.Abs(axisNormValue);
                        double temp = outputValue;

                        if (absVal <= 0.4)
                        {
                            temp = 0.8 * absVal;
                        }
                        else if (absVal <= 0.75)
                        {
                            temp = absVal - 0.08;
                        }
                        else if (absVal > 0.75)
                        {
                            temp = (absVal * 1.32) - 0.32;
                        }

                        outputValue = (axisNormValue >= 0.0 ? 1.0 : -1.0) * temp;
                    }

                    break;

                case Curve.Quadratic:
                    outputValue = (axisNormValue >= 0.0 ? 1.0 : -1.0) *
                        axisNormValue * axisNormValue;
                    break;

                case Curve.Cubic:
                    outputValue = axisNormValue * axisNormValue * axisNormValue;
                    break;

                case Curve.EaseoutQuad:
                    {
                        double sign = axisNormValue > 0.0 ? 1.0 : -1.0;
                        double absVal = Math.Abs(axisNormValue);
                        double outputRatio = absVal * (absVal - 2.0);
                        outputValue = -1.0 * outputRatio * sign;
                    }

                    break;

                case Curve.EaseoutCubic:
                    {
                        double sign = axisNormValue > 0.0 ? 1.0 : -1.0;
                        double inner = Math.Abs(axisNormValue) - 1.0;
                        double outputRatio = inner * inner * inner + 1.0;
                        outputValue = 1.0 * outputRatio * sign;
                    }

                    break;

                case Curve.Bezier:
                    outputValue = axisNormValue;
                    break;

                default:
                    outputValue = axisNormValue;
                    break;
            }

            return outputValue;
        }
    }
}

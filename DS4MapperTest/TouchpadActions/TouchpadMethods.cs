using System;

namespace DS4MapperTest.TouchpadActions
{
    public static class TouchpadMethods
    {
        public static void RotatedCoordinates(int rotation,
            int axisXVal, int axisYVal, TouchpadDefinition touchDefinition,
            out int outXVal, out int outYVal)
        {
            double radians = (Math.PI * rotation) / 180.0;
            double sinAngle = Math.Sin(radians), cosAngle = Math.Cos(radians);

            int tempX = axisXVal - touchDefinition.xAxis.mid;
            int tempY = axisYVal - touchDefinition.yAxis.mid;

            int rotX = (int)(tempX * cosAngle - tempY * sinAngle);
            int rotY = (int)(tempX * sinAngle + tempY * cosAngle);
            outXVal = Math.Clamp(rotX + touchDefinition.xAxis.mid, touchDefinition.xAxis.min, touchDefinition.xAxis.max);
            outYVal = Math.Clamp(rotY + touchDefinition.yAxis.mid, touchDefinition.yAxis.min, touchDefinition.yAxis.max);
        }
    }
}

using System;

namespace DS4MapperTest.GyroActions
{
    public class GyroSensDefinition
    {
        public double elapsedReference;

        public double mouseCoefficient;
        public double mouseOffset;

        public int accelMinLeanX;
        public int accelMaxLeanX;
        public int accelMinLeanY;
        public int accelMaxLeanY;
        public int accelMinLeanZ;
        public int accelMaxLeanZ;

        public GyroSensDefinition()
        {
        }

        public GyroSensDefinition(GyroSensDefinition sourceDefinition)
        {
            this.elapsedReference = sourceDefinition.elapsedReference;
            this.mouseCoefficient = sourceDefinition.mouseCoefficient;
            this.mouseOffset = sourceDefinition.mouseOffset;

            this.accelMinLeanX = sourceDefinition.accelMinLeanX;
            this.accelMaxLeanX = sourceDefinition.accelMaxLeanX;
            this.accelMinLeanY = sourceDefinition.accelMinLeanY;
            this.accelMaxLeanY = sourceDefinition.accelMaxLeanY;
            this.accelMinLeanZ = sourceDefinition.accelMinLeanZ;
            this.accelMaxLeanZ = sourceDefinition.accelMaxLeanZ;
        }
    }
}

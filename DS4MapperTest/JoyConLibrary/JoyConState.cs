using DS4MapperTest.SwitchProLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.JoyConLibrary
{
    public struct JoyConMotion
    {
        public const int TIME_DELTA_MS = 5;
        public const double TIME_DELTA_SEC = TIME_DELTA_MS * 0.001;
        public const double ACCEL_UNCALIB_COEFF = 0.000244;
        public const double GYRO_UNCALIB_DEG_SEC_COEFF = 0.070;

        public const int IMU_XAXIS_IDX = 0, IMU_YAW_IDX = 0;
        public const int IMU_YAXIS_IDX = 1, IMU_PITCH_IDX = 1;
        public const int IMU_ZAXIS_IDX = 2, IMU_ROLL_IDX = 2;

        public short AccelX;
        public short AccelY;
        public short AccelZ;
        public double AccelXG, AccelYG, AccelZG;

        public short GyroYaw;
        public short GyroPitch;
        public short GyroRoll;
        public double AngGyroYaw, AngGyroPitch, AngGyroRoll;

        public void Populate(short accelX, short accelY, short accelZ,
            short gyroYaw, short gyroPitch, short gyroRoll,
            double[] accelCoeff, double[] gyroCoeff)
        {
            AccelX = accelX; AccelY = accelY; AccelZ = accelZ;
            AccelXG = accelX * accelCoeff[IMU_XAXIS_IDX]; AccelYG = accelY * accelCoeff[IMU_YAXIS_IDX]; AccelZG = accelZ * accelCoeff[IMU_ZAXIS_IDX];

            GyroYaw = gyroYaw; GyroPitch = gyroPitch; GyroRoll = gyroRoll;
            AngGyroYaw = gyroYaw * gyroCoeff[IMU_YAW_IDX]; AngGyroPitch = gyroPitch * gyroCoeff[IMU_PITCH_IDX]; AngGyroRoll = gyroRoll * gyroCoeff[IMU_ROLL_IDX];
        }
    }

    public struct JoyConState
    {
        public byte FrameTimer;
        public int Battery;
        public byte ConnInfo;
        public bool Charging;
        public double timeElapsed;

        public bool A;
        public bool B;
        public bool X;
        public bool Y;
        public bool Plus;
        public bool Minus;
        public bool Home;
        public bool Capture;
        public bool LSClick;
        public bool RSClick;
        public bool LShoulder;
        public bool RShoulder;
        public bool ZL;
        public bool ZR;
        // Side buttons for Left JoyCon
        public bool SideL;
        public bool SideR;
        // Side buttons for Right JoyCon
        public bool RightSideL;
        public bool RightSideR;

        public bool DpadLeft;
        public bool DpadRight;
        public bool DpadUp;
        public bool DpadDown;

        public ushort LX;
        public ushort LY;
        public ushort RX;
        public ushort RY;

        // Motion object used by mapper
        public JoyConMotion Motion;

        // Copied motion objects for each JoyCon side. Might use later
        public JoyConMotion MotionL;
        public JoyConMotion MotionR;

        public void RotateLSCoordinates(int rotation,
            StickActions.StickDefinition stickDefinition)
        {
            double radians = (Math.PI * rotation) / 180.0;
            double sinAngle = Math.Sin(radians), cosAngle = Math.Cos(radians);

            int tempX = LX - stickDefinition.xAxis.mid;
            int tempY = LY - stickDefinition.yAxis.mid;

            int rotX = (int)(tempX * cosAngle - tempY * sinAngle);
            int rotY = (int)(tempX * sinAngle + tempY * cosAngle);

            LX = (ushort)Math.Clamp(rotX + stickDefinition.xAxis.mid, stickDefinition.xAxis.min, stickDefinition.xAxis.max);
            LY = (ushort)Math.Clamp(rotY + stickDefinition.yAxis.mid, stickDefinition.yAxis.min, stickDefinition.yAxis.max);
        }

        public void RotateRSCoordinates(int rotation,
            StickActions.StickDefinition stickDefinition)
        {
            double radians = (Math.PI * rotation) / 180.0;
            double sinAngle = Math.Sin(radians), cosAngle = Math.Cos(radians);

            int tempX = RX - stickDefinition.xAxis.mid;
            int tempY = RY - stickDefinition.yAxis.mid;

            int rotX = (int)(tempX * cosAngle - tempY * sinAngle);
            int rotY = (int)(tempX * sinAngle + tempY * cosAngle);

            RX = (ushort)Math.Clamp(rotX + stickDefinition.xAxis.mid, stickDefinition.xAxis.min, stickDefinition.xAxis.max);
            RY = (ushort)Math.Clamp(rotY + stickDefinition.yAxis.mid, stickDefinition.yAxis.min, stickDefinition.yAxis.max);

            //double xRange = stickDefinition.xAxis.max - stickDefinition.xAxis.min;
            //double yRange = stickDefinition.yAxis.max - stickDefinition.yAxis.min;
            //double xNorm = (RX - stickDefinition.xAxis.min) / xRange;
            //double yNorm = (RY - stickDefinition.yAxis.min) / yRange;

            //xNorm -= 0.5;
            //yNorm -= 0.5;
            //double outXNorm = xNorm * cosAngle - yNorm * sinAngle;
            //double outYNorm = xNorm * sinAngle + yNorm * cosAngle;

            //xNorm += 0.5; yNorm += 0.5;
            //outXNorm += 0.5;
            //outYNorm += 0.5;

            //RX = (ushort)Math.Clamp(outXNorm * xRange + stickDefinition.xAxis.min, stickDefinition.xAxis.min, stickDefinition.xAxis.max);
            //RY = (ushort)Math.Clamp(outYNorm * yRange + stickDefinition.yAxis.min, stickDefinition.yAxis.min, stickDefinition.yAxis.max);
        }
    }
}

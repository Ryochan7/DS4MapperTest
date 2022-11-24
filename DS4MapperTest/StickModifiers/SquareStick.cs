using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.StickModifiers
{
    public struct Vector2
    {
        public double x;
        public double y;

        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class SquareStick
    {
        public Vector2 current;
        public Vector2 squared;

        public SquareStick()
        {
            current = new Vector2(0.0, 0.0);
            squared = new Vector2(0.0, 0.0);
        }

        // Modification of squared stick routine documented
        // at http://theinstructionlimit.com/squaring-the-thumbsticks
        public void CircleToSquare(double roundness)
        {
            const double PiOverFour = Math.PI / 4.0;

            // Determine the theta angle
            double angle = Math.Atan2(current.y, -current.x);
            angle += Math.PI;
            double cosAng = Math.Cos(angle);
            // Scale according to which wall we're clamping to
            // X+ wall
            if (angle <= PiOverFour || angle > 7.0 * PiOverFour)
            {
                double tempVal = 1.0 / cosAng;
                //Trace.WriteLine("1 ANG: {0} | TEMP: {1}", angle, tempVal);
                squared.x = current.x * tempVal;
                squared.y = current.y * tempVal;
            }
            // Y+ wall
            else if (angle > PiOverFour && angle <= 3.0 * PiOverFour)
            {
                double tempVal = 1.0 / Math.Sin(angle);
                //Trace.WriteLine("2 ANG: {0} | TEMP: {1}", angle, tempVal);
                squared.x = current.x * tempVal;
                squared.y = current.y * tempVal;
            }
            // X- wall
            else if (angle > 3.0 * PiOverFour && angle <= 5.0 * PiOverFour)
            {
                double tempVal = -1.0 / cosAng;
                //Trace.WriteLine("3 ANG: {0} | TEMP: {1}", angle, tempVal);
                squared.x = current.x * tempVal;
                squared.y = current.y * tempVal;
            }
            // Y- wall
            else if (angle > 5.0 * PiOverFour && angle <= 7.0 * PiOverFour)
            {
                double tempVal = -1.0 / Math.Sin(angle);
                //Trace.WriteLine("4 ANG: {0} | TEMP: {1}", angle, tempVal);
                squared.x = current.x * tempVal;
                squared.y = current.y * tempVal;
            }
            else return;

            //double lengthOld = Math.Sqrt((x * x) + (y * y));
            double length = current.x / cosAng;
            //Trace.WriteLine("LENGTH TEST ({0}) ({1}) {2}", lengthOld, length, (lengthOld == length).ToString());
            double factor = Math.Pow(length, roundness);
            //double ogX = current.x, ogY = current.y;
            current.x += (squared.x - current.x) * factor;
            current.y += (squared.y - current.y) * factor;
            //Trace.WriteLine("INPUT: {0} {1} | {2} {3} | {4} {5} | {6} {7}",
            //    ogX, ogY, current.x, current.y, squared.x, squared.y, length, factor);
        }
    }
}

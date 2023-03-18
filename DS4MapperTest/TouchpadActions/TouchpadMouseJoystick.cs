using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sensorit.Base;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.StickModifiers;

namespace DS4MapperTest.TouchpadActions
{
    public class TouchpadMouseJoystick : TouchpadMapAction
    {
        public class PropertyKeyStrings
        {
            public const string NAME = "Name";
            public const string DEAD_ZONE = "DeadZone";
            public const string MAX_ZONE = "MaxZone";
            public const string ANTIDEAD_ZONE_X = "AntiDeadZoneX";
            public const string ANTIDEAD_ZONE_Y = "AntiDeadZoneY";
            public const string OUTPUT_CURVE = "OutputCurve";
            public const string OUTPUT_STICK = "OutputStick";
            public const string TRACKBALL_MODE = "Trackball";
            public const string TRACKBALL_FRICTION = "TrackballFriction";
            public const string ROTATION = "Rotation";
            public const string INVERT_X = "InvertX";
            public const string INVERT_Y = "InvertY";
            public const string JITTER_COMPENSATION = "JitterCompensation";
            public const string VERTICAL_SCALE = "VerticalScale";
            public const string SMOOTHING_ENABLED = "SmoothingEnabled";
            public const string SMOOTHING_FILTER = "SmoothingFilter";
            //public const string MAX_OUTPUT = "MaxOutput";
            //public const string MAX_OUTPUT_ENABLED = "MaxOutputEnabled";
            //public const string SQUARE_STICK_ENABLED = "SquareStickEnabled";
            //public const string SQUARE_STICK_ROUNDNESS = "SquareStickRoundness";
        }

        private HashSet<string> fullPropertySet = new HashSet<string>()
        {
            PropertyKeyStrings.NAME,
            PropertyKeyStrings.DEAD_ZONE,
            PropertyKeyStrings.MAX_ZONE,
            PropertyKeyStrings.ANTIDEAD_ZONE_X,
            PropertyKeyStrings.ANTIDEAD_ZONE_Y,
            PropertyKeyStrings.OUTPUT_CURVE,
            PropertyKeyStrings.OUTPUT_STICK,
            PropertyKeyStrings.TRACKBALL_MODE,
            PropertyKeyStrings.TRACKBALL_FRICTION,
            PropertyKeyStrings.INVERT_X,
            PropertyKeyStrings.INVERT_Y,
            PropertyKeyStrings.ROTATION,
            PropertyKeyStrings.VERTICAL_SCALE,
            PropertyKeyStrings.JITTER_COMPENSATION,
            PropertyKeyStrings.SMOOTHING_ENABLED,
            PropertyKeyStrings.SMOOTHING_FILTER,
            //PropertyKeyStrings.MAX_OUTPUT_ENABLED,
            //PropertyKeyStrings.MAX_OUTPUT,
            //PropertyKeyStrings.SQUARE_STICK_ENABLED,
            //PropertyKeyStrings.SQUARE_STICK_ROUNDNESS,
        };

        public struct SmoothingFilterSettings
        {
            public const double DEFAULT_MIN_CUTOFF = 0.4;
            public const double DEFAULT_BETA = 0.6;

            public OneEuroFilter filterX;
            public OneEuroFilter filterY;

            public double minCutOff;
            public double beta;

            public void Init()
            {
                minCutOff = DEFAULT_MIN_CUTOFF;
                beta = DEFAULT_BETA;

                filterX = new OneEuroFilter(minCutoff: minCutOff,
                    beta: beta);
                filterY = new OneEuroFilter(minCutoff: minCutOff,
                    beta: beta);
            }

            public void ResetFilters()
            {
                filterX.Reset();
                filterY.Reset();
            }

            public void UpdateSmoothingFilters()
            {
                filterX.MinCutoff = minCutOff;
                filterX.Beta = beta;
                filterX.Reset();

                filterY.MinCutoff = minCutOff;
                filterY.Beta = beta;
                filterY.Reset();
            }
        }

        public struct TouchpadMouseJoystickParams
        {
            public const bool JITTER_COMPENSATION_DEFAULT = false;

            public int deadZone;
            public int maxZone;
            public double antiDeadzoneX;
            public double antiDeadzoneY;
            public StickOutCurve.Curve outputCurve;
            public bool trackballEnabled;
            public int trackballFriction;
            public int TrackballFriction
            {
                get => trackballFriction;
                set
                {
                    trackballFriction = value;
                    TrackballFrictionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            public event EventHandler TrackballFrictionChanged;

            public bool invertX;
            public bool invertY;
            public double verticalScale;
            public StickActionCodes outputStick;
            public StickActionCodes OutputStick
            {
                get => outputStick;
                set
                {
                    outputStick = value;
                    OutputStickChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            public event EventHandler OutputStickChanged;

            public int rotation;
            public bool jitterCompensation;

            public bool smoothing;
            public SmoothingFilterSettings smoothingFilterSettings;
        }

        private const string ACTION_TYPE_NAME = "TouchMouseJoystickAction";

        private const int DEFAULT_DEADZONE = 70;
        private const int DEFAULT_MAXZONE = 430;
        private const double DEFAULT_ANTI_DEADZONE_X = 0.30;
        private const double DEFAULT_ANTI_DEADZONE_Y = 0.30;
        private const double DEFAULT_VERTICAL_SCALE = 1.0;
        private const StickActionCodes DEFAULT_OUTPUT_STICK = StickActionCodes.RS;
        private const StickOutCurve.Curve DEFAULT_OUTPUT_CURVE = StickOutCurve.Curve.Linear;
        private const bool DEFAULT_TRACKBALL_ENABLED = true;
        public const bool DEFAULT_SMOOTHING_ENABLED = true;

        private OutputActionData outputAction;
        public OutputActionData OutputAction
        {
            get => outputAction;
        }

        private double xNorm = 0.0, yNorm = 0.0;
        //private double xMotion;
        //private double yMotion;
        private int previousDX, previousDY;

        private const int TRACKBALL_INIT_FRICTION = 10;
        private const int TRACKBALL_JOY_FRICTION = 4;
        private const int TRACKBALL_MASS = 45;
        private const double TRACKBALL_RADIUS = 0.0245;
        private const double TOUCHPAD_MOUSE_OFFSET = 0.375;
        //private const double TOUCHPAD_COEFFICIENT = 0.012;
        private const double TOUCHPAD_COEFFICIENT = 0.012 * 1.1;

        private double TRACKBALL_INERTIA = 2.0 * (TRACKBALL_MASS * TRACKBALL_RADIUS * TRACKBALL_RADIUS) / 5.0;
        //private double TRACKBALL_SCALE = 0.000023;
        private double TRACKBALL_SCALE = 0.000023;
        private const int TRACKBALL_BUFFER_LEN = 8;

        private class TrackballVelData
        {
            public double[] trackballXBuffer = new double[TRACKBALL_BUFFER_LEN];
            public double[] trackballYBuffer = new double[TRACKBALL_BUFFER_LEN];
            public int trackballBufferTail = 0;
            public int trackballBufferHead = 0;
            public double trackballAccel = 0.0;
            public double trackballXVel = 0.0;
            public double trackballYVel = 0.0;
            public bool trackballActive = false;
            public double trackballDXRemain = 0.0;
            public double trackballDYRemain = 0.0;

            public void PurgeData()
            {
                Array.Clear(trackballXBuffer, 0, TRACKBALL_BUFFER_LEN);
                Array.Clear(trackballYBuffer, 0, TRACKBALL_BUFFER_LEN);
                trackballXVel = 0.0;
                trackballYVel = 0.0;
                trackballActive = false;
                trackballBufferTail = 0;
                trackballBufferHead = 0;
                trackballDXRemain = 0.0;
                trackballDYRemain = 0.0;
            }
        }

        private TrackballVelData trackData;

        private bool useParentTrackFriction;

        private TouchpadMouseJoystickParams mStickParams;
        public ref TouchpadMouseJoystickParams MStickParams
        {
            get => ref mStickParams;
        }

        private bool useParentSmoothingFilter;

        public TouchpadMouseJoystick()
        {
            actionTypeName = ACTION_TYPE_NAME;
            this.outputAction = new OutputActionData(OutputActionData.ActionType.GamepadControl,
                DEFAULT_OUTPUT_STICK);

            trackData = new TrackballVelData();
            trackData.trackballAccel = TRACKBALL_RADIUS * TRACKBALL_INIT_FRICTION / TRACKBALL_INERTIA;

            mStickParams = new TouchpadMouseJoystickParams()
            {
                deadZone = DEFAULT_DEADZONE,
                maxZone = DEFAULT_MAXZONE,
                antiDeadzoneX = DEFAULT_ANTI_DEADZONE_X,
                antiDeadzoneY = DEFAULT_ANTI_DEADZONE_Y,
                trackballEnabled = DEFAULT_TRACKBALL_ENABLED,
                trackballFriction = TRACKBALL_INIT_FRICTION,
                verticalScale = DEFAULT_VERTICAL_SCALE,
                outputCurve = DEFAULT_OUTPUT_CURVE,
                jitterCompensation = TouchpadMouseJoystickParams.JITTER_COMPENSATION_DEFAULT,
                smoothing = DEFAULT_SMOOTHING_ENABLED,
            };

            mStickParams.smoothingFilterSettings.Init();
            mStickParams.TrackballFrictionChanged += MStickParams_TrackballFrictionChanged;
            mStickParams.OutputStickChanged += MStickParams_OutputStickChanged;
        }

        private void MStickParams_OutputStickChanged(object sender, EventArgs e)
        {
            outputAction.StickCode = mStickParams.outputStick;
        }

        private void MStickParams_TrackballFrictionChanged(object sender, EventArgs e)
        {
            CalcTrackAccel();
        }

        public override void Prepare(Mapper mapper, ref TouchEventFrame touchFrame, bool alterState = true)
        {
            //Trace.WriteLine($"IN PREPARE {touchFrame.X} {touchFrame.Y}");
            //Trace.WriteLine($"IN PREPARE {touchFrame.X} {touchFrame.Y}");

            double previousXNorm = xNorm;
            double previousYNorm = yNorm;
            int axisXVal = touchFrame.X;
            int axisYVal = touchFrame.Y;
            if (mStickParams.rotation != 0)
            {
                TouchpadMethods.RotatedCoordinates(mStickParams.rotation, axisXVal, axisYVal,
                    touchpadDefinition, out axisXVal, out axisYVal);
            }

            if (mStickParams.trackballEnabled)
            {
                TrackballMouseJoystickProcess(mapper, ref touchFrame);
            }
            else if (!mStickParams.trackballEnabled && touchFrame.Touch)
            {
                ref TouchEventFrame previousTouchFrame =
                    ref mapper.GetPreviousTouchEventFrame(touchpadDefinition.touchCode);

                if (previousTouchFrame.Touch)
                {
                    // Process normal mouse
                    ProcessTouchMouseJoystick(ref touchFrame, ref previousTouchFrame);
                }
            }

            //if (xNorm != 0.0 || yNorm != 0.0)
            //{
            //    active = activeEvent = true;
            //}
            ////else if (previousXNorm != xNorm || previousYNorm != yNorm)
            ////{
            ////    active = activeEvent = true;
            ////}
            //else
            //{
            //    active = activeEvent = false;
            //}

            if (touchFrame.Touch || trackData.trackballActive)
            {
                active = activeEvent = true;
            }
            else
            {
                if (mStickParams.smoothing)
                {
                    mStickParams.smoothingFilterSettings.filterX.Filter(0.0, mapper.CurrentRate);
                    mStickParams.smoothingFilterSettings.filterY.Filter(0.0, mapper.CurrentRate);
                }

                // No matter what, flag mapper state struct as dirty
                mapper.IntermediateStateRef.Dirty = true;
                active = activeEvent = false;
            }
        }

        public override void Event(Mapper mapper)
        {
            double outXNorm = xNorm;
            double outYNorm = yNorm;

            double tempXNorm = outXNorm;
            double tempYNorm = outYNorm;

            if (mStickParams.smoothing)
            {
                // Adjust sensitivity to work around rounding in filter method
                outXNorm *= 1.0005;
                outYNorm *= 1.0005;
                tempXNorm = mStickParams.smoothingFilterSettings.filterX.Filter(outXNorm, mapper.CurrentRate);
                tempYNorm = mStickParams.smoothingFilterSettings.filterY.Filter(outYNorm, mapper.CurrentRate);

                // Filter does not go back to absolute zero for reasons. Check
                // for low number and reset to zero
                if (Math.Abs(tempXNorm) < 0.0001) tempXNorm = 0.0;
                if (Math.Abs(tempYNorm) < 0.0001) tempYNorm = 0.0;

                // Need to check bounds again
                tempXNorm = Math.Clamp(tempXNorm, -1.0, 1.0);
                tempYNorm = Math.Clamp(tempYNorm, -1.0, 1.0);

                //Trace.WriteLine($"OUTPUT: {tempXNorm} {tempYNorm} | BE {outXNorm}");
            }

            mapper.GamepadFromStickInput(outputAction, tempXNorm, tempYNorm);

            if (tempXNorm != 0.0 || tempYNorm != 0.0)
            {
                active = true;
            }
            else
            {
                active = false;
            }

            activeEvent = false;
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            xNorm = 0.0;
            yNorm = 0.0;
            previousDX = previousDY = 0;

            mapper.GamepadFromStickInput(outputAction, 0.0, 0.0);

            PurgeTrackballData();
            mStickParams.smoothingFilterSettings.ResetFilters();
            active = activeEvent = false;
        }

        public override void SoftRelease(Mapper mapper, MapAction checkAction, bool resetState = true)
        {
            if (active)
            {
                TouchpadMouseJoystick tempMouseJoyAction = checkAction as TouchpadMouseJoystick;
                if (parentAction != null && !useParentTrackFriction)
                {
                    // Re-evaluate trackball friction with parent action setting
                    tempMouseJoyAction.CalcTrackAccel();
                }

                if (parentAction != null &&
                    mStickParams.trackballEnabled != tempMouseJoyAction.mStickParams.trackballEnabled)
                {
                    trackData.PurgeData();
                }
            }

            if (!useParentSmoothingFilter)
            {
                mStickParams.smoothingFilterSettings.ResetFilters();
            }

            active = activeEvent = false;
            previousDX = previousDY = 0;
        }

        private void PurgeTrackballData()
        {
            Array.Clear(trackData.trackballXBuffer, 0, TRACKBALL_BUFFER_LEN);
            Array.Clear(trackData.trackballYBuffer, 0, TRACKBALL_BUFFER_LEN);
            trackData.trackballXVel = 0.0;
            trackData.trackballYVel = 0.0;
            trackData.trackballActive = false;
            trackData.trackballBufferTail = 0;
            trackData.trackballBufferHead = 0;
            trackData.trackballDXRemain = 0.0;
            trackData.trackballDYRemain = 0.0;
        }

        private void TrackballMouseJoystickProcess(Mapper mapper, ref TouchEventFrame touchFrame)
        {
            ref TouchEventFrame previousTouchFrame =
                ref mapper.GetPreviousTouchEventFrame(touchpadDefinition.touchCode);

            if (touchFrame.Touch && !previousTouchFrame.Touch)
            {
                if (trackData.trackballActive)
                {
                    //Trace.WriteLine("CHECKING HERE");
                }

                // Initial touch
                Array.Clear(trackData.trackballXBuffer, 0, TRACKBALL_BUFFER_LEN);
                Array.Clear(trackData.trackballYBuffer, 0, TRACKBALL_BUFFER_LEN);
                trackData.trackballXVel = 0.0;
                trackData.trackballYVel = 0.0;
                trackData.trackballActive = false;
                trackData.trackballBufferTail = 0;
                trackData.trackballBufferHead = 0;
                trackData.trackballDXRemain = 0.0;
                trackData.trackballDYRemain = 0.0;

                //Trace.WriteLine("INITIAL");
            }
            else if (touchFrame.Touch && previousTouchFrame.Touch)
            {
                // Process normal mouse
                //RightTouchMouse(ref current, ref previous);
                //Trace.WriteLine("NORMAL");
                ProcessTouchMouseJoystick(ref touchFrame, ref previousTouchFrame);

            }
            else if (!touchFrame.Touch && previousTouchFrame.Touch)
            {
                // Initially released. Calculate velocity and start Trackball
                double currentWeight = 1.0;
                double finalWeight = 0.0;
                double x_out = 0.0, y_out = 0.0;
                int idx = -1;
                for (int i = 0; i < TRACKBALL_BUFFER_LEN && idx != trackData.trackballBufferHead; i++)
                {
                    idx = (trackData.trackballBufferTail - i - 1 + TRACKBALL_BUFFER_LEN) % TRACKBALL_BUFFER_LEN;
                    x_out += trackData.trackballXBuffer[idx] * currentWeight;
                    y_out += trackData.trackballYBuffer[idx] * currentWeight;
                    finalWeight += currentWeight;
                    currentWeight *= 1.0;
                }

                x_out /= finalWeight;
                trackData.trackballXVel = x_out;
                y_out /= finalWeight;
                trackData.trackballYVel = y_out;

                double dist = Math.Sqrt(trackData.trackballXVel * trackData.trackballXVel + trackData.trackballYVel * trackData.trackballYVel);
                if (dist >= 1.0)
                {
                    trackData.trackballActive = true;

                    //Trace.WriteLine($"START TRACK {dist}");
                    ProcessTrackballJoystickFrame(ref touchFrame);
                }
            }
            else if (!touchFrame.Touch && trackData.trackballActive)
            {
                //Trace.WriteLine("CONTINUE TRACK");
                // Trackball Running
                ProcessTrackballJoystickFrame(ref touchFrame);
            }
            else if (!touchFrame.Touch)
            {
                xNorm = yNorm = 0.0;
            }
        }

        private void ProcessTouchMouseJoystick(ref TouchEventFrame touchFrame, ref TouchEventFrame previousFrame)
        {
            int dx, dy;
            if (!touchFrame.passDelta)
            {
                dx = touchFrame.X - previousFrame.X;
                dy = -(touchFrame.Y - previousFrame.Y);
            }
            else
            {
                dx = previousDX;
                dy = previousDY;
            }
            
            previousDX = dx; previousDY = dy;
            //int rawDeltaX = dx, rawDeltaY = dy;

            //Trace.WriteLine(String.Format("DELTA X: {0} Y: {1}", dx, dy));

            if (mStickParams.trackballEnabled)
            {
                // Fill trackball entry
                int iIndex = trackData.trackballBufferTail;
                double trackballScale = touchpadDefinition.trackballScale;
                trackData.trackballXBuffer[iIndex] = (dx * trackballScale) / touchFrame.timeElapsed;
                trackData.trackballYBuffer[iIndex] = (dy * trackballScale) / touchFrame.timeElapsed;
                trackData.trackballBufferTail = (iIndex + 1) % TRACKBALL_BUFFER_LEN;
                if (trackData.trackballBufferHead == trackData.trackballBufferTail)
                    trackData.trackballBufferHead = (trackData.trackballBufferHead + 1) % TRACKBALL_BUFFER_LEN;
            }

            TouchMouseJoystickPad(dx, -dy, ref touchFrame);
        }

        private void ProcessTrackballJoystickFrame(ref TouchEventFrame touchFrame)
        {
            double tempAngle = Math.Atan2(-trackData.trackballYVel, trackData.trackballXVel);
            double normX = Math.Abs(Math.Cos(tempAngle));
            double normY = Math.Abs(Math.Sin(tempAngle));
            int signX = Math.Sign(trackData.trackballXVel);
            int signY = Math.Sign(trackData.trackballYVel);

            double trackXvDecay = Math.Min(Math.Abs(trackData.trackballXVel), trackData.trackballAccel * touchFrame.timeElapsed * normX);
            double trackYvDecay = Math.Min(Math.Abs(trackData.trackballYVel), trackData.trackballAccel * touchFrame.timeElapsed * normY);
            double xVNew = trackData.trackballXVel - (trackXvDecay * signX);
            double yVNew = trackData.trackballYVel - (trackYvDecay * signY);
            double trackballScale = touchpadDefinition.trackballScale;
            double xMotion = (xVNew * touchFrame.timeElapsed) / trackballScale;
            double yMotion = (yVNew * touchFrame.timeElapsed) / trackballScale;
            if (xMotion != 0.0)
            {
                xMotion += trackData.trackballDXRemain;
            }
            else
            {
                trackData.trackballDXRemain = 0.0;
            }

            int dx = (int)xMotion;
            trackData.trackballDXRemain = xMotion - dx;

            if (yMotion != 0.0)
            {
                yMotion += trackData.trackballDYRemain;
            }
            else
            {
                trackData.trackballDYRemain = 0.0;
            }

            int dy = (int)yMotion;
            trackData.trackballDYRemain = yMotion - dy;

            trackData.trackballXVel = xVNew;
            trackData.trackballYVel = yVNew;

            //Trace.WriteLine(string.Format("DX: {0} DY: {1}", dx, dy));

            if (dx == 0 && dy == 0)
            {
                trackData.trackballActive = false;
                //Trace.WriteLine("ENDING TRACK");
                xNorm = yNorm = 0.0;
            }
            else
            {
                TouchMouseJoystickPad(dx, -dy, ref touchFrame);
            }
        }


        private void TouchMouseJoystickPad(int dx, int dy,
            ref TouchEventFrame touchFrame)
        {
            //const int deadZone = 70;
            int deadZone = mStickParams.deadZone;
            //const int maxDeadZoneAxial = 100;
            //const int minDeadZoneAxial = 20;
            int maxDeadZoneAxial = (int)(mStickParams.maxZone * 0.24);
            int minDeadZoneAxial = (int)(mStickParams.maxZone * 0.02);

            //Trace.WriteLine(touchFrame.Y);

            int maxDirX = dx >= 0 ? 32767 : -32768;
            int maxDirY = dy >= 0 ? 32767 : -32768;

            double tempAngle = Math.Atan2(dy, dx);
            double normX = Math.Abs(Math.Cos(tempAngle));
            double normY = Math.Abs(Math.Sin(tempAngle));
            int signX = Math.Sign(dx);
            int signY = Math.Sign(dy);

            //double timeElapsed = touchFrame.timeElapsed;
            // Base speed 8 ms
            //double tempDouble = timeElapsed * 125.0;

            //int maxValX = signX * 430;
            //int maxValY = signY * 430;

            int maxValX = signX * mStickParams.maxZone;
            int maxValY = signY * mStickParams.maxZone;

            double xratio = 0.0, yratio = 0.0;
            //double antiX = 0.30 * normX;
            //double antiY = 0.30 * normY;
            double antiX = mStickParams.antiDeadzoneX * normX;
            double antiY = mStickParams.antiDeadzoneY * normY;

            int deadzoneX = (int)Math.Abs(normX * deadZone);
            int radialDeadZoneY = (int)(Math.Abs(normY * deadZone));
            //int deadzoneY = (int)(Math.Abs(normY * deadZone));

            int absDX = Math.Abs(dx);
            int absDY = Math.Abs(dy);

            // Check for radial dead zone first
            double mag = (dx * dx) + (dy * dy);
            if (mag <= (deadZone * deadZone))
            {
                dx = 0;
                dy = 0;
            }
            // Past radial. Check for bowtie
            else
            {
                //Trace.WriteLine($"X ({dx}) | Y ({dy})");

                // X axis calculated with scaled radial
                /*if (absDX > deadzoneX)
                {
                    dx -= signX * deadzoneX;
                    //dx = (dx < 0 && dx < maxValX) ? maxValX :
                    //    (dx > 0 && dx > maxValX) ? maxValX : dx;
                }
                else
                {
                    dx = 0;
                }

                if (absDY > radialDeadZoneY)
                {
                    dy -= signY * radialDeadZoneY;
                    //dy = (dy < 0 && dy < maxValY) ? maxValY :
                    //    (dy > 0 && dy > maxValY) ? maxValY : dy;
                }
                else
                {
                    dy = 0;
                }
                */

                // Need to adjust Y axis dead zone based on X axis input. Bowtie
                //int deadzoneY = Math.Max(radialDeadZoneY,
                //    (int)(Math.Min(1.0, absDX / (double)maxValX) * maxDeadZoneAxialY));
                double tempRangeRatioX = absDX / Math.Abs((double)maxValX);
                double tempRangeRatioY = absDY / Math.Abs((double)maxValY);

                int axialDeadX = (int)((maxDeadZoneAxial - minDeadZoneAxial) *
                    Math.Min(1.0, tempRangeRatioY) + minDeadZoneAxial);
                int deadzoneY = (int)((maxDeadZoneAxial - minDeadZoneAxial) *
                    Math.Min(1.0, tempRangeRatioX) + minDeadZoneAxial);

                //Trace.WriteLine($"Axial {axialDeadX} DX: {dx}");
                //Trace.WriteLine(deadzoneY);
                //int deadzoneY = Math.Min(maxValX, Math.Abs(dx)) * maxDeadZoneAxialY;
                //if (absDY > radialDeadZoneY)
                //{
                //    dy -= signY * radialDeadZoneY;
                //    dy = (dy < 0 && dy < maxValY) ? maxValY :
                //        (dy > 0 && dy > maxValY) ? maxValY : dy;
                //}
                //else if (absDY > deadzoneY)

                if (Math.Abs(dx) > axialDeadX)
                {
                    int tempUseDeadX = deadzoneX > axialDeadX ? deadzoneX : axialDeadX;
                    dx -= signX * tempUseDeadX;
                    double newMaxValX = Math.Abs(maxValX) - tempUseDeadX;
                    double scaleX = Math.Abs(dx) / (double)(newMaxValX);
                    dx = (int)(maxValX * scaleX);

                    dx = (dx < 0 && dx < maxValX) ? maxValX :
                        (dx > 0 && dx > maxValX) ? maxValX : dx;
                    //Trace.WriteLine($"{scaleX} {dx}");
                }
                else
                {
                    dx = 0;
                    //Trace.WriteLine("dx zero");
                }

                if (Math.Abs(dy) > deadzoneY)
                {
                    int tempUseDeadY = radialDeadZoneY > deadzoneY ? radialDeadZoneY : deadzoneY;
                    dy -= signY * tempUseDeadY;
                    double newMaxValY = Math.Abs(maxValY) - tempUseDeadY;
                    double scaleY = Math.Abs(dy) / (double)(newMaxValY);
                    dy = (int)(maxValY * scaleY);

                    dy = (dy < 0 && dy < maxValY) ? maxValY :
                        (dy > 0 && dy > maxValY) ? maxValY : dy;
                }
                else
                {
                    dy = 0;
                }

                /*
                if (absDY > deadzoneY)
                {
                    int newMaxValY = signY * (Math.Abs(maxValY) - deadzoneY);
                    dy -= signY * deadzoneY;
                    //dy = (dy < 0 && dy < maxValY) ? maxValY :
                    //    (dy > 0 && dy > maxValY) ? maxValY : dy;
                    dy = (dy < 0 && dy < newMaxValY) ? newMaxValY :
                          (dy > 0 && dy > newMaxValY) ? newMaxValY : dy;

                    double scaleY;
                    if (dy == newMaxValY)
                    {
                        scaleY = 1.0;
                    }
                    else
                    {
                        scaleY = (double)(dy - 0.0) / (double)(newMaxValY - 0.0);
                    }
                    //scaleY = (Math.Abs(newMaxValY) - Math.Abs(dy)) /
                    //    (double)(Math.Abs(maxValY) - 0);
                    dy = (int)(scaleY * maxValY);

                    //Trace.WriteLine($"SCALE: ({scaleY}) | NEW: ({newMaxValY}) | DY({dy})");
                }
                else
                {
                    dy = 0;
                }
                //*/
            }

            if (mStickParams.jitterCompensation)
            {
                // Possibly expose threshold later
                const double threshold = 2;
                const float thresholdF = (float)threshold;

                double absX = Math.Abs(dx);
                if (absX <= normX * threshold)
                {
                    dx = (int)(signX * Math.Pow(absX / thresholdF, 1.408) * threshold);
                }

                double absY = Math.Abs(dy);
                if (absY <= normY * threshold)
                {
                    dy = (int)(signY * Math.Pow(absY / thresholdF, 1.408) * threshold);
                }
            }

            if (dx != 0) xratio = dx / (double)maxValX;
            if (dy != 0) yratio = dy / (double)maxValY;

            if (mStickParams.outputCurve != StickOutCurve.Curve.Linear)
            {
                StickOutCurve.CalcOutValue(mStickParams.outputCurve, xratio, yratio,
                    out xratio, out yratio);
            }

            if (mStickParams.verticalScale != 1.0)
            {
                yratio = Math.Clamp(yratio * mStickParams.verticalScale, 0.0, 1.0);
            }

            double maxOutRatio = 1.0;
            double maxOutXRatio = Math.Abs(Math.Cos(tempAngle)) * maxOutRatio;
            double maxOutYRatio = Math.Abs(Math.Sin(tempAngle)) * maxOutRatio;

            //Trace.WriteLine($"{maxOutXRatio} {maxOutYRatio}");
            // Expand output a bit
            maxOutXRatio = Math.Min(maxOutXRatio / 0.96, 1.0);
            // Expand output a bit
            maxOutYRatio = Math.Min(maxOutYRatio / 0.96, 1.0);

            xratio = Math.Min(Math.Max(xratio, 0.0), maxOutXRatio);
            yratio = Math.Min(Math.Max(yratio, 0.0), maxOutYRatio);

            //Trace.WriteLine($"X ({dx}) | Y ({dy})");

            //double maxOutRatio = 0.98;
            //// Expand output a bit. Likely not going to get a straight line with Gyro
            //double maxOutXRatio = Math.Min(normX / 1.0, 1.0) * maxOutRatio;
            //double maxOutYRatio = Math.Min(normY / 1.0, 1.0) * maxOutRatio;

            //xratio = Math.Min(Math.Max(xratio, 0.0), maxOutXRatio);
            //yratio = Math.Min(Math.Max(yratio, 0.0), maxOutYRatio);

            // QuadOut
            //xratio = 1.0 - ((1.0 - xratio) * (1.0 - xratio));
            //yratio = 1.0 - ((1.0 - yratio) * (1.0 - yratio));

            double xNorm = 0.0, yNorm = 0.0;
            if (xratio != 0.0)
            {
                xNorm = (1.0 - antiX) * xratio + antiX;
            }

            if (yratio != 0.0)
            {
                yNorm = (1.0 - antiY) * yratio + antiY;
            }

            this.xNorm = xNorm * signX;
            this.yNorm = yNorm * signY;

            if (mStickParams.invertX)
            {
                this.xNorm *= -1.0;
            }

            if (mStickParams.invertY)
            {
                this.yNorm *= -1.0;
            }

            //Trace.WriteLine($"OutX ({this.xNorm}) | OutY ({this.yNorm})");
            //short axisXOut = (short)filterX.Filter(xNorm * maxDirX,
            //    1.0 / currentRate);
            //short axisYOut = (short)filterY.Filter(yNorm * maxDirY,
            //    1.0 / currentRate);

            //Trace.WriteLine($"OutX ({axisXOut}) | OutY ({axisYOut})");

            //xbox.RightThumbX = axisXOut;
            //xbox.RightThumbY = axisYOut;
        }

        public override void SoftCopyFromParent(TouchpadMapAction parentAction)
        {
            if (parentAction is TouchpadMouseJoystick tempMouseJoyAction)
            {
                base.SoftCopyFromParent(parentAction);

                this.parentAction = parentAction;
                mappingId = tempMouseJoyAction.mappingId;

                this.touchpadDefinition = new TouchpadDefinition(tempMouseJoyAction.touchpadDefinition);

                tempMouseJoyAction.NotifyPropertyChanged += TempMouseJoyAction_NotifyPropertyChanged;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                foreach (string parentPropType in useParentProList)
                {
                    switch(parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = tempMouseJoyAction.name;
                            break;
                        case PropertyKeyStrings.DEAD_ZONE:
                            mStickParams.deadZone = tempMouseJoyAction.mStickParams.deadZone;
                            break;
                        case PropertyKeyStrings.MAX_ZONE:
                            mStickParams.maxZone = tempMouseJoyAction.mStickParams.maxZone;
                            break;
                        case PropertyKeyStrings.ANTIDEAD_ZONE_X:
                            mStickParams.antiDeadzoneX = tempMouseJoyAction.mStickParams.antiDeadzoneX;
                            break;
                        case PropertyKeyStrings.ANTIDEAD_ZONE_Y:
                            mStickParams.antiDeadzoneY = tempMouseJoyAction.mStickParams.antiDeadzoneY;
                            break;
                        case PropertyKeyStrings.OUTPUT_STICK:
                            outputAction.StickCode = tempMouseJoyAction.outputAction.StickCode;
                            break;
                        case PropertyKeyStrings.OUTPUT_CURVE:
                            mStickParams.outputCurve = tempMouseJoyAction.mStickParams.outputCurve;
                            break;
                        case PropertyKeyStrings.TRACKBALL_MODE:
                            mStickParams.trackballEnabled = tempMouseJoyAction.mStickParams.trackballEnabled;
                            // Copy parent ref
                            trackData = tempMouseJoyAction.trackData;
                            break;
                        case PropertyKeyStrings.TRACKBALL_FRICTION:
                            mStickParams.trackballFriction = tempMouseJoyAction.mStickParams.trackballFriction;
                            useParentTrackFriction = true;
                            CalcTrackAccel();
                            break;
                        case PropertyKeyStrings.INVERT_X:
                            mStickParams.invertX = tempMouseJoyAction.mStickParams.invertX;
                            break;
                        case PropertyKeyStrings.INVERT_Y:
                            mStickParams.invertY = tempMouseJoyAction.mStickParams.invertY;
                            break;
                        case PropertyKeyStrings.VERTICAL_SCALE:
                            mStickParams.verticalScale = tempMouseJoyAction.mStickParams.verticalScale;
                            break;
                        case PropertyKeyStrings.ROTATION:
                            mStickParams.rotation = tempMouseJoyAction.mStickParams.rotation;
                            break;
                        case PropertyKeyStrings.JITTER_COMPENSATION:
                            mStickParams.jitterCompensation = tempMouseJoyAction.mStickParams.jitterCompensation;
                            break;
                        case PropertyKeyStrings.SMOOTHING_ENABLED:
                            mStickParams.smoothing = tempMouseJoyAction.mStickParams.smoothing;
                            break;
                        case PropertyKeyStrings.SMOOTHING_FILTER:
                            mStickParams.smoothingFilterSettings = tempMouseJoyAction.mStickParams.smoothingFilterSettings;
                            useParentSmoothingFilter = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void TempMouseJoyAction_NotifyPropertyChanged(object sender, NotifyPropertyChangeArgs e)
        {
            CascadePropertyChange(e.Mapper, e.PropertyName);
        }

        private void CalcTrackAccel()
        {
            //trackData.trackballAccel = TRACKBALL_RADIUS * TRACKBALL_JOY_FRICTION / TRACKBALL_INERTIA;
            //trackData.trackballAccel = TRACKBALL_RADIUS * trackballFriction / TRACKBALL_INERTIA;
            trackData.trackballAccel = TRACKBALL_RADIUS * mStickParams.trackballFriction / TRACKBALL_INERTIA;
        }

        protected override void CascadePropertyChange(Mapper mapper, string propertyName)
        {
            if (changedProperties.Contains(propertyName))
            {
                // Property already overrridden in action. Leave
                return;
            }
            else if (parentAction == null)
            {
                // No parent action. Leave
                return;
            }

            TouchpadMouseJoystick tempMouseJoyAction = parentAction as TouchpadMouseJoystick;

            switch (propertyName)
            {
                case PropertyKeyStrings.NAME:
                    name = tempMouseJoyAction.name;
                    break;
                case PropertyKeyStrings.DEAD_ZONE:
                    mStickParams.deadZone = tempMouseJoyAction.mStickParams.deadZone;
                    break;
                case PropertyKeyStrings.MAX_ZONE:
                    mStickParams.maxZone = tempMouseJoyAction.mStickParams.maxZone;
                    break;
                case PropertyKeyStrings.ANTIDEAD_ZONE_X:
                    mStickParams.antiDeadzoneX = tempMouseJoyAction.mStickParams.antiDeadzoneX;
                    break;
                case PropertyKeyStrings.ANTIDEAD_ZONE_Y:
                    mStickParams.antiDeadzoneY = tempMouseJoyAction.mStickParams.antiDeadzoneY;
                    break;
                case PropertyKeyStrings.OUTPUT_STICK:
                    outputAction.StickCode = tempMouseJoyAction.outputAction.StickCode;
                    break;
                case PropertyKeyStrings.OUTPUT_CURVE:
                    mStickParams.outputCurve = tempMouseJoyAction.mStickParams.outputCurve;
                    break;
                case PropertyKeyStrings.TRACKBALL_MODE:
                    mStickParams.trackballEnabled = tempMouseJoyAction.mStickParams.trackballEnabled;
                    // Copy parent ref
                    trackData = tempMouseJoyAction.trackData;
                    break;
                case PropertyKeyStrings.TRACKBALL_FRICTION:
                    mStickParams.trackballFriction = tempMouseJoyAction.mStickParams.trackballFriction;
                    useParentTrackFriction = true;
                    CalcTrackAccel();
                    break;
                case PropertyKeyStrings.INVERT_X:
                    mStickParams.invertX = tempMouseJoyAction.mStickParams.invertX;
                    break;
                case PropertyKeyStrings.INVERT_Y:
                    mStickParams.invertY = tempMouseJoyAction.mStickParams.invertY;
                    break;
                case PropertyKeyStrings.VERTICAL_SCALE:
                    mStickParams.verticalScale = tempMouseJoyAction.mStickParams.verticalScale;
                    break;
                case PropertyKeyStrings.ROTATION:
                    mStickParams.rotation = tempMouseJoyAction.mStickParams.rotation;
                    break;
                case PropertyKeyStrings.JITTER_COMPENSATION:
                    mStickParams.jitterCompensation = tempMouseJoyAction.mStickParams.jitterCompensation;
                    break;
                case PropertyKeyStrings.SMOOTHING_ENABLED:
                    mStickParams.smoothing = tempMouseJoyAction.mStickParams.smoothing;
                    break;
                case PropertyKeyStrings.SMOOTHING_FILTER:
                    mStickParams.smoothingFilterSettings = tempMouseJoyAction.mStickParams.smoothingFilterSettings;
                    useParentSmoothingFilter = true;
                    break;
                default:
                    break;
            }
        }
    }
}

using DS4MapperTest.StickModifiers;
using Sensorit.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.StickActions
{
    public class StickFlickStick : StickMapAction
    {
        public const string ACTION_TYPE_NAME = "StickFlickStickAction";

        public class PropertyKeyStrings
        {
            public const string NAME = "Name";
            public const string REAL_WORLD_CALIBRATION = "RealWorldCalibration";
            public const string FLICK_THRESHOLD = "FlickThreshold";
            public const string FLICK_TIME = "FlickTime";
            public const string MIN_ANGLE_THRESHOLD = "MinAngleThreshold";
            public const string IN_GAME_SENS = "InGameSens";
        }

        private HashSet<string> fullPropertySet = new HashSet<string>()
        {
            PropertyKeyStrings.NAME,
            PropertyKeyStrings.REAL_WORLD_CALIBRATION,
            PropertyKeyStrings.FLICK_THRESHOLD,
            PropertyKeyStrings.FLICK_TIME,
            PropertyKeyStrings.MIN_ANGLE_THRESHOLD,
            PropertyKeyStrings.IN_GAME_SENS,
        };

        public class FlickStickMappingData
        {
            public const double DEFAULT_FLICK_PROGRESS = 0.0;
            public const double DEFAULT_FLICK_SIZE = 0.0;
            public const double DEFAULT_FLICK_ANGLE_REMAINDER = 0.0;

            public double flickProgress = DEFAULT_FLICK_PROGRESS;
            public double flickSize = DEFAULT_FLICK_SIZE;
            public double flickAngleRemainder = DEFAULT_FLICK_ANGLE_REMAINDER;

            public void Reset()
            {
                flickProgress = DEFAULT_FLICK_PROGRESS;
                flickSize = DEFAULT_FLICK_SIZE;
                flickAngleRemainder = DEFAULT_FLICK_ANGLE_REMAINDER;
            }
        }

        private const double DEFAULT_DEADZONE = 0.70;
        private const double IN_GAME_SENS_DEFAULT = 1.0;

        private double realWorldCalibration = 5.00;
        public double RealWorldCalibration
        {
            get => realWorldCalibration; set => realWorldCalibration = value;
        }

        private double flickThreshold = 0.9;
        public double FlickThreshold
        {
            get => flickThreshold; set => flickThreshold = value;
        }

        private double flickTime = 0.1;
        public double FlickTime
        {
            get => flickTime; set => flickTime = value;
        }

        private double minAngleThreshold;
        public double MinAngleThreshold
        {
            get => minAngleThreshold; set => minAngleThreshold = value;
        }

        private double inGameSens = IN_GAME_SENS_DEFAULT;
        public double InGameSens
        {
            get => inGameSens;
            set => inGameSens = Math.Clamp(value, 0.1, 10.0);
        }

        private StickDeadZone deadMod;
        public StickDeadZone DeadMod => deadMod;

        private FlickStickMappingData tempFlickData;

        private int prevAxisXVal;
        private int prevAxisYVal;

        private double tempMouseDeltaX;

        public StickFlickStick()
        {
            actionTypeName = ACTION_TYPE_NAME;

            tempFlickData = new FlickStickMappingData();
            deadMod = new StickDeadZone(DEFAULT_DEADZONE, 1.0, 0.0);
        }

        public StickFlickStick(StickDefinition definition) : this()
        {
            this.stickDefinition = definition;
        }

        public override void Prepare(Mapper mapper, int axisXVal, int axisYVal, bool alterState = true)
        {
            tempMouseDeltaX = 0.0;
            double angleChange = 0.0;

            angleChange = HandleFlickStickAngle(mapper, axisXVal, axisYVal, prevAxisXVal, prevAxisYVal);
            double lsangle = angleChange * 180.0 / Math.PI;
            if (lsangle == 0.0)
            {
                tempFlickData.flickAngleRemainder = 0.0;
            }
            else if (lsangle >= 0.0 && tempFlickData.flickAngleRemainder >= 0.0)
            {
                lsangle += tempFlickData.flickAngleRemainder;
            }

            tempFlickData.flickAngleRemainder = 0.0;

            if (minAngleThreshold == 0.0 && lsangle != 0.0)
            //if (Math.Abs(lsangle) >= 0.5)
            {
                tempFlickData.flickAngleRemainder = 0.0;
                //flickAngleRemainder = lsangle - (int)lsangle;
                //lsangle = (int)lsangle;
                tempMouseDeltaX += lsangle * realWorldCalibration / inGameSens;
            }
            else if (Math.Abs(lsangle) >= minAngleThreshold)
            {
                tempFlickData.flickAngleRemainder = 0.0;
                //flickAngleRemainder = lsangle - (int)lsangle;
                //lsangle = (int)lsangle;
                tempMouseDeltaX += lsangle * realWorldCalibration / inGameSens;
            }
            else
            {
                tempFlickData.flickAngleRemainder = lsangle;
            }

            if (tempMouseDeltaX != 0.0)
            {
                active = true;
                activeEvent = true;
            }
            else
            {
                active = false;
                activeEvent = false;
            }

            prevAxisXVal = axisXVal;
            prevAxisYVal = axisYVal;
        }

        private double HandleFlickStickAngle(Mapper mapper, int axisXVal, int axisYVal,
            int prevXVal, int prevYVal)
        {
            double result = 0.0;

            FlickStickMappingData flickData = tempFlickData;

            int axisXMid = stickDefinition.xAxis.mid, axisYMid = stickDefinition.yAxis.mid;
            int axisXDir = axisXVal - axisXMid, axisYDir = axisYVal - axisYMid;
            int prevAxisXDir = prevXVal - axisXMid, prevAxisYDir = prevAxisYVal - axisYMid;
            bool xNegative = axisXDir < 0;
            bool yNegative = axisYDir < 0;
            double maxDirX = (!xNegative ? stickDefinition.xAxis.max : stickDefinition.xAxis.min) - axisXMid;
            double maxDirY = (!yNegative ? stickDefinition.yAxis.max : stickDefinition.yAxis.min) - axisYMid;
            double prevMaxDirX = (prevAxisXDir >= 0 ? stickDefinition.xAxis.max : stickDefinition.xAxis.min) - axisXMid;
            double prevMaxDirY = (prevAxisYDir >= 0 ? stickDefinition.yAxis.max : stickDefinition.yAxis.min) - axisYMid;

            double lastTestX = (prevAxisXDir) / prevMaxDirX;
            double lastTestY = (prevAxisYDir) / prevMaxDirY;
            double currentTestX = (axisXDir) / maxDirX;
            double currentTestY = (axisYDir) / maxDirY;

            double lastLength = (lastTestX * lastTestX) + (lastTestY * lastTestY);
            double length = (currentTestX * currentTestX) + (currentTestY * currentTestY);
            double testLength = flickThreshold * flickThreshold;

            if (length >= testLength)
            {
                int axisYScale = (!this.stickDefinition.yAxis.invert ? -1 : 1);

                if (lastLength < testLength)
                {
                    // Start new Flick
                    flickData.flickProgress = 0.0; // Reset Flick progress
                    flickData.flickSize = Math.Atan2((axisXVal - axisXMid), axisYScale * (axisYVal - axisYMid));
                    //flickData.flickFilter.Filter(0.0, mapper.CurrentLatency);
                }
                else
                {
                    // Turn camera
                    double stickAngle = Math.Atan2((axisXVal - axisXMid), axisYScale * (axisYVal - axisYMid));
                    double lastStickAngle = Math.Atan2((prevXVal - axisXMid), axisYScale * (prevYVal - axisYMid));
                    double angleChange = (stickAngle - lastStickAngle);
                    double rawAngleChange = angleChange;
                    angleChange = (angleChange + Math.PI) % (2 * Math.PI);
                    if (angleChange < 0)
                    {
                        angleChange += 2 * Math.PI;
                    }
                    angleChange -= Math.PI;
                    //Trace.WriteLine(string.Format("ANGLE CHANGE: {0} {1} {2}", stickAngle, lastStickAngle, rawAngleChange));
                    //Trace.WriteLine(string.Format("{0} {1} | {2} {3}", axisXVal, prevXVal, axisYVal, prevYVal));
                    //angleChange = flickData.flickFilter.Filter(angleChange, mapper.CurrentLatency);
                    result += angleChange;
                }
            }
            else
            {
                // Cleanup
                //flickData.flickFilter.Filter(0.0, mapper.CurrentLatency);
                result = 0.0;
            }

            // Continue Flick motion
            double lastFlickProgress = flickData.flickProgress;
            double testFlickTime = flickTime;
            if (lastFlickProgress < testFlickTime)
            {
                flickData.flickProgress = Math.Min(flickData.flickProgress + mapper.CurrentLatency,
                    testFlickTime);

                double lastPerOne = lastFlickProgress / testFlickTime;
                double thisPerOne = flickData.flickProgress / testFlickTime;

                double warpedLastPerOne = WarpEaseOut(lastPerOne);
                double warpedThisPerone = WarpEaseOut(thisPerOne);
                //Trace.WriteLine(string.Format("{0} {1}", warpedThisPerone, warpedLastPerOne));

                result += (warpedThisPerone - warpedLastPerOne) * flickData.flickSize;
            }

            return result;
        }

        public override void Event(Mapper mapper)
        {
            if (tempMouseDeltaX != 0.0)
            {
                mapper.MouseX += tempMouseDeltaX;
                mapper.MouseSync = true;
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
            tempMouseDeltaX = 0.0;
            tempFlickData.Reset();
            active = false;
            activeEvent = false;
        }

        public override void SoftRelease(Mapper mapper, MapAction checkAction, bool resetState = true)
        {
            tempMouseDeltaX = 0.0;
            tempFlickData.Reset();
            active = false;
            activeEvent = false;
        }

        public override StickMapAction DuplicateAction()
        {
            throw new NotImplementedException();
        }

        private static double WarpEaseOut(double input)
        {
            double flipped = 1.0 - input;
            return 1.0 - flipped * flipped;
        }

        public override void SoftCopyFromParent(StickMapAction parentAction)
        {
            if (parentAction is StickFlickStick tempFlickAction)
            {
                base.SoftCopyFromParent(parentAction);

                this.parentAction = parentAction;
                tempFlickAction.hasLayeredAction = true;
                mappingId = tempFlickAction.mappingId;

                this.stickDefinition =
                    new StickDefinition(tempFlickAction.stickDefinition);

                tempFlickAction.NotifyPropertyChanged += TempFlickAction_NotifyPropertyChanged;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                foreach (string parentPropType in useParentProList)
                {
                    switch (parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = tempFlickAction.name;
                            break;
                        case PropertyKeyStrings.REAL_WORLD_CALIBRATION:
                            realWorldCalibration = tempFlickAction.realWorldCalibration;
                            break;
                        case PropertyKeyStrings.FLICK_THRESHOLD:
                            flickThreshold = tempFlickAction.flickThreshold;
                            break;
                        case PropertyKeyStrings.FLICK_TIME:
                            flickTime = tempFlickAction.flickTime;
                            break;
                        case PropertyKeyStrings.MIN_ANGLE_THRESHOLD:
                            minAngleThreshold = tempFlickAction.minAngleThreshold;
                            break;
                        case PropertyKeyStrings.IN_GAME_SENS:
                            inGameSens = tempFlickAction.inGameSens;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void TempFlickAction_NotifyPropertyChanged(object sender, NotifyPropertyChangeArgs e)
        {
            CascadePropertyChange(e.Mapper, e.PropertyName);
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

            StickFlickStick tempFlickAction = parentAction as StickFlickStick;

            switch (propertyName)
            {
                case PropertyKeyStrings.NAME:
                    name = tempFlickAction.name;
                    break;
                case PropertyKeyStrings.REAL_WORLD_CALIBRATION:
                    realWorldCalibration = tempFlickAction.realWorldCalibration;
                    break;
                case PropertyKeyStrings.FLICK_THRESHOLD:
                    flickThreshold = tempFlickAction.flickThreshold;
                    break;
                case PropertyKeyStrings.FLICK_TIME:
                    flickTime = tempFlickAction.flickTime;
                    break;
                case PropertyKeyStrings.MIN_ANGLE_THRESHOLD:
                    minAngleThreshold = tempFlickAction.minAngleThreshold;
                    break;
                case PropertyKeyStrings.IN_GAME_SENS:
                    inGameSens = tempFlickAction.inGameSens;
                    break;
                default:
                    break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.StickModifiers;
using Sensorit.Base;

namespace DS4MapperTest.TouchpadActions
{
    public class TouchpadStickAction : TouchpadMapAction
    {
        public class PropertyKeyStrings
        {
            public const string NAME = "Name";
            public const string DEAD_ZONE = "DeadZone";
            public const string DEAD_ZONE_TYPE = "DeadZoneType";
            public const string MAX_ZONE = "MaxZone";
            public const string ANTIDEAD_ZONE = "AntiDeadZone";
            public const string OUTPUT_CURVE = "OutputCurve";
            public const string OUTPUT_STICK = "OutputStick";
            public const string ROTATION = "Rotation";
            public const string INVERT_X = "InvertX";
            public const string INVERT_Y = "InvertY";
            public const string VERTICAL_SCALE = "VerticalScale";
            public const string MAX_OUTPUT = "MaxOutput";
            public const string MAX_OUTPUT_ENABLED = "MaxOutputEnabled";
            public const string SQUARE_STICK_ENABLED = "SquareStickEnabled";
            public const string SQUARE_STICK_ROUNDNESS = "SquareStickRoundness";
            public const string FORCED_CENTER = "ForcedCenter";

            public const string SMOOTHING_ENABLED = "SmoothingEnabled";
            public const string SMOOTHING_FILTER = "SmoothingFilter";
        }

        private HashSet<string> fullPropertySet = new HashSet<string>()
        {
            PropertyKeyStrings.NAME,
            PropertyKeyStrings.DEAD_ZONE,
            PropertyKeyStrings.DEAD_ZONE_TYPE,
            PropertyKeyStrings.MAX_ZONE,
            PropertyKeyStrings.ANTIDEAD_ZONE,
            PropertyKeyStrings.OUTPUT_CURVE,
            PropertyKeyStrings.OUTPUT_STICK,
            PropertyKeyStrings.INVERT_X,
            PropertyKeyStrings.INVERT_Y,
            PropertyKeyStrings.ROTATION,
            PropertyKeyStrings.VERTICAL_SCALE,
            PropertyKeyStrings.MAX_OUTPUT_ENABLED,
            PropertyKeyStrings.MAX_OUTPUT,
            PropertyKeyStrings.SQUARE_STICK_ENABLED,
            PropertyKeyStrings.SQUARE_STICK_ROUNDNESS,
            PropertyKeyStrings.FORCED_CENTER,

            PropertyKeyStrings.SMOOTHING_ENABLED,
            PropertyKeyStrings.SMOOTHING_FILTER,
        };

        public const string ACTION_TYPE_NAME = "TouchStickTranslateAction";

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


        private double xNorm = 0.0, yNorm = 0.0;
        private double prevXNorm = 0.0, prevYNorm = 0.0;
        private StickDeadZone deadMod;
        private bool wasCenterHit;

        private bool invertX;
        private bool invertY;
        private double verticalScale = 1.0;
        private int rotation;
        private StickOutCurve.Curve outputCurve = StickOutCurve.Curve.Linear;
        private bool maxOutputEnabled;
        private double maxOutput = 1.0;
        private bool squareStickEnabled;
        private double squareStickRoundness = 5.0;
        private SquareStick squaredStick = new SquareStick();

        private bool smoothing;
        private SmoothingFilterSettings smoothingFilterSettings;

        public bool Smoothing
        {
            get => smoothing;
            set => smoothing = value;
        }

        public SmoothingFilterSettings SmoothingFilterSettingsData
        {
            get => smoothingFilterSettings;
        }

        public ref SmoothingFilterSettings SmoothingFilterSettingsDataRef
        {
            get => ref smoothingFilterSettings;
        }

        private bool useParentSmoothingFilter;

        // TODO: Check on state transfer when switching Action Layer instances
        private bool forcedCenter;

        private OutputActionData outputAction;
        public OutputActionData OutputAction
        {
            get => outputAction;
        }

        public StickDeadZone DeadMod
        {
            get => deadMod;
        }

        public StickOutCurve.Curve OutputCurve
        {
            get => outputCurve;
            set => outputCurve = value;
        }

        public bool InvertX
        {
            get => invertX;
            set => invertX = value;
        }

        public bool InvertY
        {
            get => invertY;
            set => invertY = value;
        }

        public int Rotation
        {
            get => rotation;
            set => rotation = value;
        }

        public double VerticalScale
        {
            get => verticalScale;
            set => verticalScale = value;
        }

        public bool MaxOutputEnabled
        {
            get => maxOutputEnabled;
            set => maxOutputEnabled = value;
        }

        public double MaxOutput
        {
            get => maxOutput;
            set => maxOutput = value;
        }

        public bool SquareStickEnabled
        {
            get => squareStickEnabled;
            set => squareStickEnabled = value;
        }

        public double SquareStickRoundness
        {
            get => squareStickRoundness;
            set => squareStickRoundness = value;
        }

        public bool ForcedCenter
        {
            get => forcedCenter;
            set => forcedCenter = value;
        }

        bool touchingActive;
        public TouchpadStickAction()
        {
            this.outputAction = new OutputActionData(OutputActionData.ActionType.GamepadControl, StickActionCodes.X360_LS);
            this.deadMod = new StickDeadZone(0.00, 1.00, 0.00);
            deadMod.DeadZoneType = StickDeadZone.DeadZoneTypes.Radial;
            smoothingFilterSettings.Init();

            actionTypeName = ACTION_TYPE_NAME;
        }

        public override void Prepare(Mapper mapper, ref TouchEventFrame touchFrame, bool alterState = true)
        {
            xNorm = 0.0; yNorm = 0.0;

            int axisXVal = touchFrame.X;
            int axisYVal = touchFrame.Y;

            if (rotation != 0)
            {
                TouchpadMethods.RotatedCoordinates(rotation, axisXVal, axisYVal,
                    touchpadDefinition, out axisXVal, out axisYVal);
            }

            int axisXMid = touchpadDefinition.xAxis.mid, axisYMid = touchpadDefinition.yAxis.mid;
            int axisXDir = axisXVal - axisXMid, axisYDir = axisYVal - axisYMid;
            bool xNegative = axisXDir < 0;
            bool yNegative = axisYDir < 0;
            int maxDirX = (!xNegative ? touchpadDefinition.xAxis.max : touchpadDefinition.xAxis.min) - axisXMid;
            int maxDirY = (!yNegative ? touchpadDefinition.yAxis.max : touchpadDefinition.yAxis.min) - axisYMid;

            deadMod.CalcOutValues(axisXDir, axisYDir, maxDirX, maxDirY, out xNorm, out yNorm);

            //if (xNorm != 0.0 || yNorm != 0.0)
            //{

            //}
            //xNorm = axisXDir / (double)maxDirX;
            //yNorm = axisYDir / (double)maxDirY;

            //if (xNegative) xNorm *= -1.0;
            //if (yNegative) yNorm *= -1.0;
            bool inSafeZone = xNorm != 0.0 || yNorm != 0.0;
            touchingActive = touchFrame.Touch;
            if (touchingActive)
            {
                if (forcedCenter)
                {
                    wasCenterHit = wasCenterHit || (!wasCenterHit && !inSafeZone);
                    touchingActive = wasCenterHit;
                }
                else
                {
                    wasCenterHit = touchingActive = true;
                }
            }
            else
            {
                wasCenterHit = false;
            }

            if (touchingActive && inSafeZone)
            {
                if (outputCurve != StickOutCurve.Curve.Linear)
                {
                    StickOutCurve.CalcOutValue(outputCurve, xNorm, yNorm,
                        out xNorm, out yNorm);
                }

                if (squareStickEnabled)
                {
                    squaredStick.current.x = xNorm;
                    squaredStick.current.y = yNorm;
                    squaredStick.CircleToSquare(squareStickRoundness);
                    xNorm = Math.Clamp(squaredStick.current.x, -1.0, 1.0);
                    yNorm = Math.Clamp(squaredStick.current.y, -1.0, 1.0);
                }

                if (maxOutputEnabled)
                {
                    double r = Math.Atan2(-axisYVal, axisXVal);
                    double maxOutRatio = maxOutput;
                    double maxOutXRatio = Math.Abs(Math.Cos(r)) * maxOutRatio;
                    // Expand output a bit
                    maxOutXRatio = Math.Min(maxOutXRatio / 0.96, 1.0);

                    double maxOutYRatio = Math.Abs(Math.Sin(r)) * maxOutRatio;
                    // Expand output a bit
                    maxOutYRatio = Math.Min(maxOutYRatio / 0.96, 1.0);

                    xNorm = Math.Min(Math.Max(xNorm, 0.0), maxOutXRatio);
                    yNorm = Math.Min(Math.Max(yNorm, 0.0), maxOutYRatio);
                }

                if (invertX) xNorm = -1.0 * xNorm;
                if (invertY) yNorm = -1.0 * yNorm;

                if (verticalScale != 1.0)
                {
                    yNorm *= verticalScale;
                }
            }
            else
            {
                xNorm = yNorm = 0.0;

                if (smoothing)
                {
                    //smoothingFilterSettings.filterX.Filter(0.0, mapper.CurrentRate);
                    //smoothingFilterSettings.filterY.Filter(0.0, mapper.CurrentRate);
                }
            }

            active = true;
            activeEvent = true;
        }

        public override void Event(Mapper mapper)
        {
            double outXNorm = xNorm;
            double outYNorm = yNorm;

            if (smoothing)
            {
                double tempXNorm = smoothingFilterSettings.filterX.Filter(outXNorm * 1.001, mapper.CurrentRate);
                double tempYNorm = smoothingFilterSettings.filterY.Filter(outYNorm * 1.001, mapper.CurrentRate);

                // Filter does not go back to absolute zero for reasons. Check
                // for low number and reset to zero
                if (Math.Abs(tempXNorm) < 0.0001) tempXNorm = 0.0;
                if (Math.Abs(tempYNorm) < 0.0001) tempYNorm = 0.0;

                // Need to check bounds again
                tempXNorm = Math.Clamp(tempXNorm, -1.0, 1.0);
                tempYNorm = Math.Clamp(tempYNorm, -1.0, 1.0);

                outXNorm = tempXNorm;
                outYNorm = tempYNorm;
            }

            mapper.GamepadFromStickInput(outputAction, outXNorm, outYNorm);

            if (xNorm != 0.0 || yNorm != 0.0)
            {
                active = true;
            }
            else
            {
                active = false;
            }

            prevXNorm = xNorm;
            prevYNorm = yNorm;
            activeEvent = false;
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            if (active)
            {
                mapper.GamepadFromStickInput(outputAction, 0.0, 0.0);
            }

            xNorm = yNorm = 0.0;
            prevXNorm = prevYNorm = 0.0;
            wasCenterHit = false;

            active = false;
            activeEvent = false;
        }

        public override void SoftRelease(Mapper mapper, MapAction checkAction, bool resetState = true)
        {
            if (active)
            {
                mapper.GamepadFromStickInput(outputAction, 0.0, 0.0);

                TouchpadStickAction tempAction = checkAction as TouchpadStickAction;
                // Check if going back to base action
                if (parentAction != null && parentAction == tempAction &&
                    forcedCenter && tempAction.forcedCenter)
                {
                    // Copy state variable to child on release
                    wasCenterHit = tempAction.wasCenterHit;
                }
                // Check if switching to an overridden action from base action
                else if (parentAction == null && tempAction.parentAction == this &&
                    forcedCenter && tempAction.forcedCenter)
                {
                    // Copy state variable from parent on parent release
                    tempAction.wasCenterHit = wasCenterHit;
                }
            }
            else
            {
                wasCenterHit = false;
            }

            if (!useParentSmoothingFilter)
            {
                smoothingFilterSettings.ResetFilters();
            }

            xNorm = yNorm = 0.0;
            prevXNorm = prevYNorm = 0.0;

            active = false;
            activeEvent = false;
        }

        public override void SoftCopyFromParent(TouchpadMapAction parentAction)
        {
            if (parentAction is TouchpadStickAction tempStickAction)
            {
                base.SoftCopyFromParent(parentAction);

                this.parentAction = parentAction;
                mappingId = tempStickAction.mappingId;

                this.touchpadDefinition = new TouchpadDefinition(tempStickAction.touchpadDefinition);

                tempStickAction.NotifyPropertyChanged += TempStickAction_NotifyPropertyChanged;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                foreach (string parentPropType in useParentProList)
                {
                    switch(parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = tempStickAction.name;
                            break;
                        case PropertyKeyStrings.DEAD_ZONE:
                            deadMod.DeadZone = tempStickAction.deadMod.DeadZone;
                            break;
                        case PropertyKeyStrings.MAX_ZONE:
                            deadMod.MaxZone = tempStickAction.deadMod.MaxZone;
                            break;
                        case PropertyKeyStrings.ANTIDEAD_ZONE:
                            deadMod.AntiDeadZone = tempStickAction.deadMod.AntiDeadZone;
                            break;
                        case PropertyKeyStrings.OUTPUT_CURVE:
                            outputCurve = tempStickAction.outputCurve;
                            break;
                        case PropertyKeyStrings.OUTPUT_STICK:
                            outputAction.StickCode = tempStickAction.outputAction.StickCode;
                            break;
                        case PropertyKeyStrings.INVERT_X:
                            invertX = tempStickAction.invertX;
                            break;
                        case PropertyKeyStrings.INVERT_Y:
                            invertY = tempStickAction.invertY;
                            break;
                        case PropertyKeyStrings.ROTATION:
                            rotation = tempStickAction.rotation;
                            break;
                        case PropertyKeyStrings.VERTICAL_SCALE:
                            verticalScale = tempStickAction.verticalScale;
                            break;
                        case PropertyKeyStrings.MAX_OUTPUT_ENABLED:
                            maxOutputEnabled = tempStickAction.maxOutputEnabled;
                            break;
                        case PropertyKeyStrings.MAX_OUTPUT:
                            maxOutput = tempStickAction.maxOutput;
                            break;
                        case PropertyKeyStrings.SQUARE_STICK_ENABLED:
                            squareStickEnabled = tempStickAction.squareStickEnabled;
                            break;
                        case PropertyKeyStrings.SQUARE_STICK_ROUNDNESS:
                            squareStickRoundness = tempStickAction.squareStickRoundness;
                            break;
                        case PropertyKeyStrings.DEAD_ZONE_TYPE:
                            deadMod.DeadZoneType = tempStickAction.deadMod.DeadZoneType;
                            break;
                        case PropertyKeyStrings.FORCED_CENTER:
                            forcedCenter = tempStickAction.forcedCenter;
                            break;
                        case PropertyKeyStrings.SMOOTHING_ENABLED:
                            smoothing = tempStickAction.smoothing;
                            break;
                        case PropertyKeyStrings.SMOOTHING_FILTER:
                            smoothingFilterSettings = tempStickAction.smoothingFilterSettings;
                            useParentSmoothingFilter = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void TempStickAction_NotifyPropertyChanged(object sender, NotifyPropertyChangeArgs e)
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

            TouchpadStickAction tempStickAction = parentAction as TouchpadStickAction;

            switch (propertyName)
            {
                case PropertyKeyStrings.NAME:
                    name = tempStickAction.name;
                    break;
                case PropertyKeyStrings.DEAD_ZONE:
                    deadMod.DeadZone = tempStickAction.deadMod.DeadZone;
                    break;
                case PropertyKeyStrings.MAX_ZONE:
                    deadMod.MaxZone = tempStickAction.deadMod.MaxZone;
                    break;
                case PropertyKeyStrings.ANTIDEAD_ZONE:
                    deadMod.AntiDeadZone = tempStickAction.deadMod.AntiDeadZone;
                    break;
                case PropertyKeyStrings.OUTPUT_CURVE:
                    outputCurve = tempStickAction.outputCurve;
                    break;
                case PropertyKeyStrings.OUTPUT_STICK:
                    outputAction.StickCode = tempStickAction.outputAction.StickCode;
                    break;
                case PropertyKeyStrings.INVERT_X:
                    invertX = tempStickAction.invertX;
                    break;
                case PropertyKeyStrings.INVERT_Y:
                    invertY = tempStickAction.invertY;
                    break;
                case PropertyKeyStrings.ROTATION:
                    rotation = tempStickAction.rotation;
                    break;
                case PropertyKeyStrings.VERTICAL_SCALE:
                    verticalScale = tempStickAction.verticalScale;
                    break;
                case PropertyKeyStrings.MAX_OUTPUT_ENABLED:
                    maxOutputEnabled = tempStickAction.maxOutputEnabled;
                    break;
                case PropertyKeyStrings.MAX_OUTPUT:
                    maxOutput = tempStickAction.maxOutput;
                    break;
                case PropertyKeyStrings.SQUARE_STICK_ENABLED:
                    squareStickEnabled = tempStickAction.squareStickEnabled;
                    break;
                case PropertyKeyStrings.SQUARE_STICK_ROUNDNESS:
                    squareStickRoundness = tempStickAction.squareStickRoundness;
                    break;
                case PropertyKeyStrings.DEAD_ZONE_TYPE:
                    deadMod.DeadZoneType = tempStickAction.deadMod.DeadZoneType;
                    break;
                case PropertyKeyStrings.FORCED_CENTER:
                    forcedCenter = tempStickAction.forcedCenter;
                    break;
                case PropertyKeyStrings.SMOOTHING_ENABLED:
                    smoothing = tempStickAction.smoothing;
                    break;
                case PropertyKeyStrings.SMOOTHING_FILTER:
                    smoothingFilterSettings = tempStickAction.smoothingFilterSettings;
                    useParentSmoothingFilter = true;
                    break;
                default:
                    break;
            }
        }
    }
}

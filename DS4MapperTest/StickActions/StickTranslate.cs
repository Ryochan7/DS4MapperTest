using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.StickModifiers;

namespace DS4MapperTest.StickActions
{
    public class StickTranslate : StickMapAction
    {
        public class PropertyKeyStrings
        {
            public const string NAME = "Name";
            public const string DEAD_ZONE = "DeadZone";
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
        }

        private HashSet<string> fullPropertySet = new HashSet<string>()
        {
            PropertyKeyStrings.NAME,
            PropertyKeyStrings.DEAD_ZONE,
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
        };

        public const string ACTION_TYPE_NAME = "StickTranslateAction";

        private OutputActionData outputAction;
        public OutputActionData OutputAction
        {
            get => outputAction;
        }

        private bool invertX;
        private bool invertY;
        private StickDeadZone deadMod;
        private StickOutCurve.Curve outputCurve = StickOutCurve.Curve.Linear;
        private int rotation;
        private double verticalScale = 1.0;
        private bool maxOutputEnabled;
        private double maxOutput = 1.0;
        private bool squareStickEnabled;
        private double squareStickRoundness = 5.0;
        private SquareStick squaredStick = new SquareStick();

        //private StickDefinition stickDefinition;
        private double xNorm = 0.0, yNorm = 0.0;
        private double prevXNorm = 0.0, prevYNorm = 0.0;
        private StickTranslate parentTranslateAction;

        public StickDeadZone DeadMod { get => deadMod; }
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

        public StickTranslate()
        {
            actionTypeName = ACTION_TYPE_NAME;
            this.outputAction = new OutputActionData(OutputActionData.ActionType.GamepadControl, StickActionCodes.Empty);
            deadMod = new StickDeadZone(0.30, 1.0, 0.0);
        }

        public StickTranslate(StickDefinition stickDefinition,
            OutputActionData outputAction)
        {
            actionTypeName = ACTION_TYPE_NAME;
            this.stickDefinition = stickDefinition;
            deadMod = new StickDeadZone(0.30, 1.0, 0.0);
            deadMod.CircleDead = true;
            this.outputAction = outputAction;
        }

        public StickTranslate(StickTranslate parentAction)
        {
            actionTypeName = ACTION_TYPE_NAME;
            deadMod = new StickDeadZone(0.30, 1.0, 0.0);

            if (parentAction != null)
            {
                this.parentAction = parentAction;
                this.parentTranslateAction = parentAction;
                parentAction.hasLayeredAction = true;
                mappingId = parentAction.mappingId;
                deadMod = new StickDeadZone(parentAction.DeadMod);
                invertX = parentAction.invertX;
                invertY = parentAction.invertY;
            }
        }

        public override void Prepare(Mapper mapper, int axisXVal, int axisYVal, bool alterState = true)
        {
            xNorm = 0.0; yNorm = 0.0;

            if (rotation != 0)
            {
                StickMethods.RotatedCoordinates(rotation, axisXVal, axisYVal,
                    stickDefinition, out axisXVal, out axisYVal);
            }

            //int inputX = axisXVal;
            //int inputY = axisYVal;

            //if (stickDefinition.stickCode == StickActionCodes.LS)
            //{
            //    StickMethods.RotatedCoordinates(8, axisXVal, axisYVal, stickDefinition, out axisXVal, out axisYVal);
            //    //Trace.WriteLine($"Input X ({inputX}) Y ({inputY}) | Output X ({axisXVal}) Y ({axisYVal})");
            //}
            //else if (stickDefinition.stickCode == StickActionCodes.RS)
            //{
            //    StickMethods.RotatedCoordinates(-8, axisXVal, axisYVal, stickDefinition, out axisXVal, out axisYVal);
            //}

            int axisXMid = stickDefinition.xAxis.mid, axisYMid = stickDefinition.yAxis.mid;
            int axisXDir = axisXVal - axisXMid, axisYDir = axisYVal - axisYMid;
            bool xNegative = axisXDir < 0;
            bool yNegative = axisYDir < 0;
            int maxDirX = (!xNegative ? stickDefinition.xAxis.max : stickDefinition.xAxis.min) - axisXMid;
            int maxDirY = (!yNegative ? stickDefinition.yAxis.max : stickDefinition.yAxis.min) - axisYMid;
            deadMod.CalcOutValues(axisXDir, axisYDir, maxDirX,
                    maxDirY, out xNorm, out yNorm);

            if (xNorm != 0.0 || yNorm != 0.0)
            {
                //if (stickDefinition.xAxis.invert)
                //{
                //    xNorm = -1.0 * xNorm;
                //}

                //if (stickDefinition.yAxis.invert)
                //{
                //    yNorm = -1.0 * yNorm;
                //}

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

                active = true;
                activeEvent = true;
            }
        }

        public override void Event(Mapper mapper)
        {
            double outXNorm = xNorm;
            double outYNorm = yNorm;

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

            xNorm = prevXNorm = 0.0;
            yNorm = prevYNorm = 0.0;
            activeEvent = false;
            active = false;
        }

        public override StickMapAction DuplicateAction()
        {
            return new StickTranslate(this);
        }

        public override void SoftRelease(Mapper mapper, MapAction _, bool resetState = true)
        {
            if (active)
            {
                mapper.GamepadFromStickInput(outputAction, 0.0, 0.0);
            }

            xNorm = prevXNorm = 0.0;
            yNorm = prevYNorm = 0.0;
            activeEvent = false;
            active = false;
        }

        public override void SoftCopyFromParent(StickMapAction parentAction)
        {
            if (parentAction is StickTranslate tempStickAction)
            {
                base.SoftCopyFromParent(parentAction);

                this.parentAction = parentAction;
                this.parentTranslateAction = tempStickAction;
                tempStickAction.hasLayeredAction = true;
                mappingId = tempStickAction.mappingId;

                tempStickAction.NotifyPropertyChanged += TempStickAction_NotifyPropertyChanged;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                foreach (string parentPropType in useParentProList)
                {
                    switch (parentPropType)
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

            StickTranslate tempStickAction = parentAction as StickTranslate;

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
                default:
                    break;
            }
        }
    }
}

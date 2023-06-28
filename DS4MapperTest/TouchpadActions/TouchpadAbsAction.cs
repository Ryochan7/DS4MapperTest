using DS4MapperTest.ButtonActions;
using DS4MapperTest.StickModifiers;
using System;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4MapperTest.TouchpadActions
{
    public class TouchpadAbsAction : TouchpadMapAction
    {
        public class PropertyKeyStrings
        {
            public const string NAME = "Name";
            public const string DEAD_ZONE = "DeadZone";
            public const string MAX_ZONE = "MaxZone";
            public const string ANTI_RADIUS = "AntiRadius";
            public const string SNAP_TO_CENTER_RELEASE = "SnapToCenterRelease";

            public const string OUTER_RING_BUTTON = "OuterRingButton";

            public const string USE_OUTER_RING = "UseOuterRing";
            public const string OUTER_RING_DEAD_ZONE = "OuterRingDeadZone";
            public const string USE_AS_OUTER_RING = "UseAsOuterRing";
            public const string OUTER_RING_FULL_RANGE = "OuterRingFullRange";

            public const string BOX_WIDTH = "BoxWidth";
            public const string BOX_HEIGHT = "BoxHeight";
            public const string BOX_XCENTER = "BoxXCenter";
            public const string BOX_YCENTER = "BoxYCenter";
        }

        private HashSet<string> fullPropertySet = new HashSet<string>()
        {
            PropertyKeyStrings.NAME,
            PropertyKeyStrings.DEAD_ZONE,
            PropertyKeyStrings.MAX_ZONE,
            PropertyKeyStrings.ANTI_RADIUS,
            PropertyKeyStrings.SNAP_TO_CENTER_RELEASE,
            PropertyKeyStrings.OUTER_RING_BUTTON,
            PropertyKeyStrings.USE_OUTER_RING,
            PropertyKeyStrings.OUTER_RING_DEAD_ZONE,
            PropertyKeyStrings.USE_AS_OUTER_RING,
            PropertyKeyStrings.OUTER_RING_FULL_RANGE,

            PropertyKeyStrings.BOX_WIDTH,
            PropertyKeyStrings.BOX_HEIGHT,
            PropertyKeyStrings.BOX_XCENTER,
            PropertyKeyStrings.BOX_YCENTER,
        };

        public struct AbsCoordRange
        {
            //public double top;
            //public double bottom;
            //public double left;
            //public double right;

            public double xcenter;
            public double ycenter;

            public double width;
            public double height;

            public void Init()
            {
                width = 1.0;
                height = 1.0;

                xcenter = 0.5;
                ycenter = 0.5;
            }
        }

        public const string ACTION_TYPE_NAME = "TouchAbsPadAction";

        // Specify the input state of the button
        private bool inputStatus;

        private AxisDirButton ringButton = new AxisDirButton();
        private AxisDirButton usedRingButton = null;
        private bool useParentRingButton;
        public bool UseParentRingButton
        {
            get => useParentRingButton;
            set => useParentRingButton = value;
        }

        /// <summary>
        /// Used to determine outer ring mode or inner ring mode. Will change to using an Enum later
        /// </summary>
        private bool outerRing = true;
        /// <summary>
        /// Specify whether to interpret a ring binging at all
        /// </summary>
        private bool useRingButton;
        /// <summary>
        /// Displacement threshold when a ring binding should execute
        /// </summary>
        private double outerRingDeadZone = 0.7;
        private OuterRingUseRange usedOuterRingRange;

        private StickDeadZone deadMod;
        private AbsCoordRange absRange;

        private double xNorm = 0.0, yNorm = 0.0;
        private double fullXNorm = 0.0, fullYNorm = 0.0;
        private double ringDistance = 0.0;
        private double xMotion;
        private double yMotion;
        private double fuzzXNorm;
        private double fuzzYNorm;
        private double antiRadius = 0.0;

        public double AntiRadius
        {
            get => antiRadius;
            set => antiRadius = value;
        }

        private bool snapToCenterRelease = true;
        public bool SnapToCenterRelease
        {
            get => snapToCenterRelease;
            set => snapToCenterRelease = value;
        }

        public StickDeadZone DeadMod { get => deadMod; }
        public ref AbsCoordRange AbsMouseRange
        {
            get => ref absRange;
        }

        public AxisDirButton RingButton
        {
            get => ringButton;
            set => ringButton = value;
        }

        public bool UseAsOuterRing { get => outerRing; set => outerRing = value; }
        public bool UseRingButton { get => useRingButton; set => useRingButton = value; }
        public double OuterRingDeadZone { get => outerRingDeadZone; set => outerRingDeadZone = value; }
        public OuterRingUseRange UsedOuterRingRange { get => usedOuterRingRange; set => usedOuterRingRange = value; }

        public TouchpadAbsAction()
        {
            actionTypeName = ACTION_TYPE_NAME;

            deadMod = new StickDeadZone(0.0, 1.0, 0.0);
            absRange = new AbsCoordRange()
            {
                //top = 0.3, bottom = 0.7, left = 0.3, right = 0.7,
                width = 0.4,
                height = 0.4,
                xcenter = 0.5,
                ycenter = 0.5,
            };
            absRange.Init();
            outerRing = true;
            antiRadius = 0.0;
        }

        public override void Prepare(Mapper mapper, ref TouchEventFrame touchFrame, bool alterState = true)
        {
            active = false;
            activeEvent = false;

            bool wasActive = xNorm != 0.0 || yNorm != 0.0;
            double prevXNorm = xNorm, prevYNorm = yNorm;

            xNorm = 0.0; yNorm = 0.0;
            xMotion = yMotion = 0.0;

            int axisXMid = touchpadDefinition.xAxis.mid, axisYMid = touchpadDefinition.yAxis.mid;
            int axisXVal = touchFrame.X; int axisYVal = touchFrame.Y;
            int axisXDir = axisXVal - axisXMid, axisYDir = axisYVal - axisYMid;
            bool xNegative = axisXDir < 0;
            bool yNegative = axisYDir < 0;
            int maxDirX = (!xNegative ? touchpadDefinition.xAxis.max : touchpadDefinition.xAxis.min) - axisXMid;
            int maxDirY = (!yNegative ? touchpadDefinition.yAxis.max : touchpadDefinition.yAxis.min) - axisYMid;
            deadMod.CalcOutValues(axisXDir, axisYDir, maxDirX,
                    maxDirY, out xNorm, out yNorm);

            fullXNorm = axisXDir / (double)maxDirX;
            fullYNorm = axisYDir / (double)maxDirY;
            bool isActive = xNorm != 0.0 || yNorm != 0.0;
            bool inSafeZone = isActive;

            if (usedOuterRingRange == OuterRingUseRange.OnlyActive)
            {
                ringDistance = Math.Sqrt((xNorm * xNorm) + (yNorm * yNorm));
            }
            else
            {
                ringDistance = Math.Sqrt((fullXNorm * fullXNorm) + (fullYNorm * fullYNorm));
            }

            usedRingButton = ringButton;
            if (inSafeZone || (wasActive && snapToCenterRelease))
            {
                inputStatus = isActive;

                double usedXNorm = (isActive) ? xNorm : (!snapToCenterRelease ? prevXNorm : 0.0);
                double usedYNorm = (isActive) ? yNorm : (!snapToCenterRelease ? prevYNorm : 0.0);
                double xSign = usedXNorm >= 0.0 ? 1.0 : -1.0;
                double ySign = usedYNorm >= 0.0 ? 1.0 : -1.0;
                //double absXUnit = Math.Abs(usedXNorm);
                //double absYUnit = Math.Abs(usedYNorm);
                double angleRad = Math.Atan2(-axisYDir, axisXDir);
                double angCos = Math.Abs(Math.Cos(angleRad));
                double angSin = Math.Abs(Math.Sin(angleRad));

                if (inSafeZone)
                {
                    // Implement fuzz logic to make output cursor less jittery when
                    // trying to hold a position
                    double fuzzSquared = 0.01 * 0.01;
                    double dist = Math.Pow(fuzzXNorm - usedXNorm, 2) + Math.Pow(fuzzYNorm - usedYNorm, 2);
                    if (dist <= fuzzSquared)
                    {
                        active = true;
                        activeEvent = true;
                        return;
                    }
                    else
                    {
                        fuzzXNorm = usedXNorm;
                        fuzzYNorm = usedYNorm;
                    }
                }

                double outXRatio = usedXNorm, outYRatio = usedYNorm;
                //double antiDead = 0.2;
                double antiDead = antiRadius;

                // Find Release zone
                if (inSafeZone && antiDead != 0.0)
                {
                    double antiDeadX = antiDead * angCos;
                    double antiDeadY = antiDead * angSin;
                    double absX = Math.Abs(outXRatio);
                    double absY = Math.Abs(outYRatio);
                    outXRatio = ((1.0 - antiDeadX) * absX + antiDeadX) * xSign;
                    outYRatio = ((1.0 - antiDeadY) * absY + antiDeadY) * ySign;
                    //Trace.WriteLine($"{antiDeadX} {outXRatio}");
                }

                //if (absRange.xcenter != 0.5 || absRange.ycenter != 0.5)
                {
                    //Trace.WriteLine($"{absRange.xcenter} {absRange.ycenter} {absRange.width / 2.0} {absRange.height / 2.0}");
                    outXRatio = (absRange.width / 2.0) * outXRatio + absRange.xcenter;
                    outYRatio = (absRange.height / 2.0) * (outYRatio * -1.0) + absRange.ycenter;

                    //Trace.WriteLine($"Test Abs Mouse: IN ({xNorm}, {yNorm}) OUT ({outXRatio}, {outYRatio})");
                }

                // Determine position relative to box size
                //if (absRange.width != 1.0 || absRange.height != 1.0)
                //{
                //    outXRatio = absRange.width * outXRatio;
                //    outYRatio = absRange.height * outYRatio;
                //}

                // Keep final coords within [-1.0, 1.0] range
                outXRatio = Math.Clamp(outXRatio, -1.0, 1.0);
                outYRatio = Math.Clamp(outYRatio, -1.0, 1.0);

                // [-1.0, 1.0] -> [0.0, 1.0]
                //xMotion = (1.0 - 0.5) * xNorm + 0.5;
                //yMotion = (1.0 - 0.5) * (yNorm * -1.0) + 0.5; // Invert Y
                //xMotion = (1.0 - 0.5) * outXRatio + 0.5;
                //yMotion = (1.0 - 0.5) * (outYRatio * -1.0) + 0.5; // Invert Y
                xMotion = outXRatio;
                yMotion = outYRatio;
                //xMotion = (absRange.right - 0.5) * xNorm + 0.5;
                //yMotion = (absRange.bottom - 0.5) * (yNorm * -1.0) + 0.5; // Invert Y

                //Trace.WriteLine($"Test Abs Mouse: IN ({xNorm}, {yNorm}) OUT ({xMotion}, {yMotion})");

                //usedRingButton = ringButton;

                active = true;
                activeEvent = true;
            }
            else
            {
                inputStatus = false;

                active = ringDistance != 0.0 || (usedRingButton != null && usedRingButton.active);
                //usedRingButton = ringButton;
            }
        }

        public override void Event(Mapper mapper)
        {
            if (useRingButton && usedRingButton != null)
            {
                bool activeMod = outerRing ? (ringDistance > outerRingDeadZone ? true : false) :
                    (ringDistance > 0.0 && (outerRingDeadZone == 1.0 || ringDistance <= outerRingDeadZone) ? true : false);

                double tempRingDistance = activeMod ? ringDistance : 0.0;
                double tempRingUnit = activeMod ? 1.0 : 0.0;

                bool oldRingActive = usedRingButton.active;
                usedRingButton.PrepareAnalog(mapper, tempRingDistance, tempRingUnit);
                // Treat as boolean button for now
                //usedRingButton.Prepare(mapper, activeMod);
                //usedRingButton.PrepareAnalog(mapper, dist);
                if (usedRingButton.active)
                {
                    usedRingButton.Event(mapper);
                }
                //else if (oldRingActive)
                //{
                //    usedRingButton.Event(mapper);
                //}
            }

            //if (xMotion != 0.0 || yMotion != 0.0)
            {
                mapper.AbsMouseX = xMotion; mapper.AbsMouseY = yMotion;
                mapper.AbsMouseSync = true;
            }

            active = xNorm != 0.0 || yNorm != 0.0;
            activeEvent = false;
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            xNorm = yNorm = 0.0;
            xMotion = yMotion = 0.0;
            fuzzXNorm = fuzzYNorm = 0.0;

            if (useRingButton && usedRingButton != null)
            {
                usedRingButton.Release(mapper, resetState, ignoreReleaseActions);
            }

            mapper.AbsMouseX = xMotion; mapper.AbsMouseY = yMotion;
            mapper.AbsMouseSync = true;

            active = false;
            activeEvent = false;
        }

        public override void SoftRelease(Mapper mapper, MapAction checkAction, bool resetState = true)
        {
            xNorm = yNorm = 0.0;
            xMotion = yMotion = 0.0;
            fuzzXNorm = fuzzYNorm = 0.0;

            if (useRingButton && usedRingButton != null && !useParentRingButton)
            {
                usedRingButton.Release(mapper, resetState);
            }

            active = false;
            activeEvent = false;
        }

        public override void SoftCopyFromParent(TouchpadMapAction parentAction)
        {
            if (parentAction is TouchpadAbsAction tempAbsAction)
            {
                base.SoftCopyFromParent(parentAction);

                //tempAbsAction.hasLayeredAction = true;
                mappingId = tempAbsAction.mappingId;

                this.touchpadDefinition = new TouchpadDefinition(tempAbsAction.touchpadDefinition);

                tempAbsAction.NotifyPropertyChanged += TempAbsAction_NotifyPropertyChanged;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                foreach (string parentPropType in useParentProList)
                {
                    switch(parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = tempAbsAction.name;
                            break;
                        case PropertyKeyStrings.DEAD_ZONE:
                            deadMod.DeadZone = tempAbsAction.deadMod.DeadZone;
                            break;
                        case PropertyKeyStrings.MAX_ZONE:
                            deadMod.MaxZone = tempAbsAction.deadMod.MaxZone;
                            break;
                        case PropertyKeyStrings.ANTI_RADIUS:
                            antiRadius = tempAbsAction.antiRadius;
                            break;
                        case PropertyKeyStrings.OUTER_RING_BUTTON:
                            ringButton = tempAbsAction.ringButton;
                            useParentRingButton = true;
                            break;
                        case PropertyKeyStrings.USE_OUTER_RING:
                            useRingButton = tempAbsAction.useRingButton;
                            break;
                        case PropertyKeyStrings.OUTER_RING_DEAD_ZONE:
                            outerRingDeadZone = tempAbsAction.outerRingDeadZone;
                            break;
                        case PropertyKeyStrings.USE_AS_OUTER_RING:
                            outerRing = tempAbsAction.outerRing;
                            break;
                        case PropertyKeyStrings.OUTER_RING_FULL_RANGE:
                            usedOuterRingRange = tempAbsAction.usedOuterRingRange;
                            break;
                        case PropertyKeyStrings.SNAP_TO_CENTER_RELEASE:
                            snapToCenterRelease = tempAbsAction.snapToCenterRelease;
                            break;
                        case PropertyKeyStrings.BOX_WIDTH:
                            absRange.width = tempAbsAction.absRange.width;
                            break;
                        case PropertyKeyStrings.BOX_HEIGHT:
                            absRange.height = tempAbsAction.absRange.height;
                            break;
                        case PropertyKeyStrings.BOX_XCENTER:
                            absRange.xcenter = tempAbsAction.absRange.xcenter;
                            break;
                        case PropertyKeyStrings.BOX_YCENTER:
                            absRange.ycenter = tempAbsAction.absRange.ycenter;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void TempAbsAction_NotifyPropertyChanged(object sender, NotifyPropertyChangeArgs e)
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

            TouchpadAbsAction tempAbsAction = parentAction as TouchpadAbsAction;

            switch (propertyName)
            {
                case PropertyKeyStrings.NAME:
                    name = tempAbsAction.name;
                    break;
                case PropertyKeyStrings.DEAD_ZONE:
                    deadMod.DeadZone = tempAbsAction.deadMod.DeadZone;
                    break;
                case PropertyKeyStrings.MAX_ZONE:
                    deadMod.MaxZone = tempAbsAction.deadMod.MaxZone;
                    break;
                case PropertyKeyStrings.ANTI_RADIUS:
                    antiRadius = tempAbsAction.antiRadius;
                    break;
                case PropertyKeyStrings.OUTER_RING_BUTTON:
                    ringButton = tempAbsAction.ringButton;
                    useParentRingButton = true;
                    break;
                case PropertyKeyStrings.USE_OUTER_RING:
                    useRingButton = tempAbsAction.useRingButton;
                    break;
                case PropertyKeyStrings.OUTER_RING_DEAD_ZONE:
                    outerRingDeadZone = tempAbsAction.outerRingDeadZone;
                    break;
                case PropertyKeyStrings.USE_AS_OUTER_RING:
                    outerRing = tempAbsAction.outerRing;
                    break;
                case PropertyKeyStrings.OUTER_RING_FULL_RANGE:
                    usedOuterRingRange = tempAbsAction.usedOuterRingRange;
                    break;
                case PropertyKeyStrings.SNAP_TO_CENTER_RELEASE:
                    snapToCenterRelease = tempAbsAction.snapToCenterRelease;
                    break;
                case PropertyKeyStrings.BOX_WIDTH:
                    absRange.width = tempAbsAction.absRange.width;
                    break;
                case PropertyKeyStrings.BOX_HEIGHT:
                    absRange.height = tempAbsAction.absRange.height;
                    break;
                case PropertyKeyStrings.BOX_XCENTER:
                    absRange.xcenter = tempAbsAction.absRange.xcenter;
                    break;
                case PropertyKeyStrings.BOX_YCENTER:
                    absRange.ycenter = tempAbsAction.absRange.ycenter;
                    break;
                default:
                    break;
            }
        }
    }
}

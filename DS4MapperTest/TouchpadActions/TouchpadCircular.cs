using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Diagnostics;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.StickModifiers;
using DS4MapperTest.ButtonActions;
using DS4MapperTest.ActionUtil;

namespace DS4MapperTest.TouchpadActions
{
    public class TouchpadCircular : TouchpadMapAction
    {
        public class PropertyKeyStrings
        {
            public const string NAME = "Name";
            public const string SCROLL_BUTTON_1 = "ScrollButton1";
            public const string SCROLL_BUTTON_2 = "ScrollButton2";
        }

        private HashSet<string> fullPropertySet = new HashSet<string>()
        {
            PropertyKeyStrings.NAME,
            PropertyKeyStrings.SCROLL_BUTTON_1,
            PropertyKeyStrings.SCROLL_BUTTON_2,
        };

        private enum ClickDirection
        {
            Clockwise,
            CounterClockwise,
        }

        private const double CLICK_ANGLE_THRESHOLD = 12.0;
        private const double CLICK_RAD_THRESHOLD = CLICK_ANGLE_THRESHOLD * Math.PI / 180.0;
        private const double DEFAULT_DEADZONE = 0.25;
        public const string ACTION_TYPE_NAME = "TouchCircularAction";

        private double startAngleRad;
        private double currentAngleRad;
        //private double remainderAngle;
        private double travelAngleChangeRad;
        private bool activeTicks;
        private ClickDirection currentClickDir;
        private ClickDirection previousClickDir;

        private TouchpadCircularButton clockwiseBtn;
        private TouchpadCircularButton counterClockwiseBtn;
        private TouchpadCircularButton activeCircBtn;

        public TouchpadCircularButton ClockWiseBtn
        {
            get => clockwiseBtn;
            set => clockwiseBtn = value;
        }

        public TouchpadCircularButton CounterClockwiseBtn
        {
            get => counterClockwiseBtn;
            set => counterClockwiseBtn = value;
        }

        private bool[] useParentCircButtons = new bool[2];
        public bool[] UseParentCircButtons => useParentCircButtons;

        private StickDeadZone deadMod;
        public StickDeadZone DeadMod => deadMod;

        public TouchpadCircular()
        {
            actionTypeName = ACTION_TYPE_NAME;

            OutputActionData clockwiseOutputAction =
                new OutputActionData(OutputActionData.ActionType.MouseWheel, (int)MouseWheelCodes.WheelDown);
            clockwiseOutputAction.OutputCodeStr = MouseWheelCodes.WheelDown.ToString();

            OutputActionData counterClockwiseOutputAction =
                new OutputActionData(OutputActionData.ActionType.MouseWheel, (int)MouseWheelCodes.WheelUp);
            counterClockwiseOutputAction.OutputCodeStr = MouseWheelCodes.WheelUp.ToString();

            changedProperties.Add(PropertyKeyStrings.SCROLL_BUTTON_1);
            changedProperties.Add(PropertyKeyStrings.SCROLL_BUTTON_2);

            clockwiseBtn = new TouchpadCircularButton();
            clockwiseBtn.ActionFuncs.AddRange(new ActionFunc[]
            {
                new NormalPressFunc(clockwiseOutputAction)
            });

            counterClockwiseBtn = new TouchpadCircularButton();
            counterClockwiseBtn.ActionFuncs.AddRange(new ActionFunc[]
            {
                new NormalPressFunc(counterClockwiseOutputAction),
            });

            deadMod = new StickDeadZone(DEFAULT_DEADZONE, 1.0, 0.0);
        }

        public override void Prepare(Mapper mapper, ref TouchEventFrame touchFrame, bool alterState = true)
        {
            active = false;
            previousClickDir = currentClickDir;

            ref TouchEventFrame previousTouchFrame =
                    ref mapper.GetPreviousTouchEventFrame(touchpadDefinition.touchCode);

            int axisXMid = touchpadDefinition.xAxis.mid, axisYMid = touchpadDefinition.yAxis.mid;
            int axisXVal = touchFrame.X;
            int axisYVal = touchFrame.Y;
            int axisXDir = axisXVal - axisXMid, axisYDir = axisYVal - axisYMid;
            bool xNegative = axisXDir < 0;
            bool yNegative = axisYDir < 0;
            int maxDirX = (!xNegative ? touchpadDefinition.xAxis.max : touchpadDefinition.xAxis.min) - axisXMid;
            int maxDirY = (!yNegative ? touchpadDefinition.yAxis.max : touchpadDefinition.yAxis.min) - axisYMid;

            bool isActive = touchFrame.Touch;
            bool wasActive = previousTouchFrame.Touch;

            deadMod.CalcOutValues(axisXDir, axisYDir, maxDirX, maxDirY,
                out double xNorm, out double yNorm);

            bool inSafeZone = xNorm != 0.0 || yNorm != 0.0;

            if (!inSafeZone)
            {
                startAngleRad = 0;
                currentAngleRad = 0;
                travelAngleChangeRad = 0;
            }
            else if (isActive && !wasActive)
            {
                double angleRad = Math.Atan2(xNorm, yNorm);
                double angleDeg = (angleRad >= 0 ? angleRad : (2 * Math.PI + angleRad)) * 180 / Math.PI;

                startAngleRad = angleRad;
                currentAngleRad = angleRad;
                travelAngleChangeRad = 0;
            }
            else if (isActive)
            {
                double previousAngleRad = currentAngleRad;

                double angleRad = Math.Atan2(xNorm, yNorm);
                //double angleDeg = (angleRad >= 0 ? angleRad : (2 * Math.PI + angleRad)) * 180 / Math.PI;

                currentAngleRad = angleRad;

                double diffAngle = currentAngleRad - previousAngleRad;
                // Negate wrapping between PI to -PI on lower portion of Touchpad
                if (diffAngle > Math.PI)
                {
                    //Trace.WriteLine("POS FAR");
                    diffAngle -= 2 * Math.PI;
                }
                else if (diffAngle < -Math.PI)
                {
                    //Trace.WriteLine("NEG FAR");
                    diffAngle += 2 * Math.PI;
                }

                travelAngleChangeRad += diffAngle;
                //Trace.WriteLine($"DIFF {diffAngle} RAD: {angleRad} Previous: {previousAngleRad} CURRENT ANG: {currentAngleRad} TRAVEL ANG: {travelAngleChangeRad}");
                //if (Math.Abs(travelAngleChangeRad) > 5 * CLICK_RAD_THRESHOLD)
                //{
                //    Trace.WriteLine($"OUT OF WHACK {travelAngleChangeRad} {diffAngle}");
                //}

                if (Math.Abs(travelAngleChangeRad) > CLICK_RAD_THRESHOLD)
                {
                    //Trace.WriteLine("UP IN HERE");
                    active = true;
                    activeTicks = true;

                    currentClickDir = diffAngle > 0 ? ClickDirection.Clockwise : ClickDirection.CounterClockwise;
                    //travelAngleChangeRad = travelAngleChangeRad > 0 ?
                    //    travelAngleChangeRad - CLICK_RAD_THRESHOLD : travelAngleChangeRad + CLICK_RAD_THRESHOLD;
                }
            }
            else
            {
                startAngleRad = 0;
                currentAngleRad = 0;
                travelAngleChangeRad = 0;
            }

            active = true;
            activeEvent = true;
        }

        public override void Event(Mapper mapper)
        {
            // Check for previously activated output actions
            if (previousClickDir != currentClickDir || !activeTicks)
            {
                if (activeCircBtn != null)
                {
                    activeCircBtn.PrepareCircular(mapper, 0.0);
                    activeCircBtn.Event(mapper);
                }

                activeCircBtn = null;
            }

            if (activeTicks)
            {
                //Trace.WriteLine($"OUTPUT EVENT {DateTime.UtcNow.ToString("fff")}");

                //OutputActionData tempAction =
                //    currentClickDir == ClickDirection.Clockwise ? clockwiseOutputAction : counterClockwiseOutputAction;

                //int ticksSpeed = (int)(Math.Abs(travelAngleChangeRad) / CLICK_RAD_THRESHOLD);

                //mapper.RunEventFromAnalog(tempAction, true, ticksSpeed);
                //tempAction.activatedEvent = false;

                TouchpadCircularButton tempBtn =
                    currentClickDir == ClickDirection.Clockwise ? clockwiseBtn : counterClockwiseBtn;

                int ticksSpeed = (int)(Math.Abs(travelAngleChangeRad) / CLICK_RAD_THRESHOLD);
                tempBtn.PrepareCircular(mapper, ticksSpeed);
                tempBtn.Event(mapper);
                activeCircBtn = tempBtn;

                travelAngleChangeRad = travelAngleChangeRad > 0 ?
                    travelAngleChangeRad - (ticksSpeed * CLICK_RAD_THRESHOLD) : travelAngleChangeRad + (ticksSpeed * CLICK_RAD_THRESHOLD);

                activeTicks = false;
            }

            active = false;
            activeEvent = false;
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            if (activeCircBtn != null && activeCircBtn.active)
            {
                activeCircBtn.Release(mapper, resetState, ignoreReleaseActions);
            }

            startAngleRad = 0;
            currentAngleRad = 0;
            travelAngleChangeRad = 0;
            currentClickDir = ClickDirection.Clockwise;
            active = activeEvent = false;
        }

        public override void SoftRelease(Mapper mapper, MapAction checkAction, bool resetState = true)
        {
            if (activeCircBtn != null && activeCircBtn.active &&
                !useParentCircButtons[(int)currentClickDir])
            {
                activeCircBtn.Release(mapper, resetState);
            }
        }

        public override void SoftCopyFromParent(TouchpadMapAction parentAction)
        {
            if (parentAction is TouchpadCircular tempCirleAction)
            {
                base.SoftCopyFromParent(parentAction);

                tempCirleAction.NotifyPropertyChanged += TempCirleAction_NotifyPropertyChanged;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                foreach (string parentPropType in useParentProList)
                {
                    switch(parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = tempCirleAction.name;
                            break;
                        case PropertyKeyStrings.SCROLL_BUTTON_1:
                            useParentCircButtons[0] = true;
                            break;
                        case PropertyKeyStrings.SCROLL_BUTTON_2:
                            useParentCircButtons[1] = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void TempCirleAction_NotifyPropertyChanged(object sender, NotifyPropertyChangeArgs e)
        {
            CascadePropertyChange(e.Mapper, e.PropertyName);
        }

        public override void PrepareActions()
        {
            counterClockwiseBtn.PrepareActions();
            counterClockwiseBtn.PrepareActions();
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

            TouchpadCircular tempCirleAction = parentAction as TouchpadCircular;

            switch (propertyName)
            {
                case PropertyKeyStrings.NAME:
                    name = tempCirleAction.name;
                    break;
                case PropertyKeyStrings.SCROLL_BUTTON_1:
                    useParentCircButtons[0] = true;
                    break;
                case PropertyKeyStrings.SCROLL_BUTTON_2:
                    useParentCircButtons[1] = true;
                    break;
                default:
                    break;
            }
        }
    }
}

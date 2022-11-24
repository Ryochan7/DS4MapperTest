using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.AxisModifiers;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.ButtonActions;

namespace DS4MapperTest.TouchpadActions
{
    public class TouchpadAxesAction : TouchpadMapAction
    {
        public class PropertyKeyStrings
        {
            public const string NAME = "Name";
            public const string DEAD_ZONE = "DeadZone";
        }

        private HashSet<string> fullPropertySet = new HashSet<string>()
        {
            PropertyKeyStrings.NAME,
            PropertyKeyStrings.DEAD_ZONE,
        };

        private OutputActionData usedXAxisMapping =
            new OutputActionData(OutputActionData.ActionType.Empty, 0);

        private OutputActionData usedYAxisMapping =
            new OutputActionData(OutputActionData.ActionType.Empty, 0);

        private SplitAxesAnalogBindingCodes xAxisBinding;
        private SplitAxesAnalogBindingCodes yAxisBinding;

        private AxisDeadZone deadMod;

        private double xNorm = 0.0;
        private double yNorm = 0.0;
        private double prevXNorm = 0.0;
        private double prevYNorm = 0.0;

        private int relX;
        private int relY;
        private double relXRemainder;
        private double relYRemainder;

        // Just use Kozec mouse wheel coefficient for now
        private const double SENS_COEFFICIENT = 0.005;

        public SplitAxesAnalogBindingCodes XAxisBinding
        {
            get => xAxisBinding;
            set
            {
                xAxisBinding = value;
                TranslateOutputBinding();
            }
        }

        public SplitAxesAnalogBindingCodes YAxisBinding
        {
            get => yAxisBinding;
            set
            {
                yAxisBinding = value;
                TranslateOutputBinding();
            }
        }

        public AxisDeadZone DeadMod
        {
            get => deadMod;
        }

        public TouchpadAxesAction()
        {
            deadMod = new AxisDeadZone(0.0, 1.0, 0.0);
            xAxisBinding = SplitAxesAnalogBindingCodes.MouseWheelX;
            yAxisBinding = SplitAxesAnalogBindingCodes.MouseWheelY;

            TranslateOutputBinding();
        }

        public override void Prepare(Mapper mapper, ref TouchEventFrame touchFrame, bool alterState = true)
        {
            prevXNorm = xNorm;
            prevYNorm = yNorm;

            int axisXMid = touchpadDefinition.xAxis.mid, axisYMid = touchpadDefinition.yAxis.mid;
            int axisXVal = touchFrame.X;
            int axisYVal = touchFrame.Y;
            int axisXDir = axisXVal - axisXMid, axisYDir = axisYVal - axisYMid;
            bool xNegative = axisXDir < 0;
            bool yNegative = axisYDir < 0;
            int maxDirX = (!xNegative ? touchpadDefinition.xAxis.max : touchpadDefinition.xAxis.min) - axisXMid;
            int maxDirY = (!yNegative ? touchpadDefinition.yAxis.max : touchpadDefinition.yAxis.min) - axisYMid;

            deadMod.CalcOutValues(axisXDir, maxDirX, out xNorm);
            deadMod.CalcOutValues(axisYDir, maxDirY, out yNorm);

            ref TouchEventFrame previousFrame =
                ref mapper.GetPreviousTouchEventFrame(touchpadDefinition.touchCode);

            int dx = 0, dy = 0;
            if (touchFrame.Touch && previousFrame.Touch)
            {
                // Have dead zone setings just act as a cut off for relative
                if (touchFrame.X > deadMod.DeadZone &&
                    previousFrame.X > deadMod.DeadZone)
                {
                    dx = touchFrame.X - previousFrame.X;
                }
                relX = dx;

                // Have dead zone setings just act as a cut off for relative
                if (touchFrame.Y > deadMod.DeadZone &&
                    previousFrame.Y > deadMod.DeadZone)
                {
                    dy = -(touchFrame.Y - previousFrame.Y);
                }
                relY = dy;
            }

            active = true;
            activeEvent = true;
        }

        public override void Event(Mapper mapper)
        {
            if (xNorm != 0.0 || prevXNorm != 0.0)
            {
                if (xAxisBinding >= SplitAxesAnalogBindingCodes.Axis1 &&
                    xAxisBinding <= SplitAxesAnalogBindingCodes.AxisMax)
                {
                    mapper.GamepadFromAxisInput(usedXAxisMapping, xNorm);
                }
                else
                {
                    switch (xAxisBinding)
                    {
                        case SplitAxesAnalogBindingCodes.MouseX:
                            {
                                double temp = relX * SENS_COEFFICIENT;
                                if (temp > 0 && relXRemainder > 0)
                                {
                                    temp += relXRemainder;
                                }

                                int relativeX = (int)temp;
                                if (relativeX != 0)
                                {
                                    mapper.MouseX = relativeX; mapper.MouseSync = true;
                                }

                                relXRemainder = temp - relativeX;
                            }
                            
                            break;
                        case SplitAxesAnalogBindingCodes.MouseY:
                            {
                                double temp = relX * SENS_COEFFICIENT;
                                if (temp > 0 && relXRemainder > 0)
                                {
                                    temp += relXRemainder;
                                }

                                int relativeX = (int)temp;
                                if (relativeX != 0)
                                {
                                    mapper.MouseY = relativeX; mapper.MouseSync = true;
                                }

                                relXRemainder = temp - relativeX;
                            }
                            
                            break;
                        case SplitAxesAnalogBindingCodes.MouseWheelX:
                            {
                                double temp = relX * SENS_COEFFICIENT;
                                if (temp > 0 && relXRemainder > 0)
                                {
                                    temp += relXRemainder;
                                }

                                int relativeX = (int)temp;
                                if (relativeX != 0)
                                {
                                    mapper.MouseWheelX = relativeX; mapper.MouseWheelSync = true;
                                }

                                relXRemainder = temp - relativeX;
                            }
                            
                            break;
                        case SplitAxesAnalogBindingCodes.MouseWheelY:
                            {
                                double temp = relX * SENS_COEFFICIENT;
                                if (temp > 0 && relXRemainder > 0)
                                {
                                    temp += relXRemainder;
                                }

                                int relativeX = (int)temp;
                                if (relativeX != 0)
                                {
                                    mapper.MouseWheelY = relativeX; mapper.MouseWheelSync = true;
                                }

                                relXRemainder = temp - relativeX;
                            }
                            
                            break;
                        default:
                            break;
                    }
                }
            }

            if (yNorm != 0.0 || prevYNorm != 0.0)
            {
                if (yAxisBinding >= SplitAxesAnalogBindingCodes.Axis1 &&
                    yAxisBinding <= SplitAxesAnalogBindingCodes.AxisMax)
                {
                    mapper.GamepadFromAxisInput(usedYAxisMapping, yNorm);
                }
                else
                {
                    switch (yAxisBinding)
                    {
                        case SplitAxesAnalogBindingCodes.MouseX:
                            {
                                double temp = relY * SENS_COEFFICIENT;
                                if (temp > 0 && relYRemainder > 0)
                                {
                                    temp += relYRemainder;
                                }

                                int relativeY = (int)temp;
                                if (relativeY != 0)
                                {
                                    mapper.MouseX = relativeY; mapper.MouseSync = true;
                                }

                                relYRemainder = temp - relativeY;
                            }
                            
                            break;
                        case SplitAxesAnalogBindingCodes.MouseY:
                            {
                                double temp = relY * SENS_COEFFICIENT;
                                if (temp > 0 && relYRemainder > 0)
                                {
                                    temp += relYRemainder;
                                }

                                int relativeY = (int)temp;
                                if (relativeY != 0)
                                {
                                    mapper.MouseY = relativeY; mapper.MouseSync = true;
                                }

                                relYRemainder = temp - relativeY;
                            }
                            
                            break;
                        case SplitAxesAnalogBindingCodes.MouseWheelX:
                            {
                                double temp = relY * SENS_COEFFICIENT;
                                if (temp > 0 && relYRemainder > 0)
                                {
                                    temp += relYRemainder;
                                }

                                int relativeY = (int)temp;
                                if (relativeY != 0)
                                {
                                    mapper.MouseWheelX = relativeY; mapper.MouseWheelSync = true;
                                }

                                relYRemainder = temp - relativeY;
                            }
                            
                            break;
                        case SplitAxesAnalogBindingCodes.MouseWheelY:
                            {
                                double temp = relY * SENS_COEFFICIENT;
                                if (temp > 0 && relYRemainder > 0)
                                {
                                    temp += relYRemainder;
                                }

                                int relativeY = (int)temp;
                                if (relativeY != 0)
                                {
                                    mapper.MouseWheelY = relativeY; mapper.MouseWheelSync = true;
                                }

                                relYRemainder = temp - relativeY;
                            }
                            
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            xNorm = yNorm = 0.0;
            prevXNorm = prevYNorm = 0.0;
            relX = relY = 0;

            if (xAxisBinding >= SplitAxesAnalogBindingCodes.Axis1 &&
                xAxisBinding <= SplitAxesAnalogBindingCodes.AxisMax)
            {
                mapper.GamepadFromAxisInput(usedYAxisMapping, xNorm);
            }

            if (yAxisBinding >= SplitAxesAnalogBindingCodes.Axis1 &&
                yAxisBinding <= SplitAxesAnalogBindingCodes.AxisMax)
            {
                mapper.GamepadFromAxisInput(usedYAxisMapping, yNorm);
            }

            active = activeEvent = false;
        }

        public override void SoftRelease(Mapper mapper, MapAction checkAction, bool resetState = true)
        {
            active = activeEvent = false;
        }

        public override void SoftCopyFromParent(TouchpadMapAction parentAction)
        {
            if (parentAction is TouchpadAxesAction tempAxesAction)
            {
                base.SoftCopyFromParent(parentAction);

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                foreach (string parentPropType in useParentProList)
                {
                    switch (parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = tempAxesAction.name;
                            break;
                        case PropertyKeyStrings.DEAD_ZONE:
                            deadMod.DeadZone = tempAxesAction.deadMod.DeadZone;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void TranslateOutputBinding()
        {
            switch(xAxisBinding)
            {
                case SplitAxesAnalogBindingCodes.AxisLX:
                    usedXAxisMapping.OutputType = OutputActionData.ActionType.GamepadControl;
                    usedXAxisMapping.JoypadCode = JoypadActionCodes.AxisLX;
                    break;
                case SplitAxesAnalogBindingCodes.AxisLY:
                    usedXAxisMapping.OutputType = OutputActionData.ActionType.GamepadControl;
                    usedXAxisMapping.JoypadCode = JoypadActionCodes.AxisLY;
                    break;
                case SplitAxesAnalogBindingCodes.AxisRX:
                    usedXAxisMapping.OutputType = OutputActionData.ActionType.GamepadControl;
                    usedXAxisMapping.JoypadCode = JoypadActionCodes.AxisRX;
                    break;
                case SplitAxesAnalogBindingCodes.AxisRY:
                    usedXAxisMapping.OutputType = OutputActionData.ActionType.GamepadControl;
                    usedXAxisMapping.JoypadCode = JoypadActionCodes.AxisRY;
                    break;
                case SplitAxesAnalogBindingCodes.AxisLTrigger:
                    usedXAxisMapping.OutputType = OutputActionData.ActionType.GamepadControl;
                    usedXAxisMapping.JoypadCode = JoypadActionCodes.AxisLTrigger;
                    break;
                case SplitAxesAnalogBindingCodes.AxisRTrigger:
                    usedXAxisMapping.OutputType = OutputActionData.ActionType.GamepadControl;
                    usedXAxisMapping.JoypadCode = JoypadActionCodes.AxisRTrigger;
                    break;
                default:
                    break;
            }

            switch (yAxisBinding)
            {
                case SplitAxesAnalogBindingCodes.AxisLX:
                    usedYAxisMapping.OutputType = OutputActionData.ActionType.GamepadControl;
                    usedYAxisMapping.JoypadCode = JoypadActionCodes.AxisLX;
                    break;
                case SplitAxesAnalogBindingCodes.AxisLY:
                    usedYAxisMapping.OutputType = OutputActionData.ActionType.GamepadControl;
                    usedYAxisMapping.JoypadCode = JoypadActionCodes.AxisLY;
                    break;
                case SplitAxesAnalogBindingCodes.AxisRX:
                    usedYAxisMapping.OutputType = OutputActionData.ActionType.GamepadControl;
                    usedYAxisMapping.JoypadCode = JoypadActionCodes.AxisRX;
                    break;
                case SplitAxesAnalogBindingCodes.AxisRY:
                    usedYAxisMapping.OutputType = OutputActionData.ActionType.GamepadControl;
                    usedYAxisMapping.JoypadCode = JoypadActionCodes.AxisRY;
                    break;
                case SplitAxesAnalogBindingCodes.AxisLTrigger:
                    usedYAxisMapping.OutputType = OutputActionData.ActionType.GamepadControl;
                    usedYAxisMapping.JoypadCode = JoypadActionCodes.AxisLTrigger;
                    break;
                case SplitAxesAnalogBindingCodes.AxisRTrigger:
                    usedYAxisMapping.OutputType = OutputActionData.ActionType.GamepadControl;
                    usedYAxisMapping.JoypadCode = JoypadActionCodes.AxisRTrigger;
                    break;
                default:
                    break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sensorit.Base;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.GyroActions
{
    public enum GyroMouseXAxisChoice
    {
        Yaw,
        Roll,
    }

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

    public struct GyroMouseParams
    {
        public const bool JITTER_COMPENSATION_DEFAULT = true;
        public const double IN_GAME_SENS_DEFAULT = 1.0;

        public double deadzone;
        public JoypadActionCodes[] gyroTriggerButtons;
        public bool andCond;
        public bool triggerActivates;
        public double realWorldCalibration;
        public double inGameSens;
        public double sensitivity;
        public double verticalScale;
        public bool invertX;
        public bool invertY;
        public GyroMouseXAxisChoice useForXAxis;
        public double minThreshold;
        public bool toggleAction;
        public bool smoothing;
        public bool jitterCompensation;
        public SmoothingFilterSettings smoothingFilterSettings;
        //public double oneEuroMinCutoff;
        //public double oneEuroMinBeta;
    }

    public class GyroMouse : GyroMapAction
    {
        public class PropertyKeyStrings
        {
            public const string NAME = "Name";
            public const string DEAD_ZONE = "DeadZone";
            public const string SENSITIVITY = "Sensitivity";
            public const string VERTICAL_SCALE = "VerticalScale";
            public const string INVERT_X = "InvertX";
            public const string INVERT_Y = "InvertY";
            public const string X_AXIS = "XAxis";
            public const string MIN_THRESHOLD = "MinThreshold";
            //public const string OUTPUT_CURVE = "OutputCurve";
            public const string REAL_WORLD_CALIBRATION = "RealWorldCalibration";
            public const string IN_GAME_SENS = "InGameSens";

            public const string TRIGGER_BUTTONS = "Triggers";
            public const string TRIGGER_ACTIVATE = "TriggersActivate";
            public const string TRIGGER_EVAL_COND = "TriggersEvalCond";
            public const string TOGGLE_ACTION = "ToggleAction";
            public const string JITTER_COMPENSATION = "JitterCompensation";
            public const string SMOOTHING_ENABLED = "SmoothingEnabled";
            public const string SMOOTHING_FILTER = "SmoothingFilter";
            //public const string SMOOTHING_MINCUTOFF = "SmoothingMinCutoff";
            //public const string SMOOTHING_MINBETA = "SmoothingMinBeta";
        }

        private HashSet<string> fullPropertySet = new HashSet<string>()
        {
            PropertyKeyStrings.NAME,
            PropertyKeyStrings.DEAD_ZONE,
            PropertyKeyStrings.SENSITIVITY,
            PropertyKeyStrings.VERTICAL_SCALE,
            PropertyKeyStrings.INVERT_X,
            PropertyKeyStrings.INVERT_Y,
            PropertyKeyStrings.X_AXIS,
            PropertyKeyStrings.MIN_THRESHOLD,
            PropertyKeyStrings.REAL_WORLD_CALIBRATION,
            PropertyKeyStrings.IN_GAME_SENS,
            PropertyKeyStrings.TRIGGER_BUTTONS,
            PropertyKeyStrings.TRIGGER_ACTIVATE,
            PropertyKeyStrings.TRIGGER_EVAL_COND,
            PropertyKeyStrings.TOGGLE_ACTION,
            PropertyKeyStrings.SMOOTHING_ENABLED,
            PropertyKeyStrings.SMOOTHING_FILTER,
            //PropertyKeyStrings.SMOOTHING_MINCUTOFF,
            //PropertyKeyStrings.SMOOTHING_MINBETA,
        };

        public const string ACTION_TYPE_NAME = "GyroMouseAction";
        private const bool DEFAULT_SMOOTHING_ENABLED = true;

        private double xMotion;
        private double yMotion;
        public GyroMouseParams mouseParams;
        private bool previousTriggerActivated;
        private bool toggleActiveState;
        private bool useParentSmoothingFilter;

        //private OneEuroFilter smoothFilter = new OneEuroFilter(1.0, 1.0);

        public GyroMouse()
        {
            actionTypeName = ACTION_TYPE_NAME;
            mouseParams = new GyroMouseParams()
            {
                sensitivity = 1.0,
                deadzone = 0.6,
                realWorldCalibration = 5.00,
                inGameSens = GyroMouseParams.IN_GAME_SENS_DEFAULT,
                verticalScale = 1.0,
                triggerActivates = true,
                andCond = true,
                gyroTriggerButtons = new JoypadActionCodes[1]
                {
                    JoypadActionCodes.AlwaysOn,
                },
                jitterCompensation = GyroMouseParams.JITTER_COMPENSATION_DEFAULT,
                smoothing = DEFAULT_SMOOTHING_ENABLED,
            };

            mouseParams.smoothingFilterSettings.Init();
            onlyOnPrimary = true;
        }

        public GyroMouse(GyroMouseParams mouseParams)
        {
            actionTypeName = ACTION_TYPE_NAME;
            this.mouseParams = mouseParams;
            onlyOnPrimary = true;
        }

        public GyroMouse(GyroMouse parentAction)
        {
            actionTypeName = ACTION_TYPE_NAME;
            this.parentAction = parentAction;
            this.mouseParams = parentAction.mouseParams;
            onlyOnPrimary = true;
        }

        public override void Prepare(Mapper mapper, ref GyroEventFrame gyroFrame, bool alterState = true)
        {
            //const int deadZone = 28;
            //const int deadZone = 18;
            const double GYRO_MOUSE_COEFFICIENT = 0.025;
            const double GYRO_MOUSE_OFFSET = 0.3;
            //const double GYRO_MOUSE_OFFSET = 0.0;

            JoypadActionCodes[] tempTriggerButtons = mouseParams.gyroTriggerButtons;
            //bool triggerButtonActive = tempTriggerButton == JoypadActionCodes.Empty ||
            //    mapper.IsButtonActive(mouseParams.gyroTriggerButton);

            bool triggerButtonActive = mapper.IsButtonsActiveDraft(tempTriggerButtons,
                mouseParams.andCond);

            bool triggerActivated = true;
            if (!mouseParams.triggerActivates && triggerButtonActive)
            {
                triggerActivated = false;
                //previousTriggerActivated = triggerActivated;
            }
            else if (mouseParams.triggerActivates && !triggerButtonActive)
            {
                triggerActivated = false;
                //previousTriggerActivated = triggerActivated;
            }

            if (mouseParams.toggleAction)
            {
                if (triggerActivated && triggerActivated != previousTriggerActivated)
                {
                    toggleActiveState = !toggleActiveState;
                }

                previousTriggerActivated = triggerActivated;
                triggerActivated = toggleActiveState;
            }
            else
            {
                previousTriggerActivated = triggerActivated;
            }

            if (!triggerActivated)
            {
                mapper.MouseXRemainder = mapper.MouseYRemainder = 0.0;
                mouseParams.smoothingFilterSettings.filterX.Filter(0.0, mapper.CurrentRate);
                mouseParams.smoothingFilterSettings.filterY.Filter(0.0, mapper.CurrentRate);

                active = false;
                activeEvent = false;
                return;
            }

            double offset = gyroSensDefinition.mouseOffset;
            //double coefficient = gyroSensDefinition.mouseCoefficient * mouseParams.sensitivity;
            //double coefficient = (120.0 / 3.0) * mouseParams.sensitivity; // RWC / InGameSens * sens_multiplier

            // RWC / InGameSens * sens_multiplier
            double coefficient = (mouseParams.realWorldCalibration / mouseParams.inGameSens) * mouseParams.sensitivity;
            double deadZone = mouseParams.deadzone;

            double timeElapsed = gyroFrame.timeElapsed;
            double oldTimeElapsed = timeElapsed;
            timeElapsed = timeElapsed - (mapper.remainderCutoff(timeElapsed * 10000.0, 1.0) / 10000.0);
            //Trace.WriteLine($"BEFORE: {oldTimeElapsed} | AFTER {timeElapsed}");
            //Trace.WriteLine(timeElapsed);
            //double timeElapsed = current.timeElapsed;
            // Take possible lag state into account. Main routine will make sure to skip this method
            //if (previous.timeElapsed <= 0.002)
            //{
            //    timeElapsed += previous.timeElapsed;
            //    currentRate = 1.0 / timeElapsed;
            //}

            // Base speed 5 ms
            //double tempDouble = timeElapsed * 3 * 66.67;
            //double tempDouble = timeElapsed * 3 * gyroFrame.elapsedReference;
            double tempDouble = timeElapsed * gyroFrame.elapsedReference;
            int deltaX = mouseParams.useForXAxis == GyroMouseXAxisChoice.Yaw ?
                gyroFrame.GyroYaw : gyroFrame.GyroRoll;

            int deltaY = gyroFrame.GyroPitch;
            double tempAngle = Math.Atan2(-deltaY, deltaX);
            double normX = Math.Abs(Math.Cos(tempAngle));
            double normY = Math.Abs(Math.Sin(tempAngle));
            int signX = Math.Sign(deltaX);
            int signY = Math.Sign(deltaY);

            double deltaAngVelX = (mouseParams.useForXAxis == GyroMouseXAxisChoice.Yaw ?
                gyroFrame.AngGyroYaw : gyroFrame.AngGyroRoll);
            double deltaAngVelY = gyroFrame.AngGyroPitch;

            //Trace.WriteLine($"{deltaX} {deltaY}");

            double deadzoneX = Math.Abs(normX * deadZone);
            double deadzoneY = Math.Abs(normY * deadZone);

            //Trace.WriteLine($"{gyroFrame.AngGyroYaw} {deltaX} {deadZone}");

            if (Math.Abs(deltaAngVelX) > deadzoneX)
            {
                deltaAngVelX -= signX * deadzoneX;
            }
            else
            {
                deltaAngVelX = 0;
            }

            if (Math.Abs(deltaAngVelY) > deadzoneY)
            {
                deltaAngVelY -= signY * deadzoneY;
            }
            else
            {
                deltaAngVelY = 0;
            }

            double slope = (1.0 - 0.40) / (11.25 - 0.0);
            double intercept = slope - 0.40;
            double dps_test = 180.0 / 16.0;

            if (deltaAngVelX != 0 && (deltaAngVelX * signX) < (dps_test * normX))
            {
                deltaAngVelX = ((slope * Math.Abs(deltaAngVelX) - intercept) * deltaAngVelX);
                //Trace.WriteLine($"DANGEROUS: {deltaAngVelX}");
            }

            if (deltaAngVelY != 0 && (deltaAngVelY * signY) < (dps_test * normY))
            {
                deltaAngVelY = ((slope * Math.Abs(deltaAngVelY) - intercept) * deltaAngVelY);
            }

            double xAng = deltaAngVelX * timeElapsed;
            double yAng = deltaAngVelY * timeElapsed;

            xMotion = deltaX != 0 ? coefficient * (xAng * tempDouble)
                + (normX * (offset * signX)) : 0;

            yMotion = deltaY != 0 ? coefficient * (yAng * tempDouble)
                + (normY * (offset * signY)) : 0;

            if (mouseParams.verticalScale != 1.0)
            {
                yMotion = mouseParams.verticalScale * yMotion;
            }

            if (mouseParams.jitterCompensation)
            {
                // Possibly expose threshold later
                const double threshold = 0.48;
                const float thresholdF = (float)threshold;

                double absX = Math.Abs(xMotion);
                if (absX <= normX * threshold)
                {
                    xMotion = signX * Math.Pow(absX / thresholdF, 1.408) * threshold;
                }

                double absY = Math.Abs(yMotion);
                if (absY <= normY * threshold)
                {
                    yMotion = signY * Math.Pow(absY / thresholdF, 1.408) * threshold;
                }
            }

            if (xMotion != 0.0 || yMotion != 0.0)
            {
                active = true;
            }
            else
            {
                active = false;

                mouseParams.smoothingFilterSettings.filterX.Filter(0.0, mapper.CurrentRate);
                mouseParams.smoothingFilterSettings.filterY.Filter(0.0, mapper.CurrentRate);
            }

            activeEvent = true;
        }

        public override void Event(Mapper mapper)
        {
            double tempX = xMotion, tempY = yMotion;
            /*if (mouseParams.smoothing)
            {
                tempX = smoothFilter.Filter(xMotion, mapper.CurrentRate);
                tempY = smoothFilter.Filter(yMotion, mapper.CurrentRate);
            }
            */

            double outXMotion = !mouseParams.invertX ? tempX : -1.0 * tempX;
            double outYMotion = !mouseParams.invertY ? tempY : -1.0 * tempY;

            bool mouseSync = true;
            if (mouseParams.minThreshold != 1.0)
            {
                double distSqu = (xMotion * xMotion) + (yMotion * yMotion);
                if (distSqu <= (mouseParams.minThreshold * mouseParams.minThreshold))
                {
                    outXMotion = 0.0; outYMotion = 0.0;
                    mapper.MouseXRemainder = outXMotion;
                    mapper.MouseYRemainder = outYMotion;
                    mouseSync = false;
                }
            }

            //mapper.MouseX = outXMotion; mapper.MouseY = outYMotion;
            //mapper.MouseSync = mouseSync;

            if (mouseParams.smoothing)
            {
                //mapper.MouseX = outXMotion; mapper.MouseY = outYMotion;
                mapper.GenerateMouseEventFilteredV2(mouseParams.smoothingFilterSettings.filterX,
                    mouseParams.smoothingFilterSettings.filterY,
                    ref outXMotion, ref outYMotion);

                mapper.MouseX += outXMotion; mapper.MouseY += outYMotion;
                mapper.MouseSync = mouseSync;
                //mapper.MouseEventFired = true;

                //tempX = mouseParams.smoothingFilterSettings.filterX.Filter(tempX,
                //    mapper.CurrentRate);

                //tempY = mouseParams.smoothingFilterSettings.filterY.Filter(tempY,
                //    mapper.CurrentRate);
            }
            else
            {
                // Allow mapper to handle event
                mapper.MouseX += outXMotion; mapper.MouseY += outYMotion;
                mapper.MouseSync = mouseSync;
            }

            if (xMotion != 0.0 || yMotion != 0.0)
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
            xMotion = yMotion = 0.0;
            active = false;
            activeEvent = false;
            toggleActiveState = false;
            previousTriggerActivated = false;
            //smoothFilter.Reset();
            mouseParams.smoothingFilterSettings.filterX.Reset();
            mouseParams.smoothingFilterSettings.filterY.Reset();
        }

        public override void SoftRelease(Mapper mapper, MapAction checkAction, bool resetState = true)
        {
            xMotion = yMotion = 0.0;
            active = false;
            activeEvent = false;
            toggleActiveState = false;
            previousTriggerActivated = false;

            if (!useParentSmoothingFilter)
            {
                //smoothFilter.Reset();
                mouseParams.smoothingFilterSettings.filterX.Reset();
                mouseParams.smoothingFilterSettings.filterY.Reset();
            }
        }

        public override void BlankEvent(Mapper mapper)
        {
            mapper.MouseXRemainder = mapper.MouseYRemainder = 0.0;
            active = false;
            activeEvent = false;
            toggleActiveState = false;
            previousTriggerActivated = false;

            if (!useParentSmoothingFilter)
            {
                //smoothFilter.Reset();
                mouseParams.smoothingFilterSettings.filterX.Reset();
                mouseParams.smoothingFilterSettings.filterY.Reset();
            }
        }

        public override GyroMapAction DuplicateAction()
        {
            return new GyroMouse(this);
        }

        public override void SoftCopyFromParent(GyroMapAction parentAction)
        {
            if (parentAction is GyroMouse tempMouseAction)
            {
                base.SoftCopyFromParent(parentAction);

                this.parentAction = parentAction;
                tempMouseAction.hasLayeredAction = true;
                mappingId = tempMouseAction.mappingId;

                gyroSensDefinition = new GyroSensDefinition(tempMouseAction.gyroSensDefinition);

                tempMouseAction.NotifyPropertyChanged += TempMouseAction_NotifyPropertyChanged;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                //bool updateSmoothing = false;
                foreach (string parentPropType in useParentProList)
                {
                    switch(parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = tempMouseAction.name;
                            break;
                        case PropertyKeyStrings.DEAD_ZONE:
                            mouseParams.deadzone = tempMouseAction.mouseParams.deadzone;
                            break;
                        case PropertyKeyStrings.TRIGGER_BUTTONS:
                            mouseParams.gyroTriggerButtons = tempMouseAction.mouseParams.gyroTriggerButtons;
                            break;
                        case PropertyKeyStrings.TRIGGER_ACTIVATE:
                            mouseParams.triggerActivates = tempMouseAction.mouseParams.triggerActivates;
                            break;
                        case PropertyKeyStrings.TRIGGER_EVAL_COND:
                            mouseParams.andCond = tempMouseAction.mouseParams.andCond;
                            break;
                        case PropertyKeyStrings.REAL_WORLD_CALIBRATION:
                            mouseParams.realWorldCalibration = tempMouseAction.mouseParams.realWorldCalibration;
                            break;
                        case PropertyKeyStrings.IN_GAME_SENS:
                            mouseParams.inGameSens = tempMouseAction.mouseParams.inGameSens;
                            break;
                        case PropertyKeyStrings.SENSITIVITY:
                            mouseParams.sensitivity = tempMouseAction.mouseParams.sensitivity;
                            break;
                        case PropertyKeyStrings.VERTICAL_SCALE:
                            mouseParams.verticalScale = tempMouseAction.mouseParams.verticalScale;
                            break;
                        case PropertyKeyStrings.INVERT_X:
                            mouseParams.invertX = tempMouseAction.mouseParams.invertX;
                            break;
                        case PropertyKeyStrings.INVERT_Y:
                            mouseParams.invertY = tempMouseAction.mouseParams.invertY;
                            break;
                        case PropertyKeyStrings.X_AXIS:
                            mouseParams.useForXAxis = tempMouseAction.mouseParams.useForXAxis;
                            break;
                        case PropertyKeyStrings.MIN_THRESHOLD:
                            mouseParams.minThreshold = tempMouseAction.mouseParams.minThreshold;
                            break;
                        case PropertyKeyStrings.TOGGLE_ACTION:
                            mouseParams.toggleAction = tempMouseAction.mouseParams.toggleAction;
                            ResetToggleActiveState();
                            break;
                        case PropertyKeyStrings.JITTER_COMPENSATION:
                            mouseParams.jitterCompensation = tempMouseAction.mouseParams.jitterCompensation;
                            break;
                        case PropertyKeyStrings.SMOOTHING_ENABLED:
                            mouseParams.smoothing = tempMouseAction.mouseParams.smoothing;
                            break;
                        case PropertyKeyStrings.SMOOTHING_FILTER:
                            mouseParams.smoothingFilterSettings = tempMouseAction.mouseParams.smoothingFilterSettings;
                            useParentSmoothingFilter = true;
                            break;
                        //case PropertyKeyStrings.SMOOTHING_MINCUTOFF:
                        //    mouseParams.oneEuroMinCutoff = tempMouseAction.mouseParams.oneEuroMinCutoff;
                        //    updateSmoothing = true;
                        //    break;
                        //case PropertyKeyStrings.SMOOTHING_MINBETA:
                        //    mouseParams.oneEuroMinBeta = tempMouseAction.mouseParams.oneEuroMinBeta;
                        //    updateSmoothing = true;
                        //    break;
                        default:
                            break;
                    }
                }

                //if (updateSmoothing)
                //{
                //    UpdateSmoothingFilter();
                //}
            }
        }

        private void TempMouseAction_NotifyPropertyChanged(object sender, NotifyPropertyChangeArgs e)
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

            GyroMouse tempMouseAction = parentAction as GyroMouse;

            //bool updateSmoothing = false;
            switch (propertyName)
            {
                case PropertyKeyStrings.NAME:
                    name = tempMouseAction.name;
                    break;
                case PropertyKeyStrings.DEAD_ZONE:
                    mouseParams.deadzone = tempMouseAction.mouseParams.deadzone;
                    break;
                case PropertyKeyStrings.TRIGGER_BUTTONS:
                    mouseParams.gyroTriggerButtons = tempMouseAction.mouseParams.gyroTriggerButtons;
                    break;
                case PropertyKeyStrings.TRIGGER_ACTIVATE:
                    mouseParams.triggerActivates = tempMouseAction.mouseParams.triggerActivates;
                    break;
                case PropertyKeyStrings.TRIGGER_EVAL_COND:
                    mouseParams.andCond = tempMouseAction.mouseParams.andCond;
                    break;
                case PropertyKeyStrings.REAL_WORLD_CALIBRATION:
                    mouseParams.realWorldCalibration = tempMouseAction.mouseParams.realWorldCalibration;
                    break;
                case PropertyKeyStrings.IN_GAME_SENS:
                    mouseParams.inGameSens = tempMouseAction.mouseParams.inGameSens;
                    break;
                case PropertyKeyStrings.SENSITIVITY:
                    mouseParams.sensitivity = tempMouseAction.mouseParams.sensitivity;
                    break;
                case PropertyKeyStrings.VERTICAL_SCALE:
                    mouseParams.verticalScale = tempMouseAction.mouseParams.verticalScale;
                    break;
                case PropertyKeyStrings.INVERT_X:
                    mouseParams.invertX = tempMouseAction.mouseParams.invertX;
                    break;
                case PropertyKeyStrings.INVERT_Y:
                    mouseParams.invertY = tempMouseAction.mouseParams.invertY;
                    break;
                case PropertyKeyStrings.X_AXIS:
                    mouseParams.useForXAxis = tempMouseAction.mouseParams.useForXAxis;
                    break;
                case PropertyKeyStrings.MIN_THRESHOLD:
                    mouseParams.minThreshold = tempMouseAction.mouseParams.minThreshold;
                    break;
                case PropertyKeyStrings.TOGGLE_ACTION:
                    mouseParams.toggleAction = tempMouseAction.mouseParams.toggleAction;
                    ResetToggleActiveState();
                    break;
                case PropertyKeyStrings.JITTER_COMPENSATION:
                    mouseParams.jitterCompensation = tempMouseAction.mouseParams.jitterCompensation;
                    break;
                case PropertyKeyStrings.SMOOTHING_ENABLED:
                    mouseParams.smoothing = tempMouseAction.mouseParams.smoothing;
                    //updateSmoothing = true;
                    break;
                case PropertyKeyStrings.SMOOTHING_FILTER:
                    mouseParams.smoothingFilterSettings = tempMouseAction.mouseParams.smoothingFilterSettings;
                    useParentSmoothingFilter = true;
                    break;
                //case PropertyKeyStrings.SMOOTHING_MINCUTOFF:
                //    mouseParams.oneEuroMinCutoff = tempMouseAction.mouseParams.oneEuroMinCutoff;
                //    updateSmoothing = true;
                //    break;
                //case PropertyKeyStrings.SMOOTHING_MINBETA:
                //    mouseParams.oneEuroMinBeta = tempMouseAction.mouseParams.oneEuroMinBeta;
                //    updateSmoothing = true;
                //    break;
                default:
                    break;
            }

            //if (updateSmoothing)
            //{
            //    UpdateSmoothingFilter();
            //}
        }

        private void ResetToggleActiveState()
        {
            toggleActiveState = false;
            previousTriggerActivated = false;
        }

        //public void UpdateSmoothingFilter()
        //{
        //    smoothFilter = new OneEuroFilter(mouseParams.oneEuroMinCutoff,
        //        mouseParams.oneEuroMinBeta);
        //}
    }
}

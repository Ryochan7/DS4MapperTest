using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sensorit.Base;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.MapperUtil;

namespace DS4MapperTest.GyroActions
{
    public enum GyroMouseJoystickOuputAxes
    {
        All,
        XAxis,
        YAxis,
    }

    public struct GyroMouseJoystickParams
    {
        public const bool JITTER_COMPENSATION_DEFAULT = true;

        public double deadZone;
        public double maxZone;
        public double antiDeadzoneX;
        public double antiDeadzoneY;
        public JoypadActionCodes[] gyroTriggerButtons;
        public bool andCond;
        public bool triggerActivates;
        public double verticalScale;
        public GyroMouseXAxisChoice useForXAxis;
        public bool invertX;
        public bool invertY;
        public bool jitterCompensation;
        public GyroMouseJoystickOuputAxes outputAxes;
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
        public bool maxOutputEnabled;
        public double maxOutput;
        public bool toggleAction;
        public bool smoothing;
        public SmoothingFilterSettings smoothingFilterSettings;
        //public double oneEuroMinCutoff;
        //public double oneEuroMinBeta;

        public event EventHandler OutputStickChanged;
    }

    public class GyroMouseJoystick : GyroMapAction
    {
        public class PropertyKeyStrings
        {
            public const string NAME = "Name";
            public const string DEAD_ZONE = "DeadZone";
            public const string MAX_ZONE = "MaxZone";
            public const string ANTIDEAD_ZONE_X = "AntiDeadZoneX";
            public const string ANTIDEAD_ZONE_Y = "AntiDeadZoneY";
            public const string X_AXIS = "XAxis";
            public const string INVERT_X = "InvertX";
            public const string INVERT_Y = "InvertY";
            public const string VERTICAL_SCALE = "VerticalScale";
            public const string OUTPUT_AXES = "OutputAxes";
            public const string OUTPUT_STICK = "OuputStick";
            public const string MAX_OUTPUT = "MaxOutput";
            public const string MAX_OUTPUT_ENABLED = "MaxOutputEnabled";
            //public const string OUTPUT_CURVE = "OutputCurve";

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
            PropertyKeyStrings.MAX_ZONE,
            PropertyKeyStrings.X_AXIS,
            PropertyKeyStrings.ANTIDEAD_ZONE_X,
            PropertyKeyStrings.ANTIDEAD_ZONE_Y,
            PropertyKeyStrings.INVERT_X,
            PropertyKeyStrings.INVERT_Y,
            PropertyKeyStrings.VERTICAL_SCALE,
            PropertyKeyStrings.OUTPUT_AXES,
            PropertyKeyStrings.OUTPUT_STICK,
            PropertyKeyStrings.MAX_OUTPUT,
            PropertyKeyStrings.MAX_OUTPUT_ENABLED,
            PropertyKeyStrings.TRIGGER_BUTTONS,
            PropertyKeyStrings.TRIGGER_ACTIVATE,
            PropertyKeyStrings.TRIGGER_EVAL_COND,
            PropertyKeyStrings.TOGGLE_ACTION,
            PropertyKeyStrings.JITTER_COMPENSATION,
            PropertyKeyStrings.SMOOTHING_ENABLED,
            PropertyKeyStrings.SMOOTHING_FILTER,
            //PropertyKeyStrings.SMOOTHING_MINCUTOFF,
            //PropertyKeyStrings.SMOOTHING_MINBETA,
        };

        public const string ACTION_TYPE_NAME = "GyroMouseJoystickAction";
        private const double DEFAULT_MJOY_MINCUTOFFF = 0.6;
        private const double DEFAULT_MJOY_BETA = 0.7;
        private const bool DEFAULT_SMOOTHING_ENABLED = true;

        public GyroMouseJoystickParams mStickParams;
        private double xNorm, yNorm;
        private double prevXNorm, prevYNorm;
        private OutputActionData actionData = new OutputActionData(OutputActionData.ActionType.GamepadControl, StickActionCodes.RS);
        private bool previousTriggerActivated;
        private bool toggleActiveState;
        private bool useParentSmoothingFilter;
        //private OneEuroFilter smoothFilter = new OneEuroFilter(1.0, 1.0);

        //private event EventHandler<NotifyPropertyChangeArgs> NotifyPropertyChanged;

        public GyroMouseJoystick()
        {
            actionTypeName = ACTION_TYPE_NAME;
            mStickParams = new GyroMouseJoystickParams()
            {
                deadZone = 1.50, // dps
                maxZone = 37.5, // dps
                antiDeadzoneX = 0.45,
                antiDeadzoneY = 0.45,
                verticalScale = 1.0,
                outputAxes = GyroMouseJoystickOuputAxes.All,
                outputStick = StickActionCodes.RS,
                maxOutput = 1.0,
                andCond = true,
                gyroTriggerButtons = new JoypadActionCodes[1]
                {
                    JoypadActionCodes.AlwaysOn,
                },
                jitterCompensation = false,
                smoothing = DEFAULT_SMOOTHING_ENABLED,
            };

            mStickParams.smoothingFilterSettings.Init();
            mStickParams.smoothingFilterSettings.minCutOff = DEFAULT_MJOY_MINCUTOFFF;
            mStickParams.smoothingFilterSettings.beta = DEFAULT_MJOY_BETA;
            mStickParams.smoothingFilterSettings.UpdateSmoothingFilters();

            mStickParams.OutputStickChanged += MStickParms_OutputStickChanged;
        }

        private void MStickParms_OutputStickChanged(object sender, EventArgs e)
        {
            actionData.StickCode = mStickParams.outputStick;
        }

        public GyroMouseJoystick(GyroMouseJoystickParams mstickParams)
        {
            actionTypeName = ACTION_TYPE_NAME;
            this.mStickParams = mstickParams;
            mStickParams.OutputStickChanged += MStickParms_OutputStickChanged;
        }

        public GyroMouseJoystick(GyroMouseJoystick parentAction)
        {
            actionTypeName = ACTION_TYPE_NAME;
            this.parentAction = parentAction;
            this.mStickParams = parentAction.mStickParams;
            mStickParams.OutputStickChanged += MStickParms_OutputStickChanged;
        }

        public override void Prepare(Mapper mapper, ref GyroEventFrame gyroFrame, bool alterState = true)
        {
            //const int deadzone = 24;
            //const int deadzone = 24;
            //const int maxZone = 600;
            //const double antidead = 0.54;
            //const double antidead = 0.45;

            double deadzone = mStickParams.deadZone;
            double maxZone = mStickParams.maxZone;

            double timeElapsed = gyroFrame.timeElapsed;

            JoypadActionCodes[] tempTriggerButtons = mStickParams.gyroTriggerButtons;
            bool triggerButtonActive = mapper.IsButtonsActiveDraft(tempTriggerButtons, mStickParams.andCond);
            bool triggerActivated = true;

            double currentRate = gyroFrame.CurrentRate;
            //if (tempTriggerButton != JoypadActionCodes.Empty)
            {
                //bool triggerButtonActive = mapper.IsButtonActive(mStickParms.gyroTriggerButton);
                if (!mStickParams.triggerActivates && triggerButtonActive)
                {
                    triggerActivated = false;
                }
                else if (mStickParams.triggerActivates && !triggerButtonActive)
                {
                    triggerActivated = false;
                }
            }

            if (mStickParams.toggleAction)
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
                if (prevXNorm != 0.0 || prevYNorm != 0.0)
                {
                    mapper.IntermediateStateRef.Dirty = true;
                }

                prevXNorm = xNorm; prevYNorm = yNorm;
                xNorm = yNorm = 0.0;
                //mapper.IntermediateStateRef.Dirty = true;

                mStickParams.smoothingFilterSettings.filterX.Filter(0.0, currentRate);
                mStickParams.smoothingFilterSettings.filterY.Filter(0.0, currentRate);

                active = false;
                activeEvent = false;
                return;
            }

            // Base speed 15 ms
            //double tempDouble = timeElapsed * 66.67;
            double tempDouble = timeElapsed * gyroFrame.elapsedReference;
            //Console.WriteLine("Elasped: ({0}) DOUBLE {1}", current.timeElapsed, tempDouble);
            int deltaX = mStickParams.useForXAxis == GyroMouseXAxisChoice.Yaw ?
                gyroFrame.GyroYaw : gyroFrame.GyroRoll;
            int deltaY = -gyroFrame.GyroPitch;

            double tempAngle = Math.Atan2(-deltaY, deltaX);
            double normX = Math.Abs(Math.Cos(tempAngle));
            double normY = Math.Abs(Math.Sin(tempAngle));
            int signX = Math.Sign(deltaX);
            int signY = Math.Sign(deltaY);

            double deltaAngVelX = (mStickParams.useForXAxis == GyroMouseXAxisChoice.Yaw ?
                gyroFrame.AngGyroYaw : gyroFrame.AngGyroRoll);
            double deltaAngVelY = gyroFrame.AngGyroPitch;

            double deadzoneX = Math.Abs(normX * deadzone);
            double deadzoneY = Math.Abs(normY * deadzone);

            double maxValX = signX * maxZone;
            double maxValY = signY * maxZone;

            double xratio = 0.0, yratio = 0.0;
            double antiX = mStickParams.antiDeadzoneX * normX;
            double antiY = mStickParams.antiDeadzoneY * normY;

            if (Math.Abs(deltaAngVelX) > deadzoneX)
            {
                deltaAngVelX -= signX * deadzoneX;
                deltaAngVelX = deltaAngVelX * tempDouble;
                deltaAngVelX = (deltaAngVelX < 0.0 && deltaAngVelX < -maxValX) ? -maxValX :
                    (deltaAngVelX > 0.0 && deltaAngVelX > maxValX) ? maxValX : deltaAngVelX;
                //if (deltaAngVelX != maxValX) deltaAngVelX -= deltaAngVelX % (signX * GyroMouseFuzz);
            }
            else
            {
                deltaAngVelX = 0.0;
            }

            if (Math.Abs(deltaAngVelY) > deadzoneY)
            {
                deltaAngVelY -= signY * deadzoneY;
                deltaAngVelY = deltaY * tempDouble;
                deltaAngVelY = (deltaAngVelY < 0.0 && deltaAngVelY < -maxValY) ? -maxValY :
                    (deltaAngVelY > 0.0 && deltaAngVelY > maxValY) ? maxValY : deltaAngVelY;
                //if (deltaAngVelY != maxValY) deltaAngVelY -= deltaAngVelY % (signY * GyroMouseFuzz);
            }
            else
            {
                deltaAngVelY = 0.0;
            }

            if (mStickParams.jitterCompensation)
            {
                // Possibly expose threshold later
                const double threshold = 0.125; // dps
                const float thresholdF = (float)threshold;

                double absX = Math.Abs(deltaAngVelX);
                if (absX <= normX * threshold)
                {
                    deltaAngVelX = (signX * Math.Pow(absX / thresholdF, 1.408) * threshold);
                }

                double absY = Math.Abs(deltaAngVelY);
                if (absY <= normY * threshold)
                {
                    deltaAngVelY = (signY * Math.Pow(absY / thresholdF, 1.408) * threshold);
                }
            }

            //deltaAngVelX = mapper.MStickFilterX.Filter(deltaAngVelX, mapper.CurrentRate);
            //deltaAngVelY = mapper.MStickFilterY.Filter(deltaAngVelY, mapper.CurrentRate);
            if (mStickParams.smoothing)
            {
                deltaAngVelX = mStickParams.smoothingFilterSettings.filterX.Filter(deltaAngVelX, currentRate);
                deltaAngVelY = mStickParams.smoothingFilterSettings.filterY.Filter(deltaAngVelY, currentRate);
            }

            if (deltaAngVelX != 0.0) xratio = deltaAngVelX / maxValX;
            if (deltaAngVelY != 0.0) yratio = deltaAngVelY / maxValY;

            if (mStickParams.verticalScale != 1.0)
            {
                yratio = Math.Clamp(yratio * mStickParams.verticalScale, 0.0, 1.0);
            }

            if (mStickParams.maxOutputEnabled)
            {
                double maxOutRatio = mStickParams.maxOutput;
                // Expand output a bit. Likely not going to get a straight line with Gyro
                double maxOutXRatio = Math.Min(normX / 0.98, 1.0) * maxOutRatio;
                double maxOutYRatio = Math.Min(normY / 0.98, 1.0) * maxOutRatio;

                xratio = Math.Min(Math.Max(xratio, 0.0), maxOutXRatio);
                yratio = Math.Min(Math.Max(yratio, 0.0), maxOutYRatio);
            }

            xNorm = 0.0; yNorm = 0.0;
            if (xratio != 0.0)
            {
                xNorm = (1.0 - antiX) * xratio + antiX;
                xNorm = xNorm * signX; // Restore sign
            }

            if (yratio != 0.0)
            {
                yNorm = (1.0 - antiY) * yratio + antiY;
                yNorm = yNorm * signY; // Restore sign
            }

            if (xNorm != 0.0 || yNorm != 0.0)
            //if (xNorm != prevXNorm || yNorm != prevYNorm)
            {
                active = true;
                activeEvent = true;
            }
            else if (prevXNorm != 0.0 || prevYNorm != 0.0)
            {
                active = true;
                activeEvent = true;
            }
            else
            {
                active = false;
                activeEvent = false;
            }
        }

        public override void Event(Mapper mapper)
        {
            /*double tempX = xMotion, tempY = yMotion;
            if (mouseParams.smoothing)
            {
                tempX = smoothFilter.Filter(xMotion, mapper.CurrentRate);
                tempY = smoothFilter.Filter(yMotion, mapper.CurrentRate);
            }
            */

            double outXNorm = xNorm;
            double outYNorm = yNorm;

            // Adjust sensitivity to work around rounding in filter method
            /*outXNorm *= 1.0005;
            outYNorm *= 1.0005;
            double tempXNorm = mapper.MStickFilterX.Filter(outXNorm, mapper.CurrentRate);
            double tempYNorm = mapper.MStickFilterY.Filter(outYNorm, mapper.CurrentRate);

            // Filter does not go back to absolute zero for reasons. Check
            // for low number and reset to zero
            if (Math.Abs(tempXNorm) < 0.0001) tempXNorm = 0.0;
            if (Math.Abs(tempYNorm) < 0.0001) tempYNorm = 0.0;

            // Need to check bounds again
            tempXNorm = Math.Clamp(tempXNorm, -1.0, 1.0);
            tempYNorm = Math.Clamp(tempYNorm, -1.0, 1.0);
            */
            double tempXNorm = outXNorm;
            double tempYNorm = outYNorm;

            if (mStickParams.invertX) tempXNorm = -1.0 * tempXNorm;
            if (mStickParams.invertY) tempYNorm = -1.0 * tempYNorm;

            if (mStickParams.outputAxes != GyroMouseJoystickOuputAxes.All)
            {
                if (mStickParams.outputAxes != GyroMouseJoystickOuputAxes.XAxis)
                {
                    tempXNorm = 0.0;
                }
                else if (mStickParams.outputAxes != GyroMouseJoystickOuputAxes.YAxis)
                {
                    tempYNorm = 0.0;
                }
            }

            //Trace.WriteLine($"HELP {tempXNorm} {tempYNorm}");

            mapper.GamepadFromStickInput(actionData, tempXNorm, tempYNorm, force: false);

            //if (xNorm != 0.0 || yNorm != 0.0)
            //{
            //    active = true;
            //}

            prevXNorm = xNorm;
            prevYNorm = yNorm;
            active = true;
            activeEvent = false;
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            //if (active)
            {
                mapper.GamepadFromStickInput(actionData, 0.0, 0.0);
            }

            xNorm = yNorm = 0.0;
            prevXNorm = prevYNorm = 0.0;
            active = false;
            activeEvent = false;
            ResetToggleActiveState();
            //smoothFilter.Reset();
            mStickParams.smoothingFilterSettings.ResetFilters();
        }

        public override void SoftRelease(Mapper mapper, MapAction checkAction, bool resetState = true)
        {
            //if (active)
            {
                mapper.GamepadFromStickInput(actionData, 0.0, 0.0);
            }

            xNorm = yNorm = 0.0;
            prevXNorm = prevYNorm = 0.0;
            active = false;
            activeEvent = false;
            ResetToggleActiveState();
            //smoothFilter.Reset();
            if (!useParentSmoothingFilter)
            {
                mStickParams.smoothingFilterSettings.ResetFilters();
            }
        }

        public override void SoftCopyFromParent(GyroMapAction parentAction)
        {
            if (parentAction is GyroMouseJoystick tempGyroStickAction)
            {
                base.SoftCopyFromParent(parentAction);

                this.parentAction = parentAction;
                tempGyroStickAction.hasLayeredAction = true;
                mappingId = tempGyroStickAction.mappingId;

                tempGyroStickAction.NotifyPropertyChanged += TempGyroStickAction_NotifyPropertyChanged;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                //bool updateSmoothing = false;
                foreach (string parentPropType in useParentProList)
                {
                    switch (parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = tempGyroStickAction.name;
                            break;
                        case PropertyKeyStrings.DEAD_ZONE:
                            mStickParams.deadZone = tempGyroStickAction.mStickParams.deadZone;
                            break;
                        case PropertyKeyStrings.MAX_ZONE:
                            mStickParams.maxZone = tempGyroStickAction.mStickParams.maxZone;
                            break;
                        case PropertyKeyStrings.ANTIDEAD_ZONE_X:
                            mStickParams.antiDeadzoneX = tempGyroStickAction.mStickParams.antiDeadzoneX;
                            break;
                        case PropertyKeyStrings.ANTIDEAD_ZONE_Y:
                            mStickParams.antiDeadzoneY = tempGyroStickAction.mStickParams.antiDeadzoneY;
                            break;
                        case PropertyKeyStrings.INVERT_X:
                            mStickParams.invertX = tempGyroStickAction.mStickParams.invertX;
                            break;
                        case PropertyKeyStrings.INVERT_Y:
                            mStickParams.invertY = tempGyroStickAction.mStickParams.invertY;
                            break;
                        case PropertyKeyStrings.VERTICAL_SCALE:
                            mStickParams.verticalScale = tempGyroStickAction.mStickParams.verticalScale;
                            break;
                        case PropertyKeyStrings.OUTPUT_AXES:
                            mStickParams.outputAxes = tempGyroStickAction.mStickParams.outputAxes;
                            break;
                        case PropertyKeyStrings.OUTPUT_STICK:
                            mStickParams.outputStick = tempGyroStickAction.mStickParams.outputStick;
                            actionData.StickCode = mStickParams.outputStick;
                            break;
                        case PropertyKeyStrings.MAX_OUTPUT_ENABLED:
                            mStickParams.maxOutputEnabled = tempGyroStickAction.mStickParams.maxOutputEnabled;
                            break;
                        case PropertyKeyStrings.MAX_OUTPUT:
                            mStickParams.maxOutput = tempGyroStickAction.mStickParams.maxOutput;
                            break;
                        case PropertyKeyStrings.TRIGGER_BUTTONS:
                            mStickParams.gyroTriggerButtons = tempGyroStickAction.mStickParams.gyroTriggerButtons;
                            break;
                        case PropertyKeyStrings.TRIGGER_ACTIVATE:
                            mStickParams.triggerActivates = tempGyroStickAction.mStickParams.triggerActivates;
                            break;
                        case PropertyKeyStrings.TRIGGER_EVAL_COND:
                            mStickParams.andCond = tempGyroStickAction.mStickParams.andCond;
                            break;
                        case PropertyKeyStrings.X_AXIS:
                            mStickParams.useForXAxis = tempGyroStickAction.mStickParams.useForXAxis;
                            break;
                        case PropertyKeyStrings.TOGGLE_ACTION:
                            mStickParams.toggleAction = tempGyroStickAction.mStickParams.toggleAction;
                            ResetToggleActiveState();
                            break;
                        case PropertyKeyStrings.JITTER_COMPENSATION:
                            mStickParams.jitterCompensation = tempGyroStickAction.mStickParams.jitterCompensation;
                            break;
                        case PropertyKeyStrings.SMOOTHING_ENABLED:
                            mStickParams.smoothing = tempGyroStickAction.mStickParams.smoothing;
                            //updateSmoothing = true;
                            break;
                        case PropertyKeyStrings.SMOOTHING_FILTER:
                            mStickParams.smoothingFilterSettings = tempGyroStickAction.mStickParams.smoothingFilterSettings;
                            useParentSmoothingFilter = true;
                            break;
                        //case PropertyKeyStrings.SMOOTHING_MINCUTOFF:
                        //    mStickParms.oneEuroMinCutoff = tempGyroStickAction.mStickParms.oneEuroMinCutoff;
                        //    updateSmoothing = true;
                        //    break;
                        //case PropertyKeyStrings.SMOOTHING_MINBETA:
                        //    mStickParms.oneEuroMinBeta = tempGyroStickAction.mStickParms.oneEuroMinBeta;
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

        private void TempGyroStickAction_NotifyPropertyChanged(object sender, NotifyPropertyChangeArgs e)
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

            GyroMouseJoystick tempGyroStickAction = parentAction as GyroMouseJoystick;
            //bool updateSmoothing = false;
            switch (propertyName)
            {
                case PropertyKeyStrings.NAME:
                    name = tempGyroStickAction.name;
                    break;
                case PropertyKeyStrings.DEAD_ZONE:
                    mStickParams.deadZone = tempGyroStickAction.mStickParams.deadZone;
                    break;
                case PropertyKeyStrings.MAX_ZONE:
                    mStickParams.maxZone = tempGyroStickAction.mStickParams.maxZone;
                    break;
                case PropertyKeyStrings.ANTIDEAD_ZONE_X:
                    mStickParams.antiDeadzoneX = tempGyroStickAction.mStickParams.antiDeadzoneX;
                    break;
                case PropertyKeyStrings.ANTIDEAD_ZONE_Y:
                    mStickParams.antiDeadzoneY = tempGyroStickAction.mStickParams.antiDeadzoneY;
                    break;
                case PropertyKeyStrings.INVERT_X:
                    mStickParams.invertX = tempGyroStickAction.mStickParams.invertX;
                    break;
                case PropertyKeyStrings.INVERT_Y:
                    mStickParams.invertY = tempGyroStickAction.mStickParams.invertY;
                    break;
                case PropertyKeyStrings.VERTICAL_SCALE:
                    mStickParams.verticalScale = tempGyroStickAction.mStickParams.verticalScale;
                    break;
                case PropertyKeyStrings.OUTPUT_AXES:
                    mStickParams.outputAxes = tempGyroStickAction.mStickParams.outputAxes;
                    break;
                case PropertyKeyStrings.OUTPUT_STICK:
                    mStickParams.outputStick = tempGyroStickAction.mStickParams.outputStick;
                    actionData.StickCode = mStickParams.outputStick;
                    break;
                case PropertyKeyStrings.MAX_OUTPUT_ENABLED:
                    mStickParams.maxOutputEnabled = tempGyroStickAction.mStickParams.maxOutputEnabled;
                    break;
                case PropertyKeyStrings.MAX_OUTPUT:
                    mStickParams.maxOutput = tempGyroStickAction.mStickParams.maxOutput;
                    break;
                case PropertyKeyStrings.TRIGGER_BUTTONS:
                    mStickParams.gyroTriggerButtons = tempGyroStickAction.mStickParams.gyroTriggerButtons;
                    break;
                case PropertyKeyStrings.TRIGGER_ACTIVATE:
                    mStickParams.triggerActivates = tempGyroStickAction.mStickParams.triggerActivates;
                    break;
                case PropertyKeyStrings.TRIGGER_EVAL_COND:
                    mStickParams.andCond = tempGyroStickAction.mStickParams.andCond;
                    break;
                case PropertyKeyStrings.X_AXIS:
                    mStickParams.useForXAxis = tempGyroStickAction.mStickParams.useForXAxis;
                    break;
                case PropertyKeyStrings.TOGGLE_ACTION:
                    mStickParams.toggleAction = tempGyroStickAction.mStickParams.toggleAction;
                    ResetToggleActiveState();
                    break;
                case PropertyKeyStrings.JITTER_COMPENSATION:
                    mStickParams.jitterCompensation = tempGyroStickAction.mStickParams.jitterCompensation;
                    break;
                case PropertyKeyStrings.SMOOTHING_ENABLED:
                    mStickParams.smoothing = tempGyroStickAction.mStickParams.smoothing;
                    //updateSmoothing = true;
                    break;
                case PropertyKeyStrings.SMOOTHING_FILTER:
                    mStickParams.smoothingFilterSettings = tempGyroStickAction.mStickParams.smoothingFilterSettings;
                    useParentSmoothingFilter = true;
                    break;
                //case PropertyKeyStrings.SMOOTHING_MINCUTOFF:
                //    mStickParms.oneEuroMinCutoff = tempGyroStickAction.mStickParms.oneEuroMinCutoff;
                //    updateSmoothing = true;
                //    break;
                //case PropertyKeyStrings.SMOOTHING_MINBETA:
                //    mStickParms.oneEuroMinBeta = tempGyroStickAction.mStickParms.oneEuroMinBeta;
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

        public override void BlankEvent(Mapper mapper)
        {
            mapper.GamepadFromStickInput(actionData, 0.0, 0.0);
            //mapper.ResetMouseJoystickData();
        }

        public override GyroMapAction DuplicateAction()
        {
            return new GyroMouseJoystick(this);
        }

        private void ResetToggleActiveState()
        {
            toggleActiveState = false;
            previousTriggerActivated = false;
        }

        //public void UpdateSmoothingFilter()
        //{
        //    smoothFilter = new OneEuroFilter(mStickParms.oneEuroMinCutoff,
        //        mStickParms.oneEuroMinBeta);
        //}
    }
}

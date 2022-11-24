using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.AxisModifiers;
using DS4MapperTest.ButtonActions;

namespace DS4MapperTest.TriggerActions
{
    public class TriggerDualStageAction : TriggerMapAction
    {
        public class PropertyKeyStrings
        {
            public const string NAME = "Name";
            public const string DEAD_ZONE = "DeadZone";
            public const string MAX_ZONE = "MaxZone";
            public const string SOFTPULL_BUTTON = "SoftPullButton";
            public const string FULLPULL_BUTTON = "FullPullButton";
            public const string DUALSTAGE_MODE = "DualStageMode";
            public const string HIPFIRE_DELAY = "HipFireDelay";
            public const string ANTIDEAD_ZONE = "AntiDeadZone";
            public const string FORCE_HIP_FIRE_TIME = "ForceHipFireTime";
            //public const string OUTPUT_TRIGGER = "OutputTrigger";
        }

        private HashSet<string> fullPropertySet = new HashSet<string>()
        {
            PropertyKeyStrings.NAME,
            PropertyKeyStrings.DEAD_ZONE,
            PropertyKeyStrings.MAX_ZONE,
            PropertyKeyStrings.SOFTPULL_BUTTON,
            PropertyKeyStrings.FULLPULL_BUTTON,
            PropertyKeyStrings.DUALSTAGE_MODE,
            PropertyKeyStrings.HIPFIRE_DELAY,
            PropertyKeyStrings.ANTIDEAD_ZONE,
            PropertyKeyStrings.FORCE_HIP_FIRE_TIME,
            //PropertyKeyStrings.OUTPUT_TRIGGER,
        };

        public enum EngageButtonsMode : ushort
        {
            None,
            SoftPullOnly,
            FullPullOnly,
            Both,
        }

        [Flags]
        public enum ActiveZoneButtons : ushort
        {
            None,
            SoftPull,
            FullPull
        }

        public enum DualStageMode : ushort
        {
            Threshold,
            ExclusiveButtons,
            HairTrigger,
            HipFire,
            HipFireExclusiveButtons
        }

        public const string ACTION_TYPE_NAME = "TriggerDualStageAction";

        private double axisNorm;
        private AxisDeadZone deadMod;

        public bool startCheck;
        private Stopwatch checkTimeWatch = new Stopwatch();
        public bool outputActive;
        public bool softPullActActive;
        public bool fullPullActActive;
        public EngageButtonsMode actionStateMode = EngageButtonsMode.Both;
        public ActiveZoneButtons currentActiveButtons = ActiveZoneButtons.None;
        public ActiveZoneButtons previousActiveButtons = ActiveZoneButtons.None;

        private DualStageMode triggerStageMode;
        private int hipFireMs;
        private bool fullPullClick;
        private bool forceHipTime;
        public bool ForceHipTime
        {
            get => forceHipTime;
            set => forceHipTime = value;
        }

        private AxisDirButton softPullActButton = new AxisDirButton();
        private AxisDirButton fullPullActButton = new AxisDirButton();
        private bool useParentSoftPullBtn;
        public bool UseParentSoftPullBtn
        {
            get => useParentSoftPullBtn;
            set => useParentSoftPullBtn = value;
        }

        private bool useParentFullPullBtn;
        public bool UseParentFullPullBtn
        {
            get => useParentFullPullBtn;
            set => useParentFullPullBtn = value;
        }

        public AxisDirButton SoftPullActButton
        {
            get => softPullActButton;
            set => softPullActButton = value;
        }

        public AxisDirButton FullPullActButton
        {
            get => fullPullActButton;
            set => fullPullActButton = value;
        }

        public DualStageMode TriggerStateMode
        {
            get => triggerStageMode;
            set => triggerStageMode = value;
        }

        public int HipFireMS
        {
            get => hipFireMs;
            set => hipFireMs = value;
        }

        public AxisDeadZone DeadMod
        {
            get => deadMod;
        }

        public TriggerDualStageAction()
        {
            actionTypeName = ACTION_TYPE_NAME;
            deadMod = new AxisDeadZone(0.0, 1.0, 0.0);
        }

        public override void Prepare(Mapper mapper, ref TriggerEventFrame eventFrame, bool alterState = true)
        {
            int maxDir = triggerDefinition.trigAxis.max;
            deadMod.CalcOutValues(eventFrame.axisValue, maxDir, out axisNorm);
            if (triggerDefinition.trigAxis.hasClickButton)
            {
                // Trigger has dedicated click button. Check it
                fullPullClick = eventFrame.fullClick;
            }
            else
            {
                // Use interpolated soft axis range for now with normal triggers
                fullPullClick = axisNorm == 1.0;
            }

            ActiveZoneButtons currentStageBtns = ProcessCurrentStage(axisNorm);

            this.softPullActActive = this.fullPullActActive = false;

            bool softPullActActive = (currentStageBtns & ActiveZoneButtons.SoftPull) != 0;
            if (softPullActActive)
            {
                this.softPullActActive = softPullActActive;
            }

            bool fullPullActActive = (currentStageBtns & ActiveZoneButtons.FullPull) != 0;
            if (fullPullActActive)
            {
                this.fullPullActActive = fullPullActActive;
            }

            //if (mappingId == "RT" && axisNorm != 1.0 && currentStageBtns.HasFlag(ActiveZoneButtons.FullPull))
            //{
            //    Trace.WriteLine($"AXIS NORM {axisNorm} | BTNS {currentStageBtns.ToString()} | {actionStateMode}");
            //}

            currentActiveButtons = currentStageBtns;
            //outputActive = currentStageBtns != ActiveZoneButtons.None;
            active = true;
            activeEvent = true;
        }

        public override void Event(Mapper mapper)
        {
            bool wasSoftPullActive = !softPullActActive &&
                (previousActiveButtons & ActiveZoneButtons.SoftPull) != 0;
            if (wasSoftPullActive)
            {
                softPullActButton.PrepareAnalog(mapper, 0.0, 0.0);
                softPullActButton.Event(mapper);
            }

            bool wasFullPullActive = !fullPullActActive &&
                (previousActiveButtons & ActiveZoneButtons.FullPull) != 0;
            if (wasFullPullActive)
            {
                fullPullActButton.PrepareAnalog(mapper, 0.0, 0.0);
                fullPullActButton.Event(mapper);
            }

            if (softPullActActive)
            {
                softPullActButton.PrepareAnalog(mapper, axisNorm, 1.0);
                if (softPullActButton.active) softPullActButton.Event(mapper);
            }

            if (fullPullActActive)
            {
                fullPullActButton.PrepareAnalog(mapper, axisNorm, 1.0);
                if (fullPullActButton.active) fullPullActButton.Event(mapper);
            }

            previousActiveButtons = currentActiveButtons;
        }

        public override void Release(Mapper mapper, bool resetState = true, bool ignoreReleaseActions = false)
        {
            if (softPullActActive)
            {
                softPullActButton.Release(mapper, resetState, ignoreReleaseActions);
            }

            if (fullPullActActive)
            {
                fullPullActButton.Release(mapper, resetState, ignoreReleaseActions);
            }

            axisNorm = 0.0;
            currentActiveButtons = ActiveZoneButtons.None;
            previousActiveButtons = currentActiveButtons;
            fullPullClick = false;
            ResetStageState();
            outputActive = false;
            active = activeEvent = false;
        }

        public override void SoftRelease(Mapper mapper, MapAction checkAction, bool resetState = true)
        {
            if (softPullActActive && !useParentSoftPullBtn)
            {
                softPullActButton.Release(mapper, resetState);
            }

            if (fullPullActActive && !useParentFullPullBtn)
            {
                fullPullActButton.Release(mapper, resetState);
            }

            axisNorm = 0.0;
            currentActiveButtons = ActiveZoneButtons.None;
            previousActiveButtons = currentActiveButtons;
            fullPullClick = false;
            ResetStageState();
            outputActive = false;
            active = activeEvent = false;
        }

        public override void SoftCopyFromParent(TriggerMapAction parentAction)
        {
            if (parentAction is TriggerDualStageAction tempDualTrigAction)
            {
                base.SoftCopyFromParent(parentAction);

                this.parentAction = parentAction;
                mappingId = tempDualTrigAction.mappingId;

                tempDualTrigAction.NotifyPropertyChanged += TempDualTrigAction_NotifyPropertyChanged;

                // Determine the set with properties that should inherit
                // from the parent action
                IEnumerable<string> useParentProList =
                    fullPropertySet.Except(changedProperties);

                foreach (string parentPropType in useParentProList)
                {
                    switch(parentPropType)
                    {
                        case PropertyKeyStrings.NAME:
                            name = tempDualTrigAction.name;
                            break;
                        case PropertyKeyStrings.DEAD_ZONE:
                            deadMod.DeadZone = tempDualTrigAction.deadMod.DeadZone;
                            break;
                        case PropertyKeyStrings.MAX_ZONE:
                            deadMod.MaxZone = tempDualTrigAction.deadMod.MaxZone;
                            break;
                        case PropertyKeyStrings.ANTIDEAD_ZONE:
                            deadMod.AntiDeadZone = tempDualTrigAction.deadMod.AntiDeadZone;
                            break;
                        case PropertyKeyStrings.SOFTPULL_BUTTON:
                            softPullActButton = tempDualTrigAction.softPullActButton;
                            useParentSoftPullBtn = true;
                            break;
                        case PropertyKeyStrings.FULLPULL_BUTTON:
                            fullPullActButton = tempDualTrigAction.fullPullActButton;
                            useParentFullPullBtn = true;
                            break;
                        case PropertyKeyStrings.DUALSTAGE_MODE:
                            triggerStageMode = tempDualTrigAction.triggerStageMode;
                            break;
                        case PropertyKeyStrings.HIPFIRE_DELAY:
                            hipFireMs = tempDualTrigAction.hipFireMs;
                            break;
                        case PropertyKeyStrings.FORCE_HIP_FIRE_TIME:
                            forceHipTime = tempDualTrigAction.forceHipTime;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void TempDualTrigAction_NotifyPropertyChanged(object sender, NotifyPropertyChangeArgs e)
        {
            CascadePropertyChange(e.Mapper, e.PropertyName);
        }

        private void StartStageProcessing(bool useTime=true)
        {
            startCheck = true;
            if (useTime)
            {
                checkTimeWatch.Restart();
            }

            outputActive = false;
            softPullActActive = false;
            fullPullActActive = false;
            actionStateMode = EngageButtonsMode.None;
            //previousActiveButtons = ActiveZoneButtons.None;
        }

        private void ResetStageState()
        {
            startCheck = false;
            if (checkTimeWatch.IsRunning)
            {
                checkTimeWatch.Reset();
            }

            outputActive = false;
            softPullActActive = false;
            fullPullActActive = false;
            actionStateMode = EngageButtonsMode.None;
            //previousActiveButtons = ActiveZoneButtons.None;
        }

        private ActiveZoneButtons ProcessCurrentStage(double axisNorm)
        {
            ActiveZoneButtons result = ActiveZoneButtons.None;

            switch (triggerStageMode)
            {
                case DualStageMode.Threshold:
                    {
                        if (fullPullClick)
                        {
                            result = ActiveZoneButtons.SoftPull | ActiveZoneButtons.FullPull;
                        }
                        else if (axisNorm != 0.0)
                        {
                            result = ActiveZoneButtons.SoftPull; 
                        }
                        else
                        {
                            result = ActiveZoneButtons.None;
                        }
                    }

                    break;
                case DualStageMode.ExclusiveButtons:
                    {
                        if (fullPullClick)
                        {
                            actionStateMode = EngageButtonsMode.FullPullOnly;
                            result = ActiveZoneButtons.FullPull;
                        }
                        else if (axisNorm != 0.0 &&
                            actionStateMode != EngageButtonsMode.FullPullOnly)
                        {
                            actionStateMode = EngageButtonsMode.Both;
                            result = ActiveZoneButtons.SoftPull;
                        }
                        else if (axisNorm == 0.0)
                        {
                            actionStateMode = EngageButtonsMode.None;
                            result = ActiveZoneButtons.None;
                            //outputActive = false;
                        }
                    }

                    break;
                case DualStageMode.HairTrigger:
                    {
                        if (fullPullClick)
                        {
                            // Full pull now activates both. Soft pull action
                            // no longer engaged with threshold
                            result = ActiveZoneButtons.SoftPull | ActiveZoneButtons.FullPull;
                        }
                        else if (axisNorm != 0.0 && fullPullActActive)
                        {
                            // Full pull not engaged yet. Activate Soft pull action.
                            result = ActiveZoneButtons.SoftPull;
                        }
                        else if (axisNorm == 0.0 && outputActive)
                        {
                            ResetStageState();
                            //outputActive = false;
                        }
                    }

                    break;
                case DualStageMode.HipFire:
                    {
                        if (axisNorm != 0.0 && !startCheck)
                        {
                            StartStageProcessing();
                        }
                        else if (axisNorm != 0.0 && !outputActive)
                        {
                            // Consider action active depending on timer
                            // or whether full pull is achieved
                            bool nowActive = (!forceHipTime && fullPullClick) ||
                                checkTimeWatch.ElapsedMilliseconds > hipFireMs;

                            if (nowActive)
                            {
                                checkTimeWatch.Stop();
                                outputActive = nowActive;

                                if (fullPullClick)
                                {
                                    actionStateMode = EngageButtonsMode.FullPullOnly;
                                }
                                else if (axisNorm != 0.0)
                                {
                                    actionStateMode = EngageButtonsMode.Both;
                                }
                            }
                        }
                        else if (outputActive)
                        {
                            if (fullPullClick)
                            {
                                result = ActiveZoneButtons.FullPull;

                                if (actionStateMode == EngageButtonsMode.Both)
                                {
                                    result = result | ActiveZoneButtons.SoftPull;
                                }
                            }
                            else if (axisNorm != 0.0 &&
                                actionStateMode == EngageButtonsMode.Both)
                            {
                                result = ActiveZoneButtons.SoftPull;
                            }
                            else if (axisNorm == 0.0)
                            {
                                ResetStageState();
                            }
                        }
                        else if (startCheck)
                        {
                            ResetStageState();
                        }
                    }

                    break;
                case DualStageMode.HipFireExclusiveButtons:
                    {
                        if (axisNorm == 0.0)
                        {
                            if (startCheck)
                            {
                                ResetStageState();
                            }

                            actionStateMode = EngageButtonsMode.None;
                            result = ActiveZoneButtons.None;
                        }
                        else if (axisNorm != 0.0 && !startCheck)
                        {
                            actionStateMode = EngageButtonsMode.None;

                            if (!forceHipTime && fullPullClick)
                            {
                                StartStageProcessing(false);
                            }
                            else if (axisNorm != 0.0)
                            {
                                StartStageProcessing();
                            }
                        }

                        if (axisNorm != 0.0)
                        {
                            if (startCheck && !outputActive)
                            {
                                // Consider action active depending on timer
                                // or whether full pull is achieved
                                bool nowActive = (!forceHipTime && fullPullClick) ||
                                    checkTimeWatch.ElapsedMilliseconds > hipFireMs;

                                if (nowActive)
                                {
                                    if (checkTimeWatch.IsRunning)
                                    {
                                        checkTimeWatch.Stop();
                                    }

                                    outputActive = nowActive;

                                    if (fullPullClick)
                                    {
                                        actionStateMode = EngageButtonsMode.FullPullOnly;
                                        result = ActiveZoneButtons.FullPull;
                                    }
                                    else if (axisNorm != 0.0)
                                    {
                                        actionStateMode = EngageButtonsMode.SoftPullOnly;
                                        result = ActiveZoneButtons.SoftPull;
                                    }
                                }
                            }
                            else if (startCheck && outputActive)
                            {
                                if (fullPullClick &&
                                    actionStateMode == EngageButtonsMode.FullPullOnly)
                                {
                                    result = ActiveZoneButtons.FullPull;
                                }
                                else if (axisNorm != 0.0 &&
                                    actionStateMode == EngageButtonsMode.SoftPullOnly)
                                {
                                    result = ActiveZoneButtons.SoftPull;
                                }
                            }
                            //else if (startCheck)
                            //{
                            //    ResetStageState();
                            //}
                        }
                    }

                    break;
                default:
                    break;
            }

            return result;
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

            TriggerDualStageAction tempDualTrigAction = parentAction as TriggerDualStageAction;

            switch (propertyName)
            {
                case PropertyKeyStrings.NAME:
                    name = tempDualTrigAction.name;
                    break;
                case PropertyKeyStrings.DEAD_ZONE:
                    deadMod.DeadZone = tempDualTrigAction.deadMod.DeadZone;
                    break;
                case PropertyKeyStrings.MAX_ZONE:
                    deadMod.MaxZone = tempDualTrigAction.deadMod.MaxZone;
                    break;
                case PropertyKeyStrings.ANTIDEAD_ZONE:
                    deadMod.AntiDeadZone = tempDualTrigAction.deadMod.AntiDeadZone;
                    break;
                case PropertyKeyStrings.SOFTPULL_BUTTON:
                    softPullActButton = tempDualTrigAction.softPullActButton;
                    useParentSoftPullBtn = true;
                    break;
                case PropertyKeyStrings.FULLPULL_BUTTON:
                    fullPullActButton = tempDualTrigAction.fullPullActButton;
                    useParentFullPullBtn = true;
                    break;
                case PropertyKeyStrings.DUALSTAGE_MODE:
                    triggerStageMode = tempDualTrigAction.triggerStageMode;
                    break;
                case PropertyKeyStrings.HIPFIRE_DELAY:
                    hipFireMs = tempDualTrigAction.hipFireMs;
                    break;
                case PropertyKeyStrings.FORCE_HIP_FIRE_TIME:
                    forceHipTime = tempDualTrigAction.forceHipTime;
                    break;
                default:
                    break;
            }
        }
    }
}

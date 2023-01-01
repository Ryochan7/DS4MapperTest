using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4MapperTest.ViewModels.Common;
using DS4MapperTest.GyroActions;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.StickModifiers;

namespace DS4MapperTest.ViewModels.GyroActionPropViewModels
{
    public class GyroMouseJoystickPropViewModel : GyroActionPropVMBase
    {
        private GyroMouseJoystick action;
        public GyroMouseJoystick Action
        {
            get => action;
        }

        private OutputStickSelectionItemList outputStickHolder;
        public OutputStickSelectionItemList OutputStickHolder => outputStickHolder;

        private int outputStickIndex = -1;
        public int OutputStickIndex
        {
            get => outputStickIndex;
            set
            {
                outputStickIndex = value;
                OutputStickIndexChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler OutputStickIndexChanged;

        public int DeadZone
        {
            get => action.mStickParams.deadZone;
            set
            {
                action.mStickParams.deadZone = value;
                DeadZoneChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DeadZoneChanged;

        public int MaxZone
        {
            get => action.mStickParams.maxZone;
            set
            {
                action.mStickParams.maxZone = value;
                MaxZoneChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MaxZoneChanged;

        private List<GyroTriggerButtonItem> triggerButtonItems;
        public List<GyroTriggerButtonItem> TriggerButtonItems => triggerButtonItems;

        public string GyroTriggerString
        {
            get
            {
                List<string> tempList = new List<string>();
                foreach (JoypadActionCodes code in action.mStickParams.gyroTriggerButtons)
                {
                    GyroTriggerButtonItem tempItem =
                        triggerButtonItems.Find((item) => item.Code == code);

                    if (tempItem != null)
                    {
                        tempList.Add(tempItem.DisplayString);
                    }
                }

                if (tempList.Count == 0)
                {
                    tempList.Add(DEFAULT_EMPTY_TRIGGER_STR);
                }

                string result = string.Join(", ", tempList);
                return result;
            }
        }
        public event EventHandler GyroTriggerStringChanged;

        public bool GyroTriggerCondChoice
        {
            get => action.mStickParams.andCond;
            set
            {
                if (action.mStickParams.andCond == value) return;
                action.mStickParams.andCond = value;
                GyroTriggerCondChoiceChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler GyroTriggerCondChoiceChanged;

        public bool GyroTriggerActivates
        {
            get => action.mStickParams.triggerActivates;
            set
            {
                if (action.mStickParams.triggerActivates == value) return;
                action.mStickParams.triggerActivates = value;
                GyroTriggerActivatesChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler GyroTriggerActivatesChanged;

        public string AntiDeadZoneX
        {
            get => action.mStickParams.antiDeadzoneX.ToString("N2");
            set
            {
                if (double.TryParse(value, out double temp))
                {
                    action.mStickParams.antiDeadzoneX = Math.Clamp(temp, 0.0, 1.0);
                    AntiDeadZoneXChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler AntiDeadZoneXChanged;

        public string AntiDeadZoneY
        {
            get => action.mStickParams.antiDeadzoneY.ToString("N2");
            set
            {
                if (double.TryParse(value, out double temp))
                {
                    action.mStickParams.antiDeadzoneY = Math.Clamp(temp, 0.0, 1.0);
                    AntiDeadZoneYChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler AntiDeadZoneYChanged;

        public double VerticalScale
        {
            get => action.mStickParams.verticalScale;
            set
            {
                action.mStickParams.verticalScale = Math.Clamp(value, 0.0, 10.0);
                VerticalScaleChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler VerticalScaleChanged;

        private List<EnumChoiceSelection<GyroMouseJoystickOuputAxes>> outputAxesItems =
            new List<EnumChoiceSelection<GyroMouseJoystickOuputAxes>>()
        {
            new EnumChoiceSelection<GyroMouseJoystickOuputAxes>("Horizontal and Vertical", GyroMouseJoystickOuputAxes.All),
            new EnumChoiceSelection<GyroMouseJoystickOuputAxes>("Horizontal Only", GyroMouseJoystickOuputAxes.XAxis),
            new EnumChoiceSelection<GyroMouseJoystickOuputAxes>("Vertical Only", GyroMouseJoystickOuputAxes.YAxis),
        };
        public List<EnumChoiceSelection<GyroMouseJoystickOuputAxes>> OutputAxesItems => outputAxesItems;

        public GyroMouseJoystickOuputAxes OutputAxesChoice
        {
            get => action.mStickParams.outputAxes;
            set
            {
                action.mStickParams.outputAxes = value;
                OutputAxesChoiceChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler OutputAxesChoiceChanged;

        private List<InvertChoiceItem> invertChoiceItems = new List<InvertChoiceItem>()
        {
            new InvertChoiceItem("None", InvertChocies.None),
            new InvertChoiceItem("X", InvertChocies.InvertX),
            new InvertChoiceItem("Y", InvertChocies.InvertY),
            new InvertChoiceItem("X+Y", InvertChocies.InvertXY),
        };
        public List<InvertChoiceItem> InvertChoiceItems => invertChoiceItems;

        public InvertChocies InvertChoice
        {
            get
            {
                InvertChocies result = InvertChocies.None;
                if (action.mStickParams.invertX && action.mStickParams.invertY)
                {
                    result = InvertChocies.InvertXY;
                }
                else if (action.mStickParams.invertX || action.mStickParams.invertY)
                {
                    if (action.mStickParams.invertX)
                    {
                        result = InvertChocies.InvertX;
                    }
                    else
                    {
                        result = InvertChocies.InvertY;
                    }
                }

                return result;
            }
            set
            {
                action.mStickParams.invertX = action.mStickParams.invertY = false;

                switch (value)
                {
                    case InvertChocies.None:
                        break;
                    case InvertChocies.InvertX:
                        action.mStickParams.invertX = true;
                        break;
                    case InvertChocies.InvertY:
                        action.mStickParams.invertY = true;
                        break;
                    case InvertChocies.InvertXY:
                        action.mStickParams.invertX = action.mStickParams.invertY = true;
                        break;
                    default:
                        break;
                }

                InvertChoicesChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler InvertChoicesChanged;

        public bool SmoothingEnabled
        {
            get => action.mStickParams.smoothing;
            set
            {
                if (action.mStickParams.smoothing == value) return;
                action.mStickParams.smoothing = value;
                SmoothingEnabledChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SmoothingEnabledChanged;

        public double SmoothingMinCutoff
        {
            get => action.mStickParams.smoothingFilterSettings.minCutOff;
            set
            {
                if (action.mStickParams.smoothingFilterSettings.minCutOff == value) return;
                action.mStickParams.smoothingFilterSettings.minCutOff = Math.Clamp(value, 0.0, 10.0);
                SmoothingMinCutoffChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SmoothingMinCutoffChanged;

        public double SmoothingBeta
        {
            get => action.mStickParams.smoothingFilterSettings.beta;
            set
            {
                if (action.mStickParams.smoothingFilterSettings.beta == value) return;
                action.mStickParams.smoothingFilterSettings.beta = Math.Clamp(value, 0.0, 1.0);
                SmoothingBetaChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SmoothingBetaChanged;

        //private List<GyroOutputCurveItem> outputCurveItems = new List<GyroOutputCurveItem>();
        //public List<GyroOutputCurveItem> OutputCurveItems => outputCurveItems;

        //private int outputCurveIndex = -1;
        //public int OutputCurveIndex
        //{
        //    get => outputCurveIndex;
        //    set
        //    {
        //        outputCurveIndex = value;
        //        OutputCurveIndexChanged?.Invoke(this, EventArgs.Empty);
        //        ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
        //    }
        //}
        //public event EventHandler OutputCurveIndexChanged;

        public bool HighlightName
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.DEAD_ZONE);
        }
        public event EventHandler HighlightDeadZoneChanged;

        public bool HighlightMaxZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.MAX_ZONE);
        }
        public event EventHandler HighlightMaxZoneChanged;

        public bool HighlightOutputStick
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.OUTPUT_STICK);
        }
        public event EventHandler HighlightOutputStickChanged;

        public bool HighlightGyroTriggers
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.TRIGGER_BUTTONS);
        }
        public event EventHandler HighlightGyroTriggersChanged;

        public bool HighlightGyroTriggerCond
        {
            get => action.ParentAction == null ||
                baseAction.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.TRIGGER_EVAL_COND);
        }
        public event EventHandler HighlightGyroTriggerCondChanged;

        public bool HighlightGyroTriggerActivates
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.TRIGGER_ACTIVATE);
        }
        public event EventHandler HighlightGyroTriggerActivatesChanged;

        public bool HighlightAntiDeadZoneX
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.ANTIDEAD_ZONE_X);
        }
        public event EventHandler HighlightAntiDeadZoneXChanged;

        public bool HighlightAntiDeadZoneY
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.ANTIDEAD_ZONE_Y);
        }
        public event EventHandler HighlightAntiDeadZoneYChanged;

        public bool HighlightVerticalScale
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.VERTICAL_SCALE);
        }
        public event EventHandler HighlightVerticalScaleChanged;

        public bool HighlightOutputAxesChoice
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.OUTPUT_AXES);
        }
        public event EventHandler HighlightOutputAxesChoiceChanged;

        public bool HighlightInvert
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.INVERT_X) ||
                action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.INVERT_Y);
        }
        public event EventHandler HighlightInvertChanged;

        public bool HighlightSmoothingEnabled
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.SMOOTHING_ENABLED);
        }
        public event EventHandler HighlightSmoothingEnabledChanged;

        public bool HighlightSmoothingFilter
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.SMOOTHING_FILTER);
        }
        public event EventHandler HighlightSmoothingFilterChanged;

        //public bool HighlightOutputCurve
        //{
        //    get => action.ParentAction == null ||
        //        action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.ANTIDEAD_ZONE_Y);
        //}
        //public event EventHandler HighlightOutputCurveChanged;

        public override event EventHandler ActionPropertyChanged;

        public GyroMouseJoystickPropViewModel(Mapper mapper, GyroMapAction action)
        {
            this.mapper = mapper;
            this.action = action as GyroMouseJoystick;
            this.baseAction = action;

            triggerButtonItems = new List<GyroTriggerButtonItem>();
            outputStickHolder = new OutputStickSelectionItemList();

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer &&
                !mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                GyroMouseJoystick baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as GyroMouseJoystick;
                GyroMouseJoystick tempAction = new GyroMouseJoystick();
                tempAction.SoftCopyFromParent(baseLayerAction);
                //int tempLayerId = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.Index;
                int tempId = mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.FindNextAvailableId();
                tempAction.Id = tempId;
                //tempAction.MappingId = this.action.MappingId;

                this.action = tempAction;
                this.baseAction = this.action;
                usingRealAction = false;

                ActionPropertyChanged += ReplaceExistingLayerAction;
            }

            PopulateModel();

            NameChanged += GyroMouseJoystickPropViewModel_NameChanged;
            OutputStickIndexChanged += GyroMouseJoystickPropViewModel_OutputStickIndexChanged;
            DeadZoneChanged += GyroMouseJoystickPropViewModel_DeadZoneChanged;
            MaxZoneChanged += GyroMouseJoystickPropViewModel_MaxZoneChanged;
            GyroTriggerCondChoiceChanged += GyroMouseJoystickPropViewModel_GyroTriggerCondChoiceChanged;
            GyroTriggerActivatesChanged += GyroMouseJoystickPropViewModel_TriggerActivatesChanged;
            AntiDeadZoneXChanged += GyroMouseJoystickPropViewModel_AntiDeadZoneXChanged;
            AntiDeadZoneYChanged += GyroMouseJoystickPropViewModel_AntiDeadZoneYChanged;
            VerticalScaleChanged += GyroMouseJoystickPropViewModel_VerticalScaleChanged;
            InvertChoicesChanged += GyroMouseJoystickPropViewModel_InvertChoicesChanged;
            OutputAxesChoiceChanged += GyroMouseJoystickPropViewModel_OutputAxesChoiceChanged;
            SmoothingEnabledChanged += GyroMouseJoystickPropViewModel_SmoothingEnabledChanged;
            SmoothingMinCutoffChanged += GyroMouseJoystickPropViewModel_SmoothingMinCutoffChanged;
            SmoothingBetaChanged += GyroMouseJoystickPropViewModel_SmoothingBetaChanged;
        }

        private void GyroMouseJoystickPropViewModel_GyroTriggerCondChoiceChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.TRIGGER_EVAL_COND))
            {
                this.action.ChangedProperties.Add(GyroMouseJoystick.PropertyKeyStrings.TRIGGER_EVAL_COND);
            }

            action.RaiseNotifyPropertyChange(mapper, GyroMouseJoystick.PropertyKeyStrings.TRIGGER_EVAL_COND);
            HighlightGyroTriggerCondChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroMouseJoystickPropViewModel_SmoothingBetaChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.SMOOTHING_FILTER))
            {
                this.action.ChangedProperties.Add(GyroMouseJoystick.PropertyKeyStrings.SMOOTHING_FILTER);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, GyroMouseJoystick.PropertyKeyStrings.SMOOTHING_FILTER);
                action.mStickParams.smoothingFilterSettings.UpdateSmoothingFilters();
            });

            HighlightSmoothingFilterChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroMouseJoystickPropViewModel_SmoothingMinCutoffChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.SMOOTHING_FILTER))
            {
                this.action.ChangedProperties.Add(GyroMouseJoystick.PropertyKeyStrings.SMOOTHING_FILTER);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, GyroMouseJoystick.PropertyKeyStrings.SMOOTHING_FILTER);
                action.mStickParams.smoothingFilterSettings.UpdateSmoothingFilters();
            });

            HighlightSmoothingFilterChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroMouseJoystickPropViewModel_SmoothingEnabledChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.SMOOTHING_ENABLED))
            {
                this.action.ChangedProperties.Add(GyroMouseJoystick.PropertyKeyStrings.SMOOTHING_ENABLED);
            }

            action.RaiseNotifyPropertyChange(mapper, GyroMouseJoystick.PropertyKeyStrings.SMOOTHING_ENABLED);
            HighlightSmoothingEnabledChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroMouseJoystickPropViewModel_InvertChoicesChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.INVERT_X))
            {
                action.ChangedProperties.Add(GyroMouseJoystick.PropertyKeyStrings.INVERT_X);
            }

            if (!action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.INVERT_Y))
            {
                action.ChangedProperties.Add(GyroMouseJoystick.PropertyKeyStrings.INVERT_Y);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, GyroMouseJoystick.PropertyKeyStrings.INVERT_X);
                action.RaiseNotifyPropertyChange(mapper, GyroMouseJoystick.PropertyKeyStrings.INVERT_Y);
            });

            HighlightInvertChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroMouseJoystickPropViewModel_OutputAxesChoiceChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.OUTPUT_AXES))
            {
                action.ChangedProperties.Add(GyroMouseJoystick.PropertyKeyStrings.OUTPUT_AXES);
            }

            action.RaiseNotifyPropertyChange(mapper, GyroMouseJoystick.PropertyKeyStrings.OUTPUT_AXES);

            HighlightOutputAxesChoiceChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroMouseJoystickPropViewModel_VerticalScaleChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.VERTICAL_SCALE))
            {
                action.ChangedProperties.Add(GyroMouseJoystick.PropertyKeyStrings.VERTICAL_SCALE);
            }

            action.RaiseNotifyPropertyChange(mapper, GyroMouseJoystick.PropertyKeyStrings.VERTICAL_SCALE);
            HighlightVerticalScaleChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroMouseJoystickPropViewModel_MaxZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.MAX_ZONE))
            {
                action.ChangedProperties.Add(GyroMouseJoystick.PropertyKeyStrings.MAX_ZONE);
            }

            Action tempAction = () =>
            {
                action.RaiseNotifyPropertyChange(mapper, GyroMouseJoystick.PropertyKeyStrings.MAX_ZONE);
            };
            ExecuteInMapperThread(tempAction);

            HighlightMaxZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroMouseJoystickPropViewModel_OutputStickIndexChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.OUTPUT_STICK))
            {
                action.ChangedProperties.Add(GyroMouseJoystick.PropertyKeyStrings.OUTPUT_STICK);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, GyroMouseJoystick.PropertyKeyStrings.OUTPUT_STICK);
            });
            
            HighlightOutputStickChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroMouseJoystickPropViewModel_AntiDeadZoneYChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.ANTIDEAD_ZONE_Y))
            {
                action.ChangedProperties.Add(GyroMouseJoystick.PropertyKeyStrings.ANTIDEAD_ZONE_Y);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, GyroMouseJoystick.PropertyKeyStrings.ANTIDEAD_ZONE_Y);
            });
            
            HighlightAntiDeadZoneYChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroMouseJoystickPropViewModel_AntiDeadZoneXChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.ANTIDEAD_ZONE_X))
            {
                action.ChangedProperties.Add(GyroMouseJoystick.PropertyKeyStrings.ANTIDEAD_ZONE_X);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, GyroMouseJoystick.PropertyKeyStrings.ANTIDEAD_ZONE_X);
            });

            HighlightAntiDeadZoneXChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroMouseJoystickPropViewModel_TriggerActivatesChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.TRIGGER_ACTIVATE))
            {
                action.ChangedProperties.Add(GyroMouseJoystick.PropertyKeyStrings.TRIGGER_ACTIVATE);
            }

            ExecuteInMapperThread(() =>
            {
                action.RaiseNotifyPropertyChange(mapper, GyroMouseJoystick.PropertyKeyStrings.TRIGGER_ACTIVATE);
            });
            
            HighlightGyroTriggerActivatesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void PopulateModel()
        {
            foreach (ActionTriggerItem item in mapper.ActionTriggerItems)
            {
                triggerButtonItems.Add(new GyroTriggerButtonItem(item.DisplayName, item.Code));
            }

            foreach (JoypadActionCodes code in action.mStickParams.gyroTriggerButtons)
            {
                GyroTriggerButtonItem tempItem = triggerButtonItems.Find((item) => item.Code == code);
                if (tempItem != null)
                {
                    tempItem.Enabled = true;
                }
            }

            triggerButtonItems.ForEach((item) =>
            {
                item.EnabledChanged += GyroTriggerItem_EnabledChanged;
            });

            outputStickIndex =
                outputStickHolder.StickAliasIndex(action.mStickParams.outputStick);

            //outputCurveItems.AddRange(new GyroOutputCurveItem[]
            //{
            //    new GyroOutputCurveItem("Linear", StickOutCurve.Curve.Linear),
            //    new GyroOutputCurveItem("Enhanced Precision", StickOutCurve.Curve.EnhancedPrecision),
            //    new GyroOutputCurveItem("Quadratic", StickOutCurve.Curve.Quadratic),
            //    new GyroOutputCurveItem("Cubic", StickOutCurve.Curve.Cubic),
            //    new GyroOutputCurveItem("Easeout Quadratic", StickOutCurve.Curve.EaseoutQuad),
            //    new GyroOutputCurveItem("Easeout Cubic", StickOutCurve.Curve.EaseoutCubic),
            //});
        }

        private void GyroTriggerItem_EnabledChanged(object sender, EventArgs e)
        {
            GyroTriggerButtonItem tempItem = sender as GyroTriggerButtonItem;

            // Convert current array to temp List for convenience
            List<JoypadActionCodes> tempList = action.mStickParams.gyroTriggerButtons.ToList();

            // Add or remove code based on current enabled status
            if (tempItem.Enabled)
            {
                tempList.Add(tempItem.Code);
            }
            else
            {
                tempList.Remove(tempItem.Code);
            }

            if (!action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.TRIGGER_BUTTONS))
            {
                action.ChangedProperties.Add(GyroMouseJoystick.PropertyKeyStrings.TRIGGER_BUTTONS);
            }

            ExecuteInMapperThread(() =>
            {
                // Convert to array and save to action
                action.mStickParams.gyroTriggerButtons = tempList.ToArray();
                action.RaiseNotifyPropertyChange(mapper, GyroMouseJoystick.PropertyKeyStrings.TRIGGER_BUTTONS);
            });

            HighlightGyroTriggersChanged?.Invoke(this, EventArgs.Empty);
            GyroTriggerStringChanged?.Invoke(this, EventArgs.Empty);
            ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroMouseJoystickPropViewModel_DeadZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.DEAD_ZONE))
            {
                action.ChangedProperties.Add(GyroMouseJoystick.PropertyKeyStrings.DEAD_ZONE);
            }

            Action tempAction = () =>
            {
                action.RaiseNotifyPropertyChange(mapper, GyroMouseJoystick.PropertyKeyStrings.DEAD_ZONE);
            };
            ExecuteInMapperThread(tempAction);
            
            HighlightDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GyroMouseJoystickPropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(GyroMouseJoystick.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(GyroMouseJoystick.PropertyKeyStrings.NAME);
            }

            Action tempAction = () =>
            {
                action.RaiseNotifyPropertyChange(mapper, GyroMouseJoystick.PropertyKeyStrings.NAME);
            };

            ExecuteInMapperThread(tempAction);
            
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class GyroOutputCurveItem
    {
        private string displayName;
        public string DisplayName
        {
            get => displayName;
        }

        private StickOutCurve.Curve outputCurve = StickOutCurve.Curve.Linear;
        public StickOutCurve.Curve OutputCurve => outputCurve;

        public GyroOutputCurveItem(string displayName, StickOutCurve.Curve outputCurve)
        {
            this.displayName = displayName;
            this.outputCurve = outputCurve;
        }
    }
}

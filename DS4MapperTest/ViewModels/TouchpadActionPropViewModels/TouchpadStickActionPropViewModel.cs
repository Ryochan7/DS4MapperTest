using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DS4MapperTest.ActionUtil;
using DS4MapperTest.MapperUtil;
using DS4MapperTest.TouchpadActions;
using DS4MapperTest.StickModifiers;
using DS4MapperTest.ViewModels.Common;
using DS4MapperTest.StickActions;

namespace DS4MapperTest.ViewModels.TouchpadActionPropViewModels
{
    public class TouchpadStickActionPropViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private TouchpadStickAction action;
        public TouchpadStickAction Action
        {
            get => action;
        }

        public string Name
        {
            get => action.Name;
            set
            {
                if (action.Name == value) return;
                action.Name = value;
                NameChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler NameChanged;

        private List<OutputStickSelectionItem> outputStickItems =
            new List<OutputStickSelectionItem>();
        public List<OutputStickSelectionItem> OutputStickItems => outputStickItems;

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

        public string DeadZone
        {
            get => action.DeadMod.DeadZone.ToString("N2");
            set
            {
                if (double.TryParse(value, out double temp))
                {
                    action.DeadMod.DeadZone = Math.Clamp(temp, 0.0, 1.0);
                    DeadZoneChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler DeadZoneChanged;

        public string AntiDeadZone
        {
            get => action.DeadMod.AntiDeadZone.ToString("N2");
            set
            {
                if (double.TryParse(value, out double temp))
                {
                    action.DeadMod.AntiDeadZone = Math.Clamp(temp, 0.0, 1.0);
                    AntiDeadZoneChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler AntiDeadZoneChanged;

        public string MaxZone
        {
            get => action.DeadMod.MaxZone.ToString("N2");
            set
            {
                if (double.TryParse(value, out double temp))
                {
                    action.DeadMod.MaxZone = Math.Clamp(temp, 0.0, 1.0);
                    MaxZoneChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler MaxZoneChanged;

        public bool MaxOutputEnabled
        {
            get => action.MaxOutputEnabled;
            set
            {
                if (action.MaxOutputEnabled == value) return;
                action.MaxOutputEnabled = value;
                MaxOutputEnabledChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MaxOutputEnabledChanged;

        public double MaxOutput
        {
            get => action.MaxOutput;
            set
            {
                if (action.MaxOutput == value) return;
                action.MaxOutput = value;
                MaxOutputChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MaxOutputChanged;

        private List<EnumChoiceSelection<StickDeadZone.DeadZoneTypes>> deadZoneModesChoices =
            new List<EnumChoiceSelection<StickDeadZone.DeadZoneTypes>>()
            {
                new EnumChoiceSelection<StickDeadZone.DeadZoneTypes>("Radial", StickDeadZone.DeadZoneTypes.Radial),
                new EnumChoiceSelection<StickDeadZone.DeadZoneTypes>("Bowtie", StickDeadZone.DeadZoneTypes.Bowtie),
            };

        public List<EnumChoiceSelection<StickDeadZone.DeadZoneTypes>> DeadZoneModesChoices => deadZoneModesChoices;

        public StickDeadZone.DeadZoneTypes DeadZoneType
        {
            get => action.DeadMod.DeadZoneType;
            set
            {
                action.DeadMod.DeadZoneType = value;
                DeadZoneTypeChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DeadZoneTypeChanged;

        private List<EnumChoiceSelection<StickOutCurve.Curve>> outputCurveChoices = new List<EnumChoiceSelection<StickOutCurve.Curve>>()
        {
            new EnumChoiceSelection<StickOutCurve.Curve>("Linear", StickOutCurve.Curve.Linear),
            new EnumChoiceSelection<StickOutCurve.Curve>("Enhanced Precision", StickOutCurve.Curve.EnhancedPrecision),
            new EnumChoiceSelection<StickOutCurve.Curve>("Quadratic", StickOutCurve.Curve.Quadratic),
            new EnumChoiceSelection<StickOutCurve.Curve>("Cubic", StickOutCurve.Curve.Cubic),
            new EnumChoiceSelection<StickOutCurve.Curve>("Easeout Quad", StickOutCurve.Curve.EaseoutQuad),
            new EnumChoiceSelection<StickOutCurve.Curve>("Easeout Cubic", StickOutCurve.Curve.EaseoutCubic),
        };
        public List<EnumChoiceSelection<StickOutCurve.Curve>> OutputCurveChoices => outputCurveChoices;

        public StickOutCurve.Curve OutputCurve
        {
            get => action.OutputCurve;
            set
            {
                if (action.OutputCurve == value) return;
                OutputCurveChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler OutputCurveChanged;

        public double VerticalScale
        {
            get => action.VerticalScale;
            set
            {
                action.VerticalScale = Math.Clamp(value, 0.0, 10.0);
                VerticalScaleChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler VerticalScaleChanged;

        private List<EnumChoiceSelection<InvertChoices>> invertItems = new List<EnumChoiceSelection<InvertChoices>>()
        {
            new EnumChoiceSelection<InvertChoices>("None", Common.InvertChoices.None),
            new EnumChoiceSelection<InvertChoices>("X", Common.InvertChoices.InvertX),
            new EnumChoiceSelection<InvertChoices>("Y", Common.InvertChoices.InvertY),
            new EnumChoiceSelection<InvertChoices>("X+Y", Common.InvertChoices.InvertXY),
        };
        public List<EnumChoiceSelection<InvertChoices>> InvertItems => invertItems;

        public InvertChoices Invert
        {
            get
            {
                InvertChoices result = InvertChoices.None;
                if (action.InvertX && action.InvertY)
                {
                    result = InvertChoices.InvertXY;
                }
                else if (action.InvertX || action.InvertY)
                {
                    if (action.InvertX)
                    {
                        result = InvertChoices.InvertX;
                    }
                    else
                    {
                        result = InvertChoices.InvertY;
                    }
                }

                return result;
            }
            set
            {
                InvertChoices temp = InvertChoices.None;
                if (action.InvertX || action.InvertY)
                {
                    if (action.InvertX && action.InvertY)
                    {
                        temp = InvertChoices.InvertXY;
                    }
                    else if (action.InvertX)
                    {
                        temp = InvertChoices.InvertX;
                    }
                    else
                    {
                        temp = InvertChoices.InvertY;
                    }
                }

                if (temp == value) return;

                switch (value)
                {
                    case InvertChoices.None:
                        action.InvertX = action.InvertY = false;
                        break;
                    case InvertChoices.InvertX:
                        action.InvertX = true; action.InvertY = false;
                        break;
                    case InvertChoices.InvertY:
                        action.InvertX = false; action.InvertY = true;
                        break;
                    default: break;
                }

                InvertChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler InvertChanged;


        public event EventHandler ActionPropertyChanged;

        private bool usingRealAction = false;

        public bool HighlightName
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightOutputStickIndex
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.OUTPUT_STICK);
        }
        public event EventHandler HighlightOutputStickIndexChanged;

        public bool HighlightDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.DEAD_ZONE);
        }
        public event EventHandler HighlightDeadZoneChanged;

        public bool HighlightAntiDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.ANTIDEAD_ZONE);
        }
        public event EventHandler HighlightAntiDeadZoneChanged;

        public bool HighlightMaxZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.MAX_ZONE);
        }
        public event EventHandler HighlightMaxZoneChanged;

        public bool HighlightMaxOutputEnabled
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.MAX_OUTPUT_ENABLED);
        }
        public event EventHandler HighlightMaxOutputEnabledChanged;

        public bool HighlightMaxOutput
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.MAX_OUTPUT);
        }
        public event EventHandler HighlightMaxOutputChanged;

        public bool HighlightDeadZoneType
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.DEAD_ZONE_TYPE);
        }
        public event EventHandler HighlightDeadZoneTypeChanged;

        public bool HighlightOutputCurve
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.OUTPUT_CURVE);
        }
        public event EventHandler HighlightOutputCurveChanged;

        public bool HighlightVerticalScale
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.VERTICAL_SCALE);
        }
        public event EventHandler HighlightVerticalScaleChanged;

        public bool HighlightInvert
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.INVERT_X) ||
                action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.INVERT_Y);
        }
        public event EventHandler HighlightInvertChanged;

        public TouchpadStickActionPropViewModel(Mapper mapper,
            TouchpadMapAction action)
        {
            this.mapper = mapper;
            this.action = action as TouchpadStickAction;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer &&
                !mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                TouchpadStickAction baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as TouchpadStickAction;
                TouchpadStickAction tempAction = new TouchpadStickAction();
                tempAction.SoftCopyFromParent(baseLayerAction);
                //int tempLayerId = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.Index;
                int tempId = mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.FindNextAvailableId();
                tempAction.Id = tempId;
                //tempAction.MappingId = this.action.MappingId;

                this.action = tempAction;
                usingRealAction = false;

                ActionPropertyChanged += ReplaceExistingLayerAction;
            }

            outputStickItems.AddRange(new OutputStickSelectionItem[]
            {
                new OutputStickSelectionItem("Unbound", StickActionCodes.Empty),
                new OutputStickSelectionItem("Left Stick", StickActionCodes.X360_LS),
                new OutputStickSelectionItem("Right Stick", StickActionCodes.X360_RS),
            });

            PrepareModel();


            NameChanged += TouchpadStickActionPropViewModel_NameChanged;
            DeadZoneChanged += TouchpadStickActionPropViewModel_DeadZoneChanged;
            AntiDeadZoneChanged += TouchpadStickActionPropViewModel_AntiDeadZoneChanged;
            MaxZoneChanged += TouchpadStickActionPropViewModel_MaxZoneChanged;
            OutputStickIndexChanged += MarkOutputStickChanged;
            DeadZoneTypeChanged += TouchpadStickActionPropViewModel_DeadZoneTypeChanged;
            ActionPropertyChanged += SetProfileDirty;

            OutputStickIndexChanged += TouchpadStickActionPropViewModel_OutputStickIndexChanged;
            MaxOutputChanged += TouchpadStickActionPropViewModel_MaxOutputChanged;
            MaxOutputEnabledChanged += TouchpadStickActionPropViewModel_MaxOutputEnabledChanged;
            InvertChanged += TouchpadStickActionPropViewModel_InvertChanged;
            VerticalScaleChanged += TouchpadStickActionPropViewModel_VerticalScaleChanged;
            OutputCurveChanged += TouchpadStickActionPropViewModel_OutputCurveChanged;
        }

        private void TouchpadStickActionPropViewModel_OutputCurveChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.OUTPUT_CURVE))
            {
                this.action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.OUTPUT_CURVE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.OUTPUT_CURVE);
            HighlightOutputCurveChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadStickActionPropViewModel_VerticalScaleChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.VERTICAL_SCALE))
            {
                this.action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.VERTICAL_SCALE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.VERTICAL_SCALE);
            HighlightVerticalScaleChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadStickActionPropViewModel_InvertChanged(object sender, EventArgs e)
        {
            if (action.InvertX)
            {
                if (!this.action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.INVERT_X))
                {
                    this.action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.INVERT_X);
                }

                action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.INVERT_X);
            }

            if (action.InvertY)
            {
                if (!this.action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.INVERT_Y))
                {
                    this.action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.INVERT_Y);
                }

                action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.INVERT_Y);
            }

            HighlightInvertChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadStickActionPropViewModel_MaxOutputEnabledChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.MAX_OUTPUT_ENABLED))
            {
                this.action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.MAX_OUTPUT_ENABLED);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.MAX_OUTPUT_ENABLED);
            HighlightMaxOutputEnabledChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadStickActionPropViewModel_MaxOutputChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.MAX_OUTPUT))
            {
                this.action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.MAX_OUTPUT);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.MAX_OUTPUT);
            HighlightMaxOutputChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadStickActionPropViewModel_DeadZoneTypeChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.DEAD_ZONE_TYPE))
            {
                this.action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.DEAD_ZONE_TYPE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.DEAD_ZONE_TYPE);
            HighlightDeadZoneTypeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadStickActionPropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.NAME))
            {
                this.action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.NAME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MarkOutputStickChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.OUTPUT_STICK))
            {
                this.action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.OUTPUT_STICK);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.OUTPUT_STICK);
            HighlightOutputStickIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadStickActionPropViewModel_MaxZoneChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.MAX_ZONE))
            {
                this.action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.MAX_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.MAX_ZONE);
            HighlightMaxZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadStickActionPropViewModel_AntiDeadZoneChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.ANTIDEAD_ZONE))
            {
                this.action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.ANTIDEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.ANTIDEAD_ZONE);
            HighlightAntiDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TouchpadStickActionPropViewModel_DeadZoneChanged(object sender, EventArgs e)
        {
            if (!this.action.ChangedProperties.Contains(TouchpadStickAction.PropertyKeyStrings.DEAD_ZONE))
            {
                this.action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.DEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.DEAD_ZONE);
            HighlightDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ReplaceExistingLayerAction(object sender, EventArgs e)
        {
            if (!usingRealAction)
            {
                ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

                mapper.QueueEvent(() =>
                {
                    this.action.ParentAction.Release(mapper, ignoreReleaseActions: true);

                    mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.AddTouchpadAction(this.action);
                    if (mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer)
                    {
                        mapper.ActionProfile.CurrentActionSet.RecompileCompositeLayer(mapper);
                    }
                    else
                    {
                        mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.SyncActions();
                    }

                    resetEvent.Set();
                });

                resetEvent.Wait();

                usingRealAction = true;
            }
        }

        private void TouchpadStickActionPropViewModel_OutputStickIndexChanged(object sender, EventArgs e)
        {
            OutputStickSelectionItem item = outputStickItems[outputStickIndex];
            mapper.QueueEvent(() =>
            {
                action.OutputAction.StickCode = item.Code;
                action.ChangedProperties.Add(TouchpadStickAction.PropertyKeyStrings.OUTPUT_STICK);
                action.RaiseNotifyPropertyChange(mapper, TouchpadStickAction.PropertyKeyStrings.OUTPUT_STICK);
            });
        }

        private void SetProfileDirty(object sender, EventArgs e)
        {
            mapper.ActionProfile.Dirty = true;
        }

        private void PrepareModel()
        {
            switch(action.OutputAction.StickCode)
            {
                case StickActionCodes.Empty:
                    outputStickIndex = 0;
                    break;
                case StickActionCodes.X360_LS:
                    outputStickIndex = 1;
                    break;
                case StickActionCodes.X360_RS:
                    outputStickIndex = 2;
                    break;
                default:
                    break;
            }
        }
    }

    public class OutputStickSelectionItem
    {
        private string displayName;
        public string DisplayName
        {
            get => displayName;
        }

        private StickActionCodes code;
        public StickActionCodes Code => code;

        public OutputStickSelectionItem(string displayName, StickActionCodes code)
        {
            this.displayName = displayName;
            this.code = code;
        }
    }
}

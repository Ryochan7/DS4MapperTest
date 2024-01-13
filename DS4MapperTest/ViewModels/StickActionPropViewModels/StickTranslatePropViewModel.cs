using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS4MapperTest.StickActions;
using DS4MapperTest.StickModifiers;
using DS4MapperTest.ViewModels.Common;

namespace DS4MapperTest.ViewModels.StickActionPropViewModels
{
    public class StickTranslatePropViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private StickTranslate action;
        public StickTranslate Action
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

        private OutputStickSelectionItemList outputStickHolder = new OutputStickSelectionItemList();
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

        public string DeadZone
        {
            get => action.DeadMod.DeadZone.ToString();
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
            get => action.DeadMod.AntiDeadZone.ToString();
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

        public int Rotation
        {
            get => action.Rotation;
            set
            {
                if (value == action.Rotation) return;
                action.Rotation = value;
                RotationChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler RotationChanged;

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
                if (value == action.DeadMod.DeadZoneType) return;
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

                switch(value)
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

        public bool SquareStickEnabled
        {
            get => action.SquareStickEnabled;
            set
            {
                if (action.SquareStickEnabled == value) return;
                action.SquareStickEnabled = value;
                SquareStickEnabledChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler SquareStickEnabledChanged;

        public string SquareStickRoundness
        {
            get => action.SquareStickRoundness.ToString("N2");
            set
            {
                if (double.TryParse(value, out double temp))
                {
                    action.SquareStickRoundness = temp;
                    SquareStickRoundnessChanged?.Invoke(this, EventArgs.Empty);
                    ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler SquareStickRoundnessChanged;

        public bool HighlightName
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightOutputStick
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.OUTPUT_STICK);
        }
        public event EventHandler HighlightOutputStickChanged;

        public bool HighlightDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.DEAD_ZONE);
        }
        public event EventHandler HighlightDeadZoneChanged;

        public bool HighlightAntiDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.ANTIDEAD_ZONE);
        }
        public event EventHandler HighlightAntiDeadZoneChanged;

        public bool HighlightMaxZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.MAX_ZONE);
        }
        public event EventHandler HighlightMaxZoneChanged;

        public bool HighlightMaxOutputEnabled
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.MAX_OUTPUT_ENABLED);
        }
        public event EventHandler HighlightMaxOutputEnabledChanged;

        public bool HighlightMaxOutput
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.MAX_OUTPUT);
        }
        public event EventHandler HighlightMaxOutputChanged;

        public bool HighlightRotation
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.ROTATION);
        }
        public event EventHandler HighlightRotationChanged;

        public bool HighlightDeadZoneType
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.DEAD_ZONE_TYPE);
        }
        public event EventHandler HighlightDeadZoneTypeChanged;

        public bool HighlightOutputCurve
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.OUTPUT_CURVE);
        }
        public event EventHandler HighlightOutputCurveChanged;

        public bool HighlightVerticalScale
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.VERTICAL_SCALE);
        }
        public event EventHandler HighlightVerticalScaleChanged;

        public bool HighlightInvert
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.INVERT_X) ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.INVERT_Y);
        }
        public event EventHandler HighlightInvertChanged;

        public bool HighlightSquareStickEnabled
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.SQUARE_STICK_ENABLED);
        }
        public event EventHandler HighlightSquareStickEnabledChanged;

        public bool HighlightSquareStickRoundness
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.SQUARE_STICK_ROUNDNESS);
        }
        public event EventHandler HighlightSquareStickRoundnessChanged;

        public event EventHandler ActionPropertyChanged;
        public event EventHandler<StickMapAction> ActionChanged;

        private bool replacedAction = false;

        public StickTranslatePropViewModel(Mapper mapper, StickMapAction action)
        {
            this.mapper = mapper;
            this.action = action as StickTranslate;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.EditActionSet.UsingCompositeLayer &&
                !mapper.EditLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                StickTranslate baseLayerAction = mapper.EditActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as StickTranslate;
                StickTranslate tempAction = new StickTranslate();
                tempAction.SoftCopyFromParent(baseLayerAction);
                //int tempLayerId = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.Index;
                int tempId = mapper.EditLayer.FindNextAvailableId();
                tempAction.Id = tempId;
                //tempAction.MappingId = this.action.MappingId;

                this.action = tempAction;

                ActionPropertyChanged += ReplaceExistingLayerAction;
            }

            PrepareModel();

            NameChanged += StickTranslatePropViewModel_NameChanged;
            OutputStickIndexChanged += ChangeOutputStickMode;
            OutputStickIndexChanged += StickTranslatePropViewModel_OutputStickIndexChanged;
            DeadZoneChanged += StickTranslatePropViewModel_DeadZoneChanged;
            AntiDeadZoneChanged += StickTranslatePropViewModel_AntiDeadZoneChanged;
            MaxZoneChanged += StickTranslatePropViewModel_MaxZoneChanged;
            RotationChanged += StickTranslatePropViewModel_RotationChanged;
            DeadZoneTypeChanged += StickTranslatePropViewModel_DeadZoneTypeChanged;
            MaxOutputChanged += StickTranslatePropViewModel_MaxOutputChanged;
            MaxOutputEnabledChanged += StickTranslatePropViewModel_MaxOutputEnabledChanged;
            InvertChanged += StickTranslatePropViewModel_InvertChanged;
            VerticalScaleChanged += StickTranslatePropViewModel_VerticalScaleChanged;
            OutputCurveChanged += StickTranslatePropViewModel_OutputCurveChanged;
            SquareStickEnabledChanged += StickTranslatePropViewModel_SquareStickEnabledChanged;
            SquareStickRoundnessChanged += StickTranslatePropViewModel_SquareStickRoundnessChanged;
        }

        private void StickTranslatePropViewModel_SquareStickRoundnessChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.SQUARE_STICK_ROUNDNESS))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.SQUARE_STICK_ROUNDNESS);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.SQUARE_STICK_ROUNDNESS);
            HighlightSquareStickRoundnessChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickTranslatePropViewModel_SquareStickEnabledChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.SQUARE_STICK_ENABLED))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.SQUARE_STICK_ENABLED);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.SQUARE_STICK_ENABLED);
            HighlightSquareStickEnabledChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickTranslatePropViewModel_OutputCurveChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.OUTPUT_CURVE))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.OUTPUT_CURVE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.OUTPUT_CURVE);
            HighlightOutputCurveChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickTranslatePropViewModel_VerticalScaleChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.VERTICAL_SCALE))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.VERTICAL_SCALE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.VERTICAL_SCALE);
            HighlightVerticalScaleChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickTranslatePropViewModel_InvertChanged(object sender, EventArgs e)
        {
            if (action.InvertX)
            {
                if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.INVERT_X))
                {
                    action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.INVERT_X);
                }

                action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.INVERT_X);
            }

            if (action.InvertY)
            {
                if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.INVERT_Y))
                {
                    action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.INVERT_Y);
                }

                action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.INVERT_Y);
            }

            HighlightInvertChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickTranslatePropViewModel_MaxOutputEnabledChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.MAX_OUTPUT_ENABLED))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.MAX_OUTPUT_ENABLED);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.MAX_OUTPUT_ENABLED);
            HighlightMaxOutputEnabledChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickTranslatePropViewModel_MaxOutputChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.MAX_OUTPUT))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.MAX_OUTPUT);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.MAX_OUTPUT);
            HighlightMaxOutputChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickTranslatePropViewModel_DeadZoneTypeChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.DEAD_ZONE_TYPE))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.DEAD_ZONE_TYPE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.DEAD_ZONE_TYPE);
            HighlightDeadZoneTypeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickTranslatePropViewModel_RotationChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.ROTATION))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.ROTATION);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.ROTATION);
            HighlightRotationChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ChangeOutputStickMode(object sender, EventArgs e)
        {
            if (outputStickIndex == -1) return;

            OutputStickSelectionItem item = outputStickHolder.OutputStickItems[outputStickIndex];
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

            mapper.ProcessMappingChangeAction(() =>
            {
                action.Release(mapper, ignoreReleaseActions: true);

                action.OutputAction.Reset();
                action.OutputAction.Prepare(MapperUtil.OutputActionData.ActionType.GamepadControl, 0);
                action.OutputAction.StickCode = item.Code;

                resetEvent.Set();
            });

            resetEvent.Wait();
        }

        private void StickTranslatePropViewModel_MaxZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.MAX_ZONE))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.MAX_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.MAX_ZONE);
            HighlightMaxZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickTranslatePropViewModel_AntiDeadZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.ANTIDEAD_ZONE))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.ANTIDEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.ANTIDEAD_ZONE);
            HighlightAntiDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickTranslatePropViewModel_DeadZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.DEAD_ZONE))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.DEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.DEAD_ZONE);
            HighlightDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickTranslatePropViewModel_OutputStickIndexChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.OUTPUT_STICK))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.OUTPUT_STICK);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.OUTPUT_STICK);
            HighlightOutputStickChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickTranslatePropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickTranslate.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(StickTranslate.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, StickTranslate.PropertyKeyStrings.NAME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ReplaceExistingLayerAction(object sender, EventArgs e)
        {
            if (!replacedAction)
            {
                ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

                mapper.ProcessMappingChangeAction(() =>
                {
                    this.action.ParentAction.Release(mapper, ignoreReleaseActions: true);

                    mapper.EditLayer.AddStickAction(this.action);
                    if (mapper.EditActionSet.UsingCompositeLayer)
                    {
                        mapper.EditActionSet.RecompileCompositeLayer(mapper);
                    }
                    else
                    {
                        mapper.EditLayer.SyncActions();
                    }

                    resetEvent.Set();
                });

                resetEvent.Wait();

                replacedAction = true;

                ActionChanged?.Invoke(this, action);
            }
        }

        private void PrepareModel()
        {
            outputStickIndex =
                outputStickHolder.StickAliasIndex(action.OutputAction.StickCode);
        }
    }
}

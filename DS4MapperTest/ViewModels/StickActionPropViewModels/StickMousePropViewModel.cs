using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4MapperTest.ViewModels.Common;
using DS4MapperTest.StickModifiers;
using DS4MapperTest.StickActions;
using System.Threading;

namespace DS4MapperTest.ViewModels.StickActionPropViewModels
{
    public class StickMousePropViewModel
    {
        private Mapper mapper;
        public Mapper Mapper
        {
            get => mapper;
        }

        private StickMouse action;
        public StickMouse Action
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

        public double DeadZone
        {
            get => action.DeadMod.DeadZone;
            set
            {
                action.DeadMod.DeadZone = Math.Clamp(value, 0.0, 1.0);
                DeadZoneChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DeadZoneChanged;

        public int MouseSpeed
        {
            get => action.MouseSpeed;
            set
            {
                action.MouseSpeed = value;
                MouseSpeedChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MouseSpeedChanged;

        public string MouseSpeedOutput
        {
            get => (action.MouseSpeed * 20).ToString();
        }
        public event EventHandler MouseSpeedOutputChanged;

        private List<EnumChoiceSelection<StickOutCurve.Curve>> outputCurveChoiceItems =
            new List<EnumChoiceSelection<StickOutCurve.Curve>>()
        {
            new EnumChoiceSelection<StickOutCurve.Curve>("Linear", StickOutCurve.Curve.Linear),
            new EnumChoiceSelection<StickOutCurve.Curve>("Enhanced Precision", StickOutCurve.Curve.EnhancedPrecision),
            new EnumChoiceSelection<StickOutCurve.Curve>("Quadratic", StickOutCurve.Curve.Quadratic),
            new EnumChoiceSelection<StickOutCurve.Curve>("Cubic", StickOutCurve.Curve.Cubic),
            new EnumChoiceSelection<StickOutCurve.Curve>("EaseOut Quadratic", StickOutCurve.Curve.EaseoutQuad),
            new EnumChoiceSelection<StickOutCurve.Curve>("EaseOut Cubic", StickOutCurve.Curve.EaseoutCubic),
        };
        public List<EnumChoiceSelection<StickOutCurve.Curve>> OutputCurveChoiceItems => outputCurveChoiceItems;

        public StickOutCurve.Curve OutputCurveChoice
        {
            get => action.OutputCurve;
            set
            {
                action.OutputCurve = value;
                OutputCurveChoiceChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler OutputCurveChoiceChanged;

        public bool DeltaEnabled
        {
            get => action.MouseDeltaSettings.enabled;
            set
            {
                action.MouseDeltaSettings.enabled = value;
                DeltaEnabledChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DeltaEnabledChanged;

        public double DeltaMultiplier
        {
            get => action.MouseDeltaSettings.multiplier;
            set
            {
                action.MouseDeltaSettings.multiplier = value;
                DeltaMultiplierChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DeltaMultiplierChanged;

        public double DeltaMinTravel
        {
            get => action.MouseDeltaSettings.minTravel;
            set
            {
                action.MouseDeltaSettings.minTravel = value;
                DeltaMinTravelChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DeltaMinTravelChanged;

        public double DeltaMaxTravel
        {
            get => action.MouseDeltaSettings.maxTravel;
            set
            {
                action.MouseDeltaSettings.maxTravel = value;
                DeltaMaxTravelChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DeltaMaxTravelChanged;

        public double DeltaEasingDuration
        {
            get => action.MouseDeltaSettings.easingDuration;
            set
            {
                action.MouseDeltaSettings.easingDuration = value;
                DeltaEasingDurationChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DeltaEasingDurationChanged;

        public double DeltaMinFactor
        {
            get => action.MouseDeltaSettings.minfactor;
            set
            {
                action.MouseDeltaSettings.minfactor = value;
                DeltaMinFactorChanged?.Invoke(this, EventArgs.Empty);
                ActionPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler DeltaMinFactorChanged;

        public bool HighlightName
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.NAME);
        }
        public event EventHandler HighlightNameChanged;

        public bool HighlightDeadZone
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.DEAD_ZONE);
        }
        public event EventHandler HighlightDeadZoneChanged;

        public bool HighlightMouseSpeed
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.MOUSE_SPEED);
        }
        public event EventHandler HighlightMouseSpeedChanged;

        public bool HighlightOutputCurveChoice
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.OUTPUT_CURVE);
        }
        public event EventHandler HighlightOutputCurveChoiceChanged;

        public bool HighlightDelta
        {
            get => action.ParentAction == null ||
                action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
        }
        public event EventHandler HighlightDeltaChanged;

        //public bool HighlightDeltaEnabled
        //{
        //    get => action.ParentAction == null ||
        //        action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
        //}
        //public event EventHandler HighlightDeltaEnabledChanged;

        //public bool HighlightHighlightDeltaMultiplier
        //{
        //    get => action.ParentAction == null ||
        //        action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
        //}
        //public event EventHandler HighlightDeltaMultiplierChanged;

        //public bool HighlightDeltaMinTravel
        //{
        //    get => action.ParentAction == null ||
        //        action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
        //}
        //public event EventHandler HighlightDeltaMinTravelChanged;

        //public bool HighlightDeltaMaxTravel
        //{
        //    get => action.ParentAction == null ||
        //        action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
        //}
        //public event EventHandler HighlightDeltaMaxTravelChanged;

        //public bool HighlightDeltaEasingDuration
        //{
        //    get => action.ParentAction == null ||
        //        action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
        //}
        //public event EventHandler HighlightDeltaEasingDurationChanged;

        //public bool HighlightDeltaMinFactor
        //{
        //    get => action.ParentAction == null ||
        //        action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
        //}
        //public event EventHandler HighlightDeltaMinFactorChanged;


        public event EventHandler ActionPropertyChanged;
        public event EventHandler<StickMapAction> ActionChanged;

        private bool usingRealAction = false;

        public StickMousePropViewModel(Mapper mapper, StickMapAction action)
        {
            this.mapper = mapper;
            this.action = action as StickMouse;

            // Check if base ActionLayer action from composite layer
            if (action.ParentAction == null &&
                mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer &&
                !mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.LayerActions.Contains(action) &&
                MapAction.IsSameType(mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId], action))
            {
                // Test with temporary object
                StickMouse baseLayerAction = mapper.ActionProfile.CurrentActionSet.DefaultActionLayer.normalActionDict[action.MappingId] as StickMouse;
                StickMouse tempAction = new StickMouse();
                tempAction.SoftCopyFromParent(baseLayerAction);
                //int tempLayerId = mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.Index;
                int tempId = mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.FindNextAvailableId();
                tempAction.Id = tempId;
                //tempAction.MappingId = this.action.MappingId;

                this.action = tempAction;

                ActionPropertyChanged += ReplaceExistingLayerAction;
            }

            PrepareModel();

            NameChanged += StickMousePropViewModel_NameChanged;
            DeadZoneChanged += StickMousePropViewModel_DeadZoneChanged;
            MouseSpeedChanged += StickMousePropViewModel_MouseSpeedChanged;
            MouseSpeedChanged += RenderUpdatedOutputMouseSpeed;
            OutputCurveChoiceChanged += StickMousePropViewModel_OutputCurveChoiceChanged;
            DeltaEnabledChanged += StickMousePropViewModel_DeltaEnabledChanged;
            DeltaMultiplierChanged += StickMousePropViewModel_DeltaMultiplierChanged;
            DeltaMinTravelChanged += StickMousePropViewModel_DeltaMinTravelChanged;
            DeltaMaxTravelChanged += StickMousePropViewModel_DeltaMaxTravelChanged;
            DeltaEasingDurationChanged += StickMousePropViewModel_DeltaEasingDurationChanged;
            DeltaMinFactorChanged += StickMousePropViewModel_DeltaMinFactorChanged;
        }

        private void StickMousePropViewModel_DeltaMinFactorChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.DELTA_SETTINGS))
            {
                action.ChangedProperties.Add(StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
            }

            action.RaiseNotifyPropertyChange(mapper, StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
            HighlightDeltaChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickMousePropViewModel_DeltaEasingDurationChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.DELTA_SETTINGS))
            {
                action.ChangedProperties.Add(StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
            }

            action.RaiseNotifyPropertyChange(mapper, StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
            HighlightDeltaChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickMousePropViewModel_DeltaMaxTravelChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.DELTA_SETTINGS))
            {
                action.ChangedProperties.Add(StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
            }

            action.RaiseNotifyPropertyChange(mapper, StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
            HighlightDeltaChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickMousePropViewModel_DeltaMinTravelChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.DELTA_SETTINGS))
            {
                action.ChangedProperties.Add(StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
            }

            action.RaiseNotifyPropertyChange(mapper, StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
            HighlightDeltaChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickMousePropViewModel_DeltaMultiplierChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.DELTA_SETTINGS))
            {
                action.ChangedProperties.Add(StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
            }

            action.RaiseNotifyPropertyChange(mapper, StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
            HighlightDeltaChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickMousePropViewModel_DeltaEnabledChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.DELTA_SETTINGS))
            {
                action.ChangedProperties.Add(StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
            }

            action.RaiseNotifyPropertyChange(mapper, StickMouse.PropertyKeyStrings.DELTA_SETTINGS);
            HighlightDeltaChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RenderUpdatedOutputMouseSpeed(object sender, EventArgs e)
        {
            MouseSpeedOutputChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickMousePropViewModel_MouseSpeedChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.MOUSE_SPEED))
            {
                action.ChangedProperties.Add(StickMouse.PropertyKeyStrings.MOUSE_SPEED);
            }

            action.RaiseNotifyPropertyChange(mapper, StickMouse.PropertyKeyStrings.MOUSE_SPEED);
            HighlightMouseSpeedChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickMousePropViewModel_DeadZoneChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.DEAD_ZONE))
            {
                action.ChangedProperties.Add(StickMouse.PropertyKeyStrings.DEAD_ZONE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickMouse.PropertyKeyStrings.DEAD_ZONE);
            HighlightDeadZoneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickMousePropViewModel_OutputCurveChoiceChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.OUTPUT_CURVE))
            {
                action.ChangedProperties.Add(StickMouse.PropertyKeyStrings.OUTPUT_CURVE);
            }

            action.RaiseNotifyPropertyChange(mapper, StickMouse.PropertyKeyStrings.OUTPUT_CURVE);
            HighlightOutputCurveChoiceChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StickMousePropViewModel_NameChanged(object sender, EventArgs e)
        {
            if (!action.ChangedProperties.Contains(StickMouse.PropertyKeyStrings.NAME))
            {
                action.ChangedProperties.Add(StickMouse.PropertyKeyStrings.NAME);
            }

            action.RaiseNotifyPropertyChange(mapper, StickMouse.PropertyKeyStrings.NAME);
            HighlightNameChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ReplaceExistingLayerAction(object sender, EventArgs e)
        {
            if (!usingRealAction)
            {
                ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

                mapper.QueueEvent(() =>
                {
                    this.action.ParentAction.Release(mapper, ignoreReleaseActions: true);

                    mapper.ActionProfile.CurrentActionSet.RecentAppliedLayer.AddStickAction(this.action);
                    if (mapper.ActionProfile.CurrentActionSet.UsingCompositeLayer)
                    {
                        mapper.ActionProfile.CurrentActionSet.RecompileCompositeLayer(mapper);
                    }
                    else
                    {
                        mapper.ActionProfile.CurrentActionSet.CurrentActionLayer.SyncActions();
                        mapper.ActionProfile.CurrentActionSet.ClearCompositeLayerActions();
                        mapper.ActionProfile.CurrentActionSet.PrepareCompositeLayer();
                    }

                    resetEvent.Set();
                });

                resetEvent.Wait();

                usingRealAction = true;

                ActionChanged?.Invoke(this, action);
            }
        }

        private void PrepareModel()
        {

        }
    }
}
